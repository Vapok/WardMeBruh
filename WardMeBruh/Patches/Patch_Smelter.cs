using System.Collections.Generic;
using HarmonyLib;

namespace WardMeBruh.Patches;

[HarmonyPatch(typeof(Smelter))]
internal static class Patch_Smelter
{
    public static readonly List<Smelter> AllSmelters = new();

    [HarmonyPatch(nameof(Smelter.Awake))]
    [HarmonyPostfix]
    private static void AwakePostfix(Smelter __instance)
    {
        if (__instance != null && !AllSmelters.Contains(__instance))
        {
            AllSmelters.Add(__instance);
        }
    }
}
