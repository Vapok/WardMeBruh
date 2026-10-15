using System;
using System.Collections.Generic;
using Jotunn.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WardMeBruh.API;
using WardMeBruh.Models;
using WardMeBruh.Services;

namespace WardMeBruh.UI;

public class RemoteWardSelectorModal
{
    private GameObject _panel;
    private Transform _listContainer;
    private readonly List<GameObject> _itemCards = new();
    private Action<KnownWard> _onWardSelected;
    private PrivateArea _activeWard;
    private PrivateArea _physicalWard;
    private ScrollRect _scrollRect;
    private GameObject _closeBtn;
    private TextMeshProUGUI _closeBtnText;
    private GameObject _gamepadLegend;
    private readonly List<Selectable> _selectableButtons = new();
    private Selectable _currentSelected;
    private GameObject _focusReticle;
    private int _lastSubmitFrame = -1;

    public static int LastClosedFrame { get; private set; } = -1;
    public bool IsOpen => _panel != null && _panel.activeSelf;

    public void Build(Transform parent, Action<KnownWard> onWardSelected)
    {
        UIFonts.GetPrimaryFont();
        _onWardSelected = onWardSelected;

        _panel = GUIManager.Instance.CreateWoodpanel(
            parent, 
            new Vector2(0.5f, 0.5f), 
            new Vector2(0.5f, 0.5f), 
            Vector2.zero, 
            680f, 
            500f, 
            draggable: true);
        _panel.name = "RemoteWardSelectorModal";

        GameObject titleObj = new GameObject("TitleText", typeof(RectTransform), typeof(TextMeshProUGUI), typeof(WardTextElement));
        titleObj.transform.SetParent(_panel.transform, false);
        RectTransform titleRt = titleObj.GetComponent<RectTransform>();
        titleRt.anchorMin = new Vector2(0f, 1f);
        titleRt.anchorMax = new Vector2(0f, 1f);
        titleRt.pivot = new Vector2(0f, 1f);
        titleRt.anchoredPosition = new Vector2(30f, -20f);
        titleRt.sizeDelta = new Vector2(450f, 32f);

        TextMeshProUGUI titleText = titleObj.GetComponent<TextMeshProUGUI>();
        titleText.text = "$wmb_modal_territory_registry".Localize();
        titleText.font = UIFonts.GetPrimaryFont();
        titleText.fontSize = 18f;
        titleText.color = WardColorPalette.ValheimOrange;
        titleText.alignment = TextAlignmentOptions.MidlineLeft;

        WardTextElement titleWte = titleObj.GetComponent<WardTextElement>();
        titleWte.baseFontSize = 18;

        _closeBtn = GUIManager.Instance.CreateButton(
            "X", 
            _panel.transform, 
            new Vector2(1f, 1f), 
            new Vector2(1f, 1f), 
            new Vector2(-48f, -34f), 
            38f, 
            32f);
        _closeBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(38f, 32f);
        WardUIFactory.SetupButtonText(_closeBtn, "X", 14f, WardColorPalette.DangerRed);
        _closeBtnText = _closeBtn.GetComponentInChildren<TextMeshProUGUI>();
        _closeBtn.GetComponent<Button>().onClick.AddListener(Close);

        GameObject scrollAreaObj = WardUIFactory.CreateScrollArea(_panel.transform, out _listContainer);
        _scrollRect = scrollAreaObj.GetComponent<ScrollRect>();
        RectTransform scrollRt = scrollAreaObj.GetComponent<RectTransform>();
        scrollRt.offsetMin = new Vector2(25f, 25f);
        scrollRt.offsetMax = new Vector2(-25f, -60f);

        _gamepadLegend = new GameObject("GamepadLegend", typeof(RectTransform), typeof(TextMeshProUGUI));
        _gamepadLegend.transform.SetParent(_panel.transform, false);
        RectTransform legendRt = _gamepadLegend.GetComponent<RectTransform>();
        legendRt.anchorMin = new Vector2(0f, 0f);
        legendRt.anchorMax = new Vector2(1f, 0f);
        legendRt.pivot = new Vector2(0.5f, 1f);
        legendRt.offsetMin = new Vector2(30f, -26f);
        legendRt.offsetMax = new Vector2(-30f, -6f);

        TextMeshProUGUI legendText = _gamepadLegend.GetComponent<TextMeshProUGUI>();
        legendText.font = UIFonts.GetPrimaryFont();
        legendText.text = "<color=#e2d6b5><b>[D-Pad]</b> Navigate   <b>[A]</b> Select Ward   <b>[B]</b> Back   <b>[R-Stick]</b> Scroll</color>";
        legendText.fontSize = 11f;
        legendText.alignment = TextAlignmentOptions.Center;
        legendText.color = WardColorPalette.ValheimBeige;

        _focusReticle = WardGamepadNavHelper.CreateFocusReticle(_panel.transform);

        _panel.SetActive(false);
    }

