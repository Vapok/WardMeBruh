using HarmonyLib;
using WardMeBruh.UI;

namespace WardMeBruh.Patches;

[HarmonyPatch(typeof(Menu), nameof(Menu.Update))]
internal static class Patch_Menu
{
    [HarmonyPrefix]
    private static bool Prefix(Menu __instance)
    {
        if (__instance == null)
        {
            return true;
        }

        if (__instance.m_root != null && __instance.m_root.gameObject.activeSelf)
        {
            return true;
        }

        if (WardMainWindow.Instance != null && (WardMainWindow.Instance.IsOpen || WardMainWindow.LastClosedFrame == UnityEngine.Time.frameCount))
        {
            return false;
        }

        if (EntityRenameDialog.Instance != null && (EntityRenameDialog.Instance.IsOpen || EntityRenameDialog.LastClosedFrame == UnityEngine.Time.frameCount))
        {
            return false;
        }

        return true;
    }
}
