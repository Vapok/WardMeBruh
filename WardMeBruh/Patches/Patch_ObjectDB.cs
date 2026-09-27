using HarmonyLib;
using WardMeBruh.Models;

namespace WardMeBruh.Patches;

[HarmonyPatch(typeof(ObjectDB), nameof(ObjectDB.Awake))]
internal static class Patch_ObjectDB
{
    [HarmonyPostfix]
    private static void AwakePostfix(ObjectDB __instance)
    {
        if (__instance == null || __instance.m_StatusEffects == null)
            return;

        SE_WardTerritory template = Patch_WardStatusEffect.GetOrCreateTemplate();
        if (!__instance.m_StatusEffects.Contains(template))
        {
            __instance.m_StatusEffects.Add(template);
        }
    }
}
