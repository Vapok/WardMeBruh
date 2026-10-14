using System;
using System.Collections.Generic;
using BepInEx.Configuration;
using UnityEngine;
using Vapok.Common.Abstractions;
using Vapok.Common.Managers.Configuration;
using WardMeBruh.Models;
using CraftingTable = ItemManager.CraftingTable;

namespace WardMeBruh.Configuration;

public class ConfigRegistry : ConfigSyncBase
{
    internal static ConfigEntry<bool> Enabled;
    internal static ConfigEntry<KeyboardShortcut> OpenWardHubKey;
    internal static ConfigEntry<WardFontSizeMode> GuiFontSize;

    internal static ConfigEntry<float> MaxWardRadius;
    internal static ConfigEntry<float> WardSeparationDistance;
    internal static ConfigEntry<float> AutoRepairThresholdDefault;
    internal static ConfigEntry<float> AutoCloseDelayDefault;
    internal static ConfigEntry<bool> FreeAreaRepair;
    internal static ConfigEntry<bool> AllowOreFastTravel;
    internal static ConfigEntry<float> FastTravelCombatCooldown;

    internal static ConfigEntry<bool> EnableProgressionSystem;
    internal static ConfigEntry<ProgressionMode> ActiveProgressionMode;
    internal static ConfigEntry<bool> EnforceWardPlacementClearance;
    internal static ConfigEntry<bool> ShowPhysicalAttunementStones;
    internal static ConfigEntry<float> KeystoneWeight;

    public static Dictionary<WardRune, RuneConfigDefinition> RuneConfigurations = new();

    public static Waiting Waiter;

    public ConfigRegistry(IPluginInfo mod, bool enableLockedConfigs = false) : base(mod, enableLockedConfigs)
    {
        Waiter = new Waiting();

        InitializeConfigurationSettings();
    }

