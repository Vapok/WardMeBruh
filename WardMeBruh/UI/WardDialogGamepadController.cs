using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using WardMeBruh.Configuration;

namespace WardMeBruh.UI;

public class WardDialogGamepadController
{
    private readonly WardMainWindow _window;
    private readonly GameObject _panel;

    private GameObject _tabBumperLeft;
    private GameObject _tabBumperRight;
    private GameObject _gamepadLegend;
    private TextMeshProUGUI _legendText;

    private Selectable _currentSelected;
    private bool _wasGamepadActive;

    public WardDialogGamepadController(WardMainWindow window, GameObject panel)
    {
        _window = window;
        _panel = panel;
    }

    public void InitializeElements(GameObject tabBar, RectTransform panelRt, TextMeshProUGUI otherWardsText, TextMeshProUGUI closeText)
    {
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
            tmpLeft.text = "<color=yellow><b>LB</b></color>";
            tmpLeft.fontSize = 13f;
            tmpLeft.alignment = TextAlignmentOptions.Center;

            _tabBumperRight = new GameObject("TabBumperRight", typeof(RectTransform), typeof(TextMeshProUGUI), typeof(LayoutElement));
            _tabBumperRight.transform.SetParent(tabBar.transform, false);

            LayoutElement leRight = _tabBumperRight.GetComponent<LayoutElement>();
            leRight.minWidth = 36f;
            leRight.preferredWidth = 36f;
            leRight.minHeight = 36f;
            leRight.preferredHeight = 36f;
            leRight.flexibleWidth = 0f;

            TextMeshProUGUI tmpRight = _tabBumperRight.GetComponent<TextMeshProUGUI>();
            tmpRight.font = UIFonts.GetPrimaryFont();
            tmpRight.text = "<color=yellow><b>RB</b></color>";
            tmpRight.fontSize = 13f;
            tmpRight.alignment = TextAlignmentOptions.Center;
        }

        _gamepadLegend = new GameObject("GamepadLegend", typeof(RectTransform), typeof(TextMeshProUGUI));
        _gamepadLegend.transform.SetParent(_panel.transform, false);

        RectTransform legendRt = _gamepadLegend.GetComponent<RectTransform>();
        legendRt.anchorMin = new Vector2(0f, 0f);
        legendRt.anchorMax = new Vector2(1f, 0f);
        legendRt.pivot = new Vector2(0.5f, 0f);
        legendRt.offsetMin = new Vector2(30f, 2f);
        legendRt.offsetMax = new Vector2(-30f, 16f);

        _legendText = _gamepadLegend.GetComponent<TextMeshProUGUI>();
        _legendText.font = UIFonts.GetPrimaryFont();
        _legendText.text = "<color=#e2d6b5><b>[D-Pad]</b> Navigate   <b>[LT/RT]</b> Sections   <b>[A]</b> Select/Toggle   <b>[R-Stick]</b> Scroll</color>";
        _legendText.fontSize = 11f;
        _legendText.alignment = TextAlignmentOptions.Center;
        _legendText.color = WardColorPalette.ValheimBeige;

