using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using WardMeBruh.Configuration;
using WardMeBruh.UI.Elements;

namespace WardMeBruh.UI;

public class WardDialogGamepadController
{
    private readonly WardMainWindow _window;
    private readonly GameObject _panel;

    private GameObject _tabBumperLeft;
    private GameObject _tabBumperRight;
    private GameObject _gamepadLegend;
    private TextMeshProUGUI _legendText;
    private GameObject _focusReticle;

    private Selectable _currentSelected;
    private bool _wasGamepadActive;
    private int _lastFocusedIndex = 0;

    public WardDialogGamepadController(WardMainWindow window, GameObject panel)
    {
        _window = window;
        _panel = panel;
    }

    public void InitializeElements(GameObject tabBar, RectTransform panelRt, TextMeshProUGUI otherWardsText, TextMeshProUGUI closeText)
    {
        _focusReticle = WardGamepadNavHelper.CreateFocusReticle(_panel.transform);

        if (tabBar != null)
        {
            _tabBumperLeft = new GameObject("TabBumperLeft", typeof(RectTransform), typeof(TextMeshProUGUI), typeof(LayoutElement));
            _tabBumperLeft.transform.SetParent(tabBar.transform, false);
            _tabBumperLeft.transform.SetSiblingIndex(0);

            LayoutElement leLeft = _tabBumperLeft.GetComponent<LayoutElement>();
            leLeft.minWidth = 36f;
            leLeft.preferredWidth = 36f;
            leLeft.minHeight = 36f;
            leLeft.preferredHeight = 36f;
            leLeft.flexibleWidth = 0f;

            TextMeshProUGUI tmpLeft = _tabBumperLeft.GetComponent<TextMeshProUGUI>();
            tmpLeft.font = UIFonts.GetPrimaryFont();
            tmpLeft.fontSize = 18f;
            tmpLeft.alignment = TextAlignmentOptions.Center;
            tmpLeft.overflowMode = TextOverflowModes.Overflow;
            tmpLeft.textWrappingMode = TextWrappingModes.NoWrap;
            tmpLeft.raycastTarget = false;

            _tabBumperRight = new GameObject("TabBumperRight", typeof(RectTransform), typeof(TextMeshProUGUI), typeof(LayoutElement));
            _tabBumperRight.transform.SetParent(tabBar.transform, false);
            _tabBumperRight.transform.SetAsLastSibling();

            LayoutElement leRight = _tabBumperRight.GetComponent<LayoutElement>();
            leRight.minWidth = 36f;
            leRight.preferredWidth = 36f;
            leRight.minHeight = 36f;
            leRight.preferredHeight = 36f;
            leRight.flexibleWidth = 0f;

            TextMeshProUGUI tmpRight = _tabBumperRight.GetComponent<TextMeshProUGUI>();
            tmpRight.font = UIFonts.GetPrimaryFont();
            tmpRight.fontSize = 18f;
            tmpRight.alignment = TextAlignmentOptions.Center;
            tmpRight.overflowMode = TextOverflowModes.Overflow;
            tmpRight.textWrappingMode = TextWrappingModes.NoWrap;
            tmpRight.raycastTarget = false;
        }

        if (_window.TabButtons != null)
        {
            for (int i = 0; i < _window.TabButtons.Length; i++)
            {
                Button tb = _window.TabButtons[i];
                if (tb != null)
                {
                    Navigation nav = tb.navigation;
                    nav.mode = Navigation.Mode.None;
                    tb.navigation = nav;
                }
            }
        }

        _gamepadLegend = new GameObject("GamepadLegend", typeof(RectTransform), typeof(TextMeshProUGUI));
        _gamepadLegend.transform.SetParent(_panel.transform, false);

        RectTransform legendRt = _gamepadLegend.GetComponent<RectTransform>();
        legendRt.anchorMin = new Vector2(0f, 0f);
        legendRt.anchorMax = new Vector2(1f, 0f);
        legendRt.pivot = new Vector2(0.5f, 1f);
        legendRt.offsetMin = new Vector2(30f, -28f);
        legendRt.offsetMax = new Vector2(-30f, -6f);

        _legendText = _gamepadLegend.GetComponent<TextMeshProUGUI>();
        _legendText.font = UIFonts.GetPrimaryFont();
        _legendText.fontSize = 12f;
        _legendText.alignment = TextAlignmentOptions.Center;
        _legendText.color = WardColorPalette.ValheimBeige;

        UpdateKeyhints(WardGamepadNavHelper.IsGamepadActive());
    }

