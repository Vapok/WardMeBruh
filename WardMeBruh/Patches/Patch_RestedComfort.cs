using HarmonyLib;
using UnityEngine;
using WardMeBruh.Configuration;
using WardMeBruh.Services;

namespace WardMeBruh.Patches;

[HarmonyPatch(typeof(SE_Rested))]
internal static class Patch_RestedComfort
{
    [HarmonyPatch(nameof(SE_Rested.CalculateComfortLevel), typeof(bool), typeof(Vector3))]
    [HarmonyPostfix]
    private static void CalculateComfortLevelPostfix(bool inShelter, Vector3 position, ref int __result)
    {
        if (ConfigRegistry.Enabled != null && !ConfigRegistry.Enabled.Value)
            return;

        PrivateArea ward = WardQueryService.GetCoveringWard(position);
        if (ward == null || !ward.IsEnabled())
            return;

        ZDO wardZdo = (ward.m_nview != null && ward.m_nview.IsValid()) ? ward.m_nview.GetZDO() : null;
        long playerId = Player.m_localPlayer != null ? Player.m_localPlayer.GetPlayerID() : 0L;
        if (wardZdo == null || !WardQueryService.HasFeatureEnabled(wardZdo, WardZdoKeys.MaxComfort, playerId))
            return;

        int wardMaxComfort = WardComfortService.GetWardComfort(ward);
        if (wardMaxComfort > __result)
        {
            __result = wardMaxComfort;
        }
    }
}