    public sealed override void InitializeConfigurationSettings()
    {
        if (_config == null)
            return;

        SyncedConfig("Server Settings", "Enable Ward Me Bruh", true,
            new ConfigDescription("If enabled, enables WardMeBruh features.",
                null, new ConfigurationManagerAttributes { IsAdminOnly = true, Order = 1 }), ref Enabled);

        UnsyncedConfig("Local Config", "Open Ward Hub Keybind", new KeyboardShortcut(KeyCode.E),
            new ConfigDescription("Key shortcut used to open the Ward Hub GUI when looking at a ward.",
                null, new ConfigurationManagerAttributes { Order = 2 }), ref OpenWardHubKey);

        UnsyncedConfig("Local Config", "UI Font Size", WardFontSizeMode.Small,
            new ConfigDescription("Font size mode for the Ward Hub GUI (Small, Medium, Large).",
                null, new ConfigurationManagerAttributes { Order = 6 }), ref GuiFontSize);

        SyncedConfig("Server Settings", "Max Ward Radius", 100.0f,
            new ConfigDescription("Maximum radius in meters that any ward can be expanded to.",
                new AcceptableValueRange<float>(10.0f, 100.0f), new ConfigurationManagerAttributes { IsAdminOnly = true, Order = 10 }), ref MaxWardRadius);

        SyncedConfig("Server Settings", "Ward Separation Distance", 50.0f,
            new ConfigDescription("Minimum distance required between perimeters of wards owned by different players.",
                new AcceptableValueRange<float>(0.0f, 500.0f), new ConfigurationManagerAttributes { IsAdminOnly = true, Order = 11 }), ref WardSeparationDistance);

        SyncedConfig("Server Settings", "Default Repair Threshold", 1.0f,
            new ConfigDescription("Default health threshold (1.0 = 100% health) before area repair triggers.",
                new AcceptableValueRange<float>(0.01f, 1.0f), new ConfigurationManagerAttributes { IsAdminOnly = true, Order = 12 }), ref AutoRepairThresholdDefault);

        SyncedConfig("Server Settings", "Default Auto-Close Delay", 5.0f,
            new ConfigDescription("Default delay in seconds before an open door automatically closes.",
                new AcceptableValueRange<float>(1.0f, 60.0f), new ConfigurationManagerAttributes { IsAdminOnly = true, Order = 13 }), ref AutoCloseDelayDefault);

        SyncedConfig("Server Settings", "Free Area Repair", true,
            new ConfigDescription("If enabled, automated area repair requires zero resources.",
                null, new ConfigurationManagerAttributes { IsAdminOnly = true, Order = 14 }), ref FreeAreaRepair);

        SyncedConfig("Server Settings", "Allow Ore Fast-Travel", false,
            new ConfigDescription("If enabled, ward pin fast-travel permits transporting raw ore and metals.",
                null, new ConfigurationManagerAttributes { IsAdminOnly = true, Order = 15 }), ref AllowOreFastTravel);

        SyncedConfig("Server Settings", "Fast-Travel Combat Lockout", 10.0f,
            new ConfigDescription("Seconds out of combat required before ward fast-travel is permitted.",
                new AcceptableValueRange<float>(0.0f, 60.0f), new ConfigurationManagerAttributes { IsAdminOnly = true, Order = 16 }), ref FastTravelCombatCooldown);

        SyncedConfig("Progression System", "Enable Runic Attunement Progression", true,
            new ConfigDescription("Enables or disables the Runic Attunement progression system. When disabled, wards have full features unlocked according to server settings.",
                null, new ConfigurationManagerAttributes { IsAdminOnly = true, Order = 20 }), ref EnableProgressionSystem);

        SyncedConfig("Progression System", "Progression Mode", ProgressionMode.Hybrid,
            new ConfigDescription("Mode governing how runes are unlocked (RunicKeystones = physical craftable keystones; WorldBossKeys = defeated boss keys; Hybrid = requires both keystone attunement and defeated boss keys).",
                null, new ConfigurationManagerAttributes { IsAdminOnly = true, Order = 21 }), ref ActiveProgressionMode);

        SyncedConfig("Progression System", "Enforce Ward Placement Clearance", true,
            new ConfigDescription("If enabled, prevents placing a new ward within 15 meters of another player's existing ward perimeter.",
                null, new ConfigurationManagerAttributes { IsAdminOnly = true, Order = 23 }), ref EnforceWardPlacementClearance);

        UnsyncedConfig("Local Config", "Show Physical Attunement Stones", true,
            new ConfigDescription("If enabled, renders physical glowing pedestal runestones around attuned ward monuments.",
                null, new ConfigurationManagerAttributes { Order = 24 }), ref ShowPhysicalAttunementStones);

        SyncedConfig("Progression System", "Keystone Weight", 200.0f,
            new ConfigDescription("Inventory weight in units for Runic Keystones.",
                new AcceptableValueRange<float>(1.0f, 1000.0f), new ConfigurationManagerAttributes { IsAdminOnly = true, Order = 25 }), ref KeystoneWeight);

        RegisterRuneConfig(WardRune.Fehu, "Progression - 1. Fehu", 25.0f, "defeated_eikthyr", "TrophyEikthyr:1,Flint:20,FineWood:40", CraftingTable.Workbench, 2, WardFeature.AutoCloseDoors | WardFeature.ProtectTames, 30);
        RegisterRuneConfig(WardRune.Uruz, "Progression - 2. Uruz", 40.0f, "defeated_gdking", "TrophyTheElder:1,Bronze:20,SurtlingCore:10", CraftingTable.Forge, 1, WardFeature.PortalAccess | WardFeature.ProtectCrops | WardFeature.WeatherRoof | WardFeature.AutoFuel, 40);
        RegisterRuneConfig(WardRune.Thurisaz, "Progression - 3. Thurisaz", 55.0f, "defeated_bonemass", "TrophyBonemass:1,Iron:20,Guck:10", CraftingTable.Forge, 3, WardFeature.VehicleLocking | WardFeature.AreaRepair | WardFeature.PushbackForce | WardFeature.CreatureShock, 50);
        RegisterRuneConfig(WardRune.Isa, "Progression - 4. Isa", 70.0f, "defeated_dragon", "TrophyDragonQueen:1,Silver:20,FreezeGland:10", CraftingTable.Forge, 4, WardFeature.MaxComfort | WardFeature.SnowShield | WardFeature.FoodFreeze, 60);
        RegisterRuneConfig(WardRune.Sowilo, "Progression - 5. Sowilo", 80.0f, "defeated_goblinking", "TrophyGoblinKing:1,BlackMetal:20,Tar:20", CraftingTable.Forge, 6, WardFeature.GhostBase | WardFeature.RaidSuppression | WardFeature.SpawnSuppression | WardFeature.PlayerShock, 70);
        RegisterRuneConfig(WardRune.Laguz, "Progression - 6. Laguz", 90.0f, "defeated_queen", "TrophySeekerQueen:1,Eitr:20,BlackMarble:20", CraftingTable.MageTable, 1, WardFeature.WaterDrain | WardFeature.DeathSkillShield | WardFeature.RegenBoost, 80);
        RegisterRuneConfig(WardRune.Dagaz, "Progression - 7. Dagaz", 100.0f, "defeated_fader", "TrophyFader:1,FlametalNew:20,Blackwood:20", CraftingTable.BlackForge, 2, WardFeature.AshShield | WardFeature.LavaShield, 90);
        RegisterRuneConfig(WardRune.Hagalaz, "Progression - 8. Hagalaz", 120.0f, "defeated_frozenking", "FrozenKingDrop:1,FrostCore:10,Frostwood:20,GoldOre:20", CraftingTable.BlackForge, 3, WardFeature.FastTravel | WardFeature.ProtectTerrain | WardFeature.ArcticHearth | WardFeature.PackIceShield, 100);
    }

