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

        if (WardMainWindow.HasInstanceAndOpen)
            return false;

        if (EntityRenameDialog.HasInstanceAndOpen)
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
        bool hasUseInput = ZInput.GetButton("Use") || ZInput.GetButtonDown("Use") || ZInput.GetButton("JoyUse") || ZInput.GetButtonDown("JoyUse")
            || (UnityEngine.InputSystem.Gamepad.current != null && UnityEngine.InputSystem.Gamepad.current.buttonSouth.wasPressedThisFrame);
        bool gamepadAlt = ZInput.GetButton("JoyAltKeys") || ZInput.GetButton("JoyLTrigger") || ZInput.GetButton("JoyAltPlace")
            || (UnityEngine.InputSystem.Gamepad.current != null && (UnityEngine.InputSystem.Gamepad.current.leftTrigger.isPressed || UnityEngine.InputSystem.Gamepad.current.leftShoulder.isPressed));
        bool keyboardAlt = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) || Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl) || Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
        bool isAlt = hasUseInput && (alt || gamepadAlt || keyboardAlt);

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

    private static ZDOID _cachedContainerId = ZDOID.None;
    private static string _cachedBaseText = string.Empty;
    private static string _cachedFinalText = string.Empty;
    private static int _cachedMinRole = -1;
    private static bool _cachedIsFuel;
    private static string _cachedCustomName = string.Empty;

    [HarmonyPatch(nameof(Container.GetHoverText))]
    [HarmonyPostfix]
    private static void GetHoverTextPostfix(Container __instance, ref string __result)
    {
        if (__instance == null || __instance.m_nview == null || !__instance.m_nview.IsValid())
            return;

        ZDO containerZdo = __instance.m_nview.GetZDO();
        string customName = containerZdo.GetString(WardZdoKeys.ContainerName, string.Empty);
        int minRoleInt = containerZdo.GetInt(WardZdoKeys.ContainerMinRole, (int)WardRole.Resident);
        bool isFuelDepot = containerZdo.GetBool(WardZdoKeys.ContainerIsFuel, false);

        if (containerZdo.m_uid == _cachedContainerId &&
            string.Equals(__result, _cachedBaseText, StringComparison.Ordinal) &&
            string.Equals(customName, _cachedCustomName, StringComparison.Ordinal) &&
            minRoleInt == _cachedMinRole &&
            isFuelDepot == _cachedIsFuel)
        {
            __result = _cachedFinalText;
            return;
        }

        string originalResult = __result;
        WardRole minRole = (WardRole)minRoleInt;

        string localizedName = Localization.instance.Localize(__instance.m_name);
        if (string.IsNullOrEmpty(localizedName))
        {
            Piece piece = __instance.GetComponent<Piece>();
            if (piece != null)
                localizedName = Localization.instance.Localize(piece.m_name);
        }

        bool hasCustomName = !string.IsNullOrEmpty(customName) && !customName.Equals("Container", StringComparison.OrdinalIgnoreCase);
        string displayName = hasCustomName ? $"<color=#fbbf24><b>{customName}</b></color>" : localizedName;
        if (isFuelDepot)
        {
            displayName = $"[<color=#ea580c>🔥</color>] {displayName}";
        }

        if (hasCustomName || isFuelDepot)
        {
            int nameIndex = -1;
            if (!string.IsNullOrEmpty(localizedName))
            {
                nameIndex = __result.IndexOf(localizedName, StringComparison.Ordinal);
            }

            if (nameIndex >= 0)
            {
                __result = __result.Substring(0, nameIndex) + displayName + __result.Substring(nameIndex + localizedName.Length);
            }
            else
            {
                __result = displayName + "\n" + __result;
            }
        }

        PrivateArea coveringWard = WardQueryService.GetCoveringWard(__instance.transform.position);
        bool canRename = false;
        long playerId = Player.m_localPlayer != null ? Player.m_localPlayer.GetPlayerID() : 0L;

        string statusLine = string.Empty;

        if (coveringWard != null)
        {
            ZDO wardZdo = coveringWard.m_nview.GetZDO();

            if (minRoleInt != (int)WardRole.Resident)
            {
                bool hasAccess = WardPermissions.CanAccessContainer(containerZdo, wardZdo, playerId);
                string colorHex = hasAccess ? "#a3e635" : "#f87171";
                string roleDisplayName = WardRoleManager.GetRoleDisplayName(wardZdo, minRoleInt);
                statusLine += $"<color={colorHex}>[{roleDisplayName} {"$wmb_clearance".Localize()}]</color>";
            }

            canRename = WardPermissions.HasRequiredRole(wardZdo, playerId, WardRole.Resident);
        }
        else
        {
            Piece piece = __instance.GetComponent<Piece>();
            canRename = piece != null && (piece.IsCreator() || piece.GetCreator() == playerId || piece.GetCreator() == 0L);
        }

        statusLine = statusLine.TrimEnd();
        if (!string.IsNullOrEmpty(statusLine))
        {
            int firstNewline = __result.IndexOf('\n');
            if (firstNewline >= 0)
            {
                __result = __result.Substring(0, firstNewline) + "\n" + statusLine + __result.Substring(firstNewline);
            }
            else
            {
                __result = __result + "\n" + statusLine;
            }
        }

        if (canRename)
        {
            string modKey = ZInput.IsGamepadActive() ? WardGamepadNavHelper.GetGamepadGlyph("LT") : "$KEY_AltPlace";
            string useKey = ZInput.IsGamepadActive() ? WardGamepadNavHelper.GetGamepadGlyph("A") : "$KEY_Use";
            string renamePrompt = Localization.instance.Localize($"\n[<color=yellow><b>{modKey} + {useKey}</b></color>] {"$wmb_configure_chest".Localize()}");
            __result += renamePrompt;
        }

        _cachedContainerId = containerZdo.m_uid;
        _cachedBaseText = originalResult;
        _cachedFinalText = __result;
        _cachedMinRole = minRoleInt;
        _cachedIsFuel = isFuelDepot;
        _cachedCustomName = customName;
    }
}
