using System;
using HarmonyLib;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using WardMeBruh.Configuration;
using WardMeBruh.Models;
using WardMeBruh.Services;
using WardMeBruh.UI;

namespace WardMeBruh.Patches;

[HarmonyPatch(typeof(TeleportWorld))]
internal static class Patch_TeleportWorld
{
    private static float _lastNotifyTime;
    public static event Action<TeleportWorld> OnConfigurePortalRequested;

    private static ZDOID _cachedPortalId = ZDOID.None;
    private static string _cachedPortalBaseText = string.Empty;
    private static string _cachedPortalFinalText = string.Empty;
    private static int _cachedPortalMinRole = -1;

    [HarmonyPatch(nameof(TeleportWorld.Interact))]
    [HarmonyPrefix]
    private static bool InteractPrefix(TeleportWorld __instance, Humanoid human, bool hold, bool alt)
    {
        if (hold)
            return false;

        if (__instance == null || __instance.m_nview == null || !__instance.m_nview.IsValid())
            return true;

        if (WardMainWindow.HasInstanceAndOpen)
            return false;

        if (EntityRenameDialog.HasInstanceAndOpen)
            return false;

        if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject != null)
        {
            if (EventSystem.current.currentSelectedGameObject.GetComponent<InputField>() != null)
                return false;
        }

        Player player = human as Player;
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
            bool canConfigure = false;
            if (coveringWard != null && coveringWard.m_nview != null && coveringWard.m_nview.IsValid())
            {
                ZDO wardZdo = coveringWard.m_nview.GetZDO();
                canConfigure = WardPermissions.HasRequiredRole(wardZdo, playerId, WardRole.Resident);
            }
            else
            {
                Piece piece = __instance.GetComponent<Piece>();
                canConfigure = piece != null && (piece.IsCreator() || piece.GetCreator() == playerId || piece.GetCreator() == 0L);
            }

