using System.Reflection;
using System.Text;
using HarmonyLib;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using WardMeBruh.Configuration;
using WardMeBruh.Models;
using WardMeBruh.Services;
using WardMeBruh.UI;

namespace WardMeBruh.Patches;

[HarmonyPatch(typeof(PrivateArea))]
internal static class Patch_PrivateArea
{
    public static event System.Action<PrivateArea> OnOpenWardGuiRequested;

    [HarmonyPatch(nameof(PrivateArea.Awake))]
    [HarmonyPostfix]
    private static void AwakePostfix(PrivateArea __instance)
    {
        if (ConfigRegistry.Enabled != null && !ConfigRegistry.Enabled.Value)
            return;

        if (__instance == null || __instance.m_nview == null || !__instance.m_nview.IsValid())
            return;

        ZDO zdo = __instance.m_nview.GetZDO();
        WardRegistryManager.Instance.RegisterOrUpdate(zdo);

        float customRadius = zdo.GetFloat(WardZdoKeys.Radius, 32.0f);
        if (__instance.m_areaMarker != null)
        {
            __instance.m_areaMarker.m_radius = customRadius;
        }

        string wardName = zdo.GetString(WardZdoKeys.CustomName, __instance.m_name);
        WardMeBruh.Log.Debug($"[Ward Lifecycle] Registered Ward {zdo.m_uid} ('{wardName}') at {__instance.transform.position}, Radius: {customRadius}m");

        if (!Jotunn.Managers.GUIManager.IsHeadless())
        {
            if (__instance.GetComponent<Components.WardForcefieldDome>() == null)
            {
                __instance.gameObject.AddComponent<Components.WardForcefieldDome>();
            }
        }
    }

    [HarmonyPatch(nameof(PrivateArea.OnDestroy))]
    [HarmonyPrefix]
    private static void OnDestroyPrefix(PrivateArea __instance)
    {
        if (__instance == null || __instance.m_nview == null || !__instance.m_nview.IsValid())
            return;

        ZDO zdo = __instance.m_nview.GetZDO();
        string wardName = zdo.GetString(WardZdoKeys.CustomName, __instance.m_name);
        WardMeBruh.Log.Debug($"[Ward Lifecycle] Unregistering Ward {zdo.m_uid} ('{wardName}') on destroy");
        WardRegistryManager.Instance.Unregister(zdo.m_uid);
    }

    [HarmonyPatch(nameof(PrivateArea.HaveLocalAccess))]
    [HarmonyPrefix]
    private static bool HaveLocalAccessPrefix(PrivateArea __instance, ref bool __result)
    {
        if (ConfigRegistry.Enabled != null && !ConfigRegistry.Enabled.Value)
            return true;

        if (__instance == null || __instance.m_nview == null || !__instance.m_nview.IsValid())
            return true;

        if (Player.m_localPlayer == null)
            return true;

        long playerId = Player.m_localPlayer.GetPlayerID();
        ZDO zdo = __instance.m_nview.GetZDO();
        WardRole playerRole = WardPermissions.GetPlayerRole(zdo, playerId);
        bool hasAccess = WardPermissions.HasWardAccess(zdo, playerId);

        WardMeBruh.Log.Debug($"[Access Check] HaveLocalAccess for Player {playerId} (Role: {playerRole}) in Ward {zdo.m_uid}: {hasAccess}");

        __result = hasAccess;
        return false;
    }

    [HarmonyPatch(nameof(PrivateArea.IsInside))]
    [HarmonyPrefix]
    private static bool IsInsidePrefix(PrivateArea __instance, Vector3 point, float radius, ref bool __result)
    {
        if (ConfigRegistry.Enabled != null && !ConfigRegistry.Enabled.Value)
            return true;

        if (__instance == null || __instance.m_nview == null || !__instance.m_nview.IsValid())
            return true;

        float customRadius = WardQueryService.GetWardRadius(__instance);
        float distance = Utils.DistanceXZ(__instance.transform.position, point);

        __result = distance <= customRadius + radius;
        return false;
    }

    public static void RequestOpenWardGui(PrivateArea ward)
    {
        OnOpenWardGuiRequested?.Invoke(ward);
    }

    private static readonly MethodInfo SetEnabledMethod = AccessTools.Method(typeof(PrivateArea), "SetEnabled", new[] { typeof(bool) });

    [HarmonyPatch("RPC_ToggleEnabled")]
    [HarmonyPrefix]
    private static bool RPC_ToggleEnabledPrefix(PrivateArea __instance, long uid, long playerID)
    {
        if (ConfigRegistry.Enabled != null && !ConfigRegistry.Enabled.Value)
            return true;

        if (__instance == null || __instance.m_nview == null || !__instance.m_nview.IsValid())
            return true;

        if (!__instance.m_nview.IsOwner())
            return false;

        ZDO zdo = __instance.m_nview.GetZDO();
        bool isAuthorized = WardPermissions.CanManageWard(zdo, playerID);
        WardMeBruh.Log.Debug($"[Ward Power] ToggleEnabled RPC received for Ward {zdo.m_uid} from Player {playerID}. Authorized: {isAuthorized}");

        if (isAuthorized)
        {
            bool newState = !__instance.IsEnabled();
            WardMeBruh.Log.Debug($"[Ward Power] Ward {zdo.m_uid} toggled power state to {(newState ? "ENABLED" : "DISABLED")}");
            SetEnabledMethod?.Invoke(__instance, new object[] { newState });
            return false;
        }

        return true;
    }

