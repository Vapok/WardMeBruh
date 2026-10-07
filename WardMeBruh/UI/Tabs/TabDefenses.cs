using System;
using System.Collections.Generic;
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
    private Action<float, float, float> _setRadiusSliderRange;
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

    private readonly List<GameObject> _standardDefenseCards = new();
    private GameObject _delegatedCard;
    private GameObject _visualsCard;
    private Action<string, Color> _setDelegatedBadge;
    private bool _isRefreshing;

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
            out _setRadiusSliderRange,
            5f,
            true);
        WardUIFactory.CreateBadgeRow(cardDomain, "$wmb_defenses_land_coverage", "~2,827 m²", WardColorPalette.ValheimBeige, out _setAreaBadge);

        Transform delegatedCard = WardUIFactory.CreateCard(listContainer, "$wmb_tab_defenses");
        _delegatedCard = delegatedCard.parent.parent.gameObject;
        WardUIFactory.CreateBadgeRow(delegatedCard, "$wmb_territory_label", "...", WardColorPalette.GoldAmber, out _setDelegatedBadge);
        _delegatedCard.SetActive(false);

        Transform card1 = WardUIFactory.CreateCard(listContainer, "$wmb_defenses_card_repulsion");
        _standardDefenseCards.Add(card1.parent.parent.gameObject);
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
        _standardDefenseCards.Add(card2.parent.parent.gameObject);
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
        _standardDefenseCards.Add(cardBiome.parent.parent.gameObject);
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
        _standardDefenseCards.Add(card3.parent.parent.gameObject);
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
        _visualsCard = card4.parent.parent.gameObject;
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

        _isRefreshing = true;
        try
        {
            ZDO zdo = _ward.m_nview.GetZDO();
        long playerId = Player.m_localPlayer != null ? Player.m_localPlayer.GetPlayerID() : 0L;
        bool canAdjustRadius = WardRoleManager.CanPerform(zdo, playerId, "defenses.dome_size");

        float maxRadius = 100f;
        if (ConfigRegistry.MaxWardRadius != null && ConfigRegistry.MaxWardRadius.Value > 10f)
        {
            maxRadius = Mathf.Min(100f, (float)ConfigRegistry.MaxWardRadius.Value);
        }

        WardType wardType = (WardType)zdo.GetInt(WardZdoKeys.WardType, (int)WardType.Main);
        float sliderMin = 10f;
        float sliderMax = maxRadius;
        float sliderStep = 5f;

        if (wardType == WardType.SubWard)
        {
            float maxAllowed = WardQueryService.GetMaxAllowedSubWardRadius(zdo, _ward.transform.position);
            sliderMax = Mathf.Max(1f, Mathf.Floor(maxAllowed));
            if (sliderMax < 50f)
            {
                sliderStep = 1f;
                sliderMin = 1f;
            }
            else
            {
                sliderMin = 10f;
                sliderStep = 5f;
            }
        }

        ZDOID parentId = zdo.GetZDOID(WardZdoKeys.ParentWard);
        string parentName = "Parent Ward";
        if (parentId != ZDOID.None)
        {
            if (ZDOMan.instance != null)
            {
                ZDO parentZdo = ZDOMan.instance.GetZDO(parentId);
                if (parentZdo != null)
                {
                    parentName = parentZdo.GetString(WardZdoKeys.CustomName, "Parent Ward");
                }
            }

            if (parentName == "Parent Ward")
            {
                KnownWard parentKnown = WardRegistryManager.Instance.GetKnownWard(parentId);
                if (parentKnown != null && !string.IsNullOrEmpty(parentKnown.CustomName))
                {
                    parentName = parentKnown.CustomName;
                }
            }

            if (parentName == "Parent Ward" && PrivateArea.m_allAreas != null)
            {
                for (int i = 0; i < PrivateArea.m_allAreas.Count; i++)
                {
                    PrivateArea area = PrivateArea.m_allAreas[i];
                    if (area != null && area.m_nview != null && area.m_nview.IsValid() && area.m_nview.GetZDO().m_uid == parentId)
                    {
                        parentName = area.m_nview.GetZDO().GetString(WardZdoKeys.CustomName, area.m_name);
                        break;
                    }
                }
            }
        }

        bool isDelegated = wardType == WardType.SubWard || wardType == WardType.Extension;
        if (_delegatedCard != null)
        {
            _delegatedCard.SetActive(isDelegated);
            if (isDelegated)
            {
                string msg = wardType == WardType.SubWard
                    ? string.Format("$wmb_subward_defenses_delegated".Localize(), parentName)
                    : string.Format("$wmb_extension_managed_by".Localize(), parentName);
                _setDelegatedBadge?.Invoke(msg, WardColorPalette.GoldAmber);
            }
        }

        for (int i = 0; i < _standardDefenseCards.Count; i++)
        {
            if (_standardDefenseCards[i] != null)
            {
                _standardDefenseCards[i].SetActive(!isDelegated);
            }
        }

        if (_visualsCard != null)
        {
            _visualsCard.SetActive(true);
        }

        float currentRadius = zdo.GetFloat(WardZdoKeys.Radius, 30.0f);
        if (currentRadius > sliderMax)
        {
            currentRadius = sliderMax;
            zdo.Set(WardZdoKeys.Radius, currentRadius);
            WardNetworkRouter.SendWardFloat(zdo.m_uid, WardZdoKeys.Radius, currentRadius);
            if (_ward.m_areaMarker != null)
            {
                _ward.m_areaMarker.m_radius = currentRadius;
            }
        }
        else if (currentRadius < sliderMin)
        {
            currentRadius = sliderMin;
            zdo.Set(WardZdoKeys.Radius, currentRadius);
            WardNetworkRouter.SendWardFloat(zdo.m_uid, WardZdoKeys.Radius, currentRadius);
            if (_ward.m_areaMarker != null)
            {
                _ward.m_areaMarker.m_radius = currentRadius;
            }
        }

        _setRadiusSliderRange?.Invoke(sliderMin, sliderMax, sliderStep);
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
    finally
    {
        _isRefreshing = false;
    }
}

    private void SetZdoBool(int key, bool val, string permissionId)
    {
        if (_isRefreshing)
            return;

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
        if (_isRefreshing)
            return;

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
        if (_isRefreshing)
            return;

        if (_ward == null || _ward.m_nview == null || !_ward.m_nview.IsValid())
            return;

        long playerId = Player.m_localPlayer != null ? Player.m_localPlayer.GetPlayerID() : 0L;
        ZDO zdo = _ward.m_nview.GetZDO();
        if (!WardRoleManager.CanPerform(zdo, playerId, "defenses.dome_size"))
            return;

        float maxRadius = 100f;
        if (ConfigRegistry.MaxWardRadius != null && ConfigRegistry.MaxWardRadius.Value > 10f)
        {
            maxRadius = Mathf.Min(100f, (float)ConfigRegistry.MaxWardRadius.Value);
        }

        float minRadius = 10f;
        WardType wardType = (WardType)zdo.GetInt(WardZdoKeys.WardType, (int)WardType.Main);
        if (wardType == WardType.SubWard)
        {
            float maxAllowed = WardQueryService.GetMaxAllowedSubWardRadius(zdo, _ward.transform.position);
            maxRadius = Mathf.Max(1f, Mathf.Floor(maxAllowed));
            minRadius = maxRadius < 50f ? 1f : 10f;
        }

        float clampedValue = Mathf.Clamp(value, minRadius, maxRadius);
        WardNetworkRouter.SendWardFloat(zdo.m_uid, WardZdoKeys.Radius, clampedValue);
        if (_ward.m_areaMarker != null)
        {
            _ward.m_areaMarker.m_radius = clampedValue;
        }

        float areaSqMeters = Mathf.PI * clampedValue * clampedValue;
        _setAreaBadge?.Invoke($"~{Mathf.RoundToInt(areaSqMeters):N0} m²", WardColorPalette.ValheimBeige);
    }
}
