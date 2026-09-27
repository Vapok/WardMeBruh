using System.Collections.Generic;
using System.Linq;
using Jotunn.Managers;
using UnityEngine;
using UnityEngine.UI;
using WardMeBruh.API;
using WardMeBruh.Configuration;
using WardMeBruh.Models;
using WardMeBruh.Networking;
using WardMeBruh.Services;

namespace WardMeBruh.UI.Tabs;

public class TabRoles
{
    private GameObject _root;
    private PrivateArea _ward;
    private Transform _listContainer;
    private Dropdown _knownPlayerDropdown;
    private readonly List<KeyValuePair<long, string>> _dropdownPlayers = new();
    private InputField _addPlayerInput;
    private readonly List<GameObject> _rosterCards = new();

    public GameObject Root => _root;

    public void Build(Transform parent)
    {
        _root = new GameObject("Tab_Roles", typeof(RectTransform));
        _root.transform.SetParent(parent, false);
        RectTransform rt = _root.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        WardUIFactory.CreateScrollArea(_root.transform, out _listContainer);
    }

    public void Bind(PrivateArea ward)
    {
        _ward = ward;
        Refresh();
    }

    public void Refresh()
    {
        for (int i = 0; i < _rosterCards.Count; i++)
        {
            if (_rosterCards[i] != null)
                Object.Destroy(_rosterCards[i]);
        }
        _rosterCards.Clear();

        if (_ward == null || _ward.m_nview == null || !_ward.m_nview.IsValid())
            return;

        ZDO zdo = _ward.m_nview.GetZDO();
        ZDO effectiveZdo = WardQueryService.GetEffectiveWardZdo(zdo) ?? zdo;
        long playerId = Player.m_localPlayer != null ? Player.m_localPlayer.GetPlayerID() : 0L;
        bool canManage = WardRoleManager.CanPerform(effectiveZdo, playerId, WardPermissionIds.PersonnelRoster);
        int callerRoleId = WardRoleManager.GetPlayerRoleId(effectiveZdo, playerId);
        bool isOwner = callerRoleId == (int)WardRole.Owner || WardPermissions.IsAdmin(playerId);

        WardRoleAssignments assignments = WardRoleManager.GetRoleAssignments(effectiveZdo);
        List<WardRoleDefinition> allRoles = WardRoleManager.GetRoleDefinitions(effectiveZdo);

        WardRoleDefinition callerDef = null;
        for (int i = 0; i < allRoles.Count; i++)
        {
            if (allRoles[i].Id == callerRoleId)
            {
                callerDef = allRoles[i];
                break;
            }
        }
        int callerLevel = isOwner ? int.MaxValue : (callerDef != null ? callerDef.Level : 0);

        List<WardRoleDefinition> rosterRoles = new();
        for (int i = 0; i < allRoles.Count; i++)
        {
            WardRoleDefinition r = allRoles[i];
            if (r.Id != (int)WardRole.Guest && r.Id != (int)WardRole.Owner)
            {
                rosterRoles.Add(r);
            }
        }
        rosterRoles.Sort((a, b) =>
        {
            int levelComp = a.Level.CompareTo(b.Level);
            return levelComp != 0 ? levelComp : a.Id.CompareTo(b.Id);
        });

        long ownerId = effectiveZdo.GetLong(ZDOVars.s_creator, 0L);

        if (canManage)
        {
            Transform addCard = WardUIFactory.CreateCard(_listContainer, "$wmb_roles_card_enroll");
            _rosterCards.Add(addCard.parent.parent.gameObject);

            List<KeyValuePair<long, string>> allKnown = PlayerRegistryService.GetKnownPlayers();
            List<KeyValuePair<long, string>> knownPlayers = new();
            for (int i = 0; i < allKnown.Count; i++)
            {
                KeyValuePair<long, string> kp = allKnown[i];
                if (kp.Key == ownerId)
                    continue;
                if (assignments.Contains(kp.Key))
                    continue;
                knownPlayers.Add(kp);
            }

            WardMeBruh.Log.Info($"[TabRoles] Refresh. allKnown={allKnown.Count}, knownPlayers={knownPlayers.Count}, ownerId={ownerId}");

            _dropdownPlayers.Clear();

            WardUIFactory.CreateSettingRow(addCard, "$wmb_roles_known_characters", out Transform dropCtrlContainer);

            GameObject dropdownObj = WardUIExtensions.CreateValheimDropdown(dropCtrlContainer, 210f, 28f);
            dropdownObj.GetComponent<RectTransform>().sizeDelta = new Vector2(210f, 28f);
            dropdownObj.SetLayoutDimensions(210f, 28f);
            _knownPlayerDropdown = dropdownObj.GetComponent<Dropdown>();
            _knownPlayerDropdown.ClearOptions();

            GameObject dropAddBtn = GUIManager.Instance.CreateButton(
                "$wmb_roles_add".Localize(),
                dropCtrlContainer,
                Vector2.zero,
                Vector2.zero,
                Vector2.zero,
                85f,
                28f);
            dropAddBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(85f, 28f);
            dropAddBtn.SetLayoutDimensions(85f, 28f);
            Text dropAddTxt = dropAddBtn.GetComponentInChildren<Text>();
            if (dropAddTxt != null)
            {
                dropAddTxt.font = GUIManager.Instance.AveriaSerifBold;
                dropAddTxt.fontSize = 11;
                dropAddTxt.verticalOverflow = VerticalWrapMode.Overflow;
                dropAddTxt.horizontalOverflow = HorizontalWrapMode.Overflow;
                dropAddTxt.alignment = TextAnchor.MiddleCenter;
            }
            dropAddBtn.GetComponent<Button>().onClick.AddListener(() => AddDropdownPlayerToRole((int)WardRole.Resident));

            if (knownPlayers.Count == 0)
            {
                _knownPlayerDropdown.interactable = false;
                _knownPlayerDropdown.AddOptions(new List<Dropdown.OptionData> { new Dropdown.OptionData("$wmb_roles_no_characters".Localize()) });
                _knownPlayerDropdown.value = 0;
                _knownPlayerDropdown.RefreshShownValue();

                dropAddBtn.GetComponent<Button>().interactable = false;
                if (dropAddTxt != null)
                    dropAddTxt.color = WardColorPalette.TextMuted;
            }
            else
            {
                _knownPlayerDropdown.interactable = true;
                List<Dropdown.OptionData> options = new();
                options.Add(new Dropdown.OptionData("$wmb_roles_select_character".Localize()));

                for (int i = 0; i < knownPlayers.Count; i++)
                {
                    _dropdownPlayers.Add(knownPlayers[i]);
                    options.Add(new Dropdown.OptionData(knownPlayers[i].Value));
                }
                _knownPlayerDropdown.AddOptions(options);
                _knownPlayerDropdown.value = 0;
                _knownPlayerDropdown.RefreshShownValue();

                dropAddBtn.GetComponent<Button>().interactable = false;
                if (dropAddTxt != null)
                    dropAddTxt.color = WardColorPalette.TextMuted;

                _knownPlayerDropdown.onValueChanged.AddListener((int index) =>
                {
                    bool hasSelection = index > 0 && index - 1 < _dropdownPlayers.Count;
                    dropAddBtn.GetComponent<Button>().interactable = hasSelection;
                    if (dropAddTxt != null)
                        dropAddTxt.color = hasSelection ? WardColorPalette.ValheimOrange : WardColorPalette.TextMuted;

                    if (hasSelection && _addPlayerInput != null)
                    {
                        _addPlayerInput.text = _dropdownPlayers[index - 1].Value;
                    }
                });
            }

            WardUIFactory.CreateSettingRow(addCard, "$wmb_roles_manual_entry", out Transform inputCtrlContainer);

            GameObject inputObj = GUIManager.Instance.CreateInputField(
                inputCtrlContainer,
                Vector2.zero,
                Vector2.zero,
                Vector2.zero,
                InputField.ContentType.Standard,
                "$wmb_roles_enter_name_placeholder".Localize(),
                14,
                210f,
                28f);
            inputObj.GetComponent<RectTransform>().sizeDelta = new Vector2(210f, 28f);
            inputObj.SetLayoutDimensions(210f, 28f);
            _addPlayerInput = inputObj.GetComponent<InputField>();

            GameObject addManualBtn = GUIManager.Instance.CreateButton(
                "$wmb_roles_add".Localize(),
                inputCtrlContainer,
                Vector2.zero,
                Vector2.zero,
                Vector2.zero,
                85f,
                28f);
            addManualBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(85f, 28f);
            addManualBtn.SetLayoutDimensions(85f, 28f);
            Text addManualTxt = addManualBtn.GetComponentInChildren<Text>();
            if (addManualTxt != null)
            {
                addManualTxt.font = GUIManager.Instance.AveriaSerifBold;
                addManualTxt.fontSize = 11;
                addManualTxt.verticalOverflow = VerticalWrapMode.Overflow;
                addManualTxt.horizontalOverflow = HorizontalWrapMode.Overflow;
                addManualTxt.alignment = TextAnchor.MiddleCenter;
            }
            addManualBtn.GetComponent<Button>().onClick.AddListener(() => AddManualPlayerToRole((int)WardRole.Resident));
        }

        int totalCount = assignments.Entries.Count;
        Transform rosterCard = WardUIFactory.CreateCard(_listContainer, $"{"$wmb_roles_card_roster".Localize()} ({totalCount} {"$wmb_roles_members_unit".Localize()})");
        _rosterCards.Add(rosterCard.parent.parent.gameObject);

        if (totalCount == 0)
        {
            WardUIFactory.CreateBadgeRow(rosterCard, "$wmb_roles_no_players".Localize(), "-", WardColorPalette.TextMuted, out _);
        }
        else
        {
            for (int i = 0; i < assignments.Entries.Count; i++)
            {
                CreateRosterRow(rosterCard, assignments.Entries[i], rosterRoles, allRoles, canManage, isOwner, callerLevel);
            }
        }

        if (WardMainWindow.Instance != null)
        {
            WardMainWindow.ApplyFontSizeToHierarchy(_root, WardMainWindow.Instance.CurrentFontSizeMode);
        }
    }

