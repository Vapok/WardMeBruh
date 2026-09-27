using System.Collections.Generic;
using Jotunn.Managers;
using UnityEngine;
using UnityEngine.UI;
using WardMeBruh.Configuration;
using WardMeBruh.Models;
using WardMeBruh.Networking;
using WardMeBruh.Services;

namespace WardMeBruh.UI.Tabs;

public class TabManagement
{
    private GameObject _root;
    private PrivateArea _ward;
    private Transform _leftContainer;
    private Transform _rightContainer;

    private int _selectedRoleId = (int)WardRole.Manager;
    private readonly List<GameObject> _roleButtons = new();
    private readonly List<GameObject> _permRows = new();

    public GameObject Root => _root;

    public void Build(Transform parent)
    {
        _root = new GameObject("Tab_Management", typeof(RectTransform));
        _root.transform.SetParent(parent, false);
        RectTransform rt = _root.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        GameObject leftPanel = new GameObject("LeftPanel", typeof(RectTransform));
        leftPanel.transform.SetParent(_root.transform, false);
        RectTransform leftRt = leftPanel.GetComponent<RectTransform>();
        leftRt.anchorMin = new Vector2(0f, 0f);
        leftRt.anchorMax = new Vector2(0f, 1f);
        leftRt.pivot = new Vector2(0f, 0.5f);
        leftRt.offsetMin = new Vector2(0f, 0f);
        leftRt.offsetMax = new Vector2(230f, 0f);

        WardUIFactory.CreateScrollArea(leftPanel.transform, out _leftContainer);

        GameObject rightPanel = new GameObject("RightPanel", typeof(RectTransform));
        rightPanel.transform.SetParent(_root.transform, false);
        RectTransform rightRt = rightPanel.GetComponent<RectTransform>();
        rightRt.anchorMin = new Vector2(0f, 0f);
        rightRt.anchorMax = new Vector2(1f, 1f);
        rightRt.pivot = new Vector2(1f, 0.5f);
        rightRt.offsetMin = new Vector2(240f, 0f);
        rightRt.offsetMax = new Vector2(0f, 0f);

        WardUIFactory.CreateScrollArea(rightPanel.transform, out _rightContainer);
    }

    public void Bind(PrivateArea ward)
    {
        _ward = ward;
        Refresh();
    }

    public void Refresh()
    {
        ScrollRect rightSr = _rightContainer != null ? _rightContainer.GetComponentInParent<ScrollRect>() : null;
        float rightScrollY = rightSr != null ? rightSr.verticalNormalizedPosition : 1f;

        ClearLeft();
        ClearRight();

        if (_ward == null || _ward.m_nview == null || !_ward.m_nview.IsValid())
            return;

        ZDO zdo = _ward.m_nview.GetZDO();
        long playerId = Player.m_localPlayer != null ? Player.m_localPlayer.GetPlayerID() : 0L;
        bool canManageGovernance = WardRoleManager.CanPerform(zdo, playerId, "personnel.governance") || WardPermissions.IsAdmin(playerId);

        List<WardRoleDefinition> roles = WardRoleManager.GetRoleDefinitions(zdo);

        bool roleExists = false;
        for (int i = 0; i < roles.Count; i++)
        {
            if (roles[i].Id == _selectedRoleId)
            {
                roleExists = true;
                break;
            }
        }
        if (!roleExists && roles.Count > 0)
        {
            _selectedRoleId = roles[roles.Count - 1].Id;
        }

        BuildRoleList(roles, canManageGovernance);
        BuildPermissionsView(roles, canManageGovernance);

        if (rightSr != null && WardMainWindow.Instance != null)
        {
            WardMainWindow.Instance.StartCoroutine(RestoreScroll(rightSr, rightScrollY));
        }
    }

    private static System.Collections.IEnumerator RestoreScroll(ScrollRect sr, float pos)
    {
        yield return null;
        if (sr != null)
        {
            sr.verticalNormalizedPosition = pos;
        }
    }