    [HarmonyPatch(nameof(PrivateArea.Interact))]
    [HarmonyPrefix]
    private static bool InteractPrefix(PrivateArea __instance, Humanoid human, bool hold, bool alt)
    {
        if (hold)
            return false;

        if (ConfigRegistry.Enabled != null && !ConfigRegistry.Enabled.Value)
            return true;

        if (__instance == null || __instance.m_nview == null || !__instance.m_nview.IsValid())
            return true;

        if (WardMainWindow.Instance != null && WardMainWindow.Instance.IsOpen)
            return false;

        if (EntityRenameDialog.Instance != null && EntityRenameDialog.Instance.IsOpen)
            return false;

        if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject != null)
        {
            if (EventSystem.current.currentSelectedGameObject.GetComponent<InputField>() != null)
                return false;
        }

        Player player = human as Player;
        if (player == null)
            return true;

        long playerId = player.GetPlayerID();
        ZDO zdo = __instance.m_nview.GetZDO();
        bool isMoving = player.IsRunning() || player.m_run || Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D);
        bool hasUseInput = ZInput.GetButton("Use") || ZInput.GetButtonDown("Use") || ZInput.GetButton("JoyUse") || ZInput.GetButtonDown("JoyUse");
        bool isAlt = hasUseInput && ((alt && !isMoving) || Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl) || Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt));

        if (isAlt)
        {
            if (WardPermissions.CanManageWard(zdo, playerId))
            {
                WardMeBruh.Log.Debug($"[Ward Interact] Player '{player.GetPlayerName()}' ({playerId}) triggering power toggle for Ward {zdo.m_uid}");
                __instance.m_nview.InvokeRPC("ToggleEnabled", playerId);
                return false;
            }
            else
            {
                WardMeBruh.Log.Debug($"[Ward Interact] Player '{player.GetPlayerName()}' ({playerId}) denied power toggle for Ward {zdo.m_uid} (Manager clearance required)");
                player.Message(MessageHud.MessageType.Center, Localization.instance.Localize("$piece_noaccess"));
                return false;
            }
        }

        if (ConfigRegistry.OpenWardHubKey == null || ConfigRegistry.OpenWardHubKey.Value.MainKey == KeyCode.E)
        {
            if (WardPermissions.CanManageWard(zdo, playerId))
            {
                WardMeBruh.Log.Debug($"[Ward Interact] Player '{player.GetPlayerName()}' ({playerId}) opening Ward Hub for Ward {zdo.m_uid}");
                OnOpenWardGuiRequested?.Invoke(__instance);
                return false;
            }
            else
            {
                WardMeBruh.Log.Debug($"[Ward Interact] Player '{player.GetPlayerName()}' ({playerId}) denied opening Ward Hub for Ward {zdo.m_uid} (Manager clearance required)");
                player.Message(MessageHud.MessageType.Center, Localization.instance.Localize("$piece_noaccess"));
                return false;
            }
        }

        return true;
    }

    [HarmonyPatch(nameof(PrivateArea.GetHoverText))]
    [HarmonyPrefix]
    private static bool GetHoverTextPrefix(PrivateArea __instance, ref string __result)
    {
        if (ConfigRegistry.Enabled != null && !ConfigRegistry.Enabled.Value)
            return true;

        if (__instance == null || __instance.m_nview == null || !__instance.m_nview.IsValid())
            return true;

        if (Player.m_localPlayer == null)
            return true;

        if (__instance.m_ownerFaction != Character.Faction.Players)
            return true;

        __instance.ShowAreaMarker();

        ZDO zdo = __instance.m_nview.GetZDO();
        long playerId = Player.m_localPlayer.GetPlayerID();
        string customName = zdo.GetString(WardZdoKeys.CustomName, string.Empty);
        string baseName = string.IsNullOrEmpty(customName) ? __instance.m_name : customName;

        StringBuilder sb = new(256);
        bool isEnabled = __instance.IsEnabled();
        string statusToken = isEnabled ? "$piece_guardstone_active" : "$piece_guardstone_inactive";
        sb.Append($"{baseName} ( {statusToken} )");

        string creatorName = zdo.GetString(ZDOVars.s_creatorName, string.Empty);
        if (!string.IsNullOrEmpty(creatorName))
        {
            sb.Append($"\n$piece_guardstone_owner: {creatorName}");
        }

        WardRole role = WardPermissions.GetPlayerRole(zdo, playerId);
        string roleColor = role >= WardRole.Manager ? "#fcd34d" : "#38bdf8";
        sb.Append($"\nClearance: <color={roleColor}><b>[{role}]</b></color>");

        if (WardPermissions.CanManageWard(zdo, playerId))
        {
            string hubKey = (ConfigRegistry.OpenWardHubKey != null && ConfigRegistry.OpenWardHubKey.Value.MainKey != KeyCode.None)
                ? ConfigRegistry.OpenWardHubKey.Value.ToString()
                : "E";
            sb.Append($"\n[<color=yellow><b>{hubKey}</b></color>] Open Ward Hub");

            string toggleAction = isEnabled ? "$piece_guardstone_deactivate" : "$piece_guardstone_activate";
            sb.Append($"\n[<color=yellow><b>LeftShift + E</b></color>] {toggleAction}");
        }

        __result = Localization.instance.Localize(sb.ToString());
        return false;
    }
}