            if (canConfigure)
            {
                WardMeBruh.Log.Debug($"[UI] Player '{player.GetPlayerName()}' ({playerId}) opening config dialog for Portal '{__instance.GetText()}' ({__instance.m_nview.GetZDO().m_uid})");
                OnConfigurePortalRequested?.Invoke(__instance);
                EntityRenameDialog.Instance?.OpenForPortal(__instance);
                return false;
            }
        }

        return true;
    }

    [HarmonyPatch(nameof(TeleportWorld.GetHoverText))]
    [HarmonyPostfix]
    private static void GetHoverTextPostfix(TeleportWorld __instance, ref string __result)
    {
        if (__instance == null || __instance.m_nview == null || !__instance.m_nview.IsValid())
            return;

        ZDO portalZdo = __instance.m_nview.GetZDO();
        int minRoleInt = portalZdo.GetInt(WardZdoKeys.PortalMinRole, (int)WardRole.Resident);

        if (portalZdo.m_uid == _cachedPortalId &&
            string.Equals(__result, _cachedPortalBaseText, StringComparison.Ordinal) &&
            minRoleInt == _cachedPortalMinRole)
        {
            __result = _cachedPortalFinalText;
            return;
        }

        string originalResult = __result;
        PrivateArea coveringWard = WardQueryService.GetCoveringWard(__instance.transform.position);
        bool canConfigure = false;
        long playerId = Player.m_localPlayer != null ? Player.m_localPlayer.GetPlayerID() : 0L;

        string statusLine = string.Empty;

        if (coveringWard != null && coveringWard.m_nview != null && coveringWard.m_nview.IsValid())
        {
            ZDO wardZdo = coveringWard.m_nview.GetZDO();
            if (wardZdo != null)
            {
                bool isFeatureActive = WardQueryService.HasFeatureEnabled(wardZdo, WardZdoKeys.PortalAccess, playerId);
                bool hasAccess = !isFeatureActive || WardPermissions.HasRequiredRole(wardZdo, playerId, (WardRole)minRoleInt);
                string colorHex = hasAccess ? "#a3e635" : "#f87171";
                string roleDisplayName = WardRoleManager.GetRoleDisplayName(wardZdo, minRoleInt);
                statusLine = $"<color={colorHex}>[{roleDisplayName} {"$wmb_clearance".Localize()}]</color>";

                canConfigure = WardPermissions.HasRequiredRole(wardZdo, playerId, WardRole.Resident);
            }
        }
        else
        {
            Piece piece = __instance.GetComponent<Piece>();
            canConfigure = piece != null && (piece.IsCreator() || piece.GetCreator() == playerId || piece.GetCreator() == 0L);
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

        if (canConfigure)
        {
            string modKey = ZInput.IsGamepadActive() ? WardGamepadNavHelper.GetGamepadGlyph("LT") : "$KEY_AltPlace";
            string useKey = ZInput.IsGamepadActive() ? WardGamepadNavHelper.GetGamepadGlyph("A") : "$KEY_Use";
            string configPrompt = Localization.instance.Localize($"\n[<color=yellow><b>{modKey} + {useKey}</b></color>] {"$wmb_configure_portal".Localize()}");
            __result += configPrompt;
        }

        _cachedPortalId = portalZdo.m_uid;
        _cachedPortalBaseText = originalResult;
        _cachedPortalFinalText = __result;
        _cachedPortalMinRole = minRoleInt;
    }

    [HarmonyPatch(nameof(TeleportWorld.Teleport))]
    [HarmonyPrefix]
    private static bool TeleportPrefix(TeleportWorld __instance, Player player)
    {
        if (ConfigRegistry.Enabled != null && !ConfigRegistry.Enabled.Value)
            return true;

        if (__instance == null || __instance.m_nview == null || !__instance.m_nview.IsValid())
            return true;

        if (player == null)
            return true;

        long playerId = player.GetPlayerID();
        if (playerId == 0L)
            return true;

        Vector3 localPos = __instance.transform.position;
        PrivateArea localWard = WardQueryService.GetCoveringWard(localPos);
        if (localWard != null && localWard.m_nview != null && localWard.m_nview.IsValid())
        {
            ZDO localWardZdo = localWard.m_nview.GetZDO();
            if (localWardZdo != null && WardQueryService.HasFeatureEnabled(localWardZdo, WardZdoKeys.PortalAccess, playerId))
            {
                ZDO localPortalZdo = __instance.m_nview.GetZDO();
                int roleInt = localPortalZdo.GetInt(WardZdoKeys.PortalMinRole, (int)WardRole.Resident);
                WardRole requiredRole = (WardRole)roleInt;
                if (!WardPermissions.HasRequiredRole(localWardZdo, playerId, requiredRole))
                {
                    NotifyBlocked(player, "$wmb_msg_portal_blocked_local".Localize());
                    return false;
                }
            }
        }

        ZDO connectionZdo = __instance.m_nview.GetZDO();
        ZDOID destId = connectionZdo.GetConnectionZDOID(ZDOExtraData.ConnectionType.Portal);
        if (destId != ZDOID.None && ZDOMan.instance != null)
        {
            ZDO destZdo = ZDOMan.instance.GetZDO(destId);
            if (destZdo != null)
            {
                Vector3 destPos = destZdo.GetPosition();

                PrivateArea destWard = WardQueryService.GetCoveringWard(destPos);
                if (destWard != null && destWard.m_nview != null && destWard.m_nview.IsValid())
                {
                    ZDO destWardZdo = destWard.m_nview.GetZDO();
                    if (destWardZdo != null && WardQueryService.HasFeatureEnabled(destWardZdo, WardZdoKeys.PortalAccess, playerId))
                    {
                        int roleInt = destZdo.GetInt(WardZdoKeys.PortalMinRole, (int)WardRole.Resident);
                        WardRole destRequiredRole = (WardRole)roleInt;
                        if (!WardPermissions.HasRequiredRole(destWardZdo, playerId, destRequiredRole))
                        {
                            NotifyBlocked(player, "$wmb_msg_portal_blocked_dest".Localize());
                            return false;
                        }
                    }
                }
                else
                {
                    KnownWard knownWard = WardRegistryManager.Instance.GetWardCoveringPoint(destPos);
                    if (knownWard != null)
                    {
                        ZDO destWardZdo = ZDOMan.instance.GetZDO(knownWard.Id);
                        if (destWardZdo != null && WardQueryService.HasFeatureEnabled(destWardZdo, WardZdoKeys.PortalAccess, playerId))
                        {
                            int roleInt = destZdo.GetInt(WardZdoKeys.PortalMinRole, (int)WardRole.Resident);
                            WardRole destRequiredRole = (WardRole)roleInt;
                            if (!WardPermissions.HasRequiredRole(destWardZdo, playerId, destRequiredRole))
                            {
                                NotifyBlocked(player, "$wmb_msg_portal_blocked_dest".Localize());
                                return false;
                            }
                        }
                    }
                }
            }
        }

        return true;
    }

    private static void NotifyBlocked(Player player, string message)
    {
        if (Time.time - _lastNotifyTime > 2.0f)
        {
            _lastNotifyTime = Time.time;
            player.Message(MessageHud.MessageType.Center, message);
        }
    }
}
