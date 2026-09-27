using HarmonyLib;
using WardMeBruh.Networking;
using WardMeBruh.Services;

namespace WardMeBruh.Patches;

[HarmonyPatch(typeof(ZNet), "RPC_PlayerID")]
internal static class Patch_ZNetPlayerID
{
    private static void Postfix(ZNet __instance, ZRpc rpc, long playerID)
    {
        if (__instance == null || !__instance.IsServer())
            return;

        if (playerID == 0L)
            return;

        ZNetPeer peer = __instance.GetPeer(rpc);
        if (peer != null && !string.IsNullOrEmpty(peer.m_playerName))
        {
            WardMeBruh.Log.Info($"[Server] Recorded character from connection handshake: '{peer.m_playerName}' (ID: {playerID})");
            PlayerRegistryService.RegisterAndPersistPlayer(playerID, peer.m_playerName, broadcast: true);
            WardNetworkRouter.SendKnownPlayers(peer.m_uid);
        }
    }
}
