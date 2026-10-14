using HarmonyLib;
using WardMeBruh.Configuration;
using WardMeBruh.Services;

namespace WardMeBruh.Patches;

[HarmonyPatch(typeof(CreatureSpawner))]
internal static class Patch_CreatureSpawner
{
    [HarmonyPatch("Spawn")]
    [HarmonyPrefix]
    private static bool Prefix_Spawn(CreatureSpawner __instance, ref ZNetView __result)
    {
        if (__instance != null && WardQueryService.HasFeatureEnabled(__instance.transform.position, WardZdoKeys.SpawnSuppress))
        {
            __result = null;
            return false;
        }

        return true;
    }
}
