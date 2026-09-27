using System.Collections.Generic;
using Jotunn.Managers;
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
        bool canManage = WardPermissions.CanManageWard(wardZdo, playerId);
        bool portalAccessEnabled = wardZdo.GetBool(WardZdoKeys.PortalAccess, false);

        List<Container> allContainers = Patch_Container.AllContainers;
        List<Container> wardContainers = new();
        for (int i = 0; i < allContainers.Count; i++)
        {
            Container c = allContainers[i];
            if (c == null || c.m_nview == null || !c.m_nview.IsValid())
                continue;

            if (Utils.DistanceXZ(wardPos, c.transform.position) <= radius)
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

            if (Utils.DistanceXZ(wardPos, d.transform.position) <= radius)
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

            if (Utils.DistanceXZ(wardPos, p.transform.position) <= radius)
            {
                wardPortals.Add(p);
            }
        }

        Transform summaryCard = WardUIFactory.CreateCard(_listContainer, "Perimeter Access Scanner");
        _entryRows.Add(summaryCard.parent.parent.gameObject);

        WardUIFactory.CreateBadgeRow(summaryCard, "Scanned Perimeter Radius", $"{Mathf.RoundToInt(radius)}m", WardColorPalette.ValheimOrange, out _);
        WardUIFactory.CreateBadgeRow(summaryCard, "Active Containers Detected", $"{wardContainers.Count} Chests", WardColorPalette.ValheimBeige, out _);
        WardUIFactory.CreateBadgeRow(summaryCard, "Active Doors & Gates Detected", $"{wardDoors.Count} Doors", WardColorPalette.ValheimBeige, out _);
        WardUIFactory.CreateBadgeRow(summaryCard, "Active Portals Detected", $"{wardPortals.Count} Portals", WardColorPalette.ValheimBeige, out _);

        Transform containerCard = WardUIFactory.CreateCard(_listContainer, $"Storage Containers ({wardContainers.Count})");
        _entryRows.Add(containerCard.parent.parent.gameObject);

        if (wardContainers.Count == 0)
        {
            WardUIFactory.CreateBadgeRow(containerCard, "No chests found in territory", "-", WardColorPalette.TextMuted, out _);
        }
        else
        {
            for (int i = 0; i < wardContainers.Count; i++)
            {
                CreateContainerRow(containerCard, wardContainers[i], canManage);
            }
        }

        Transform doorCard = WardUIFactory.CreateCard(_listContainer, $"Doors & Gateways ({wardDoors.Count})");
        _entryRows.Add(doorCard.parent.parent.gameObject);

        if (wardDoors.Count == 0)
        {
            WardUIFactory.CreateBadgeRow(doorCard, "No doors found in territory", "-", WardColorPalette.TextMuted, out _);
        }
        else
        {
            for (int i = 0; i < wardDoors.Count; i++)
            {
                CreateDoorRow(doorCard, wardDoors[i], canManage);
            }
        }

        Transform portalCard = WardUIFactory.CreateCard(_listContainer, $"Portals & Teleportation ({wardPortals.Count})");
        _entryRows.Add(portalCard.parent.parent.gameObject);

        WardUIFactory.CreateToggleRow(
            portalCard,
            "Enforce Portal Access & Transit Clearance",
            portalAccessEnabled,
            val =>
            {
                if (canManage && _ward != null && _ward.m_nview != null && _ward.m_nview.IsValid())
                {
                    _ward.m_nview.GetZDO().Set(WardZdoKeys.PortalAccess, val);
                    Refresh();
                }
            },
            out _);

        if (portalAccessEnabled)
        {
            if (wardPortals.Count == 0)
            {
                WardUIFactory.CreateBadgeRow(portalCard, "No portals found in territory", "-", WardColorPalette.TextMuted, out _);
            }
            else
            {
                for (int i = 0; i < wardPortals.Count; i++)
                {
                    CreatePortalRow(portalCard, wardPortals[i], canManage);
                }
            }
        }
        else
        {
            WardUIFactory.CreateBadgeRow(portalCard, "Portal Access Control is Disabled", "All players may transit freely", WardColorPalette.TextMuted, out _);
        }
    }

    private void CreateContainerRow(Transform parent, Container container, bool canManage)
    {
        ZDO zdo = container.m_nview.GetZDO();
        string customName = zdo.GetString(WardZdoKeys.ContainerName, string.Empty);

        string defaultName = !string.IsNullOrEmpty(container.m_name)
            ? Localization.instance.Localize(container.m_name)
            : (container.GetComponent<Piece>() != null ? Localization.instance.Localize(container.GetComponent<Piece>().m_name) : "Chest");
        if (string.IsNullOrEmpty(defaultName))
            defaultName = "Chest";

        string displayName = (!string.IsNullOrEmpty(customName) && !customName.Equals("Container", System.StringComparison.OrdinalIgnoreCase))
            ? customName
            : defaultName;

        int roleInt = zdo.GetInt(WardZdoKeys.ContainerMinRole, (int)WardRole.Resident);
        WardRole role = (WardRole)roleInt;
        bool isFuel = zdo.GetBool(WardZdoKeys.ContainerIsFuel, false);

        WardUIFactory.CreateSettingRow(parent, displayName, out Transform ctrlContainer);

        GameObject roleBtnObj = GUIManager.Instance.CreateButton(
            $"[ Role: {role} ]", 
            ctrlContainer, 
            Vector2.zero, 
            Vector2.zero, 
            Vector2.zero, 
            135f, 
            28f);
        roleBtnObj.GetComponent<RectTransform>().sizeDelta = new Vector2(135f, 28f);
        roleBtnObj.SetLayoutDimensions(135f, 28f);
        Text roleTxt = roleBtnObj.GetComponentInChildren<Text>();
        if (roleTxt != null)
        {
            roleTxt.font = GUIManager.Instance.AveriaSerifBold;
            roleTxt.fontSize = 12;
            roleTxt.color = role >= WardRole.Manager ? WardColorPalette.GoldAmber : (role == WardRole.Resident ? WardColorPalette.IceBlue : WardColorPalette.TextMuted);
        }

        Button roleBtn = roleBtnObj.GetComponent<Button>();
        roleBtn.interactable = canManage;
        roleBtn.onClick.AddListener(() =>
        {
            WardRole nextRole = (WardRole)(((int)role + 1) % 4);
            WardNetworkRouter.SendContainerRole(zdo.m_uid, (int)nextRole);
            Refresh();
        });

        GameObject fuelBtnObj = GUIManager.Instance.CreateButton(
            isFuel ? "✔ FUEL DEPOT" : "✖ NO DEPOT", 
            ctrlContainer, 
            Vector2.zero, 
            Vector2.zero, 
            Vector2.zero, 
            125f, 
            28f);
        fuelBtnObj.GetComponent<RectTransform>().sizeDelta = new Vector2(125f, 28f);
        fuelBtnObj.SetLayoutDimensions(125f, 28f);
        Text fuelTxt = fuelBtnObj.GetComponentInChildren<Text>();
        if (fuelTxt != null)
        {
            fuelTxt.font = GUIManager.Instance.AveriaSerifBold;
            fuelTxt.fontSize = 12;
            fuelTxt.color = isFuel ? WardColorPalette.GoldAmber : WardColorPalette.TextMuted;
        }

        Button fuelBtn = fuelBtnObj.GetComponent<Button>();
        fuelBtn.interactable = canManage;
        fuelBtn.onClick.AddListener(() =>
        {
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
            : (door.GetComponent<Piece>() != null ? Localization.instance.Localize(door.GetComponent<Piece>().m_name) : "Door");
        if (string.IsNullOrEmpty(defaultName))
            defaultName = "Door";

        string displayName = (!string.IsNullOrEmpty(customName) && !customName.Equals("Door", System.StringComparison.OrdinalIgnoreCase))
            ? customName
            : defaultName;

        int roleInt = zdo.GetInt(WardZdoKeys.DoorMinRole, (int)WardRole.Guest);
        WardRole role = (WardRole)roleInt;

        WardUIFactory.CreateSettingRow(parent, displayName, out Transform ctrlContainer);

        GameObject roleBtnObj = GUIManager.Instance.CreateButton(
            $"[ Role: {role} ]", 
            ctrlContainer, 
            Vector2.zero, 
            Vector2.zero, 
            Vector2.zero, 
            135f, 
            28f);
        roleBtnObj.GetComponent<RectTransform>().sizeDelta = new Vector2(135f, 28f);
        roleBtnObj.SetLayoutDimensions(135f, 28f);
        Text roleTxt = roleBtnObj.GetComponentInChildren<Text>();
        if (roleTxt != null)
        {
            roleTxt.font = GUIManager.Instance.AveriaSerifBold;
            roleTxt.fontSize = 12;
            roleTxt.color = role >= WardRole.Manager ? WardColorPalette.GoldAmber : (role == WardRole.Resident ? WardColorPalette.IceBlue : WardColorPalette.TextMuted);
        }

        Button roleBtn = roleBtnObj.GetComponent<Button>();
        roleBtn.interactable = canManage;
        roleBtn.onClick.AddListener(() =>
        {
            WardRole nextRole = (WardRole)(((int)role + 1) % 4);
            WardNetworkRouter.SendDoorRole(zdo.m_uid, (int)nextRole);
            Refresh();
        });
    }

    private void CreatePortalRow(Transform parent, TeleportWorld portal, bool canManage)
    {
        ZDO zdo = portal.m_nview.GetZDO();
        string tag = portal.GetText();
        string displayName = !string.IsNullOrEmpty(tag) ? $"Portal: \"{tag}\"" : "Portal (Untagged)";

        int roleInt = zdo.GetInt(WardZdoKeys.PortalMinRole, (int)WardRole.Resident);
        WardRole role = (WardRole)roleInt;

        WardUIFactory.CreateSettingRow(parent, displayName, out Transform ctrlContainer);

        GameObject roleBtnObj = GUIManager.Instance.CreateButton(
            $"[ Role: {role} ]", 
            ctrlContainer, 
            Vector2.zero, 
            Vector2.zero, 
            Vector2.zero, 
            135f, 
            28f);
        roleBtnObj.GetComponent<RectTransform>().sizeDelta = new Vector2(135f, 28f);
        roleBtnObj.SetLayoutDimensions(135f, 28f);
        Text roleTxt = roleBtnObj.GetComponentInChildren<Text>();
        if (roleTxt != null)
        {
            roleTxt.font = GUIManager.Instance.AveriaSerifBold;
            roleTxt.fontSize = 12;
            roleTxt.color = role >= WardRole.Manager ? WardColorPalette.GoldAmber : (role == WardRole.Resident ? WardColorPalette.IceBlue : WardColorPalette.TextMuted);
        }

        Button roleBtn = roleBtnObj.GetComponent<Button>();
        roleBtn.interactable = canManage;
        roleBtn.onClick.AddListener(() =>
        {
            WardRole nextRole = (WardRole)(((int)role + 1) % 4);
            WardNetworkRouter.SendPortalRole(zdo.m_uid, (int)nextRole);
            Refresh();
        });
    }
}
