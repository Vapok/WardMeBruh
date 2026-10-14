using System;
using System.Collections.Generic;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;
using WardMeBruh.Components;
using WardMeBruh.Configuration;
using WardMeBruh.Models;
using WardMeBruh.Services;
using CraftingTable = ItemManager.CraftingTable;

namespace WardMeBruh.Items;

public static class KeystoneItemFactory
{
    private static readonly Dictionary<WardRune, CustomItem> _customKeystones = new();
    private static readonly Dictionary<WardRune, Sprite> _keystoneIcons = new();
    private static bool _registered;

    public static void Initialize()
    {
        if (_registered)
            return;

        _registered = true;

        PrefabManager.OnVanillaPrefabsAvailable += RegisterKeystoneItems;

        ConfigRegistry.EnableProgressionSystem.SettingChanged += OnProgressionConfigChanged;
        ConfigRegistry.ActiveProgressionMode.SettingChanged += OnProgressionConfigChanged;
        ConfigRegistry.KeystoneWeight.SettingChanged += OnWeightConfigChanged;

        foreach (KeyValuePair<WardRune, RuneConfigDefinition> kvp in ConfigRegistry.RuneConfigurations)
        {
            RuneConfigDefinition def = kvp.Value;
            def.RecipeConfig.SettingChanged += (s, e) => UpdateKeystoneRecipe(def.Rune);
            def.StationConfig.SettingChanged += (s, e) => UpdateKeystoneRecipe(def.Rune);
            def.StationLevelConfig.SettingChanged += (s, e) => UpdateKeystoneRecipe(def.Rune);
        }
    }

    private static void RegisterKeystoneItems()
    {
        for (int r = 0; r <= 7; r++)
        {
            WardRune rune = (WardRune)r;
            RegisterKeystone(rune);
        }
    }

    private static void RegisterKeystone(WardRune rune)
    {
        if (!ConfigRegistry.RuneConfigurations.TryGetValue(rune, out RuneConfigDefinition def))
            return;

        string prefabName = WardProgressionService.GetRunePrefabName(rune);
        string nameKey = WardProgressionService.GetRuneNameLocalizationKey(rune);
        string descKey = WardProgressionService.GetRuneDescLocalizationKey(rune);

        bool recipeEnabled = ConfigRegistry.EnableProgressionSystem.Value &&
                             ConfigRegistry.ActiveProgressionMode.Value != ProgressionMode.WorldBossKeys;

        ItemConfig itemConfig = new()
        {
            Name = nameKey,
            Description = descKey,
            CraftingStation = GetStationPrefabName(def.StationConfig.Value) ?? "none",
            MinStationLevel = def.StationLevelConfig.Value,
            Requirements = ParseToRequirementConfigs(def.RecipeConfig.Value),
            Weight = ConfigRegistry.KeystoneWeight.Value,
            Enabled = recipeEnabled
        };

        CustomItem customItem = new(prefabName, "Thunderstone", itemConfig);

        ConfigureKeystoneVisuals(customItem, rune);

        Jotunn.Managers.ItemManager.Instance.AddItem(customItem);
        _customKeystones[rune] = customItem;
    }

    private static void ConfigureKeystoneVisuals(CustomItem customItem, WardRune rune)
    {
        Color runeColor = WardAttunementStones.GetRuneColor(rune);
        GameObject prefab = customItem.ItemPrefab;
        ItemDrop drop = customItem.ItemDrop;

        if (drop != null && drop.m_itemData != null && drop.m_itemData.m_shared != null)
        {
            string sharedJson = JsonUtility.ToJson(drop.m_itemData.m_shared);
            ItemDrop.ItemData.SharedData clonedShared = JsonUtility.FromJson<ItemDrop.ItemData.SharedData>(sharedJson);
            if (clonedShared != null)
            {
                drop.m_itemData.m_shared = clonedShared;
            }

            drop.m_itemData.m_shared.m_weight = ConfigRegistry.KeystoneWeight.Value;
            drop.m_itemData.m_shared.m_maxStackSize = 1;
            drop.m_itemData.m_shared.m_teleportable = false;

            Sprite baseIcon = drop.m_itemData.GetIcon();
            Sprite recolored = LoadKeystoneIcon(rune, baseIcon);
            if (recolored != null)
            {
                drop.m_itemData.m_shared.m_icons = new Sprite[] { recolored };
                _keystoneIcons[rune] = recolored;
            }
        }

        if (prefab == null)
            return;

        Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer rend = renderers[i];
            if (rend == null || rend is ParticleSystemRenderer)
                continue;

            Material[] origMats = rend.sharedMaterials;
            Material[] newMats = new Material[origMats.Length];

            for (int m = 0; m < origMats.Length; m++)
            {
                if (origMats[m] == null)
                    continue;

                Material newMat = new Material(origMats[m]);
                newMat.name = $"{prefab.name}_Mat_{m}";

                if (newMat.HasProperty("_EmissionColor"))
                {
                    newMat.SetColor("_EmissionColor", runeColor * 3.5f);
                    newMat.EnableKeyword("_EMISSION");
                }

                if (newMat.HasProperty("_EmissionMap"))
                {
                    Texture origEmission = newMat.GetTexture("_EmissionMap");
                    if (origEmission is Texture2D emissionTex)
                    {
                        Texture2D grayEmission = CreateGrayscaleTexture(emissionTex);
                        if (grayEmission != null)
                        {
                            newMat.SetTexture("_EmissionMap", grayEmission);
                        }
                    }
                }

                newMats[m] = newMat;
            }
            rend.sharedMaterials = newMats;
        }

