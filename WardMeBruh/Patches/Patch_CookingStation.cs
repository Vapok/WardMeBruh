using System.Collections.Generic;
using HarmonyLib;

namespace WardMeBruh.Patches;

[HarmonyPatch(typeof(CookingStation))]
internal static class Patch_CookingStation
{
    public static readonly List<CookingStation> AllCookingStations = new();

    [HarmonyPatch(nameof(CookingStation.Awake))]
    [HarmonyPostfix]
    private static void AwakePostfix(CookingStation __instance)
    {
        if (__instance != null && !AllCookingStations.Contains(__instance))
        {
            AllCookingStations.Add(__instance);
        }
    }
}
