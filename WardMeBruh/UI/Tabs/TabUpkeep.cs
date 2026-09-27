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

        Transform card1 = WardUIFactory.CreateCard(listContainer, "$wmb_upkeep_card_repair");
        WardUIFactory.CreateToggleRow(
            card1, 
            "$wmb_upkeep_repair", 
            false, 
            v => SetZdoBool(WardZdoKeys.RepairEnabled, v, "upkeep.repair"), 
            out _setRepair);

        WardUIFactory.CreateSliderRow(
            card1, 
            "$wmb_upkeep_repair_threshold", 
            20f, 
            100f, 
            100f, 
            true, 
            "%", 
            v => SetZdoFloat(WardZdoKeys.RepairThreshold, v / 100f, "upkeep.repair"), 
            out _setRepairThresholdSlider);

        WardUIFactory.CreateBadgeRow(card1, "$wmb_upkeep_repair_cost", "$wmb_upkeep_repair_cost_val".Localize(), WardColorPalette.ValheimBeige, out _);

        Transform card2 = WardUIFactory.CreateCard(listContainer, "$wmb_upkeep_card_fuel");
        WardUIFactory.CreateToggleRow(
            card2, 
            "$wmb_upkeep_fuel", 
            false, 
            v => SetZdoBool(WardZdoKeys.AutoFuelEnabled, v, "upkeep.autofuel"), 
            out _setAutoFuel);

        WardUIFactory.CreateBadgeRow(card2, "$wmb_upkeep_fuel_source", "$wmb_upkeep_fuel_source_val".Localize(), WardColorPalette.GoldAmber, out _);
        WardUIFactory.CreateBadgeRow(card2, "$wmb_upkeep_appliances", "$wmb_upkeep_appliances_val".Localize(), WardColorPalette.ValheimBeige, out _);

        Transform card3 = WardUIFactory.CreateCard(listContainer, "$wmb_upkeep_card_doors");
        WardUIFactory.CreateToggleRow(
            card3, 
            "$wmb_upkeep_auto_close_doors", 
            false, 
            v => SetZdoBool(WardZdoKeys.AutoCloseDoors, v, "upkeep.autodoors"), 
            out _setAutoClose);

        WardUIFactory.CreateSliderRow(
            card3, 
            "$wmb_upkeep_auto_close_delay", 
            1f, 
            30f, 
            5f, 
            true, 
            "s", 
            v => SetZdoFloat(WardZdoKeys.AutoCloseDelay, v, "upkeep.autodoors"), 
            out _setAutoCloseSlider);

        Transform card4 = WardUIFactory.CreateCard(listContainer, "$wmb_upkeep_card_biome");
        WardUIFactory.CreateToggleRow(
            card4, 
            "$wmb_upkeep_weather_roof", 
            false, 
            v => SetZdoBool(WardZdoKeys.WeatherRoof, v, "defenses.biome"), 
            out _setWeatherRoof);

        WardUIFactory.CreateToggleRow(
            card4, 
            "$wmb_upkeep_ash_shield", 
            false, 
            v => SetZdoBool(WardZdoKeys.AshShield, v, "defenses.biome"), 
            out _setAshShield);

        WardUIFactory.CreateToggleRow(
            card4, 
            "$wmb_upkeep_lava_shield", 
            false, 
            v => SetZdoBool(WardZdoKeys.LavaShield, v, "defenses.biome"), 
            out _setLavaShield);

        WardUIFactory.CreateToggleRow(
            card4, 
            "$wmb_upkeep_snow_shield", 
            false, 
            v => SetZdoBool(WardZdoKeys.SnowShield, v, "defenses.biome"), 
            out _setSnowShield);

        WardUIFactory.CreateToggleRow(
            card4, 
            "$wmb_upkeep_water_drain", 
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
