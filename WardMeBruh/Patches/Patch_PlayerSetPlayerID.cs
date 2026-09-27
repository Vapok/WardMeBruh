using HarmonyLib;
using WardMeBruh.Networking;
using WardMeBruh.Services;

namespace WardMeBruh.Patches;

[HarmonyPatch(typeof(Player), nameof(Player.SetPlayerID))]
internal static class Patch_PlayerSetPlayerID
{
    private static void Postfix(long playerID, string name)
    {
        if (playerID != 0L && !string.IsNullOrEmpty(name))
        {
            PlayerRegistryService.RegisterAndPersistPlayer(playerID, name);
            WardNetworkRouter.RegisterPlayerWithServer(playerID, name);
        }
    }
}
