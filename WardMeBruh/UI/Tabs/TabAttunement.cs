using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WardMeBruh.API;
using WardMeBruh.Components;
using WardMeBruh.Configuration;
using WardMeBruh.Items;
using WardMeBruh.Models;
using WardMeBruh.Networking;
using WardMeBruh.Services;
using WardMeBruh.UI.Elements;

namespace WardMeBruh.UI.Tabs;

public class TabAttunement
{
    private class RuneCardUi
    {
        public WardRune Rune;
        public GameObject CardRoot;
        public TextMeshProUGUI CardTitleTmp;
        public Action<string, Color> SetStatusBadge;
        public Action<string, Color> SetRadiusBadge;
        public GameObject FeaturesPanelObj;
        public TextMeshProUGUI ToggleButtonText;
        public bool IsFeaturesExpanded;
        public int FeatureCount;
        public GameObject ReqsRowObj;
        public GameObject BossRowObj;
        public Image BossIcon;
        public TextMeshProUGUI BossText;
        public GameObject KeystoneRowObj;
        public Image KeystoneIcon;
        public TextMeshProUGUI KeystoneText;
        public GameObject AttuneButtonObj;
        public Button AttuneButton;
        public TextMeshProUGUI AttuneButtonText;
        public GameObject ChannelBarObj;
        public Image ChannelBarFill;
        public RectTransform ChannelBarFillRt;
        public TextMeshProUGUI ChannelBarText;
    }

    private GameObject _root;
    private PrivateArea _ward;
    private GameObject _bannerCardObj;
    private Action<string, Color> _setBannerBadge;
    private readonly List<RuneCardUi> _runeCards = new();
    private bool _isRefreshing;
    private Coroutine _channelCoroutine;
    private WardRune? _channelingRune;
    private static ZDOID _lastCeremonyWard = ZDOID.None;
    private static float _lastCeremonyTime = -10f;

    public bool IsChanneling => _channelingRune.HasValue;

    public GameObject Root => _root;

    public void Build(Transform parent)
    {
        _root = new GameObject("Tab_Attunement", typeof(RectTransform));
        _root.transform.SetParent(parent, false);
        RectTransform rt = _root.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        WardUIFactory.CreateScrollArea(_root.transform, out Transform listContainer);

        Transform bannerCard = WardUIFactory.CreateCard(listContainer, "$wmb_attunement_card_title");
        _bannerCardObj = bannerCard.parent.parent.gameObject;
        WardUIFactory.CreateBadgeRow(bannerCard, "$wmb_status_label", "...", WardColorPalette.GoldAmber, out _setBannerBadge);
        _bannerCardObj.SetActive(false);

        for (int r = 0; r <= 7; r++)
        {
            WardRune rune = (WardRune)r;
            BuildRuneCard(listContainer, rune);
        }
    }

    private void BuildRuneCard(Transform container, WardRune rune)
    {
        string runeNameKey = WardProgressionService.GetRuneNameLocalizationKey(rune);
        string[] romanRanks = { "I", "II", "III", "IV", "V", "VI", "VII", "VIII" };
        int rankIndex = Mathf.Clamp((int)rune, 0, 7);
        string cardTitle = string.Format("$wmb_attunement_rank_title".Localize(), romanRanks[rankIndex], runeNameKey.Localize());

        Transform cardContent = WardUIFactory.CreateCard(container, cardTitle);
        GameObject cardRoot = cardContent.parent.parent.gameObject;

        RuneCardUi ui = new RuneCardUi
        {
            Rune = rune,
            CardRoot = cardRoot,
            CardTitleTmp = cardContent.parent.Find("CardHeader/HeaderText")?.GetComponent<TextMeshProUGUI>()
        };

        WardUIFactory.CreateBadgeRow(cardContent, "$wmb_attunement_status", "...", WardColorPalette.TextMuted, out ui.SetStatusBadge);
        WardUIFactory.CreateBadgeRow(cardContent, "$wmb_attunement_max_radius", "...", WardColorPalette.ValheimBeige, out ui.SetRadiusBadge);

        WardUIFactory.CreateSettingRow(cardContent, "$wmb_attunement_role_features", out Transform ctrlContainer);

        GameObject expandBtn = Jotunn.Managers.GUIManager.Instance.CreateButton(
            "...",
            ctrlContainer,
            Vector2.zero,
            Vector2.zero,
            Vector2.zero,
            120f,
            26f);
        expandBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(120f, 26f);
        expandBtn.SetLayoutDimensions(120f, 26f);
        ui.ToggleButtonText = WardUIFactory.SetupButtonText(expandBtn, "...", 13f, WardColorPalette.ValheimBeige);

        GameObject panelObj = new GameObject("FeaturesPanel", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter), typeof(LayoutElement));
        panelObj.transform.SetParent(cardContent, false);

        LayoutElement panelLe = panelObj.GetComponent<LayoutElement>();
        panelLe.flexibleWidth = 1f;

        Image panelBg = panelObj.GetComponent<Image>();
        panelBg.color = new Color(0.04f, 0.03f, 0.02f, 0.75f);

