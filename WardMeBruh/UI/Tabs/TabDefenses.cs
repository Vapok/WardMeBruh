using System;
using UnityEngine;
using WardMeBruh.Configuration;
using WardMeBruh.Models;
using WardMeBruh.Networking;
using WardMeBruh.Services;

namespace WardMeBruh.UI.Tabs;

public class TabDefenses
{
    private GameObject _root;
    private PrivateArea _ward;

    private Action<float> _setRadiusSlider;
    private Action<bool> _setRadiusSliderInteractable;
    private Action<string, Color> _setAreaBadge;

    private Action<float> _setPushbackSlider;
    private Action<float> _setShockCreatureSlider;
    private Action<float> _setShockPlayerSlider;

    private Action<bool> _setGhostBase;
    private Action<bool> _setRaidSuppress;
    private Action<bool> _setSpawnSuppress;

    private Action<bool> _setWeatherRoof;
    private Action<bool> _setAshShield;
    private Action<bool> _setLavaShield;
    private Action<bool> _setSnowShield;
    private Action<bool> _setWaterDrain;

    private Action<bool> _setMaxComfort;
    private Action<bool> _setRegenBoost;
    private Action<bool> _setDeathSkill;
    private Action<bool> _setFoodFreeze;

    private Action<bool> _setShowDomeToggle;
    private Action<bool> _setShowRingToggle;

    public GameObject Root => _root;

