using HarmonyLib;
using WardMeBruh.Networking;
using WardMeBruh.Services;

namespace WardMeBruh.Patches;

[HarmonyPatch(typeof(ZDOMan))]
internal static class Patch_ZDOMan
{
    [HarmonyPatch("HandleDestroyedZDO")]
    [HarmonyPrefix]
    private static void HandleDestroyedZDOPrefix(ZDOID uid)
    {
        if (WardRegistryManager.Instance.GetKnownWard(uid) != null)
        {
            WardMeBruh.Log.Debug($"[Ward Lifecycle] Unregistering Ward {uid} on ZDOMan.HandleDestroyedZDO");
            WardRegistryManager.Instance.Unregister(uid);

            if (ZNet.instance != null && ZNet.instance.IsServer())
            {
                WardNetworkRouter.BroadcastRegistry();
            }
        }
    }
}
