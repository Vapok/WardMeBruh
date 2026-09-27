using System;
using System.Collections.Generic;
using UnityEngine;
using WardMeBruh.Configuration;
using WardMeBruh.Models;
using WardMeBruh.Networking;
using WardMeBruh.Services;

namespace WardMeBruh.UI.Tabs;

public class TabOverview
{
    private GameObject _root;
    private PrivateArea _ward;

    private Action<string, bool> _setNameInput;
    private Action<string, Color> _setOwnerBadge;
    private Action<string, Color> _setRoleBadge;
    private Action<string, Color, string, bool> _setPowerRow;
    private Transform _grantedContainer;
    private GameObject _grantedCardRoot;
    private readonly List<GameObject> _grantedRows = new();

    private Action<string, Color> _setWeatherRoofBadge;
    private Action<string, Color> _setRegenBadge;
    private Action<string, Color> _setDeathSkillBadge;
    private Action<string, Color> _setFoodFreezeBadge;

    private Action<string, Color> _setGhostBaseBadge;
    private Action<string, Color> _setRaidBadge;
    private Action<string, Color> _setSpawnBadge;
    private Action<string, Color> _setTamesBadge;
    private Action<string, Color> _setRepairBadge;
    private Action<string, Color> _setFuelBadge;
    private Action<string, Color> _setDoorBadge;
    private Action<string, Color> _setComfortBadge;
    private Action<string, Color> _setPortalBadge;

    private string _currentEnteredName = string.Empty;

    public GameObject Root => _root;

    public void Build(Transform parent)
    {
        _root = new GameObject("Tab_Overview", typeof(RectTransform));
        _root.transform.SetParent(parent, false);
        RectTransform rt = _root.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        WardUIFactory.CreateScrollArea(_root.transform, out Transform listContainer);

        Transform card1 = WardUIFactory.CreateCard(listContainer, "Ward Identity & Clearance");
        WardUIFactory.CreateInputRow(card1, "Ward Designation", "Ward Stone", "SAVE", OnSaveNameClicked, out _setNameInput);
        WardUIFactory.CreateBadgeRow(card1, "Owner & Creator", "-", WardColorPalette.ValheimBeige, out _setOwnerBadge);
        WardUIFactory.CreateBadgeRow(card1, "Your Clearance Tier", "-", WardColorPalette.GoldAmber, out _setRoleBadge);
        WardUIFactory.CreatePowerRow(card1, "Ward Power State", "...", WardColorPalette.RunicGreen, "TOGGLE", OnTogglePowerClicked, out _setPowerRow);

        Transform card2 = WardUIFactory.CreateCard(listContainer, "Granted Role Capabilities");
        _grantedContainer = card2;
        _grantedCardRoot = card2.parent.parent.gameObject;

        Transform card3 = WardUIFactory.CreateCard(listContainer, "Active Sanctuary & Territorial Protections");
        WardUIFactory.CreateBadgeRow(card3, "Weather Proofing (Rain Rot & Ash)", "...", WardColorPalette.TextMuted, out _setWeatherRoofBadge);
        WardUIFactory.CreateBadgeRow(card3, "Sanctuary Vitality Boost (50% Regen)", "...", WardColorPalette.TextMuted, out _setRegenBadge);
        WardUIFactory.CreateBadgeRow(card3, "Death Skill Loss Protection", "...", WardColorPalette.TextMuted, out _setDeathSkillBadge);
        WardUIFactory.CreateBadgeRow(card3, "Food Duration Freeze", "...", WardColorPalette.TextMuted, out _setFoodFreezeBadge);

        WardUIFactory.CreateBadgeRow(card3, "Ghost Base (Monster Camouflage)", "...", WardColorPalette.TextMuted, out _setGhostBaseBadge);
        WardUIFactory.CreateBadgeRow(card3, "Raid Event Suppression", "...", WardColorPalette.TextMuted, out _setRaidBadge);
        WardUIFactory.CreateBadgeRow(card3, "Monster Spawn Suppression", "...", WardColorPalette.TextMuted, out _setSpawnBadge);
        WardUIFactory.CreateBadgeRow(card3, "Tamed Creature Sanctuary", "...", WardColorPalette.TextMuted, out _setTamesBadge);
        WardUIFactory.CreateBadgeRow(card3, "Automated Area Repair", "...", WardColorPalette.TextMuted, out _setRepairBadge);
        WardUIFactory.CreateBadgeRow(card3, "Auto-Fuel Supply Depot", "...", WardColorPalette.TextMuted, out _setFuelBadge);
        WardUIFactory.CreateBadgeRow(card3, "Auto-Closing Doors", "...", WardColorPalette.TextMuted, out _setDoorBadge);
        WardUIFactory.CreateBadgeRow(card3, "Territory Max Comfort", "...", WardColorPalette.TextMuted, out _setComfortBadge);
        WardUIFactory.CreateBadgeRow(card3, "Portal Access Control", "...", WardColorPalette.TextMuted, out _setPortalBadge);
    }

    public void Bind(PrivateArea ward)
    {
        _ward = ward;
        Refresh();
    }