    public void Build(Transform parent)
    {
        _root = new GameObject("Tab_Defenses", typeof(RectTransform));
        _root.transform.SetParent(parent, false);
        RectTransform rt = _root.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        WardUIFactory.CreateScrollArea(_root.transform, out Transform listContainer);

        float maxRadius = 100f;
        if (ConfigRegistry.MaxWardRadius != null && ConfigRegistry.MaxWardRadius.Value > 10f)
        {
            maxRadius = Mathf.Min(100f, (float)ConfigRegistry.MaxWardRadius.Value);
        }

        Transform cardDomain = WardUIFactory.CreateCard(listContainer, "$wmb_defenses_card_domain");
        WardUIFactory.CreateSliderRow(
            cardDomain, 
            "$wmb_defenses_radius", 
            10f, 
            maxRadius, 
            30f, 
            true, 
            $"{"$wmb_meters".Localize()}", 
            OnRadiusChanged, 
            out _setRadiusSlider,
            out _setRadiusSliderInteractable,
            5f,
            true);
        WardUIFactory.CreateBadgeRow(cardDomain, "$wmb_defenses_land_coverage", "~2,827 m²", WardColorPalette.ValheimBeige, out _setAreaBadge);

        Transform card1 = WardUIFactory.CreateCard(listContainer, "$wmb_defenses_card_repulsion");
        WardUIFactory.CreateSliderRow(
            card1, 
            "$wmb_defenses_pushback", 
            0f, 
            50f, 
            0f, 
            true, 
            " N", 
            v => SetZdoFloat(WardZdoKeys.PushbackForce, v, "defenses.combat"), 
            out _setPushbackSlider);

        WardUIFactory.CreateSliderRow(
            card1, 
            "$wmb_defenses_shock_creatures", 
            0f, 
            100f, 
            0f, 
            true, 
            " dmg", 
            v => SetZdoFloat(WardZdoKeys.ShockCreatureDmg, v, "defenses.combat"), 
            out _setShockCreatureSlider);

        WardUIFactory.CreateSliderRow(
            card1, 
            "$wmb_defenses_shock_players", 
            0f, 
            100f, 
            0f, 
            true, 
            " dmg", 
            v => SetZdoFloat(WardZdoKeys.ShockPlayerDmg, v, "defenses.combat"), 
            out _setShockPlayerSlider);

        Transform card2 = WardUIFactory.CreateCard(listContainer, "$wmb_defenses_card_camouflage");
        WardUIFactory.CreateToggleRow(
            card2, 
            "$wmb_defenses_ghost_base", 
            false, 
            v => SetZdoBool(WardZdoKeys.GhostBase, v, "defenses.camouflage"), 
            out _setGhostBase);

        WardUIFactory.CreateToggleRow(
            card2, 
            "$wmb_defenses_raid_suppression", 
            false, 
            v => SetZdoBool(WardZdoKeys.RaidSuppress, v, "defenses.camouflage"), 
            out _setRaidSuppress);

        WardUIFactory.CreateToggleRow(
            card2, 
            "$wmb_defenses_spawn_suppression", 
            false, 
            v => SetZdoBool(WardZdoKeys.SpawnSuppress, v, "defenses.camouflage"), 
            out _setSpawnSuppress);

        Transform cardBiome = WardUIFactory.CreateCard(listContainer, "$wmb_upkeep_card_biome");
        WardUIFactory.CreateToggleRow(
            cardBiome, 
            "$wmb_upkeep_weather_roof", 
            false, 
            v => SetZdoBool(WardZdoKeys.WeatherRoof, v, "defenses.biome"), 
            out _setWeatherRoof);

        WardUIFactory.CreateToggleRow(
            cardBiome, 
            "$wmb_upkeep_ash_shield", 
            false, 
            v => SetZdoBool(WardZdoKeys.AshShield, v, "defenses.biome"), 
            out _setAshShield);

        WardUIFactory.CreateToggleRow(
            cardBiome, 
            "$wmb_upkeep_lava_shield", 
            false, 
            v => SetZdoBool(WardZdoKeys.LavaShield, v, "defenses.biome"), 
            out _setLavaShield);

        WardUIFactory.CreateToggleRow(
            cardBiome, 
            "$wmb_upkeep_snow_shield", 
            false, 
            v => SetZdoBool(WardZdoKeys.SnowShield, v, "defenses.biome"), 
            out _setSnowShield);

        WardUIFactory.CreateToggleRow(
            cardBiome, 
            "$wmb_upkeep_water_drain", 
            false, 
            v => SetZdoBool(WardZdoKeys.WaterDrain, v, "upkeep.waterdrain"), 
            out _setWaterDrain);

        Transform card3 = WardUIFactory.CreateCard(listContainer, "$wmb_defenses_card_sanctuary");
        WardUIFactory.CreateToggleRow(
            card3, 
            "$wmb_defenses_max_comfort", 
            false, 
            v => SetZdoBool(WardZdoKeys.MaxComfort, v, "upkeep.perks"), 
            out _setMaxComfort);

        WardUIFactory.CreateToggleRow(
            card3,
            "$wmb_defenses_regen_boost",
            true,
            v => SetZdoBool(WardZdoKeys.RegenBoost, v, "upkeep.perks"),
            out _setRegenBoost);

        WardUIFactory.CreateToggleRow(
            card3,
            "$wmb_defenses_death_skill",
            true,
            v => SetZdoBool(WardZdoKeys.DeathSkillShield, v, "upkeep.perks"),
            out _setDeathSkill);

        WardUIFactory.CreateToggleRow(
            card3,
            "$wmb_defenses_food_freeze",
            true,
            v => SetZdoBool(WardZdoKeys.FoodFreeze, v, "upkeep.perks"),
            out _setFoodFreeze);

        Transform card4 = WardUIFactory.CreateCard(listContainer, "$wmb_defenses_card_visuals");
        WardUIFactory.CreateToggleRow(
            card4,
            "$wmb_defenses_show_dome",
            true,
            v => SetZdoBool(WardZdoKeys.ShowShieldDome, v, "defenses.combat"),
            out _setShowDomeToggle);

        WardUIFactory.CreateToggleRow(
            card4,
            "$wmb_defenses_show_ring",
            true,
            v => SetZdoBool(WardZdoKeys.ShowPerimeterRing, v, "defenses.combat"),
            out _setShowRingToggle);
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
        bool canAdjustRadius = WardRoleManager.CanPerform(zdo, playerId, "defenses.dome_size");

        float maxRadius = 100f;
        if (ConfigRegistry.MaxWardRadius != null && ConfigRegistry.MaxWardRadius.Value > 10f)
        {
            maxRadius = Mathf.Min(100f, (float)ConfigRegistry.MaxWardRadius.Value);
        }

        float currentRadius = zdo.GetFloat(WardZdoKeys.Radius, 30.0f);
        if (currentRadius > maxRadius)
        {
            currentRadius = maxRadius;
            zdo.Set(WardZdoKeys.Radius, currentRadius);
        }
        _setRadiusSlider?.Invoke(currentRadius);
        _setRadiusSliderInteractable?.Invoke(canAdjustRadius);

        float areaSqMeters = Mathf.PI * currentRadius * currentRadius;
        _setAreaBadge?.Invoke($"~{Mathf.RoundToInt(areaSqMeters):N0} m²", WardColorPalette.ValheimBeige);

        float push = zdo.GetFloat(WardZdoKeys.PushbackForce, 0f);
        _setPushbackSlider?.Invoke(push);

        float shockC = zdo.GetFloat(WardZdoKeys.ShockCreatureDmg, 0f);
        _setShockCreatureSlider?.Invoke(shockC);

        float shockP = zdo.GetFloat(WardZdoKeys.ShockPlayerDmg, 0f);
        _setShockPlayerSlider?.Invoke(shockP);

        _setGhostBase?.Invoke(zdo.GetBool(WardZdoKeys.GhostBase, false));
        _setRaidSuppress?.Invoke(zdo.GetBool(WardZdoKeys.RaidSuppress, false));
        _setSpawnSuppress?.Invoke(zdo.GetBool(WardZdoKeys.SpawnSuppress, false));

        _setWeatherRoof?.Invoke(zdo.GetBool(WardZdoKeys.WeatherRoof, false));
        _setAshShield?.Invoke(zdo.GetBool(WardZdoKeys.AshShield, false));
        _setLavaShield?.Invoke(zdo.GetBool(WardZdoKeys.LavaShield, false));
        _setSnowShield?.Invoke(zdo.GetBool(WardZdoKeys.SnowShield, false));
        _setWaterDrain?.Invoke(zdo.GetBool(WardZdoKeys.WaterDrain, false));

        _setMaxComfort?.Invoke(zdo.GetBool(WardZdoKeys.MaxComfort, false));
        _setRegenBoost?.Invoke(zdo.GetBool(WardZdoKeys.RegenBoost, true));
        _setDeathSkill?.Invoke(zdo.GetBool(WardZdoKeys.DeathSkillShield, true));
        _setFoodFreeze?.Invoke(zdo.GetBool(WardZdoKeys.FoodFreeze, true));

        _setShowDomeToggle?.Invoke(zdo.GetBool(WardZdoKeys.ShowShieldDome, true));
        _setShowRingToggle?.Invoke(zdo.GetBool(WardZdoKeys.ShowPerimeterRing, true));
    }

