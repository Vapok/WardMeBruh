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

        if (ShieldGenerator.m_shieldDomeEffect != null)
        {
            ShieldGenerator.m_shieldDomeEffect.RemoveShield(__instance);
        }

        ShieldGenerator.m_instances.Remove(__instance);
        ShieldGenerator.m_instanceChangeID++;
        Character.SetupContinuousEffect(__instance.transform, __instance.transform.position, false, __instance.m_shieldLowLoop, ref __instance.m_lowLoopInstances);

        return false;
    }
}
