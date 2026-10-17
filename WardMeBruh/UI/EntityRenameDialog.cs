using System.Collections.Generic;
using Jotunn.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using WardMeBruh.Configuration;
using WardMeBruh.Models;
using WardMeBruh.Networking;
using WardMeBruh.Patches;
using WardMeBruh.Services;

namespace WardMeBruh.UI;

public class EntityRenameDialog : MonoBehaviour
{
    private static EntityRenameDialog _instance;
    private GameObject _panel;
    private InputField _nameInput;
    private TextMeshProUGUI _headerText;
    private Button _roleBtn;
    private TextMeshProUGUI _roleTxt;
    private Button _depotBtn;
    private TextMeshProUGUI _depotTxt;
    private Button _saveBtn;
    private Button _cancelBtn;
    private TextMeshProUGUI _saveBtnTxt;
    private TextMeshProUGUI _cancelBtnTxt;
    private GameObject _gamepadLegend;
    private GameObject _focusReticle;
    private Selectable _focusedSelectable;
    private int _lastSubmitFrame = -1;

    private enum ConfigEntityType
    {
        Door,
        Container,
        Portal
    }

    private ZDOID _activeEntityId;
    private ConfigEntityType _entityType;
    private bool _isInputBlocked;
    private PrivateArea _coveringWard;
    private bool _canManage;

    public static EntityRenameDialog Instance
    {
        get
        {
            if (_instance == null && !GUIManager.IsHeadless() && GUIManager.CustomGUIFront != null)
            {
                GameObject root = new GameObject("EntityRenameDialog_Host");
                root.transform.SetParent(GUIManager.CustomGUIFront.transform, false);
                _instance = root.AddComponent<EntityRenameDialog>();
            }
            return _instance;
        }
    }

    public bool IsOpen => _panel != null && _panel.activeSelf;
    public static bool HasInstanceAndOpen => _instance != null && _instance.IsOpen;
    public static int LastClosedFrame { get; private set; } = -1;

    private void Awake()
    {
        _instance = this;
        Build();
    }

    private void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
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
            if (Player.m_localPlayer != null)
            {
                Player.m_localPlayer.SetControls(Vector3.zero, false, false, false, false, false, false, false, false, false, false);
            }

            bool closeRequested = Input.GetKeyDown(KeyCode.Escape) ||
                ZInput.GetKeyDown(KeyCode.Escape) ||
                (UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.escapeKey.wasPressedThisFrame) ||
                WardGamepadNavHelper.IsButtonDownB() ||
                (ZInput.instance != null && (ZInput.GetButtonDown("JoyMenu") || ZInput.GetButtonDown("JoyRadialClose")));

            if (closeRequested)
            {
                Close();
                return;
            }

