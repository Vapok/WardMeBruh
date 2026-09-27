using Jotunn.Managers;
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
    private Text _headerText;
    private Button _roleBtn;
    private Text _roleTxt;
    private Button _depotBtn;
    private Text _depotTxt;

    private ZDOID _activeEntityId;
    private bool _isDoor;
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
        if (IsOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            Close();
        }
    }

    private void Build()
    {
        _panel = GUIManager.Instance.CreateWoodpanel(
            transform, 
            new Vector2(0.5f, 0.5f), 
            new Vector2(0.5f, 0.5f), 
            Vector2.zero, 
            480f, 
            260f, 
            draggable: true);
        _panel.name = "EntityRenameDialog";

        GameObject headerObj = GUIManager.Instance.CreateText(
            "CONFIGURE", 
            _panel.transform, 
            new Vector2(0.5f, 1f), 
            new Vector2(0.5f, 1f), 
            new Vector2(0f, -22f), 
            GUIManager.Instance.AveriaSerifBold, 
            18, 
            WardColorPalette.ValheimOrange, 
            false, 
            Color.clear, 
            420f, 
            28f, 
            false);
        _headerText = headerObj.GetComponent<Text>();

        GameObject inputObj = GUIManager.Instance.CreateInputField(
            _panel.transform, 
            new Vector2(0.5f, 0.5f), 
            new Vector2(0.5f, 0.5f), 
            new Vector2(0f, 30f), 
            InputField.ContentType.Standard, 
            "Enter custom name...", 
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
            "Role: Resident", 
            _panel.transform, 
            new Vector2(0.5f, 0.5f), 
            new Vector2(0.5f, 0.5f), 
            new Vector2(-80f, -18f), 
            140f, 
            34f);
        _roleBtn = roleObj.GetComponent<Button>();
        _roleBtn.onClick.AddListener(OnRoleClicked);
        _roleTxt = roleObj.GetComponentInChildren<Text>();
        if (_roleTxt != null)
        {
            _roleTxt.font = GUIManager.Instance.AveriaSerifBold;
            _roleTxt.fontSize = 12;
            _roleTxt.color = WardColorPalette.IceBlue;
        }

        GameObject depotObj = GUIManager.Instance.CreateButton(
            "✖ No Depot", 
            _panel.transform, 
            new Vector2(0.5f, 0.5f), 
            new Vector2(0.5f, 0.5f), 
            new Vector2(80f, -18f), 
            140f, 
            34f);
        _depotBtn = depotObj.GetComponent<Button>();
        _depotBtn.onClick.AddListener(OnDepotClicked);
        _depotTxt = depotObj.GetComponentInChildren<Text>();
        if (_depotTxt != null)
        {
            _depotTxt.font = GUIManager.Instance.AveriaSerifBold;
            _depotTxt.fontSize = 12;
            _depotTxt.color = WardColorPalette.TextMuted;
        }

        GameObject saveBtn = GUIManager.Instance.CreateButton(
            "SAVE", 
            _panel.transform, 
            new Vector2(0.5f, 0f), 
            new Vector2(0.5f, 0f), 
            new Vector2(-80f, 32f), 
            130f, 
            36f);
        saveBtn.GetComponent<Button>().onClick.AddListener(OnSaveClicked);

        GameObject cancelBtn = GUIManager.Instance.CreateButton(
            "CANCEL", 
            _panel.transform, 
            new Vector2(0.5f, 0f), 
            new Vector2(0.5f, 0f), 
            new Vector2(80f, 32f), 
            130f, 
            36f);
        cancelBtn.GetComponent<Button>().onClick.AddListener(Close);

        _panel.SetActive(false);
    }

    public void OpenForDoor(Door door)
    {
        if (door == null || door.m_nview == null || !door.m_nview.IsValid())
            return;

        _activeEntityId = door.m_nview.GetZDO().m_uid;
        _isDoor = true;
        _headerText.text = "CONFIGURE DOOR";

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
            : (door.GetComponent<Piece>() != null ? Localization.instance.Localize(door.GetComponent<Piece>().m_name) : "Door");
        if (string.IsNullOrEmpty(defaultDoorName))
            defaultDoorName = "Door";

        ZDO zdo = door.m_nview.GetZDO();
        string customDoorName = zdo.GetString(WardZdoKeys.DoorName, string.Empty);
        _nameInput.text = (!string.IsNullOrEmpty(customDoorName) && !customDoorName.Equals("Door", System.StringComparison.OrdinalIgnoreCase))
            ? customDoorName
            : defaultDoorName;

        _depotBtn.gameObject.SetActive(false);
        RectTransform roleRt = _roleBtn.GetComponent<RectTransform>();
        roleRt.anchoredPosition = new Vector2(0f, -18f);
        roleRt.sizeDelta = new Vector2(160f, 34f);

        WardRole role = (WardRole)zdo.GetInt(WardZdoKeys.DoorMinRole, (int)WardRole.Resident);
        UpdateRoleButton(role);

        _panel.SetActive(true);
        _nameInput.ActivateInputField();
        _nameInput.Select();
        SetInputBlocked(true);
    }

    public void OpenForContainer(Container container)
    {
        if (container == null || container.m_nview == null || !container.m_nview.IsValid())
            return;

        _activeEntityId = container.m_nview.GetZDO().m_uid;
        _isDoor = false;
        _headerText.text = "CONFIGURE CHEST";

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
            : (container.GetComponent<Piece>() != null ? Localization.instance.Localize(container.GetComponent<Piece>().m_name) : "Chest");
        if (string.IsNullOrEmpty(defaultContainerName))
            defaultContainerName = "Chest";

        ZDO zdo = container.m_nview.GetZDO();
        string customContainerName = zdo.GetString(WardZdoKeys.ContainerName, string.Empty);
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

        WardRole role = (WardRole)zdo.GetInt(WardZdoKeys.ContainerMinRole, (int)WardRole.Resident);
        UpdateRoleButton(role);

        bool isFuel = zdo.GetBool(WardZdoKeys.ContainerIsFuel, false);
        UpdateDepotButton(isFuel);

        _panel.SetActive(true);
        _nameInput.ActivateInputField();
        _nameInput.Select();
        SetInputBlocked(true);
    }

    private void UpdateRoleButton(WardRole role)
    {
        _roleBtn.interactable = _canManage;
        if (_roleTxt != null)
        {
            _roleTxt.text = $"Role: {role}";
            _roleTxt.color = role >= WardRole.Manager ? WardColorPalette.GoldAmber : (role == WardRole.Resident ? WardColorPalette.IceBlue : WardColorPalette.TextMuted);
        }
    }

    private void UpdateDepotButton(bool isFuel)
    {
        _depotBtn.interactable = _canManage && _coveringWard != null;
        if (_depotTxt != null)
        {
            _depotTxt.text = isFuel ? "✔ Fuel Depot" : "✖ No Depot";
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

        int currentRoleInt = _isDoor
            ? zdo.GetInt(WardZdoKeys.DoorMinRole, (int)WardRole.Resident)
            : zdo.GetInt(WardZdoKeys.ContainerMinRole, (int)WardRole.Resident);

        WardRole nextRole = (WardRole)(((currentRoleInt + 1) % 4));

        if (_isDoor)
        {
            zdo.Set(WardZdoKeys.DoorMinRole, (int)nextRole);
            WardNetworkRouter.SendSetRoleLock(_activeEntityId, nextRole, true);
        }
        else
        {
            zdo.Set(WardZdoKeys.ContainerMinRole, (int)nextRole);
            WardNetworkRouter.SendSetRoleLock(_activeEntityId, nextRole, false);
        }

        UpdateRoleButton(nextRole);
        WardMeBruh.Log.Debug($"[Dialog] Set clearance role for {(_isDoor ? "Door" : "Chest")} {_activeEntityId} to {nextRole}");
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

        zdo.Set(WardZdoKeys.ContainerIsFuel, nextFuel);
        WardNetworkRouter.SendToggleFuelDepot(_activeEntityId, nextFuel);

        UpdateDepotButton(nextFuel);
        WardMeBruh.Log.Debug($"[Dialog] Toggled Fuel Depot for Chest {_activeEntityId} to {nextFuel}");
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
        string newName = _nameInput.text.Trim();
        if (!string.IsNullOrEmpty(newName))
        {
            if (_isDoor)
            {
                WardNetworkRouter.SendRenameDoor(_activeEntityId, newName);
            }
            else
            {
                WardNetworkRouter.SendRenameContainer(_activeEntityId, newName);
            }
        }
        Close();
    }
}