    public void OnWindowOpened()
    {
        _currentSelected = null;
        _lastFocusedIndex = 0;
        bool isGamepad = WardGamepadNavHelper.IsGamepadActive();
        UpdateKeyhints(isGamepad);
        if (isGamepad)
        {
            FocusFirstElement();
        }
        else
        {
            WardGamepadNavHelper.AttachFocusReticle(_focusReticle, null);
        }
    }

    public void OnTabChanged()
    {
        _currentSelected = null;
        _lastFocusedIndex = 0;
        if (_window != null && _window.TabManagement != null)
        {
            _window.TabManagement.HideHoverTooltip();
        }
        if (WardGamepadNavHelper.IsGamepadActive())
        {
            FocusFirstElement();
        }
        else
        {
            WardGamepadNavHelper.AttachFocusReticle(_focusReticle, null);
        }
    }

    public void UpdateKeyhints(bool isGamepad)
    {
        _wasGamepadActive = isGamepad;

        if (_tabBumperLeft != null)
        {
            _tabBumperLeft.SetActive(isGamepad);
            if (isGamepad)
            {
                TextMeshProUGUI tmpLeft = _tabBumperLeft.GetComponent<TextMeshProUGUI>();
                if (tmpLeft != null)
                {
                    tmpLeft.text = WardGamepadNavHelper.GetGamepadGlyph("LB");
                }
            }
        }

        if (_tabBumperRight != null)
        {
            _tabBumperRight.SetActive(isGamepad);
            if (isGamepad)
            {
                TextMeshProUGUI tmpRight = _tabBumperRight.GetComponent<TextMeshProUGUI>();
                if (tmpRight != null)
                {
                    tmpRight.text = WardGamepadNavHelper.GetGamepadGlyph("RB");
                }
            }
        }

        if (_gamepadLegend != null)
        {
            _gamepadLegend.SetActive(isGamepad);
            if (isGamepad && _legendText != null)
            {
                string dpad = WardGamepadNavHelper.GetGamepadGlyph("DPad");
                string lt = WardGamepadNavHelper.GetGamepadGlyph("LT");
                string rt = WardGamepadNavHelper.GetGamepadGlyph("RT");
                string a = WardGamepadNavHelper.GetGamepadGlyph("A");
                string rstick = WardGamepadNavHelper.GetGamepadGlyph("RStick");
                string b = WardGamepadNavHelper.GetGamepadGlyph("B");

                _legendText.text = $"<color=#e2d6b5>{dpad} Navigate   {lt}{rt} Sections   {a} Select/Toggle   {rstick} Scroll   {b} Close</color>";
            }
        }

        _window.UpdateTabButtonDimensions();
    }

    public void UpdateNavigation()
    {
        bool isGamepad = WardGamepadNavHelper.IsGamepadActive();
        if (isGamepad != _wasGamepadActive)
        {
            UpdateKeyhints(isGamepad);
            _window.RefreshCurrentKeyhints();
        }

        if (!isGamepad)
        {
            WardGamepadNavHelper.AttachFocusReticle(_focusReticle, null);
            return;
        }

        if (_window.IsRemoteSelectorOpen)
        {
            WardGamepadNavHelper.AttachFocusReticle(_focusReticle, null);
            if (_window.RemoteSelector != null)
            {
                _window.RemoteSelector.UpdateNavigation();
            }
            return;
        }

        if (IsDropdownOpen(out GameObject dropdownList))
        {
            WardGamepadNavHelper.AttachFocusReticle(_focusReticle, null);

            if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject != null)
            {
                Selectable sel = EventSystem.current.currentSelectedGameObject.GetComponent<Selectable>();
                if (sel != null && dropdownList != null && sel.transform.IsChildOf(dropdownList.transform))
                {
                    ScrollRect ddSr = dropdownList.GetComponentInChildren<ScrollRect>(true);
                    RectTransform selRt = sel.transform as RectTransform;
                    if (ddSr != null && selRt != null)
                    {
                        WardGamepadNavHelper.EnsureVisible(ddSr, selRt);

                        float rStick = WardGamepadNavHelper.GetRightStickScrollDelta();
                        if (Mathf.Abs(rStick) > 0.15f)
                        {
                            float scrollDelta = rStick * 2.5f * Time.unscaledDeltaTime;
                            ddSr.verticalNormalizedPosition = Mathf.Clamp01(ddSr.verticalNormalizedPosition + scrollDelta);
                        }
                    }
                }
            }

            return;
        }
        else
        {
            if (_focusReticle != null && _panel != null && _focusReticle.transform.parent != _panel.transform)
            {
                _focusReticle.transform.SetParent(_panel.transform, false);
            }
        }

