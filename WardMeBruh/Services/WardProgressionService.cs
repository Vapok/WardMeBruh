using System.Collections.Generic;
using AdventureBackpacks.API.Client;
using UnityEngine;
using WardMeBruh.Configuration;
using WardMeBruh.Models;

namespace WardMeBruh.Services;

public static class WardProgressionService
{
    public const float BaselineRadius = 15.0f;

    public static string GetRuneSymbol(WardRune rune) => rune switch
    {
        WardRune.Fehu => "ᚠ",
        WardRune.Uruz => "ᚢ",
        WardRune.Thurisaz => "ᚦ",
        WardRune.Isa => "ᛁ",
        WardRune.Sowilo => "ᛋ",
        WardRune.Laguz => "ᛚ",
        WardRune.Dagaz => "ᛞ",
        WardRune.Hagalaz => "ᚺ",
        _ => string.Empty
    };

    public static string GetRunePrefabName(WardRune rune) => rune switch
    {
        WardRune.Fehu => "WMB_Keystone_Fehu",
        WardRune.Uruz => "WMB_Keystone_Uruz",
        WardRune.Thurisaz => "WMB_Keystone_Thurisaz",
        WardRune.Isa => "WMB_Keystone_Isa",
        WardRune.Sowilo => "WMB_Keystone_Sowilo",
        WardRune.Laguz => "WMB_Keystone_Laguz",
        WardRune.Dagaz => "WMB_Keystone_Dagaz",
        WardRune.Hagalaz => "WMB_Keystone_Hagalaz",
        _ => string.Empty
    };

    public static string GetRuneNameLocalizationKey(WardRune rune) => rune switch
    {
        WardRune.Fehu => "$wmb_rune_fehu_name",
        WardRune.Uruz => "$wmb_rune_uruz_name",
        WardRune.Thurisaz => "$wmb_rune_thurisaz_name",
        WardRune.Isa => "$wmb_rune_isa_name",
        WardRune.Sowilo => "$wmb_rune_sowilo_name",
        WardRune.Laguz => "$wmb_rune_laguz_name",
        WardRune.Dagaz => "$wmb_rune_dagaz_name",
        WardRune.Hagalaz => "$wmb_rune_hagalaz_name",
        _ => string.Empty
    };

    public static string GetRuneDescLocalizationKey(WardRune rune) => rune switch
    {
        WardRune.Fehu => "$wmb_rune_fehu_desc",
        WardRune.Uruz => "$wmb_rune_uruz_desc",
        WardRune.Thurisaz => "$wmb_rune_thurisaz_desc",
        WardRune.Isa => "$wmb_rune_isa_desc",
        WardRune.Sowilo => "$wmb_rune_sowilo_desc",
        WardRune.Laguz => "$wmb_rune_laguz_desc",
        WardRune.Dagaz => "$wmb_rune_dagaz_desc",
        WardRune.Hagalaz => "$wmb_rune_hagalaz_desc",
        _ => string.Empty
    };

    public static int GetAttunedMask(ZDO wardZdo)
    {
        if (wardZdo == null || !wardZdo.IsValid())
            return 0;

        ZDO effectiveZdo = WardQueryService.GetEffectiveWardZdo(wardZdo);
        if (effectiveZdo != null && effectiveZdo.IsValid())
        {
            int mask = effectiveZdo.GetInt(WardZdoKeys.AttunedRunes, 0);
            if (mask != 0)
                return mask;
        }

        return wardZdo.GetInt(WardZdoKeys.AttunedRunes, 0);
    }

    public static bool IsRuneAttuned(ZDO wardZdo, WardRune rune)
    {
        if ((int)rune < 0 || (int)rune > 7)
            return false;

        int mask = GetAttunedMask(wardZdo);
        return (mask & (1 << (int)rune)) != 0;
    }

    public static bool IsBossDefeated(string bossKey, Player player = null)
    {
        if (string.IsNullOrEmpty(bossKey))
            return true;

        if (ZoneSystem.instance != null && ZoneSystem.instance.GetGlobalKey(bossKey))
            return true;

        if (player != null && player.HaveUniqueKey(bossKey))
            return true;

        if (Player.m_localPlayer != null && Player.m_localPlayer.HaveUniqueKey(bossKey))
            return true;

        return false;
    }

