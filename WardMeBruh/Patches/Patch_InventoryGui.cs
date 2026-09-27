using HarmonyLib;
using TMPro;
using WardMeBruh.Configuration;

namespace WardMeBruh.Patches;

[HarmonyPatch(typeof(InventoryGui))]
internal static class Patch_InventoryGui
{
    [HarmonyPatch("UpdateContainer")]
    [HarmonyPostfix]
    private static void UpdateContainerPostfix(InventoryGui __instance)
    {
        if (ConfigRegistry.Enabled != null && !ConfigRegistry.Enabled.Value)
            return;

        if (__instance == null || __instance.m_container == null || __instance.m_currentContainer == null)
            return;

        if (__instance.m_currentContainer.m_nview == null || !__instance.m_currentContainer.m_nview.IsValid())
            return;

        ZDO zdo = __instance.m_currentContainer.m_nview.GetZDO();
        string customName = zdo.GetString(WardZdoKeys.ContainerName, string.Empty);
        if (!string.IsNullOrEmpty(customName) && !customName.Equals("Container", System.StringComparison.OrdinalIgnoreCase))
        {
            __instance.m_containerName.text = $"<color=#fbbf24><b>{customName}</b></color>";
        }
    }
}
