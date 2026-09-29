using System.Collections.Generic;
using Jotunn.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WardMeBruh.Configuration;
using WardMeBruh.Models;
using WardMeBruh.Networking;
using WardMeBruh.Patches;
using WardMeBruh.Services;

namespace WardMeBruh.UI.Tabs;

public class TabAccess
{
    private GameObject _root;
    private PrivateArea _ward;
    private Transform _listContainer;
    private readonly List<GameObject> _entryRows = new();

    public GameObject Root => _root;

    public void Build(Transform parent)
    {
        _root = new GameObject("Tab_Access", typeof(RectTransform));
        _root.transform.SetParent(parent, false);
        RectTransform rt = _root.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        WardUIFactory.CreateScrollArea(_root.transform, out _listContainer);
    }

    public void Bind(PrivateArea ward)
    {
        _ward = ward;
        Refresh();
    }

    public void Refresh()
    {
        for (int i = 0; i < _entryRows.Count; i++)
        {
            if (_entryRows[i] != null)
                Object.Destroy(_entryRows[i]);
        }
        _entryRows.Clear();

        if (_ward == null || _ward.m_nview == null || !_ward.m_nview.IsValid())
            return;

        float radius = WardQueryService.GetWardRadius(_ward);
        Vector3 wardPos = _ward.transform.position;
        long playerId = Player.m_localPlayer != null ? Player.m_localPlayer.GetPlayerID() : 0L;
        ZDO wardZdo = _ward.m_nview.GetZDO();
        bool canManageChests = WardRoleManager.CanPerform(wardZdo, playerId, "access.chests");
        bool canManageDoors = WardRoleManager.CanPerform(wardZdo, playerId, "access.doors");
        bool canManagePortals = WardRoleManager.CanPerform(wardZdo, playerId, "access.portals");
        bool portalAccessEnabled = wardZdo.GetBool(WardZdoKeys.PortalAccess, false);

        List<Container> allContainers = Patch_Container.AllContainers;
        List<Container> wardContainers = new();
        for (int i = 0; i < allContainers.Count; i++)
        {
            Container c = allContainers[i];
            if (c == null || c.m_nview == null || !c.m_nview.IsValid())
                continue;

            if (WardQueryService.IsCoveredByWardOrExtensions(wardZdo, c.transform.position))
            {
                wardContainers.Add(c);
            }
        }

        Door[] allDoors = Object.FindObjectsByType<Door>(FindObjectsSortMode.None);
        List<Door> wardDoors = new();
        for (int i = 0; i < allDoors.Length; i++)
        {
            Door d = allDoors[i];
            if (d == null || d.m_nview == null || !d.m_nview.IsValid())
                continue;

            if (WardQueryService.IsCoveredByWardOrExtensions(wardZdo, d.transform.position))
            {
                wardDoors.Add(d);
            }
        }

        TeleportWorld[] allPortals = Object.FindObjectsByType<TeleportWorld>(FindObjectsSortMode.None);
        List<TeleportWorld> wardPortals = new();
        for (int i = 0; i < allPortals.Length; i++)
        {
            TeleportWorld p = allPortals[i];
            if (p == null || p.m_nview == null || !p.m_nview.IsValid())
                continue;

            if (WardQueryService.IsCoveredByWardOrExtensions(wardZdo, p.transform.position))
            {
                wardPortals.Add(p);
            }
        }

        Transform summaryCard = WardUIFactory.CreateCard(_listContainer, "$wmb_access_card_scanner");
        _entryRows.Add(summaryCard.parent.parent.gameObject);

        WardUIFactory.CreateBadgeRow(summaryCard, "$wmb_access_scanned_radius", $"{Mathf.RoundToInt(radius)}{"$wmb_meters".Localize()}", WardColorPalette.ValheimOrange, out _);
        WardUIFactory.CreateBadgeRow(summaryCard, "$wmb_access_containers_detected", $"{wardContainers.Count} {"$wmb_access_chests_unit".Localize()}", WardColorPalette.ValheimBeige, out _);
        WardUIFactory.CreateBadgeRow(summaryCard, "$wmb_access_doors_detected", $"{wardDoors.Count} {"$wmb_access_doors_unit".Localize()}", WardColorPalette.ValheimBeige, out _);
        WardUIFactory.CreateBadgeRow(summaryCard, "$wmb_access_portals_detected", $"{wardPortals.Count} {"$wmb_access_portals_unit".Localize()}", WardColorPalette.ValheimBeige, out _);

        Transform containerCard = WardUIFactory.CreateCard(_listContainer, $"{"$wmb_access_card_containers".Localize()} ({wardContainers.Count})");
        _entryRows.Add(containerCard.parent.parent.gameObject);

        if (wardContainers.Count == 0)
        {
            WardUIFactory.CreateBadgeRow(containerCard, "$wmb_access_no_containers".Localize(), "-", WardColorPalette.TextMuted, out _);
        }
        else
        {
            for (int i = 0; i < wardContainers.Count; i++)
            {
                CreateContainerRow(containerCard, wardContainers[i], canManageChests);
            }
        }

        Transform doorCard = WardUIFactory.CreateCard(_listContainer, $"{"$wmb_access_card_doors".Localize()} ({wardDoors.Count})");
        _entryRows.Add(doorCard.parent.parent.gameObject);

        if (wardDoors.Count == 0)
        {
            WardUIFactory.CreateBadgeRow(doorCard, "$wmb_access_no_doors".Localize(), "-", WardColorPalette.TextMuted, out _);
        }
        else
        {
            for (int i = 0; i < wardDoors.Count; i++)
            {
                CreateDoorRow(doorCard, wardDoors[i], canManageDoors);
            }
        }

        Transform portalCard = WardUIFactory.CreateCard(_listContainer, $"{"$wmb_access_card_portals".Localize()} ({wardPortals.Count})");
        _entryRows.Add(portalCard.parent.parent.gameObject);

        WardUIFactory.CreateToggleRow(
            portalCard,
            "$wmb_access_enforce_portal",
            portalAccessEnabled,
            val =>
            {
                if (canManagePortals && _ward != null && _ward.m_nview != null && _ward.m_nview.IsValid())
                {
                    _ward.m_nview.ClaimOwnership();
                    _ward.m_nview.GetZDO().Set(WardZdoKeys.PortalAccess, val);
                    Refresh();
                }
            },
            out _);

        if (portalAccessEnabled)
        {
            if (wardPortals.Count == 0)
            {
                WardUIFactory.CreateBadgeRow(portalCard, "$wmb_access_no_portals".Localize(), "-", WardColorPalette.TextMuted, out _);
            }
            else
            {
                for (int i = 0; i < wardPortals.Count; i++)
                {
                    CreatePortalRow(portalCard, wardPortals[i], canManagePortals);
                }
            }
        }
        else
        {
            WardUIFactory.CreateBadgeRow(portalCard, "$wmb_access_portal_disabled", "$wmb_access_portal_disabled_desc".Localize(), WardColorPalette.TextMuted, out _);
        }

        if (WardMainWindow.Instance != null)
        {
            WardMainWindow.ApplyFontSizeToHierarchy(_root, WardMainWindow.Instance.CurrentFontSizeMode);
        }
    }

