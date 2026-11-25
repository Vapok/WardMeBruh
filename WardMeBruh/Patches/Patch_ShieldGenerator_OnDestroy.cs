using HarmonyLib;
using WardMeBruh.Components;

namespace WardMeBruh.Patches;

[HarmonyPatch(typeof(ShieldGenerator), nameof(ShieldGenerator.OnDestroy))]
internal static class Patch_ShieldGenerator_OnDestroy
{
    private static bool Prefix(ShieldGenerator __instance)
    {
        if (__instance == null)
        {
            return false;
        }

        if (__instance.GetComponent<WardForcefieldDome>() != null || __instance.m_isPlacementGhost)
        {
            return false;
        }

        return true;
    }
}
