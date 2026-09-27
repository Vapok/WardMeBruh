using HarmonyLib;
using UnityEngine;
using WardMeBruh.Configuration;
using WardMeBruh.Models;
using WardMeBruh.Services;

namespace WardMeBruh.Patches;

[HarmonyPatch(typeof(TeleportWorld))]
internal static class Patch_TeleportWorld
{
    private static float _lastNotifyTime;

    [HarmonyPatch(nameof(TeleportWorld.Teleport))]
    [HarmonyPrefix]
    private static bool TeleportPrefix(TeleportWorld __instance, Player player)
    {
        if (ConfigRegistry.Enabled != null && !ConfigRegistry.Enabled.Value)
            return true;

        if (__instance == null || __instance.m_nview == null || !__instance.m_nview.IsValid())
            return true;

        if (player == null)
            return true;

        long playerId = player.GetPlayerID();
        if (playerId == 0L)
            return true;

        Vector3 localPos = __instance.transform.position;
        PrivateArea localWard = WardQueryService.GetCoveringWard(localPos);
        if (localWard != null && localWard.m_nview != null && localWard.m_nview.IsValid())
        {
            ZDO localWardZdo = localWard.m_nview.GetZDO();
            if (localWardZdo != null && localWardZdo.GetBool(WardZdoKeys.PortalAccess, false))
            {
                ZDO localPortalZdo = __instance.m_nview.GetZDO();
                int roleInt = localPortalZdo.GetInt(WardZdoKeys.PortalMinRole, (int)WardRole.Resident);
                WardRole requiredRole = (WardRole)roleInt;
                if (!WardPermissions.HasRequiredRole(localWardZdo, playerId, requiredRole))
                {
                    NotifyBlocked(player, "Portal transit blocked: Insufficient clearance for this portal");
                    return false;
                }
            }
        }

        ZDO connectionZdo = __instance.m_nview.GetZDO();
        ZDOID destId = connectionZdo.GetConnectionZDOID(ZDOExtraData.ConnectionType.Portal);
        if (destId != ZDOID.None && ZDOMan.instance != null)
        {
            ZDO destZdo = ZDOMan.instance.GetZDO(destId);
            if (destZdo != null)
            {
                Vector3 destPos = destZdo.GetPosition();

                PrivateArea destWard = WardQueryService.GetCoveringWard(destPos);
                if (destWard != null && destWard.m_nview != null && destWard.m_nview.IsValid())
                {
                    ZDO destWardZdo = destWard.m_nview.GetZDO();
                    if (destWardZdo != null && destWardZdo.GetBool(WardZdoKeys.PortalAccess, false))
                    {
                        int roleInt = destZdo.GetInt(WardZdoKeys.PortalMinRole, (int)WardRole.Resident);
                        WardRole destRequiredRole = (WardRole)roleInt;
                        if (!WardPermissions.HasRequiredRole(destWardZdo, playerId, destRequiredRole))
                        {
                            NotifyBlocked(player, "Destination portal transit blocked: Insufficient clearance at destination");
                            return false;
                        }
                    }
                }
                else
                {
                    KnownWard knownWard = WardRegistryManager.Instance.GetWardCoveringPoint(destPos);
                    if (knownWard != null)
                    {
                        ZDO destWardZdo = ZDOMan.instance.GetZDO(knownWard.Id);
                        if (destWardZdo != null && destWardZdo.GetBool(WardZdoKeys.PortalAccess, false))
                        {
                            int roleInt = destZdo.GetInt(WardZdoKeys.PortalMinRole, (int)WardRole.Resident);
                            WardRole destRequiredRole = (WardRole)roleInt;
                            if (!WardPermissions.HasRequiredRole(destWardZdo, playerId, destRequiredRole))
                            {
                                NotifyBlocked(player, "Destination portal transit blocked: Insufficient clearance at destination");
                                return false;
                            }
                        }
                    }
                }
            }
        }

        return true;
    }

    private static void NotifyBlocked(Player player, string message)
    {
        if (Time.time - _lastNotifyTime > 2.0f)
        {
            _lastNotifyTime = Time.time;
            player.Message(MessageHud.MessageType.Center, message);
        }
    }
}
