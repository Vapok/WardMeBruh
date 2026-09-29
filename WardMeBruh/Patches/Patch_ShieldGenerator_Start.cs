using HarmonyLib;
using WardMeBruh.Components;

namespace WardMeBruh.Patches;

[HarmonyPatch(typeof(ShieldGenerator), "Start")]
internal static class Patch_ShieldGenerator_Start
{
    private static bool Prefix(ShieldGenerator __instance)
    {
        if (__instance == null)
        {
            return false;
        }

        if (__instance.GetComponent<WardForcefieldDome>() != null)
        {
            __instance.enabled = false;
            __instance.m_isPlacementGhost = true;
            return false;
        }

        return true;
    }
}
