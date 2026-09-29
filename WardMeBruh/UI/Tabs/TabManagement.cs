using System.Collections.Generic;
using Jotunn.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WardMeBruh.Configuration;
using WardMeBruh.Models;
using WardMeBruh.Networking;
using WardMeBruh.Services;
using WardMeBruh.UI.Elements;

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
    private GameObject _hoverTooltip;
    private TextMeshProUGUI _tooltipTitle;
    private TextMeshProUGUI _tooltipDesc;
    private RectTransform _tooltipRt;

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
        HideHoverTooltip();
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

        if (WardMainWindow.Instance != null)
        {
            WardMainWindow.ApplyFontSizeToHierarchy(_root, WardMainWindow.Instance.CurrentFontSizeMode);
        }

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
        Transform card = WardUIFactory.CreateCard(_leftContainer, "$wmb_mgmt_card_hierarchy");
        _roleButtons.Add(card.parent.parent.gameObject);
        VerticalLayoutGroup cardVlg = card.GetComponent<VerticalLayoutGroup>();
        if (cardVlg != null)
        {
            cardVlg.spacing = 4f;
            cardVlg.padding = new RectOffset(4, 4, 4, 4);
        }

        List<WardRoleDefinition> sortedRoles = new(roles);
        sortedRoles.Sort((a, b) => b.Level.CompareTo(a.Level));

        for (int i = 0; i < sortedRoles.Count; i++)
        {
            WardRoleDefinition role = sortedRoles[i];
            int roleId = role.Id;
            bool isSelected = (role.Id == _selectedRoleId);

            string title = role.Name.Localize();
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
            TextMeshProUGUI btnTxt = WardUIFactory.SetupButtonText(btnObj, title, 13f, isSelected ? WardColorPalette.ValheimOrange : WardColorPalette.TextNormal);

            if (isSelected)
            {
                btnImg.color = WardColorPalette.ValheimOrange;
            }
            else
            {
                btnImg.color = WardColorPalette.CardBorder;
            }

            btn.onClick.AddListener(() =>
            {
                _selectedRoleId = roleId;
                Refresh();
            });
        }

        if (canManageGovernance)
        {
            Transform actionsCard = WardUIFactory.CreateCard(_leftContainer, "$wmb_mgmt_card_actions");
            _roleButtons.Add(actionsCard.parent.parent.gameObject);
            VerticalLayoutGroup actionsVlg = actionsCard.GetComponent<VerticalLayoutGroup>();
            if (actionsVlg != null)
            {
                actionsVlg.spacing = 6f;
                actionsVlg.padding = new RectOffset(4, 4, 4, 4);
            }

            GameObject addBtnObj = GUIManager.Instance.CreateButton(
                "$wmb_mgmt_add_role".Localize(),
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

            WardUIFactory.SetupButtonText(addBtnObj, "$wmb_mgmt_add_role".Localize(), 12f);
            Button addBtn = addBtnObj.GetComponent<Button>();
            addBtn.onClick.AddListener(() => OnAddCustomRole(roles));

            int selectedIndex = -1;
            for (int i = 0; i < sortedRoles.Count; i++)
            {
                if (sortedRoles[i].Id == _selectedRoleId)
                {
                    selectedIndex = i;
                    break;
                }
            }

            if (selectedIndex > 0 && selectedIndex < sortedRoles.Count - 1)
            {
                if (selectedIndex > 1)
                {
                    GameObject moveUpBtnObj = GUIManager.Instance.CreateButton(
                        "$wmb_mgmt_move_up".Localize(),
                        actionsCard,
                        Vector2.zero,
                        Vector2.zero,
                        Vector2.zero,
                        200f,
                        28f);

                    LayoutElement tuLe = moveUpBtnObj.AddComponent<LayoutElement>();
                    tuLe.minHeight = 28f;
                    tuLe.preferredHeight = 28f;
                    tuLe.flexibleWidth = 1f;

                    WardUIFactory.SetupButtonText(moveUpBtnObj, "$wmb_mgmt_move_up".Localize(), 11f);
                    Button tuBtn = moveUpBtnObj.GetComponent<Button>();
                    tuBtn.onClick.AddListener(() => OnMoveRole(roles, 1));
                }

                if (selectedIndex < sortedRoles.Count - 2)
                {
                    GameObject moveDownBtnObj = GUIManager.Instance.CreateButton(
                        "$wmb_mgmt_move_down".Localize(),
                        actionsCard,
                        Vector2.zero,
                        Vector2.zero,
                        Vector2.zero,
                        200f,
                        28f);

                    LayoutElement tdLe = moveDownBtnObj.AddComponent<LayoutElement>();
                    tdLe.minHeight = 28f;
                    tdLe.preferredHeight = 28f;
                    tdLe.flexibleWidth = 1f;

                    WardUIFactory.SetupButtonText(moveDownBtnObj, "$wmb_mgmt_move_down".Localize(), 11f);
                    Button tdBtn = moveDownBtnObj.GetComponent<Button>();
                    tdBtn.onClick.AddListener(() => OnMoveRole(roles, -1));
                }
            }

            if (_selectedRoleId >= 100)
            {
                GameObject renameBtnObj = GUIManager.Instance.CreateButton(
                    "$wmb_mgmt_rename_role".Localize(),
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

                WardUIFactory.SetupButtonText(renameBtnObj, "$wmb_mgmt_rename_role".Localize(), 11f);
                Button renBtn = renameBtnObj.GetComponent<Button>();
                renBtn.onClick.AddListener(() => OnRenameCustomRole(roles));

                GameObject delBtnObj = GUIManager.Instance.CreateButton(
                    "$wmb_mgmt_delete_role".Localize(),
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

                WardUIFactory.SetupButtonText(delBtnObj, "$wmb_mgmt_delete_role".Localize(), 11f, WardColorPalette.DangerRed);
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
            WardCapabilityCategory.Personnel,
            WardCapabilityCategory.External
        };

        string[] categoryTitles =
        {
            "$wmb_cat_core",
            "$wmb_cat_access",
            "$wmb_cat_defenses",
            "$wmb_cat_upkeep",
            "$wmb_cat_personnel",
            "$wmb_cat_external"
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
        bool isOwner = selectedRole.Id == (int)WardRole.Owner;

        GameObject rowObj = new GameObject($"Perm_{perm.Id}", typeof(RectTransform), typeof(Image), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
        rowObj.transform.SetParent(parent, false);

        LayoutElement rowLe = rowObj.GetComponent<LayoutElement>();
        rowLe.minHeight = 36f;
        rowLe.preferredHeight = 36f;
        rowLe.flexibleWidth = 1f;

        Image rowBg = rowObj.GetComponent<Image>();
        rowBg.color = (parent.childCount % 2 == 1) ? WardColorPalette.RowOdd : WardColorPalette.RowEven;

        HorizontalLayoutGroup hlg = rowObj.GetComponent<HorizontalLayoutGroup>();
        hlg.padding = new RectOffset(10, 10, 4, 4);
        hlg.spacing = 8f;
        hlg.childControlWidth = true;
        hlg.childControlHeight = true;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = false;
        hlg.childAlignment = TextAnchor.MiddleLeft;

        GameObject labelObj = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI), typeof(LayoutElement), typeof(WardTextElement));
        labelObj.transform.SetParent(rowObj.transform, false);

        LayoutElement labelLe = labelObj.GetComponent<LayoutElement>();
        labelLe.minWidth = 280f;
        labelLe.preferredWidth = 360f;
        labelLe.flexibleWidth = 1f;
        labelLe.minHeight = 28f;
        labelLe.preferredHeight = 28f;

        TextMeshProUGUI label = labelObj.GetComponent<TextMeshProUGUI>();
        label.text = perm.DisplayName.Localize();
        label.font = UIFonts.GetPrimaryFont();
        label.fontSize = 13f;
        label.color = WardColorPalette.ValheimBeige;
        label.alignment = TextAlignmentOptions.MidlineLeft;
        label.overflowMode = TextOverflowModes.Ellipsis;
        label.textWrappingMode = TextWrappingModes.Normal;

        WardTextElement wte = labelObj.GetComponent<WardTextElement>();
        wte.baseFontSize = 13;

        RowHoverTooltipTrigger hoverTrigger = rowObj.AddComponent<RowHoverTooltipTrigger>();
        hoverTrigger.Permission = perm;
        hoverTrigger.Tab = this;

        GameObject btnObj = GUIManager.Instance.CreateButton(
            "...",
            rowObj.transform,
            Vector2.zero,
            Vector2.zero,
            Vector2.zero,
            175f,
            28f);

        LayoutElement btnLe = btnObj.AddComponent<LayoutElement>();
        btnLe.minWidth = 175f;
        btnLe.preferredWidth = 175f;
        btnLe.minHeight = 28f;
        btnLe.preferredHeight = 28f;
        btnLe.flexibleWidth = 0f;

        Button btn = btnObj.GetComponent<Button>();
        Image btnImg = btnObj.GetComponent<Image>();
        TextMeshProUGUI btnTxt = WardUIFactory.SetupButtonText(btnObj, "...", 10f);

        if (isOwner)
        {
            if (btnTxt != null)
            {
                btnTxt.text = $"✔ {"$wmb_mgmt_owner_authority".Localize()}";
                btnTxt.color = new Color(0.85f, 0.75f, 0.45f);
            }
            btnImg.color = new Color(0.20f, 0.18f, 0.14f);
            btn.interactable = false;
        }
        else if (isInherited)
        {
            if (btnTxt != null)
            {
                btnTxt.text = $"✔ {"$wmb_mgmt_inherited".Localize()} ({inheritedFrom.Localize()})";
                btnTxt.color = new Color(0.65f, 0.72f, 0.80f);
            }
            btnImg.color = new Color(0.18f, 0.22f, 0.28f);
            btn.interactable = false;
        }
        else if (hasExplicit)
        {
            if (btnTxt != null)
            {
                btnTxt.text = $"✔ {"$wmb_mgmt_granted".Localize()}";
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
                btnTxt.text = $"✖ {"$wmb_mgmt_not_granted".Localize()}";
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

    public void ShowHoverTooltip(FeaturePermission perm, Vector2 screenPos)
    {
        if (perm == null)
            return;

        EnsureHoverTooltip();

        _tooltipTitle.text = perm.DisplayName.Localize();
        _tooltipDesc.text = perm.Description.Localize();

        if (WardMainWindow.Instance != null)
        {
            WardMainWindow.ApplyFontSizeToHierarchy(_hoverTooltip, WardMainWindow.Instance.CurrentFontSizeMode);
        }

        UpdateHoverTooltipPosition(screenPos);

        _hoverTooltip.SetActive(true);
        _hoverTooltip.transform.SetAsLastSibling();
    }

    public void UpdateHoverTooltipPosition(Vector2 screenPos)
    {
        if (_hoverTooltip == null || _tooltipRt == null)
            return;

        _tooltipRt.pivot = new Vector2(0f, 1f);
        _hoverTooltip.transform.position = new Vector3(screenPos.x + 16f, screenPos.y - 12f, 0f);
        Utils.ClampUIToScreen(_tooltipRt);
    }

    public void HideHoverTooltip()
    {
        if (_hoverTooltip != null)
        {
            _hoverTooltip.SetActive(false);
        }
    }

    private void EnsureHoverTooltip()
    {
        if (_hoverTooltip != null)
            return;

        Transform canvasTransform = _root != null && _root.GetComponentInParent<Canvas>() != null
            ? _root.GetComponentInParent<Canvas>().transform
            : (_root != null ? _root.transform : null);

        if (canvasTransform == null)
            return;

        _hoverTooltip = new GameObject("PermHoverTooltip", typeof(RectTransform), typeof(CanvasGroup));
        _hoverTooltip.transform.SetParent(canvasTransform, false);

        CanvasGroup cg = _hoverTooltip.GetComponent<CanvasGroup>();
        cg.blocksRaycasts = false;
        cg.interactable = false;

        _tooltipRt = _hoverTooltip.GetComponent<RectTransform>();
        _tooltipRt.anchorMin = new Vector2(0f, 1f);
        _tooltipRt.anchorMax = new Vector2(0f, 1f);
        _tooltipRt.pivot = new Vector2(0f, 1f);
        _tooltipRt.sizeDelta = new Vector2(320f, 0f);

        GameObject borderObj = new GameObject("Border", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        borderObj.transform.SetParent(_hoverTooltip.transform, false);

        RectTransform borderRt = borderObj.GetComponent<RectTransform>();
        borderRt.anchorMin = Vector2.zero;
        borderRt.anchorMax = Vector2.one;
        borderRt.offsetMin = Vector2.zero;
        borderRt.offsetMax = Vector2.zero;

        Image borderImg = borderObj.GetComponent<Image>();
        borderImg.color = WardColorPalette.IceBlue;

        VerticalLayoutGroup borderVlg = borderObj.GetComponent<VerticalLayoutGroup>();
        borderVlg.padding = new RectOffset(1, 1, 1, 1);
        borderVlg.childControlWidth = true;
        borderVlg.childControlHeight = true;
        borderVlg.childForceExpandWidth = true;
        borderVlg.childForceExpandHeight = false;

        ContentSizeFitter borderCsf = borderObj.GetComponent<ContentSizeFitter>();
        borderCsf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        GameObject box = new GameObject("Box", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        box.transform.SetParent(borderObj.transform, false);

        RectTransform boxRt = box.GetComponent<RectTransform>();
        boxRt.anchorMin = new Vector2(0f, 1f);
        boxRt.anchorMax = new Vector2(0f, 1f);
        boxRt.pivot = new Vector2(0f, 1f);
        boxRt.anchoredPosition = Vector2.zero;
        boxRt.sizeDelta = new Vector2(318f, 0f);

        Image boxImg = box.GetComponent<Image>();
        boxImg.color = new Color(0.06f, 0.08f, 0.12f, 0.98f);

        VerticalLayoutGroup vlg = box.GetComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(10, 10, 8, 8);
        vlg.spacing = 3f;
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        ContentSizeFitter csf = box.GetComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        GameObject titleObj = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI), typeof(WardTextElement));
        titleObj.transform.SetParent(box.transform, false);
        _tooltipTitle = titleObj.GetComponent<TextMeshProUGUI>();
        _tooltipTitle.font = UIFonts.GetPrimaryFont();
        _tooltipTitle.fontSize = 13f;
        _tooltipTitle.color = WardColorPalette.ValheimOrange;
        _tooltipTitle.alignment = TextAlignmentOptions.TopLeft;
        WardTextElement titleWte = titleObj.GetComponent<WardTextElement>();
        titleWte.baseFontSize = 13;

        GameObject descObj = new GameObject("Desc", typeof(RectTransform), typeof(TextMeshProUGUI), typeof(WardTextElement));
        descObj.transform.SetParent(box.transform, false);
        _tooltipDesc = descObj.GetComponent<TextMeshProUGUI>();
        _tooltipDesc.font = UIFonts.GetPrimaryFont();
        _tooltipDesc.fontSize = 13f;
        _tooltipDesc.color = WardColorPalette.ValheimBeige;
        _tooltipDesc.alignment = TextAlignmentOptions.TopLeft;
        _tooltipDesc.overflowMode = TextOverflowModes.Overflow;
        _tooltipDesc.textWrappingMode = TextWrappingModes.Normal;
        WardTextElement descWte = descObj.GetComponent<WardTextElement>();
        descWte.baseFontSize = 13;

        _hoverTooltip.SetActive(false);
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

        string roleName = $"{"$wmb_mgmt_custom_rank".Localize()} {customCount + 1}";
        WardRoleDefinition newRole = new WardRoleDefinition(nextId, roleName, 2, "#93c5fd");
        roles.Add(newRole);

        List<WardRoleDefinition> sorted = new(roles);
        sorted.Sort((a, b) => b.Level.CompareTo(a.Level));
        for (int i = 0; i < sorted.Count; i++)
        {
            sorted[i].Level = sorted.Count - 1 - i;
        }

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
            TextInput.instance.RequestText(new RoleNameListener(role, roles, this), "$wmb_mgmt_rename_prompt".Localize(), 24);
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

    private void OnMoveRole(List<WardRoleDefinition> roles, int delta)
    {
        List<WardRoleDefinition> sorted = new(roles);
        sorted.Sort((a, b) => b.Level.CompareTo(a.Level));

        int currentIndex = -1;
        for (int i = 0; i < sorted.Count; i++)
        {
            if (sorted[i].Id == _selectedRoleId)
            {
                currentIndex = i;
                break;
            }
        }

        if (currentIndex <= 0 || currentIndex >= sorted.Count - 1)
            return;

        int targetIndex = currentIndex - delta;
        if (targetIndex < 1 || targetIndex > sorted.Count - 2)
            return;

        WardRoleDefinition temp = sorted[currentIndex];
        sorted[currentIndex] = sorted[targetIndex];
        sorted[targetIndex] = temp;

        for (int i = 0; i < sorted.Count; i++)
        {
            sorted[i].Level = sorted.Count - 1 - i;
        }

        SaveAndBroadcast(roles);
        Refresh();
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

        List<WardRoleDefinition> sorted = new(roles);
        sorted.Sort((a, b) => b.Level.CompareTo(a.Level));
        for (int i = 0; i < sorted.Count; i++)
        {
            sorted[i].Level = sorted.Count - 1 - i;
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
        HideHoverTooltip();
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
