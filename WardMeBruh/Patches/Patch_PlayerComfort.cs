using HarmonyLib;
using UnityEngine;
using WardMeBruh.Configuration;
using WardMeBruh.Models;
using WardMeBruh.Services;

namespace WardMeBruh.Patches;

[HarmonyPatch(typeof(Player))]
internal static class Patch_PlayerComfort
{
    private static readonly AccessTools.FieldRef<Player, bool> s_underRoofRef = 
        AccessTools.FieldRefAccess<Player, bool>("m_underRoof");

    [HarmonyPatch(nameof(Player.GetComfortLevel))]
    [HarmonyPostfix]
    private static void GetComfortLevelPostfix(Player __instance, ref int __result)
    {
        if (ConfigRegistry.Enabled != null && !ConfigRegistry.Enabled.Value)
            return;

        if (__instance == null)
            return;

        PrivateArea ward = WardQueryService.GetCoveringWard(__instance.transform.position);
        if (ward == null || !ward.IsEnabled() || !WardPermissions.HasBenefactorAccess(ward, __instance.GetPlayerID()))
            return;

        ZDO wardZdo = (ward.m_nview != null && ward.m_nview.IsValid()) ? ward.m_nview.GetZDO() : null;
        if (wardZdo == null || !WardQueryService.HasFeatureEnabled(wardZdo, WardZdoKeys.MaxComfort, __instance.GetPlayerID()))
            return;

        int wardMaxComfort = WardComfortService.GetWardComfort(ward);
        if (wardMaxComfort > __result)
        {
            __result = wardMaxComfort;
        }
    }

    [HarmonyPatch(nameof(Player.InShelter))]
    [HarmonyPostfix]
    private static void InShelterPostfix(Player __instance, ref bool __result)
    {
        if (__result)
            return;

        if (ConfigRegistry.Enabled != null && !ConfigRegistry.Enabled.Value)
            return;

        if (__instance == null)
            return;

        if (WardQueryService.HasFeatureEnabled(__instance.transform.position, WardZdoKeys.WeatherRoof, __instance.GetPlayerID()) ||
            WardQueryService.HasFeatureEnabled(__instance.transform.position, WardZdoKeys.MaxComfort, __instance.GetPlayerID()))
        {
            __result = true;
        }
    }

    [HarmonyPatch("UpdateEnvStatusEffects")]
    [HarmonyPrefix]
    private static void UpdateEnvStatusEffectsPrefix(Player __instance)
    {
        if (ConfigRegistry.Enabled != null && !ConfigRegistry.Enabled.Value)
            return;

        if (__instance == null)
            return;

        if (WardQueryService.HasFeatureEnabled(__instance.transform.position, WardZdoKeys.WeatherRoof))
        {
            s_underRoofRef(__instance) = true;
        }
        else
        {
            s_underRoofRef(__instance) = Cover.IsUnderRoof(__instance.GetCenterPoint());
        }

        if (WardQueryService.HasFeatureEnabled(__instance.transform.position, WardZdoKeys.MaxComfort, __instance.GetPlayerID()))
        {
            __instance.OnNearFire(__instance.transform.position);
        }
    }

    [HarmonyPatch("UpdateEnvStatusEffects")]
    [HarmonyPostfix]
    private static void UpdateEnvStatusEffectsPostfix(Player __instance)
    {
        if (ConfigRegistry.Enabled != null && !ConfigRegistry.Enabled.Value)
            return;

        if (__instance == null || __instance.GetSEMan() == null)
            return;

        bool hasWeatherRoof = WardQueryService.HasFeatureEnabled(__instance.transform.position, WardZdoKeys.WeatherRoof, __instance.GetPlayerID());
        bool hasMaxComfort = WardQueryService.HasFeatureEnabled(__instance.transform.position, WardZdoKeys.MaxComfort, __instance.GetPlayerID());

        if (hasWeatherRoof || hasMaxComfort)
        {
            __instance.GetSEMan().AddStatusEffect(SEMan.s_statusEffectShelter, resetTime: false, 0, 0f, -1);
        }

        if (hasMaxComfort)
        {
            if (!__instance.IsSensed() && !__instance.GetSEMan().HaveStatusEffect(SEMan.s_statusEffectBurning))
            {
                __instance.GetSEMan().AddStatusEffect(SEMan.s_statusEffectCampFire, resetTime: false, 0, 0f, -1);

                if (!__instance.GetSEMan().HaveStatusEffect(SEMan.s_statusEffectWet) && __instance.InShelter())
                {
                    __instance.GetSEMan().AddStatusEffect(SEMan.s_statusEffectResting, resetTime: false, 0, 0f, -1);
                }
            }
        }
    }
}