    private void CreateRosterRow(
        Transform parent,
        PlayerRoleAssignment entry,
        List<WardRoleDefinition> rosterRoles,
        List<WardRoleDefinition> allRoles,
        bool canManage,
        bool isOwner,
        int callerLevel)
    {
        string labelText = $"{entry.PlayerName} (ID: {entry.PlayerId})";
        WardUIFactory.CreateSettingRow(parent, labelText, out Transform ctrlContainer);

        WardRoleDefinition currentRole = null;
        for (int i = 0; i < allRoles.Count; i++)
        {
            if (allRoles[i].Id == entry.RoleId)
            {
                currentRole = allRoles[i];
                break;
            }
        }
        if (currentRole == null)
        {
            currentRole = new WardRoleDefinition(entry.RoleId, $"Role {entry.RoleId}", 1, WardColorPalette.ColorHex(WardColorPalette.IceBlue));
        }

        List<WardRoleDefinition> assignableRoles = new();
        for (int i = 0; i < rosterRoles.Count; i++)
        {
            WardRoleDefinition r = rosterRoles[i];
            if (isOwner || (canManage && callerLevel > r.Level))
            {
                assignableRoles.Add(r);
            }
        }

        bool canEditThisPlayer = canManage && (isOwner || callerLevel > currentRole.Level);
        bool canChangeRole = canEditThisPlayer && assignableRoles.Count > 1;
        bool canRemove = canEditThisPlayer;

        string roleBtnText = $"[ {"$wmb_role".Localize()}: {currentRole.Name.Localize()} ]";
        float roleBtnWidth = Mathf.Clamp(roleBtnText.Length * 8f + 25f, 135f, 200f);

        GameObject roleBtnObj = GUIManager.Instance.CreateButton(
            roleBtnText,
            ctrlContainer,
            Vector2.zero,
            Vector2.zero,
            Vector2.zero,
            roleBtnWidth,
            28f);
        roleBtnObj.GetComponent<RectTransform>().sizeDelta = new Vector2(roleBtnWidth, 28f);
        roleBtnObj.SetLayoutDimensions(roleBtnWidth, 28f);

        Text roleTxt = roleBtnObj.GetComponentInChildren<Text>();
        if (roleTxt != null)
        {
            roleTxt.font = GUIManager.Instance.AveriaSerifBold;
            roleTxt.fontSize = 12;
            if (!string.IsNullOrEmpty(currentRole.ColorHex) && ColorUtility.TryParseHtmlString(currentRole.ColorHex, out Color parsedColor))
            {
                roleTxt.color = parsedColor;
            }
            else if (currentRole.Level >= 2)
            {
                roleTxt.color = WardColorPalette.GoldAmber;
            }
            else
            {
                roleTxt.color = WardColorPalette.IceBlue;
            }
            roleTxt.verticalOverflow = VerticalWrapMode.Overflow;
            roleTxt.horizontalOverflow = HorizontalWrapMode.Overflow;
            roleTxt.alignment = TextAnchor.MiddleCenter;
        }

        Button roleBtn = roleBtnObj.GetComponent<Button>();
        roleBtn.interactable = canChangeRole;
        if (!canChangeRole && roleTxt != null && !canManage)
        {
            roleTxt.color = WardColorPalette.TextMuted;
        }

        roleBtn.onClick.AddListener(() =>
        {
            int currIdx = -1;
            for (int i = 0; i < assignableRoles.Count; i++)
            {
                if (assignableRoles[i].Id == entry.RoleId)
                {
                    currIdx = i;
                    break;
                }
            }
            if (currIdx == -1)
            {
                for (int i = 0; i < assignableRoles.Count; i++)
                {
                    if (assignableRoles[i].Level == currentRole.Level)
                    {
                        currIdx = i;
                        break;
                    }
                }
            }
            int nextIdx = currIdx >= 0 ? (currIdx + 1) % assignableRoles.Count : 0;
            WardRoleDefinition nextRole = assignableRoles[nextIdx];
            SetPlayerRole(entry.PlayerId, entry.PlayerName, nextRole.Id);
        });

        if (canRemove)
        {
            GameObject removeBtn = GUIManager.Instance.CreateButton(
                "$wmb_roles_remove".Localize(),
                ctrlContainer,
                Vector2.zero,
                Vector2.zero,
                Vector2.zero,
                75f,
                28f);
            removeBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(75f, 28f);
            removeBtn.SetLayoutDimensions(75f, 28f);

            Text rmTxt = removeBtn.GetComponentInChildren<Text>();
            if (rmTxt != null)
            {
                rmTxt.font = GUIManager.Instance.AveriaSerifBold;
                rmTxt.fontSize = 11;
                rmTxt.color = WardColorPalette.DangerRed;
                rmTxt.verticalOverflow = VerticalWrapMode.Overflow;
                rmTxt.horizontalOverflow = HorizontalWrapMode.Overflow;
                rmTxt.alignment = TextAnchor.MiddleCenter;
            }

            removeBtn.GetComponent<Button>().onClick.AddListener(() => RemovePlayerFromRoster(entry.PlayerId));
        }
    }

