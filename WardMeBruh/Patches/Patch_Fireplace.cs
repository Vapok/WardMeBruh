using System.Collections.Generic;
using HarmonyLib;

namespace WardMeBruh.Patches;

[HarmonyPatch(typeof(Fireplace))]
internal static class Patch_Fireplace
{
    public static readonly List<Fireplace> AllFireplaces = new();

    [HarmonyPatch(nameof(Fireplace.Awake))]
    [HarmonyPostfix]
    private static void AwakePostfix(Fireplace __instance)
    {
        if (__instance != null && !AllFireplaces.Contains(__instance))
        {
            AllFireplaces.Add(__instance);
        }
    }
}
