using Jotunn.Managers;
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
    private Text _titleText;
    private Text _fontSizeBtnText;
    private WardFontSizeMode _currentFontSizeMode = WardFontSizeMode.Small;
    private int _openedFrame = -1;

    private TabOverview _tabOverview;
    private TabDefenses _tabDefenses;
    private TabUpkeep _tabUpkeep;
    private TabAccess _tabAccess;
    private TabRoles _tabRoles;
    private TabManagement _tabManagement;
    private bool _isInputBlocked;

    private RemoteWardSelectorModal _remoteSelector;
    private PrivateArea _activeWard;
    private int _currentTabIndex;

    private readonly Button[] _tabButtons = new Button[6];

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

    private void Awake()
    {
        Patch_PrivateArea.OnOpenWardGuiRequested += Open;
        BuildWindow();
    }

    private void OnDestroy()
    {
        Patch_PrivateArea.OnOpenWardGuiRequested -= Open;
        SetInputBlocked(false);
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

    private void Update()
    {
        if (IsOpen)
        {
            if (TextInput.IsVisible())
            {
                return;
            }

            bool closeRequested = Input.GetKeyDown(KeyCode.Escape) || (ZInput.instance != null && (ZInput.GetKeyDown(KeyCode.Escape) || ZInput.GetButtonDown("JoyMenu") || ZInput.GetButtonDown("JoyButtonB")));
            bool isInputFieldFocused = EventSystem.current != null && 
                EventSystem.current.currentSelectedGameObject != null && 
                EventSystem.current.currentSelectedGameObject.GetComponent<InputField>() != null;

            if (!closeRequested && !isInputFieldFocused && Time.frameCount != _openedFrame && ConfigRegistry.OpenWardHubKey != null && ConfigRegistry.OpenWardHubKey.Value.IsDown())
            {
                closeRequested = true;
            }

            if (closeRequested)
            {
                if (_remoteSelector != null && _remoteSelector.IsOpen)
                {
                    _remoteSelector.Close();
                }
                else
                {
                    Close();
                }
            }
        }
    }

    private void BuildWindow()
    {
        _panel = GUIManager.Instance.CreateWoodpanel(
            transform, 
            new Vector2(0.5f, 0.5f), 
            new Vector2(0.5f, 0.5f), 
            Vector2.zero, 
            880f, 
            640f, 
            draggable: true);
        _panel.name = "WardMainWindow";

        GameObject titleObj = new GameObject("TitleText", typeof(RectTransform), typeof(Text));
        titleObj.transform.SetParent(_panel.transform, false);
        RectTransform titleRt = titleObj.GetComponent<RectTransform>();
        titleRt.anchorMin = new Vector2(0f, 1f);
        titleRt.anchorMax = new Vector2(0f, 1f);
        titleRt.pivot = new Vector2(0f, 1f);
        titleRt.anchoredPosition = new Vector2(30f, -20f);
        titleRt.sizeDelta = new Vector2(520f, 32f);

        _titleText = titleObj.GetComponent<Text>();
        _titleText.text = "$wmb_window_title".Localize();
        _titleText.font = GUIManager.Instance.AveriaSerifBold;
        _titleText.fontSize = 20;
        _titleText.color = WardColorPalette.ValheimOrange;
        _titleText.alignment = TextAnchor.MiddleLeft;

        GameObject fontBtn = GUIManager.Instance.CreateButton(
            "$wmb_font_small".Localize(), 
            _panel.transform, 
            new Vector2(1f, 1f), 
            new Vector2(1f, 1f), 
            new Vector2(-316f, -36f), 
            125f, 
            34f);
        fontBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(125f, 34f);
        _fontSizeBtnText = fontBtn.GetComponentInChildren<Text>();
        if (_fontSizeBtnText != null)
        {
            _fontSizeBtnText.font = GUIManager.Instance.AveriaSerifBold;
            _fontSizeBtnText.fontSize = 13;
        }
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
        Text remoteTxt = remoteBtn.GetComponentInChildren<Text>();
        if (remoteTxt != null)
        {
            remoteTxt.font = GUIManager.Instance.AveriaSerifBold;
            remoteTxt.fontSize = 13;
        }
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
        closeBtn.GetComponent<Button>().onClick.AddListener(Close);

        GameObject tabBar = new GameObject("TabBar");
        tabBar.transform.SetParent(_panel.transform, false);
        RectTransform tabRt = tabBar.AddComponent<RectTransform>();
        tabRt.anchorMin = new Vector2(0f, 1f);
        tabRt.anchorMax = new Vector2(1f, 1f);
        tabRt.pivot = new Vector2(0.5f, 1f);
        tabRt.offsetMin = new Vector2(24f, -96f);
        tabRt.offsetMax = new Vector2(-24f, -60f);

        HorizontalLayoutGroup hlg = tabBar.AddComponent<HorizontalLayoutGroup>();
        hlg.childControlWidth = true;
        hlg.childControlHeight = true;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = true;
        hlg.spacing = 8f;

        string[] tabTokens = { "$wmb_tab_overview", "$wmb_tab_defenses", "$wmb_tab_upkeep", "$wmb_tab_access", "$wmb_tab_roster", "$wmb_tab_management" };
        for (int i = 0; i < tabTokens.Length; i++)
        {
            int index = i;
            GameObject btnObj = GUIManager.Instance.CreateButton(tabTokens[i].Localize(), tabBar.transform, Vector2.zero, Vector2.zero, Vector2.zero, 130f, 36f);
            Text btnTxt = btnObj.GetComponentInChildren<Text>();
            if (btnTxt != null)
            {
                btnTxt.font = GUIManager.Instance.AveriaSerifBold;
                btnTxt.fontSize = 13;
                btnTxt.alignment = TextAnchor.MiddleCenter;
                btnTxt.horizontalOverflow = HorizontalWrapMode.Overflow;
            }

            LayoutElement le = btnObj.AddComponent<LayoutElement>();
            le.minHeight = 36f;
            le.preferredHeight = 36f;
            le.flexibleWidth = 1f;

            _tabButtons[i] = btnObj.GetComponent<Button>();
            _tabButtons[i].onClick.AddListener(() => SwitchTab(index));
        }

        UpdateTabButtonDimensions();

        GameObject contentArea = new GameObject("ContentArea");
        contentArea.transform.SetParent(_panel.transform, false);
        RectTransform contentRt = contentArea.AddComponent<RectTransform>();
        contentRt.anchorMin = Vector2.zero;
        contentRt.anchorMax = Vector2.one;
        contentRt.offsetMin = new Vector2(30f, 25f);
        contentRt.offsetMax = new Vector2(-30f, -106f);

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

        PrivateArea effectiveWard = WardQueryService.GetEffectiveWard(ward);
        if (effectiveWard == null || effectiveWard.m_nview == null || !effectiveWard.m_nview.IsValid())
            effectiveWard = ward;

        ZDO zdo = effectiveWard.m_nview.GetZDO();
        long playerId = Player.m_localPlayer != null ? Player.m_localPlayer.GetPlayerID() : 0L;
        if (!WardRoleManager.CanPerform(zdo, playerId, "core.view_gui"))
            return;

        _openedFrame = Time.frameCount;
        _activeWard = effectiveWard;
        _panel.SetActive(true);
        SetInputBlocked(true);

        WardRole role = WardPermissions.GetPlayerRole(zdo, playerId);

        string customName = zdo.GetString(Configuration.WardZdoKeys.CustomName, "$wmb_default_ward_name".Localize());
        _titleText.text = customName.Localize().ToUpperInvariant();

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

        _tabOverview.Bind(_activeWard);
        _tabDefenses.Bind(_activeWard);
        _tabUpkeep.Bind(_activeWard);
        _tabAccess.Bind(_activeWard);
        _tabRoles.Bind(_activeWard);
        _tabManagement.Bind(_activeWard);

        SwitchTab(_currentTabIndex);
        ApplyFontSizeToHierarchy(_panel, _currentFontSizeMode);
        UpdateTabButtonDimensions();
    }

    public void Close()
    {
        LastClosedFrame = Time.frameCount;
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

        SetInputBlocked(false);
        PlayerController.SetTakeInputDelay(0.1f);
    }

    private void SwitchTab(int tabIndex)
    {
        _currentTabIndex = tabIndex;

        for (int i = 0; i < _tabButtons.Length; i++)
        {
            if (_tabButtons[i] != null)
            {
                Text txt = _tabButtons[i].GetComponentInChildren<Text>();
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

        if (tabIndex == 0) _tabOverview.Refresh();
        else if (tabIndex == 1) _tabDefenses.Refresh();
        else if (tabIndex == 2) _tabUpkeep.Refresh();
        else if (tabIndex == 3) _tabAccess.Refresh();
        else if (tabIndex == 4) _tabRoles.Refresh();
        else if (tabIndex == 5) _tabManagement.Refresh();

        ApplyFontSizeToHierarchy(_panel, _currentFontSizeMode);
    }

    private void OnOtherWardsClicked()
    {
        _remoteSelector.Open(_activeWard);
    }

    private void OnRemoteWardSelected(KnownWard known)
    {
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

            Text txt = btn.GetComponentInChildren<Text>();
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
