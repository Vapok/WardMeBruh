using HarmonyLib;
using WardMeBruh.UI;

namespace WardMeBruh.Patches;

[HarmonyPatch(typeof(PlayerController), "TakeInput")]
internal static class Patch_PlayerController
{
    [HarmonyPrefix]
    private static bool Prefix(ref bool __result)
    {
        if (WardMainWindow.Instance != null && WardMainWindow.Instance.IsOpen)
        {
            __result = false;
            return false;
        }

        if (EntityRenameDialog.Instance != null && EntityRenameDialog.Instance.IsOpen)
        {
            __result = false;
            return false;
        }

        return true;
    }
}