    private void BuildRoleList(List<WardRoleDefinition> roles, bool canManageGovernance)
    {
        Transform card = WardUIFactory.CreateCard(_leftContainer, "Ward Hierarchy");
        _roleButtons.Add(card.parent.parent.gameObject);

        List<WardRoleDefinition> sortedRoles = new(roles);
        sortedRoles.Sort((a, b) => b.Level.CompareTo(a.Level));

        for (int i = 0; i < sortedRoles.Count; i++)
        {
            WardRoleDefinition role = sortedRoles[i];
            int roleId = role.Id;
            bool isSelected = (role.Id == _selectedRoleId);

            string roleIcon = role.Id switch
            {
                (int)WardRole.Owner => "👑",
                (int)WardRole.Manager => "⚔️",
                (int)WardRole.Resident => "🌲",
                (int)WardRole.Guest => "👤",
                _ => "🛡️"
            };

            string title = $"{roleIcon} {role.Name} (Tier {role.Level})";
            GameObject btnObj = GUIManager.Instance.CreateButton(
                title,
                card,
                Vector2.zero,
                Vector2.zero,
                Vector2.zero,
                200f,
                32f);

            LayoutElement le = btnObj.AddComponent<LayoutElement>();
            le.minHeight = 32f;
            le.preferredHeight = 32f;
            le.flexibleWidth = 1f;

            Button btn = btnObj.GetComponent<Button>();
            Image btnImg = btnObj.GetComponent<Image>();
            Text btnTxt = btnObj.GetComponentInChildren<Text>();

            if (isSelected)
            {
                btnImg.color = WardColorPalette.ValheimOrange;
                if (btnTxt != null) btnTxt.color = Color.black;
            }
            else
            {
                btnImg.color = WardColorPalette.CardBorder;
                if (btnTxt != null) btnTxt.color = WardColorPalette.TextNormal;
            }

            btn.onClick.AddListener(() =>
            {
                _selectedRoleId = roleId;
                Refresh();
            });
        }

        if (canManageGovernance)
        {
            Transform actionsCard = WardUIFactory.CreateCard(_leftContainer, "Role Actions");
            _roleButtons.Add(actionsCard.parent.parent.gameObject);

            GameObject addBtnObj = GUIManager.Instance.CreateButton(
                "+ Add Custom Role",
                actionsCard,
                Vector2.zero,
                Vector2.zero,
                Vector2.zero,
                200f,
                30f);

            LayoutElement addLe = addBtnObj.AddComponent<LayoutElement>();
            addLe.minHeight = 30f;
            addLe.preferredHeight = 30f;
            addLe.flexibleWidth = 1f;

            Button addBtn = addBtnObj.GetComponent<Button>();
            addBtn.onClick.AddListener(() => OnAddCustomRole(roles));

            if (_selectedRoleId >= 100)
            {
                GameObject renameBtnObj = GUIManager.Instance.CreateButton(
                    "✏️ Rename Role",
                    actionsCard,
                    Vector2.zero,
                    Vector2.zero,
                    Vector2.zero,
                    200f,
                    28f);

                LayoutElement renLe = renameBtnObj.AddComponent<LayoutElement>();
                renLe.minHeight = 28f;
                renLe.preferredHeight = 28f;
                renLe.flexibleWidth = 1f;

                Button renBtn = renameBtnObj.GetComponent<Button>();
                renBtn.onClick.AddListener(() => OnRenameCustomRole(roles));

                GameObject delBtnObj = GUIManager.Instance.CreateButton(
                    "🗑️ Delete Role",
                    actionsCard,
                    Vector2.zero,
                    Vector2.zero,
                    Vector2.zero,
                    200f,
                    28f);

                LayoutElement delLe = delBtnObj.AddComponent<LayoutElement>();
                delLe.minHeight = 28f;
                delLe.preferredHeight = 28f;
                delLe.flexibleWidth = 1f;

                Button delBtn = delBtnObj.GetComponent<Button>();
                Image delImg = delBtnObj.GetComponent<Image>();
                if (delImg != null) delImg.color = new Color(0.5f, 0.15f, 0.15f);
                delBtn.onClick.AddListener(() => OnDeleteCustomRole(roles));
            }
        }
    }

