using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using WardMeBruh.Configuration;
using WardMeBruh.Models;
using WardMeBruh.Services;
using WardMeBruh.UI;

namespace WardMeBruh.Patches;

[HarmonyPatch(typeof(Door))]
internal static class Patch_Door
{
    private static readonly Dictionary<ZDOID, float> _doorCloseTimestamps = new();
    public static event Action<Door> OnRenameDoorRequested;

    [HarmonyPatch(nameof(Door.Interact))]
    [HarmonyPrefix]
    private static bool InteractPrefix(Door __instance, Humanoid character, bool hold, bool alt)
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
                string doorName = __instance.m_nview.GetZDO().GetString(WardZdoKeys.DoorName, "Door");
                WardMeBruh.Log.Debug($"[UI] Player '{player.GetPlayerName()}' ({playerId}) opening rename dialog for Door '{doorName}' ({__instance.m_nview.GetZDO().m_uid})");
                OnRenameDoorRequested?.Invoke(__instance);
                EntityRenameDialog.Instance?.OpenForDoor(__instance);
                return false;
            }
        }

        if (coveringWard != null)
        {
            ZDO wardZdo = coveringWard.m_nview.GetZDO();
            ZDO doorZdo = __instance.m_nview.GetZDO();
            WardRole requiredRole = (WardRole)doorZdo.GetInt(WardZdoKeys.DoorMinRole, (int)WardRole.Guest);
            WardRole playerRole = WardPermissions.GetPlayerRole(wardZdo, playerId);
            string doorName = doorZdo.GetString(WardZdoKeys.DoorName, "Door");
            string wardName = wardZdo.GetString(WardZdoKeys.CustomName, "Ward");

            if (!WardPermissions.CanOpenDoor(doorZdo, wardZdo, playerId))
            {
                WardMeBruh.Log.Debug($"[Access DENIED] Player '{player.GetPlayerName()}' ({playerId}, Role: {playerRole}) denied access to Door '{doorName}' ({doorZdo.m_uid}) in Ward '{wardName}' ({wardZdo.m_uid}). Required Role: {requiredRole}");
                character.Message(MessageHud.MessageType.Center, "$msg_privatezone");
                return false;
            }

            WardMeBruh.Log.Debug($"[Access GRANTED] Player '{player.GetPlayerName()}' ({playerId}, Role: {playerRole}) opened Door '{doorName}' ({doorZdo.m_uid}) in Ward '{wardName}' ({wardZdo.m_uid}). Required Role: {requiredRole}");
        }

        return true;
    }

    [HarmonyPatch(nameof(Door.Interact))]
    [HarmonyPostfix]
    private static void InteractPostfix(Door __instance)
    {
        if (__instance == null || __instance.m_nview == null || !__instance.m_nview.IsValid())
            return;

        int currentState = __instance.m_nview.GetZDO().GetInt(ZDOVars.s_state, 0);
        if (currentState == 0)
        {
            AutoDoorService.Instance.CancelDoorClose(__instance);
            return;
        }

        PrivateArea coveringWard = WardQueryService.GetCoveringWard(__instance.transform.position);
        if (coveringWard == null)
            return;

        ZDO wardZdo = coveringWard.m_nview.GetZDO();
        long playerId = Player.m_localPlayer != null ? Player.m_localPlayer.GetPlayerID() : 0L;
        bool autoCloseEnabled = WardQueryService.HasFeatureEnabled(wardZdo, WardZdoKeys.AutoCloseDoors, playerId);
        if (!autoCloseEnabled)
            return;

        AutoDoorService.Instance.QueueDoorClose(__instance, coveringWard);
    }

    [HarmonyPatch("RPC_UseDoor")]
    [HarmonyPrefix]
    private static bool RPC_UseDoorPrefix(Door __instance, long uid, bool forward)
    {
        if (__instance == null || __instance.m_nview == null || !__instance.m_nview.IsValid())
            return true;

        ZDO zdo = __instance.m_nview.GetZDO();
        int currentState = zdo.GetInt(ZDOVars.s_state, 0);

        if (currentState == 0)
        {
            if (_doorCloseTimestamps.TryGetValue(zdo.m_uid, out float closedTime))
            {
                if (Time.time - closedTime < 1.2f)
                {
                    WardMeBruh.Log.Debug($"[Door] Suppressed race-condition toggle on recently closed Door '{zdo.GetString(WardZdoKeys.DoorName, "Door")}' ({zdo.m_uid})");
                    return false;
                }
            }
        }
        else
        {
            PruneOldTimestamps();
            _doorCloseTimestamps[zdo.m_uid] = Time.time;
            AutoDoorService.Instance.CancelDoorClose(__instance);
        }

        return true;
    }

    private static void PruneOldTimestamps()
    {
        if (_doorCloseTimestamps.Count < 50)
            return;

        float now = Time.time;
        List<ZDOID> staleKeys = new();
        foreach (KeyValuePair<ZDOID, float> kvp in _doorCloseTimestamps)
        {
            if (now - kvp.Value > 10f)
            {
                staleKeys.Add(kvp.Key);
            }
        }
        for (int i = 0; i < staleKeys.Count; i++)
        {
            _doorCloseTimestamps.Remove(staleKeys[i]);
        }
    }

    private static ZDOID _cachedDoorId = ZDOID.None;
    private static string _cachedDoorBaseText = string.Empty;
    private static string _cachedDoorFinalText = string.Empty;
    private static int _cachedDoorMinRole = -1;
    private static string _cachedDoorCustomName = string.Empty;

    [HarmonyPatch(nameof(Door.GetHoverText))]
    [HarmonyPostfix]
    private static void GetHoverTextPostfix(Door __instance, ref string __result)
    {
        if (__instance == null || __instance.m_nview == null || !__instance.m_nview.IsValid())
            return;

        ZDO doorZdo = __instance.m_nview.GetZDO();
        string customName = doorZdo.GetString(WardZdoKeys.DoorName, string.Empty);
        int minRoleInt = doorZdo.GetInt(WardZdoKeys.DoorMinRole, (int)WardRole.Guest);

        if (doorZdo.m_uid == _cachedDoorId &&
            string.Equals(__result, _cachedDoorBaseText, StringComparison.Ordinal) &&
            string.Equals(customName, _cachedDoorCustomName, StringComparison.Ordinal) &&
            minRoleInt == _cachedDoorMinRole)
        {
            __result = _cachedDoorFinalText;
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

        bool hasCustomName = !string.IsNullOrEmpty(customName) && !customName.Equals("Door", StringComparison.OrdinalIgnoreCase);
        if (hasCustomName)
        {
            int nameIndex = -1;
            if (!string.IsNullOrEmpty(localizedName))
            {
                nameIndex = __result.IndexOf(localizedName, StringComparison.Ordinal);
            }

            if (nameIndex >= 0)
            {
                __result = __result.Substring(0, nameIndex) + $"<color=#38bdf8><b>{customName}</b></color>" + __result.Substring(nameIndex + localizedName.Length);
            }
            else
            {
                __result = $"<color=#38bdf8><b>{customName}</b></color>\n" + __result;
            }
        }

        PrivateArea coveringWard = WardQueryService.GetCoveringWard(__instance.transform.position);
        bool canRename = false;
        long playerId = Player.m_localPlayer != null ? Player.m_localPlayer.GetPlayerID() : 0L;

        string statusLine = string.Empty;

        if (coveringWard != null)
        {
            ZDO wardZdo = coveringWard.m_nview.GetZDO();

            if (minRoleInt != (int)WardRole.Guest)
            {
                bool hasAccess = WardPermissions.CanOpenDoor(doorZdo, wardZdo, playerId);
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
            string renamePrompt = Localization.instance.Localize($"\n[<color=yellow><b>{modKey} + {useKey}</b></color>] {"$wmb_configure_door".Localize()}");
            __result += renamePrompt;
        }

        _cachedDoorId = doorZdo.m_uid;
        _cachedDoorBaseText = originalResult;
        _cachedDoorFinalText = __result;
        _cachedDoorMinRole = minRoleInt;
        _cachedDoorCustomName = customName;
    }
}
