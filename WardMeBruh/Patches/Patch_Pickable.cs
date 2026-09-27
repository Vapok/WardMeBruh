using HarmonyLib;
using WardMeBruh.Configuration;
using WardMeBruh.Models;
using WardMeBruh.Services;

namespace WardMeBruh.Patches;

[HarmonyPatch(typeof(Pickable))]
internal static class Patch_Pickable
{
    [HarmonyPatch(nameof(Pickable.Interact))]
    [HarmonyPrefix]
    private static bool InteractPrefix(Pickable __instance, Humanoid character, bool repeat, bool alt)
    {
        if (__instance == null || __instance.m_nview == null || !__instance.m_nview.IsValid())
            return true;

        Player player = character as Player;
        if (player == null)
            return true;

        PrivateArea coveringWard = WardQueryService.GetCoveringWard(__instance.transform.position);
        if (coveringWard == null || coveringWard.m_nview == null || !coveringWard.m_nview.IsValid())
            return true;

        ZDO wardZdo = coveringWard.m_nview.GetZDO();
        long playerId = player.GetPlayerID();
        WardRole playerRole = WardPermissions.GetPlayerRole(wardZdo, playerId);
        string wardName = wardZdo.GetString(WardZdoKeys.CustomName, "Ward");
        string cropName = __instance.GetHoverName();

        if (!WardPermissions.CanHarvest(wardZdo, playerId))
        {
            WardMeBruh.Log.Debug($"[Access DENIED] Player '{player.GetPlayerName()}' ({playerId}, Role: {playerRole}) denied picking '{cropName}' ({__instance.m_nview.GetZDO().m_uid}) in Ward '{wardName}' ({wardZdo.m_uid}). Required Permission: core.harvest");
            player.Message(MessageHud.MessageType.Center, "$msg_privatezone");
            return false;
        }

        WardMeBruh.Log.Debug($"[Access GRANTED] Player '{player.GetPlayerName()}' ({playerId}, Role: {playerRole}) picked '{cropName}' ({__instance.m_nview.GetZDO().m_uid}) in Ward '{wardName}' ({wardZdo.m_uid})");
        return true;
    }
}
