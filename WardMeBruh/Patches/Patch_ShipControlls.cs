using HarmonyLib;
using WardMeBruh.Configuration;
using WardMeBruh.Models;
using WardMeBruh.Services;

namespace WardMeBruh.Patches;

[HarmonyPatch(typeof(ShipControlls))]
internal static class Patch_ShipControlls
{
    [HarmonyPatch(nameof(ShipControlls.Interact))]
    [HarmonyPrefix]
    private static bool InteractPrefix(ShipControlls __instance, Humanoid character, bool repeat, bool alt)
    {
        if (__instance == null || __instance.m_nview == null || !__instance.m_nview.IsValid())
            return true;

        Player player = character as Player;
        if (player == null)
            return true;

        PrivateArea coveringWard = WardQueryService.GetCoveringWard(__instance.transform.position);
        if (coveringWard == null)
            return true;

        ZDO wardZdo = coveringWard.m_nview.GetZDO();
        bool lockVehicles = wardZdo.GetBool(WardZdoKeys.LockVehicles, false);
        if (!lockVehicles)
            return true;

        long playerId = player.GetPlayerID();
        WardRole playerRole = WardPermissions.GetPlayerRole(wardZdo, playerId);
        string wardName = wardZdo.GetString(WardZdoKeys.CustomName, "Ward");

        if (!WardPermissions.HasRequiredRole(wardZdo, playerId, WardRole.Resident))
        {
            WardMeBruh.Log.Debug($"[Access DENIED] Player '{player.GetPlayerName()}' ({playerId}, Role: {playerRole}) denied access to ShipControlls ({__instance.m_nview.GetZDO().m_uid}) in Ward '{wardName}' ({wardZdo.m_uid}). Required Role: Resident");
            player.Message(MessageHud.MessageType.Center, "$msg_privatezone");
            return false;
        }

        WardMeBruh.Log.Debug($"[Access GRANTED] Player '{player.GetPlayerName()}' ({playerId}, Role: {playerRole}) accessed ShipControlls ({__instance.m_nview.GetZDO().m_uid}) in Ward '{wardName}' ({wardZdo.m_uid})");
        return true;
    }
}
