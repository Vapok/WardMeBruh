using HarmonyLib;
using UnityEngine;
using WardMeBruh.Configuration;
using WardMeBruh.Networking;
using WardMeBruh.Services;
using WardMeBruh.UI;

namespace WardMeBruh.Patches;

[HarmonyPatch(typeof(WearNTear))]
internal static class Patch_WearNTear
{
    private static readonly AccessTools.FieldRef<WearNTear, bool> s_haveRoofRef = 
        AccessTools.FieldRefAccess<WearNTear, bool>("m_haveRoof");

    private static readonly AccessTools.FieldRef<WearNTear, bool> s_haveAshRoofRef = 
        AccessTools.FieldRefAccess<WearNTear, bool>("m_haveAshRoof");

    private static readonly AccessTools.FieldRef<WearNTear, bool> s_rainWetRef = 
        AccessTools.FieldRefAccess<WearNTear, bool>("m_rainWet");

    [HarmonyPatch("HaveRoof")]
    [HarmonyPostfix]
    private static void Postfix_HaveRoof(WearNTear __instance, ref bool __result)
    {
        if (__result)
            return;

        if (WardQueryService.HasFeatureEnabled(__instance.transform.position, WardZdoKeys.WeatherRoof))
        {
            __result = true;
        }
    }

    [HarmonyPatch("HaveAshRoof")]
    [HarmonyPostfix]
    private static void Postfix_HaveAshRoof(WearNTear __instance, ref bool __result)
    {
        if (__result)
            return;

        if (WardQueryService.HasFeatureEnabled(__instance.transform.position, WardZdoKeys.AshShield))
        {
            __result = true;
        }
    }

    [HarmonyPatch(nameof(WearNTear.UpdateWear))]
    [HarmonyPrefix]
    private static void Prefix_UpdateWear(WearNTear __instance)
    {
        if (ConfigRegistry.Enabled != null && !ConfigRegistry.Enabled.Value)
            return;

        Vector3 position = __instance.transform.position;
        ZDO wardZdo = WardQueryService.GetCoveringWardZdo(position);
        if (wardZdo == null || !wardZdo.GetBool(ZDOVars.s_enabled, true))
            return;

        if (wardZdo.GetBool(WardZdoKeys.WeatherRoof, false))
        {
            s_haveRoofRef(__instance) = true;
            s_rainWetRef(__instance) = false;
        }

        if (wardZdo.GetBool(WardZdoKeys.AshShield, false))
        {
            s_haveAshRoofRef(__instance) = true;
            __instance.m_ashDamageImmune = true;
        }

        if (wardZdo.GetBool(WardZdoKeys.LavaShield, false))
        {
            __instance.m_ashDamageImmune = true;
        }

        if (wardZdo.GetBool(WardZdoKeys.SnowShield, false))
        {
            __instance.m_snowDamageImmune = true;
        }
    }

    [HarmonyPatch(nameof(WearNTear.Destroy))]
    [HarmonyPrefix]
    private static void Prefix_Destroy(WearNTear __instance)
    {
        if (__instance == null || __instance.m_nview == null || !__instance.m_nview.IsValid())
            return;

        PrivateArea ward = __instance.GetComponent<PrivateArea>();
        if (ward != null)
        {
            ZDOID wardId = __instance.m_nview.GetZDO().m_uid;
            string wardName = __instance.m_nview.GetZDO().GetString(WardZdoKeys.CustomName, ward.m_name);
            WardMeBruh.Log.Debug($"[Ward Lifecycle] Unregistering Ward {wardId} ('{wardName}') on WearNTear.Destroy");
            WardRegistryManager.Instance.Unregister(wardId);

            if (ZNet.instance != null && ZNet.instance.IsServer())
            {
                WardNetworkRouter.BroadcastRegistry();
            }
            else
            {
                WardNetworkRouter.SendUnregisterWard(wardId);
            }
        }
    }
}