        Light[] lights = prefab.GetComponentsInChildren<Light>(true);
        for (int l = 0; l < lights.Length; l++)
        {
            lights[l].color = runeColor;
        }

        ParticleSystem[] particles = prefab.GetComponentsInChildren<ParticleSystem>(true);
        for (int p = 0; p < particles.Length; p++)
        {
            ParticleSystem.MainModule main = particles[p].main;
            main.startColor = runeColor;
        }

        ParticleSystemRenderer[] psRenderers = prefab.GetComponentsInChildren<ParticleSystemRenderer>(true);
        for (int pr = 0; pr < psRenderers.Length; pr++)
        {
            if (psRenderers[pr].sharedMaterial != null)
            {
                Material psMat = new Material(psRenderers[pr].sharedMaterial);
                if (psMat.HasProperty("_Color")) psMat.SetColor("_Color", runeColor);
                if (psMat.HasProperty("_TintColor")) psMat.SetColor("_TintColor", runeColor);
                psRenderers[pr].sharedMaterial = psMat;
            }
        }
    }

    private static Sprite LoadKeystoneIcon(WardRune rune, Sprite baseIcon)
    {
        try
        {
            string runeName = rune.ToString().ToLowerInvariant();
            string resourceName = $"WardMeBruh.Resources.Icons.keystone_{runeName}.png";
            System.Reflection.Assembly assembly = typeof(KeystoneItemFactory).Assembly;

            using (System.IO.Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                {
                    WardMeBruh.Log.Warning($"Embedded icon resource not found: {resourceName}");
                    return null;
                }

                byte[] buffer = new byte[stream.Length];
                stream.Read(buffer, 0, buffer.Length);

                Texture2D tex = Jotunn.Utils.AssetUtils.LoadImage(buffer);
                if (tex == null)
                {
                    WardMeBruh.Log.Warning($"Failed to load image data for {resourceName}");
                    return null;
                }

                float ppu = (baseIcon != null) ? baseIcon.pixelsPerUnit : 100f;
                Vector2 pivot = (baseIcon != null && baseIcon.rect.width > 0f && baseIcon.rect.height > 0f)
                    ? new Vector2(baseIcon.pivot.x / baseIcon.rect.width, baseIcon.pivot.y / baseIcon.rect.height)
                    : new Vector2(0.5f, 0.5f);

                Sprite sprite = Sprite.Create(
                    tex,
                    new Rect(0f, 0f, (float)tex.width, (float)tex.height),
                    pivot,
                    ppu);
                sprite.name = $"Icon_Keystone_{rune}";
                return sprite;
            }
        }
        catch (Exception ex)
        {
            WardMeBruh.Log.Warning($"Failed to load keystone icon for {rune}: {ex.Message}");
            return null;
        }
    }

    private static Texture2D CreateGrayscaleTexture(Texture2D source)
    {
        try
        {
            RenderTexture rt = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.ARGB32);
            Graphics.Blit(source, rt);

            RenderTexture prev = RenderTexture.active;
            RenderTexture.active = rt;

            Texture2D readable = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false);
            readable.ReadPixels(new Rect(0, 0, source.width, source.height), 0, 0);
            readable.Apply();

            RenderTexture.active = prev;
            RenderTexture.ReleaseTemporary(rt);

            Color[] pixels = readable.GetPixels();
            for (int i = 0; i < pixels.Length; i++)
            {
                Color c = pixels[i];
                float maxComp = Mathf.Max(c.r, Mathf.Max(c.g, c.b));
                pixels[i] = new Color(maxComp, maxComp, maxComp, c.a);
            }
            readable.SetPixels(pixels);
            readable.Apply(false, true);
            return readable;
        }
        catch (Exception ex)
        {
            WardMeBruh.Log.Warning($"Failed to create grayscale emission texture: {ex.Message}");
            return null;
        }
    }

    public static void SyncObjectDbItems(ObjectDB odb)
    {
        if (odb == null || odb.m_items == null)
            return;

        for (int r = 0; r <= 7; r++)
        {
            WardRune rune = (WardRune)r;
            string prefabName = WardProgressionService.GetRunePrefabName(rune);
            GameObject prefab = odb.GetItemPrefab(prefabName);
            if (prefab != null)
            {
                ItemDrop drop = prefab.GetComponent<ItemDrop>();
                if (drop != null && drop.m_itemData != null && drop.m_itemData.m_shared != null)
                {
                    drop.m_itemData.m_shared.m_maxStackSize = 1;
                    drop.m_itemData.m_shared.m_weight = ConfigRegistry.KeystoneWeight.Value;
                    drop.m_itemData.m_shared.m_teleportable = false;

                    if (!_keystoneIcons.TryGetValue(rune, out Sprite coloredIcon) || coloredIcon == null)
                    {
                        Sprite baseIcon = drop.m_itemData.GetIcon();
                        coloredIcon = LoadKeystoneIcon(rune, baseIcon);
                        if (coloredIcon != null)
                        {
                            _keystoneIcons[rune] = coloredIcon;
                        }
                    }

                    if (coloredIcon != null)
                    {
                        drop.m_itemData.m_shared.m_icons = new Sprite[] { coloredIcon };
                    }
                }
            }
        }
    }

    public static Sprite GetKeystoneIcon(WardRune rune)
    {
        if (_keystoneIcons.TryGetValue(rune, out Sprite coloredIcon) && coloredIcon != null)
            return coloredIcon;

        Sprite loaded = LoadKeystoneIcon(rune, null);
        if (loaded != null)
        {
            _keystoneIcons[rune] = loaded;
            return loaded;
        }

        if (_customKeystones.TryGetValue(rune, out CustomItem customItem) && customItem != null)
        {
            if (customItem.ItemDrop != null && customItem.ItemDrop.m_itemData != null)
            {
                Sprite icon = customItem.ItemDrop.m_itemData.GetIcon();
                if (icon != null)
                    return icon;
            }
        }

        if (ObjectDB.instance != null)
        {
            string prefabName = WardProgressionService.GetRunePrefabName(rune);
            GameObject prefab = ObjectDB.instance.GetItemPrefab(prefabName);
            if (prefab != null)
            {
                ItemDrop drop = prefab.GetComponent<ItemDrop>();
                if (drop != null && drop.m_itemData != null)
                {
                    Sprite icon = drop.m_itemData.GetIcon();
                    if (icon != null)
                        return icon;
                }
            }

            GameObject thunderstone = ObjectDB.instance.GetItemPrefab("Thunderstone");
            if (thunderstone != null)
            {
                ItemDrop tsDrop = thunderstone.GetComponent<ItemDrop>();
                if (tsDrop != null && tsDrop.m_itemData != null)
                {
                    return tsDrop.m_itemData.GetIcon();
                }
            }
        }

        return null;
    }

    private static void OnProgressionConfigChanged(object sender, EventArgs args)
    {
        for (int r = 0; r <= 7; r++)
        {
            UpdateKeystoneRecipe((WardRune)r);
        }
    }

    private static void OnWeightConfigChanged(object sender, EventArgs args)
    {
        float newWeight = ConfigRegistry.KeystoneWeight.Value;
        foreach (KeyValuePair<WardRune, CustomItem> kvp in _customKeystones)
        {
            ItemDrop drop = kvp.Value.ItemDrop;
            if (drop != null && drop.m_itemData != null && drop.m_itemData.m_shared != null)
            {
                drop.m_itemData.m_shared.m_weight = newWeight;
            }
        }
    }

    private static void UpdateKeystoneRecipe(WardRune rune)
    {
        if (!_customKeystones.TryGetValue(rune, out CustomItem customItem))
            return;

        if (!ConfigRegistry.RuneConfigurations.TryGetValue(rune, out RuneConfigDefinition def))
            return;

        bool recipeEnabled = ConfigRegistry.EnableProgressionSystem.Value &&
                             ConfigRegistry.ActiveProgressionMode.Value != ProgressionMode.WorldBossKeys &&
                             def.StationConfig.Value != CraftingTable.Disabled;

        if (customItem.Recipe != null && customItem.Recipe.Recipe != null)
        {
            Recipe recipe = customItem.Recipe.Recipe;
            recipe.m_enabled = recipeEnabled;
            recipe.m_minStationLevel = def.StationLevelConfig.Value;

            CraftingStation station = ResolveCraftingStation(def.StationConfig.Value);
            recipe.m_craftingStation = station;
            recipe.m_resources = ParseToPieceRequirements(def.RecipeConfig.Value);
        }
    }

    public static RequirementConfig[] ParseToRequirementConfigs(string costs)
    {
        if (string.IsNullOrWhiteSpace(costs))
            return new RequirementConfig[0];

        List<RequirementConfig> list = new();
        string[] items = costs.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < items.Length; i++)
        {
            string[] parts = items[i].Trim().Split(':');
            if (parts.Length == 0)
                continue;

            string itemName = parts[0].Trim();
            int amount = 1;
            if (parts.Length > 1)
            {
                int.TryParse(parts[1].Trim(), out amount);
            }

            if (!string.IsNullOrEmpty(itemName) && amount > 0)
            {
                list.Add(new RequirementConfig { Item = itemName, Amount = amount, Recover = true });
            }
        }

        return list.ToArray();
    }

    public static Piece.Requirement[] ParseToPieceRequirements(string costs)
    {
        if (string.IsNullOrWhiteSpace(costs))
            return new Piece.Requirement[0];

        List<Piece.Requirement> list = new();
        string[] items = costs.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < items.Length; i++)
        {
            string[] parts = items[i].Trim().Split(':');
            if (parts.Length == 0)
                continue;

            string itemName = parts[0].Trim();
            int amount = 1;
            if (parts.Length > 1)
            {
                int.TryParse(parts[1].Trim(), out amount);
            }

            if (string.IsNullOrEmpty(itemName) || amount <= 0)
                continue;

            GameObject prefab = null;
            if (ObjectDB.instance != null)
            {
                prefab = ObjectDB.instance.GetItemPrefab(itemName);
            }
            if (prefab == null && PrefabManager.Instance != null)
            {
                prefab = PrefabManager.Instance.GetPrefab(itemName);
            }

            if (prefab != null)
            {
                ItemDrop drop = prefab.GetComponent<ItemDrop>();
                if (drop != null)
                {
                    list.Add(new Piece.Requirement
                    {
                        m_resItem = drop,
                        m_amount = amount,
                        m_amountPerLevel = 0,
                        m_recover = true
                    });
                }
            }
        }

        return list.ToArray();
    }

    public static string GetStationPrefabName(CraftingTable table) => table switch
    {
        CraftingTable.Workbench => "piece_workbench",
        CraftingTable.Cauldron => "piece_cauldron",
        CraftingTable.Forge => "forge",
        CraftingTable.ArtisanTable => "piece_artisanstation",
        CraftingTable.StoneCutter => "piece_stonecutter",
        CraftingTable.MageTable => "piece_magetable",
        CraftingTable.BlackForge => "blackforge",
        CraftingTable.FoodPreparationTable => "piece_preptable",
        CraftingTable.MeadKetill => "piece_MeadCauldron",
        _ => null
    };

    private static CraftingStation ResolveCraftingStation(CraftingTable table)
    {
        string normalized = GetStationPrefabName(table);
        if (string.IsNullOrWhiteSpace(normalized))
            return null;

        if (PrefabManager.Instance != null)
        {
            GameObject stationPrefab = PrefabManager.Instance.GetPrefab(normalized);
            if (stationPrefab != null)
            {
                CraftingStation station = stationPrefab.GetComponent<CraftingStation>();
                if (station != null)
                    return station;
            }
        }

        if (ZNetScene.instance != null)
        {
            GameObject stationPrefab = ZNetScene.instance.GetPrefab(normalized);
            if (stationPrefab != null)
            {
                CraftingStation station = stationPrefab.GetComponent<CraftingStation>();
                if (station != null)
                    return station;
            }
        }

        return null;
    }
}
