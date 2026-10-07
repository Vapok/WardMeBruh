/* WardMeBruh by Vapok */

using System;
using System.Reflection;
using APIManager;
using BepInEx;
using HarmonyLib;
using JetBrains.Annotations;
using Jotunn.Managers;
using WardMeBruh.Configuration;
using WardMeBruh.Logging;
using WardMeBruh.Services;
using Vapok.Common.Abstractions;
using Vapok.Common.Managers;
using Vapok.Common.Managers.Configuration;
using Vapok.Common.Managers.LocalizationManager;
using Vapok.Common.Managers.Splash;
using Vapok.Common.Tools;

namespace WardMeBruh;

[BepInPlugin(_pluginId, _displayName, _version)]
[BepInDependency(Jotunn.Main.ModGuid)]
[BepInDependency("com.ValheimModding.YamlDotNetDetector")]
public class WardMeBruh : BaseUnityPlugin, IPluginInfo
{
    private const string _pluginId = "vapok.mods.wardmebruh";
    private const string _displayName = "WardMeBruh";
    private const string _version = "0.0.5";
    public static bool ValheimAwake;
    public static Waiting Waiter;

    private static WardMeBruh _instance;
    private static ConfigSyncBase _config;
    private static ILogIt _log;
    private Harmony _harmony;

    public static ILogIt Log => _log;

    [UsedImplicitly]
    private void Awake()
    {
        _instance = this;

        Waiter = new Waiting();

        Jotunn.Entities.CustomLocalization localization = LocalizationManager.Instance.GetLocalization();

        LogManager.Init(PluginId, out ILogIt baseLog);
        _log = new DeduplicatingLog(baseLog);

        Initializer.LoadManagers(localization);

        WardRoleManager.Initialize();

        _config = new ConfigRegistry(_instance);

        Localizer.Waiter.StatusChanged += InitializeModule;

        _harmony = new Harmony(Info.Metadata.GUID);
        _harmony.PatchAll(Assembly.GetExecutingAssembly());

        Patcher.Patch(new[] { "WardMeBruh.API" });

        ModSplashManager.Register(new ModSplashDossier(_instance)
        {
            Tagline = "A quality-of-life mod for ward interaction and protection in Valheim.",
            ShowOnStartup = ConfigRegistry.ShowSplashOnStartup,
        });

        if (GUIManager.IsHeadless())
        {
            InitializeModule(this, EventArgs.Empty);
        }
    }

    private void OnDestroy()
    {
        _instance = null;
    }

    public string PluginId => _pluginId;
    public string DisplayName => _displayName;
    public string Version => _version;
    public BaseUnityPlugin Instance => _instance;

    public void InitializeModule(object sender, EventArgs args)
    {
        if (ValheimAwake)
            return;

        ConfigRegistry.Waiter.ConfigurationComplete(true);

        ValheimAwake = true;
    }

    public class Waiting
    {
        public void ValheimIsAwake(bool awakeFlag)
        {
            if (awakeFlag)
                StatusChanged?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler StatusChanged;
    }
}
