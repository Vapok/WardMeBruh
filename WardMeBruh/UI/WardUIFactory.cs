using System;
using Jotunn.Managers;
using UnityEngine;
using UnityEngine.UI;
using WardMeBruh.Models;

namespace WardMeBruh.UI;

public static class WardUIFactory
{
    public static GameObject CreateScrollArea(Transform parent, out Transform listContainer)
    {
        GameObject scrollObj = new GameObject("ScrollArea", typeof(RectTransform), typeof(ScrollRect));
        scrollObj.transform.SetParent(parent, false);
        RectTransform scrollRt = scrollObj.GetComponent<RectTransform>();
        scrollRt.anchorMin = Vector2.zero;
        scrollRt.anchorMax = Vector2.one;
        scrollRt.offsetMin = Vector2.zero;
        scrollRt.offsetMax = Vector2.zero;

        GameObject viewport = new GameObject("Viewport", typeof(RectTransform), typeof(RectMask2D), typeof(Image));
        viewport.transform.SetParent(scrollObj.transform, false);
        Image vpImg = viewport.GetComponent<Image>();
        vpImg.color = Color.clear;
        RectTransform vpRt = viewport.GetComponent<RectTransform>();
        vpRt.anchorMin = Vector2.zero;
        vpRt.anchorMax = Vector2.one;
        vpRt.offsetMin = Vector2.zero;
        vpRt.offsetMax = new Vector2(-22f, 0f);

        GameObject listObj = new GameObject("List", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        listObj.transform.SetParent(viewport.transform, false);
        RectTransform listRt = listObj.GetComponent<RectTransform>();
        listRt.anchorMin = new Vector2(0f, 1f);
        listRt.anchorMax = new Vector2(1f, 1f);
        listRt.pivot = new Vector2(0.5f, 1f);
        listRt.offsetMin = Vector2.zero;
        listRt.offsetMax = Vector2.zero;

        VerticalLayoutGroup vlg = listObj.GetComponent<VerticalLayoutGroup>();
        vlg.spacing = 8f;
        vlg.padding = new RectOffset(4, 4, 4, 12);
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        ContentSizeFitter csf = listObj.GetComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        GameObject scrollbarObj = new GameObject("Scrollbar", typeof(RectTransform), typeof(Scrollbar), typeof(Image));
        scrollbarObj.transform.SetParent(scrollObj.transform, false);
        RectTransform sbRt = scrollbarObj.GetComponent<RectTransform>();
        sbRt.anchorMin = new Vector2(1f, 0f);
        sbRt.anchorMax = new Vector2(1f, 1f);
        sbRt.pivot = new Vector2(1f, 0.5f);
        sbRt.sizeDelta = new Vector2(16f, 0f);
        sbRt.anchoredPosition = Vector2.zero;

        Image sbImg = scrollbarObj.GetComponent<Image>();
        sbImg.color = new Color(0.04f, 0.03f, 0.02f, 0.85f);
        sbImg.raycastTarget = true;

        GameObject slidingArea = new GameObject("SlidingArea", typeof(RectTransform));
        slidingArea.transform.SetParent(scrollbarObj.transform, false);
        RectTransform saRt = slidingArea.GetComponent<RectTransform>();
        saRt.anchorMin = Vector2.zero;
        saRt.anchorMax = Vector2.one;
        saRt.offsetMin = new Vector2(0f, 6f);
        saRt.offsetMax = new Vector2(0f, -6f);

        GameObject handleObj = new GameObject("Handle", typeof(RectTransform), typeof(Image));
        handleObj.transform.SetParent(slidingArea.transform, false);
        RectTransform handleRt = handleObj.GetComponent<RectTransform>();
        handleRt.anchorMin = Vector2.zero;
        handleRt.anchorMax = Vector2.one;
        handleRt.offsetMin = Vector2.zero;
        handleRt.offsetMax = Vector2.zero;

        Image handleImg = handleObj.GetComponent<Image>();
        handleImg.color = WardColorPalette.GoldAmber;
        handleImg.raycastTarget = true;

        Scrollbar scrollbar = scrollbarObj.GetComponent<Scrollbar>();
        scrollbar.handleRect = handleRt;
        scrollbar.targetGraphic = handleImg;
        scrollbar.direction = Scrollbar.Direction.BottomToTop;

        ScrollRect scrollRect = scrollObj.GetComponent<ScrollRect>();
        scrollRect.content = listRt;
        scrollRect.viewport = vpRt;
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.scrollSensitivity = 380f;
        scrollRect.inertia = true;
        scrollRect.decelerationRate = 0.5f;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        scrollRect.verticalScrollbar = scrollbar;
        scrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.Permanent;

        listContainer = listObj.transform;
        return scrollObj;
    }

    public static Transform CreateCard(Transform parent, string title)
    {
        GameObject outerObj = new GameObject($"Card_{title}", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        outerObj.transform.SetParent(parent, false);

        Image outerImg = outerObj.GetComponent<Image>();
        outerImg.color = WardColorPalette.CardBorder;

        VerticalLayoutGroup outerVlg = outerObj.GetComponent<VerticalLayoutGroup>();
        outerVlg.padding = new RectOffset(1, 1, 1, 1);
        outerVlg.childControlWidth = true;
        outerVlg.childControlHeight = true;
        outerVlg.childForceExpandWidth = true;
        outerVlg.childForceExpandHeight = false;

        ContentSizeFitter outerCsf = outerObj.GetComponent<ContentSizeFitter>();
        outerCsf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        GameObject cardObj = new GameObject("CardInner", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        cardObj.transform.SetParent(outerObj.transform, false);

        Image cardImg = cardObj.GetComponent<Image>();
        cardImg.color = WardColorPalette.CardBackground;

        VerticalLayoutGroup vlg = cardObj.GetComponent<VerticalLayoutGroup>();
        vlg.spacing = 0f;
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        ContentSizeFitter csf = cardObj.GetComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        GameObject headerObj = new GameObject("CardHeader", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
        headerObj.transform.SetParent(cardObj.transform, false);
        Image headerImg = headerObj.GetComponent<Image>();
        headerImg.color = WardColorPalette.CardHeader;
        headerObj.SetLayoutDimensions(-1f, 28f);

        GameObject headerTextObj = new GameObject("HeaderText", typeof(RectTransform), typeof(Text));
        headerTextObj.transform.SetParent(headerObj.transform, false);
        RectTransform htRt = headerTextObj.GetComponent<RectTransform>();
        htRt.anchorMin = Vector2.zero;
        htRt.anchorMax = Vector2.one;
        htRt.offsetMin = new Vector2(12f, 0f);
        htRt.offsetMax = new Vector2(-12f, 0f);

        Text headerText = headerTextObj.GetComponent<Text>();
        headerText.text = title.ToUpperInvariant();
        headerText.font = GUIManager.Instance.AveriaSerifBold;
        headerText.fontSize = 13;
        headerText.color = WardColorPalette.ValheimOrange;
        headerText.alignment = TextAnchor.MiddleLeft;

        GameObject bodyObj = new GameObject("CardBody", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        bodyObj.transform.SetParent(cardObj.transform, false);

        VerticalLayoutGroup bodyVlg = bodyObj.GetComponent<VerticalLayoutGroup>();
        bodyVlg.spacing = 2f;
        bodyVlg.padding = new RectOffset(6, 6, 6, 6);
        bodyVlg.childControlWidth = true;
        bodyVlg.childControlHeight = true;
        bodyVlg.childForceExpandWidth = true;
        bodyVlg.childForceExpandHeight = false;

        ContentSizeFitter bodyCsf = bodyObj.GetComponent<ContentSizeFitter>();
        bodyCsf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        return bodyObj.transform;
    }

    public static GameObject CreateSettingRow(Transform parent, string labelText, out Transform controlContainer, float rowHeight = 34f)
    {
        GameObject rowObj = new GameObject($"Row_{labelText}", typeof(RectTransform), typeof(Image), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
        rowObj.transform.SetParent(parent, false);
        rowObj.SetLayoutDimensions(-1f, rowHeight);

        Image rowBg = rowObj.GetComponent<Image>();
        int siblingIndex = parent.childCount;
        rowBg.color = (siblingIndex % 2 == 1) ? WardColorPalette.RowOdd : WardColorPalette.RowEven;

        HorizontalLayoutGroup hlg = rowObj.GetComponent<HorizontalLayoutGroup>();
        hlg.padding = new RectOffset(10, 10, 2, 2);
        hlg.spacing = 10f;
        hlg.childControlWidth = true;
        hlg.childControlHeight = true;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = false;
        hlg.childAlignment = TextAnchor.MiddleLeft;

        GameObject labelObj = new GameObject("Label", typeof(RectTransform), typeof(Text), typeof(LayoutElement));
        labelObj.transform.SetParent(rowObj.transform, false);
        labelObj.GetComponent<RectTransform>().sizeDelta = new Vector2(330f, rowHeight);
        labelObj.SetLayoutDimensions(330f, rowHeight, 280f, rowHeight);

        Text label = labelObj.GetComponent<Text>();
        label.text = labelText;
        label.font = GUIManager.Instance.AveriaSerifBold;
        label.fontSize = 13;
        label.color = WardColorPalette.ValheimBeige;
        label.alignment = TextAnchor.MiddleLeft;
        label.horizontalOverflow = HorizontalWrapMode.Wrap;
        label.verticalOverflow = VerticalWrapMode.Truncate;

        GameObject controlsObj = new GameObject("Controls", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
        controlsObj.transform.SetParent(rowObj.transform, false);
        controlsObj.GetComponent<RectTransform>().sizeDelta = new Vector2(410f, rowHeight);
        LayoutElement ctrlLe = controlsObj.SetLayoutDimensions(410f, rowHeight, 200f, rowHeight);
        ctrlLe.flexibleWidth = 1f;

        HorizontalLayoutGroup ctrlHlg = controlsObj.GetComponent<HorizontalLayoutGroup>();
        ctrlHlg.spacing = 8f;
        ctrlHlg.childControlWidth = false;
        ctrlHlg.childControlHeight = true;
        ctrlHlg.childForceExpandWidth = false;
        ctrlHlg.childForceExpandHeight = false;
        ctrlHlg.childAlignment = TextAnchor.MiddleRight;

        controlContainer = controlsObj.transform;
        return rowObj;
    }

    public static void CreateToggleRow(Transform parent, string labelText, bool initialValue, Action<bool> onToggle, out Action<bool> setValue)
    {
        CreateSettingRow(parent, labelText, out Transform controlContainer);

        GameObject btnObj = GUIManager.Instance.CreateButton(
            initialValue ? "✔  ENABLED" : "✖  DISABLED",
            controlContainer,
            Vector2.zero,
            Vector2.zero,
            Vector2.zero,
            130f,
            28f);

        btnObj.GetComponent<RectTransform>().sizeDelta = new Vector2(130f, 28f);
        btnObj.SetLayoutDimensions(130f, 28f);
        Text btnText = btnObj.GetComponentInChildren<Text>();
        btnText.font = GUIManager.Instance.AveriaSerifBold;
        btnText.fontSize = 13;
        btnText.color = initialValue ? WardColorPalette.RunicGreen : WardColorPalette.TextMuted;

        bool currentValue = initialValue;

        void UpdateVisual(bool state)
        {
            currentValue = state;
            btnText.text = state ? "✔  ENABLED" : "✖  DISABLED";
            btnText.color = state ? WardColorPalette.RunicGreen : WardColorPalette.TextMuted;
        }

        btnObj.GetComponent<Button>().onClick.AddListener(() =>
        {
            bool nextState = !currentValue;
            UpdateVisual(nextState);
            onToggle?.Invoke(nextState);
        });

        setValue = UpdateVisual;
    }

    public static Color GetRoleColor(WardRole role)
    {
        return role switch
        {
            WardRole.Owner => WardColorPalette.ValheimOrange,
            WardRole.Manager => WardColorPalette.GoldAmber,
            WardRole.Resident => WardColorPalette.IceBlue,
            _ => WardColorPalette.TextMuted
        };
    }

    public static void CreateFeatureToggleRow(
        Transform parent,
        string labelText,
        bool initialToggle,
        WardRole initialRole,
        Action<bool> onToggle,
        Action<WardRole> onRoleChanged,
        out Action<bool, WardRole, bool> updateVisual)
    {
        CreateSettingRow(parent, labelText, out Transform controlContainer);

        GameObject toggleBtnObj = GUIManager.Instance.CreateButton(
            initialToggle ? "✔  ENABLED" : "✖  DISABLED",
            controlContainer,
            Vector2.zero,
            Vector2.zero,
            Vector2.zero,
            120f,
            28f);
        toggleBtnObj.GetComponent<RectTransform>().sizeDelta = new Vector2(120f, 28f);
        toggleBtnObj.SetLayoutDimensions(120f, 28f);

        Text toggleTxt = toggleBtnObj.GetComponentInChildren<Text>();
        toggleTxt.font = GUIManager.Instance.AveriaSerifBold;
        toggleTxt.fontSize = 12;
        toggleTxt.color = initialToggle ? WardColorPalette.RunicGreen : WardColorPalette.TextMuted;

        Button toggleBtn = toggleBtnObj.GetComponent<Button>();
        bool currentToggle = initialToggle;

        GameObject roleBtnObj = GUIManager.Instance.CreateButton(
            $"[ Role: {initialRole} ]",
            controlContainer,
            Vector2.zero,
            Vector2.zero,
            Vector2.zero,
            135f,
            28f);
        roleBtnObj.GetComponent<RectTransform>().sizeDelta = new Vector2(135f, 28f);
        roleBtnObj.SetLayoutDimensions(135f, 28f);

        Text roleTxt = roleBtnObj.GetComponentInChildren<Text>();
        roleTxt.font = GUIManager.Instance.AveriaSerifBold;
        roleTxt.fontSize = 12;
        roleTxt.color = GetRoleColor(initialRole);

        Button roleBtn = roleBtnObj.GetComponent<Button>();
        WardRole currentRole = initialRole;

        toggleBtn.onClick.AddListener(() =>
        {
            currentToggle = !currentToggle;
            toggleTxt.text = currentToggle ? "✔  ENABLED" : "✖  DISABLED";
            toggleTxt.color = currentToggle ? WardColorPalette.RunicGreen : WardColorPalette.TextMuted;
            onToggle?.Invoke(currentToggle);
        });

        roleBtn.onClick.AddListener(() =>
        {
            currentRole = (WardRole)(((int)currentRole + 1) % 4);
            roleTxt.text = $"[ Role: {currentRole} ]";
            roleTxt.color = GetRoleColor(currentRole);
            onRoleChanged?.Invoke(currentRole);
        });

        updateVisual = (state, role, interactable) =>
        {
            currentToggle = state;
            toggleTxt.text = state ? "✔  ENABLED" : "✖  DISABLED";
            toggleTxt.color = state ? WardColorPalette.RunicGreen : WardColorPalette.TextMuted;

            currentRole = role;
            roleTxt.text = $"[ Role: {role} ]";
            roleTxt.color = GetRoleColor(role);

            toggleBtn.interactable = interactable;
            roleBtn.interactable = interactable;
        };
    }

    public static void CreateSliderRow(
        Transform parent, 
        string labelText, 
        float min, 
        float max, 
        float initialValue, 
        bool wholeNumbers, 
        string unit, 
        Action<float> onValueChanged, 
        out Action<float> setValue)
    {
        CreateSettingRow(parent, labelText, out Transform controlContainer);

        GameObject sliderObj = WardUIExtensions.CreateValheimSlider(controlContainer, min, max, initialValue, wholeNumbers, 250f, 18f);
        sliderObj.GetComponent<RectTransform>().sizeDelta = new Vector2(250f, 18f);
        sliderObj.SetLayoutDimensions(250f, 18f);
        Slider slider = sliderObj.GetComponent<Slider>();

        GameObject readoutObj = new GameObject("Readout", typeof(RectTransform), typeof(Text), typeof(LayoutElement));
        readoutObj.transform.SetParent(controlContainer, false);
        readoutObj.GetComponent<RectTransform>().sizeDelta = new Vector2(65f, 28f);
        readoutObj.SetLayoutDimensions(65f, 28f);

        Text readoutText = readoutObj.GetComponent<Text>();
        readoutText.font = GUIManager.Instance.AveriaSerifBold;
        readoutText.fontSize = 14;
        readoutText.color = WardColorPalette.GoldAmber;
        readoutText.alignment = TextAnchor.MiddleRight;

        string FormatValue(float v)
        {
            return wholeNumbers ? $"{Mathf.RoundToInt(v)}{unit}" : $"{v:F1}{unit}";
        }

        readoutText.text = FormatValue(initialValue);

        slider.onValueChanged.AddListener(v =>
        {
            readoutText.text = FormatValue(v);
            onValueChanged?.Invoke(v);
        });

        setValue = v =>
        {
            slider.SetValueWithoutNotify(v);
            readoutText.text = FormatValue(v);
        };
    }

    public static void CreatePowerRow(
        Transform parent, 
        string labelText, 
        string initialBadge, 
        Color initialBadgeColor, 
        string initialButtonText, 
        Action onButtonClicked, 
        out Action<string, Color, string> updateVisual)
    {
        CreateSettingRow(parent, labelText, out Transform controlContainer);

        GameObject badgeObj = new GameObject("PowerBadge", typeof(RectTransform), typeof(Text), typeof(LayoutElement));
        badgeObj.transform.SetParent(controlContainer, false);
        badgeObj.GetComponent<RectTransform>().sizeDelta = new Vector2(130f, 28f);
        badgeObj.SetLayoutDimensions(130f, 28f);

        Text badgeText = badgeObj.GetComponent<Text>();
        badgeText.text = initialBadge;
        badgeText.font = GUIManager.Instance.AveriaSerifBold;
        badgeText.fontSize = 13;
        badgeText.color = initialBadgeColor;
        badgeText.alignment = TextAnchor.MiddleRight;

        GameObject btnObj = GUIManager.Instance.CreateButton(
            initialButtonText,
            controlContainer,
            Vector2.zero,
            Vector2.zero,
            Vector2.zero,
            125f,
            28f);
        btnObj.GetComponent<RectTransform>().sizeDelta = new Vector2(125f, 28f);
        btnObj.SetLayoutDimensions(125f, 28f);

        Text btnText = btnObj.GetComponentInChildren<Text>();
        btnText.font = GUIManager.Instance.AveriaSerifBold;
        btnText.fontSize = 13;

        btnObj.GetComponent<Button>().onClick.AddListener(() =>
        {
            onButtonClicked?.Invoke();
        });

        updateVisual = (text, color, buttonLabel) =>
        {
            badgeText.text = text;
            badgeText.color = color;
            btnText.text = buttonLabel;
            btnText.color = buttonLabel == "DEACTIVATE" ? WardColorPalette.DangerRed : WardColorPalette.RunicGreen;
        };
    }

    public static void CreateRoleRow(
        Transform parent, 
        string labelText, 
        string initialRoleText, 
        Color initialRoleColor, 
        Action onRoleClicked, 
        out Action<string, Color, bool> updateVisual)
    {
        CreateSettingRow(parent, labelText, out Transform controlContainer);

        GameObject btnObj = GUIManager.Instance.CreateButton(
            initialRoleText,
            controlContainer,
            Vector2.zero,
            Vector2.zero,
            Vector2.zero,
            135f,
            28f);
        btnObj.GetComponent<RectTransform>().sizeDelta = new Vector2(135f, 28f);
        btnObj.SetLayoutDimensions(135f, 28f);

        Text btnText = btnObj.GetComponentInChildren<Text>();
        if (btnText != null)
        {
            btnText.font = GUIManager.Instance.AveriaSerifBold;
            btnText.fontSize = 12;
            btnText.color = initialRoleColor;
        }

        Button btn = btnObj.GetComponent<Button>();
        btn.onClick.AddListener(() =>
        {
            onRoleClicked?.Invoke();
        });

        updateVisual = (text, color, interactable) =>
        {
            if (btnText != null)
            {
                btnText.text = text;
                btnText.color = color;
            }
            if (btn != null)
            {
                btn.interactable = interactable;
            }
        };
    }

    public static void CreateBadgeRow(Transform parent, string labelText, string initialBadge, Color badgeColor, out Action<string, Color> setBadge)
    {
        CreateSettingRow(parent, labelText, out Transform controlContainer);

        GameObject badgeObj = new GameObject("Badge", typeof(RectTransform), typeof(Text), typeof(LayoutElement));
        badgeObj.transform.SetParent(controlContainer, false);
        badgeObj.GetComponent<RectTransform>().sizeDelta = new Vector2(380f, 28f);
        badgeObj.SetLayoutDimensions(380f, 28f);

        Text badgeText = badgeObj.GetComponent<Text>();
        badgeText.text = initialBadge;
        badgeText.font = GUIManager.Instance.AveriaSerifBold;
        badgeText.fontSize = 13;
        badgeText.color = badgeColor;
        badgeText.alignment = TextAnchor.MiddleRight;

        setBadge = (text, color) =>
        {
            badgeText.text = text;
            badgeText.color = color;
        };
    }

    public static void CreateInputRow(Transform parent, string labelText, string initialValue, string buttonText, Action<string> onCommit, out Action<string> setValue)
    {
        CreateSettingRow(parent, labelText, out Transform controlContainer);

        GameObject inputObj = GUIManager.Instance.CreateInputField(
            controlContainer,
            Vector2.zero,
            Vector2.zero,
            Vector2.zero,
            InputField.ContentType.Standard,
            "...",
            14,
            250f,
            30f);
        inputObj.GetComponent<RectTransform>().sizeDelta = new Vector2(250f, 30f);
        inputObj.SetLayoutDimensions(250f, 30f);
        InputField input = inputObj.GetComponent<InputField>();
        input.text = initialValue;

        input.onEndEdit.AddListener(text =>
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                onCommit?.Invoke(text);
            }
        });

        GameObject btnObj = GUIManager.Instance.CreateButton(
            buttonText,
            controlContainer,
            Vector2.zero,
            Vector2.zero,
            Vector2.zero,
            75f,
            30f);
        btnObj.GetComponent<RectTransform>().sizeDelta = new Vector2(75f, 30f);
        btnObj.SetLayoutDimensions(75f, 30f);

        btnObj.GetComponent<Button>().onClick.AddListener(() =>
        {
            onCommit?.Invoke(input.text);
        });

        setValue = v => input.text = v;
    }
}
