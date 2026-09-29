using HarmonyLib;
using WardMeBruh.Networking;
using WardMeBruh.Services;

namespace WardMeBruh.Patches;

[HarmonyPatch(typeof(Game), nameof(Game.Start))]
internal static class GameStartPatch
{
    private static void Postfix()
    {
        string worldName = ZNet.instance != null && !string.IsNullOrEmpty(ZNet.instance.GetWorldName())
            ? ZNet.instance.GetWorldName()
            : "default";
        PlayerRegistryService.LoadWorld(worldName);

        WardNetworkRouter.Register();

        _ = AreaRepairService.Instance;
        _ = AutoFuelService.Instance;
        _ = AutoDoorService.Instance;
        _ = WardRepulsionService.Instance;

        if (!Jotunn.Managers.GUIManager.IsHeadless())
        {
            _ = UI.UIFonts.GetPrimaryFont();
            _ = UI.WardMainWindow.Instance;
            _ = UI.EntityRenameDialog.Instance;
        }

        if (ZNet.instance != null && !ZNet.instance.IsServer())
        {
            WardNetworkRouter.RequestRegistrySync();
        }
    }
}
