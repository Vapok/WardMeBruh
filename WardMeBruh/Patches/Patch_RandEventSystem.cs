using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using WardMeBruh.Configuration;
using WardMeBruh.Services;

namespace WardMeBruh.Patches;

[HarmonyPatch(typeof(RandEventSystem))]
internal static class Patch_RandEventSystem
{
    [HarmonyPatch("CheckBase", typeof(RandomEvent), typeof(RandEventSystem.PlayerEventData))]
    [HarmonyPrefix]
    private static bool Prefix_CheckBase(RandomEvent ev, RandEventSystem.PlayerEventData player, ref bool __result)
    {
        if (ev != null && ev.m_nearBaseOnly && WardQueryService.HasFeatureEnabled(player.position, WardZdoKeys.GhostBase))
        {
            __result = false;
            return false;
        }

        return true;
    }

    [HarmonyPatch("GetValidEventPoints", typeof(RandomEvent), typeof(List<RandEventSystem.PlayerEventData>))]
    [HarmonyPostfix]
    private static void Postfix_GetValidEventPoints(List<Vector3> __result)
    {
        if (__result == null || __result.Count == 0)
            return;

        for (int i = __result.Count - 1; i >= 0; i--)
        {
            if (WardQueryService.HasFeatureEnabled(__result[i], WardZdoKeys.RaidSuppress))
            {
                __result.RemoveAt(i);
            }
        }
    }

    [HarmonyPatch("SetRandomEvent")]
    [HarmonyPrefix]
    private static bool Prefix_SetRandomEvent(RandomEvent ev, Vector3 pos)
    {
        if (ev == null)
            return true;

        if (WardQueryService.HasFeatureEnabled(pos, WardZdoKeys.RaidSuppress))
        {
            WardMeBruh.Log.Info($"[Raid Event] Raid '{ev.m_name}' suppressed by active ward at {pos}");
            return false;
        }

        if (ev.m_nearBaseOnly && WardQueryService.HasFeatureEnabled(pos, WardZdoKeys.GhostBase))
        {
            WardMeBruh.Log.Info($"[Raid Event] Base raid '{ev.m_name}' suppressed by Ghost Base at {pos}");
            return false;
        }

        return true;
    }
}