    private void RegisterRuneConfig(WardRune rune, string section, float defaultMaxRadius, string defaultBossKey, string defaultRecipe, CraftingTable defaultStation, int defaultStationLevel, WardFeature defaultFeatures, int baseOrder)
    {
        ConfigEntry<float> maxRadiusEntry = null;
        ConfigEntry<string> bossKeyEntry = null;
        ConfigEntry<string> recipeEntry = null;
        ConfigEntry<CraftingTable> stationEntry = null;
        ConfigEntry<int> stationLevelEntry = null;
        ConfigEntry<WardFeature> featuresEntry = null;

        SyncedConfig(section, "Max Ward Radius", defaultMaxRadius,
            new ConfigDescription("Maximum ward radius unlocked by attuning this rune.",
                new AcceptableValueRange<float>(15.0f, 250.0f), new ConfigurationManagerAttributes { IsAdminOnly = true, Order = baseOrder + 1 }), ref maxRadiusEntry);

        SyncedConfig(section, "Required Boss Key", defaultBossKey,
            new ConfigDescription("Global boss key required for this rune in WorldBossKeys or Hybrid mode.",
                null, new ConfigurationManagerAttributes { IsAdminOnly = true, Order = baseOrder + 2 }), ref bossKeyEntry);

        SyncedConfig(section, "Crafting Recipe", defaultRecipe,
            new ConfigDescription("Item costs to craft this rune keystone in ItemName:Amount format, separated by commas.",
                null, new ConfigurationManagerAttributes { IsAdminOnly = true, Order = baseOrder + 3 }), ref recipeEntry);

        SyncedConfig(section, "Crafting Station", defaultStation,
            new ConfigDescription("Crafting station required to craft this rune keystone.",
                null, new ConfigurationManagerAttributes { IsAdminOnly = true, Order = baseOrder + 4 }), ref stationEntry);

        SyncedConfig(section, "Min Station Level", defaultStationLevel,
            new ConfigDescription("Minimum station level required to craft this rune keystone.",
                new AcceptableValueRange<int>(1, 10), new ConfigurationManagerAttributes { IsAdminOnly = true, Order = baseOrder + 5 }), ref stationLevelEntry);

        SyncedConfig(section, "Unlocked Features", defaultFeatures,
            new ConfigDescription("Ward features unlocked when this rune is attuned.",
                null, new ConfigurationManagerAttributes { IsAdminOnly = true, Order = baseOrder + 6 }), ref featuresEntry);

        RuneConfigDefinition def = new RuneConfigDefinition
        {
            Rune = rune,
            SectionName = section,
            MaxRadiusConfig = maxRadiusEntry,
            BossKeyConfig = bossKeyEntry,
            RecipeConfig = recipeEntry,
            StationConfig = stationEntry,
            StationLevelConfig = stationLevelEntry,
            FeaturesConfig = featuresEntry
        };

        ParseFeatures(def);
        featuresEntry.SettingChanged += (sender, args) => ParseFeatures(def);

        RuneConfigurations[rune] = def;
    }

    public static void ParseFeatures(RuneConfigDefinition def)
    {
        if (def == null || def.FeaturesConfig == null)
            return;

        def.ParsedFeatures.Clear();
        WardFeature val = def.FeaturesConfig.Value;
        foreach (WardFeature f in (WardFeature[])Enum.GetValues(typeof(WardFeature)))
        {
            if (f != WardFeature.None && (val & f) == f)
            {
                def.ParsedFeatures.Add(f);
            }
        }
    }
}
