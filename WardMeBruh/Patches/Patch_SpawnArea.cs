using HarmonyLib;
using UnityEngine;
using WardMeBruh.Configuration;
using WardMeBruh.Services;

namespace WardMeBruh.Patches;

[HarmonyPatch(typeof(SpawnArea))]
internal static class Patch_SpawnArea
{
    [HarmonyPatch("SpawnOne")]
    [HarmonyPrefix]
    private static bool Prefix_SpawnOne(SpawnArea __instance)
    {
        if (__instance != null && WardQueryService.HasFeatureEnabled(__instance.transform.position, WardZdoKeys.SpawnSuppress))
        {
            return false;
        }

        return true;
    }

    [HarmonyPatch("FindSpawnPoint", new[] { typeof(GameObject), typeof(Vector3) }, new[] { ArgumentType.Normal, ArgumentType.Out })]
    [HarmonyPostfix]
    private static void Postfix_FindSpawnPoint(ref Vector3 point, ref bool __result)
    {
        if (__result && WardQueryService.HasFeatureEnabled(point, WardZdoKeys.SpawnSuppress))
        {
            __result = false;
            point = Vector3.zero;
        }
    }
}
