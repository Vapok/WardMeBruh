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
    public static int LastClosedFrame { get; private set; } = -1;

    private void Awake()
    {
        Build();
    }

    private void OnDestroy()
    {
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
            bool closeRequested = Input.GetKeyDown(KeyCode.Escape) ||
                ZInput.GetKeyDown(KeyCode.Escape) ||
                (UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.escapeKey.wasPressedThisFrame) ||
                (ZInput.instance != null && (ZInput.GetButtonDown("JoyMenu") || ZInput.GetButtonDown("JoyButtonB")));

            if (closeRequested)
            {
                Close();
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
        WardUIFactory.SetupButtonText(saveBtn, "$wmb_save".Localize(), 13f, WardColorPalette.ValheimOrange);
        saveBtn.GetComponent<Button>().onClick.AddListener(OnSaveClicked);

        GameObject cancelBtn = GUIManager.Instance.CreateButton(
            "$wmb_cancel".Localize(), 
            _panel.transform, 
            new Vector2(0.5f, 0f), 
            new Vector2(0.5f, 0f), 
            new Vector2(80f, 32f), 
            130f, 
            36f);
        WardUIFactory.SetupButtonText(cancelBtn, "$wmb_cancel".Localize(), 13f, WardColorPalette.TextMuted);
        cancelBtn.GetComponent<Button>().onClick.AddListener(Close);

        _panel.SetActive(false);
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
        _nameInput.ActivateInputField();
        _nameInput.Select();
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
        _nameInput.ActivateInputField();
        _nameInput.Select();
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
        if (_panel != null)
        {
            _panel.SetActive(false);
        }

        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }

        ZInput.ResetButtonStatus("Chat");
        StartCoroutine(DeferredUnblockInput());
    }

    private System.Collections.IEnumerator DeferredUnblockInput()
    {
        yield return null;
        ZInput.ResetButtonStatus("Chat");
        SetInputBlocked(false);
        PlayerController.SetTakeInputDelay(0.15f);
    }

    private void OnSaveClicked()
    {
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
