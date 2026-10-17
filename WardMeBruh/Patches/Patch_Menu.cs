using HarmonyLib;
using UnityEngine;
using UnityEngine.InputSystem;
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

        if (WardMainWindow.HasInstanceAndOpen)
        {
            bool escapePressed = Input.GetKeyDown(KeyCode.Escape) ||
                                 ZInput.GetKeyDown(KeyCode.Escape) ||
                                 (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame) ||
                                 (ZInput.instance != null && (ZInput.GetButtonDown("JoyMenu") || ZInput.GetButtonDown("JoyButtonB")));

            if (escapePressed)
            {
                WardMainWindow.Instance.HandleEscapeKey();
            }

            return false;
        }

        if (WardMainWindow.LastClosedFrame == Time.frameCount)
        {
            return false;
        }

        if (EntityRenameDialog.HasInstanceAndOpen)
        {
            bool escapePressed = Input.GetKeyDown(KeyCode.Escape) ||
                                 ZInput.GetKeyDown(KeyCode.Escape) ||
                                 (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame) ||
                                 (ZInput.instance != null && (ZInput.GetButtonDown("JoyMenu") || ZInput.GetButtonDown("JoyButtonB")));

            if (escapePressed)
            {
                EntityRenameDialog.Instance.Close();
            }

            return false;
        }

        if (EntityRenameDialog.LastClosedFrame == Time.frameCount)
        {
            return false;
        }

        return true;
    }
}
