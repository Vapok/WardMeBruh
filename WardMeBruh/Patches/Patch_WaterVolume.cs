using HarmonyLib;
using UnityEngine;
using WardMeBruh.Configuration;
using WardMeBruh.Services;

namespace WardMeBruh.Patches;

[HarmonyPatch(typeof(WaterVolume))]
internal static class Patch_WaterVolume
{
    private const float DrainedSurfaceLevel = -1000f;

    [HarmonyPatch(nameof(WaterVolume.GetWaterSurface))]
    [HarmonyPostfix]
    private static void Postfix_GetWaterSurface(Vector3 point, ref float __result)
    {
        if (!WardQueryService.HasAnyActiveWaterDrain())
            return;

        if (WardQueryService.HasFeatureEnabled(point, WardZdoKeys.WaterDrain))
        {
            __result = DrainedSurfaceLevel;
        }
    }
}