            if (WardGamepadNavHelper.IsGamepadActive())
            {
                if (_focusedSelectable == null || !_focusedSelectable.gameObject.activeInHierarchy)
                {
                    SetFocus(_roleBtn);
                }

                if (WardGamepadNavHelper.IsButtonDownX())
                {
                    OnSaveClicked();
                    return;
                }

                HandleGamepadNavigation();
            }
            else
            {
                WardGamepadNavHelper.AttachFocusReticle(_focusReticle, null);
            }
        }
    }

    private void SetFocus(Selectable target)
    {
        if (target == null || !target.gameObject.activeInHierarchy)
            return;

        if (_focusedSelectable != null && _focusedSelectable != target)
        {
            InputField prevInput = _focusedSelectable as InputField;
            if (prevInput != null)
            {
                prevInput.DeactivateInputField();
            }
        }

        _focusedSelectable = target;

        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(target.gameObject);
        }

        target.Select();
        WardGamepadNavHelper.ApplyGamepadColorBlock(target);
        WardGamepadNavHelper.AttachFocusReticle(_focusReticle, target);
    }

    private void HandleGamepadNavigation()
    {
        if (WardGamepadNavHelper.IsButtonDownA() && Time.frameCount != _lastSubmitFrame)
        {
            _lastSubmitFrame = Time.frameCount;
            InputField activeInput = _focusedSelectable as InputField;
            if (activeInput != null && activeInput == _nameInput && _nameInput.interactable)
            {
                _nameInput.ActivateInputField();
                return;
            }

            Button activeBtn = _focusedSelectable as Button;
            if (activeBtn != null && activeBtn.interactable)
            {
                activeBtn.onClick?.Invoke();
                return;
            }
        }

        if (!WardGamepadNavHelper.GetDirectionInput(out bool up, out bool down, out bool left, out bool right))
        {
            return;
        }

        bool hasInput = _nameInput != null && _nameInput.gameObject.activeInHierarchy;
        bool hasDepot = _depotBtn != null && _depotBtn.gameObject.activeInHierarchy;

        Selectable target = _focusedSelectable;

        if (_focusedSelectable == _nameInput)
        {
            if (down)
            {
                target = _roleBtn;
            }
        }
        else if (_focusedSelectable == _roleBtn)
        {
            if (up && hasInput)
            {
                target = _nameInput;
            }
            else if (down)
            {
                target = _saveBtn;
            }
            else if (right && hasDepot)
            {
                target = _depotBtn;
            }
        }
        else if (_focusedSelectable == _depotBtn)
        {
            if (up && hasInput)
            {
                target = _nameInput;
            }
            else if (down)
            {
                target = _cancelBtn;
            }
            else if (left)
            {
                target = _roleBtn;
            }
        }
        else if (_focusedSelectable == _saveBtn)
        {
            if (up)
            {
                target = _roleBtn;
            }
            else if (right)
            {
                target = _cancelBtn;
            }
        }
        else if (_focusedSelectable == _cancelBtn)
        {
            if (up)
            {
                target = hasDepot ? _depotBtn : _roleBtn;
            }
            else if (left)
            {
                target = _saveBtn;
            }
        }
        else
        {
            target = _roleBtn;
        }

        if (target != null && target != _focusedSelectable)
        {
            SetFocus(target);
        }
    }

    private void RefreshKeyhints()
    {
        bool isGamepad = ZInput.IsGamepadActive();
        if (_saveBtnTxt != null)
        {
            string xGlyph = WardGamepadNavHelper.GetGamepadGlyph("X");
            _saveBtnTxt.text = isGamepad ? $"{xGlyph} {"$wmb_save".Localize()}" : "$wmb_save".Localize();
        }
        if (_cancelBtnTxt != null)
        {
            string bGlyph = WardGamepadNavHelper.GetGamepadGlyph("B");
            _cancelBtnTxt.text = isGamepad ? $"{bGlyph} {"$wmb_cancel".Localize()}" : "$wmb_cancel".Localize();
        }
        if (_gamepadLegend != null)
        {
            _gamepadLegend.SetActive(isGamepad);
            if (isGamepad)
            {
                TextMeshProUGUI legendText = _gamepadLegend.GetComponent<TextMeshProUGUI>();
                if (legendText != null)
                {
                    string dpad = WardGamepadNavHelper.GetGamepadGlyph("DPad");
                    string a = WardGamepadNavHelper.GetGamepadGlyph("A");
                    string x = WardGamepadNavHelper.GetGamepadGlyph("X");
                    string b = WardGamepadNavHelper.GetGamepadGlyph("B");
                    legendText.text = $"<color=#e2d6b5>{dpad} {"$wmb_legend_navigate".Localize()}   {a} {"$wmb_legend_toggle".Localize()}   {x} {"$wmb_legend_save".Localize()}   {b} {"$wmb_legend_cancel".Localize()}</color>";
                }
            }
        }
    }

    private void Build()
    {
        UIFonts.GetPrimaryFont();

        _panel = GUIManager.Instance.CreateWoodpanel(
            transform, 
            new Vector2(0.5f, 0.5f), 
            new Vector2(0.5f, 0.5f), 
            Vector2.zero, 
            480f, 
            260f, 
            draggable: true);
        _panel.name = "EntityRenameDialog";

        GameObject headerObj = new GameObject("HeaderText", typeof(RectTransform), typeof(TextMeshProUGUI), typeof(WardTextElement));
        headerObj.transform.SetParent(_panel.transform, false);
        RectTransform headerRt = headerObj.GetComponent<RectTransform>();
        headerRt.anchorMin = new Vector2(0.5f, 1f);
        headerRt.anchorMax = new Vector2(0.5f, 1f);
        headerRt.pivot = new Vector2(0.5f, 1f);
        headerRt.anchoredPosition = new Vector2(0f, -22f);
        headerRt.sizeDelta = new Vector2(420f, 28f);

        _headerText = headerObj.GetComponent<TextMeshProUGUI>();
        _headerText.font = UIFonts.GetPrimaryFont();
        _headerText.text = "$wmb_dialog_configure".Localize();
        _headerText.fontSize = 18f;
        _headerText.color = WardColorPalette.ValheimOrange;
        _headerText.alignment = TextAlignmentOptions.Center;

        WardTextElement headerWte = headerObj.GetComponent<WardTextElement>();
        headerWte.baseFontSize = 18;

        GameObject inputObj = GUIManager.Instance.CreateInputField(
            _panel.transform, 
            new Vector2(0.5f, 0.5f), 
            new Vector2(0.5f, 0.5f), 
            new Vector2(0f, 30f), 
            InputField.ContentType.Standard, 
            "$wmb_enter_custom_name".Localize(), 
            16, 
            400f, 
            36f);
        _nameInput = inputObj.GetComponent<InputField>();
        DisableNavigation(_nameInput);
        _nameInput.onEndEdit.AddListener(text =>
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                ZInput.ResetButtonStatus("Chat");
                OnSaveClicked();
            }
        });

        GameObject roleObj = GUIManager.Instance.CreateButton(
            "$wmb_role_resident".Localize(), 
            _panel.transform, 
            new Vector2(0.5f, 0.5f), 
            new Vector2(0.5f, 0.5f), 
            new Vector2(-80f, -18f), 
            140f, 
            34f);
        _roleBtn = roleObj.GetComponent<Button>();
        DisableNavigation(_roleBtn);
        _roleBtn.onClick.AddListener(OnRoleClicked);
        _roleTxt = WardUIFactory.SetupButtonText(roleObj, "$wmb_role_resident".Localize(), 12f, WardColorPalette.IceBlue);

        GameObject depotObj = GUIManager.Instance.CreateButton(
            "$wmb_depot_no".Localize(), 
            _panel.transform, 
            new Vector2(0.5f, 0.5f), 
            new Vector2(0.5f, 0.5f), 
            new Vector2(80f, -18f), 
            140f, 
            34f);
        _depotBtn = depotObj.GetComponent<Button>();
        DisableNavigation(_depotBtn);
        _depotBtn.onClick.AddListener(OnDepotClicked);
        _depotTxt = WardUIFactory.SetupButtonText(depotObj, "$wmb_depot_no".Localize(), 12f, WardColorPalette.TextMuted);

        GameObject saveBtn = GUIManager.Instance.CreateButton(
            "$wmb_save".Localize(), 
            _panel.transform, 
            new Vector2(0.5f, 0f), 
            new Vector2(0.5f, 0f), 
            new Vector2(-80f, 32f), 
            130f, 
            36f);
        _saveBtn = saveBtn.GetComponent<Button>();
        DisableNavigation(_saveBtn);
        _saveBtnTxt = WardUIFactory.SetupButtonText(saveBtn, "$wmb_save".Localize(), 13f, WardColorPalette.ValheimOrange);
        _saveBtn.onClick.AddListener(OnSaveClicked);

        GameObject cancelBtn = GUIManager.Instance.CreateButton(
            "$wmb_cancel".Localize(), 
            _panel.transform, 
            new Vector2(0.5f, 0f), 
            new Vector2(0.5f, 0f), 
            new Vector2(80f, 32f), 
            130f, 
            36f);
        _cancelBtn = cancelBtn.GetComponent<Button>();
        DisableNavigation(_cancelBtn);
        _cancelBtnTxt = WardUIFactory.SetupButtonText(cancelBtn, "$wmb_cancel".Localize(), 13f, WardColorPalette.TextMuted);
        _cancelBtn.onClick.AddListener(Close);

        _gamepadLegend = new GameObject("GamepadLegend", typeof(RectTransform), typeof(TextMeshProUGUI));
        _gamepadLegend.transform.SetParent(_panel.transform, false);
        RectTransform glRt = _gamepadLegend.GetComponent<RectTransform>();
        glRt.anchorMin = new Vector2(0f, 0f);
        glRt.anchorMax = new Vector2(1f, 0f);
        glRt.pivot = new Vector2(0.5f, 1f);
        glRt.anchoredPosition = new Vector2(0f, -6f);
        glRt.sizeDelta = new Vector2(440f, 20f);

        TextMeshProUGUI glText = _gamepadLegend.GetComponent<TextMeshProUGUI>();
        glText.font = UIFonts.GetPrimaryFont();
        glText.text = $"<color=#e2d6b5><b>[D-Pad]</b> {"$wmb_legend_navigate".Localize()}   <b>[A]</b> {"$wmb_legend_toggle".Localize()}   <b>[X]</b> {"$wmb_legend_save".Localize()}   <b>[B]</b> {"$wmb_legend_cancel".Localize()}</color>";
        glText.fontSize = 11f;
        glText.alignment = TextAlignmentOptions.Center;
        glText.color = WardColorPalette.ValheimBeige;

        _focusReticle = WardGamepadNavHelper.CreateFocusReticle(_panel.transform);

        _panel.SetActive(false);
    }

    private static void DisableNavigation(Selectable selectable)
    {
        if (selectable == null)
            return;

        Navigation nav = selectable.navigation;
        nav.mode = Navigation.Mode.None;
        selectable.navigation = nav;
    }

    public void OpenForDoor(Door door)
    {
        if (door == null || door.m_nview == null || !door.m_nview.IsValid())
            return;

        _activeEntityId = door.m_nview.GetZDO().m_uid;
        _entityType = ConfigEntityType.Door;
        _headerText.text = "$wmb_configure_door".Localize().ToUpperInvariant();

        long playerId = Player.m_localPlayer != null ? Player.m_localPlayer.GetPlayerID() : 0L;
        _coveringWard = WardQueryService.GetCoveringWard(door.transform.position);
        if (_coveringWard != null && _coveringWard.m_nview != null && _coveringWard.m_nview.IsValid())
        {
            _canManage = WardPermissions.CanManageWard(_coveringWard.m_nview.GetZDO(), playerId);
        }
        else
        {
            Piece piece = door.GetComponent<Piece>();
            _canManage = piece != null && (piece.IsCreator() || piece.GetCreator() == playerId || piece.GetCreator() == 0L);
        }

        string defaultDoorName = !string.IsNullOrEmpty(door.m_name)
            ? Localization.instance.Localize(door.m_name)
            : (door.GetComponent<Piece>() != null ? Localization.instance.Localize(door.GetComponent<Piece>().m_name) : "$wmb_door".Localize());
        if (string.IsNullOrEmpty(defaultDoorName))
            defaultDoorName = "$wmb_door".Localize();

        ZDO zdo = door.m_nview.GetZDO();
        string customDoorName = zdo.GetString(WardZdoKeys.DoorName, string.Empty);
        _nameInput.gameObject.SetActive(true);
        _nameInput.text = (!string.IsNullOrEmpty(customDoorName) && !customDoorName.Equals("Door", System.StringComparison.OrdinalIgnoreCase))
            ? customDoorName
            : defaultDoorName;

        _depotBtn.gameObject.SetActive(false);
        RectTransform roleRt = _roleBtn.GetComponent<RectTransform>();
        roleRt.anchoredPosition = new Vector2(0f, -18f);
        roleRt.sizeDelta = new Vector2(160f, 34f);

        int role = zdo.GetInt(WardZdoKeys.DoorMinRole, (int)WardRole.Resident);
        UpdateRoleButton(role);

        ApplyCurrentFontSize();

        _panel.SetActive(true);
        RefreshKeyhints();
        if (WardGamepadNavHelper.IsGamepadActive())
        {
            SetFocus(_roleBtn);
        }
        else
        {
            WardGamepadNavHelper.AttachFocusReticle(_focusReticle, null);
            _nameInput.ActivateInputField();
            _nameInput.Select();
        }
        SetInputBlocked(true);
    }

    public void OpenForPortal(TeleportWorld portal)
    {
        if (portal == null || portal.m_nview == null || !portal.m_nview.IsValid())
            return;

        _activeEntityId = portal.m_nview.GetZDO().m_uid;
        _entityType = ConfigEntityType.Portal;

        string tag = portal.GetText();
        _headerText.text = !string.IsNullOrEmpty(tag)
            ? $"{"$wmb_configure_portal".Localize().ToUpperInvariant()}: \"{tag}\""
            : "$wmb_configure_portal".Localize().ToUpperInvariant();

        long playerId = Player.m_localPlayer != null ? Player.m_localPlayer.GetPlayerID() : 0L;
        _coveringWard = WardQueryService.GetCoveringWard(portal.transform.position);
        if (_coveringWard != null && _coveringWard.m_nview != null && _coveringWard.m_nview.IsValid())
        {
            _canManage = WardPermissions.HasRequiredRole(_coveringWard.m_nview.GetZDO(), playerId, WardRole.Resident);
        }
        else
        {
            Piece piece = portal.GetComponent<Piece>();
            _canManage = piece != null && (piece.IsCreator() || piece.GetCreator() == playerId || piece.GetCreator() == 0L);
        }

        _nameInput.gameObject.SetActive(false);
        _depotBtn.gameObject.SetActive(false);

        RectTransform roleRt = _roleBtn.GetComponent<RectTransform>();
        roleRt.anchoredPosition = new Vector2(0f, 15f);
        roleRt.sizeDelta = new Vector2(200f, 38f);

        ZDO zdo = portal.m_nview.GetZDO();
        int role = zdo.GetInt(WardZdoKeys.PortalMinRole, (int)WardRole.Resident);
        UpdateRoleButton(role);

        ApplyCurrentFontSize();

        _panel.SetActive(true);
        RefreshKeyhints();
        if (WardGamepadNavHelper.IsGamepadActive())
        {
            SetFocus(_roleBtn);
        }
        else
        {
            WardGamepadNavHelper.AttachFocusReticle(_focusReticle, null);
        }
        SetInputBlocked(true);
    }

    public void OpenForContainer(Container container)
    {
        if (container == null || container.m_nview == null || !container.m_nview.IsValid())
            return;

        _activeEntityId = container.m_nview.GetZDO().m_uid;
        _entityType = ConfigEntityType.Container;
        _headerText.text = "$wmb_configure_chest".Localize().ToUpperInvariant();

        long playerId = Player.m_localPlayer != null ? Player.m_localPlayer.GetPlayerID() : 0L;
        _coveringWard = WardQueryService.GetCoveringWard(container.transform.position);
        if (_coveringWard != null && _coveringWard.m_nview != null && _coveringWard.m_nview.IsValid())
        {
            _canManage = WardPermissions.CanManageWard(_coveringWard.m_nview.GetZDO(), playerId);
        }
        else
        {
            Piece piece = container.GetComponent<Piece>();
            _canManage = piece != null && (piece.IsCreator() || piece.GetCreator() == playerId || piece.GetCreator() == 0L);
        }

        string defaultContainerName = !string.IsNullOrEmpty(container.m_name)
            ? Localization.instance.Localize(container.m_name)
            : (container.GetComponent<Piece>() != null ? Localization.instance.Localize(container.GetComponent<Piece>().m_name) : "$wmb_chest".Localize());
        if (string.IsNullOrEmpty(defaultContainerName))
            defaultContainerName = "$wmb_chest".Localize();

        ZDO zdo = container.m_nview.GetZDO();
        string customContainerName = zdo.GetString(WardZdoKeys.ContainerName, string.Empty);
        _nameInput.gameObject.SetActive(true);
        _nameInput.text = (!string.IsNullOrEmpty(customContainerName) && !customContainerName.Equals("Container", System.StringComparison.OrdinalIgnoreCase))
            ? customContainerName
            : defaultContainerName;

        _depotBtn.gameObject.SetActive(true);
        RectTransform roleRt = _roleBtn.GetComponent<RectTransform>();
        roleRt.anchoredPosition = new Vector2(-80f, -18f);
        roleRt.sizeDelta = new Vector2(140f, 34f);

        RectTransform depotRt = _depotBtn.GetComponent<RectTransform>();
        depotRt.anchoredPosition = new Vector2(80f, -18f);
        depotRt.sizeDelta = new Vector2(140f, 34f);

        int role = zdo.GetInt(WardZdoKeys.ContainerMinRole, (int)WardRole.Resident);
        UpdateRoleButton(role);

        bool isFuel = zdo.GetBool(WardZdoKeys.ContainerIsFuel, false);
        UpdateDepotButton(isFuel);

        ApplyCurrentFontSize();

        _panel.SetActive(true);
        RefreshKeyhints();
        if (WardGamepadNavHelper.IsGamepadActive())
        {
            SetFocus(_roleBtn);
        }
        else
        {
            WardGamepadNavHelper.AttachFocusReticle(_focusReticle, null);
            _nameInput.ActivateInputField();
            _nameInput.Select();
        }
        SetInputBlocked(true);
    }

    public void ApplyFontSize(WardFontSizeMode mode)
    {
        if (_panel == null)
            return;

        WardMainWindow.ApplyFontSizeToHierarchy(_panel, mode);
    }

    private void ApplyCurrentFontSize()
    {
        WardFontSizeMode fontMode = ConfigRegistry.GuiFontSize != null 
            ? ConfigRegistry.GuiFontSize.Value 
            : WardFontSizeMode.Small;
        ApplyFontSize(fontMode);
    }

    private void UpdateRoleButton(int roleId)
    {
        _roleBtn.interactable = _canManage;
        if (_roleTxt != null)
        {
            ZDO wardZdo = _coveringWard != null && _coveringWard.m_nview != null && _coveringWard.m_nview.IsValid()
                ? _coveringWard.m_nview.GetZDO()
                : null;
            string roleName = WardRoleManager.GetRoleDisplayName(wardZdo, roleId);
            Color roleColor = WardRoleManager.GetRoleColor(wardZdo, roleId);
            _roleTxt.text = roleName;
            _roleTxt.color = roleColor;
        }
    }

    private void UpdateDepotButton(bool isFuel)
    {
        _depotBtn.interactable = _canManage && _coveringWard != null;
        if (_depotTxt != null)
        {
            _depotTxt.text = isFuel ? "$wmb_depot_yes".Localize() : "$wmb_depot_no".Localize();
            _depotTxt.color = isFuel ? WardColorPalette.GoldAmber : WardColorPalette.TextMuted;
        }
    }

    private void OnRoleClicked()
    {
        if (!_canManage)
            return;

        if (Time.frameCount == _lastSubmitFrame)
            return;
        _lastSubmitFrame = Time.frameCount;

        ZDO zdo = ZDOMan.instance?.GetZDO(_activeEntityId);
        if (zdo == null)
            return;

        ZDO wardZdo = _coveringWard != null && _coveringWard.m_nview != null && _coveringWard.m_nview.IsValid()
            ? _coveringWard.m_nview.GetZDO()
            : null;

        int currentRoleInt = _entityType switch
        {
            ConfigEntityType.Door => zdo.GetInt(WardZdoKeys.DoorMinRole, (int)WardRole.Guest),
            ConfigEntityType.Container => zdo.GetInt(WardZdoKeys.ContainerMinRole, (int)WardRole.Resident),
            ConfigEntityType.Portal => zdo.GetInt(WardZdoKeys.PortalMinRole, (int)WardRole.Resident),
            _ => (int)WardRole.Resident
        };

        int nextRole = WardRoleManager.GetNextRoleId(wardZdo, currentRoleInt);

        if (ZDOMan.instance != null)
        {
            zdo.SetOwner(ZDOMan.GetSessionID());
        }

        if (_entityType == ConfigEntityType.Door)
        {
            zdo.Set(WardZdoKeys.DoorMinRole, nextRole);
            WardNetworkRouter.SendSetRoleLock(_activeEntityId, (WardRole)nextRole, true);
        }
        else if (_entityType == ConfigEntityType.Container)
        {
            zdo.Set(WardZdoKeys.ContainerMinRole, nextRole);
            WardNetworkRouter.SendSetRoleLock(_activeEntityId, (WardRole)nextRole, false);
        }
        else if (_entityType == ConfigEntityType.Portal)
        {
            zdo.Set(WardZdoKeys.PortalMinRole, nextRole);
            if (ZDOMan.instance != null)
            {
                ZDOMan.instance.SetDirtyPortals();
            }
            WardNetworkRouter.SendPortalRole(_activeEntityId, nextRole);
        }

        UpdateRoleButton(nextRole);
        WardMeBruh.Log.Debug($"[Dialog] Set clearance role for {_entityType} {_activeEntityId} to {nextRole}");
    }

    private void OnDepotClicked()
    {
        if (!_canManage || _coveringWard == null)
            return;

        if (Time.frameCount == _lastSubmitFrame)
            return;
        _lastSubmitFrame = Time.frameCount;

        ZDO zdo = ZDOMan.instance?.GetZDO(_activeEntityId);
        if (zdo == null)
            return;

        bool currentFuel = zdo.GetBool(WardZdoKeys.ContainerIsFuel, false);
        bool nextFuel = !currentFuel;

        if (ZDOMan.instance != null)
        {
            zdo.SetOwner(ZDOMan.GetSessionID());
        }

        zdo.Set(WardZdoKeys.ContainerIsFuel, nextFuel);
        WardNetworkRouter.SendToggleFuelDepot(_activeEntityId, nextFuel);

        UpdateDepotButton(nextFuel);
        WardMeBruh.Log.Debug($"[Dialog] Toggled Fuel Depot for Chest {_activeEntityId} to {nextFuel}");
    }

    public void Close()
    {
        LastClosedFrame = Time.frameCount;
        InputField curInput = _focusedSelectable as InputField;
        if (curInput != null)
        {
            curInput.DeactivateInputField();
        }
        _focusedSelectable = null;
        WardGamepadNavHelper.AttachFocusReticle(_focusReticle, null);
        if (_panel != null)
        {
            _panel.SetActive(false);
        }

        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }

        ZInput.ResetButtonStatus("Chat");
        ZInput.ResetButtonStatus("JoySit");
        ZInput.ResetButtonStatus("Sit");
        ZInput.ResetButtonStatus("JoyRadial");
        ZInput.ResetButtonStatus("JoyButtonX");
        StartCoroutine(DeferredUnblockInput());
    }

    private System.Collections.IEnumerator DeferredUnblockInput()
    {
        yield return null;
        ZInput.ResetButtonStatus("Chat");
        ZInput.ResetButtonStatus("JoySit");
        ZInput.ResetButtonStatus("Sit");
        ZInput.ResetButtonStatus("JoyRadial");
        ZInput.ResetButtonStatus("JoyButtonX");
        SetInputBlocked(false);
        PlayerController.SetTakeInputDelay(0.25f);
    }

    private void OnSaveClicked()
    {
        if (Time.frameCount == _lastSubmitFrame)
            return;
        _lastSubmitFrame = Time.frameCount;
        if (_entityType != ConfigEntityType.Portal)
        {
            string newName = _nameInput.text.Trim();
            if (!string.IsNullOrEmpty(newName))
            {
                if (_entityType == ConfigEntityType.Door)
                {
                    WardNetworkRouter.SendRenameDoor(_activeEntityId, newName);
                }
                else
                {
                    WardNetworkRouter.SendRenameContainer(_activeEntityId, newName);
                }
            }
        }
        Close();
    }
}