        VerticalLayoutGroup panelVlg = panelObj.GetComponent<VerticalLayoutGroup>();
        panelVlg.padding = new RectOffset(16, 16, 6, 8);
        panelVlg.spacing = 3f;
        panelVlg.childControlWidth = true;
        panelVlg.childControlHeight = true;
        panelVlg.childForceExpandWidth = true;
        panelVlg.childForceExpandHeight = false;

        ContentSizeFitter panelCsf = panelObj.GetComponent<ContentSizeFitter>();
        panelCsf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        panelObj.SetActive(false);
        ui.FeaturesPanelObj = panelObj;
        ui.IsFeaturesExpanded = false;

        expandBtn.GetComponent<Button>().onClick.AddListener(() =>
        {
            ui.IsFeaturesExpanded = !ui.IsFeaturesExpanded;
            ui.FeaturesPanelObj.SetActive(ui.IsFeaturesExpanded);
            if (ui.ToggleButtonText != null)
            {
                string toggleText = ui.IsFeaturesExpanded
                    ? "$wmb_attunement_hide_features".Localize()
                    : string.Format("$wmb_attunement_view_features".Localize(), ui.FeatureCount);
                ui.ToggleButtonText.text = toggleText;
            }
            if (WardMainWindow.Instance != null && _root != null)
            {
                WardMainWindow.ApplyFontSizeToHierarchy(_root, WardMainWindow.Instance.CurrentFontSizeMode);
            }
        });

        RebuildFeaturesList(ui);