        UpdateKeyhints(ZInput.IsGamepadActive());
    }

    public void OnWindowOpened()
    {
        UpdateKeyhints(ZInput.IsGamepadActive());
        if (ZInput.IsGamepadActive())
        {
            FocusFirstElement();
        }
    }

    public void OnTabChanged()
    {
        if (ZInput.IsGamepadActive())
        {
            FocusFirstElement();
        }
    }

    public void UpdateKeyhints(bool isGamepad)
    {
        _wasGamepadActive = isGamepad;

        if (_tabBumperLeft != null)
        {
            _tabBumperLeft.SetActive(isGamepad);
        }

        if (_tabBumperRight != null)
        {
            _tabBumperRight.SetActive(isGamepad);
        }

        if (_gamepadLegend != null)
        {
            _gamepadLegend.SetActive(isGamepad);
        }
    }

    public void UpdateNavigation()
    {
        bool isGamepad = ZInput.IsGamepadActive();
        if (isGamepad != _wasGamepadActive)
        {
            UpdateKeyhints(isGamepad);
            _window.RefreshCurrentKeyhints();
        }

        if (!isGamepad)
            return;

        if (_window.IsRemoteSelectorOpen)
        {
            if (_window.RemoteSelector != null)
            {
                _window.RemoteSelector.UpdateNavigation();
            }
            return;
        }

        HandleTabSwitching();
        HandleSectionJumping();
        HandleDirectShortcuts();
        HandleDirectionalNavigation();
        HandleRightStickScrolling();
    }

    private void HandleTabSwitching()
    {
        if (ZInput.GetButtonDown("JoyTabLeft"))
        {
            _window.NavigateTabPrevious();
        }
        else if (ZInput.GetButtonDown("JoyTabRight"))
        {
            _window.NavigateTabNext();
        }
    }

    private void HandleDirectShortcuts()
    {
        if (ZInput.GetButtonDown("JoyButtonY"))
        {
            bool progressionEnabled = ConfigRegistry.EnableProgressionSystem != null && ConfigRegistry.EnableProgressionSystem.Value;
            if (progressionEnabled)
            {
                _window.OnFooterAttunementClicked();
            }
        }
        else if (ZInput.GetButtonDown("JoyButtonX"))
        {
            _window.OnOtherWardsClicked();
        }
        else if (ZInput.GetButtonDown("JoyRStick"))
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

    private void HandleDirectionalNavigation()
    {
        if (!WardGamepadNavHelper.GetDirectionInput(out bool up, out bool down, out bool left, out bool right))
        {
            if (ZInput.GetButtonDown("JoyButtonA") || ZInput.GetButtonDown("Use"))
            {
                SubmitCurrent();
            }
            return;
        }

        GameObject activeRoot = _window.GetActiveTabRoot();
        if (activeRoot == null)
            return;

        ScrollRect scrollRect = activeRoot.GetComponentInChildren<ScrollRect>(true);
        List<Selectable> selectables = WardGamepadNavHelper.FindSelectables(activeRoot);
        if (selectables.Count == 0)
            return;

        if (left || right)
        {
            if (_currentSelected != null)
            {
                Slider slider = _currentSelected.GetComponent<Slider>();
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
                            SetFocus(rowSelectables[nextRowIdx], scrollRect);
                            return;
                        }
                    }
                }
            }
        }

        if (up || down)
        {
            int curIdx = (_currentSelected != null) ? selectables.IndexOf(_currentSelected) : -1;
            int nextIdx = 0;

            if (curIdx < 0)
            {
                nextIdx = down ? 0 : selectables.Count - 1;
            }
            else
            {
                nextIdx = down
                    ? (curIdx + 1) % selectables.Count
                    : (curIdx - 1 + selectables.Count) % selectables.Count;
            }

            SetFocus(selectables[nextIdx], scrollRect);
        }
    }

    private void HandleRightStickScrolling()
    {
        float rightStickY = ZInput.GetJoyRightStickY();
        if (Mathf.Abs(rightStickY) > 0.15f)
        {
            GameObject activeRoot = _window.GetActiveTabRoot();
            if (activeRoot == null)
                return;

            ScrollRect scrollRect = activeRoot.GetComponentInChildren<ScrollRect>(true);
            if (scrollRect != null)
            {
                float scrollDelta = rightStickY * 2.5f * Time.unscaledDeltaTime;
                scrollRect.verticalNormalizedPosition = Mathf.Clamp01(scrollRect.verticalNormalizedPosition + scrollDelta);
            }
        }
    }

    private void SubmitCurrent()
    {
        if (_currentSelected == null || !_currentSelected.interactable || !_currentSelected.gameObject.activeInHierarchy)
            return;

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
        }
    }

    public void FocusFirstElement()
    {
        GameObject activeRoot = _window.GetActiveTabRoot();
        if (activeRoot == null)
            return;

        ScrollRect scrollRect = activeRoot.GetComponentInChildren<ScrollRect>(true);
        List<Selectable> selectables = WardGamepadNavHelper.FindSelectables(activeRoot);
        if (selectables.Count > 0)
        {
            SetFocus(selectables[0], scrollRect);
        }
    }

    private void SetFocus(Selectable selectable, ScrollRect scrollRect)
    {
        if (selectable == null)
            return;

        _currentSelected = selectable;
        WardGamepadNavHelper.ApplyGamepadColorBlock(selectable);

        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(selectable.gameObject);
        }

        RectTransform rt = selectable.transform as RectTransform;
        if (rt != null && scrollRect != null)
        {
            WardGamepadNavHelper.EnsureVisible(scrollRect, rt);
        }
    }
}