    public static bool IsRuneUnlocked(ZDO wardZdo, WardRune rune, Player player = null)
    {
        if (!ConfigRegistry.EnableProgressionSystem.Value)
            return true;

        if ((int)rune < 0 || (int)rune > 7)
            return false;

        ProgressionMode mode = ConfigRegistry.ActiveProgressionMode.Value;
        if (!ConfigRegistry.RuneConfigurations.TryGetValue(rune, out RuneConfigDefinition def))
            return false;

        if (mode == ProgressionMode.WorldBossKeys)
        {
            return IsBossDefeated(def.BossKeyConfig.Value, player);
        }
        else if (mode == ProgressionMode.RunicKeystones)
        {
            return IsRuneAttuned(wardZdo, rune);
        }
        else
        {
            return IsRuneAttuned(wardZdo, rune) && IsBossDefeated(def.BossKeyConfig.Value, player);
        }
    }

    public static bool IsFeatureUnlocked(ZDO wardZdo, WardFeature feature, long playerId = 0)
    {
        if (!ConfigRegistry.EnableProgressionSystem.Value)
            return true;

        if (wardZdo == null || !wardZdo.IsValid())
            return false;

        Player player = null;
        if (playerId != 0)
        {
            player = Player.GetPlayer(playerId);
        }

        for (int r = 0; r <= 7; r++)
        {
            WardRune rune = (WardRune)r;
            if (!IsRuneUnlocked(wardZdo, rune, player))
                continue;

            if (ConfigRegistry.RuneConfigurations.TryGetValue(rune, out RuneConfigDefinition def))
            {
                if (def.ParsedFeatures.Contains(feature))
                    return true;
            }
        }

        return false;
    }

    public static bool IsExternalFeatureUnlocked(ZDO wardZdo, string externalFeatureId, long playerId = 0)
    {
        if (!ConfigRegistry.EnableProgressionSystem.Value)
            return true;

        if (string.IsNullOrEmpty(externalFeatureId))
            return true;

        if (wardZdo == null || !wardZdo.IsValid())
            return false;

        Player player = null;
        if (playerId != 0)
        {
            player = Player.GetPlayer(playerId);
        }

        for (int r = 0; r <= 7; r++)
        {
            WardRune rune = (WardRune)r;
            if (!IsRuneUnlocked(wardZdo, rune, player))
                continue;

            if (ConfigRegistry.RuneConfigurations.TryGetValue(rune, out RuneConfigDefinition def))
            {
                if (def.ParsedExternalFeatureIds.Contains(externalFeatureId))
                    return true;
            }
        }

        return false;
    }

    public static float GetMaxAllowedRadius(ZDO wardZdo, Player player = null)
    {
        if (!ConfigRegistry.EnableProgressionSystem.Value)
            return ConfigRegistry.MaxWardRadius != null ? ConfigRegistry.MaxWardRadius.Value : 100.0f;

        if (wardZdo == null || !wardZdo.IsValid())
            return BaselineRadius;

        float maxRadius = BaselineRadius;
        for (int r = 0; r <= 7; r++)
        {
            WardRune rune = (WardRune)r;
            if (!IsRuneUnlocked(wardZdo, rune, player))
                continue;

            if (ConfigRegistry.RuneConfigurations.TryGetValue(rune, out RuneConfigDefinition def))
            {
                float runeMax = def.MaxRadiusConfig.Value;
                if (runeMax > maxRadius)
                    maxRadius = runeMax;
            }
        }

        float serverMax = ConfigRegistry.MaxWardRadius != null ? ConfigRegistry.MaxWardRadius.Value : 100.0f;
        return Mathf.Min(maxRadius, serverMax);
    }

    public static float GetEffectiveSliderMaxRadius(ZDO wardZdo, long playerId)
    {
        Player player = null;
        if (playerId != 0)
        {
            player = Player.GetPlayer(playerId);
        }

        float progressionMax = GetMaxAllowedRadius(wardZdo, player);
        if (wardZdo == null || WardRegistryManager.Instance == null)
            return progressionMax;

        float spatialMax = WardRegistryManager.Instance.GetMaxAllowableSpatialRadius(wardZdo.GetPosition(), wardZdo.m_uid, playerId);
        float clamped = Mathf.Min(progressionMax, spatialMax);
        return Mathf.Max(10.0f, clamped);
    }