        GameObject reqsRow = new GameObject("RequirementsRow", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
        reqsRow.transform.SetParent(cardContent, false);

        LayoutElement rowLe = reqsRow.GetComponent<LayoutElement>();
        rowLe.minHeight = 44f;
        rowLe.preferredHeight = 44f;
        rowLe.flexibleWidth = 1f;

        HorizontalLayoutGroup rowHlg = reqsRow.GetComponent<HorizontalLayoutGroup>();
        rowHlg.spacing = 32f;
        rowHlg.childControlWidth = false;
        rowHlg.childControlHeight = false;
        rowHlg.childForceExpandWidth = false;
        rowHlg.childForceExpandHeight = false;
        rowHlg.childAlignment = TextAnchor.MiddleCenter;

        GameObject bossBlock = new GameObject("BossReqBlock", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(ContentSizeFitter), typeof(LayoutElement));
        bossBlock.transform.SetParent(reqsRow.transform, false);

        ContentSizeFitter bossCsf = bossBlock.GetComponent<ContentSizeFitter>();
        bossCsf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        bossCsf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        LayoutElement bossBlockLe = bossBlock.GetComponent<LayoutElement>();
        bossBlockLe.minHeight = 40f;
        bossBlockLe.preferredHeight = 40f;

        HorizontalLayoutGroup bossHlg = bossBlock.GetComponent<HorizontalLayoutGroup>();
        bossHlg.spacing = 10f;
        bossHlg.childControlWidth = false;
        bossHlg.childControlHeight = false;
        bossHlg.childForceExpandWidth = false;
        bossHlg.childForceExpandHeight = false;
        bossHlg.childAlignment = TextAnchor.MiddleLeft;

        GameObject bossIconObj = new GameObject("BossIcon", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
        bossIconObj.transform.SetParent(bossBlock.transform, false);
        bossIconObj.GetComponent<RectTransform>().sizeDelta = new Vector2(40f, 40f);
        LayoutElement bossIconLe = bossIconObj.GetComponent<LayoutElement>();
        bossIconLe.minWidth = 40f;
        bossIconLe.preferredWidth = 40f;
        bossIconLe.minHeight = 40f;
        bossIconLe.preferredHeight = 40f;
        bossIconLe.flexibleWidth = 0f;
        bossIconLe.flexibleHeight = 0f;
        Image bossImg = bossIconObj.GetComponent<Image>();
        bossImg.preserveAspect = true;

        GameObject bossTextObj = new GameObject("BossText", typeof(RectTransform), typeof(TextMeshProUGUI), typeof(ContentSizeFitter), typeof(LayoutElement), typeof(WardTextElement));
        bossTextObj.transform.SetParent(bossBlock.transform, false);
        ContentSizeFitter bossTextCsf = bossTextObj.GetComponent<ContentSizeFitter>();
        bossTextCsf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        bossTextCsf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        LayoutElement bossTextLe = bossTextObj.GetComponent<LayoutElement>();
        bossTextLe.minHeight = 40f;
        bossTextLe.preferredHeight = 40f;
        TextMeshProUGUI bossText = bossTextObj.GetComponent<TextMeshProUGUI>();
        bossText.font = UIFonts.GetPrimaryFont();
        bossText.fontSize = 13f;
        bossText.alignment = TextAlignmentOptions.MidlineLeft;
        bossText.overflowMode = TextOverflowModes.Overflow;
        bossText.textWrappingMode = TextWrappingModes.NoWrap;
        bossTextObj.GetComponent<WardTextElement>().baseFontSize = 13;

        ui.BossRowObj = bossBlock;
        ui.BossIcon = bossImg;
        ui.BossText = bossText;

        GameObject stoneBlock = new GameObject("KeystoneReqBlock", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(ContentSizeFitter), typeof(LayoutElement));
        stoneBlock.transform.SetParent(reqsRow.transform, false);

        ContentSizeFitter stoneCsf = stoneBlock.GetComponent<ContentSizeFitter>();
        stoneCsf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        stoneCsf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        LayoutElement stoneBlockLe = stoneBlock.GetComponent<LayoutElement>();
        stoneBlockLe.minHeight = 40f;
        stoneBlockLe.preferredHeight = 40f;

        HorizontalLayoutGroup stoneHlg = stoneBlock.GetComponent<HorizontalLayoutGroup>();
        stoneHlg.spacing = 10f;
        stoneHlg.childControlWidth = false;
        stoneHlg.childControlHeight = false;
        stoneHlg.childForceExpandWidth = false;
        stoneHlg.childForceExpandHeight = false;
        stoneHlg.childAlignment = TextAnchor.MiddleLeft;

        GameObject stoneIconObj = new GameObject("KeystoneIcon", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
        stoneIconObj.transform.SetParent(stoneBlock.transform, false);
        stoneIconObj.GetComponent<RectTransform>().sizeDelta = new Vector2(40f, 40f);
        LayoutElement stoneIconLe = stoneIconObj.GetComponent<LayoutElement>();
        stoneIconLe.minWidth = 40f;
        stoneIconLe.preferredWidth = 40f;
        stoneIconLe.minHeight = 40f;
        stoneIconLe.preferredHeight = 40f;
        stoneIconLe.flexibleWidth = 0f;
        stoneIconLe.flexibleHeight = 0f;
        Image stoneImg = stoneIconObj.GetComponent<Image>();
        stoneImg.preserveAspect = true;

        GameObject stoneTextObj = new GameObject("KeystoneText", typeof(RectTransform), typeof(TextMeshProUGUI), typeof(ContentSizeFitter), typeof(LayoutElement), typeof(WardTextElement));
        stoneTextObj.transform.SetParent(stoneBlock.transform, false);
        ContentSizeFitter stoneTextCsf = stoneTextObj.GetComponent<ContentSizeFitter>();
        stoneTextCsf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        stoneTextCsf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        LayoutElement stoneTextLe = stoneTextObj.GetComponent<LayoutElement>();
        stoneTextLe.minHeight = 40f;
        stoneTextLe.preferredHeight = 40f;
        TextMeshProUGUI stoneText = stoneTextObj.GetComponent<TextMeshProUGUI>();
        stoneText.font = UIFonts.GetPrimaryFont();
        stoneText.fontSize = 13f;
        stoneText.alignment = TextAlignmentOptions.MidlineLeft;
        stoneText.overflowMode = TextOverflowModes.Overflow;
        stoneText.textWrappingMode = TextWrappingModes.NoWrap;
        stoneTextObj.GetComponent<WardTextElement>().baseFontSize = 13;

        ui.ReqsRowObj = reqsRow;
        ui.KeystoneRowObj = stoneBlock;
        ui.KeystoneIcon = stoneImg;
        ui.KeystoneText = stoneText;

        GameObject btnObj = Jotunn.Managers.GUIManager.Instance.CreateButton(
            "$wmb_btn_attune_keystone".Localize(),
            cardContent,
            Vector2.zero,
            Vector2.zero,
            Vector2.zero,
            200f,
            32f);
        ui.AttuneButtonObj = btnObj;
        ui.AttuneButton = btnObj.GetComponent<Button>();
        ui.AttuneButtonText = WardUIFactory.SetupButtonText(btnObj, "$wmb_btn_attune_keystone".Localize(), 13f, WardColorPalette.ValheimGreen);

        LayoutElement le = btnObj.AddComponent<LayoutElement>();
        le.minHeight = 32f;
        le.preferredHeight = 32f;
        le.flexibleWidth = 0f;

        ui.AttuneButton.onClick.AddListener(() => OnAttuneClicked(rune));

        GameObject channelBarObj = new GameObject("ChannelBar", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
        channelBarObj.transform.SetParent(cardContent, false);
        LayoutElement channelLe = channelBarObj.GetComponent<LayoutElement>();
        channelLe.minHeight = 32f;
        channelLe.preferredHeight = 32f;
        channelLe.flexibleWidth = 1f;

        Image channelBg = channelBarObj.GetComponent<Image>();
        channelBg.color = new Color(0.06f, 0.05f, 0.04f, 0.95f);

        GameObject fillObj = new GameObject("Fill", typeof(RectTransform), typeof(Image));
        fillObj.transform.SetParent(channelBarObj.transform, false);
        RectTransform fillRt = fillObj.GetComponent<RectTransform>();
        fillRt.anchorMin = new Vector2(0f, 0f);
        fillRt.anchorMax = new Vector2(0f, 1f);
        fillRt.pivot = new Vector2(0f, 0.5f);
        fillRt.offsetMin = Vector2.zero;
        fillRt.offsetMax = Vector2.zero;

        Image fillImg = fillObj.GetComponent<Image>();
        fillImg.color = WardColorPalette.GoldAmber;

        GameObject textObj = new GameObject("ChannelText", typeof(RectTransform), typeof(TextMeshProUGUI), typeof(WardTextElement));
        textObj.transform.SetParent(channelBarObj.transform, false);
        RectTransform textRt = textObj.GetComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = Vector2.zero;
        textRt.offsetMax = Vector2.zero;

        TextMeshProUGUI channelText = textObj.GetComponent<TextMeshProUGUI>();
        channelText.font = UIFonts.GetPrimaryFont();
        channelText.fontSize = 12f;
        channelText.alignment = TextAlignmentOptions.Center;
        channelText.color = WardColorPalette.ValheimBeige;
        textObj.GetComponent<WardTextElement>().baseFontSize = 12;

        channelBarObj.SetActive(false);

        ui.ChannelBarObj = channelBarObj;
        ui.ChannelBarFill = fillImg;
        ui.ChannelBarFillRt = fillRt;
        ui.ChannelBarText = channelText;

        _runeCards.Add(ui);
    }

    public static Color GetRuneColor(WardRune rune) => rune switch
    {
        WardRune.Fehu => new Color(1.00f, 0.15f, 0.15f, 1f),
        WardRune.Uruz => new Color(1.00f, 0.52f, 0.05f, 1f),
        WardRune.Thurisaz => new Color(1.00f, 0.85f, 0.10f, 1f),
        WardRune.Isa => new Color(0.20f, 0.95f, 0.25f, 1f),
        WardRune.Sowilo => new Color(0.10f, 0.85f, 1.00f, 1f),
        WardRune.Laguz => new Color(0.25f, 0.48f, 1.00f, 1f),
        WardRune.Dagaz => new Color(0.68f, 0.25f, 1.00f, 1f),
        WardRune.Hagalaz => new Color(1.00f, 0.22f, 0.75f, 1f),
        _ => Color.white
    };

    private static List<string> GetRuneFeatureNames(WardRune rune)
    {
        List<string> result = new();
        HashSet<string> seen = new(StringComparer.OrdinalIgnoreCase);

        if (ConfigRegistry.RuneConfigurations.TryGetValue(rune, out RuneConfigDefinition def))
        {
            foreach (WardFeature feature in def.ParsedFeatures)
            {
                string name = GetFeatureDisplayName(feature);
                if (!string.IsNullOrEmpty(name) && seen.Add(name))
                {
                    result.Add(name);
                }
            }

            foreach (string extId in def.ParsedExternalFeatureIds)
            {
                if (!string.IsNullOrEmpty(extId) && seen.Add(extId))
                {
                    result.Add(extId);
                }
            }
        }

        return result;
    }

    private static void RebuildFeaturesList(RuneCardUi ui)
    {
        if (ui == null || ui.FeaturesPanelObj == null)
            return;

        List<string> features = GetRuneFeatureNames(ui.Rune);
        ui.FeatureCount = features.Count;

        Transform panelTransform = ui.FeaturesPanelObj.transform;
        for (int i = panelTransform.childCount - 1; i >= 0; i--)
        {
            Transform child = panelTransform.GetChild(i);
            if (child != null && child.gameObject != null)
            {
                UnityEngine.Object.Destroy(child.gameObject);
            }
        }

        string amberHex = ColorUtility.ToHtmlStringRGB(WardColorPalette.GoldAmber);
        for (int f = 0; f < features.Count; f++)
        {
            string featName = features[f];
            GameObject featRowObj = new GameObject($"Feature_{f}", typeof(RectTransform), typeof(TextMeshProUGUI), typeof(LayoutElement), typeof(WardTextElement));
            featRowObj.transform.SetParent(panelTransform, false);

            LayoutElement featLe = featRowObj.GetComponent<LayoutElement>();
            featLe.minHeight = 22f;
            featLe.preferredHeight = 24f;
            featLe.flexibleWidth = 1f;

            TextMeshProUGUI featText = featRowObj.GetComponent<TextMeshProUGUI>();
            featText.font = UIFonts.GetPrimaryFont();
            featText.fontSize = 13f;
            featText.color = WardColorPalette.ValheimBeige;
            featText.alignment = TextAlignmentOptions.MidlineLeft;
            featText.overflowMode = TextOverflowModes.Overflow;
            featText.textWrappingMode = TextWrappingModes.NoWrap;
            featText.text = $"<color=#{amberHex}>•</color>  {featName}";

            WardTextElement featWte = featRowObj.GetComponent<WardTextElement>();
            featWte.baseFontSize = 13;
        }

        if (ui.ToggleButtonText != null)
        {
            ui.ToggleButtonText.text = ui.IsFeaturesExpanded
                ? "$wmb_attunement_hide_features".Localize()
                : string.Format("$wmb_attunement_view_features".Localize(), ui.FeatureCount);
        }
    }

    private static string GetFeatureDisplayName(WardFeature feature) => feature switch
    {
        WardFeature.ProtectCrops => "$wmb_feature_protect_crops".Localize(),
        WardFeature.PickableHarvestProtection => "$wmb_perm_core_harvest_name".Localize(),
        WardFeature.ProtectTames => "$wmb_perm_defenses_tames_name".Localize(),
        WardFeature.PortalAccess => "$wmb_perm_access_portals_name".Localize(),
        WardFeature.PushbackForce => "$wmb_defenses_pushback".Localize(),
        WardFeature.CreatureShock => "$wmb_defenses_shock_creatures".Localize(),
        WardFeature.PlayerShock => "$wmb_defenses_shock_players".Localize(),
        WardFeature.GhostBase => "$wmb_perm_defenses_camouflage_name".Localize(),
        WardFeature.WaterDrain => "$wmb_perm_upkeep_waterdrain_name".Localize(),
        WardFeature.AreaRepair => "$wmb_perm_upkeep_repair_name".Localize(),
        WardFeature.FoodFreeze => "$wmb_defenses_food_freeze".Localize(),
        WardFeature.SnowShield => "$wmb_feature_snow_shield".Localize(),
        WardFeature.PassiveBubble => "$wmb_feature_passive_bubble".Localize(),
        WardFeature.WeatherRoof => "$wmb_feature_weather_roof".Localize(),
        WardFeature.AutoFuel => "$wmb_perm_upkeep_autofuel_name".Localize(),
        WardFeature.AutoCloseDoors => "$wmb_perm_upkeep_autodoors_name".Localize(),
        WardFeature.RegenBoost or WardFeature.StaminaRegenBoost => "$wmb_defenses_regen_boost".Localize(),
        WardFeature.WardExtensions => "$wmb_perm_core_extend_ward_name".Localize(),
        WardFeature.RaidSuppression => "$wmb_defenses_raid_suppression".Localize(),
        WardFeature.SpawnSuppression => "$wmb_defenses_spawn_suppression".Localize(),
        WardFeature.MaxComfort => "$wmb_defenses_max_comfort".Localize(),
        WardFeature.EitrRegenBoost => "$wmb_feature_eitr_regen".Localize(),
        WardFeature.AshShield => "$wmb_feature_ash_shield".Localize(),
        WardFeature.LavaShield => "$wmb_feature_lava_shield".Localize(),
        WardFeature.DeathSkillShield => "$wmb_defenses_death_skill".Localize(),
        WardFeature.ArcticHearth => "$wmb_feature_deep_north_shield".Localize(),
        WardFeature.PackIceShield => "$wmb_feature_pack_ice_shield".Localize(),
        WardFeature.VehicleLocking => "$wmb_feature_vehicle_locking".Localize(),
        WardFeature.FastTravel => "$wmb_feature_fast_travel".Localize(),
        WardFeature.ProtectTerrain => "$wmb_feature_protect_terrain".Localize(),
        _ => feature.ToString()
    };

    public void Bind(PrivateArea ward)
    {
        if (_ward != ward)
        {
            CancelChannel();
        }
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
            WardType wardType = (WardType)zdo.GetInt(WardZdoKeys.WardType, (int)WardType.Main);
            ZDO effectiveZdo = WardQueryService.GetEffectiveWardZdo(zdo);

            bool isExtension = wardType == WardType.Extension;
            if (_bannerCardObj != null)
            {
                if (!ConfigRegistry.EnableProgressionSystem.Value)
                {
                    _bannerCardObj.SetActive(true);
                    _setBannerBadge?.Invoke("$wmb_progression_disabled".Localize(), WardColorPalette.TextMuted);
                }
                else if (isExtension)
                {
                    _bannerCardObj.SetActive(true);
                    string parentName = "$wmb_default_ward_name".Localize();
                    if (effectiveZdo != null)
                    {
                        parentName = effectiveZdo.GetString(WardZdoKeys.CustomName, parentName);
                    }
                    string bannerMsg = string.Format("$wmb_attunement_managed_by".Localize(), parentName);
                    _setBannerBadge?.Invoke(bannerMsg, WardColorPalette.GoldAmber);
                }
                else
                {
                    _bannerCardObj.SetActive(false);
                }
            }

            bool canManage = WardRoleManager.CanPerform(zdo, playerId, WardPermissionIds.CoreAttuneWard) || WardPermissions.IsAdmin(playerId);

            for (int i = 0; i < _runeCards.Count; i++)
            {
                RuneCardUi ui = _runeCards[i];
                WardRune rune = ui.Rune;

                if (!ConfigRegistry.RuneConfigurations.TryGetValue(rune, out RuneConfigDefinition def))
                    continue;

                float maxRadius = def.MaxRadiusConfig.Value;
                ui.SetRadiusBadge?.Invoke($"{maxRadius:F0}m", WardColorPalette.ValheimBeige);

                string runeNameKey = WardProgressionService.GetRuneNameLocalizationKey(rune);
                string[] romanRanks = { "I", "II", "III", "IV", "V", "VI", "VII", "VIII" };
                int rankIndex = Mathf.Clamp((int)rune, 0, 7);
                if (ui.CardTitleTmp != null)
                {
                    ui.CardTitleTmp.text = string.Format("$wmb_attunement_rank_title".Localize(), romanRanks[rankIndex], runeNameKey.Localize()).ToUpperInvariant();
                }

                RebuildFeaturesList(ui);

                bool isAttuned = WardProgressionService.IsRuneAttuned(effectiveZdo, rune);
                if (isAttuned)
                {
                    ui.SetStatusBadge?.Invoke("$wmb_status_attuned".Localize(), WardColorPalette.ValheimGreen);
                    if (ui.ReqsRowObj != null)
                    {
                        ui.ReqsRowObj.SetActive(false);
                    }
                    if (ui.AttuneButtonObj != null)
                    {
                        ui.AttuneButtonObj.SetActive(false);
                    }
                    if (ui.ChannelBarObj != null)
                    {
                        ui.ChannelBarObj.SetActive(false);
                    }
                }
                else
                {
                    bool canAttune = WardProgressionService.CanAttuneRune(effectiveZdo, rune, Player.m_localPlayer, out string reason);
                    Color statusColor = canAttune ? WardColorPalette.GoldAmber : WardColorPalette.TextMuted;
                    string statusText = canAttune ? "$wmb_status_ready_to_attune".Localize() : "$wmb_status_locked".Localize();
                    ui.SetStatusBadge?.Invoke(statusText, statusColor);

                    ProgressionMode mode = ConfigRegistry.ActiveProgressionMode.Value;
                    bool reqBoss = (mode == ProgressionMode.WorldBossKeys || mode == ProgressionMode.Hybrid);
                    bool reqStone = (mode == ProgressionMode.RunicKeystones || mode == ProgressionMode.Hybrid);

                    string greenHex = ColorUtility.ToHtmlStringRGB(WardColorPalette.ValheimGreen);
                    string redHex = ColorUtility.ToHtmlStringRGB(WardColorPalette.DangerRed);
                    string amberHex = ColorUtility.ToHtmlStringRGB(WardColorPalette.GoldAmber);

                    bool hasPrev = true;
                    string reqRankText = null;
                    if ((int)rune > 0)
                    {
                        WardRune prevRune = (WardRune)((int)rune - 1);
                        hasPrev = WardProgressionService.IsRuneAttuned(effectiveZdo, prevRune);
                        if (!hasPrev)
                        {
                            string prevRank = romanRanks[(int)prevRune];
                            string prevRuneName = WardProgressionService.GetRuneNameLocalizationKey(prevRune).Localize();
                            reqRankText = string.Format("$wmb_attunement_requires_rank".Localize(), prevRank, prevRuneName);
                        }
                    }

                    if (ui.ReqsRowObj != null)
                    {
                        ui.ReqsRowObj.SetActive(reqBoss || reqStone);

                        if (ui.BossRowObj != null)
                        {
                            if (reqBoss)
                            {
                                ui.BossRowObj.SetActive(true);
                                bool bossDefeated = WardProgressionService.IsBossDefeated(def.BossKeyConfig.Value, Player.m_localPlayer);
                                string bossName = WardProgressionService.GetBossDisplayName(rune);

                                Sprite bossSprite = WardProgressionService.GetBossTrophyIcon(rune);
                                if (bossSprite != null)
                                {
                                    ui.BossIcon.sprite = bossSprite;
                                    ui.BossIcon.enabled = true;
                                }
                                else
                                {
                                    ui.BossIcon.enabled = false;
                                }

                                if (bossDefeated)
                                {
                                    ui.BossIcon.color = Color.white;
                                    string defStr = string.Format("$wmb_boss_defeated".Localize(), bossName);
                                    ui.BossText.text = $"<color=#{greenHex}>✓ {defStr}</color>";
                                }
                                else
                                {
                                    ui.BossIcon.color = new Color(0.35f, 0.35f, 0.35f, 0.45f);
                                    string defReqStr = string.Format("$wmb_boss_defeat_req".Localize(), bossName);
                                    ui.BossText.text = $"<color=#{redHex}>✖ {defReqStr}</color>";
                                }
                            }
                            else
                            {
                                ui.BossRowObj.SetActive(false);
                            }
                        }

                        if (ui.KeystoneRowObj != null)
                        {
                            if (reqStone)
                            {
                                ui.KeystoneRowObj.SetActive(true);
                                bool hasStone = WardProgressionService.HasKeystoneItem(Player.m_localPlayer, rune);

                                Sprite icon = KeystoneItemFactory.GetKeystoneIcon(rune);
                                if (icon != null)
                                {
                                    ui.KeystoneIcon.sprite = icon;
                                    ui.KeystoneIcon.enabled = true;
                                }
                                else
                                {
                                    ui.KeystoneIcon.enabled = false;
                                }

                                if (hasStone)
                                {
                                    ui.KeystoneIcon.color = GetRuneColor(rune);
                                    ui.KeystoneText.text = $"<color=#{greenHex}>✓ {"$wmb_keystone_detected".Localize()}</color>";
                                }
                                else
                                {
                                    ui.KeystoneIcon.color = new Color(0.35f, 0.35f, 0.35f, 0.45f);
                                    ui.KeystoneText.text = $"<color=#{amberHex}>✖ {"$wmb_keystone_not_detected".Localize()}</color>";
                                }
                            }
                            else
                            {
                                ui.KeystoneRowObj.SetActive(false);
                            }
                        }
                    }

                    if (_channelingRune.HasValue && _channelingRune.Value == rune)
                    {
                        if (ui.AttuneButtonObj != null) ui.AttuneButtonObj.SetActive(false);
                        if (ui.ChannelBarObj != null) ui.ChannelBarObj.SetActive(true);
                    }
                    else
                    {
                        if (ui.ChannelBarObj != null) ui.ChannelBarObj.SetActive(false);
                        if (ui.AttuneButtonObj != null)
                        {
                            bool showButton = !isExtension && ConfigRegistry.EnableProgressionSystem.Value;
                            ui.AttuneButtonObj.SetActive(showButton);
                            if (ui.AttuneButton != null)
                            {
                                ui.AttuneButton.interactable = canAttune && canManage && !_channelingRune.HasValue;
                            }
                            if (ui.AttuneButtonText != null)
                            {
                                if (!hasPrev && reqRankText != null)
                                {
                                    ui.AttuneButtonText.text = reqRankText.ToUpperInvariant();
                                    ui.AttuneButtonText.color = WardColorPalette.DangerRed;
                                }
                                else
                                {
                                    ui.AttuneButtonText.text = "$wmb_btn_attune_keystone".Localize();
                                    ui.AttuneButtonText.color = (canAttune && canManage && !_channelingRune.HasValue) ? WardColorPalette.ValheimGreen : WardColorPalette.TextMuted;
                                }
                            }
                        }
                    }
                }
            }

            if (WardMainWindow.Instance != null && _root != null)
            {
                WardMainWindow.ApplyFontSizeToHierarchy(_root, WardMainWindow.Instance.CurrentFontSizeMode);
            }
        }
        finally
        {
            _isRefreshing = false;
        }
    }

    public void CancelChannel()
    {
        if (_channelCoroutine != null && WardMainWindow.Instance != null)
        {
            WardMainWindow.Instance.StopCoroutine(_channelCoroutine);
            _channelCoroutine = null;
        }

        if (_channelingRune.HasValue)
        {
            _channelingRune = null;
            Refresh();
        }
    }

    private void OnAttuneClicked(WardRune rune)
    {
        if (_isRefreshing || _channelingRune.HasValue)
            return;

        if (_ward == null || _ward.m_nview == null || !_ward.m_nview.IsValid())
            return;

        ZDO zdo = _ward.m_nview.GetZDO();
        ZDO effectiveZdo = WardQueryService.GetEffectiveWardZdo(zdo);
        if (effectiveZdo == null || !effectiveZdo.IsValid())
            return;

        long playerId = Player.m_localPlayer != null ? Player.m_localPlayer.GetPlayerID() : 0L;
        if (!WardRoleManager.CanPerform(effectiveZdo, playerId, WardPermissionIds.CoreAttuneWard) && !WardPermissions.IsAdmin(playerId))
        {
            if (Player.m_localPlayer != null)
            {
                Player.m_localPlayer.Message(MessageHud.MessageType.Center, "$wmb_missing_attune_permission".Localize());
            }
            return;
        }

        if (!WardProgressionService.CanAttuneRune(effectiveZdo, rune, Player.m_localPlayer, out string reason))
        {
            if (Player.m_localPlayer != null)
            {
                Player.m_localPlayer.Message(MessageHud.MessageType.Center, reason.Localize());
            }
            return;
        }

        if (WardMainWindow.Instance != null)
        {
            _channelingRune = rune;
            _channelCoroutine = WardMainWindow.Instance.StartCoroutine(AttuneChannelRoutine(effectiveZdo, rune));
            Refresh();
        }
    }

    private IEnumerator AttuneChannelRoutine(ZDO effectiveZdo, WardRune rune)
    {
        RuneCardUi card = null;
        for (int i = 0; i < _runeCards.Count; i++)
        {
            if (_runeCards[i].Rune == rune)
            {
                card = _runeCards[i];
                break;
            }
        }

        if (card != null)
        {
            if (card.AttuneButtonObj != null) card.AttuneButtonObj.SetActive(false);
            if (card.ChannelBarObj != null) card.ChannelBarObj.SetActive(true);
            if (card.ChannelBarFill != null) card.ChannelBarFill.color = GetRuneColor(rune);
            if (card.ChannelBarFillRt != null)
            {
                card.ChannelBarFillRt.anchorMax = new Vector2(0f, 1f);
                card.ChannelBarFillRt.offsetMin = Vector2.zero;
                card.ChannelBarFillRt.offsetMax = Vector2.zero;
            }
        }

        Vector3 wardPos = effectiveZdo.GetPosition();
        SpawnPrefab("sfx_staffthunderblood_cast", wardPos);

        const float duration = 5.0f;
        float elapsed = 0f;

        string runeNameKey = WardProgressionService.GetRuneNameLocalizationKey(rune);
        string runeName = runeNameKey.Localize();
        string attuningPrefix = "$wmb_attuning_keystone".Localize();

        while (elapsed < duration)
        {
            if (WardMainWindow.Instance == null || !WardMainWindow.Instance.IsOpen || _ward == null)
            {
                _channelingRune = null;
                _channelCoroutine = null;
                yield break;
            }

            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / duration);
            float remaining = Mathf.Max(0f, duration - elapsed);

            if (card != null && card.ChannelBarFillRt != null)
            {
                card.ChannelBarFillRt.anchorMax = new Vector2(progress, 1f);
                card.ChannelBarFillRt.offsetMin = Vector2.zero;
                card.ChannelBarFillRt.offsetMax = Vector2.zero;
            }

            if (card != null && card.ChannelBarText != null)
            {
                card.ChannelBarText.text = $"{attuningPrefix} {runeName}... {remaining:F1}s";
            }

            yield return null;
        }

        _channelingRune = null;
        _channelCoroutine = null;

        WardProgressionService.AttuneRune(effectiveZdo, rune, Player.m_localPlayer);
        WardNetworkRouter.SendAttuneKeystone(effectiveZdo.m_uid, rune);

        if (WardMainWindow.Instance != null)
        {
            WardMainWindow.Instance.Close();
        }

        TriggerAttunementCeremony(effectiveZdo, rune);

        if (Player.m_localPlayer != null)
        {
            string msg = string.Format("$wmb_attunement_success".Localize(), runeName);
            Player.m_localPlayer.Message(MessageHud.MessageType.Center, msg);
        }
    }

    public static void TriggerAttunementCeremony(ZDO wardZdo, WardRune rune)
    {
        if (wardZdo == null || !wardZdo.IsValid())
            return;

        if (_lastCeremonyWard == wardZdo.m_uid && Time.time - _lastCeremonyTime < 3f)
            return;

        _lastCeremonyWard = wardZdo.m_uid;
        _lastCeremonyTime = Time.time;

        Vector3 wardPos = wardZdo.GetPosition();
        Vector3 strikePos = wardPos + Vector3.up * 1.5f;

        SpawnPrefab("lightningAOE", strikePos);
        SpawnPrefab("sfx_staffthunderblood_thunder", wardPos);

        int attunedMask = wardZdo.GetInt(WardZdoKeys.AttunedRunes, 0);
        HashSet<ZDOID> processedExtensions = new();

        if (WardRegistryManager.Instance != null)
        {
            List<KnownExtensionWard> extensions = WardRegistryManager.Instance.GetExtensionsForParent(wardZdo.m_uid);
            for (int i = 0; i < extensions.Count; i++)
            {
                KnownExtensionWard ext = extensions[i];
                processedExtensions.Add(ext.Id);
                Vector3 extPos = ext.Position;
                Vector3 extStrikePos = extPos + Vector3.up * 1.5f;

                SpawnPrefab("lightningAOE", extStrikePos);
                SpawnPrefab("sfx_staffthunderblood_thunder", extPos);
            }
        }

        if (PrivateArea.m_allAreas != null)
        {
            for (int i = 0; i < PrivateArea.m_allAreas.Count; i++)
            {
                PrivateArea area = PrivateArea.m_allAreas[i];
                if (area == null || area.m_nview == null || !area.m_nview.IsValid())
                    continue;

                ZDO aZdo = area.m_nview.GetZDO();
                if (aZdo == wardZdo)
                {
                    WardAttunementStones stones = area.GetComponent<WardAttunementStones>();
                    if (stones != null)
                    {
                        stones.UpdateStones(attunedMask);
                    }
                    continue;
                }

                WardType aType = (WardType)aZdo.GetInt(WardZdoKeys.WardType, (int)WardType.Main);
                ZDOID aParent = aZdo.GetZDOID(WardZdoKeys.ParentWard);
                if (aType == WardType.Extension && aParent == wardZdo.m_uid)
                {
                    if (processedExtensions.Add(aZdo.m_uid))
                    {
                        Vector3 extPos = aZdo.GetPosition();
                        Vector3 extStrikePos = extPos + Vector3.up * 1.5f;
                        SpawnPrefab("lightningAOE", extStrikePos);
                        SpawnPrefab("sfx_staffthunderblood_thunder", extPos);
                    }

                    WardAttunementStones extStones = area.GetComponent<WardAttunementStones>();
                    if (extStones != null)
                    {
                        extStones.UpdateStones(attunedMask);
                    }
                }
            }
        }
    }

    private static void SpawnPrefab(string prefabName, Vector3 position)
    {
        if (string.IsNullOrEmpty(prefabName))
            return;

        try
        {
            GameObject prefab = ZNetScene.instance != null ? ZNetScene.instance.GetPrefab(prefabName) : null;
            if (prefab == null && Jotunn.Managers.PrefabManager.Instance != null)
            {
                prefab = Jotunn.Managers.PrefabManager.Instance.GetPrefab(prefabName);
            }

            if (prefab != null)
            {
                UnityEngine.Object.Instantiate(prefab, position, Quaternion.identity);
            }
        }
        catch (Exception ex)
        {
            WardMeBruh.Log.Warning($"Failed to spawn effect prefab '{prefabName}': {ex.Message}");
        }
    }
}
