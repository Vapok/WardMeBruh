using System;
using BepInEx.Configuration;
using UnityEngine;
using Vapok.Common.Abstractions;
using Vapok.Common.Managers.Configuration;
using WardMeBruh.Models;

namespace WardMeBruh.Configuration;

public class ConfigRegistry : ConfigSyncBase
{
    internal static ConfigEntry<bool> Enabled;
    internal static ConfigEntry<KeyboardShortcut> OpenWardHubKey;
    internal static ConfigEntry<bool> ShowSplashOnStartup;
    internal static ConfigEntry<bool> EnableTelemetry;
    internal static ConfigEntry<WardFontSizeMode> GuiFontSize;

    internal static ConfigEntry<float> MaxWardRadius;
    internal static ConfigEntry<float> WardSeparationDistance;
    internal static ConfigEntry<float> AutoRepairThresholdDefault;
    internal static ConfigEntry<float> AutoCloseDelayDefault;
    internal static ConfigEntry<bool> FreeAreaRepair;
    internal static ConfigEntry<bool> AllowOreFastTravel;
    internal static ConfigEntry<float> FastTravelCombatCooldown;

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

        UnsyncedConfig("Local Config", "Show Splash on Startup", true,
            new ConfigDescription("If enabled, displays the mod overview and links splash screen on game startup.",
                null, new ConfigurationManagerAttributes { Order = 4 }), ref ShowSplashOnStartup);

        UnsyncedConfig("Local Config", "Enable Anonymous Telemetry", true,
            new ConfigDescription("If enabled, sends anonymous mod launch and heartbeat telemetry to help improve mod stability and track active versions.",
                null, new ConfigurationManagerAttributes { Order = 5 }), ref EnableTelemetry);

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
    }
}
