using HarmonyLib;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using WardMeBruh.Configuration;
using WardMeBruh.Models;
using WardMeBruh.Services;
using WardMeBruh.UI;

namespace WardMeBruh.Patches;

[HarmonyPatch(typeof(Player), nameof(Player.Update))]
internal static class Patch_PlayerInput
{
    [HarmonyPostfix]
    private static void UpdatePostfix(Player __instance)
    {
        if (ConfigRegistry.Enabled != null && !ConfigRegistry.Enabled.Value)
            return;

        if (__instance == null || __instance != Player.m_localPlayer || __instance.IsDead())
            return;

        if (Chat.instance != null && Chat.instance.HasFocus())
            return;

        if (Console.IsVisible() || TextInput.IsVisible() || Menu.IsVisible() || Minimap.IsOpen())
            return;

        if (WardMainWindow.Instance != null && WardMainWindow.Instance.IsOpen)
            return;

        if (EntityRenameDialog.Instance != null && EntityRenameDialog.Instance.IsOpen)
            return;

        if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject != null)
        {
            if (EventSystem.current.currentSelectedGameObject.GetComponent<InputField>() != null)
                return;
        }

        if (ConfigRegistry.OpenWardHubKey == null)
            return;

        if (ConfigRegistry.OpenWardHubKey.Value.MainKey == KeyCode.None || ConfigRegistry.OpenWardHubKey.Value.MainKey == KeyCode.E)
            return;

        if (!ConfigRegistry.OpenWardHubKey.Value.IsDown())
            return;

        GameObject hoverObj = __instance.GetHoverObject();
        if (hoverObj == null)
            return;

        PrivateArea ward = hoverObj.GetComponentInParent<PrivateArea>();
        if (ward == null || ward.m_nview == null || !ward.m_nview.IsValid())
            return;

        ZDO zdo = ward.m_nview.GetZDO();
        ZDO effectiveZdo = WardQueryService.GetEffectiveWardZdo(zdo);
        long playerId = __instance.GetPlayerID();

        if (WardRoleManager.CanPerform(effectiveZdo, playerId, "core.view_gui"))
        {
            Patch_PrivateArea.RequestOpenWardGui(ward);
        }
    }

    [HarmonyPatch("TakeInput")]
    [HarmonyPrefix]
    private static bool TakeInputPrefix(ref bool __result)
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

        if (WardPlacementChoiceModal.Instance != null && WardPlacementChoiceModal.Instance.IsOpen)
        {
            __result = false;
            return false;
        }

        return true;
    }
}