    public void Open(PrivateArea activeWard = null, PrivateArea physicalWard = null)
    {
        if (_panel == null)
            return;

        _activeWard = activeWard;
        _physicalWard = physicalWard;
        _panel.SetActive(true);
        Refresh();
        UpdateKeyhints(WardGamepadNavHelper.IsGamepadActive());
    }

    public void Close()
    {
        LastClosedFrame = Time.frameCount;
        WardGamepadNavHelper.AttachFocusReticle(_focusReticle, null);
        if (_panel != null)
        {
            _panel.SetActive(false);
        }
    }

    public void Refresh()
    {
        for (int i = 0; i < _itemCards.Count; i++)
        {
            if (_itemCards[i] != null)
                UnityEngine.Object.Destroy(_itemCards[i]);
        }
        _itemCards.Clear();
        _selectableButtons.Clear();
        _currentSelected = null;

        if (_listContainer != null)
        {
            RectTransform listRt = _listContainer.GetComponent<RectTransform>();
            if (listRt != null)
            {
                listRt.anchoredPosition = Vector2.zero;
            }
        }

        long playerId = Player.m_localPlayer != null ? Player.m_localPlayer.GetPlayerID() : 0L;
        List<KnownWard> wards = WardRegistryManager.Instance.GetPlayerOwnedWards(playerId);

        Transform card = WardUIFactory.CreateCard(_listContainer, $"{"$wmb_modal_registered_territories".Localize()} ({wards.Count})");
        _itemCards.Add(card.parent.parent.gameObject);

        int offset = WardMainWindow.Instance != null ? WardMainWindow.Instance.CurrentFontSizeMode switch
        {
            WardFontSizeMode.Medium => 2,
            WardFontSizeMode.Large => 4,
            _ => 0
        } : 0;
        int subSize = 11 + offset;
        float rowHeight = 44f + offset * 3f;

        if (wards.Count == 0)
        {
            WardUIFactory.CreateBadgeRow(card, "$wmb_modal_no_owned_wards".Localize(), "-", WardColorPalette.TextMuted, out _);
            if (WardMainWindow.Instance != null && _panel != null)
            {
                WardMainWindow.ApplyFontSizeToHierarchy(_panel, WardMainWindow.Instance.CurrentFontSizeMode);
            }
            return;
        }

        Vector3 playerPos = Player.m_localPlayer != null ? Player.m_localPlayer.transform.position : Vector3.zero;
        ZDOID activeWardId = (_activeWard != null && _activeWard.m_nview != null && _activeWard.m_nview.IsValid()) 
            ? _activeWard.m_nview.GetZDO().m_uid 
            : (WardMainWindow.Instance != null ? WardMainWindow.Instance.ActiveWardId : ZDOID.None);

        ZDOID physicalWardId = ZDOID.None;
        if (_physicalWard != null && _physicalWard.m_nview != null && _physicalWard.m_nview.IsValid())
        {
            physicalWardId = _physicalWard.m_nview.GetZDO().m_uid;
        }
        else if (WardMainWindow.Instance != null && WardMainWindow.Instance.PhysicalWardId != ZDOID.None)
        {
            physicalWardId = WardMainWindow.Instance.PhysicalWardId;
        }
        else if (Player.m_localPlayer != null)
        {
            KnownWard covering = WardRegistryManager.Instance.GetWardCoveringPoint(playerPos, false);
            if (covering != null)
            {
                physicalWardId = covering.Id;
            }
        }

        List<KnownWard> rootWards = new();
        Dictionary<ZDOID, List<KnownWard>> childrenByParent = new();
        HashSet<ZDOID> rootIds = new();

        for (int i = 0; i < wards.Count; i++)
        {
            KnownWard ward = wards[i];
            if (ward.Type == WardType.Main || ward.ParentId == ZDOID.None || ward.ParentId == ward.Id)
            {
                rootWards.Add(ward);
                rootIds.Add(ward.Id);
            }
            else
            {
                if (!childrenByParent.TryGetValue(ward.ParentId, out List<KnownWard> children))
                {
                    children = new List<KnownWard>();
                    childrenByParent[ward.ParentId] = children;
                }
                children.Add(ward);
            }
        }

        List<KnownWard> orphanedChildren = null;
        foreach (KeyValuePair<ZDOID, List<KnownWard>> kvp in childrenByParent)
        {
            if (!rootIds.Contains(kvp.Key))
            {
                orphanedChildren ??= new List<KnownWard>();
                orphanedChildren.AddRange(kvp.Value);
            }
        }

        if (orphanedChildren != null)
        {
            for (int i = 0; i < orphanedChildren.Count; i++)
            {
                KnownWard orphan = orphanedChildren[i];
                rootWards.Add(orphan);
                rootIds.Add(orphan.Id);
                if (childrenByParent.TryGetValue(orphan.ParentId, out List<KnownWard> list))
                {
                    list.Remove(orphan);
                }
            }
        }

        rootWards.Sort((a, b) =>
        {
            bool aIsCurrent = a.Id == physicalWardId || a.Id == activeWardId;
            bool bIsCurrent = b.Id == physicalWardId || b.Id == activeWardId;
            if (aIsCurrent && !bIsCurrent) return -1;
            if (!aIsCurrent && bIsCurrent) return 1;

            float distA = Utils.DistanceXZ(playerPos, a.Position);
            float distB = Utils.DistanceXZ(playerPos, b.Position);
            return distA.CompareTo(distB);
        });

        List<(KnownWard Ward, bool IsChild, bool IsLastChild)> displayItems = new();
        for (int r = 0; r < rootWards.Count; r++)
        {
            KnownWard root = rootWards[r];
            displayItems.Add((root, false, false));

            if (childrenByParent.TryGetValue(root.Id, out List<KnownWard> children) && children.Count > 0)
            {
                children.Sort((a, b) =>
                {
                    int typeComp = a.Type.CompareTo(b.Type);
                    if (typeComp != 0) return typeComp;
                    return string.Compare(a.CustomName, b.CustomName, StringComparison.OrdinalIgnoreCase);
                });

                for (int c = 0; c < children.Count; c++)
                {
                    bool isLast = (c == children.Count - 1);
                    displayItems.Add((children[c], true, isLast));
                }
            }
        }

        for (int i = 0; i < displayItems.Count; i++)
        {
            (KnownWard ward, bool isChild, bool isLastChild) = displayItems[i];
            bool isManaging = ward.Id == activeWardId;
            bool isPhysicalLocation = ward.Id == physicalWardId;
            bool canManage = CanPlayerManageWard(ward, playerId);

            float dist = Utils.DistanceXZ(playerPos, ward.Position);
            string distStr = dist < 1000f ? $"{Mathf.RoundToInt(dist)}{"$wmb_meters_away".Localize()}" : $"{dist / 1000f:F1}{"$wmb_km_away".Localize()}";
            if (isPhysicalLocation)
            {
                distStr = "$wmb_modal_current_location".Localize();
            }

            string typeBadge = ward.Type == WardType.SubWard 
                ? "<color=#93c5fd><b>[Sub-Ward]</b></color> " 
                : (ward.Type == WardType.Extension ? "<color=#fcd34d><b>[Extension]</b></color> " : string.Empty);

            string labelText = $"{typeBadge}<b>{ward.CustomName}</b>  <color=#fcd34d>[r: {Mathf.RoundToInt(ward.Radius)}{"$wmb_meters".Localize()}]</color>\n<size={subSize}><color=#9ca3af>{"$wmb_owner_label".Localize()} {ward.CreatorName}  •  {distStr}</color></size>";

            GameObject rowObj = WardUIFactory.CreateSettingRow(card, labelText, out Transform ctrlContainer, rowHeight);

            if (isChild)
            {
                HorizontalLayoutGroup hlg = rowObj.GetComponent<HorizontalLayoutGroup>();
                if (hlg != null)
                {
                    hlg.padding = new RectOffset(36, 10, 2, 2);
                }
            }

            Transform labelTrans = rowObj.transform.Find("Label");
            if (labelTrans != null)
            {
                LayoutElement labelLe = labelTrans.GetComponent<LayoutElement>();
                if (labelLe != null)
                {
                    labelLe.minWidth = 340f;
                    labelLe.preferredWidth = 390f;
                    labelLe.flexibleWidth = 1f;
                }

                TextMeshProUGUI label = labelTrans.GetComponent<TextMeshProUGUI>();
                if (label != null)
                {
                    label.textWrappingMode = TextWrappingModes.NoWrap;
                    label.overflowMode = TextOverflowModes.Ellipsis;
                }
            }

            LayoutElement ctrlLe = ctrlContainer.GetComponent<LayoutElement>();
            if (ctrlLe != null)
            {
                ctrlLe.minWidth = 210f;
                ctrlLe.preferredWidth = 210f;
                ctrlLe.flexibleWidth = 0f;
            }

            GameObject statusBadgeObj = new GameObject("StatusBadge", typeof(RectTransform), typeof(TextMeshProUGUI), typeof(LayoutElement), typeof(WardTextElement));
            statusBadgeObj.transform.SetParent(ctrlContainer, false);
            statusBadgeObj.GetComponent<RectTransform>().sizeDelta = new Vector2(100f, 28f);
            statusBadgeObj.SetLayoutDimensions(100f, 28f);

            TextMeshProUGUI statusText = statusBadgeObj.GetComponent<TextMeshProUGUI>();
            statusText.text = ward.IsEnabled ? $"● {"$wmb_active".Localize()}" : $"○ {"$wmb_inactive".Localize()}";
            statusText.font = UIFonts.GetPrimaryFont();
            statusText.fontSize = 12f;
            statusText.color = ward.IsEnabled ? WardColorPalette.RunicGreen : WardColorPalette.TextMuted;
            statusText.alignment = TextAlignmentOptions.MidlineRight;
            statusText.overflowMode = TextOverflowModes.Overflow;
            statusText.textWrappingMode = TextWrappingModes.NoWrap;

            WardTextElement statusWte = statusBadgeObj.GetComponent<WardTextElement>();
            statusWte.baseFontSize = 12;

            if (isManaging || !canManage)
            {
                GameObject spacerObj = new GameObject("Spacer", typeof(RectTransform), typeof(LayoutElement));
                spacerObj.transform.SetParent(ctrlContainer, false);
                spacerObj.GetComponent<RectTransform>().sizeDelta = new Vector2(100f, 28f);
                spacerObj.SetLayoutDimensions(100f, 28f);
            }
            else
            {
                GameObject manageBtn = GUIManager.Instance.CreateButton(
                    "$wmb_modal_manage_btn".Localize(), 
                    ctrlContainer, 
                    Vector2.zero, 
                    Vector2.zero, 
                    Vector2.zero, 
                    100f, 
                    28f);
                manageBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(100f, 28f);
                manageBtn.SetLayoutDimensions(100f, 28f);

                WardUIFactory.SetupButtonText(manageBtn, "$wmb_modal_manage_btn".Localize(), 12f, WardColorPalette.ValheimBeige);

                Button btn = manageBtn.GetComponent<Button>();
                WardGamepadNavHelper.ApplyGamepadColorBlock(btn);
                _selectableButtons.Add(btn);

                btn.onClick.AddListener(() =>
                {
                    Close();
                    _onWardSelected?.Invoke(ward);
                });
            }
        }

        if (WardMainWindow.Instance != null && _panel != null)
        {
            WardMainWindow.ApplyFontSizeToHierarchy(_panel, WardMainWindow.Instance.CurrentFontSizeMode);
        }

        if (WardGamepadNavHelper.IsGamepadActive() && _selectableButtons.Count > 0)
        {
            _currentSelected = _selectableButtons[0];
            _currentSelected.Select();
            WardGamepadNavHelper.AttachFocusReticle(_focusReticle, _currentSelected);
        }
        else
        {
            WardGamepadNavHelper.AttachFocusReticle(_focusReticle, null);
        }
    }

