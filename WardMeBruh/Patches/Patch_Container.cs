using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using WardMeBruh.Configuration;
using WardMeBruh.Models;
using WardMeBruh.Services;
using WardMeBruh.UI;

namespace WardMeBruh.Patches;

[HarmonyPatch(typeof(Container))]
internal static class Patch_Container
{
    public static readonly List<Container> AllContainers = new();
    public static event Action<Container> OnRenameContainerRequested;

    [HarmonyPatch(nameof(Container.Awake))]
    [HarmonyPostfix]
    private static void AwakePostfix(Container __instance)
    {
        if (__instance != null && !AllContainers.Contains(__instance))
        {
            AllContainers.Add(__instance);
        }
    }

    [HarmonyPatch(nameof(Container.Interact))]
    [HarmonyPrefix]
    private static bool InteractPrefix(Container __instance, Humanoid character, bool hold, bool alt)
    {
        if (hold)
            return false;

        if (__instance == null || __instance.m_nview == null || !__instance.m_nview.IsValid())
            return true;

        if (WardMainWindow.Instance != null && WardMainWindow.Instance.IsOpen)
            return false;

        if (EntityRenameDialog.Instance != null && EntityRenameDialog.Instance.IsOpen)
            return false;

        if (UnityEngine.EventSystems.EventSystem.current != null && UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject != null)
        {
            if (UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.GetComponent<UnityEngine.UI.InputField>() != null)
                return false;
        }

        Player player = character as Player;
        if (player == null)
            return true;

        PrivateArea coveringWard = WardQueryService.GetCoveringWard(__instance.transform.position);
        long playerId = player.GetPlayerID();
        bool isMoving = player.IsRunning() || player.m_run || Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D);
        bool hasUseInput = ZInput.GetButton("Use") || ZInput.GetButtonDown("Use") || ZInput.GetButton("JoyUse") || ZInput.GetButtonDown("JoyUse");
        bool isAlt = hasUseInput && ((alt && !isMoving) || Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl) || Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt));

        if (isAlt)
        {
            bool canRename = false;
            if (coveringWard != null)
            {
                ZDO wardZdo = coveringWard.m_nview.GetZDO();
                canRename = WardPermissions.HasRequiredRole(wardZdo, playerId, WardRole.Resident);
            }
            else
            {
                Piece piece = __instance.GetComponent<Piece>();
                canRename = piece != null && (piece.IsCreator() || piece.GetCreator() == playerId || piece.GetCreator() == 0L);
            }

            if (canRename)
            {
                string containerName = __instance.m_nview.GetZDO().GetString(WardZdoKeys.ContainerName, "Chest");
                WardMeBruh.Log.Debug($"[UI] Player '{player.GetPlayerName()}' ({playerId}) opening rename dialog for Container '{containerName}' ({__instance.m_nview.GetZDO().m_uid})");
                OnRenameContainerRequested?.Invoke(__instance);
                EntityRenameDialog.Instance?.OpenForContainer(__instance);
                return false;
            }
        }

        if (coveringWard != null)
        {
            ZDO wardZdo = coveringWard.m_nview.GetZDO();
            ZDO containerZdo = __instance.m_nview.GetZDO();
            WardRole requiredRole = (WardRole)containerZdo.GetInt(WardZdoKeys.ContainerMinRole, (int)WardRole.Resident);
            WardRole playerRole = WardPermissions.GetPlayerRole(wardZdo, playerId);
            string containerName = containerZdo.GetString(WardZdoKeys.ContainerName, "Chest");
            string wardName = wardZdo.GetString(WardZdoKeys.CustomName, "Ward");

            if (!WardPermissions.CanAccessContainer(containerZdo, wardZdo, playerId))
            {
                WardMeBruh.Log.Debug($"[Access DENIED] Player '{player.GetPlayerName()}' ({playerId}, Role: {playerRole}) denied access to Container '{containerName}' ({containerZdo.m_uid}) in Ward '{wardName}' ({wardZdo.m_uid}). Required Role: {requiredRole}");
                character.Message(MessageHud.MessageType.Center, "$msg_privatezone");
                return false;
            }

            WardMeBruh.Log.Debug($"[Access GRANTED] Player '{player.GetPlayerName()}' ({playerId}, Role: {playerRole}) opened Container '{containerName}' ({containerZdo.m_uid}) in Ward '{wardName}' ({wardZdo.m_uid}). Required Role: {requiredRole}");
        }

        return true;
    }

    [HarmonyPatch(nameof(Container.GetHoverText))]
    [HarmonyPostfix]
    private static void GetHoverTextPostfix(Container __instance, ref string __result)
    {
        if (__instance == null || __instance.m_nview == null || !__instance.m_nview.IsValid())
            return;

        ZDO containerZdo = __instance.m_nview.GetZDO();
        string customName = containerZdo.GetString(WardZdoKeys.ContainerName, string.Empty);
        int minRoleInt = containerZdo.GetInt(WardZdoKeys.ContainerMinRole, (int)WardRole.Resident);
        WardRole minRole = (WardRole)minRoleInt;
        bool isFuelDepot = containerZdo.GetBool(WardZdoKeys.ContainerIsFuel, false);

        string prefix = string.IsNullOrEmpty(customName) ? string.Empty : $"<color=#fbbf24><b>[{customName}]</b></color>\n";

        if (isFuelDepot)
        {
            prefix += $"<color=#f97316>[{"$wmb_access_fuel_depot".Localize()}]</color> ";
        }

        PrivateArea coveringWard = WardQueryService.GetCoveringWard(__instance.transform.position);
        bool canRename = false;
        long playerId = Player.m_localPlayer != null ? Player.m_localPlayer.GetPlayerID() : 0L;

        if (coveringWard != null)
        {
            ZDO wardZdo = coveringWard.m_nview.GetZDO();

            if (minRole > WardRole.Resident)
            {
                bool hasAccess = WardPermissions.CanAccessContainer(containerZdo, wardZdo, playerId);
                string colorHex = hasAccess ? "#a3e635" : "#f87171";
                prefix += $"<color={colorHex}>[{WardRoleManager.GetRoleName(minRole).Localize()} {"$wmb_clearance".Localize()}]</color>\n";
            }

            canRename = WardPermissions.HasRequiredRole(wardZdo, playerId, WardRole.Resident);
        }
        else
        {
            Piece piece = __instance.GetComponent<Piece>();
            canRename = piece != null && (piece.IsCreator() || piece.GetCreator() == playerId || piece.GetCreator() == 0L);
        }

        if (canRename)
        {
            string renamePrompt = Localization.instance.Localize($"\n[<color=yellow><b>LeftShift + $KEY_Use</b></color>] {"$wmb_configure_chest".Localize()}");
            __result += renamePrompt;
        }

        __result = prefix + __result;
    }
}