    private void BuildPermissionsView(List<WardRoleDefinition> roles, bool canManageGovernance)
    {
        WardRoleDefinition selectedRole = null;
        for (int i = 0; i < roles.Count; i++)
        {
            if (roles[i].Id == _selectedRoleId)
            {
                selectedRole = roles[i];
                break;
            }
        }

        if (selectedRole == null)
            return;

        WardCapabilityCategory[] categories =
        {
            WardCapabilityCategory.Core,
            WardCapabilityCategory.Access,
            WardCapabilityCategory.Defenses,
            WardCapabilityCategory.Upkeep,
            WardCapabilityCategory.Personnel
        };

        string[] categoryTitles =
        {
            "Core Ward Authority",
            "Access & Entity Governance",
            "Defenses & Combat",
            "Base Upkeep & Sanctuary",
            "Personnel Governance"
        };

        for (int cIndex = 0; cIndex < categories.Length; cIndex++)
        {
            WardCapabilityCategory cat = categories[cIndex];
            List<FeaturePermission> perms = WardRoleManager.GetPermissionsByCategory(cat);
            if (perms.Count == 0)
                continue;

            Transform card = WardUIFactory.CreateCard(_rightContainer, categoryTitles[cIndex]);
            _permRows.Add(card.parent.parent.gameObject);

            for (int pIndex = 0; pIndex < perms.Count; pIndex++)
            {
                FeaturePermission perm = perms[pIndex];
                CreatePermissionRow(card, roles, selectedRole, perm, canManageGovernance);
            }
        }
    }

