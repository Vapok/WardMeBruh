using HarmonyLib;
using WardMeBruh.Configuration;
using WardMeBruh.Services;

namespace WardMeBruh.Patches;

[HarmonyPatch(typeof(BaseAI))]
internal static class Patch_BaseAI
{
    [HarmonyPatch("CanSeeTarget", typeof(StaticTarget))]
    [HarmonyPrefix]
    private static bool Prefix_CanSeeTarget(StaticTarget target, ref bool __result)
    {
        if (target == null)
            return true;

        if (WardQueryService.HasFeatureEnabled(target.GetCenter(), WardZdoKeys.GhostBase))
        {
            __result = false;
            return false;
        }

        return true;
    }
}
