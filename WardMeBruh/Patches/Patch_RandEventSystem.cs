using HarmonyLib;
using UnityEngine;
using WardMeBruh.Configuration;
using WardMeBruh.Services;

namespace WardMeBruh.Patches;

[HarmonyPatch(typeof(RandEventSystem))]
internal static class Patch_RandEventSystem
{
    [HarmonyPatch("SetRandomEvent")]
    [HarmonyPrefix]
    private static bool Prefix_SetRandomEvent(RandomEvent ev, Vector3 pos)
    {
        if (ev == null)
            return true;

        if (WardQueryService.HasFeatureEnabled(pos, WardZdoKeys.RaidSuppress))
        {
            WardMeBruh.Log.Debug($"[Raid Event] Raid '{ev.m_name}' suppressed by active ward at {pos}");
            return false;
        }

        return true;
    }
}
