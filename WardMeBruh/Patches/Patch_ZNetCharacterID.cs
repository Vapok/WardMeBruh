using HarmonyLib;
using WardMeBruh.Networking;
using WardMeBruh.Services;

namespace WardMeBruh.Patches;

[HarmonyPatch(typeof(ZNet), "RPC_CharacterID")]
internal static class Patch_ZNetCharacterID
{
    private static void Postfix(ZNet __instance, ZRpc rpc, ZDOID characterID)
    {
        if (characterID.IsNone())
            return;

        ZNetPeer peer = __instance.GetPeer(rpc);
        if (peer != null && !string.IsNullOrEmpty(peer.m_playerName) && ZDOMan.instance != null)
        {
            ZDO zdo = ZDOMan.instance.GetZDO(characterID);
            if (zdo != null)
            {
                long playerId = zdo.GetLong(ZDOVars.s_playerID, 0L);
                if (playerId != 0L)
                {
                    PlayerRegistryService.RegisterAndPersistPlayer(playerId, peer.m_playerName, broadcast: true);
                    WardNetworkRouter.SendKnownPlayers(peer.m_uid);
                }
            }
        }
    }
}
