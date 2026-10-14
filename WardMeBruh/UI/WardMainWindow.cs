using Jotunn.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using WardMeBruh.Configuration;
using WardMeBruh.Models;
using WardMeBruh.Patches;
using WardMeBruh.Services;
using WardMeBruh.UI.Tabs;

namespace WardMeBruh.UI;

public class WardMainWindow : MonoBehaviour
{
    private static WardMainWindow _instance;
    private GameObject _panel;
    private TextMeshProUGUI _titleText;
    private TextMeshProUGUI _fontSizeBtnText;
    private WardFontSizeMode _currentFontSizeMode = WardFontSizeMode.Small;
    private int _openedFrame = -1;

    private TabOverview _tabOverview;
    private TabAttunement _tabAttunement;
    private TabDefenses _tabDefenses;
    private TabUpkeep _tabUpkeep;
    private TabAccess _tabAccess;
    private TabRoles _tabRoles;
    private TabManagement _tabManagement;
    private bool _isInputBlocked;

    private RemoteWardSelectorModal _remoteSelector;
    private PrivateArea _activeWard;
    private PrivateArea _physicalWard;
    private int _currentTabIndex;
    private readonly Button[] _tabButtons = new Button[6];
    private GameObject _tabBar;
    private RectTransform _contentRt;
    private GameObject _footerBar;
    private TextMeshProUGUI _footerStatusText;
    private Button _footerAttunementBtn;
    private TextMeshProUGUI _footerAttunementBtnText;
    private bool _isAttunementViewOpen;

    public static WardMainWindow Instance
    {
        get
        {
            if (_instance == null && !GUIManager.IsHeadless() && GUIManager.CustomGUIFront != null)
            {
                GameObject root = new GameObject("WardMainWindow_Host");
                root.transform.SetParent(GUIManager.CustomGUIFront.transform, false);
                _instance = root.AddComponent<WardMainWindow>();
            }
            return _instance;
        }
    }

    public bool IsOpen => _panel != null && _panel.activeSelf;
    public static int LastClosedFrame { get; private set; } = -1;
    public WardFontSizeMode CurrentFontSizeMode => _currentFontSizeMode;
    public ZDOID ActiveWardId => (_activeWard != null && _activeWard.m_nview != null && _activeWard.m_nview.IsValid()) ? _activeWard.m_nview.GetZDO().m_uid : ZDOID.None;
    public ZDOID PhysicalWardId => (_physicalWard != null && _physicalWard.m_nview != null && _physicalWard.m_nview.IsValid()) ? _physicalWard.m_nview.GetZDO().m_uid : ZDOID.None;

    private void Awake()
    {
        Patch_PrivateArea.OnOpenWardGuiRequested += Open;
        if (ConfigRegistry.EnableProgressionSystem != null)
        {
            ConfigRegistry.EnableProgressionSystem.SettingChanged += OnProgressionSystemToggled;
        }
        BuildWindow();
    }

    private void OnDestroy()
    {
        Patch_PrivateArea.OnOpenWardGuiRequested -= Open;
        if (ConfigRegistry.EnableProgressionSystem != null)
        {
            ConfigRegistry.EnableProgressionSystem.SettingChanged -= OnProgressionSystemToggled;
        }
        SetInputBlocked(false);
    }

    private void OnProgressionSystemToggled(object sender, System.EventArgs e)
    {
        UpdateFooterVisibility();
        if (IsOpen)
        {
            RefreshCurrentTab();
        }
    }

    private void SetInputBlocked(bool block)
    {
        if (block)
        {
            if (!_isInputBlocked)
            {
                _isInputBlocked = true;
                GUIManager.BlockInput(true);
            }
        }
        else
        {
            _isInputBlocked = false;
            WardInputHelper.ForceUnblockInput();
        }
    }

    public bool HandleEscapeKey()
    {
        if (!IsOpen)
        {
            return false;
        }

        if (_remoteSelector != null && _remoteSelector.IsOpen)
        {
            _remoteSelector.Close();
            return true;
        }

        Close();
        return true;
    }

