using HarmonyLib;
using UnityEngine;
using WardMeBruh.Configuration;
using WardMeBruh.Networking;
using WardMeBruh.Services;

namespace WardMeBruh.Patches;

[HarmonyPatch(typeof(ZNetScene))]
internal static class Patch_ZNetScene
{
    [HarmonyPatch(nameof(ZNetScene.Destroy))]
    [HarmonyPrefix]
    private static void DestroyPrefix(GameObject go)
    {
        if (go == null)
            return;

        PrivateArea ward = go.GetComponent<PrivateArea>();
        if (ward != null && ward.m_nview != null && ward.m_nview.GetZDO() != null)
        {
            ZDOID wardId = ward.m_nview.GetZDO().m_uid;
            string wardName = ward.m_nview.GetZDO().GetString(WardZdoKeys.CustomName, ward.m_name);
            WardMeBruh.Log.Debug($"[Ward Lifecycle] Unregistering Ward {wardId} ('{wardName}') on ZNetScene.Destroy");
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