    private void CreateContainerRow(Transform parent, Container container, bool canManage)
    {
        ZDO zdo = container.m_nview.GetZDO();
        string customName = zdo.GetString(WardZdoKeys.ContainerName, string.Empty);

        string defaultName = !string.IsNullOrEmpty(container.m_name)
            ? Localization.instance.Localize(container.m_name)
            : (container.GetComponent<Piece>() != null ? Localization.instance.Localize(container.GetComponent<Piece>().m_name) : "$wmb_chest".Localize());
        if (string.IsNullOrEmpty(defaultName))
            defaultName = "$wmb_chest".Localize();

        string displayName = (!string.IsNullOrEmpty(customName) && !customName.Equals("Container", System.StringComparison.OrdinalIgnoreCase))
            ? customName
            : defaultName;

        int roleInt = zdo.GetInt(WardZdoKeys.ContainerMinRole, (int)WardRole.Resident);
        WardRole role = (WardRole)roleInt;
        bool isFuel = zdo.GetBool(WardZdoKeys.ContainerIsFuel, false);

        WardUIFactory.CreateSettingRow(parent, displayName, out Transform ctrlContainer);

        GameObject roleBtnObj = GUIManager.Instance.CreateButton(
            $"[ {"$wmb_role".Localize()}: {WardRoleManager.GetRoleName(role).Localize()} ]", 
            ctrlContainer, 
            Vector2.zero, 
            Vector2.zero, 
            Vector2.zero, 
            135f, 
            28f);
        roleBtnObj.GetComponent<RectTransform>().sizeDelta = new Vector2(135f, 28f);
        roleBtnObj.SetLayoutDimensions(135f, 28f);
        Color roleColor = role >= WardRole.Manager ? WardColorPalette.GoldAmber : (role == WardRole.Resident ? WardColorPalette.IceBlue : WardColorPalette.TextMuted);
        TextMeshProUGUI roleTxt = WardUIFactory.SetupButtonText(roleBtnObj, $"[ {"$wmb_role".Localize()}: {WardRoleManager.GetRoleName(role).Localize()} ]", 12f, roleColor);

        Button roleBtn = roleBtnObj.GetComponent<Button>();
        roleBtn.interactable = canManage;
        roleBtn.onClick.AddListener(() =>
        {
            WardRole nextRole = (WardRole)(((int)role + 1) % 4);
            if (container != null && container.m_nview != null && container.m_nview.IsValid())
            {
                container.m_nview.ClaimOwnership();
            }
            zdo.Set(WardZdoKeys.ContainerMinRole, (int)nextRole);
            WardNetworkRouter.SendContainerRole(zdo.m_uid, (int)nextRole);
            Refresh();
        });

        GameObject fuelBtnObj = GUIManager.Instance.CreateButton(
            isFuel ? $"✔ {"$wmb_access_fuel_depot".Localize()}" : $"✖ {"$wmb_access_no_depot".Localize()}", 
            ctrlContainer, 
            Vector2.zero, 
            Vector2.zero, 
            Vector2.zero, 
            140f, 
            28f);
        fuelBtnObj.GetComponent<RectTransform>().sizeDelta = new Vector2(140f, 28f);
        fuelBtnObj.SetLayoutDimensions(140f, 28f);
        Color fuelColor = isFuel ? WardColorPalette.GoldAmber : WardColorPalette.TextMuted;
        TextMeshProUGUI fuelTxt = WardUIFactory.SetupButtonText(fuelBtnObj, isFuel ? $"✔ {"$wmb_access_fuel_depot".Localize()}" : $"✖ {"$wmb_access_no_depot".Localize()}", 12f, fuelColor);

        Button fuelBtn = fuelBtnObj.GetComponent<Button>();
        fuelBtn.interactable = canManage;
        fuelBtn.onClick.AddListener(() =>
        {
            if (container != null && container.m_nview != null && container.m_nview.IsValid())
            {
                container.m_nview.ClaimOwnership();
            }
            zdo.Set(WardZdoKeys.ContainerIsFuel, !isFuel);
            WardNetworkRouter.SendContainerFuel(zdo.m_uid, !isFuel);
            Refresh();
        });
    }