    private void Update()
    {
        if (IsOpen)
        {
            if (TextInput.IsVisible())
            {
                return;
            }

            bool closeRequested = Input.GetKeyDown(KeyCode.Escape) ||
                ZInput.GetKeyDown(KeyCode.Escape) ||
                (UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.escapeKey.wasPressedThisFrame) ||
                (ZInput.instance != null && (ZInput.GetButtonDown("JoyMenu") || ZInput.GetButtonDown("JoyButtonB")));

            bool isInputFieldFocused = EventSystem.current != null && 
                EventSystem.current.currentSelectedGameObject != null && 
                (EventSystem.current.currentSelectedGameObject.GetComponent<InputField>() != null ||
                 EventSystem.current.currentSelectedGameObject.GetComponent<TMP_InputField>() != null);

            if (!closeRequested && !isInputFieldFocused && Time.frameCount != _openedFrame && ConfigRegistry.OpenWardHubKey != null && ConfigRegistry.OpenWardHubKey.Value.IsDown())
            {
                closeRequested = true;
            }

            if (closeRequested)
            {
                HandleEscapeKey();
            }
        }
    }

    private void BuildWindow()
    {
        UIFonts.GetPrimaryFont();

        _panel = GUIManager.Instance.CreateWoodpanel(
            transform, 
            new Vector2(0.5f, 0.5f), 
            new Vector2(0.5f, 0.5f), 
            Vector2.zero, 
            880f, 
            640f, 
            draggable: true);
        _panel.name = "WardMainWindow";

        GameObject titleObj = new GameObject("TitleText", typeof(RectTransform), typeof(TextMeshProUGUI), typeof(WardTextElement));
        titleObj.transform.SetParent(_panel.transform, false);
        RectTransform titleRt = titleObj.GetComponent<RectTransform>();
        titleRt.anchorMin = new Vector2(0f, 1f);
        titleRt.anchorMax = new Vector2(0f, 1f);
        titleRt.pivot = new Vector2(0f, 1f);
        titleRt.anchoredPosition = new Vector2(30f, -20f);
        titleRt.sizeDelta = new Vector2(520f, 32f);

        _titleText = titleObj.GetComponent<TextMeshProUGUI>();
        _titleText.text = "$wmb_window_title".Localize();
        _titleText.font = UIFonts.GetPrimaryFont();
        _titleText.fontSize = 20;
        _titleText.color = WardColorPalette.ValheimOrange;
        _titleText.alignment = TextAlignmentOptions.MidlineLeft;

        WardTextElement titleWte = titleObj.GetComponent<WardTextElement>();
        titleWte.baseFontSize = 20;

        GameObject fontBtn = GUIManager.Instance.CreateButton(
            "$wmb_font_small".Localize(), 
            _panel.transform, 
            new Vector2(1f, 1f), 
            new Vector2(1f, 1f), 
            new Vector2(-316f, -36f), 
            125f, 
            34f);
        fontBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(125f, 34f);
        _fontSizeBtnText = WardUIFactory.SetupButtonText(fontBtn, "$wmb_font_small".Localize(), 13f);
        fontBtn.GetComponent<Button>().onClick.AddListener(OnCycleFontSizeClicked);

        GameObject remoteBtn = GUIManager.Instance.CreateButton(
            "$wmb_other_wards".Localize(), 
            _panel.transform, 
            new Vector2(1f, 1f), 
            new Vector2(1f, 1f), 
            new Vector2(-156f, -36f), 
            175f, 
            34f);
        remoteBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(175f, 34f);
        WardUIFactory.SetupButtonText(remoteBtn, "$wmb_other_wards".Localize(), 13f);
        remoteBtn.GetComponent<Button>().onClick.AddListener(OnOtherWardsClicked);

        GameObject closeBtn = GUIManager.Instance.CreateButton(
            "X", 
            _panel.transform, 
            new Vector2(1f, 1f), 
            new Vector2(1f, 1f), 
            new Vector2(-36f, -36f), 
            36f, 
            34f);
        closeBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(36f, 34f);
        WardUIFactory.SetupButtonText(closeBtn, "X", 14f, WardColorPalette.DangerRed);
        closeBtn.GetComponent<Button>().onClick.AddListener(Close);

        _tabBar = new GameObject("TabBar");
        _tabBar.transform.SetParent(_panel.transform, false);
        RectTransform tabRt = _tabBar.AddComponent<RectTransform>();
        tabRt.anchorMin = new Vector2(0f, 1f);
        tabRt.anchorMax = new Vector2(1f, 1f);
        tabRt.pivot = new Vector2(0.5f, 1f);
        tabRt.offsetMin = new Vector2(24f, -96f);
        tabRt.offsetMax = new Vector2(-24f, -60f);

        HorizontalLayoutGroup hlg = _tabBar.AddComponent<HorizontalLayoutGroup>();
        hlg.childControlWidth = true;
        hlg.childControlHeight = true;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = true;
        hlg.spacing = 8f;

        string[] tabTokens = { "$wmb_tab_overview", "$wmb_tab_defenses", "$wmb_tab_upkeep", "$wmb_tab_access", "$wmb_tab_roster", "$wmb_tab_management" };
        for (int i = 0; i < tabTokens.Length; i++)
        {
            int index = i;
            GameObject btnObj = GUIManager.Instance.CreateButton(tabTokens[i].Localize(), _tabBar.transform, Vector2.zero, Vector2.zero, Vector2.zero, 130f, 36f);
            WardUIFactory.SetupButtonText(btnObj, tabTokens[i].Localize(), 13f, null, TextAlignmentOptions.Center);

            LayoutElement le = btnObj.AddComponent<LayoutElement>();
            le.minHeight = 36f;
            le.preferredHeight = 36f;
            le.flexibleWidth = 1f;

            Button btn = btnObj.GetComponent<Button>();
            btn.onClick.AddListener(() => SwitchTab(index));

            _tabButtons[i] = btn;
        }

        UpdateTabButtonDimensions();

        GameObject contentArea = new GameObject("ContentArea");
        contentArea.transform.SetParent(_panel.transform, false);
        _contentRt = contentArea.AddComponent<RectTransform>();
        _contentRt.anchorMin = Vector2.zero;
        _contentRt.anchorMax = Vector2.one;
        _contentRt.offsetMin = new Vector2(30f, 56f);
        _contentRt.offsetMax = new Vector2(-30f, -106f);

        _tabOverview = new TabOverview();
        _tabOverview.Build(contentArea.transform);

        _tabDefenses = new TabDefenses();
        _tabDefenses.Build(contentArea.transform);

        _tabUpkeep = new TabUpkeep();
        _tabUpkeep.Build(contentArea.transform);

        _tabAccess = new TabAccess();
        _tabAccess.Build(contentArea.transform);

        _tabRoles = new TabRoles();
        _tabRoles.Build(contentArea.transform);

        _tabManagement = new TabManagement();
        _tabManagement.Build(contentArea.transform);

        _tabAttunement = new TabAttunement();
        _tabAttunement.Build(contentArea.transform);

        _footerBar = new GameObject("AttunementFooterBar");
        _footerBar.transform.SetParent(_panel.transform, false);
        RectTransform footerRt = _footerBar.AddComponent<RectTransform>();
        footerRt.anchorMin = new Vector2(0f, 0f);
        footerRt.anchorMax = new Vector2(1f, 0f);
        footerRt.pivot = new Vector2(0.5f, 0f);
        footerRt.offsetMin = new Vector2(30f, 14f);
        footerRt.offsetMax = new Vector2(-30f, 50f);

        GameObject statusObj = new GameObject("FooterStatusText", typeof(RectTransform), typeof(TextMeshProUGUI), typeof(WardTextElement));
        statusObj.transform.SetParent(_footerBar.transform, false);
        RectTransform statusRt = statusObj.GetComponent<RectTransform>();
        statusRt.anchorMin = new Vector2(0f, 0f);
        statusRt.anchorMax = new Vector2(0.68f, 1f);
        statusRt.pivot = new Vector2(0f, 0.5f);
        statusRt.offsetMin = Vector2.zero;
        statusRt.offsetMax = Vector2.zero;

        _footerStatusText = statusObj.GetComponent<TextMeshProUGUI>();
        _footerStatusText.font = UIFonts.GetPrimaryFont();
        _footerStatusText.fontSize = 13;
        _footerStatusText.color = WardColorPalette.ValheimBeige;
        _footerStatusText.alignment = TextAlignmentOptions.MidlineLeft;
        statusObj.GetComponent<WardTextElement>().baseFontSize = 13;

        GameObject attuneBtnObj = GUIManager.Instance.CreateButton(
            "$wmb_btn_ward_attunement".Localize(),
            _footerBar.transform,
            new Vector2(1f, 0.5f),
            new Vector2(1f, 0.5f),
            Vector2.zero,
            240f,
            34f);
        RectTransform attuneBtnRt = attuneBtnObj.GetComponent<RectTransform>();
        attuneBtnRt.pivot = new Vector2(1f, 0.5f);
        attuneBtnRt.anchoredPosition = Vector2.zero;
        attuneBtnRt.sizeDelta = new Vector2(240f, 34f);
        _footerAttunementBtnText = WardUIFactory.SetupButtonText(attuneBtnObj, "$wmb_btn_ward_attunement".Localize(), 12f, WardColorPalette.GoldAmber, TextAlignmentOptions.Center);
        _footerAttunementBtn = attuneBtnObj.GetComponent<Button>();
        _footerAttunementBtn.onClick.AddListener(OnFooterAttunementClicked);

        _remoteSelector = new RemoteWardSelectorModal();
        _remoteSelector.Build(_panel.transform, OnRemoteWardSelected);

        SwitchTab(0);
        _currentFontSizeMode = ConfigRegistry.GuiFontSize != null ? ConfigRegistry.GuiFontSize.Value : WardFontSizeMode.Small;
        ApplyFontSize(_currentFontSizeMode);
        _panel.SetActive(false);
    }

