using HarmonyLib;
using WardMeBruh.Services;

namespace WardMeBruh.Patches;

[HarmonyPatch(typeof(ZNet), "SaveWorld")]
internal static class Patch_ZNetSaveWorld
{
    private static void Postfix()
    {
        PlayerRegistryService.SaveWorld();
    }
}