    public static string GetBossDisplayName(WardRune rune) => rune switch
    {
        WardRune.Fehu => "Eikthyr",
        WardRune.Uruz => "The Elder",
        WardRune.Thurisaz => "Bonemass",
        WardRune.Isa => "Moder",
        WardRune.Sowilo => "Yagluth",
        WardRune.Laguz => "The Queen",
        WardRune.Dagaz => "Fader",
        WardRune.Hagalaz => "Frozen King",
        _ => "Boss"
    };

    public static string GetBossTrophyPrefabName(WardRune rune) => rune switch
    {
        WardRune.Fehu => "TrophyEikthyr",
        WardRune.Uruz => "TrophyTheElder",
        WardRune.Thurisaz => "TrophyBonemass",
        WardRune.Isa => "TrophyDragonQueen",
        WardRune.Sowilo => "TrophyGoblinKing",
        WardRune.Laguz => "TrophySeekerQueen",
        WardRune.Dagaz => "TrophyFader",
        WardRune.Hagalaz => "FrozenKingDrop",
        _ => null
    };

    public static Sprite GetBossTrophyIcon(WardRune rune)
    {
        string trophyPrefabName = GetBossTrophyPrefabName(rune);
        if (!string.IsNullOrEmpty(trophyPrefabName) && ObjectDB.instance != null)
        {
            GameObject prefab = ObjectDB.instance.GetItemPrefab(trophyPrefabName);
            if (prefab != null)
            {
                ItemDrop drop = prefab.GetComponent<ItemDrop>();
                if (drop != null && drop.m_itemData != null)
                    return drop.m_itemData.GetIcon();
            }
        }
        return null;
    }

    public static bool HasKeystoneItem(Player player, WardRune rune)
    {
        if (player == null)
            return false;

        string prefabName = GetRunePrefabName(rune);
        string nameKey = GetRuneNameLocalizationKey(rune);

        Inventory inv = player.GetInventory();
        if (inv != null && ContainsKeystone(inv, nameKey, prefabName))
            return true;

        if (ABAPIClient.TryGetEquippedBackpackInventory(player, out Inventory bpInv) && bpInv != null)
        {
            if (ContainsKeystone(bpInv, nameKey, prefabName))
                return true;
        }

        return false;
    }

    private static bool ContainsKeystone(Inventory inventory, string nameKey, string prefabName)
    {
        List<ItemDrop.ItemData> items = inventory.GetAllItems();
        for (int i = 0; i < items.Count; i++)
        {
            ItemDrop.ItemData item = items[i];
            if (item == null)
                continue;

            if (item.m_shared != null && (item.m_shared.m_name == nameKey || item.m_shared.m_name == prefabName))
                return true;

            if (item.m_dropPrefab != null && item.m_dropPrefab.name == prefabName)
                return true;
        }

        return false;
    }

    public static bool RemoveKeystoneItem(Player player, WardRune rune)
    {
        if (player == null)
            return false;

        string prefabName = GetRunePrefabName(rune);
        string nameKey = GetRuneNameLocalizationKey(rune);

        Inventory inv = player.GetInventory();
        if (inv != null && TryRemoveKeystone(inv, nameKey, prefabName))
            return true;

        if (ABAPIClient.TryGetEquippedBackpackInventory(player, out Inventory bpInv) && bpInv != null)
        {
            if (TryRemoveKeystone(bpInv, nameKey, prefabName))
                return true;
        }

        return false;
    }

    private static bool TryRemoveKeystone(Inventory inventory, string nameKey, string prefabName)
    {
        List<ItemDrop.ItemData> items = inventory.GetAllItems();
        for (int i = 0; i < items.Count; i++)
        {
            ItemDrop.ItemData item = items[i];
            if (item == null)
                continue;

            if ((item.m_shared != null && (item.m_shared.m_name == nameKey || item.m_shared.m_name == prefabName)) ||
                (item.m_dropPrefab != null && item.m_dropPrefab.name == prefabName))
            {
                inventory.RemoveOneItem(item);
                return true;
            }
        }

        return false;
    }