        if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject != null)
        {
            Selectable eventSelected = EventSystem.current.currentSelectedGameObject.GetComponent<Selectable>();
            GameObject activeRoot = _window.GetActiveTabRoot();
            if (eventSelected != null && eventSelected != _currentSelected && !(eventSelected is Scrollbar)
                && eventSelected.gameObject.activeInHierarchy
                && activeRoot != null && eventSelected.transform.IsChildOf(activeRoot.transform))
            {
                _currentSelected = eventSelected;
                WardGamepadNavHelper.AttachFocusReticle(_focusReticle, _currentSelected);
            }
        }

        if (_currentSelected == null || !_currentSelected.gameObject.activeInHierarchy)
        {
            RestoreFocusOrSelectFirst();
        }

        HandleTabSwitching();
        HandleSectionJumping();
        HandleDirectShortcuts();
        HandleDirectionalNavigation();
        HandleRightStickScrolling();

        if (_currentSelected != null && _currentSelected.gameObject.activeInHierarchy)
        {
            WardGamepadNavHelper.AttachFocusReticle(_focusReticle, _currentSelected);
        }
        else
        {
            WardGamepadNavHelper.AttachFocusReticle(_focusReticle, null);
        }
    }

    private void HandleTabSwitching()
    {
        if (WardGamepadNavHelper.IsButtonDownLB())
        {
            _window.NavigateTabPrevious();
        }
        else if (WardGamepadNavHelper.IsButtonDownRB())
        {
            _window.NavigateTabNext();
        }
    }

    private void HandleDirectShortcuts()
    {
        if (WardGamepadNavHelper.IsButtonDownY())
        {
            bool progressionEnabled = ConfigRegistry.EnableProgressionSystem != null && ConfigRegistry.EnableProgressionSystem.Value;
            if (progressionEnabled)
            {
                _window.OnFooterAttunementClicked();
            }
        }
        else if (WardGamepadNavHelper.IsButtonDownX())
        {
            _window.OnOtherWardsClicked();
        }
        else if (WardGamepadNavHelper.IsButtonDownLS())
        {
            _window.OnCycleFontSizeClicked();
        }
    }

    private void HandleSectionJumping()
    {
        if (!WardGamepadNavHelper.GetTriggerInput(out bool lt, out bool rt))
            return;

        GameObject activeRoot = _window.GetActiveTabRoot();
        if (activeRoot == null)
            return;

        ScrollRect scrollRect = activeRoot.GetComponentInChildren<ScrollRect>(true);
        Transform listContainer = (scrollRect != null && scrollRect.content != null) ? scrollRect.content : activeRoot.transform;

        List<Transform> cards = WardGamepadNavHelper.FindCards(listContainer);
        if (cards.Count == 0)
            return;

        int currentCardIndex = -1;
        if (_currentSelected != null)
        {
            for (int i = 0; i < cards.Count; i++)
            {
                if (_currentSelected.transform.IsChildOf(cards[i]))
                {
                    currentCardIndex = i;
                    break;
                }
            }
        }

        int targetCardIndex = currentCardIndex;
        if (rt)
        {
            targetCardIndex = (currentCardIndex + 1) % cards.Count;
        }
        else if (lt)
        {
            targetCardIndex = (currentCardIndex - 1 + cards.Count) % cards.Count;
        }

        for (int attempts = 0; attempts < cards.Count; attempts++)
        {
            Transform card = cards[targetCardIndex];
            List<Selectable> selectables = WardGamepadNavHelper.FindSelectables(card.gameObject);
            if (selectables.Count > 0)
            {
                SetFocus(selectables[0], scrollRect);
                RectTransform cardRt = card as RectTransform;
                if (cardRt != null && scrollRect != null)
                {
                    WardGamepadNavHelper.EnsureVisible(scrollRect, cardRt);
                }
                return;
            }

            targetCardIndex = rt
                ? (targetCardIndex + 1) % cards.Count
                : (targetCardIndex - 1 + cards.Count) % cards.Count;
        }
    }

    public List<Selectable> GetContentSelectables()
    {
        List<Selectable> list = new();

        GameObject activeRoot = _window.GetActiveTabRoot();
        if (activeRoot != null)
        {
            List<Selectable> contentSel = WardGamepadNavHelper.FindSelectables(activeRoot);
            list.AddRange(contentSel);
        }

        if (_window.FooterAttunementBtn != null && _window.FooterAttunementBtn.gameObject.activeInHierarchy && _window.FooterAttunementBtn.interactable)
        {
            Navigation nav = _window.FooterAttunementBtn.navigation;
            nav.mode = Navigation.Mode.None;
            _window.FooterAttunementBtn.navigation = nav;

            list.Add(_window.FooterAttunementBtn);
            WardGamepadNavHelper.ApplyGamepadColorBlock(_window.FooterAttunementBtn);
        }

        return list;
    }

    private void HandleDirectionalNavigation()
    {
        if (WardGamepadNavHelper.IsButtonDownA())
        {
            SubmitCurrent();
            return;
        }

        if (!WardGamepadNavHelper.GetDirectionInput(out bool up, out bool down, out bool left, out bool right))
        {
            return;
        }

        List<Selectable> selectables = GetContentSelectables();
        if (selectables.Count == 0)
            return;

        GameObject activeRoot = _window.GetActiveTabRoot();
        if (activeRoot == null)
            return;

        Transform leftPanel = activeRoot.transform.Find("LeftPanel");
        Transform rightPanel = activeRoot.transform.Find("RightPanel");

        if (leftPanel != null && rightPanel != null)
        {
            List<Selectable> leftSel = WardGamepadNavHelper.FindSelectables(leftPanel.gameObject);
            List<Selectable> rightSel = WardGamepadNavHelper.FindSelectables(rightPanel.gameObject);

            bool inLeft = _currentSelected != null && _currentSelected.transform.IsChildOf(leftPanel);
            bool inRight = _currentSelected != null && _currentSelected.transform.IsChildOf(rightPanel);

            if (inLeft)
            {
                if (right && rightSel.Count > 0)
                {
                    SetFocus(rightSel[0]);
                    return;
                }
                if (down && leftSel.Count > 0)
                {
                    int idx = leftSel.IndexOf(_currentSelected);
                    int next = idx < 0 ? 0 : (idx + 1) % leftSel.Count;
                    SetFocus(leftSel[next]);
                    return;
                }
                if (up && leftSel.Count > 0)
                {
                    int idx = leftSel.IndexOf(_currentSelected);
                    int next = idx < 0 ? leftSel.Count - 1 : (idx - 1 + leftSel.Count) % leftSel.Count;
                    SetFocus(leftSel[next]);
                    return;
                }
                return;
            }
            else if (inRight)
            {
                if (left && leftSel.Count > 0)
                {
                    int targetLeft = 0;
                    for (int i = 0; i < leftSel.Count; i++)
                    {
                        Image img = leftSel[i] != null ? leftSel[i].GetComponent<Image>() : null;
                        if (img != null && img.color == WardColorPalette.ValheimOrange)
                        {
                            targetLeft = i;
                            break;
                        }
                    }
                    SetFocus(leftSel[targetLeft]);
                    return;
                }
                if (down && rightSel.Count > 0)
                {
                    int idx = rightSel.IndexOf(_currentSelected);
                    int next = idx < 0 ? 0 : (idx + 1) % rightSel.Count;
                    SetFocus(rightSel[next]);
                    return;
                }
                if (up && rightSel.Count > 0)
                {
                    int idx = rightSel.IndexOf(_currentSelected);
                    int next = idx < 0 ? rightSel.Count - 1 : (idx - 1 + rightSel.Count) % rightSel.Count;
                    SetFocus(rightSel[next]);
                    return;
                }
                return;
            }
        }

        int curIdx = (_currentSelected != null) ? selectables.IndexOf(_currentSelected) : -1;

        if (left || right)
        {
            if (_currentSelected != null)
            {
                Slider slider = _currentSelected as Slider;
                if (slider != null && slider.interactable)
                {
                    float step = slider.wholeNumbers ? 1f : 5f;
                    float newVal = right ? slider.value + step : slider.value - step;
                    slider.value = Mathf.Clamp(newVal, slider.minValue, slider.maxValue);
                    return;
                }

                Transform parentRow = _currentSelected.transform.parent;
                if (parentRow != null)
                {
                    List<Selectable> rowSelectables = WardGamepadNavHelper.FindSelectables(parentRow.gameObject);
                    if (rowSelectables.Count > 1)
                    {
                        int rowIdx = rowSelectables.IndexOf(_currentSelected);
                        if (rowIdx >= 0)
                        {
                            int nextRowIdx = right
                                ? (rowIdx + 1) % rowSelectables.Count
                                : (rowIdx - 1 + rowSelectables.Count) % rowSelectables.Count;
                            SetFocus(rowSelectables[nextRowIdx]);
                            return;
                        }
                    }
                }
            }

            int hIdx = curIdx;
            if (right)
            {
                hIdx = curIdx < 0 ? 0 : (curIdx + 1) % selectables.Count;
            }
            else if (left)
            {
                hIdx = curIdx < 0 ? selectables.Count - 1 : (curIdx - 1 + selectables.Count) % selectables.Count;
            }
            SetFocus(selectables[hIdx]);
            return;
        }

        if (down)
        {
            int nextIdx = curIdx < 0 ? 0 : (curIdx + 1) % selectables.Count;
            SetFocus(selectables[nextIdx]);
            return;
        }

        if (up)
        {
            int nextIdx = curIdx < 0 ? selectables.Count - 1 : (curIdx - 1 + selectables.Count) % selectables.Count;
            SetFocus(selectables[nextIdx]);
            return;
        }
    }

    private void HandleRightStickScrolling()
    {
        float rightStickY = WardGamepadNavHelper.GetRightStickScrollDelta();
        if (Mathf.Abs(rightStickY) > 0.15f)
        {
            GameObject activeRoot = _window.GetActiveTabRoot();
            if (activeRoot == null)
                return;

            ScrollRect scrollRect = null;
            if (_currentSelected != null)
            {
                scrollRect = _currentSelected.GetComponentInParent<ScrollRect>();
            }

            Transform rightPanel = activeRoot.transform.Find("RightPanel");
            if (rightPanel != null)
            {
                ScrollRect rightSr = rightPanel.GetComponentInChildren<ScrollRect>(true);
                if (rightSr != null && rightSr.content != null)
                {
                    RectTransform rightVp = rightSr.viewport != null ? rightSr.viewport : (RectTransform)rightSr.transform;
                    bool rightCanScroll = rightSr.content.rect.height > rightVp.rect.height;

                    if (scrollRect == null || scrollRect == rightSr)
                    {
                        scrollRect = rightSr;
                    }
                    else
                    {
                        RectTransform curVp = scrollRect.viewport != null ? scrollRect.viewport : (RectTransform)scrollRect.transform;
                        bool curCanScroll = scrollRect.content != null && scrollRect.content.rect.height > curVp.rect.height;
                        if (!curCanScroll && rightCanScroll)
                        {
                            scrollRect = rightSr;
                        }
                    }
                }
            }

            if (scrollRect == null)
            {
                scrollRect = activeRoot.GetComponentInChildren<ScrollRect>(true);
            }

            if (scrollRect != null)
            {
                float scrollDelta = rightStickY * 2.5f * Time.unscaledDeltaTime;
                scrollRect.verticalNormalizedPosition = Mathf.Clamp01(scrollRect.verticalNormalizedPosition + scrollDelta);
                if (_window != null && _window.CurrentTabIndex == 5 && _window.TabManagement != null)
                {
                    _window.TabManagement.UpdateHoverTooltipPosition();
                }
            }
        }
    }

    public bool IsDropdownOpen(out GameObject dropdownList)
    {
        dropdownList = null;

        if (_panel != null)
        {
            Canvas canvas = _panel.GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                Transform ddList = canvas.transform.Find("Dropdown List");
                if (ddList != null && ddList.gameObject.activeInHierarchy)
                {
                    dropdownList = ddList.gameObject;
                    return true;
                }
            }

            Transform localDdList = _panel.transform.Find("Dropdown List");
            if (localDdList != null && localDdList.gameObject.activeInHierarchy)
            {
                dropdownList = localDdList.gameObject;
                return true;
            }
        }

        if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject != null)
        {
            Transform current = EventSystem.current.currentSelectedGameObject.transform;
            while (current != null)
            {
                if (current.name == "Dropdown List")
                {
                    dropdownList = current.gameObject;
                    return true;
                }
                current = current.parent;
            }
        }

        return false;
    }

    public bool IsDropdownOpen()
    {
        return IsDropdownOpen(out _);
    }

    private int _lastSubmitFrame;
    private float _lastSubmitTime;

    private void SubmitCurrent()
    {
        if (_currentSelected == null || !_currentSelected.interactable || !_currentSelected.gameObject.activeInHierarchy)
            return;

        if (Time.frameCount == _lastSubmitFrame || Time.unscaledTime - _lastSubmitTime < 0.25f)
            return;
        _lastSubmitFrame = Time.frameCount;
        _lastSubmitTime = Time.unscaledTime;

        InputField input = _currentSelected as InputField;
        if (input != null)
        {
            if (EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(input.gameObject);
            }
            input.ActivateInputField();
            return;
        }

        TMP_InputField tmpInput = _currentSelected as TMP_InputField;
        if (tmpInput != null)
        {
            if (EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(tmpInput.gameObject);
            }
            tmpInput.ActivateInputField();
            return;
        }

        if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject == _currentSelected.gameObject)
        {
            return;
        }

        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(_currentSelected.gameObject);
            BaseEventData eventData = new(EventSystem.current);
            ExecuteEvents.Execute(_currentSelected.gameObject, eventData, ExecuteEvents.submitHandler);
            return;
        }

        Button btn = _currentSelected as Button;
        if (btn != null)
        {
            btn.onClick?.Invoke();
            return;
        }

        Toggle toggle = _currentSelected as Toggle;
        if (toggle != null)
        {
            toggle.isOn = !toggle.isOn;
            return;
        }

        Dropdown dropdown = _currentSelected as Dropdown;
        if (dropdown != null)
        {
            dropdown.Show();
            return;
        }
    }

    public void FocusFirstElement()
    {
        List<Selectable> selectables = GetContentSelectables();
        if (selectables.Count == 0)
        {
            WardGamepadNavHelper.AttachFocusReticle(_focusReticle, null);
            return;
        }

        Selectable target = null;
        for (int i = 0; i < selectables.Count; i++)
        {
            Selectable s = selectables[i];
            if (s != null && s.interactable && s.gameObject.activeInHierarchy && !(s is InputField) && !(s is TMP_InputField))
            {
                target = s;
                break;
            }
        }

        if (target == null)
        {
            target = selectables[0];
        }

        SetFocus(target);
    }

    public void RestoreFocusOrSelectFirst()
    {
        List<Selectable> selectables = GetContentSelectables();
        if (selectables.Count == 0)
        {
            WardGamepadNavHelper.AttachFocusReticle(_focusReticle, null);
            return;
        }

        int targetIndex = Mathf.Clamp(_lastFocusedIndex, 0, selectables.Count - 1);
        SetFocus(selectables[targetIndex]);
    }

    private void SetFocus(Selectable selectable, ScrollRect scrollRect = null)
    {
        if (selectable == null)
        {
            WardGamepadNavHelper.AttachFocusReticle(_focusReticle, null);
            UpdateManagementTooltip();
            return;
        }

        _currentSelected = selectable;
        List<Selectable> selectables = GetContentSelectables();
        int idx = selectables.IndexOf(selectable);
        if (idx >= 0)
        {
            _lastFocusedIndex = idx;
        }

        selectable.Select();
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(selectable.gameObject);
        }

        WardGamepadNavHelper.ApplyGamepadColorBlock(selectable);
        WardGamepadNavHelper.AttachFocusReticle(_focusReticle, selectable);
        UpdateManagementTooltip();

        ScrollRect targetSr = scrollRect != null ? scrollRect : selectable.GetComponentInParent<ScrollRect>();
        RectTransform rt = selectable.transform as RectTransform;
        if (rt != null && targetSr != null)
        {
            WardGamepadNavHelper.EnsureVisible(targetSr, rt);
        }
    }

    private void UpdateManagementTooltip()
    {
        if (_window == null || _window.TabManagement == null)
            return;

        if (_window.CurrentTabIndex != 5)
        {
            _window.TabManagement.HideHoverTooltip();
            return;
        }

        if (_currentSelected != null && _currentSelected.gameObject.activeInHierarchy)
        {
            RowHoverTooltipTrigger trigger = _currentSelected.GetComponentInParent<RowHoverTooltipTrigger>();
            if (trigger != null && trigger.Permission != null)
            {
                _window.TabManagement.ShowHoverTooltip(trigger.Permission, trigger.GetComponent<RectTransform>());
                return;
            }
        }

        _window.TabManagement.HideHoverTooltip();
    }
}
