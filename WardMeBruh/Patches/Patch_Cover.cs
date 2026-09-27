using HarmonyLib;
using UnityEngine;
using WardMeBruh.Configuration;
using WardMeBruh.Services;

namespace WardMeBruh.Patches;

[HarmonyPatch(typeof(Cover), nameof(Cover.IsUnderRoof))]
internal static class Patch_Cover
{
    [HarmonyPostfix]
    private static void Postfix(Vector3 startPos, ref bool __result)
    {
        if (__result)
            return;

        if (ConfigRegistry.Enabled != null && !ConfigRegistry.Enabled.Value)
            return;

        ZDO wardZdo = WardQueryService.GetCoveringWardZdo(startPos);
        if (wardZdo == null || !wardZdo.GetBool(ZDOVars.s_enabled, true))
            return;

        if (wardZdo.GetBool(WardZdoKeys.WeatherRoof, false) ||
            wardZdo.GetBool(WardZdoKeys.MaxComfort, false))
        {
            __result = true;
        }
    }
}
