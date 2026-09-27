using System;
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

    private Action<string> _setNameInput;
    private Action<string, Color> _setOwnerBadge;
    private Action<string, Color> _setRoleBadge;
    private Action<string, Color, string> _setPowerRow;
    private Action<string, Color, bool> _setWardRoleRow;
    private Action<float> _setRadiusSlider;
    private Action<string, Color> _setAreaBadge;

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
        WardUIFactory.CreateRoleRow(card1, "Dome Access Clearance", "[ Role: Guest ]", WardColorPalette.TextMuted, OnCycleWardRoleClicked, out _setWardRoleRow);

        Transform card2 = WardUIFactory.CreateCard(listContainer, "Territorial Domain");
        WardUIFactory.CreateSliderRow(
            card2, 
            "Ward Perimeter Radius", 
            10f, 
            (float)ConfigRegistry.MaxWardRadius.Value, 
            32f, 
            true, 
            "m", 
            OnRadiusChanged, 
            out _setRadiusSlider);
        WardUIFactory.CreateBadgeRow(card2, "Protected Land Coverage", "~3,217 m²", WardColorPalette.ValheimBeige, out _setAreaBadge);

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
        _setNameInput?.Invoke(customName);

        long creatorId = zdo.GetLong(ZDOVars.s_creator, 0L);
        string creatorName = zdo.GetString(ZDOVars.s_creatorName, creatorId != 0L ? $"ID: {creatorId}" : "Unknown");
        _setOwnerBadge?.Invoke(creatorName, WardColorPalette.ValheimBeige);

        Color roleColor = role >= WardRole.Manager ? WardColorPalette.GoldAmber : WardColorPalette.IceBlue;
        _setRoleBadge?.Invoke($"[ {role} ]", roleColor);

        bool isEnabled = _ward.IsEnabled();
        string statusText = isEnabled ? "● ACTIVE" : "○ INACTIVE";
        Color statusColor = isEnabled ? WardColorPalette.RunicGreen : WardColorPalette.DangerRed;
        string btnText = isEnabled ? "DEACTIVATE" : "ACTIVATE";
        _setPowerRow?.Invoke(statusText, statusColor, btnText);

        int minRoleInt = zdo.GetInt(WardZdoKeys.WardMinRole, (int)WardRole.Guest);
        WardRole minRole = (WardRole)minRoleInt;
        Color minRoleColor = minRole >= WardRole.Manager 
            ? WardColorPalette.GoldAmber 
            : (minRole == WardRole.Resident ? WardColorPalette.IceBlue : WardColorPalette.TextMuted);
        bool canManage = WardPermissions.CanManageWard(zdo, playerId);
        _setWardRoleRow?.Invoke($"[ Role: {minRole} ]", minRoleColor, canManage);

        float currentRadius = zdo.GetFloat(WardZdoKeys.Radius, 32.0f);
        _setRadiusSlider?.Invoke(currentRadius);

        float areaSqMeters = Mathf.PI * currentRadius * currentRadius;
        _setAreaBadge?.Invoke($"~{Mathf.RoundToInt(areaSqMeters):N0} m²", WardColorPalette.ValheimBeige);

        UpdatePerkBadge(_setWeatherRoofBadge, zdo.GetBool(WardZdoKeys.WeatherRoof, false), (WardRole)zdo.GetInt(WardZdoKeys.WeatherRoofMinRole, 0));
        UpdatePerkBadge(_setRegenBadge, zdo.GetBool(WardZdoKeys.RegenBoost, true), (WardRole)zdo.GetInt(WardZdoKeys.RegenBoostMinRole, 0));
        UpdatePerkBadge(_setDeathSkillBadge, zdo.GetBool(WardZdoKeys.DeathSkillShield, true), (WardRole)zdo.GetInt(WardZdoKeys.DeathSkillShieldMinRole, 0));
        UpdatePerkBadge(_setFoodFreezeBadge, zdo.GetBool(WardZdoKeys.FoodFreeze, true), (WardRole)zdo.GetInt(WardZdoKeys.FoodFreezeMinRole, 0));

        UpdatePerkBadge(_setGhostBaseBadge, zdo.GetBool(WardZdoKeys.GhostBase, false), (WardRole)zdo.GetInt(WardZdoKeys.GhostBaseMinRole, 0));
        UpdatePerkBadge(_setRaidBadge, zdo.GetBool(WardZdoKeys.RaidSuppress, false), (WardRole)zdo.GetInt(WardZdoKeys.RaidSuppressMinRole, 0));
        UpdatePerkBadge(_setSpawnBadge, zdo.GetBool(WardZdoKeys.SpawnSuppress, false), (WardRole)zdo.GetInt(WardZdoKeys.SpawnSuppressMinRole, 0));
        UpdatePerkBadge(_setTamesBadge, isEnabled);
        UpdatePerkBadge(_setRepairBadge, zdo.GetBool(WardZdoKeys.RepairEnabled, false), (WardRole)zdo.GetInt(WardZdoKeys.RepairMinRole, 0));
        UpdatePerkBadge(_setFuelBadge, zdo.GetBool(WardZdoKeys.AutoFuelEnabled, false), (WardRole)zdo.GetInt(WardZdoKeys.AutoFuelMinRole, 0));
        UpdatePerkBadge(_setDoorBadge, zdo.GetBool(WardZdoKeys.AutoCloseDoors, false), (WardRole)zdo.GetInt(WardZdoKeys.AutoCloseDoorsMinRole, 0));

        bool maxComfortEnabled = zdo.GetBool(WardZdoKeys.MaxComfort, false);
        if (maxComfortEnabled && isEnabled)
        {
            int comfortLvl = WardComfortService.GetWardComfort(_ward);
            WardRole comfortMinRole = (WardRole)zdo.GetInt(WardZdoKeys.MaxComfortMinRole, 0);
            string roleSuffix = comfortMinRole > WardRole.Guest ? $" [{comfortMinRole}]" : string.Empty;
            _setComfortBadge?.Invoke($"✔  LVL {comfortLvl}{roleSuffix}", WardColorPalette.RunicGreen);
        }
        else
        {
            UpdatePerkBadge(_setComfortBadge, false);
        }

        UpdatePerkBadge(_setPortalBadge, zdo.GetBool(WardZdoKeys.PortalAccess, false), (WardRole)zdo.GetInt(WardZdoKeys.PortalMinRole, 0));
    }

    private void OnCycleWardRoleClicked()
    {
        if (_ward == null || _ward.m_nview == null || !_ward.m_nview.IsValid())
            return;

        long playerId = Player.m_localPlayer != null ? Player.m_localPlayer.GetPlayerID() : 0L;
        ZDO zdo = _ward.m_nview.GetZDO();
        if (!WardPermissions.CanManageWard(zdo, playerId))
            return;

        int currentRoleInt = zdo.GetInt(WardZdoKeys.WardMinRole, (int)WardRole.Guest);
        WardRole nextRole = (WardRole)(((int)currentRoleInt + 1) % 4);

        zdo.Set(WardZdoKeys.WardMinRole, (int)nextRole);
        WardNetworkRouter.SendWardRole(zdo.m_uid, (int)nextRole);
        Refresh();
    }

    private void OnTogglePowerClicked()
    {
        if (_ward == null || _ward.m_nview == null || !_ward.m_nview.IsValid())
            return;

        long playerId = Player.m_localPlayer != null ? Player.m_localPlayer.GetPlayerID() : 0L;
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

    private static void UpdatePerkBadge(Action<string, Color> badgeSetter, bool isActive, WardRole minRole = WardRole.Guest)
    {
        if (badgeSetter == null)
            return;

        if (isActive)
        {
            string roleSuffix = minRole > WardRole.Guest ? $" [{minRole}]" : string.Empty;
            badgeSetter($"✔  ACTIVE{roleSuffix}", WardColorPalette.RunicGreen);
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

        string trimmed = (newName ?? string.Empty).Trim();
        if (string.IsNullOrEmpty(trimmed))
            trimmed = "Ward Stone";

        _currentEnteredName = trimmed;
        WardNetworkRouter.SendRenameWard(_ward.m_nview.GetZDO().m_uid, trimmed);
    }

    private void OnRadiusChanged(float value)
    {
        if (_ward == null || _ward.m_nview == null || !_ward.m_nview.IsValid())
            return;

        _ward.m_nview.GetZDO().Set(WardZdoKeys.Radius, value);
        if (_ward.m_areaMarker != null)
        {
            _ward.m_areaMarker.m_radius = value;
        }

        float areaSqMeters = Mathf.PI * value * value;
        _setAreaBadge?.Invoke($"~{Mathf.RoundToInt(areaSqMeters):N0} m²", WardColorPalette.ValheimBeige);
    }
}
