using System.Collections.Generic;
using System.Linq;
using Jotunn.Managers;
using UnityEngine;
using UnityEngine.UI;
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
        long playerId = Player.m_localPlayer != null ? Player.m_localPlayer.GetPlayerID() : 0L;
        bool canManage = WardPermissions.CanManageWard(zdo, playerId);
        bool isOwner = WardPermissions.GetPlayerRole(zdo, playerId) == WardRole.Owner;

        Transform hierarchyCard = WardUIFactory.CreateCard(_listContainer, "Clearance Tier Privileges");
        _rosterCards.Add(hierarchyCard.parent.parent.gameObject);

        WardUIFactory.CreateBadgeRow(hierarchyCard, "Tier 3: Owner", "Master Control, Ownership Transfer, Role Grants", WardColorPalette.GoldAmber, out _);
        WardUIFactory.CreateBadgeRow(hierarchyCard, "Tier 2: Manager", "Ward Configuration, Roster Management, Door & Chest Locks", WardColorPalette.GoldAmber, out _);
        WardUIFactory.CreateBadgeRow(hierarchyCard, "Tier 1: Resident", "Access Doors & Chests, Fast-Travel, Harvesting Crops", WardColorPalette.IceBlue, out _);
        WardUIFactory.CreateBadgeRow(hierarchyCard, "Tier 0: Guest", "Passive Sanctuary, No Storage or Gate Access", WardColorPalette.TextMuted, out _);

        string managersData = zdo.GetString(WardZdoKeys.RolesManagers, string.Empty);
        WardRoster managersRoster = WardRoster.Deserialize(managersData);

        string residentsData = zdo.GetString(WardZdoKeys.RolesResidents, string.Empty);
        WardRoster residentsRoster = WardRoster.Deserialize(residentsData);

        long ownerId = zdo.GetLong(ZDOVars.s_creator, 0L);

        if (canManage)
        {
            Transform addCard = WardUIFactory.CreateCard(_listContainer, "Enroll Player to Roster");
            _rosterCards.Add(addCard.parent.parent.gameObject);

            List<KeyValuePair<long, string>> allKnown = PlayerRegistryService.GetKnownPlayers();
            List<KeyValuePair<long, string>> knownPlayers = new();
            for (int i = 0; i < allKnown.Count; i++)
            {
                KeyValuePair<long, string> kp = allKnown[i];
                if (kp.Key == ownerId)
                    continue;
                if (managersRoster.Contains(kp.Key) || residentsRoster.Contains(kp.Key))
                    continue;
                knownPlayers.Add(kp);
            }

            _dropdownPlayers.Clear();

            WardUIFactory.CreateSettingRow(addCard, "Known World Characters", out Transform dropCtrlContainer);

            GameObject dropdownObj = WardUIExtensions.CreateValheimDropdown(dropCtrlContainer, 210f, 28f);
            dropdownObj.GetComponent<RectTransform>().sizeDelta = new Vector2(210f, 28f);
            dropdownObj.SetLayoutDimensions(210f, 28f);
            _knownPlayerDropdown = dropdownObj.GetComponent<Dropdown>();
            _knownPlayerDropdown.ClearOptions();

            GameObject dropResBtn = GUIManager.Instance.CreateButton(
                "+ RESIDENT",
                dropCtrlContainer,
                Vector2.zero,
                Vector2.zero,
                Vector2.zero,
                95f,
                28f);
            dropResBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(95f, 28f);
            dropResBtn.SetLayoutDimensions(95f, 28f);
            dropResBtn.GetComponent<Button>().onClick.AddListener(() => AddDropdownPlayerToRole(WardRole.Resident));

            GameObject dropMgrBtn = null;
            if (isOwner)
            {
                dropMgrBtn = GUIManager.Instance.CreateButton(
                    "+ MANAGER",
                    dropCtrlContainer,
                    Vector2.zero,
                    Vector2.zero,
                    Vector2.zero,
                    95f,
                    28f);
                dropMgrBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(95f, 28f);
                dropMgrBtn.SetLayoutDimensions(95f, 28f);
                dropMgrBtn.GetComponent<Button>().onClick.AddListener(() => AddDropdownPlayerToRole(WardRole.Manager));
            }

            if (knownPlayers.Count == 0)
            {
                _knownPlayerDropdown.interactable = false;
                _knownPlayerDropdown.AddOptions(new List<Dropdown.OptionData> { new Dropdown.OptionData("No Characters") });
                _knownPlayerDropdown.value = 0;

                dropResBtn.GetComponent<Button>().interactable = false;
                Text resTxt = dropResBtn.GetComponentInChildren<Text>();
                if (resTxt != null)
                    resTxt.color = WardColorPalette.TextMuted;

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
                options.Add(new Dropdown.OptionData("-- Select Character --"));

                for (int i = 0; i < knownPlayers.Count; i++)
                {
                    _dropdownPlayers.Add(knownPlayers[i]);
                    options.Add(new Dropdown.OptionData(knownPlayers[i].Value));
                }
                _knownPlayerDropdown.AddOptions(options);
                _knownPlayerDropdown.value = 0;

                dropResBtn.GetComponent<Button>().interactable = true;
                Text resTxt = dropResBtn.GetComponentInChildren<Text>();
                if (resTxt != null)
                    resTxt.color = WardColorPalette.ValheimOrange;

                if (dropMgrBtn != null)
                {
                    dropMgrBtn.GetComponent<Button>().interactable = true;
                    Text mgrTxt = dropMgrBtn.GetComponentInChildren<Text>();
                    if (mgrTxt != null)
                        mgrTxt.color = WardColorPalette.ValheimOrange;
                }
            }

            WardUIFactory.CreateSettingRow(addCard, "Manual Entry (Unlisted)", out Transform inputCtrlContainer);

            GameObject inputObj = GUIManager.Instance.CreateInputField(
                inputCtrlContainer,
                Vector2.zero,
                Vector2.zero,
                Vector2.zero,
                InputField.ContentType.Standard,
                "Enter character name...",
                14,
                210f,
                28f);
            inputObj.GetComponent<RectTransform>().sizeDelta = new Vector2(210f, 28f);
            inputObj.SetLayoutDimensions(210f, 28f);
            _addPlayerInput = inputObj.GetComponent<InputField>();

            GameObject addResBtn = GUIManager.Instance.CreateButton(
                "+ RESIDENT",
                inputCtrlContainer,
                Vector2.zero,
                Vector2.zero,
                Vector2.zero,
                95f,
                28f);
            addResBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(95f, 28f);
            addResBtn.SetLayoutDimensions(95f, 28f);
            addResBtn.GetComponent<Button>().onClick.AddListener(() => AddManualPlayerToRole(WardRole.Resident));

            if (isOwner)
            {
                GameObject addMgrBtn = GUIManager.Instance.CreateButton(
                    "+ MANAGER",
                    inputCtrlContainer,
                    Vector2.zero,
                    Vector2.zero,
                    Vector2.zero,
                    95f,
                    28f);
                addMgrBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(95f, 28f);
                addMgrBtn.SetLayoutDimensions(95f, 28f);
                addMgrBtn.GetComponent<Button>().onClick.AddListener(() => AddManualPlayerToRole(WardRole.Manager));
            }
        }



        int totalCount = managersRoster.Entries.Count + residentsRoster.Entries.Count;
        Transform rosterCard = WardUIFactory.CreateCard(_listContainer, $"Permitted Roster ({totalCount} Members)");
        _rosterCards.Add(rosterCard.parent.parent.gameObject);

        if (totalCount == 0)
        {
            WardUIFactory.CreateBadgeRow(rosterCard, "No players currently permitted", "-", WardColorPalette.TextMuted, out _);
        }
        else
        {
            for (int i = 0; i < managersRoster.Entries.Count; i++)
            {
                CreateRosterRow(rosterCard, managersRoster.Entries[i], WardRole.Manager, canManage, isOwner);
            }

            for (int i = 0; i < residentsRoster.Entries.Count; i++)
            {
                CreateRosterRow(rosterCard, residentsRoster.Entries[i], WardRole.Resident, canManage, isOwner);
            }
        }
    }

    private void CreateRosterRow(Transform parent, RosterEntry entry, WardRole role, bool canManage, bool isOwner)
    {
        string labelText = $"{entry.PlayerName} (ID: {entry.PlayerId})";
        WardUIFactory.CreateSettingRow(parent, labelText, out Transform ctrlContainer);

        GameObject roleBadgeObj = new GameObject("RoleBadge", typeof(RectTransform), typeof(Text), typeof(LayoutElement));
        roleBadgeObj.transform.SetParent(ctrlContainer, false);
        roleBadgeObj.SetLayoutDimensions(100f, 28f);

        Text roleBadge = roleBadgeObj.GetComponent<Text>();
        roleBadge.text = $"[{role}]";
        roleBadge.font = GUIManager.Instance.AveriaSerifBold;
        roleBadge.fontSize = 13;
        roleBadge.color = role == WardRole.Manager ? WardColorPalette.GoldAmber : WardColorPalette.IceBlue;
        roleBadge.alignment = TextAnchor.MiddleRight;

        if (isOwner && role == WardRole.Resident)
        {
            GameObject promoteBtn = GUIManager.Instance.CreateButton(
                "Promote", 
                ctrlContainer, 
                Vector2.zero, 
                Vector2.zero, 
                Vector2.zero, 
                80f, 
                26f);
            promoteBtn.SetLayoutDimensions(80f, 26f);
            promoteBtn.GetComponent<Button>().onClick.AddListener(() => PromoteToManager(entry));
        }
        else if (isOwner && role == WardRole.Manager)
        {
            GameObject demoteBtn = GUIManager.Instance.CreateButton(
                "Demote", 
                ctrlContainer, 
                Vector2.zero, 
                Vector2.zero, 
                Vector2.zero, 
                80f, 
                26f);
            demoteBtn.SetLayoutDimensions(80f, 26f);
            demoteBtn.GetComponent<Button>().onClick.AddListener(() => DemoteToResident(entry));
        }

        if (canManage)
        {
            GameObject removeBtn = GUIManager.Instance.CreateButton(
                "Remove", 
                ctrlContainer, 
                Vector2.zero, 
                Vector2.zero, 
                Vector2.zero, 
                80f, 
                26f);
            removeBtn.SetLayoutDimensions(80f, 26f);
            Text rmTxt = removeBtn.GetComponentInChildren<Text>();
            if (rmTxt != null)
            {
                rmTxt.color = WardColorPalette.DangerRed;
            }
            removeBtn.GetComponent<Button>().onClick.AddListener(() => RemoveFromRoster(entry, role));
        }
    }

    private void AddDropdownPlayerToRole(WardRole targetRole)
    {
        if (_ward == null || _ward.m_nview == null || !_ward.m_nview.IsValid())
            return;

        if (_knownPlayerDropdown == null)
            return;

        int selectedIndex = _knownPlayerDropdown.value;
        if (selectedIndex <= 0 || selectedIndex - 1 >= _dropdownPlayers.Count)
            return;

        KeyValuePair<long, string> player = _dropdownPlayers[selectedIndex - 1];
        long playerId = player.Key;
        string playerName = player.Value;

        ZDO zdo = _ward.m_nview.GetZDO();
        int key = targetRole == WardRole.Manager ? WardZdoKeys.RolesManagers : WardZdoKeys.RolesResidents;
        string currentData = zdo.GetString(key, string.Empty);
        WardRoster roster = WardRoster.Deserialize(currentData);

        roster.AddOrUpdate(playerId, playerName);
        zdo.Set(key, roster.Serialize());

        WardMeBruh.Log.Debug($"[TabRoles] Enrolled dropdown character {playerName} (ID: {playerId}) to {targetRole}");
        Refresh();
    }

    private void AddManualPlayerToRole(WardRole targetRole)
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

        ZDO zdo = _ward.m_nview.GetZDO();
        int key = targetRole == WardRole.Manager ? WardZdoKeys.RolesManagers : WardZdoKeys.RolesResidents;
        string currentData = zdo.GetString(key, string.Empty);
        WardRoster roster = WardRoster.Deserialize(currentData);

        roster.AddOrUpdate(playerId, playerName);
        zdo.Set(key, roster.Serialize());

        _addPlayerInput.text = string.Empty;
        WardMeBruh.Log.Debug($"[TabRoles] Enrolled manual character {playerName} (ID: {playerId}) to {targetRole}");
        Refresh();
    }

    private void PromoteToManager(RosterEntry entry)
    {
        if (_ward == null || _ward.m_nview == null || !_ward.m_nview.IsValid())
            return;

        ZDO zdo = _ward.m_nview.GetZDO();

        WardRoster res = WardRoster.Deserialize(zdo.GetString(WardZdoKeys.RolesResidents, string.Empty));
        res.Remove(entry.PlayerId);
        zdo.Set(WardZdoKeys.RolesResidents, res.Serialize());

        WardRoster mgr = WardRoster.Deserialize(zdo.GetString(WardZdoKeys.RolesManagers, string.Empty));
        mgr.AddOrUpdate(entry.PlayerId, entry.PlayerName);
        zdo.Set(WardZdoKeys.RolesManagers, mgr.Serialize());

        Refresh();
    }

    private void DemoteToResident(RosterEntry entry)
    {
        if (_ward == null || _ward.m_nview == null || !_ward.m_nview.IsValid())
            return;

        ZDO zdo = _ward.m_nview.GetZDO();

        WardRoster mgr = WardRoster.Deserialize(zdo.GetString(WardZdoKeys.RolesManagers, string.Empty));
        mgr.Remove(entry.PlayerId);
        zdo.Set(WardZdoKeys.RolesManagers, mgr.Serialize());

        WardRoster res = WardRoster.Deserialize(zdo.GetString(WardZdoKeys.RolesResidents, string.Empty));
        res.AddOrUpdate(entry.PlayerId, entry.PlayerName);
        zdo.Set(WardZdoKeys.RolesResidents, res.Serialize());

        Refresh();
    }

    private void RemoveFromRoster(RosterEntry entry, WardRole role)
    {
        if (_ward == null || _ward.m_nview == null || !_ward.m_nview.IsValid())
            return;

        ZDO zdo = _ward.m_nview.GetZDO();
        int key = role == WardRole.Manager ? WardZdoKeys.RolesManagers : WardZdoKeys.RolesResidents;

        WardRoster roster = WardRoster.Deserialize(zdo.GetString(key, string.Empty));
        roster.Remove(entry.PlayerId);
        zdo.Set(key, roster.Serialize());

        Refresh();
    }
}