    public void UpdateKeyhints(bool isGamepad)
    {
        if (_closeBtnText != null)
        {
            string xGlyph = WardGamepadNavHelper.GetGamepadGlyph("X");
            _closeBtnText.text = isGamepad ? xGlyph : "X";
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
                    string rstick = WardGamepadNavHelper.GetGamepadGlyph("RStick");
                    legendText.text = $"<color=#e2d6b5>{dpad} Navigate   {a} Select Ward   {x} Close   {rstick} Scroll</color>";
                }
            }
        }
    }

    public void UpdateNavigation()
    {
        bool isGamepad = WardGamepadNavHelper.IsGamepadActive();
        UpdateKeyhints(isGamepad);

        if (!isGamepad || !IsOpen)
        {
            WardGamepadNavHelper.AttachFocusReticle(_focusReticle, null);
            return;
        }

        if (WardGamepadNavHelper.IsButtonDownB() || WardGamepadNavHelper.IsButtonDownX() || Input.GetKeyDown(KeyCode.Escape))
        {
            Close();
            if (WardMainWindow.Instance != null && WardMainWindow.Instance.GamepadController != null)
            {
                WardMainWindow.Instance.GamepadController.RestoreFocusOrSelectFirst();
            }
            return;
        }

        if (WardGamepadNavHelper.GetDirectionInput(out int navX, out int navY))
        {
            if (navY != 0 && _selectableButtons.Count > 0)
            {
                StepSelection(-navY);
            }
        }

        if (WardGamepadNavHelper.IsButtonDownA() && _currentSelected != null && Time.frameCount != _lastSubmitFrame)
        {
            if (_currentSelected is Button btn && btn.interactable)
            {
                _lastSubmitFrame = Time.frameCount;
                btn.onClick.Invoke();
            }
        }

        float joyY = WardGamepadNavHelper.GetRightStickScrollDelta();
        if (Mathf.Abs(joyY) > 0.15f && _scrollRect != null)
        {
            float scrollDelta = joyY * 2.5f * Time.unscaledDeltaTime;
            _scrollRect.verticalNormalizedPosition = Mathf.Clamp01(_scrollRect.verticalNormalizedPosition + scrollDelta);
        }

        if (_currentSelected != null && _currentSelected.gameObject.activeInHierarchy)
        {
            WardGamepadNavHelper.AttachFocusReticle(_focusReticle, _currentSelected);
        }
        else
        {
            WardGamepadNavHelper.AttachFocusReticle(_focusReticle, null);
        }
    }

    private void StepSelection(int direction)
    {
        if (_selectableButtons.Count == 0)
            return;

        int currentIndex = _selectableButtons.IndexOf(_currentSelected);
        int nextIndex = currentIndex + direction;
        if (currentIndex < 0)
        {
            nextIndex = direction > 0 ? 0 : _selectableButtons.Count - 1;
        }
        else
        {
            nextIndex = Mathf.Clamp(nextIndex, 0, _selectableButtons.Count - 1);
        }

        Selectable target = _selectableButtons[nextIndex];
        if (target != null && target.interactable)
        {
            _currentSelected = target;
            target.Select();
            WardGamepadNavHelper.AttachFocusReticle(_focusReticle, _currentSelected);

            RectTransform targetRt = target.transform as RectTransform;
            if (targetRt != null && _scrollRect != null)
            {
                WardGamepadNavHelper.EnsureVisible(_scrollRect, targetRt);
            }
        }
    }

    private static bool CanPlayerManageWard(KnownWard ward, long playerId)
    {
        if (ward == null || playerId == 0L)
            return false;

        if (WardPermissions.IsAdmin(playerId))
            return true;

        if (ward.CreatorId == playerId)
            return true;

        if (ZDOMan.instance != null)
        {
            ZDO zdo = ZDOMan.instance.GetZDO(ward.Id);
            if (zdo != null)
            {
                long creatorId = zdo.GetLong(ZDOVars.s_creator, ward.CreatorId);
                if (creatorId == playerId)
                    return true;

                if (WardRoleManager.CanPerform(zdo, playerId, WardPermissionIds.CoreViewGui))
                    return true;
            }

            if (ward.ParentId != ZDOID.None)
            {
                ZDO parentZdo = ZDOMan.instance.GetZDO(ward.ParentId);
                if (parentZdo != null)
                {
                    long parentCreatorId = parentZdo.GetLong(ZDOVars.s_creator, 0L);
                    if (parentCreatorId == playerId)
                        return true;

                    if (WardRoleManager.CanPerform(parentZdo, playerId, WardPermissionIds.CoreViewGui))
                        return true;
                }
            }
        }

        return false;
    }
}
