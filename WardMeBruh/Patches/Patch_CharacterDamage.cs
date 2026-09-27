using HarmonyLib;
using WardMeBruh.Configuration;
using WardMeBruh.Models;
using WardMeBruh.Services;

namespace WardMeBruh.Patches;

[HarmonyPatch(typeof(Character), "RPC_Damage")]
internal static class Patch_CharacterDamage
{
    [HarmonyPrefix]
    private static bool Prefix(Character __instance, HitData hit)
    {
        if (!WardQueryService.IsTamedOrTaming(__instance))
            return true;

        PrivateArea ward = WardQueryService.GetCoveringWard(__instance.transform.position);
        if (ward == null || !ward.IsEnabled())
            return true;

        string status = __instance.IsTamed() ? "tamed" : "taming";
        Character attacker = hit.GetAttacker();
        if (attacker is Player player)
        {
            if (!WardPermissions.HasAccess(ward, player.GetPlayerID(), WardRole.Resident))
            {
                WardMeBruh.Log.Debug($"[Damage Blocked] Prevented attack on {status} {__instance.name} by unauthorized player '{player.GetPlayerName()}' ({player.GetPlayerID()}) in Ward {ward.m_nview.GetZDO().m_uid}");
                return false;
            }
            return true;
        }

        WardMeBruh.Log.Debug($"[Damage Blocked] Prevented {(attacker != null ? attacker.name : "environmental")} damage to {status} {__instance.name} in protected ward area");
        return false;
    }
}
