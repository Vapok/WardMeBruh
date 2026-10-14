using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using WardMeBruh.Configuration;
using WardMeBruh.Models;
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

        if (WardQueryService.HasFeatureEnabled(wardZdo, WardZdoKeys.WeatherRoof))
        {
            s_haveRoofRef(__instance) = true;
            s_rainWetRef(__instance) = false;
        }

        if (WardQueryService.HasFeatureEnabled(wardZdo, WardZdoKeys.AshShield))
        {
            s_haveAshRoofRef(__instance) = true;
            __instance.m_ashDamageImmune = true;
        }

        if (WardQueryService.HasFeatureEnabled(wardZdo, WardZdoKeys.LavaShield))
        {
            __instance.m_ashDamageImmune = true;
        }

        if (WardQueryService.HasFeatureEnabled(wardZdo, WardZdoKeys.SnowShield))
        {
            __instance.m_snowDamageImmune = true;
        }
    }

    private static readonly HashSet<ZDOID> s_destroyingWards = new();

    [HarmonyPatch(nameof(WearNTear.Destroy))]
    [HarmonyPrefix]
    private static void Prefix_Destroy(WearNTear __instance)
    {
        if (__instance == null || __instance.m_nview == null || !__instance.m_nview.IsValid())
            return;

        PrivateArea ward = __instance.GetComponent<PrivateArea>();
        if (ward != null)
        {
            ZDO zdo = __instance.m_nview.GetZDO();
            ZDOID wardId = zdo.m_uid;

            if (!s_destroyingWards.Add(wardId))
                return;

            try
            {
                WardType wardType = (WardType)zdo.GetInt(WardZdoKeys.WardType, (int)WardType.Main);
                if (wardType == WardType.Main)
                {
                    DestroyConnectedExtensionWards(wardId);
                }

                string wardName = zdo.GetString(WardZdoKeys.CustomName, ward.m_name);
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
            finally
            {
                s_destroyingWards.Remove(wardId);
            }
        }
    }

    private static void DestroyConnectedExtensionWards(ZDOID mainWardId)
    {
        List<WearNTear> extensionsToDestroy = new();

        if (PrivateArea.m_allAreas != null)
        {
            for (int i = 0; i < PrivateArea.m_allAreas.Count; i++)
            {
                PrivateArea area = PrivateArea.m_allAreas[i];
                if (area == null || area.m_nview == null || !area.m_nview.IsValid())
                    continue;

                ZDO aZdo = area.m_nview.GetZDO();
                WardType aType = (WardType)aZdo.GetInt(WardZdoKeys.WardType, (int)WardType.Main);
                ZDOID aParent = aZdo.GetZDOID(WardZdoKeys.ParentWard);
                if (aType == WardType.Extension && aParent == mainWardId)
                {
                    WearNTear wnt = area.GetComponent<WearNTear>();
                    if (wnt != null && !extensionsToDestroy.Contains(wnt))
                    {
                        extensionsToDestroy.Add(wnt);
                    }
                }
            }
        }

        if (WardRegistryManager.Instance != null)
        {
            List<KnownExtensionWard> registeredExts = WardRegistryManager.Instance.GetExtensionsForParent(mainWardId);
            for (int i = 0; i < registeredExts.Count; i++)
            {
                KnownExtensionWard ext = registeredExts[i];
                if (ZDOMan.instance != null)
                {
                    ZDO extZdo = ZDOMan.instance.GetZDO(ext.Id);
                    if (extZdo != null && extZdo.IsValid())
                    {
                        ZNetView nview = (ZNetScene.instance != null) ? ZNetScene.instance.FindInstance(extZdo) : null;
                        if (nview != null)
                        {
                            WearNTear wnt = nview.GetComponent<WearNTear>();
                            if (wnt != null && !extensionsToDestroy.Contains(wnt))
                            {
                                extensionsToDestroy.Add(wnt);
                                continue;
                            }
                        }

                        DropExtensionVanillaResources(extZdo.GetPosition());
                        if (extZdo.IsOwner())
                        {
                            ZDOMan.instance.DestroyZDO(extZdo);
                        }
                    }
                }
                WardRegistryManager.Instance.Unregister(ext.Id);
            }
        }

        for (int i = 0; i < extensionsToDestroy.Count; i++)
        {
            WearNTear wnt = extensionsToDestroy[i];
            if (wnt != null && wnt.gameObject != null)
            {
                wnt.Remove();
            }
        }
    }

    private static void DropExtensionVanillaResources(Vector3 position)
    {
        GameObject wardPrefab = ZNetScene.instance != null ? ZNetScene.instance.GetPrefab("piece_protect") : null;
        if (wardPrefab == null)
            return;

        Piece piece = wardPrefab.GetComponent<Piece>();
        if (piece == null || piece.m_resources == null)
            return;

        for (int r = 0; r < piece.m_resources.Length; r++)
        {
            Piece.Requirement req = piece.m_resources[r];
            if (req != null && req.m_resItem != null && req.m_amount > 0)
            {
                Vector3 dropPos = position + Vector3.up * 0.5f + UnityEngine.Random.insideUnitSphere * 0.25f;
                ItemDrop.DropItem(req.m_resItem.m_itemData, req.m_amount, dropPos, Quaternion.identity);
            }
        }
    }
}
