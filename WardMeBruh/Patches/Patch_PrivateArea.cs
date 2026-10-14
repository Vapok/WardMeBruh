using System.Collections.Generic;
using System.Reflection;
using System.Text;
using HarmonyLib;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using WardMeBruh.API;
using WardMeBruh.Configuration;
using WardMeBruh.Models;
using WardMeBruh.Networking;
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
        bool changed = WardRegistryManager.Instance.RegisterOrUpdate(zdo);

        float customRadius = WardQueryService.GetWardRadius(__instance);
        if (__instance.m_areaMarker != null)
        {
            __instance.m_areaMarker.m_radius = customRadius;
        }

        string wardName = zdo.GetString(WardZdoKeys.CustomName, __instance.m_name);
        WardMeBruh.Log.Debug($"[Ward Lifecycle] Registered Ward {zdo.m_uid} ('{wardName}') at {__instance.transform.position}, Radius: {customRadius}m");

        if (changed && ZNet.instance != null && !ZNet.instance.IsServer())
        {
            WardNetworkRouter.SendRegisterWard(zdo.m_uid);
        }

        if (!Jotunn.Managers.GUIManager.IsHeadless())
        {
            if (__instance.GetComponent<Components.WardForcefieldDome>() == null)
            {
                __instance.gameObject.AddComponent<Components.WardForcefieldDome>();
            }
            if (__instance.GetComponent<Components.WardAttunementStones>() == null)
            {
                __instance.gameObject.AddComponent<Components.WardAttunementStones>();
            }
        }
    }

    [HarmonyPatch(nameof(PrivateArea.Setup))]
    [HarmonyPostfix]
    private static void SetupPostfix(PrivateArea __instance)
    {
        if (ConfigRegistry.Enabled != null && !ConfigRegistry.Enabled.Value)
            return;

        if (__instance == null || __instance.m_nview == null || !__instance.m_nview.IsValid())
            return;

        if (!__instance.m_nview.IsOwner() && (ZNet.instance == null || !ZNet.instance.IsServer()))
            return;

        ZDO zdo = __instance.m_nview.GetZDO();
        ZDOID existingParentId = zdo.GetZDOID(WardZdoKeys.ParentWard);
        if (existingParentId != ZDOID.None)
            return;

        PrivateArea coveringWard = null;
        if (PrivateArea.m_allAreas != null)
        {
            for (int i = 0; i < PrivateArea.m_allAreas.Count; i++)
            {
                PrivateArea area = PrivateArea.m_allAreas[i];
                if (area == null || area == __instance || !area.IsEnabled())
                    continue;

                if (WardQueryService.IsInsideWard(area, __instance.transform.position))
                {
                    coveringWard = area;
                    break;
                }
            }
        }

        if (coveringWard != null && coveringWard.m_nview != null && coveringWard.m_nview.IsValid())
        {
            ZDO coveringZdo = coveringWard.m_nview.GetZDO();
            WardType coveringType = (WardType)coveringZdo.GetInt(WardZdoKeys.WardType, (int)WardType.Main);
            if (coveringType == WardType.SubWard)
            {
                Piece piece = __instance.GetComponent<Piece>();
                if (piece != null)
                {
                    piece.DropResources();
                }

                if (Player.m_localPlayer != null)
                {
                    Player.m_localPlayer.Message(MessageHud.MessageType.Center, "$wmb_msg_subward_placement_denied".Localize());
                }

                if (ZNetScene.instance != null)
                {
                    ZNetScene.instance.Destroy(__instance.gameObject);
                }
                else
                {
                    UnityEngine.Object.Destroy(__instance.gameObject);
                }
                return;
            }

            ZDO rootParentZdo = WardQueryService.GetEffectiveWardZdo(coveringZdo);
            if (rootParentZdo != null && rootParentZdo.m_uid != zdo.m_uid)
            {
                long playerId = Player.m_localPlayer != null ? Player.m_localPlayer.GetPlayerID() : 0L;
                bool canExtend = WardRoleManager.CanPerform(rootParentZdo, playerId, WardPermissionIds.CoreExtendWard) || WardPermissions.IsAdmin(playerId);
                bool canSubWard = WardRoleManager.CanPerform(rootParentZdo, playerId, WardPermissionIds.CoreSubWard) || WardPermissions.IsAdmin(playerId);

                if (!canExtend && !canSubWard)
                {
                    Piece piece = __instance.GetComponent<Piece>();
                    if (piece != null)
                    {
                        piece.DropResources();
                    }

                    if (Player.m_localPlayer != null)
                    {
                        Player.m_localPlayer.Message(MessageHud.MessageType.Center, "$wmb_msg_subward_placement_denied".Localize());
                    }

                    if (ZNetScene.instance != null)
                    {
                        ZNetScene.instance.Destroy(__instance.gameObject);
                    }
                    else
                    {
                        UnityEngine.Object.Destroy(__instance.gameObject);
                    }
                    return;
                }

                if (canExtend && canSubWard && !Jotunn.Managers.GUIManager.IsHeadless() && Player.m_localPlayer != null)
                {
                    WardPlacementChoiceModal.Instance?.Open(__instance, coveringWard);
                    return;
                }

                if (canExtend && !canSubWard)
                {
                    WardPlacementChoiceModal.ConfigureAsExtension(__instance, coveringWard);
                    return;
                }

                if (canSubWard)
                {
                    WardPlacementChoiceModal.ConfigureAsSubWard(__instance, coveringWard);
                    return;
                }
            }
        }

        if (WardRegistryManager.Instance.RegisterOrUpdate(zdo) && ZNet.instance != null && !ZNet.instance.IsServer())
        {
            WardNetworkRouter.SendRegisterWard(zdo.m_uid);
        }
    }

    [HarmonyPatch(nameof(PrivateArea.IsEnabled))]
    [HarmonyPrefix]
    private static bool IsEnabledPrefix(PrivateArea __instance, ref bool __result)
    {
        if (ConfigRegistry.Enabled != null && !ConfigRegistry.Enabled.Value)
            return true;

        if (__instance == null || __instance.m_nview == null || !__instance.m_nview.IsValid())
            return true;

        ZDO zdo = __instance.m_nview.GetZDO();
        ZDOID parentId = zdo.GetZDOID(WardZdoKeys.ParentWard);
        if (parentId == ZDOID.None)
            return true;

        WardType wardType = (WardType)zdo.GetInt(WardZdoKeys.WardType, (int)WardType.Main);
        ZDO parentZdo = (parentId != ZDOID.None && ZDOMan.instance != null) ? ZDOMan.instance.GetZDO(parentId) : null;
        bool parentEnabled = parentZdo == null || !parentZdo.IsValid() || parentZdo.GetBool(ZDOVars.s_enabled, true);

        bool localEnabled = zdo.GetBool(ZDOVars.s_enabled, true);
        __result = parentEnabled && localEnabled;
        return false;
    }


    private static int _lastFarmingCheckFrame = -1;
    private static bool _lastFarmingResult;

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
        ZDO effectiveZdo = WardQueryService.GetEffectiveWardZdo(zdo);

        bool isFarming = GetCachedIsFarmingAction(Player.m_localPlayer);
        bool allowed = isFarming
            ? (WardPermissions.CanFarm(effectiveZdo, playerId) || WardPermissions.CanHarvest(effectiveZdo, playerId))
            : WardPermissions.CanBuild(effectiveZdo, playerId);

        __result = allowed;
        return false;
    }

    private static bool GetCachedIsFarmingAction(Player player)
    {
        if (player == null)
            return false;

        int currentFrame = Time.frameCount;
        if (currentFrame == _lastFarmingCheckFrame)
            return _lastFarmingResult;

        _lastFarmingCheckFrame = currentFrame;
        _lastFarmingResult = IsFarmingAction(player);
        return _lastFarmingResult;
    }

    private static bool IsFarmingAction(Player player)
    {
        if (player == null)
            return false;

        ItemDrop.ItemData rightItem = player.GetRightItem();
        if (rightItem != null && rightItem.m_shared != null)
        {
            if (rightItem.m_dropPrefab != null && string.Equals(rightItem.m_dropPrefab.name, "Cultivator", System.StringComparison.OrdinalIgnoreCase))
                return true;

            if (!string.IsNullOrEmpty(rightItem.m_shared.m_name) && rightItem.m_shared.m_name.IndexOf("cultivator", System.StringComparison.OrdinalIgnoreCase) >= 0)
                return true;

            if (rightItem.m_shared.m_buildPieces != null && rightItem.m_shared.m_buildPieces.name.IndexOf("cultivator", System.StringComparison.OrdinalIgnoreCase) >= 0)
                return true;
        }

        if (player.m_placementGhost != null)
        {
            Piece ghostPiece = player.m_placementGhost.GetComponent<Piece>();
            if (ghostPiece != null)
            {
                if (ghostPiece.m_cultivatedGroundOnly)
                    return true;

                Plant plant = ghostPiece.GetComponent<Plant>();
                if (plant != null)
                    return true;

                Pickable pickable = ghostPiece.GetComponent<Pickable>();
                if (pickable != null)
                    return true;
            }
        }

        Piece selectedPiece = player.m_buildPieces != null ? player.m_buildPieces.GetSelectedPiece() : null;
        if (selectedPiece != null)
        {
            if (selectedPiece.m_cultivatedGroundOnly)
                return true;

            Plant plant = selectedPiece.GetComponent<Plant>();
            if (plant != null)
                return true;

            Pickable pickable = selectedPiece.GetComponent<Pickable>();
            if (pickable != null)
                return true;
        }

        Piece hoveringPiece = player.GetHoveringPiece();
        if (hoveringPiece != null)
        {
            if (hoveringPiece.m_cultivatedGroundOnly)
                return true;

            Plant plant = hoveringPiece.GetComponent<Plant>();
            if (plant != null)
                return true;

            Pickable pickable = hoveringPiece.GetComponent<Pickable>();
            if (pickable != null)
                return true;
        }

        GameObject hoverObj = player.GetHoverObject();
        if (hoverObj != null)
        {
            if (hoverObj.GetComponent<Pickable>() != null || hoverObj.GetComponent<Plant>() != null)
                return true;

            if (hoverObj.GetComponentInParent<Pickable>() != null || hoverObj.GetComponentInParent<Plant>() != null)
                return true;
        }

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

    [HarmonyPatch(nameof(PrivateArea.CheckAccess))]
    [HarmonyPrefix]
    private static bool CheckAccessPrefix(Vector3 point, float radius, bool flash, bool wardCheck, ref bool __result)
    {
        if (ConfigRegistry.Enabled != null && !ConfigRegistry.Enabled.Value)
            return true;

        List<PrivateArea> list = new();
        bool flag = true;
        if (wardCheck)
        {
            flag = true;
            if (PrivateArea.m_allAreas != null)
            {
                long playerId = Player.m_localPlayer != null ? Player.m_localPlayer.GetPlayerID() : 0L;
                for (int i = 0; i < PrivateArea.m_allAreas.Count; i++)
                {
                    PrivateArea allArea = PrivateArea.m_allAreas[i];
                    if (allArea == null || !allArea.IsEnabled())
                        continue;

                    ZDO coveringZdo = allArea.m_nview != null ? allArea.m_nview.GetZDO() : null;
                    if (coveringZdo == null)
                        continue;

                    WardType coveringType = (WardType)coveringZdo.GetInt(WardZdoKeys.WardType, (int)WardType.Main);
                    float areaRadius = WardQueryService.GetWardRadius(allArea);
                    float dist = Utils.DistanceXZ(point, allArea.transform.position);

                    if (coveringType == WardType.SubWard)
                    {
                        if (dist < areaRadius + 1.0f)
                        {
                            flag = false;
                            list.Add(allArea);
                        }
                    }
                    else
                    {
                        if (dist <= areaRadius)
                        {
                            ZDO rootParentZdo = WardQueryService.GetEffectiveWardZdo(coveringZdo);
                            bool canExtend = WardRoleManager.CanPerform(rootParentZdo, playerId, WardPermissionIds.CoreExtendWard) || WardPermissions.IsAdmin(playerId);
                            bool canSubWard = WardRoleManager.CanPerform(rootParentZdo, playerId, WardPermissionIds.CoreSubWard) || WardPermissions.IsAdmin(playerId);

                            if (!canExtend && !canSubWard)
                            {
                                flag = false;
                                list.Add(allArea);
                            }
                        }
                        else if (allArea.IsInside(point, radius) && !allArea.HaveLocalAccess())
                        {
                            flag = false;
                            list.Add(allArea);
                        }
                    }
                }
            }
        }
        else
        {
            flag = false;
            bool insideAnyWard = false;
            if (PrivateArea.m_allAreas != null)
            {
                for (int i = 0; i < PrivateArea.m_allAreas.Count; i++)
                {
                    PrivateArea allArea2 = PrivateArea.m_allAreas[i];
                    if (allArea2 != null && allArea2.IsInside(point, radius))
                    {
                        insideAnyWard = true;
                        if (allArea2.HaveLocalAccess())
                        {
                            flag = true;
                        }
                        else
                        {
                            list.Add(allArea2);
                        }
                    }
                }
            }
            if (!insideAnyWard)
            {
                flag = true;
            }
        }

        if (!flag && list.Count > 0)
        {
            if (flash)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    PrivateArea item = list[i];
                    if (item != null && item.IsEnabled())
                    {
                        item.FlashShield(flashConnected: false);
                    }
                }
            }
            __result = false;
            return false;
        }

        __result = true;
        return false;
    }

    public static void RequestOpenWardGui(PrivateArea ward)
    {
        OnOpenWardGuiRequested?.Invoke(ward);
    }

    private static readonly MethodInfo SetEnabledMethod = AccessTools.Method(typeof(PrivateArea), "SetEnabled", new[] { typeof(bool) });

    [HarmonyPatch(nameof(PrivateArea.SetEnabled))]
    [HarmonyPostfix]
    private static void SetEnabledPostfix(PrivateArea __instance, bool enabled)
    {
        if (__instance == null || __instance.m_nview == null || !__instance.m_nview.IsValid())
            return;

        ZDO zdo = __instance.m_nview.GetZDO();
        WardRegistryManager.Instance.UpdateWardEnabled(zdo.m_uid, enabled);
    }

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

            if (PrivateArea.m_allAreas != null)
            {
                for (int i = 0; i < PrivateArea.m_allAreas.Count; i++)
                {
                    PrivateArea area = PrivateArea.m_allAreas[i];
                    if (area != null && area != __instance && area.m_nview != null && area.m_nview.IsValid())
                    {
                        if (area.m_nview.GetZDO().GetZDOID(WardZdoKeys.ParentWard) == zdo.m_uid)
                        {
                            SetEnabledMethod?.Invoke(area, new object[] { newState });
                        }
                    }
                }
            }

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
        ZDO effectiveZdo = WardQueryService.GetEffectiveWardZdo(zdo);
        PrivateArea effectiveWard = WardQueryService.GetEffectiveWard(__instance);

        bool hasUseInput = ZInput.GetButton("Use") || ZInput.GetButtonDown("Use") || ZInput.GetButton("JoyUse") || ZInput.GetButtonDown("JoyUse")
            || (UnityEngine.InputSystem.Gamepad.current != null && UnityEngine.InputSystem.Gamepad.current.buttonSouth.wasPressedThisFrame);
        bool gamepadAlt = ZInput.GetButton("JoyAltKeys") || ZInput.GetButton("JoyLTrigger") || ZInput.GetButton("JoyAltPlace")
            || (UnityEngine.InputSystem.Gamepad.current != null && (UnityEngine.InputSystem.Gamepad.current.leftTrigger.isPressed || UnityEngine.InputSystem.Gamepad.current.leftShoulder.isPressed));
        bool keyboardAlt = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) || Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl) || Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
        bool isAlt = hasUseInput && (alt || gamepadAlt || keyboardAlt);

        if (isAlt)
        {
            if (WardRoleManager.CanPerform(effectiveZdo, playerId, "core.power"))
            {
                WardMeBruh.Log.Debug($"[Ward Interact] Player '{player.GetPlayerName()}' ({playerId}) triggering power toggle for Ward {zdo.m_uid}");
                __instance.m_nview.InvokeRPC("ToggleEnabled", playerId);
                return false;
            }
            else
            {
                WardMeBruh.Log.Debug($"[Ward Interact] Player '{player.GetPlayerName()}' ({playerId}) denied power toggle for Ward {zdo.m_uid} (core.power permission required)");
                player.Message(MessageHud.MessageType.Center, Localization.instance.Localize("$piece_noaccess"));
                return false;
            }
        }

        if (ConfigRegistry.OpenWardHubKey == null || ConfigRegistry.OpenWardHubKey.Value.MainKey == KeyCode.E)
        {
            if (WardRoleManager.CanPerform(effectiveZdo, playerId, "core.view_gui"))
            {
                WardMeBruh.Log.Debug($"[Ward Interact] Player '{player.GetPlayerName()}' ({playerId}) opening Ward Hub for Ward {zdo.m_uid}");
                OnOpenWardGuiRequested?.Invoke(__instance);
                return false;
            }
            else
            {
                WardMeBruh.Log.Debug($"[Ward Interact] Player '{player.GetPlayerName()}' ({playerId}) denied opening Ward Hub for Ward {zdo.m_uid} (core.view_gui permission required)");
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
        ZDO effectiveZdo = WardQueryService.GetEffectiveWardZdo(zdo);
        bool isExtension = effectiveZdo.m_uid != zdo.m_uid;
        long playerId = Player.m_localPlayer.GetPlayerID();
        string customName = effectiveZdo.GetString(WardZdoKeys.CustomName, string.Empty);
        string baseName = string.IsNullOrEmpty(customName) ? __instance.m_name : customName;

        StringBuilder sb = new(256);
        bool isEnabled = __instance.IsEnabled();
        string statusToken = isEnabled ? "$piece_guardstone_active" : "$piece_guardstone_inactive";
        if (isExtension)
        {
            sb.Append($"{baseName} {"$wmb_base_extension".Localize()} ( {statusToken} )");
        }
        else
        {
            sb.Append($"{baseName} ( {statusToken} )");
        }

        string creatorName = effectiveZdo.GetString(ZDOVars.s_creatorName, string.Empty);
        if (!string.IsNullOrEmpty(creatorName))
        {
            sb.Append($"\n$piece_guardstone_owner: {creatorName}");
        }

        int roleId = WardRoleManager.GetPlayerRoleId(effectiveZdo, playerId);
        Color roleColor = WardRoleManager.GetRoleColor(effectiveZdo, roleId);
        string roleName = WardRoleManager.GetRoleDisplayName(effectiveZdo, roleId);
        sb.Append($"\n{"$wmb_clearance_label".Localize()} <color=#{ColorUtility.ToHtmlStringRGB(roleColor)}><b>[{roleName}]</b></color>");

        if (WardRoleManager.CanPerform(effectiveZdo, playerId, "core.view_gui"))
        {
            string hubKey = ZInput.IsGamepadActive()
                ? "$KEY_Use"
                : ((ConfigRegistry.OpenWardHubKey != null && ConfigRegistry.OpenWardHubKey.Value.MainKey != KeyCode.None)
                    ? ConfigRegistry.OpenWardHubKey.Value.ToString()
                    : "E");
            sb.Append($"\n[<color=yellow><b>{hubKey}</b></color>] {"$wmb_open_ward_hub".Localize()}");
        }

        if (WardRoleManager.CanPerform(effectiveZdo, playerId, "core.power"))
        {
            string toggleAction = isEnabled ? "$piece_guardstone_deactivate" : "$piece_guardstone_activate";
            string modKey = ZInput.IsGamepadActive() ? WardGamepadNavHelper.GetGamepadGlyph("LT") : "$KEY_AltPlace";
            string useKey = ZInput.IsGamepadActive() ? WardGamepadNavHelper.GetGamepadGlyph("A") : "$KEY_Use";
            sb.Append($"\n[<color=yellow><b>{modKey} + {useKey}</b></color>] {toggleAction.Localize()}");
        }

        __result = Localization.instance.Localize(sb.ToString());
        return false;
    }
}