    private void CreateDoorRow(Transform parent, Door door, bool canManage)
    {
        ZDO zdo = door.m_nview.GetZDO();
        string customName = zdo.GetString(WardZdoKeys.DoorName, string.Empty);

        string defaultName = !string.IsNullOrEmpty(door.m_name)
            ? Localization.instance.Localize(door.m_name)
            : (door.GetComponent<Piece>() != null ? Localization.instance.Localize(door.GetComponent<Piece>().m_name) : "$wmb_door".Localize());
        if (string.IsNullOrEmpty(defaultName))
            defaultName = "$wmb_door".Localize();

        string displayName = (!string.IsNullOrEmpty(customName) && !customName.Equals("Door", System.StringComparison.OrdinalIgnoreCase))
            ? customName
            : defaultName;

        int roleInt = zdo.GetInt(WardZdoKeys.DoorMinRole, (int)WardRole.Guest);
        WardRole role = (WardRole)roleInt;

        WardUIFactory.CreateSettingRow(parent, displayName, out Transform ctrlContainer);

        GameObject roleBtnObj = GUIManager.Instance.CreateButton(
            $"[ {"$wmb_role".Localize()}: {WardRoleManager.GetRoleName(role).Localize()} ]", 
            ctrlContainer, 
            Vector2.zero, 
            Vector2.zero, 
            Vector2.zero, 
            135f, 
            28f);
        roleBtnObj.GetComponent<RectTransform>().sizeDelta = new Vector2(135f, 28f);
        roleBtnObj.SetLayoutDimensions(135f, 28f);
        Color doorRoleColor = role >= WardRole.Manager ? WardColorPalette.GoldAmber : (role == WardRole.Resident ? WardColorPalette.IceBlue : WardColorPalette.TextMuted);
        TextMeshProUGUI roleTxt = WardUIFactory.SetupButtonText(roleBtnObj, $"[ {"$wmb_role".Localize()}: {WardRoleManager.GetRoleName(role).Localize()} ]", 12f, doorRoleColor);

        Button roleBtn = roleBtnObj.GetComponent<Button>();
        roleBtn.interactable = canManage;
        roleBtn.onClick.AddListener(() =>
        {
            WardRole nextRole = (WardRole)(((int)role + 1) % 4);
            if (door != null && door.m_nview != null && door.m_nview.IsValid())
            {
                door.m_nview.ClaimOwnership();
            }
            zdo.Set(WardZdoKeys.DoorMinRole, (int)nextRole);
            WardNetworkRouter.SendDoorRole(zdo.m_uid, (int)nextRole);
            Refresh();
        });
    }

