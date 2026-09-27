using System;
using UnityEngine;
using WardMeBruh.Configuration;
using WardMeBruh.Models;
using WardMeBruh.Networking;

namespace WardMeBruh.UI.Tabs;

public class TabUpkeep
{
    private GameObject _root;
    private PrivateArea _ward;

    private Action<bool, WardRole, bool> _setRepair;
    private Action<float> _setRepairThresholdSlider;

    private Action<bool, WardRole, bool> _setAutoFuel;

    private Action<bool, WardRole, bool> _setAutoClose;
    private Action<float> _setAutoCloseSlider;

    private Action<bool, WardRole, bool> _setWeatherRoof;
    private Action<bool, WardRole, bool> _setAshShield;
    private Action<bool, WardRole, bool> _setLavaShield;
    private Action<bool, WardRole, bool> _setSnowShield;
    private Action<bool, WardRole, bool> _setWaterDrain;

    public GameObject Root => _root;

    public void Build(Transform parent)
    {
        _root = new GameObject("Tab_Upkeep", typeof(RectTransform));
        _root.transform.SetParent(parent, false);
        RectTransform rt = _root.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        WardUIFactory.CreateScrollArea(_root.transform, out Transform listContainer);

        Transform card1 = WardUIFactory.CreateCard(listContainer, "Automated Area Maintenance & Repair");
        WardUIFactory.CreateFeatureToggleRow(
            card1, 
            "Automated Area Repair", 
            false, 
            WardRole.Guest,
            v => SetZdoBool(WardZdoKeys.RepairEnabled, v), 
            r => SetFeatureRole(WardZdoKeys.RepairMinRole, r),
            out _setRepair);

        WardUIFactory.CreateSliderRow(
            card1, 
            "Repair Damage Threshold", 
            20f, 
            100f, 
            100f, 
            true, 
            "%", 
            v => SetZdoFloat(WardZdoKeys.RepairThreshold, v / 100f), 
            out _setRepairThresholdSlider);

        WardUIFactory.CreateBadgeRow(card1, "Repair Resource Cost", "Free (0 Wood / Iron / Stone)", WardColorPalette.ValheimBeige, out _);

        Transform card2 = WardUIFactory.CreateCard(listContainer, "Auto-Fuel Supply Depot");
        WardUIFactory.CreateFeatureToggleRow(
            card2, 
            "Automated Fuel Distribution", 
            false, 
            WardRole.Guest,
            v => SetZdoBool(WardZdoKeys.AutoFuelEnabled, v), 
            r => SetFeatureRole(WardZdoKeys.AutoFuelMinRole, r),
            out _setAutoFuel);

        WardUIFactory.CreateBadgeRow(card2, "Fuel Container Source", "Marked Containers ('Fuel')", WardColorPalette.GoldAmber, out _);
        WardUIFactory.CreateBadgeRow(card2, "Supported Appliances", "Fireplaces, Torches, Smelters, Kilns", WardColorPalette.ValheimBeige, out _);

        Transform card3 = WardUIFactory.CreateCard(listContainer, "Automated Door Control");
        WardUIFactory.CreateFeatureToggleRow(
            card3, 
            "Auto-Close Doors Behind Players", 
            false, 
            WardRole.Guest,
            v => SetZdoBool(WardZdoKeys.AutoCloseDoors, v), 
            r => SetFeatureRole(WardZdoKeys.AutoCloseDoorsMinRole, r),
            out _setAutoClose);

        WardUIFactory.CreateSliderRow(
            card3, 
            "Door Auto-Close Delay", 
            1f, 
            30f, 
            5f, 
            true, 
            "s", 
            v => SetZdoFloat(WardZdoKeys.AutoCloseDelay, v), 
            out _setAutoCloseSlider);

        Transform card4 = WardUIFactory.CreateCard(listContainer, "Environmental & Biome Immunity Shields");
        WardUIFactory.CreateFeatureToggleRow(
            card4, 
            "Virtual Weather Roof", 
            false, 
            WardRole.Guest,
            v => SetZdoBool(WardZdoKeys.WeatherRoof, v), 
            r => SetFeatureRole(WardZdoKeys.WeatherRoofMinRole, r),
            out _setWeatherRoof);

        WardUIFactory.CreateFeatureToggleRow(
            card4, 
            "Ashlands Ash Rain Shield", 
            false, 
            WardRole.Guest,
            v => SetZdoBool(WardZdoKeys.AshShield, v), 
            r => SetFeatureRole(WardZdoKeys.AshShieldMinRole, r),
            out _setAshShield);

        WardUIFactory.CreateFeatureToggleRow(
            card4, 
            "Lava Damage Shield", 
            false, 
            WardRole.Guest,
            v => SetZdoBool(WardZdoKeys.LavaShield, v), 
            r => SetFeatureRole(WardZdoKeys.LavaShieldMinRole, r),
            out _setLavaShield);

        WardUIFactory.CreateFeatureToggleRow(
            card4, 
            "Deep North Blizzard Shield", 
            false, 
            WardRole.Guest,
            v => SetZdoBool(WardZdoKeys.SnowShield, v), 
            r => SetFeatureRole(WardZdoKeys.SnowShieldMinRole, r),
            out _setSnowShield);

        WardUIFactory.CreateFeatureToggleRow(
            card4, 
            "Subsea Water Drain (Dry Basement)", 
            false, 
            WardRole.Guest,
            v => SetZdoBool(WardZdoKeys.WaterDrain, v), 
            r => SetFeatureRole(WardZdoKeys.WaterDrainMinRole, r),
            out _setWaterDrain);
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
        bool canManage = WardPermissions.CanManageWard(zdo, playerId);

        _setRepair?.Invoke(zdo.GetBool(WardZdoKeys.RepairEnabled, false), (WardRole)zdo.GetInt(WardZdoKeys.RepairMinRole, 0), canManage);

        float repThresh = zdo.GetFloat(WardZdoKeys.RepairThreshold, (float)ConfigRegistry.AutoRepairThresholdDefault.Value);
        _setRepairThresholdSlider?.Invoke(Mathf.Round(repThresh * 100f));

        _setAutoFuel?.Invoke(zdo.GetBool(WardZdoKeys.AutoFuelEnabled, false), (WardRole)zdo.GetInt(WardZdoKeys.AutoFuelMinRole, 0), canManage);

        _setAutoClose?.Invoke(zdo.GetBool(WardZdoKeys.AutoCloseDoors, false), (WardRole)zdo.GetInt(WardZdoKeys.AutoCloseDoorsMinRole, 0), canManage);

        float closeDelay = zdo.GetFloat(WardZdoKeys.AutoCloseDelay, (float)ConfigRegistry.AutoCloseDelayDefault.Value);
        _setAutoCloseSlider?.Invoke(closeDelay);

        _setWeatherRoof?.Invoke(zdo.GetBool(WardZdoKeys.WeatherRoof, false), (WardRole)zdo.GetInt(WardZdoKeys.WeatherRoofMinRole, 0), canManage);
        _setAshShield?.Invoke(zdo.GetBool(WardZdoKeys.AshShield, false), (WardRole)zdo.GetInt(WardZdoKeys.AshShieldMinRole, 0), canManage);
        _setLavaShield?.Invoke(zdo.GetBool(WardZdoKeys.LavaShield, false), (WardRole)zdo.GetInt(WardZdoKeys.LavaShieldMinRole, 0), canManage);
        _setSnowShield?.Invoke(zdo.GetBool(WardZdoKeys.SnowShield, false), (WardRole)zdo.GetInt(WardZdoKeys.SnowShieldMinRole, 0), canManage);
        _setWaterDrain?.Invoke(zdo.GetBool(WardZdoKeys.WaterDrain, false), (WardRole)zdo.GetInt(WardZdoKeys.WaterDrainMinRole, 0), canManage);
    }

    private void SetFeatureRole(int roleKey, WardRole role)
    {
        if (_ward != null && _ward.m_nview != null && _ward.m_nview.IsValid())
        {
            ZDO zdo = _ward.m_nview.GetZDO();
            zdo.Set(roleKey, (int)role);
            WardNetworkRouter.SendFeatureRole(zdo.m_uid, roleKey, (int)role);
        }
    }

    private void SetZdoBool(int key, bool val)
    {
        if (_ward != null && _ward.m_nview != null && _ward.m_nview.IsValid())
        {
            _ward.m_nview.GetZDO().Set(key, val);
        }
    }

    private void SetZdoFloat(int key, float val)
    {
        if (_ward != null && _ward.m_nview != null && _ward.m_nview.IsValid())
        {
            _ward.m_nview.GetZDO().Set(key, val);
        }
    }
}