    private void CreatePermissionRow(
        Transform parent,
        List<WardRoleDefinition> roles,
        WardRoleDefinition selectedRole,
        FeaturePermission perm,
        bool canManageGovernance)
    {
        bool isInherited = WardRoleManager.IsPermissionInherited(roles, selectedRole.Id, perm.Id, out string inheritedFrom);
        bool hasExplicit = selectedRole.HasExplicitPermission(perm.Id);
        bool isOwner = selectedRole.Id == (int)WardRole.Owner || selectedRole.Level >= 3;

        GameObject rowObj = new GameObject($"Perm_{perm.Id}", typeof(RectTransform), typeof(Image), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
        rowObj.transform.SetParent(parent, false);

        LayoutElement rowLe = rowObj.GetComponent<LayoutElement>();
        rowLe.minHeight = 44f;
        rowLe.preferredHeight = 44f;
        rowLe.flexibleWidth = 1f;

        Image rowBg = rowObj.GetComponent<Image>();
        rowBg.color = (parent.childCount % 2 == 1) ? WardColorPalette.RowOdd : WardColorPalette.RowEven;

        HorizontalLayoutGroup hlg = rowObj.GetComponent<HorizontalLayoutGroup>();
        hlg.padding = new RectOffset(10, 10, 3, 3);
        hlg.spacing = 10f;
        hlg.childControlWidth = true;
        hlg.childControlHeight = true;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = false;
        hlg.childAlignment = TextAnchor.MiddleLeft;

        GameObject textCol = new GameObject("TextCol", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(LayoutElement));
        textCol.transform.SetParent(rowObj.transform, false);

        LayoutElement textLe = textCol.GetComponent<LayoutElement>();
        textLe.minWidth = 320f;
        textLe.preferredWidth = 340f;
        textLe.flexibleWidth = 1f;

        VerticalLayoutGroup textVlg = textCol.GetComponent<VerticalLayoutGroup>();
        textVlg.childControlWidth = true;
        textVlg.childControlHeight = true;
        textVlg.childForceExpandWidth = true;
        textVlg.childForceExpandHeight = false;
        textVlg.childAlignment = TextAnchor.MiddleLeft;
        textVlg.spacing = 2f;

        GameObject titleObj = new GameObject("Title", typeof(RectTransform), typeof(Text), typeof(LayoutElement));
        titleObj.transform.SetParent(textCol.transform, false);
        LayoutElement titleLe = titleObj.GetComponent<LayoutElement>();
        titleLe.minHeight = 16f;
        titleLe.preferredHeight = 16f;
        titleLe.flexibleHeight = 0f;

        Text titleTxt = titleObj.GetComponent<Text>();
        titleTxt.text = perm.DisplayName;
        titleTxt.font = GUIManager.Instance.AveriaSerifBold;
        titleTxt.fontSize = 12;
        titleTxt.color = WardColorPalette.ValheimOrange;
        titleTxt.verticalOverflow = VerticalWrapMode.Overflow;

        GameObject descObj = new GameObject("Desc", typeof(RectTransform), typeof(Text), typeof(LayoutElement));
        descObj.transform.SetParent(textCol.transform, false);
        LayoutElement descLe = descObj.GetComponent<LayoutElement>();
        descLe.minHeight = 14f;
        descLe.preferredHeight = 14f;
        descLe.flexibleHeight = 0f;

        Text descTxt = descObj.GetComponent<Text>();
        descTxt.text = perm.Description;
        descTxt.font = GUIManager.Instance.AveriaSerif;
        descTxt.fontSize = 10;
        descTxt.color = WardColorPalette.TextMuted;
        descTxt.verticalOverflow = VerticalWrapMode.Overflow;

        GameObject btnObj = GUIManager.Instance.CreateButton(
            "...",
            rowObj.transform,
            Vector2.zero,
            Vector2.zero,
            Vector2.zero,
            160f,
            26f);

        LayoutElement btnLe = btnObj.AddComponent<LayoutElement>();
        btnLe.minWidth = 160f;
        btnLe.preferredWidth = 160f;
        btnLe.minHeight = 26f;
        btnLe.preferredHeight = 26f;
        btnLe.flexibleWidth = 0f;

        Button btn = btnObj.GetComponent<Button>();
        Image btnImg = btnObj.GetComponent<Image>();
        Text btnTxt = btnObj.GetComponentInChildren<Text>();
        if (btnTxt != null)
        {
            btnTxt.font = GUIManager.Instance.AveriaSerifBold;
            btnTxt.fontSize = 10;
        }

        if (isOwner)
        {
            if (btnTxt != null)
            {
                btnTxt.text = "✔ Owner Authority";
                btnTxt.color = new Color(0.85f, 0.75f, 0.45f);
            }
            btnImg.color = new Color(0.20f, 0.18f, 0.14f);
            btn.interactable = false;
        }
        else if (isInherited)
        {
            if (btnTxt != null)
            {
                btnTxt.text = $"✔ Inherited ({inheritedFrom})";
                btnTxt.color = new Color(0.65f, 0.72f, 0.80f);
            }
            btnImg.color = new Color(0.18f, 0.22f, 0.28f);
            btn.interactable = false;
        }
        else if (hasExplicit)
        {
            if (btnTxt != null)
            {
                btnTxt.text = "✔ Granted";
                btnTxt.color = new Color(0.9f, 1f, 0.9f);
            }
            btnImg.color = new Color(0.10f, 0.40f, 0.20f);
            btn.interactable = canManageGovernance;

            btn.onClick.AddListener(() =>
            {
                selectedRole.RemovePermission(perm.Id);
                SaveAndBroadcast(roles);
                Refresh();
            });
        }
        else
        {
            if (btnTxt != null)
            {
                btnTxt.text = "✖ Not Granted";
                btnTxt.color = WardColorPalette.TextMuted;
            }
            btnImg.color = new Color(0.14f, 0.14f, 0.14f);
            btn.interactable = canManageGovernance;

            btn.onClick.AddListener(() =>
            {
                selectedRole.AddPermission(perm.Id);
                SaveAndBroadcast(roles);
                Refresh();
            });
        }
    }

    private void OnAddCustomRole(List<WardRoleDefinition> roles)
    {
        int nextId = 100;
        HashSet<int> existingIds = new();
        for (int i = 0; i < roles.Count; i++) existingIds.Add(roles[i].Id);
        while (existingIds.Contains(nextId)) nextId++;

        int customCount = 0;
        for (int i = 0; i < roles.Count; i++)
        {
            if (roles[i].Id >= 100) customCount++;
        }

        string roleName = $"Custom Rank {customCount + 1}";
        WardRoleDefinition newRole = new WardRoleDefinition(nextId, roleName, 2, "#93c5fd");
        roles.Add(newRole);

        SaveAndBroadcast(roles);
        _selectedRoleId = nextId;
        Refresh();
    }

    private void OnRenameCustomRole(List<WardRoleDefinition> roles)
    {
        WardRoleDefinition role = null;
        for (int i = 0; i < roles.Count; i++)
        {
            if (roles[i].Id == _selectedRoleId)
            {
                role = roles[i];
                break;
            }
        }

        if (role == null)
            return;

        if (TextInput.instance != null)
        {
            EnsureTextInputOnTop();
            TextInput.instance.RequestText(new RoleNameListener(role, roles, this), "Set Custom Role Name", 24);
        }
    }

    private static void EnsureTextInputOnTop()
    {
        if (TextInput.instance == null || TextInput.instance.m_panel == null)
            return;

        Canvas canvas = TextInput.instance.m_panel.GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = TextInput.instance.m_panel.AddComponent<Canvas>();
        }
        canvas.overrideSorting = true;
        canvas.sortingOrder = 30000;

        GraphicRaycaster raycaster = TextInput.instance.m_panel.GetComponent<GraphicRaycaster>();
        if (raycaster == null)
        {
            TextInput.instance.m_panel.AddComponent<GraphicRaycaster>();
        }

        TextInput.instance.transform.SetAsLastSibling();
    }