    private void CreatePortalRow(Transform parent, TeleportWorld portal, bool canManage)
    {
        ZDO zdo = portal.m_nview.GetZDO();
        string tag = portal.GetText();
        string displayName = !string.IsNullOrEmpty(tag) ? $"{"$wmb_access_portals_unit".Localize()}: \"{tag}\"" : $"{"$wmb_access_portals_unit".Localize()} ({"$wmb_untagged".Localize()})";

        int roleInt = zdo.GetInt(WardZdoKeys.PortalMinRole, (int)WardRole.Resident);
        WardRole role = (WardRole)roleInt;

        WardUIFactory.CreateSettingRow(parent, displayName, out Transform ctrlContainer);

        GameObject roleBtnObj = GUIManager.Instance.CreateButton(
            $"[ {"$wmb_role".Localize()}: {WardRoleManager.GetRoleName(role).Localize()} ]", 
            ctrlContainer, 
            Vector2.zero, 
            Vector2.zero, 
            Vector2.zero, 
            135f, 
            28f);
        roleBtnObj.GetComponent<RectTransform>().sizeDelta = new Vector2(135f, 28f);
        roleBtnObj.SetLayoutDimensions(135f, 28f);
        Color portalRoleColor = role >= WardRole.Manager ? WardColorPalette.GoldAmber : (role == WardRole.Resident ? WardColorPalette.IceBlue : WardColorPalette.TextMuted);
        TextMeshProUGUI roleTxt = WardUIFactory.SetupButtonText(roleBtnObj, $"[ {"$wmb_role".Localize()}: {WardRoleManager.GetRoleName(role).Localize()} ]", 12f, portalRoleColor);

        Button roleBtn = roleBtnObj.GetComponent<Button>();
        roleBtn.interactable = canManage;
        roleBtn.onClick.AddListener(() =>
        {
            WardRole nextRole = (WardRole)(((int)role + 1) % 4);
            if (portal != null && portal.m_nview != null && portal.m_nview.IsValid())
            {
                portal.m_nview.ClaimOwnership();
            }
            zdo.Set(WardZdoKeys.PortalMinRole, (int)nextRole);
            if (ZDOMan.instance != null)
            {
                ZDOMan.instance.SetDirtyPortals();
            }
            WardNetworkRouter.SendPortalRole(zdo.m_uid, (int)nextRole);
            Refresh();
        });
    }
}
