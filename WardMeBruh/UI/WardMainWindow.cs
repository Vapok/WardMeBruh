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
    private Text _roleBadge;
    private int _openedFrame = -1;

    private TabOverview _tabOverview;
    private TabDefenses _tabDefenses;
    private TabUpkeep _tabUpkeep;
    private TabAccess _tabAccess;
    private TabRoles _tabRoles;
    private bool _isInputBlocked;

    private RemoteWardSelectorModal _remoteSelector;
    private PrivateArea _activeWard;
    private int _currentTabIndex;

    private readonly Button[] _tabButtons = new Button[5];

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
            bool closeRequested = Input.GetKeyDown(KeyCode.Escape);
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
        titleRt.sizeDelta = new Vector2(380f, 32f);

        _titleText = titleObj.GetComponent<Text>();
        _titleText.text = "WARD CONTROL MATRIX";
        _titleText.font = GUIManager.Instance.AveriaSerifBold;
        _titleText.fontSize = 20;
        _titleText.color = WardColorPalette.ValheimOrange;
        _titleText.alignment = TextAnchor.MiddleLeft;

        GameObject roleBadgeObj = new GameObject("RoleBadge", typeof(RectTransform), typeof(Text));
        roleBadgeObj.transform.SetParent(_panel.transform, false);
        RectTransform roleRt = roleBadgeObj.GetComponent<RectTransform>();
        roleRt.anchorMin = new Vector2(0f, 1f);
        roleRt.anchorMax = new Vector2(0f, 1f);
        roleRt.pivot = new Vector2(0f, 1f);
        roleRt.anchoredPosition = new Vector2(400f, -22f);
        roleRt.sizeDelta = new Vector2(160f, 30f);

        _roleBadge = roleBadgeObj.GetComponent<Text>();
        _roleBadge.text = "[Owner]";
        _roleBadge.font = GUIManager.Instance.AveriaSerifBold;
        _roleBadge.fontSize = 15;
        _roleBadge.color = WardColorPalette.GoldAmber;
        _roleBadge.alignment = TextAnchor.MiddleLeft;

        GameObject remoteBtn = GUIManager.Instance.CreateButton(
            "OTHER WARDS ▾", 
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
        tabRt.offsetMin = new Vector2(30f, -96f);
        tabRt.offsetMax = new Vector2(-30f, -60f);

        HorizontalLayoutGroup hlg = tabBar.AddComponent<HorizontalLayoutGroup>();
        hlg.childControlWidth = true;
        hlg.childControlHeight = true;
        hlg.childForceExpandWidth = true;
        hlg.childForceExpandHeight = true;
        hlg.spacing = 8f;

        string[] tabNames = { "OVERVIEW", "DEFENSES", "UPKEEP", "ACCESS", "ROSTER" };
        for (int i = 0; i < tabNames.Length; i++)
        {
            int index = i;
            GameObject btnObj = GUIManager.Instance.CreateButton(tabNames[i], tabBar.transform, Vector2.zero, Vector2.zero, Vector2.zero, 150f, 36f);
            Text btnTxt = btnObj.GetComponentInChildren<Text>();
            if (btnTxt != null)
            {
                btnTxt.font = GUIManager.Instance.AveriaSerifBold;
                btnTxt.fontSize = 14;
            }
            _tabButtons[i] = btnObj.GetComponent<Button>();
            _tabButtons[i].onClick.AddListener(() => SwitchTab(index));
        }

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

        _remoteSelector = new RemoteWardSelectorModal();
        _remoteSelector.Build(_panel.transform, OnRemoteWardSelected);

        SwitchTab(0);
        _panel.SetActive(false);
    }

    public void Open(PrivateArea ward)
    {
        if (ward == null || ward.m_nview == null || !ward.m_nview.IsValid())
            return;

        _openedFrame = Time.frameCount;
        _activeWard = ward;
        _panel.SetActive(true);
        SetInputBlocked(true);

        ZDO zdo = ward.m_nview.GetZDO();
        long playerId = Player.m_localPlayer != null ? Player.m_localPlayer.GetPlayerID() : 0L;
        WardRole role = WardPermissions.GetPlayerRole(zdo, playerId);

        string customName = zdo.GetString(Configuration.WardZdoKeys.CustomName, "Ward Stone");
        _titleText.text = customName.ToUpperInvariant();
        _roleBadge.text = $"[Role: {role}]";
        _roleBadge.color = role >= WardRole.Manager ? WardColorPalette.GoldAmber : (role == WardRole.Resident ? WardColorPalette.IceBlue : WardColorPalette.TextMuted);

        _tabOverview.Bind(_activeWard);
        _tabDefenses.Bind(_activeWard);
        _tabUpkeep.Bind(_activeWard);
        _tabAccess.Bind(_activeWard);
        _tabRoles.Bind(_activeWard);

        SwitchTab(_currentTabIndex);
    }

    public void Close()
    {
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

        if (tabIndex == 0) _tabOverview.Refresh();
        else if (tabIndex == 1) _tabDefenses.Refresh();
        else if (tabIndex == 2) _tabUpkeep.Refresh();
        else if (tabIndex == 3) _tabAccess.Refresh();
        else if (tabIndex == 4) _tabRoles.Refresh();
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
}