    private void OnDeleteCustomRole(List<WardRoleDefinition> roles)
    {
        for (int i = 0; i < roles.Count; i++)
        {
            if (roles[i].Id == _selectedRoleId)
            {
                roles.RemoveAt(i);
                break;
            }
        }

        SaveAndBroadcast(roles);
        _selectedRoleId = (int)WardRole.Resident;
        Refresh();
    }

    public void SaveAndBroadcast(List<WardRoleDefinition> roles)
    {
        if (_ward == null || _ward.m_nview == null || !_ward.m_nview.IsValid())
            return;

        ZDO zdo = _ward.m_nview.GetZDO();
        WardRoleManager.SaveRoleDefinitions(zdo, roles);

        string serialized = WardRoleManager.SerializeRoles(roles);
        WardNetworkRouter.SendRoleDefinitions(zdo.m_uid, serialized);
    }

    private void ClearLeft()
    {
        for (int i = 0; i < _roleButtons.Count; i++)
        {
            if (_roleButtons[i] != null)
                Object.Destroy(_roleButtons[i]);
        }
        _roleButtons.Clear();
    }

    private void ClearRight()
    {
        for (int i = 0; i < _permRows.Count; i++)
        {
            if (_permRows[i] != null)
                Object.Destroy(_permRows[i]);
        }
        _permRows.Clear();
    }

    private class RoleNameListener : TextReceiver
    {
        private readonly WardRoleDefinition _role;
        private readonly List<WardRoleDefinition> _roles;
        private readonly TabManagement _tab;

        public RoleNameListener(WardRoleDefinition role, List<WardRoleDefinition> roles, TabManagement tab)
        {
            _role = role;
            _roles = roles;
            _tab = tab;
        }

        public string GetText() => _role.Name;

        public void SetText(string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                _role.Name = text.Trim();
                _tab.SaveAndBroadcast(_roles);
                _tab.Refresh();
            }
        }
    }
}