    private void AddDropdownPlayerToRole(int targetRoleId)
    {
        if (_ward == null || _ward.m_nview == null || !_ward.m_nview.IsValid())
            return;

        if (_knownPlayerDropdown == null)
            return;

        int selectedIndex = _knownPlayerDropdown.value;
        if (selectedIndex <= 0 || selectedIndex - 1 >= _dropdownPlayers.Count)
            return;

        KeyValuePair<long, string> player = _dropdownPlayers[selectedIndex - 1];
        SetPlayerRole(player.Key, player.Value, targetRoleId);
    }

    private void AddManualPlayerToRole(int targetRoleId)
    {
        if (_ward == null || _ward.m_nview == null || !_ward.m_nview.IsValid())
            return;

        if (_addPlayerInput == null)
            return;

        string input = _addPlayerInput.text.Trim();
        if (string.IsNullOrEmpty(input))
            return;

        long playerId = 0L;
        string playerName = input;

        List<KeyValuePair<long, string>> known = PlayerRegistryService.GetKnownPlayers();
        KeyValuePair<long, string> matched = known.FirstOrDefault(p => p.Value.Equals(input, System.StringComparison.OrdinalIgnoreCase));

        if (matched.Key != 0L)
        {
            playerId = matched.Key;
            playerName = matched.Value;
        }
        else if (long.TryParse(input, out long parsedId))
        {
            playerId = parsedId;
            playerName = $"Player_{parsedId}";
        }
        else if (Player.m_localPlayer != null && input.Equals(Player.m_localPlayer.GetPlayerName(), System.StringComparison.OrdinalIgnoreCase))
        {
            playerId = Player.m_localPlayer.GetPlayerID();
            playerName = Player.m_localPlayer.GetPlayerName();
        }

        _addPlayerInput.text = string.Empty;
        SetPlayerRole(playerId, playerName, targetRoleId);
    }

