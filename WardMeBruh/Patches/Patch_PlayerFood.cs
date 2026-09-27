using System.Collections.Generic;
using HarmonyLib;
using WardMeBruh.Configuration;
using WardMeBruh.Services;

namespace WardMeBruh.Patches;

[HarmonyPatch(typeof(Player), "UpdateFood")]
internal static class Patch_PlayerFood
{
    private static readonly AccessTools.FieldRef<Player, List<Player.Food>> s_foodsRef = 
        AccessTools.FieldRefAccess<Player, List<Player.Food>>("m_foods");

    private static readonly AccessTools.FieldRef<Player, float> s_foodTimerRef = 
        AccessTools.FieldRefAccess<Player, float>("m_foodUpdateTimer");

    [HarmonyPrefix]
    private static void Prefix(Player __instance, float dt, bool forceUpdate)
    {
        if (forceUpdate)
            return;

        float timer = s_foodTimerRef(__instance) + dt * Game.m_foodRate;
        if (timer < 1f)
            return;

        if (!WardQueryService.HasFeatureEnabled(__instance.transform.position, WardZdoKeys.FoodFreeze, __instance.GetPlayerID()))
            return;

        List<Player.Food> foods = s_foodsRef(__instance);
        if (foods == null)
            return;

        for (int i = 0; i < foods.Count; i++)
        {
            Player.Food food = foods[i];
            if (food != null)
            {
                food.m_time += 1f;
            }
        }
    }
}