    public void Open(PrivateArea ward)
    {
        if (ward == null || ward.m_nview == null || !ward.m_nview.IsValid())
            return;

        ZDO zdo = ward.m_nview.GetZDO();
        long playerId = Player.m_localPlayer != null ? Player.m_localPlayer.GetPlayerID() : 0L;
        if (!WardRoleManager.CanPerform(zdo, playerId, "core.view_gui"))
            return;

        if (!IsOpen || _physicalWard == null)
        {
            _physicalWard = ward;
        }

        _openedFrame = Time.frameCount;
        _activeWard = ward;
        _panel.SetActive(true);
        SetInputBlocked(true);

        WardRole role = WardPermissions.GetPlayerRole(zdo, playerId);

        string customName = zdo.GetString(Configuration.WardZdoKeys.CustomName, "$wmb_default_ward_name".Localize());
        WardType wardType = (WardType)zdo.GetInt(WardZdoKeys.WardType, (int)WardType.Main);
        string typeSuffix = string.Empty;
        if (wardType == WardType.Extension && !customName.Contains("$wmb_ward_type_extension") && !customName.Contains("Extension"))
        {
            typeSuffix = $" ({"$wmb_ward_type_extension".Localize()})";
        }
        else if (wardType == WardType.SubWard && !customName.Contains("$wmb_ward_type_subward") && !customName.Contains("Sub-Ward"))
        {
            typeSuffix = $" ({"$wmb_ward_type_subward".Localize()})";
        }
        _titleText.text = $"{customName.Localize()}{typeSuffix}".ToUpperInvariant();

        UpdateTabButtonVisibility(zdo, playerId);

        _tabOverview.Bind(_activeWard);
        _tabDefenses.Bind(_activeWard);
        _tabUpkeep.Bind(_activeWard);
        _tabAccess.Bind(_activeWard);
        _tabRoles.Bind(_activeWard);
        _tabManagement.Bind(_activeWard);
        _tabAttunement.Bind(_activeWard);

        UpdateFooterVisibility();
        _isAttunementViewOpen = false;
        SwitchTab(_currentTabIndex);
        ApplyFontSizeToHierarchy(_panel, _currentFontSizeMode);
    }

