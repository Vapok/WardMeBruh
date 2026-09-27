using HarmonyLib;
using UnityEngine;
using WardMeBruh.Configuration;
using WardMeBruh.Models;
using WardMeBruh.Services;

namespace WardMeBruh.Patches;

[HarmonyPatch(typeof(Player), "UpdateEnvStatusEffects")]
internal static class Patch_WardStatusEffect
{
    private static SE_WardTerritory _template;

    public static SE_WardTerritory GetOrCreateTemplate()
    {
        if (_template == null)
        {
            _template = ScriptableObject.CreateInstance<SE_WardTerritory>();
            _template.name = "SE_WardTerritory";
            _template.m_name = "Ward Territory";
            _template.m_icon = WardIconHelper.GetWardIcon();
        }
        return _template;
    }

    [HarmonyPostfix]
    private static void UpdateEnvStatusEffectsPostfix(Player __instance)
    {
        if (ConfigRegistry.Enabled != null && !ConfigRegistry.Enabled.Value)
            return;

        if (__instance == null || __instance.GetSEMan() == null)
            return;

        PrivateArea ward = WardQueryService.GetCoveringWard(__instance.transform.position);
        if (ward != null && ward.m_nview != null && ward.m_nview.IsValid())
        {
            if (!__instance.GetSEMan().HaveStatusEffect(SE_WardTerritory.EffectHash))
            {
                SE_WardTerritory template = GetOrCreateTemplate();
                __instance.GetSEMan().AddStatusEffect(template);
            }
        }
        else
        {
            if (__instance.GetSEMan().HaveStatusEffect(SE_WardTerritory.EffectHash))
            {
                __instance.GetSEMan().RemoveStatusEffect(SE_WardTerritory.EffectHash);
            }
        }
    }
}
