using System;
using UnityEngine;
using WardMeBruh.Configuration;
using WardMeBruh.Services;

namespace WardMeBruh.UI.Tabs;

public class TabUpkeep
{
    private GameObject _root;
    private PrivateArea _ward;

    private Action<bool> _setRepair;
    private Action<float> _setRepairThresholdSlider;

    private Action<bool> _setAutoFuel;

    private Action<bool> _setAutoClose;
    private Action<float> _setAutoCloseSlider;

    private Action<bool> _setWeatherRoof;
    private Action<bool> _setAshShield;
    private Action<bool> _setLavaShield;
    private Action<bool> _setSnowShield;
    private Action<bool> _setWaterDrain;

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
        WardUIFactory.CreateToggleRow(
            card1, 
            "Automated Area Repair", 
            false, 
            v => SetZdoBool(WardZdoKeys.RepairEnabled, v, "upkeep.repair"), 
            out _setRepair);

        WardUIFactory.CreateSliderRow(
            card1, 
            "Repair Damage Threshold", 
            20f, 
            100f, 
            100f, 
            true, 
            "%", 
            v => SetZdoFloat(WardZdoKeys.RepairThreshold, v / 100f, "upkeep.repair"), 
            out _setRepairThresholdSlider);

        WardUIFactory.CreateBadgeRow(card1, "Repair Resource Cost", "Free (0 Wood / Iron / Stone)", WardColorPalette.ValheimBeige, out _);

        Transform card2 = WardUIFactory.CreateCard(listContainer, "Auto-Fuel Supply Depot");
        WardUIFactory.CreateToggleRow(
            card2, 
            "Automated Fuel Distribution", 
            false, 
            v => SetZdoBool(WardZdoKeys.AutoFuelEnabled, v, "upkeep.autofuel"), 
            out _setAutoFuel);

        WardUIFactory.CreateBadgeRow(card2, "Fuel Container Source", "Marked Containers ('Fuel')", WardColorPalette.GoldAmber, out _);
        WardUIFactory.CreateBadgeRow(card2, "Supported Appliances", "Fireplaces, Torches, Smelters, Kilns", WardColorPalette.ValheimBeige, out _);

        Transform card3 = WardUIFactory.CreateCard(listContainer, "Automated Door Control");
        WardUIFactory.CreateToggleRow(
            card3, 
            "Auto-Close Doors Behind Players", 
            false, 
            v => SetZdoBool(WardZdoKeys.AutoCloseDoors, v, "upkeep.autodoors"), 
            out _setAutoClose);

        WardUIFactory.CreateSliderRow(
            card3, 
            "Door Auto-Close Delay", 
            1f, 
            30f, 
            5f, 
            true, 
            "s", 
            v => SetZdoFloat(WardZdoKeys.AutoCloseDelay, v, "upkeep.autodoors"), 
            out _setAutoCloseSlider);

        Transform card4 = WardUIFactory.CreateCard(listContainer, "Environmental & Biome Immunity Shields");
        WardUIFactory.CreateToggleRow(
            card4, 
            "Virtual Weather Roof", 
            false, 
            v => SetZdoBool(WardZdoKeys.WeatherRoof, v, "defenses.biome"), 
            out _setWeatherRoof);

        WardUIFactory.CreateToggleRow(
            card4, 
            "Ashlands Ash Rain Shield", 
            false, 
            v => SetZdoBool(WardZdoKeys.AshShield, v, "defenses.biome"), 
            out _setAshShield);

        WardUIFactory.CreateToggleRow(
            card4, 
            "Lava Damage Shield", 
            false, 
            v => SetZdoBool(WardZdoKeys.LavaShield, v, "defenses.biome"), 
            out _setLavaShield);

        WardUIFactory.CreateToggleRow(
            card4, 
            "Deep North Blizzard Shield", 
            false, 
            v => SetZdoBool(WardZdoKeys.SnowShield, v, "defenses.biome"), 
            out _setSnowShield);

        WardUIFactory.CreateToggleRow(
            card4, 
            "Subsea Water Drain (Dry Basement)", 
            false, 
            v => SetZdoBool(WardZdoKeys.WaterDrain, v, "upkeep.waterdrain"), 
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

        _setRepair?.Invoke(zdo.GetBool(WardZdoKeys.RepairEnabled, false));

        float repThresh = zdo.GetFloat(WardZdoKeys.RepairThreshold, (float)ConfigRegistry.AutoRepairThresholdDefault.Value);
        _setRepairThresholdSlider?.Invoke(Mathf.Round(repThresh * 100f));

        _setAutoFuel?.Invoke(zdo.GetBool(WardZdoKeys.AutoFuelEnabled, false));

        _setAutoClose?.Invoke(zdo.GetBool(WardZdoKeys.AutoCloseDoors, false));

        float closeDelay = zdo.GetFloat(WardZdoKeys.AutoCloseDelay, (float)ConfigRegistry.AutoCloseDelayDefault.Value);
        _setAutoCloseSlider?.Invoke(closeDelay);

        _setWeatherRoof?.Invoke(zdo.GetBool(WardZdoKeys.WeatherRoof, false));
        _setAshShield?.Invoke(zdo.GetBool(WardZdoKeys.AshShield, false));
        _setLavaShield?.Invoke(zdo.GetBool(WardZdoKeys.LavaShield, false));
        _setSnowShield?.Invoke(zdo.GetBool(WardZdoKeys.SnowShield, false));
        _setWaterDrain?.Invoke(zdo.GetBool(WardZdoKeys.WaterDrain, false));
    }

    private void SetZdoBool(int key, bool val, string permissionId)
    {
        if (_ward != null && _ward.m_nview != null && _ward.m_nview.IsValid())
        {
            long playerId = Player.m_localPlayer != null ? Player.m_localPlayer.GetPlayerID() : 0L;
            ZDO zdo = _ward.m_nview.GetZDO();
            if (WardRoleManager.CanPerform(zdo, playerId, permissionId))
            {
                zdo.Set(key, val);
                Refresh();
            }
            else
            {
                Refresh();
            }
        }
    }

    private void SetZdoFloat(int key, float val, string permissionId)
    {
        if (_ward != null && _ward.m_nview != null && _ward.m_nview.IsValid())
        {
            long playerId = Player.m_localPlayer != null ? Player.m_localPlayer.GetPlayerID() : 0L;
            ZDO zdo = _ward.m_nview.GetZDO();
            if (WardRoleManager.CanPerform(zdo, playerId, permissionId))
            {
                zdo.Set(key, val);
            }
            else
            {
                Refresh();
            }
        }
    }
}
