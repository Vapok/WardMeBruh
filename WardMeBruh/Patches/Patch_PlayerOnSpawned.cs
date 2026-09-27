using HarmonyLib;
using WardMeBruh.Services;

namespace WardMeBruh.Patches;

[HarmonyPatch(typeof(Player), nameof(Player.OnSpawned))]
internal static class Patch_PlayerOnSpawned
{
    private static void Postfix(Player __instance)
    {
        if (__instance != null && __instance.m_nview != null && __instance.m_nview.IsValid())
        {
            long id = __instance.GetPlayerID();
            string name = __instance.GetPlayerName();
            if (id != 0L && !string.IsNullOrEmpty(name))
            {
                PlayerRegistryService.RegisterAndPersistPlayer(id, name);
            }
        }
    }
}
