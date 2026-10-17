using HarmonyLib;
using WardMeBruh.UI;

namespace WardMeBruh.Patches;

[HarmonyPatch(typeof(PlayerController), "TakeInput")]
internal static class Patch_PlayerController
{
    [HarmonyPrefix]
    private static bool Prefix(ref bool __result)
    {
        if (WardMainWindow.HasInstanceAndOpen)
        {
            __result = false;
            return false;
        }

        if (EntityRenameDialog.HasInstanceAndOpen)
        {
            __result = false;
            return false;
        }

        return true;
    }
}