    private void SetPlayerRole(long playerId, string playerName, int targetRoleId)
    {
        if (_ward == null || _ward.m_nview == null || !_ward.m_nview.IsValid())
            return;

        ZDO zdo = _ward.m_nview.GetZDO();
        ZDO effectiveZdo = WardQueryService.GetEffectiveWardZdo(zdo) ?? zdo;

        WardRoleAssignments assignments = WardRoleManager.GetRoleAssignments(effectiveZdo);
        assignments.SetRole(playerId, playerName, targetRoleId);
        WardRoleManager.SaveRoleAssignments(effectiveZdo, assignments);

        string serialized = assignments.Serialize();
        WardNetworkRouter.SendRoleAssignments(effectiveZdo.m_uid, serialized);

        WardMeBruh.Log.Info($"[TabRoles] Set player {playerName} (ID: {playerId}) to role {targetRoleId}");
        Refresh();
    }

    private void RemovePlayerFromRoster(long playerId)
    {
        if (_ward == null || _ward.m_nview == null || !_ward.m_nview.IsValid())
            return;

        ZDO zdo = _ward.m_nview.GetZDO();
        ZDO effectiveZdo = WardQueryService.GetEffectiveWardZdo(zdo) ?? zdo;

        WardRoleAssignments assignments = WardRoleManager.GetRoleAssignments(effectiveZdo);
        assignments.Remove(playerId);
        WardRoleManager.SaveRoleAssignments(effectiveZdo, assignments);

        string serialized = assignments.Serialize();
        WardNetworkRouter.SendRoleAssignments(effectiveZdo.m_uid, serialized);

        WardMeBruh.Log.Info($"[TabRoles] Removed player ID: {playerId} from roster");
        Refresh();
    }
}