    public static bool CanAttuneRune(ZDO wardZdo, WardRune rune, Player player, out string reason)
    {
        reason = string.Empty;
        if (wardZdo == null || !wardZdo.IsValid())
        {
            reason = "$wmb_invalid_ward";
            return false;
        }

        if ((int)rune < 0 || (int)rune > 7)
        {
            reason = "$wmb_invalid_rune";
            return false;
        }

        if (IsRuneAttuned(wardZdo, rune))
        {
            reason = "$wmb_rune_already_attuned";
            return false;
        }

        if ((int)rune > 0)
        {
            WardRune prevRune = (WardRune)((int)rune - 1);
            if (!IsRuneAttuned(wardZdo, prevRune))
            {
                reason = "$wmb_previous_rune_required";
                return false;
            }
        }

        if (!ConfigRegistry.RuneConfigurations.TryGetValue(rune, out RuneConfigDefinition def))
        {
            reason = "$wmb_invalid_rune_config";
            return false;
        }

        if (ConfigRegistry.ActiveProgressionMode.Value == ProgressionMode.Hybrid || ConfigRegistry.ActiveProgressionMode.Value == ProgressionMode.WorldBossKeys)
        {
            if (!IsBossDefeated(def.BossKeyConfig.Value, player))
            {
                reason = "$wmb_boss_not_defeated";
                return false;
            }
        }

        if (player != null && (ConfigRegistry.ActiveProgressionMode.Value == ProgressionMode.RunicKeystones || ConfigRegistry.ActiveProgressionMode.Value == ProgressionMode.Hybrid))
        {
            if (!HasKeystoneItem(player, rune))
            {
                reason = "$wmb_missing_keystone";
                return false;
            }
        }

        return true;
    }

    public static bool AttuneRune(ZDO wardZdo, WardRune rune, Player player)
    {
        if (!CanAttuneRune(wardZdo, rune, player, out _))
            return false;

        ZDO effectiveZdo = WardQueryService.GetEffectiveWardZdo(wardZdo);
        if (effectiveZdo == null || !effectiveZdo.IsValid())
            return false;

        if (player != null && (ConfigRegistry.ActiveProgressionMode.Value == ProgressionMode.RunicKeystones || ConfigRegistry.ActiveProgressionMode.Value == ProgressionMode.Hybrid))
        {
            RemoveKeystoneItem(player, rune);
        }

        int currentMask = effectiveZdo.GetInt(WardZdoKeys.AttunedRunes, 0);
        int newMask = currentMask | (1 << (int)rune);
        effectiveZdo.Set(WardZdoKeys.AttunedRunes, newMask);
        effectiveZdo.Set(WardZdoKeys.ProgressionLevel, (int)rune + 1);

        SyncAttunementToExtensions(effectiveZdo, newMask, (int)rune + 1);

        List<WardRoleDefinition> roles = WardRoleManager.GetRoleDefinitions(effectiveZdo);
        if (WardRoleManager.EnsureRoleDefaultsForUnlockedFeatures(effectiveZdo, roles))
        {
            WardRoleManager.SaveRoleDefinitions(effectiveZdo, roles);
        }

        return true;
    }

    public static void SyncAttunementToExtensions(ZDO baseWardZdo, int mask, int level)
    {
        if (baseWardZdo == null || !baseWardZdo.IsValid() || WardRegistryManager.Instance == null)
            return;

        List<KnownExtensionWard> extensions = WardRegistryManager.Instance.GetExtensionsForParent(baseWardZdo.m_uid);
        for (int i = 0; i < extensions.Count; i++)
        {
            if (ZDOMan.instance != null)
            {
                ZDO extZdo = ZDOMan.instance.GetZDO(extensions[i].Id);
                if (extZdo != null && extZdo.IsValid())
                {
                    extZdo.Set(WardZdoKeys.AttunedRunes, mask);
                    extZdo.Set(WardZdoKeys.ProgressionLevel, level);
                }
            }
        }
    }

    public static List<string> GetAttunementRefundItems(ZDO wardZdo)
    {
        List<string> items = new();
        if (wardZdo == null || !wardZdo.IsValid())
            return items;

        WardType type = (WardType)wardZdo.GetInt(WardZdoKeys.WardType, (int)WardType.Main);
        if (type == WardType.Extension)
            return items;

        int mask = wardZdo.GetInt(WardZdoKeys.AttunedRunes, 0);
        for (int r = 0; r <= 7; r++)
        {
            if ((mask & (1 << r)) != 0)
            {
                items.Add(GetRunePrefabName((WardRune)r));
            }
        }

        return items;
    }
}