    private void UpdateTabButtonVisibility(ZDO zdo, long playerId)
    {
        bool canDefend = WardRoleManager.CanPerform(zdo, playerId, "defenses.dome_size") ||
                         WardRoleManager.CanPerform(zdo, playerId, "defenses.combat") ||
                         WardRoleManager.CanPerform(zdo, playerId, "defenses.camouflage") ||
                         WardRoleManager.CanPerform(zdo, playerId, "defenses.biome") ||
                         WardRoleManager.CanPerform(zdo, playerId, "defenses.tames");

        bool canUpkeep = WardRoleManager.CanPerform(zdo, playerId, "upkeep.repair") ||
                         WardRoleManager.CanPerform(zdo, playerId, "upkeep.autofuel") ||
                         WardRoleManager.CanPerform(zdo, playerId, "upkeep.autodoors") ||
                         WardRoleManager.CanPerform(zdo, playerId, "upkeep.perks") ||
                         WardRoleManager.CanPerform(zdo, playerId, "upkeep.waterdrain") ||
                         WardRoleManager.CanPerform(zdo, playerId, "upkeep.security");

        bool canAccess = WardRoleManager.CanPerform(zdo, playerId, "access.doors") ||
                         WardRoleManager.CanPerform(zdo, playerId, "access.chests") ||
                         WardRoleManager.CanPerform(zdo, playerId, "access.portals");

        bool canRoster = WardRoleManager.CanPerform(zdo, playerId, "personnel.roster");
        bool canGovern = WardRoleManager.CanPerform(zdo, playerId, "personnel.governance");

        if (_tabButtons[1] != null) _tabButtons[1].gameObject.SetActive(canDefend);
        if (_tabButtons[2] != null) _tabButtons[2].gameObject.SetActive(canUpkeep);
        if (_tabButtons[3] != null) _tabButtons[3].gameObject.SetActive(canAccess);
        if (_tabButtons[4] != null) _tabButtons[4].gameObject.SetActive(canRoster);
        if (_tabButtons[5] != null) _tabButtons[5].gameObject.SetActive(canGovern);

        if ((_currentTabIndex == 1 && !canDefend) ||
            (_currentTabIndex == 2 && !canUpkeep) ||
            (_currentTabIndex == 3 && !canAccess) ||
            (_currentTabIndex == 4 && !canRoster) ||
            (_currentTabIndex == 5 && !canGovern))
        {
            _currentTabIndex = 0;
        }

        UpdateTabButtonDimensions();
    }