    private void SetZdoBool(int key, bool val, string permissionId)
    {
        if (_ward != null && _ward.m_nview != null && _ward.m_nview.IsValid())
        {
            long playerId = Player.m_localPlayer != null ? Player.m_localPlayer.GetPlayerID() : 0L;
            ZDO zdo = _ward.m_nview.GetZDO();
            if (WardRoleManager.CanPerform(zdo, playerId, permissionId))
            {
                WardNetworkRouter.SendWardBool(zdo.m_uid, key, val);
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
                WardNetworkRouter.SendWardFloat(zdo.m_uid, key, val);
            }
            else
            {
                Refresh();
            }
        }
    }

    private void OnRadiusChanged(float value)
    {
        if (_ward == null || _ward.m_nview == null || !_ward.m_nview.IsValid())
            return;

        long playerId = Player.m_localPlayer != null ? Player.m_localPlayer.GetPlayerID() : 0L;
        ZDO zdo = _ward.m_nview.GetZDO();
        if (!WardRoleManager.CanPerform(zdo, playerId, "defenses.dome_size"))
            return;

        float clampedValue = Mathf.Clamp(value, 10f, 100f);
        WardNetworkRouter.SendWardFloat(zdo.m_uid, WardZdoKeys.Radius, clampedValue);
        if (_ward.m_areaMarker != null)
        {
            _ward.m_areaMarker.m_radius = clampedValue;
        }

        float areaSqMeters = Mathf.PI * clampedValue * clampedValue;
        _setAreaBadge?.Invoke($"~{Mathf.RoundToInt(areaSqMeters):N0} m²", WardColorPalette.ValheimBeige);
    }
}
