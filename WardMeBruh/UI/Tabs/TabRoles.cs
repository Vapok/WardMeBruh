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

            GameObject dropResBtn = GUIManager.Instance.CreateButton(
                "$wmb_roles_add_resident".Localize(),
                dropCtrlContainer,
                Vector2.zero,
                Vector2.zero,
                Vector2.zero,
                100f,
                28f);
            dropResBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(100f, 28f);
            dropResBtn.SetLayoutDimensions(100f, 28f);
            Text dropResTxt = dropResBtn.GetComponentInChildren<Text>();
            if (dropResTxt != null)
            {
                dropResTxt.fontSize = 11;
                dropResTxt.verticalOverflow = VerticalWrapMode.Overflow;
                dropResTxt.horizontalOverflow = HorizontalWrapMode.Overflow;
                dropResTxt.alignment = TextAnchor.MiddleCenter;
            }
            dropResBtn.GetComponent<Button>().onClick.AddListener(() => AddDropdownPlayerToRole((int)WardRole.Resident));

            GameObject dropMgrBtn = null;
            if (isOwner)
            {
                dropMgrBtn = GUIManager.Instance.CreateButton(
                    "$wmb_roles_add_manager".Localize(),
                    dropCtrlContainer,
                    Vector2.zero,
                    Vector2.zero,
                    Vector2.zero,
                    100f,
                    28f);
                dropMgrBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(100f, 28f);
                dropMgrBtn.SetLayoutDimensions(100f, 28f);
                Text dropMgrTxt = dropMgrBtn.GetComponentInChildren<Text>();
                if (dropMgrTxt != null)
                {
                    dropMgrTxt.fontSize = 11;
                    dropMgrTxt.verticalOverflow = VerticalWrapMode.Overflow;
                    dropMgrTxt.horizontalOverflow = HorizontalWrapMode.Overflow;
                    dropMgrTxt.alignment = TextAnchor.MiddleCenter;
                }
                dropMgrBtn.GetComponent<Button>().onClick.AddListener(() => AddDropdownPlayerToRole((int)WardRole.Manager));
            }

            if (knownPlayers.Count == 0)
            {
                _knownPlayerDropdown.interactable = false;
                _knownPlayerDropdown.AddOptions(new List<Dropdown.OptionData> { new Dropdown.OptionData("$wmb_roles_no_characters".Localize()) });
                _knownPlayerDropdown.value = 0;
                _knownPlayerDropdown.RefreshShownValue();

                dropResBtn.GetComponent<Button>().interactable = false;
                if (dropResTxt != null)
                    dropResTxt.color = WardColorPalette.TextMuted;

                if (dropMgrBtn != null)
                {
                    dropMgrBtn.GetComponent<Button>().interactable = false;
                    Text mgrTxt = dropMgrBtn.GetComponentInChildren<Text>();
                    if (mgrTxt != null)
                        mgrTxt.color = WardColorPalette.TextMuted;
                }
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

                dropResBtn.GetComponent<Button>().interactable = false;
                if (dropResTxt != null)
                    dropResTxt.color = WardColorPalette.TextMuted;

                if (dropMgrBtn != null)
                {
                    dropMgrBtn.GetComponent<Button>().interactable = false;
                    Text mgrTxt = dropMgrBtn.GetComponentInChildren<Text>();
                    if (mgrTxt != null)
                        mgrTxt.color = WardColorPalette.TextMuted;
                }

                _knownPlayerDropdown.onValueChanged.AddListener((int index) =>
                {
                    bool hasSelection = index > 0 && index - 1 < _dropdownPlayers.Count;
                    dropResBtn.GetComponent<Button>().interactable = hasSelection;
                    if (dropResTxt != null)
                        dropResTxt.color = hasSelection ? WardColorPalette.ValheimOrange : WardColorPalette.TextMuted;

                    if (dropMgrBtn != null)
                    {
                        dropMgrBtn.GetComponent<Button>().interactable = hasSelection;
                        Text mgrTxt = dropMgrBtn.GetComponentInChildren<Text>();
                        if (mgrTxt != null)
                            mgrTxt.color = hasSelection ? WardColorPalette.ValheimOrange : WardColorPalette.TextMuted;
                    }

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

            GameObject addResBtn = GUIManager.Instance.CreateButton(
                "$wmb_roles_add_resident".Localize(),
                inputCtrlContainer,
                Vector2.zero,
                Vector2.zero,
                Vector2.zero,
                100f,
                28f);
            addResBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(100f, 28f);
            addResBtn.SetLayoutDimensions(100f, 28f);
            Text addResTxt = addResBtn.GetComponentInChildren<Text>();
            if (addResTxt != null)
            {
                addResTxt.fontSize = 11;
                addResTxt.verticalOverflow = VerticalWrapMode.Overflow;
                addResTxt.horizontalOverflow = HorizontalWrapMode.Overflow;
                addResTxt.alignment = TextAnchor.MiddleCenter;
            }
            addResBtn.GetComponent<Button>().onClick.AddListener(() => AddManualPlayerToRole((int)WardRole.Resident));

            if (isOwner)
            {
                GameObject addMgrBtn = GUIManager.Instance.CreateButton(
                    "$wmb_roles_add_manager".Localize(),
                    inputCtrlContainer,
                    Vector2.zero,
                    Vector2.zero,
                    Vector2.zero,
                    100f,
                    28f);
                addMgrBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(100f, 28f);
                addMgrBtn.SetLayoutDimensions(100f, 28f);
                Text addMgrTxt = addMgrBtn.GetComponentInChildren<Text>();
                if (addMgrTxt != null)
                {
                    addMgrTxt.fontSize = 11;
                    addMgrTxt.verticalOverflow = VerticalWrapMode.Overflow;
                    addMgrTxt.horizontalOverflow = HorizontalWrapMode.Overflow;
                    addMgrTxt.alignment = TextAnchor.MiddleCenter;
                }
                addMgrBtn.GetComponent<Button>().onClick.AddListener(() => AddManualPlayerToRole((int)WardRole.Manager));
            }
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

        int currentIndex = -1;
        for (int i = 0; i < rosterRoles.Count; i++)
        {
            if (rosterRoles[i].Id == entry.RoleId)
            {
                currentIndex = i;
                break;
            }
        }
        if (currentIndex == -1)
        {
            for (int i = 0; i < rosterRoles.Count; i++)
            {
                if (rosterRoles[i].Level == currentRole.Level)
                {
                    currentIndex = i;
                    break;
                }
            }
        }
        if (currentIndex == -1 && rosterRoles.Count > 0)
        {
            currentIndex = 0;
        }

        WardRoleDefinition prevRole = (currentIndex > 0) ? rosterRoles[currentIndex - 1] : null;
        WardRoleDefinition nextRole = (currentIndex >= 0 && currentIndex < rosterRoles.Count - 1) ? rosterRoles[currentIndex + 1] : null;

        bool canDemote = prevRole != null && (isOwner || (canManage && callerLevel > currentRole.Level));
        bool canPromote = nextRole != null && (isOwner || (canManage && callerLevel > currentRole.Level && callerLevel > nextRole.Level));
        bool canRemove = canManage && (isOwner || callerLevel > currentRole.Level);

        GameObject roleBadgeObj = new GameObject("RoleBadge", typeof(RectTransform), typeof(Text), typeof(LayoutElement));
        roleBadgeObj.transform.SetParent(ctrlContainer, false);
        float badgeWidth = Mathf.Clamp(currentRole.Name.Length * 9f + 24f, 80f, 150f);
        roleBadgeObj.SetLayoutDimensions(badgeWidth, 28f);

        Color badgeColor = WardColorPalette.IceBlue;
        if (!string.IsNullOrEmpty(currentRole.ColorHex) && ColorUtility.TryParseHtmlString(currentRole.ColorHex, out Color parsedColor))
        {
            badgeColor = parsedColor;
        }
        else if (currentRole.Level >= 2)
        {
            badgeColor = WardColorPalette.GoldAmber;
        }

        Text roleBadge = roleBadgeObj.GetComponent<Text>();
        roleBadge.text = $"[{currentRole.Name.Localize()}]";
        roleBadge.font = GUIManager.Instance.AveriaSerifBold;
        roleBadge.fontSize = 13;
        roleBadge.color = badgeColor;
        roleBadge.alignment = TextAnchor.MiddleRight;

        if (canDemote)
        {
            string demoteText = $"{"$wmb_roles_demote".Localize()} ({prevRole.Name.Localize()})";
            float demoteWidth = Mathf.Clamp(demoteText.Length * 7.5f + 16f, 85f, 175f);

            GameObject demoteBtn = GUIManager.Instance.CreateButton(
                demoteText,
                ctrlContainer,
                Vector2.zero,
                Vector2.zero,
                Vector2.zero,
                demoteWidth,
                26f);
            demoteBtn.SetLayoutDimensions(demoteWidth, 26f);

            Text demTxt = demoteBtn.GetComponentInChildren<Text>();
            if (demTxt != null)
            {
                demTxt.font = GUIManager.Instance.AveriaSerifBold;
                demTxt.fontSize = 11;
                demTxt.color = WardColorPalette.ValheimOrange;
                demTxt.verticalOverflow = VerticalWrapMode.Overflow;
                demTxt.horizontalOverflow = HorizontalWrapMode.Overflow;
                demTxt.alignment = TextAnchor.MiddleCenter;
            }

            int targetRoleId = prevRole.Id;
            demoteBtn.GetComponent<Button>().onClick.AddListener(() => SetPlayerRole(entry.PlayerId, entry.PlayerName, targetRoleId));
        }

        if (canPromote)
        {
            string promoteText = $"{"$wmb_roles_promote".Localize()} ({nextRole.Name.Localize()})";
            float promoteWidth = Mathf.Clamp(promoteText.Length * 7.5f + 16f, 85f, 175f);

            GameObject promoteBtn = GUIManager.Instance.CreateButton(
                promoteText,
                ctrlContainer,
                Vector2.zero,
                Vector2.zero,
                Vector2.zero,
                promoteWidth,
                26f);
            promoteBtn.SetLayoutDimensions(promoteWidth, 26f);

            Text promTxt = promoteBtn.GetComponentInChildren<Text>();
            if (promTxt != null)
            {
                promTxt.font = GUIManager.Instance.AveriaSerifBold;
                promTxt.fontSize = 11;
                promTxt.color = WardColorPalette.ValheimOrange;
                promTxt.verticalOverflow = VerticalWrapMode.Overflow;
                promTxt.horizontalOverflow = HorizontalWrapMode.Overflow;
                promTxt.alignment = TextAnchor.MiddleCenter;
            }

            int targetRoleId = nextRole.Id;
            promoteBtn.GetComponent<Button>().onClick.AddListener(() => SetPlayerRole(entry.PlayerId, entry.PlayerName, targetRoleId));
        }

        if (canRemove)
        {
            GameObject removeBtn = GUIManager.Instance.CreateButton(
                "$wmb_roles_remove".Localize(),
                ctrlContainer,
                Vector2.zero,
                Vector2.zero,
                Vector2.zero,
                75f,
                26f);
            removeBtn.SetLayoutDimensions(75f, 26f);

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