    public void Close()
    {
        LastClosedFrame = Time.frameCount;
        if (_tabAttunement != null)
        {
            _tabAttunement.CancelChannel();
        }

        if (_remoteSelector != null && _remoteSelector.IsOpen)
        {
            _remoteSelector.Close();
        }

        if (_panel != null)
        {
            _panel.SetActive(false);
        }

        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }

        _physicalWard = null;
        _activeWard = null;
        SetInputBlocked(false);
        PlayerController.SetTakeInputDelay(0.1f);
    }

    public void RefreshCurrentTab()
    {
        if (!IsOpen || _activeWard == null)
            return;

        if (_activeWard.m_nview != null && _activeWard.m_nview.IsValid())
        {
            ZDO zdo = _activeWard.m_nview.GetZDO();
            long playerId = Player.m_localPlayer != null ? Player.m_localPlayer.GetPlayerID() : 0L;
            UpdateTabButtonVisibility(zdo, playerId);
        }

        UpdateAttunementFooter();
        if (_isAttunementViewOpen)
        {
            if (_tabBar != null) _tabBar.SetActive(false);
            if (_contentRt != null) _contentRt.offsetMax = new Vector2(-30f, -60f);
            _tabAttunement.Refresh();
        }
        else
        {
            if (_tabBar != null) _tabBar.SetActive(true);
            if (_contentRt != null) _contentRt.offsetMax = new Vector2(-30f, -106f);
            SwitchTab(_currentTabIndex);
        }
    }

    private void OnFooterAttunementClicked()
    {
        if (_isAttunementViewOpen)
        {
            SwitchTab(_currentTabIndex);
        }
        else
        {
            OpenAttunementView();
        }
    }

    private void OpenAttunementView()
    {
        _isAttunementViewOpen = true;

        if (_tabBar != null)
        {
            _tabBar.SetActive(false);
        }
        if (_contentRt != null)
        {
            _contentRt.offsetMax = new Vector2(-30f, -60f);
        }

        for (int i = 0; i < _tabButtons.Length; i++)
        {
            if (_tabButtons[i] != null)
            {
                TextMeshProUGUI txt = _tabButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                if (txt != null)
                {
                    txt.color = WardColorPalette.ValheimBeige;
                }
            }
        }

        _tabOverview.Root.SetActive(false);
        _tabDefenses.Root.SetActive(false);
        _tabUpkeep.Root.SetActive(false);
        _tabAccess.Root.SetActive(false);
        _tabRoles.Root.SetActive(false);
        _tabManagement.Root.SetActive(false);

        _tabAttunement.Root.SetActive(true);
        _tabAttunement.Refresh();

        UpdateAttunementFooter();
        ApplyFontSizeToHierarchy(_panel, _currentFontSizeMode);
    }

    private void SwitchTab(int tabIndex)
    {
        _isAttunementViewOpen = false;
        _currentTabIndex = tabIndex;

        if (_tabBar != null)
        {
            _tabBar.SetActive(true);
        }
        if (_contentRt != null)
        {
            _contentRt.offsetMax = new Vector2(-30f, -106f);
        }

        for (int i = 0; i < _tabButtons.Length; i++)
        {
            if (_tabButtons[i] != null)
            {
                TextMeshProUGUI txt = _tabButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                if (txt != null)
                {
                    txt.color = (i == tabIndex) ? WardColorPalette.ValheimOrange : WardColorPalette.ValheimBeige;
                }
            }
        }

        _tabOverview.Root.SetActive(tabIndex == 0);
        _tabDefenses.Root.SetActive(tabIndex == 1);
        _tabUpkeep.Root.SetActive(tabIndex == 2);
        _tabAccess.Root.SetActive(tabIndex == 3);
        _tabRoles.Root.SetActive(tabIndex == 4);
        _tabManagement.Root.SetActive(tabIndex == 5);
        _tabAttunement.Root.SetActive(false);

        if (tabIndex == 0) _tabOverview.Refresh();
        else if (tabIndex == 1) _tabDefenses.Refresh();
        else if (tabIndex == 2) _tabUpkeep.Refresh();
        else if (tabIndex == 3) _tabAccess.Refresh();
        else if (tabIndex == 4) _tabRoles.Refresh();
        else if (tabIndex == 5) _tabManagement.Refresh();

        UpdateAttunementFooter();
        ApplyFontSizeToHierarchy(_panel, _currentFontSizeMode);
    }

    private static readonly string[] RomanRanks = { "0", "I", "II", "III", "IV", "V", "VI", "VII", "VIII" };

    private void UpdateAttunementFooter()
    {
        if (_activeWard == null || _activeWard.m_nview == null || !_activeWard.m_nview.IsValid())
            return;

        ZDO zdo = _activeWard.m_nview.GetZDO();
        WardType wardType = (WardType)zdo.GetInt(WardZdoKeys.WardType, (int)WardType.Main);
        bool isExtension = wardType == WardType.Extension;
        ZDO effectiveZdo = WardQueryService.GetEffectiveWardZdo(zdo);

        int highestAttuned = -1;
        for (int r = 0; r <= 7; r++)
        {
            WardRune rune = (WardRune)r;
            if (WardProgressionService.IsRuneAttuned(effectiveZdo, rune))
            {
                highestAttuned = r;
            }
        }

        if (_footerStatusText != null)
        {
            string orangeHex = ColorUtility.ToHtmlStringRGB(WardColorPalette.ValheimOrange);
            string greenHex = ColorUtility.ToHtmlStringRGB(WardColorPalette.ValheimGreen);
            string grayHex = ColorUtility.ToHtmlStringRGB(WardColorPalette.TextMuted);

            string keystoneName = highestAttuned >= 0
                ? WardProgressionService.GetRuneNameLocalizationKey((WardRune)highestAttuned).Localize()
                : string.Empty;

            string rankStr = highestAttuned < 0
                ? $"<color=#{grayHex}>None</color>"
                : $"<color=#{greenHex}>Rank {RomanRanks[highestAttuned + 1]} ({keystoneName})</color>";

            _footerStatusText.text = $"<color=#{orangeHex}>⚡ ATTUNEMENT:</color> {rankStr}";
        }

        if (_footerAttunementBtn != null)
        {
            if (isExtension)
            {
                _footerAttunementBtn.gameObject.SetActive(false);
                if (_isAttunementViewOpen)
                {
                    SwitchTab(_currentTabIndex);
                }
            }
            else
            {
                _footerAttunementBtn.gameObject.SetActive(true);
                if (_footerAttunementBtnText != null)
                {
                    if (_isAttunementViewOpen)
                    {
                        _footerAttunementBtnText.text = "$wmb_btn_return_to_base_settings".Localize();
                        _footerAttunementBtnText.color = WardColorPalette.ValheimBeige;
                    }
                    else
                    {
                        int nextRuneInt = highestAttuned + 1;
                        bool isReady = false;
                        if (nextRuneInt <= 7)
                        {
                            WardRune nextRune = (WardRune)nextRuneInt;
                            isReady = WardProgressionService.CanAttuneRune(effectiveZdo, nextRune, Player.m_localPlayer, out _);
                        }

                        _footerAttunementBtnText.text = "$wmb_btn_ward_attunement".Localize();
                        if (isReady)
                        {
                            _footerAttunementBtnText.color = WardColorPalette.GoldAmber;
                        }
                        else
                        {
                            _footerAttunementBtnText.color = WardColorPalette.ValheimBeige;
                        }
                    }
                }
            }
        }
    }

    private void UpdateFooterVisibility()
    {
        bool progressionEnabled = ConfigRegistry.EnableProgressionSystem != null && ConfigRegistry.EnableProgressionSystem.Value;
        if (_footerBar != null)
        {
            _footerBar.SetActive(progressionEnabled);
        }

        if (_contentRt != null)
        {
            _contentRt.offsetMin = new Vector2(30f, progressionEnabled ? 56f : 25f);
        }
    }

    private void OnOtherWardsClicked()
    {
        _remoteSelector.Open(_activeWard, _physicalWard);
    }

    public void RefreshRemoteSelector()
    {
        if (_remoteSelector != null && _remoteSelector.IsOpen)
        {
            _remoteSelector.Refresh();
        }
    }

    private void OnRemoteWardSelected(KnownWard known)
    {
        if (known == null)
            return;

        if (PrivateArea.m_allAreas != null)
        {
            for (int i = 0; i < PrivateArea.m_allAreas.Count; i++)
            {
                PrivateArea area = PrivateArea.m_allAreas[i];
                if (area != null && area.m_nview != null && area.m_nview.IsValid() && area.m_nview.GetZDO().m_uid == known.ZdoId)
                {
                    Open(area);
                    return;
                }
            }
        }

        Player localPlayer = Player.m_localPlayer;
        if (localPlayer != null)
        {
            localPlayer.Message(MessageHud.MessageType.Center, "$wmb_modal_ward_out_of_range".Localize());
        }
    }

    private void OnCycleFontSizeClicked()
    {
        WardFontSizeMode nextMode = _currentFontSizeMode switch
        {
            WardFontSizeMode.Small => WardFontSizeMode.Medium,
            WardFontSizeMode.Medium => WardFontSizeMode.Large,
            _ => WardFontSizeMode.Small
        };

        ApplyFontSize(nextMode);
    }

    public void ApplyFontSize(WardFontSizeMode mode)
    {
        _currentFontSizeMode = mode;
        if (ConfigRegistry.GuiFontSize != null)
        {
            ConfigRegistry.GuiFontSize.Value = mode;
        }

        if (_fontSizeBtnText != null)
        {
            _fontSizeBtnText.text = mode switch
            {
                WardFontSizeMode.Medium => "$wmb_font_medium".Localize(),
                WardFontSizeMode.Large => "$wmb_font_large".Localize(),
                _ => "$wmb_font_small".Localize()
            };
        }

        ApplyFontSizeToHierarchy(_panel, mode);
        UpdateTabButtonDimensions();

        if (_remoteSelector != null && _remoteSelector.IsOpen)
        {
            _remoteSelector.Refresh();
        }

        if (EntityRenameDialog.Instance != null && EntityRenameDialog.Instance.IsOpen)
        {
            EntityRenameDialog.Instance.ApplyFontSize(mode);
        }
    }

    private void UpdateTabButtonDimensions()
    {
        for (int i = 0; i < _tabButtons.Length; i++)
        {
            Button btn = _tabButtons[i];
            if (btn == null)
                continue;

            LayoutElement le = btn.GetComponent<LayoutElement>();
            if (le == null)
                le = btn.gameObject.AddComponent<LayoutElement>();

            TextMeshProUGUI txt = btn.GetComponentInChildren<TextMeshProUGUI>();
            if (txt != null)
            {
                float textWidth = txt.preferredWidth;
                le.minWidth = textWidth + 24f;
                le.preferredWidth = textWidth + 34f;
                le.minHeight = 36f;
                le.preferredHeight = 36f;
                le.flexibleWidth = 1f;
            }
        }
    }

    public static void ApplyFontSizeToHierarchy(GameObject root, WardFontSizeMode mode)
    {
        if (root == null)
            return;

        int offset = mode switch
        {
            WardFontSizeMode.Medium => 2,
            WardFontSizeMode.Large => 4,
            _ => 0
        };

        TextMeshProUGUI[] tmpTexts = root.GetComponentsInChildren<TextMeshProUGUI>(true);
        for (int i = 0; i < tmpTexts.Length; i++)
        {
            TextMeshProUGUI tmp = tmpTexts[i];
            if (tmp == null)
                continue;

            WardTextElement el = tmp.GetComponent<WardTextElement>();
            if (el == null)
            {
                el = tmp.gameObject.AddComponent<WardTextElement>();
                el.baseFontSize = Mathf.RoundToInt(tmp.fontSize);
            }

            tmp.fontSize = el.baseFontSize + offset;
        }

        Text[] texts = root.GetComponentsInChildren<Text>(true);
        for (int i = 0; i < texts.Length; i++)
        {
            Text txt = texts[i];
            if (txt == null)
                continue;

            WardTextElement el = txt.GetComponent<WardTextElement>();
            if (el == null)
            {
                el = txt.gameObject.AddComponent<WardTextElement>();
                el.baseFontSize = txt.fontSize;
            }

            txt.fontSize = el.baseFontSize + offset;
        }
    }
}
