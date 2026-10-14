using System;
using System.Collections.Generic;
using Jotunn.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WardMeBruh.Components;
using WardMeBruh.Configuration;
using WardMeBruh.Models;
using WardMeBruh.Networking;
using WardMeBruh.Services;
using WardMeBruh.UI.Elements;

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

    private GameObject _cardRepulsionObj;
    private GameObject _pushbackRowObj;
    private GameObject _shockCreatureRowObj;
    private GameObject _shockPlayerRowObj;

    private GameObject _cardCamouflageObj;
    private GameObject _ghostBaseRowObj;
    private GameObject _raidSuppressRowObj;
    private GameObject _spawnSuppressRowObj;

    private GameObject _cardBiomeObj;
    private GameObject _weatherRoofRowObj;
    private GameObject _ashShieldRowObj;
    private GameObject _lavaShieldRowObj;
    private GameObject _snowShieldRowObj;
    private GameObject _waterDrainRowObj;

    private GameObject _cardSanctuaryObj;
    private GameObject _maxComfortRowObj;
    private GameObject _regenBoostRowObj;
    private GameObject _deathSkillRowObj;
    private GameObject _foodFreezeRowObj;
    private Action<string, Color> _setDelegatedBadge;

    private GameObject _domeColorRowObj;
    private GameObject _colorWheelPanelObj;
    private Image _colorSwatchImg;
    private TextMeshProUGUI _colorHexTmp;
    private WardColorWheelPicker _colorPicker;
    private Slider _brightnessSlider;
    private TextMeshProUGUI _brightnessValTmp;
    private TextMeshProUGUI _expandBtnText;
    private bool _isColorWheelExpanded;
    private Color _currentColor = WardColorPalette.AetherIce;

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
            0f, 
            maxRadius, 
            25f, 
            true, 
            $"{"$wmb_meters".Localize()}", 
            OnRadiusChanged, 
            out _setRadiusSlider,
            out _setRadiusSliderInteractable,
            out _setRadiusSliderRange,
            maxRadius <= 50f ? 1f : 5f,
            true);
        WardUIFactory.CreateBadgeRow(cardDomain, "$wmb_defenses_land_coverage", "~2,827 m²", WardColorPalette.ValheimBeige, out _setAreaBadge);

        Transform delegatedCard = WardUIFactory.CreateCard(listContainer, "$wmb_tab_defenses");
        _delegatedCard = delegatedCard.parent.parent.gameObject;
        WardUIFactory.CreateBadgeRow(delegatedCard, "$wmb_territory_label", "...", WardColorPalette.GoldAmber, out _setDelegatedBadge);
        _delegatedCard.SetActive(false);

        Transform card1 = WardUIFactory.CreateCard(listContainer, "$wmb_defenses_card_repulsion");
        _cardRepulsionObj = card1.parent.parent.gameObject;
        _standardDefenseCards.Add(_cardRepulsionObj);
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
        _pushbackRowObj = card1.GetChild(card1.childCount - 1).gameObject;

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
        _shockCreatureRowObj = card1.GetChild(card1.childCount - 1).gameObject;

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
        _shockPlayerRowObj = card1.GetChild(card1.childCount - 1).gameObject;

        Transform card2 = WardUIFactory.CreateCard(listContainer, "$wmb_defenses_card_camouflage");
        _cardCamouflageObj = card2.parent.parent.gameObject;
        _standardDefenseCards.Add(_cardCamouflageObj);
        WardUIFactory.CreateToggleRow(
            card2, 
            "$wmb_defenses_ghost_base", 
            false, 
            v => SetZdoBool(WardZdoKeys.GhostBase, v, "defenses.camouflage"), 
            out _setGhostBase);
        _ghostBaseRowObj = card2.GetChild(card2.childCount - 1).gameObject;

        WardUIFactory.CreateToggleRow(
            card2, 
            "$wmb_defenses_raid_suppression", 
            false, 
            v => SetZdoBool(WardZdoKeys.RaidSuppress, v, "defenses.camouflage"), 
            out _setRaidSuppress);
        _raidSuppressRowObj = card2.GetChild(card2.childCount - 1).gameObject;

        WardUIFactory.CreateToggleRow(
            card2, 
            "$wmb_defenses_spawn_suppression", 
            false, 
            v => SetZdoBool(WardZdoKeys.SpawnSuppress, v, "defenses.camouflage"), 
            out _setSpawnSuppress);
        _spawnSuppressRowObj = card2.GetChild(card2.childCount - 1).gameObject;

        Transform cardBiome = WardUIFactory.CreateCard(listContainer, "$wmb_upkeep_card_biome");
        _cardBiomeObj = cardBiome.parent.parent.gameObject;
        _standardDefenseCards.Add(_cardBiomeObj);
        WardUIFactory.CreateToggleRow(
            cardBiome, 
            "$wmb_upkeep_weather_roof", 
            false, 
            v => SetZdoBool(WardZdoKeys.WeatherRoof, v, "defenses.biome"), 
            out _setWeatherRoof);
        _weatherRoofRowObj = cardBiome.GetChild(cardBiome.childCount - 1).gameObject;

        WardUIFactory.CreateToggleRow(
            cardBiome, 
            "$wmb_upkeep_ash_shield", 
            false, 
            v => SetZdoBool(WardZdoKeys.AshShield, v, "defenses.biome"), 
            out _setAshShield);
        _ashShieldRowObj = cardBiome.GetChild(cardBiome.childCount - 1).gameObject;

        WardUIFactory.CreateToggleRow(
            cardBiome, 
            "$wmb_upkeep_lava_shield", 
            false, 
            v => SetZdoBool(WardZdoKeys.LavaShield, v, "defenses.biome"), 
            out _setLavaShield);
        _lavaShieldRowObj = cardBiome.GetChild(cardBiome.childCount - 1).gameObject;

        WardUIFactory.CreateToggleRow(
            cardBiome, 
            "$wmb_upkeep_snow_shield", 
            false, 
            v => SetZdoBool(WardZdoKeys.SnowShield, v, "defenses.biome"), 
            out _setSnowShield);
        _snowShieldRowObj = cardBiome.GetChild(cardBiome.childCount - 1).gameObject;

        WardUIFactory.CreateToggleRow(
            cardBiome, 
            "$wmb_upkeep_water_drain", 
            false, 
            v => SetZdoBool(WardZdoKeys.WaterDrain, v, "upkeep.waterdrain"), 
            out _setWaterDrain);
        _waterDrainRowObj = cardBiome.GetChild(cardBiome.childCount - 1).gameObject;

        Transform card3 = WardUIFactory.CreateCard(listContainer, "$wmb_defenses_card_sanctuary");
        _cardSanctuaryObj = card3.parent.parent.gameObject;
        _standardDefenseCards.Add(_cardSanctuaryObj);
        WardUIFactory.CreateToggleRow(
            card3, 
            "$wmb_defenses_max_comfort", 
            false, 
            v => SetZdoBool(WardZdoKeys.MaxComfort, v, "upkeep.perks"), 
            out _setMaxComfort);
        _maxComfortRowObj = card3.GetChild(card3.childCount - 1).gameObject;

        WardUIFactory.CreateToggleRow(
            card3,
            "$wmb_defenses_regen_boost",
            true,
            v => SetZdoBool(WardZdoKeys.RegenBoost, v, "upkeep.perks"),
            out _setRegenBoost);
        _regenBoostRowObj = card3.GetChild(card3.childCount - 1).gameObject;

        WardUIFactory.CreateToggleRow(
            card3,
            "$wmb_defenses_death_skill",
            true,
            v => SetZdoBool(WardZdoKeys.DeathSkillShield, v, "upkeep.perks"),
            out _setDeathSkill);
        _deathSkillRowObj = card3.GetChild(card3.childCount - 1).gameObject;

        WardUIFactory.CreateToggleRow(
            card3,
            "$wmb_defenses_food_freeze",
            true,
            v => SetZdoBool(WardZdoKeys.FoodFreeze, v, "upkeep.perks"),
            out _setFoodFreeze);
        _foodFreezeRowObj = card3.GetChild(card3.childCount - 1).gameObject;

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

        _domeColorRowObj = WardUIFactory.CreateSettingRow(card4, "$wmb_defenses_dome_color".Localize(), out Transform colorCtrlContainer, 34f);

        GameObject swatchBorder = new("SwatchBorder", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
        swatchBorder.transform.SetParent(colorCtrlContainer, false);
        swatchBorder.GetComponent<RectTransform>().sizeDelta = new Vector2(32f, 24f);
        swatchBorder.SetLayoutDimensions(32f, 24f);
        swatchBorder.GetComponent<Image>().color = WardColorPalette.CardBorder;

        GameObject swatchObj = new("Swatch", typeof(RectTransform), typeof(Image));
        swatchObj.transform.SetParent(swatchBorder.transform, false);
        RectTransform swatchRt = swatchObj.GetComponent<RectTransform>();
        swatchRt.anchorMin = Vector2.zero;
        swatchRt.anchorMax = Vector2.one;
        swatchRt.offsetMin = new Vector2(2f, 2f);
        swatchRt.offsetMax = new Vector2(-2f, -2f);
        _colorSwatchImg = swatchObj.GetComponent<Image>();
        _colorSwatchImg.color = WardColorPalette.AetherIce;

        GameObject hexTextObj = new("HexText", typeof(RectTransform), typeof(TextMeshProUGUI), typeof(LayoutElement), typeof(WardTextElement));
        hexTextObj.transform.SetParent(colorCtrlContainer, false);
        hexTextObj.GetComponent<RectTransform>().sizeDelta = new Vector2(80f, 24f);
        hexTextObj.SetLayoutDimensions(80f, 24f);
        _colorHexTmp = hexTextObj.GetComponent<TextMeshProUGUI>();
        _colorHexTmp.text = "#38BDF8";
        _colorHexTmp.font = UIFonts.GetPrimaryFont();
        _colorHexTmp.fontSize = 12f;
        _colorHexTmp.color = WardColorPalette.ValheimBeige;
        _colorHexTmp.alignment = TextAlignmentOptions.Center;

        WardTextElement hexWte = hexTextObj.GetComponent<WardTextElement>();
        hexWte.baseFontSize = 12;

        GameObject expandBtn = GUIManager.Instance.CreateButton(
            "▼",
            colorCtrlContainer,
            Vector2.zero,
            Vector2.zero,
            Vector2.zero,
            36f,
            24f);
        expandBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(36f, 24f);
        expandBtn.SetLayoutDimensions(36f, 24f);
        _expandBtnText = WardUIFactory.SetupButtonText(expandBtn, "▼", 12f, WardColorPalette.ValheimBeige);

        expandBtn.GetComponent<Button>().onClick.AddListener(() =>
        {
            _isColorWheelExpanded = !_isColorWheelExpanded;
            _colorWheelPanelObj?.SetActive(_isColorWheelExpanded);
            _expandBtnText.text = _isColorWheelExpanded ? "▲" : "▼";
        });

        _colorWheelPanelObj = new GameObject("ColorWheelPanel", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter), typeof(LayoutElement));
        _colorWheelPanelObj.transform.SetParent(card4, false);
        _colorWheelPanelObj.SetLayoutDimensions(-1f, -1f);

        Image panelBg = _colorWheelPanelObj.GetComponent<Image>();
        panelBg.color = new Color(0.04f, 0.03f, 0.02f, 0.92f);

        VerticalLayoutGroup panelVlg = _colorWheelPanelObj.GetComponent<VerticalLayoutGroup>();
        panelVlg.padding = new RectOffset(16, 16, 12, 12);
        panelVlg.spacing = 10f;
        panelVlg.childControlWidth = true;
        panelVlg.childControlHeight = true;
        panelVlg.childForceExpandWidth = true;
        panelVlg.childForceExpandHeight = false;

        ContentSizeFitter panelCsf = _colorWheelPanelObj.GetComponent<ContentSizeFitter>();
        panelCsf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        GameObject contentRow = new("ContentRow", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
        contentRow.transform.SetParent(_colorWheelPanelObj.transform, false);
        contentRow.SetLayoutDimensions(-1f, 160f);

        HorizontalLayoutGroup contentHlg = contentRow.GetComponent<HorizontalLayoutGroup>();
        contentHlg.spacing = 16f;
        contentHlg.childAlignment = TextAnchor.MiddleCenter;
        contentHlg.childControlWidth = false;
        contentHlg.childControlHeight = false;
        contentHlg.childForceExpandWidth = false;
        contentHlg.childForceExpandHeight = false;

        WardColorWheelPicker.Create(contentRow.transform, 150f, OnWheelColorChanged, out _colorPicker);

        GameObject controlsCol = new("ControlsCol", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(LayoutElement));
        controlsCol.transform.SetParent(contentRow.transform, false);
        controlsCol.SetLayoutDimensions(270f, 150f);

        VerticalLayoutGroup colVlg = controlsCol.GetComponent<VerticalLayoutGroup>();
        colVlg.spacing = 8f;
        colVlg.childAlignment = TextAnchor.UpperLeft;
        colVlg.childControlWidth = true;
        colVlg.childControlHeight = false;
        colVlg.childForceExpandWidth = true;
        colVlg.childForceExpandHeight = false;

        GameObject brightRow = new("BrightnessRow", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
        brightRow.transform.SetParent(controlsCol.transform, false);
        brightRow.SetLayoutDimensions(270f, 24f);
        HorizontalLayoutGroup brightHlg = brightRow.GetComponent<HorizontalLayoutGroup>();
        brightHlg.spacing = 8f;
        brightHlg.childAlignment = TextAnchor.MiddleLeft;
        brightHlg.childControlWidth = false;
        brightHlg.childControlHeight = true;

        GameObject brightLbl = new("BrightLbl", typeof(RectTransform), typeof(TextMeshProUGUI), typeof(LayoutElement));
        brightLbl.transform.SetParent(brightRow.transform, false);
        brightLbl.SetLayoutDimensions(80f, 24f);
        TextMeshProUGUI bLblTmp = brightLbl.GetComponent<TextMeshProUGUI>();
        bLblTmp.text = "Brightness:";
        bLblTmp.font = UIFonts.GetPrimaryFont();
        bLblTmp.fontSize = 11f;
        bLblTmp.color = WardColorPalette.ValheimBeige;
        bLblTmp.alignment = TextAlignmentOptions.MidlineLeft;

        GameObject sliderObj = WardUIExtensions.CreateValheimSlider(brightRow.transform, 0.2f, 1.0f, 1.0f, false, 120f, 18f);
        sliderObj.SetLayoutDimensions(120f, 18f);
        _brightnessSlider = sliderObj.GetComponent<Slider>();
        _brightnessSlider.onValueChanged.AddListener(OnBrightnessChanged);

        GameObject brightValObj = new("BrightVal", typeof(RectTransform), typeof(TextMeshProUGUI), typeof(LayoutElement));
        brightValObj.transform.SetParent(brightRow.transform, false);
        brightValObj.SetLayoutDimensions(40f, 24f);
        _brightnessValTmp = brightValObj.GetComponent<TextMeshProUGUI>();
        _brightnessValTmp.text = "100%";
        _brightnessValTmp.font = UIFonts.GetPrimaryFont();
        _brightnessValTmp.fontSize = 11f;
        _brightnessValTmp.color = WardColorPalette.ValheimBeige;
        _brightnessValTmp.alignment = TextAlignmentOptions.MidlineRight;

        GameObject presetsRow = new("PresetsRow", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
        presetsRow.transform.SetParent(controlsCol.transform, false);
        presetsRow.SetLayoutDimensions(270f, 26f);
        HorizontalLayoutGroup pHLG = presetsRow.GetComponent<HorizontalLayoutGroup>();
        pHLG.spacing = 6f;
        pHLG.childAlignment = TextAnchor.MiddleLeft;
        pHLG.childControlWidth = false;
        pHLG.childControlHeight = false;

        Color[] presets = new Color[]
        {
            new(0.22f, 0.74f, 0.97f),
            new(0.06f, 0.72f, 0.51f),
            new(0.95f, 0.25f, 0.25f),
            new(0.65f, 0.35f, 0.95f),
            new(0.98f, 0.75f, 0.14f),
            new(0.95f, 0.25f, 0.55f),
            new(0.10f, 0.80f, 0.75f),
            new(0.95f, 0.95f, 0.95f)
        };

        for (int p = 0; p < presets.Length; p++)
        {
            Color presetCol = presets[p];
            GameObject pBtnObj = new($"Preset_{p}", typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
            pBtnObj.transform.SetParent(presetsRow.transform, false);
            pBtnObj.SetLayoutDimensions(26f, 24f);
            pBtnObj.GetComponent<RectTransform>().sizeDelta = new Vector2(26f, 24f);
            pBtnObj.GetComponent<Image>().color = presetCol;
            pBtnObj.GetComponent<Button>().onClick.AddListener(() =>
            {
                ApplyNewColor(presetCol);
            });
        }

        GameObject actionRow = new("ActionRow", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
        actionRow.transform.SetParent(controlsCol.transform, false);
        actionRow.SetLayoutDimensions(270f, 28f);
        HorizontalLayoutGroup actHlg = actionRow.GetComponent<HorizontalLayoutGroup>();
        actHlg.spacing = 10f;
        actHlg.childAlignment = TextAnchor.MiddleLeft;
        actHlg.childControlWidth = false;
        actHlg.childControlHeight = true;

        GameObject resetBtn = GUIManager.Instance.CreateButton(
            "$wmb_defenses_color_reset".Localize(),
            actionRow.transform,
            Vector2.zero,
            Vector2.zero,
            Vector2.zero,
            130f,
            26f);
        resetBtn.SetLayoutDimensions(130f, 26f);
        WardUIFactory.SetupButtonText(resetBtn, "$wmb_defenses_color_reset".Localize(), 11f, WardColorPalette.ValheimBeige);
        resetBtn.GetComponent<Button>().onClick.AddListener(() =>
        {
            ApplyNewColor(WardColorPalette.AetherIce);
        });

        _colorWheelPanelObj.SetActive(false);
        _domeColorRowObj.SetActive(false);
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
        float sliderMin = 0f;
        float sliderMax = maxRadius;
        float sliderStep = 1f;

        if (wardType == WardType.SubWard)
        {
            float maxAllowed = WardQueryService.GetMaxAllowedSubWardRadius(zdo, _ward.transform.position);
            sliderMax = Mathf.Max(1f, Mathf.Floor(maxAllowed));
        }
        else
        {
            float progressionSliderMax = WardProgressionService.GetEffectiveSliderMaxRadius(zdo, playerId);
            sliderMax = Mathf.Min(sliderMax, progressionSliderMax);
        }

        sliderMin = 0f;
        sliderStep = (sliderMax - sliderMin) <= 50f ? 1f : 5f;

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

        bool hasPushback = WardProgressionService.IsFeatureUnlocked(zdo, WardFeature.PushbackForce, playerId);
        bool hasShock = WardProgressionService.IsFeatureUnlocked(zdo, WardFeature.CreatureShock, playerId);
        bool hasPlayerShock = WardProgressionService.IsFeatureUnlocked(zdo, WardFeature.PlayerShock, playerId);
        if (_pushbackRowObj != null) _pushbackRowObj.SetActive(hasPushback);
        if (_shockCreatureRowObj != null) _shockCreatureRowObj.SetActive(hasShock);
        if (_shockPlayerRowObj != null) _shockPlayerRowObj.SetActive(hasPlayerShock);
        if (_cardRepulsionObj != null) _cardRepulsionObj.SetActive(!isDelegated && (hasPushback || hasShock || hasPlayerShock));

        bool hasGhostBase = WardProgressionService.IsFeatureUnlocked(zdo, WardFeature.GhostBase, playerId);
        bool hasRaidSuppress = WardProgressionService.IsFeatureUnlocked(zdo, WardFeature.RaidSuppression, playerId);
        bool hasSpawnSuppress = WardProgressionService.IsFeatureUnlocked(zdo, WardFeature.SpawnSuppression, playerId);
        if (_ghostBaseRowObj != null) _ghostBaseRowObj.SetActive(hasGhostBase);
        if (_raidSuppressRowObj != null) _raidSuppressRowObj.SetActive(hasRaidSuppress);
        if (_spawnSuppressRowObj != null) _spawnSuppressRowObj.SetActive(hasSpawnSuppress);
        if (_cardCamouflageObj != null) _cardCamouflageObj.SetActive(!isDelegated && (hasGhostBase || hasRaidSuppress || hasSpawnSuppress));

        bool hasWeatherRoof = WardProgressionService.IsFeatureUnlocked(zdo, WardFeature.WeatherRoof, playerId);
        bool hasAshShield = WardProgressionService.IsFeatureUnlocked(zdo, WardFeature.AshShield, playerId);
        bool hasLavaShield = WardProgressionService.IsFeatureUnlocked(zdo, WardFeature.LavaShield, playerId);
        bool hasSnowShield = WardProgressionService.IsFeatureUnlocked(zdo, WardFeature.SnowShield, playerId);
        bool hasWaterDrain = WardProgressionService.IsFeatureUnlocked(zdo, WardFeature.WaterDrain, playerId);
        if (_weatherRoofRowObj != null) _weatherRoofRowObj.SetActive(hasWeatherRoof);
        if (_ashShieldRowObj != null) _ashShieldRowObj.SetActive(hasAshShield);
        if (_lavaShieldRowObj != null) _lavaShieldRowObj.SetActive(hasLavaShield);
        if (_snowShieldRowObj != null) _snowShieldRowObj.SetActive(hasSnowShield);
        if (_waterDrainRowObj != null) _waterDrainRowObj.SetActive(hasWaterDrain);
        if (_cardBiomeObj != null) _cardBiomeObj.SetActive(!isDelegated && (hasWeatherRoof || hasAshShield || hasLavaShield || hasSnowShield || hasWaterDrain));

        bool hasMaxComfort = WardProgressionService.IsFeatureUnlocked(zdo, WardFeature.MaxComfort, playerId);
        bool hasRegenBoost = WardProgressionService.IsFeatureUnlocked(zdo, WardFeature.RegenBoost, playerId) ||
                             WardProgressionService.IsFeatureUnlocked(zdo, WardFeature.StaminaRegenBoost, playerId) ||
                             WardProgressionService.IsFeatureUnlocked(zdo, WardFeature.EitrRegenBoost, playerId);
        bool hasDeathSkill = WardProgressionService.IsFeatureUnlocked(zdo, WardFeature.DeathSkillShield, playerId);
        bool hasFoodFreeze = WardProgressionService.IsFeatureUnlocked(zdo, WardFeature.FoodFreeze, playerId);
        if (_maxComfortRowObj != null) _maxComfortRowObj.SetActive(hasMaxComfort);
        if (_regenBoostRowObj != null) _regenBoostRowObj.SetActive(hasRegenBoost);
        if (_deathSkillRowObj != null) _deathSkillRowObj.SetActive(hasDeathSkill);
        if (_foodFreezeRowObj != null) _foodFreezeRowObj.SetActive(hasFoodFreeze);
        if (_cardSanctuaryObj != null) _cardSanctuaryObj.SetActive(!isDelegated && (hasMaxComfort || hasRegenBoost || hasDeathSkill || hasFoodFreeze));

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

        bool isSubWard = wardType == WardType.SubWard;
        if (_domeColorRowObj != null)
        {
            _domeColorRowObj.SetActive(isSubWard);
        }
        if (_colorWheelPanelObj != null)
        {
            _colorWheelPanelObj.SetActive(isSubWard && _isColorWheelExpanded);
        }

        if (isSubWard)
        {
            string hex = zdo.GetString(WardZdoKeys.DomeColor, null);
            Color initialColor = WardColorPalette.AetherIce;
            if (!string.IsNullOrEmpty(hex) && ColorUtility.TryParseHtmlString(hex, out Color parsed))
            {
                initialColor = parsed;
            }

            _currentColor = initialColor;
            UpdateColorDisplay(initialColor);
            if (_colorPicker != null)
            {
                _colorPicker.SetColor(initialColor);
                if (_brightnessSlider != null)
                {
                    _brightnessSlider.value = _colorPicker.CurrentBrightness;
                    if (_brightnessValTmp != null)
                    {
                        _brightnessValTmp.text = $"{Mathf.RoundToInt(_colorPicker.CurrentBrightness * 100f)}%";
                    }
                }
            }
        }
    }
    finally
    {
        _isRefreshing = false;
    }
}

    private void OnWheelColorChanged(Color newColor)
    {
        _currentColor = newColor;
        UpdateColorDisplay(newColor);
        SaveColorToZdo(newColor);
    }

    private void OnBrightnessChanged(float val)
    {
        if (_colorPicker != null)
        {
            _colorPicker.SetBrightness(val);
        }
        if (_brightnessValTmp != null)
        {
            _brightnessValTmp.text = $"{Mathf.RoundToInt(val * 100f)}%";
        }
    }

    private void ApplyNewColor(Color newColor)
    {
        _currentColor = newColor;
        if (_colorPicker != null)
        {
            _colorPicker.SetColor(newColor);
            if (_brightnessSlider != null)
            {
                _brightnessSlider.value = _colorPicker.CurrentBrightness;
                if (_brightnessValTmp != null)
                {
                    _brightnessValTmp.text = $"{Mathf.RoundToInt(_colorPicker.CurrentBrightness * 100f)}%";
                }
            }
        }
        UpdateColorDisplay(newColor);
        SaveColorToZdo(newColor);
    }

    private void UpdateColorDisplay(Color color)
    {
        if (_colorSwatchImg != null)
        {
            _colorSwatchImg.color = color;
        }
        if (_colorHexTmp != null)
        {
            _colorHexTmp.text = $"#{ColorUtility.ToHtmlStringRGB(color)}";
        }
    }

    private void SaveColorToZdo(Color color)
    {
        if (_isRefreshing)
            return;

        if (_ward == null || _ward.m_nview == null || !_ward.m_nview.IsValid())
            return;

        long playerId = Player.m_localPlayer != null ? Player.m_localPlayer.GetPlayerID() : 0L;
        ZDO zdo = _ward.m_nview.GetZDO();
        if (!WardRoleManager.CanPerform(zdo, playerId, "defenses.combat"))
            return;

        string hex = $"#{ColorUtility.ToHtmlStringRGB(color)}";
        zdo.Set(WardZdoKeys.DomeColor, hex);
        WardNetworkRouter.SendEntityName(zdo.m_uid, WardZdoKeys.DomeColor, hex);

        WardForcefieldDome dome = _ward.GetComponent<WardForcefieldDome>();
        if (dome != null)
        {
            dome.ForceUpdateVisuals();
        }
    }

    private static WardFeature? GetFeatureForKey(int key)
    {
        if (key == WardZdoKeys.PushbackForce) return WardFeature.PushbackForce;
        if (key == WardZdoKeys.ShockCreatureDmg) return WardFeature.CreatureShock;
        if (key == WardZdoKeys.ShockPlayerDmg) return WardFeature.PlayerShock;
        if (key == WardZdoKeys.GhostBase) return WardFeature.GhostBase;
        if (key == WardZdoKeys.RaidSuppress) return WardFeature.RaidSuppression;
        if (key == WardZdoKeys.SpawnSuppress) return WardFeature.SpawnSuppression;
        if (key == WardZdoKeys.WeatherRoof) return WardFeature.WeatherRoof;
        if (key == WardZdoKeys.AshShield) return WardFeature.AshShield;
        if (key == WardZdoKeys.LavaShield) return WardFeature.LavaShield;
        if (key == WardZdoKeys.SnowShield) return WardFeature.SnowShield;
        if (key == WardZdoKeys.WaterDrain) return WardFeature.WaterDrain;
        if (key == WardZdoKeys.MaxComfort) return WardFeature.MaxComfort;
        if (key == WardZdoKeys.RegenBoost) return WardFeature.RegenBoost;
        if (key == WardZdoKeys.DeathSkillShield) return WardFeature.DeathSkillShield;
        if (key == WardZdoKeys.FoodFreeze) return WardFeature.FoodFreeze;
        return null;
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
                WardFeature? feature = GetFeatureForKey(key);
                if (feature.HasValue && !WardProgressionService.IsFeatureUnlocked(zdo, feature.Value, playerId))
                {
                    if (Player.m_localPlayer != null)
                    {
                        Player.m_localPlayer.Message(MessageHud.MessageType.Center, "$wmb_feature_locked_by_rune".Localize());
                    }
                    Refresh();
                    return;
                }

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
                WardFeature? feature = GetFeatureForKey(key);
                if (feature.HasValue && !WardProgressionService.IsFeatureUnlocked(zdo, feature.Value, playerId))
                {
                    if (Player.m_localPlayer != null)
                    {
                        Player.m_localPlayer.Message(MessageHud.MessageType.Center, "$wmb_feature_locked_by_rune".Localize());
                    }
                    Refresh();
                    return;
                }

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

        float minRadius = 0f;
        WardType wardType = (WardType)zdo.GetInt(WardZdoKeys.WardType, (int)WardType.Main);
        if (wardType == WardType.SubWard)
        {
            float maxAllowed = WardQueryService.GetMaxAllowedSubWardRadius(zdo, _ward.transform.position);
            maxRadius = Mathf.Max(1f, Mathf.Floor(maxAllowed));
        }
        else
        {
            maxRadius = WardProgressionService.GetEffectiveSliderMaxRadius(zdo, playerId);
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