    public void Refresh()
    {
        if (_ward == null || _ward.m_nview == null || !_ward.m_nview.IsValid())
            return;

        ZDO zdo = _ward.m_nview.GetZDO();
        long playerId = Player.m_localPlayer != null ? Player.m_localPlayer.GetPlayerID() : 0L;
        WardRole role = WardPermissions.GetPlayerRole(zdo, playerId);

        string customName = zdo.GetString(WardZdoKeys.CustomName, "Ward Stone");
        _currentEnteredName = customName;
        bool canRename = WardRoleManager.CanPerform(zdo, playerId, "core.rename");
        _setNameInput?.Invoke(customName, canRename);

        long creatorId = zdo.GetLong(ZDOVars.s_creator, 0L);
        string creatorName = zdo.GetString(ZDOVars.s_creatorName, creatorId != 0L ? $"ID: {creatorId}" : "Unknown");
        _setOwnerBadge?.Invoke(creatorName, WardColorPalette.ValheimBeige);

        Color roleColor = role >= WardRole.Manager ? WardColorPalette.GoldAmber : WardColorPalette.IceBlue;
        _setRoleBadge?.Invoke($"[ {role} ]", roleColor);

        bool isEnabled = _ward.IsEnabled();
        string statusText = isEnabled ? "● ACTIVE" : "○ INACTIVE";
        Color statusColor = isEnabled ? WardColorPalette.RunicGreen : WardColorPalette.DangerRed;
        string btnText = isEnabled ? "DEACTIVATE" : "ACTIVATE";
        bool canPower = WardRoleManager.CanPerform(zdo, playerId, "core.power");
        _setPowerRow?.Invoke(statusText, statusColor, btnText, canPower);

        for (int i = 0; i < _grantedRows.Count; i++)
        {
            if (_grantedRows[i] != null)
            {
                UnityEngine.Object.Destroy(_grantedRows[i]);
            }
        }
        _grantedRows.Clear();

        List<FeaturePermission> granted = WardRoleManager.GetGrantedPermissions(zdo, playerId);
        if (_grantedCardRoot != null)
        {
            _grantedCardRoot.SetActive(granted.Count > 0);
        }

        for (int i = 0; i < granted.Count; i++)
        {
            FeaturePermission perm = granted[i];
            GameObject row = WardUIFactory.CreateBadgeRow(_grantedContainer, perm.DisplayName, $"✔  {perm.Category}", WardColorPalette.RunicGreen, out _);
            _grantedRows.Add(row);
        }

        UpdatePerkBadge(_setWeatherRoofBadge, zdo.GetBool(WardZdoKeys.WeatherRoof, false));
        UpdatePerkBadge(_setRegenBadge, zdo.GetBool(WardZdoKeys.RegenBoost, true));
        UpdatePerkBadge(_setDeathSkillBadge, zdo.GetBool(WardZdoKeys.DeathSkillShield, true));
        UpdatePerkBadge(_setFoodFreezeBadge, zdo.GetBool(WardZdoKeys.FoodFreeze, true));

        UpdatePerkBadge(_setGhostBaseBadge, zdo.GetBool(WardZdoKeys.GhostBase, false));
        UpdatePerkBadge(_setRaidBadge, zdo.GetBool(WardZdoKeys.RaidSuppress, false));
        UpdatePerkBadge(_setSpawnBadge, zdo.GetBool(WardZdoKeys.SpawnSuppress, false));
        UpdatePerkBadge(_setTamesBadge, isEnabled);
        UpdatePerkBadge(_setRepairBadge, zdo.GetBool(WardZdoKeys.RepairEnabled, false));
        UpdatePerkBadge(_setFuelBadge, zdo.GetBool(WardZdoKeys.AutoFuelEnabled, false));
        UpdatePerkBadge(_setDoorBadge, zdo.GetBool(WardZdoKeys.AutoCloseDoors, false));

        bool maxComfortEnabled = zdo.GetBool(WardZdoKeys.MaxComfort, false);
        if (maxComfortEnabled && isEnabled)
        {
            int comfortLvl = WardComfortService.GetWardComfort(_ward);
            _setComfortBadge?.Invoke($"✔  LVL {comfortLvl}", WardColorPalette.RunicGreen);
        }
        else
        {
            UpdatePerkBadge(_setComfortBadge, false);
        }

        UpdatePerkBadge(_setPortalBadge, zdo.GetBool(WardZdoKeys.PortalAccess, false));
    }

    private void OnTogglePowerClicked()
    {
        if (_ward == null || _ward.m_nview == null || !_ward.m_nview.IsValid())
            return;

        long playerId = Player.m_localPlayer != null ? Player.m_localPlayer.GetPlayerID() : 0L;
        ZDO zdo = _ward.m_nview.GetZDO();
        if (!WardRoleManager.CanPerform(zdo, playerId, "core.power"))
            return;

        _ward.m_nview.InvokeRPC("ToggleEnabled", playerId);

        if (WardMainWindow.Instance != null)
        {
            WardMainWindow.Instance.StartCoroutine(DelayedRefresh());
        }
    }

    private System.Collections.IEnumerator DelayedRefresh()
    {
        yield return new WaitForSeconds(0.15f);
        Refresh();
    }

    private static void UpdatePerkBadge(Action<string, Color> badgeSetter, bool isActive)
    {
        if (badgeSetter == null)
            return;

        if (isActive)
        {
            badgeSetter("✔  ACTIVE", WardColorPalette.RunicGreen);
        }
        else
        {
            badgeSetter("✖  DISABLED", WardColorPalette.TextMuted);
        }
    }

    private void OnSaveNameClicked(string newName)
    {
        if (_ward == null || _ward.m_nview == null || !_ward.m_nview.IsValid())
            return;

        long playerId = Player.m_localPlayer != null ? Player.m_localPlayer.GetPlayerID() : 0L;
        ZDO zdo = _ward.m_nview.GetZDO();
        if (!WardRoleManager.CanPerform(zdo, playerId, "core.rename"))
            return;

        string trimmed = (newName ?? string.Empty).Trim();
        if (string.IsNullOrEmpty(trimmed))
            trimmed = "Ward Stone";

        _currentEnteredName = trimmed;
        WardNetworkRouter.SendRenameWard(zdo.m_uid, trimmed);
    }
}