[HarmonyPatch(typeof(Piece), nameof(Piece.DropResources))]
internal static class Patch_Piece_DropResources
{
    [HarmonyPostfix]
    private static void Postfix(Piece __instance)
    {
        if (__instance == null)
            return;

        PrivateArea privateArea = __instance.GetComponent<PrivateArea>();
        if (privateArea == null || privateArea.m_nview == null || !privateArea.m_nview.IsValid())
            return;

        ZDO zdo = privateArea.m_nview.GetZDO();
        List<string> refundItems = WardProgressionService.GetAttunementRefundItems(zdo);
        if (refundItems.Count == 0)
            return;

        for (int i = 0; i < refundItems.Count; i++)
        {
            string prefabName = refundItems[i];
            GameObject prefab = ObjectDB.instance != null ? ObjectDB.instance.GetItemPrefab(prefabName) : null;
            if (prefab == null && Jotunn.Managers.PrefabManager.Instance != null)
            {
                prefab = Jotunn.Managers.PrefabManager.Instance.GetPrefab(prefabName);
            }

            if (prefab != null)
            {
                Vector3 dropPos = __instance.transform.position + Vector3.up * 0.6f + UnityEngine.Random.insideUnitSphere * 0.25f;
                ItemDrop itemDrop = prefab.GetComponent<ItemDrop>();
                if (itemDrop != null)
                {
                    ItemDrop.DropItem(itemDrop.m_itemData, 1, dropPos, UnityEngine.Random.rotation);
                }
                else
                {
                    UnityEngine.Object.Instantiate(prefab, dropPos, UnityEngine.Random.rotation);
                }
            }
        }

        zdo.Set(WardZdoKeys.AttunedRunes, 0);
        zdo.Set(WardZdoKeys.ProgressionLevel, 0);
    }
}
