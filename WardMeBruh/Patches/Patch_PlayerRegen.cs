using HarmonyLib;
using WardMeBruh.Configuration;
using WardMeBruh.Services;

namespace WardMeBruh.Patches;

[HarmonyPatch(typeof(SEMan))]
internal static class Patch_PlayerRegen
{
    private static readonly AccessTools.FieldRef<SEMan, Character> s_characterRef = 
        AccessTools.FieldRefAccess<SEMan, Character>("m_character");

    private const float RegenMultiplier = 1.5f;

    [HarmonyPatch(nameof(SEMan.ModifyHealthRegen))]
    [HarmonyPostfix]
    private static void Postfix_ModifyHealthRegen(SEMan __instance, ref float regenMultiplier)
    {
        Character character = s_characterRef(__instance);
        if (character is Player player && WardQueryService.HasFeatureEnabled(player.transform.position, WardZdoKeys.RegenBoost, player.GetPlayerID()))
        {
            regenMultiplier *= RegenMultiplier;
        }
    }

    [HarmonyPatch(nameof(SEMan.ModifyStaminaRegen))]
    [HarmonyPostfix]
    private static void Postfix_ModifyStaminaRegen(SEMan __instance, ref float staminaMultiplier)
    {
        Character character = s_characterRef(__instance);
        if (character is Player player && WardQueryService.HasFeatureEnabled(player.transform.position, WardZdoKeys.RegenBoost, player.GetPlayerID()))
        {
            staminaMultiplier *= RegenMultiplier;
        }
    }

    [HarmonyPatch(nameof(SEMan.ModifyEitrRegen))]
    [HarmonyPostfix]
    private static void Postfix_ModifyEitrRegen(SEMan __instance, ref float eitrMultiplier)
    {
        Character character = s_characterRef(__instance);
        if (character is Player player && WardQueryService.HasFeatureEnabled(player.transform.position, WardZdoKeys.RegenBoost, player.GetPlayerID()))
        {
            eitrMultiplier *= RegenMultiplier;
        }
    }
}
