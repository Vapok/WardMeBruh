using System;
using Jotunn.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WardMeBruh.Models;
using WardMeBruh.Services;

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
        vpRt.offsetMax = new Vector2(-12f, 0f);

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
        sbRt.sizeDelta = new Vector2(8f, 0f);
        sbRt.anchoredPosition = Vector2.zero;

        Image sbImg = scrollbarObj.GetComponent<Image>();
        sbImg.color = new Color(0.04f, 0.03f, 0.02f, 0.45f);
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
        handleImg.color = new Color(0.85f, 0.65f, 0.25f, 0.45f);
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
        scrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHide;

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

        GameObject headerTextObj = new GameObject("HeaderText", typeof(RectTransform), typeof(TextMeshProUGUI), typeof(WardTextElement));
        headerTextObj.transform.SetParent(headerObj.transform, false);
        RectTransform htRt = headerTextObj.GetComponent<RectTransform>();
        htRt.anchorMin = Vector2.zero;
        htRt.anchorMax = Vector2.one;
        htRt.offsetMin = new Vector2(12f, 0f);
        htRt.offsetMax = new Vector2(-12f, 0f);

        TextMeshProUGUI headerText = headerTextObj.GetComponent<TextMeshProUGUI>();
        headerText.text = title.Localize().ToUpperInvariant();
        headerText.font = UIFonts.GetPrimaryFont();
        headerText.fontSize = 13f;
        headerText.color = WardColorPalette.ValheimOrange;
        headerText.alignment = TextAlignmentOptions.MidlineLeft;
        headerText.overflowMode = TextOverflowModes.Ellipsis;

        WardTextElement htWte = headerTextObj.GetComponent<WardTextElement>();
        htWte.baseFontSize = 13;

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

        GameObject labelObj = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI), typeof(LayoutElement), typeof(WardTextElement));
        labelObj.transform.SetParent(rowObj.transform, false);
        labelObj.GetComponent<RectTransform>().sizeDelta = new Vector2(300f, rowHeight);
        labelObj.SetLayoutDimensions(300f, rowHeight, 200f, rowHeight);

        TextMeshProUGUI label = labelObj.GetComponent<TextMeshProUGUI>();
        label.text = labelText.Localize();
        label.font = UIFonts.GetPrimaryFont();
        label.fontSize = 13f;
        label.color = WardColorPalette.ValheimBeige;
        label.alignment = TextAlignmentOptions.MidlineLeft;
        label.overflowMode = TextOverflowModes.Ellipsis;
        label.textWrappingMode = TextWrappingModes.Normal;

        WardTextElement labelWte = labelObj.GetComponent<WardTextElement>();
        labelWte.baseFontSize = 13;

        GameObject controlsObj = new GameObject("Controls", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
        controlsObj.transform.SetParent(rowObj.transform, false);
        controlsObj.GetComponent<RectTransform>().sizeDelta = new Vector2(430f, rowHeight);
        LayoutElement ctrlLe = controlsObj.SetLayoutDimensions(430f, rowHeight, 200f, rowHeight);
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

    public static TextMeshProUGUI SetupButtonText(
        GameObject btnObj,
        string text,
        float fontSize = 13f,
        Color? color = null,
        TextAlignmentOptions alignment = TextAlignmentOptions.Center,
        bool autoResizeWidth = true)
    {
        if (btnObj == null)
            return null;

        UIFonts.GetPrimaryFont();

        Text legacyText = btnObj.GetComponentInChildren<Text>(true);
        GameObject textGo;
        if (legacyText != null)
        {
            textGo = legacyText.gameObject;
            UnityEngine.Object.DestroyImmediate(legacyText);
        }
        else
        {
            TextMeshProUGUI existingTmp = btnObj.GetComponentInChildren<TextMeshProUGUI>(true);
            if (existingTmp != null)
            {
                existingTmp.text = text;
                existingTmp.fontSize = fontSize;
                if (color.HasValue) existingTmp.color = color.Value;
                existingTmp.alignment = alignment;

                WardTextElement existingWte = existingTmp.GetComponent<WardTextElement>();
                if (existingWte == null)
                {
                    existingWte = existingTmp.gameObject.AddComponent<WardTextElement>();
                }
                existingWte.baseFontSize = Mathf.RoundToInt(fontSize);

                if (autoResizeWidth)
                {
                    AttachOrUpdateAutoResizer(btnObj, existingTmp);
                }

                return existingTmp;
            }
            textGo = new GameObject("Text", typeof(RectTransform));
            textGo.transform.SetParent(btnObj.transform, false);
            RectTransform rt = textGo.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        TextMeshProUGUI tmp = textGo.GetComponent<TextMeshProUGUI>();
        if (tmp == null)
        {
            tmp = textGo.AddComponent<TextMeshProUGUI>();
        }

        tmp.font = UIFonts.GetPrimaryFont();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = color ?? WardColorPalette.ValheimBeige;
        tmp.alignment = alignment;
        tmp.overflowMode = TextOverflowModes.Overflow;
        tmp.textWrappingMode = TextWrappingModes.NoWrap;

        WardTextElement wte = textGo.GetComponent<WardTextElement>();
        if (wte == null)
        {
            wte = textGo.AddComponent<WardTextElement>();
        }
        wte.baseFontSize = Mathf.RoundToInt(fontSize);

        if (autoResizeWidth)
        {
            AttachOrUpdateAutoResizer(btnObj, tmp);
        }

        return tmp;
    }

    private static void AttachOrUpdateAutoResizer(GameObject btnObj, TextMeshProUGUI tmp)
    {
        RectTransform btnRt = btnObj.GetComponent<RectTransform>();
        if (btnRt == null)
            return;

        bool isSquareIcon = Mathf.Abs(btnRt.sizeDelta.x - btnRt.sizeDelta.y) < 1f && btnRt.sizeDelta.x <= 36f;
        if (isSquareIcon)
            return;

        WardButtonAutoResizer resizer = btnObj.GetComponent<WardButtonAutoResizer>();
        if (resizer == null)
        {
            resizer = btnObj.AddComponent<WardButtonAutoResizer>();
            resizer.Initialize(btnRt, tmp, btnRt.sizeDelta.x > 0f ? btnRt.sizeDelta.x : 100f, 24f);
        }
        else
        {
            resizer.RefreshWidth();
        }
    }

    public static void CreateToggleRow(Transform parent, string labelText, bool initialValue, Action<bool> onToggle, out Action<bool> setValue)
    {
        CreateSettingRow(parent, labelText, out Transform controlContainer);

        GameObject btnObj = GUIManager.Instance.CreateButton(
            initialValue ? $"✔  {"$wmb_enabled".Localize()}" : $"✖  {"$wmb_disabled".Localize()}",
            controlContainer,
            Vector2.zero,
            Vector2.zero,
            Vector2.zero,
            130f,
            28f);

        btnObj.GetComponent<RectTransform>().sizeDelta = new Vector2(130f, 28f);
        btnObj.SetLayoutDimensions(130f, 28f);
        TextMeshProUGUI btnText = SetupButtonText(
            btnObj,
            initialValue ? $"✔  {"$wmb_enabled".Localize()}" : $"✖  {"$wmb_disabled".Localize()}",
            13f,
            initialValue ? WardColorPalette.RunicGreen : WardColorPalette.TextMuted);

        bool currentValue = initialValue;

        void UpdateVisual(bool state)
        {
            currentValue = state;
            btnText.text = state ? $"✔  {"$wmb_enabled".Localize()}" : $"✖  {"$wmb_disabled".Localize()}";
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
            initialToggle ? $"✔  {"$wmb_enabled".Localize()}" : $"✖  {"$wmb_disabled".Localize()}",
            controlContainer,
            Vector2.zero,
            Vector2.zero,
            Vector2.zero,
            120f,
            28f);
        toggleBtnObj.GetComponent<RectTransform>().sizeDelta = new Vector2(120f, 28f);
        toggleBtnObj.SetLayoutDimensions(120f, 28f);

        TextMeshProUGUI toggleTxt = SetupButtonText(
            toggleBtnObj,
            initialToggle ? $"✔  {"$wmb_enabled".Localize()}" : $"✖  {"$wmb_disabled".Localize()}",
            12f,
            initialToggle ? WardColorPalette.RunicGreen : WardColorPalette.TextMuted);

        Button toggleBtn = toggleBtnObj.GetComponent<Button>();
        bool currentToggle = initialToggle;

        GameObject roleBtnObj = GUIManager.Instance.CreateButton(
            $"[ {"$wmb_role".Localize()}: {WardRoleManager.GetRoleName(initialRole).Localize()} ]",
            controlContainer,
            Vector2.zero,
            Vector2.zero,
            Vector2.zero,
            135f,
            28f);
        roleBtnObj.GetComponent<RectTransform>().sizeDelta = new Vector2(135f, 28f);
        roleBtnObj.SetLayoutDimensions(135f, 28f);

        TextMeshProUGUI roleTxt = SetupButtonText(
            roleBtnObj,
            $"[ {"$wmb_role".Localize()}: {WardRoleManager.GetRoleName(initialRole).Localize()} ]",
            12f,
            GetRoleColor(initialRole));

        Button roleBtn = roleBtnObj.GetComponent<Button>();
        WardRole currentRole = initialRole;

        toggleBtn.onClick.AddListener(() =>
        {
            currentToggle = !currentToggle;
            toggleTxt.text = currentToggle ? $"✔  {"$wmb_enabled".Localize()}" : $"✖  {"$wmb_disabled".Localize()}";
            toggleTxt.color = currentToggle ? WardColorPalette.RunicGreen : WardColorPalette.TextMuted;
            onToggle?.Invoke(currentToggle);
        });

        roleBtn.onClick.AddListener(() =>
        {
            currentRole = (WardRole)(((int)currentRole + 1) % 4);
            roleTxt.text = $"[ {"$wmb_role".Localize()}: {WardRoleManager.GetRoleName(currentRole).Localize()} ]";
            roleTxt.color = GetRoleColor(currentRole);
            onRoleChanged?.Invoke(currentRole);
        });

        updateVisual = (state, role, interactable) =>
        {
            currentToggle = state;
            toggleTxt.text = state ? $"✔  {"$wmb_enabled".Localize()}" : $"✖  {"$wmb_disabled".Localize()}";
            toggleTxt.color = state ? WardColorPalette.RunicGreen : WardColorPalette.TextMuted;

            currentRole = role;
            roleTxt.text = $"[ {"$wmb_role".Localize()}: {WardRoleManager.GetRoleName(role).Localize()} ]";
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
        out Action<float> setValue,
        float step = 5f,
        bool showTicks = true)
    {
        CreateSliderRow(parent, labelText, min, max, initialValue, wholeNumbers, unit, onValueChanged, out setValue, out _, step, showTicks);
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
        out Action<float> setValue,
        out Action<bool> setInteractable,
        float step = 5f,
        bool showTicks = true)
    {
        CreateSliderRow(parent, labelText, min, max, initialValue, wholeNumbers, unit, onValueChanged, out setValue, out setInteractable, out _, step, showTicks);
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
        out Action<float> setValue,
        out Action<bool> setInteractable,
        out Action<float, float, float> setRange,
        float step = 5f,
        bool showTicks = true)
    {
        if ((max - min) <= 50f)
        {
            step = 1f;
        }

        float rowHeight = showTicks ? 48f : 34f;
        CreateSettingRow(parent, labelText, out Transform controlContainer, rowHeight);

        GameObject sliderWrapper = new GameObject("SliderWrapper", typeof(RectTransform), typeof(LayoutElement));
        sliderWrapper.transform.SetParent(controlContainer, false);
        sliderWrapper.GetComponent<RectTransform>().sizeDelta = new Vector2(250f, showTicks ? 38f : 18f);
        sliderWrapper.SetLayoutDimensions(250f, showTicks ? 38f : 18f);

        GameObject ticksObj = null;
        CanvasGroup ticksCg = null;
        if (showTicks)
        {
            ticksObj = new GameObject("Ticks", typeof(RectTransform), typeof(CanvasGroup));
            ticksObj.transform.SetParent(sliderWrapper.transform, false);
            RectTransform ticksRt = ticksObj.GetComponent<RectTransform>();
            ticksRt.anchorMin = Vector2.zero;
            ticksRt.anchorMax = Vector2.one;
            ticksRt.offsetMin = new Vector2(10f, 0f);
            ticksRt.offsetMax = new Vector2(-10f, 0f);
            ticksCg = ticksObj.GetComponent<CanvasGroup>();
        }

        void RebuildTicks(float tMin, float tMax, float tStep)
        {
            if (ticksObj == null)
                return;

            for (int i = ticksObj.transform.childCount - 1; i >= 0; i--)
            {
                UnityEngine.Object.Destroy(ticksObj.transform.GetChild(i).gameObject);
            }

            float range = tMax - tMin;
            if (range <= 0f)
                return;

            bool isSubFifty = range <= 50f;
            float effStep = isSubFifty ? 1f : (tStep > 0f ? tStep : 5f);
            int count = Mathf.RoundToInt(range / effStep);
            if (count <= 0)
                return;

            float majorInterval = isSubFifty ? 5f : ((range <= 35f) ? 10f : 25f);

            for (int i = 0; i <= count; i++)
            {
                float val = (i == count) ? tMax : (tMin + i * effStep);
                float t = (float)i / count;

                bool isMin = (i == 0) || Mathf.Approximately(val, tMin);
                bool isMax = (i == count) || Mathf.Approximately(val, tMax);
                int roundVal = Mathf.RoundToInt(val);
                int roundMajor = Mathf.RoundToInt(majorInterval);
                bool isInterval = (roundMajor > 0) && (roundVal % roundMajor == 0);

                float pixelDistFromMin = t * 230f;
                float pixelDistFromMax = (1f - t) * 230f;
                bool collidesWithMin = !isMin && pixelDistFromMin < 18f;
                bool collidesWithMax = !isMax && pixelDistFromMax < 18f;

                bool isMajor = (isMin || isMax || isInterval) && !collidesWithMin && !collidesWithMax;
                float tickH = isMajor ? 7f : 3.5f;
                float tickW = isMajor ? 2f : 1f;
                Color tickColor = isMajor ? WardColorPalette.GoldAmber : new Color(0.60f, 0.52f, 0.40f, 0.55f);

                GameObject tickMark = new GameObject($"Tick_{val}", typeof(RectTransform), typeof(Image));
                tickMark.transform.SetParent(ticksObj.transform, false);
                RectTransform tmRt = tickMark.GetComponent<RectTransform>();
                tmRt.anchorMin = new Vector2(t, 0.5f);
                tmRt.anchorMax = new Vector2(t, 0.5f);
                tmRt.pivot = new Vector2(0.5f, 1f);
                tmRt.anchoredPosition = new Vector2(0f, 3.5f);
                tmRt.sizeDelta = new Vector2(tickW, tickH);

                Image tickImg = tickMark.GetComponent<Image>();
                tickImg.color = tickColor;

                if (isMajor)
                {
                    GameObject labelObj = new GameObject($"TickLabel_{val}", typeof(RectTransform), typeof(TextMeshProUGUI), typeof(WardTextElement));
                    labelObj.transform.SetParent(ticksObj.transform, false);
                    RectTransform lblRt = labelObj.GetComponent<RectTransform>();
                    lblRt.anchorMin = new Vector2(t, 0.5f);
                    lblRt.anchorMax = new Vector2(t, 0.5f);
                    lblRt.pivot = new Vector2(0.5f, 1f);
                    lblRt.anchoredPosition = new Vector2(0f, -5f);
                    lblRt.sizeDelta = new Vector2(28f, 12f);

                    TextMeshProUGUI lblText = labelObj.GetComponent<TextMeshProUGUI>();
                    lblText.font = UIFonts.GetPrimaryFont();
                    lblText.fontSize = isSubFifty ? 8f : 9f;
                    lblText.color = WardColorPalette.ValheimBeige;
                    lblText.alignment = TextAlignmentOptions.Top;
                    lblText.overflowMode = TextOverflowModes.Overflow;
                    lblText.textWrappingMode = TextWrappingModes.NoWrap;
                    lblText.text = $"{Mathf.RoundToInt(val)}";

                    WardTextElement lblWte = labelObj.GetComponent<WardTextElement>();
                    lblWte.baseFontSize = isSubFifty ? 8 : 9;
                }
            }
        }

        RebuildTicks(min, max, step);

        GameObject sliderObj = WardUIExtensions.CreateValheimSlider(sliderWrapper.transform, min, max, initialValue, wholeNumbers, 250f, 18f);
        RectTransform sRt = sliderObj.GetComponent<RectTransform>();
        sRt.anchorMin = new Vector2(0f, 0.5f);
        sRt.anchorMax = new Vector2(1f, 0.5f);
        sRt.pivot = new Vector2(0.5f, 0.5f);
        sRt.anchoredPosition = new Vector2(0f, showTicks ? 8f : 0f);
        sRt.sizeDelta = new Vector2(0f, 18f);
        Slider slider = sliderObj.GetComponent<Slider>();

        GameObject readoutObj = new GameObject("Readout", typeof(RectTransform), typeof(TextMeshProUGUI), typeof(LayoutElement), typeof(WardTextElement));
        readoutObj.transform.SetParent(controlContainer, false);
        readoutObj.GetComponent<RectTransform>().sizeDelta = new Vector2(65f, rowHeight);
        readoutObj.SetLayoutDimensions(65f, rowHeight);

        TextMeshProUGUI readoutText = readoutObj.GetComponent<TextMeshProUGUI>();
        readoutText.font = UIFonts.GetPrimaryFont();
        readoutText.fontSize = 14f;
        readoutText.color = WardColorPalette.GoldAmber;
        readoutText.alignment = TextAlignmentOptions.MidlineRight;
        readoutText.overflowMode = TextOverflowModes.Overflow;
        readoutText.textWrappingMode = TextWrappingModes.NoWrap;

        WardTextElement roWte = readoutObj.GetComponent<WardTextElement>();
        roWte.baseFontSize = 14;

        string FormatValue(float v)
        {
            return wholeNumbers ? $"{Mathf.RoundToInt(v)}{unit}" : $"{v:F1}{unit}";
        }

        float SnapValue(float v)
        {
            float activeStep = ((max - min) <= 50f) ? 1f : step;
            if (activeStep > 0f)
            {
                float snapped = Mathf.Round((v - min) / activeStep) * activeStep + min;
                return Mathf.Clamp(snapped, min, max);
            }
            return Mathf.Clamp(v, min, max);
        }

        float currentVal = SnapValue(initialValue);
        readoutText.text = FormatValue(currentVal);

        bool suppressCallback = false;

        slider.onValueChanged.AddListener(v =>
        {
            if (suppressCallback)
                return;

            float val = SnapValue(v);
            if (Mathf.Abs(slider.value - val) > 0.001f)
            {
                slider.SetValueWithoutNotify(val);
            }
            readoutText.text = FormatValue(val);
            onValueChanged?.Invoke(val);
        });

        setValue = v =>
        {
            suppressCallback = true;
            try
            {
                float val = SnapValue(v);
                slider.SetValueWithoutNotify(val);
                readoutText.text = FormatValue(val);
            }
            finally
            {
                suppressCallback = false;
            }
        };

        setInteractable = interactable =>
        {
            slider.interactable = interactable;
            if (ticksCg != null)
            {
                ticksCg.alpha = interactable ? 1f : 0.4f;
            }
        };

        setRange = (newMin, newMax, newStep) =>
        {
            suppressCallback = true;
            try
            {
                min = newMin;
                max = newMax;
                step = ((newMax - newMin) <= 50f) ? 1f : newStep;
                slider.minValue = newMin;
                slider.maxValue = newMax;
                RebuildTicks(newMin, newMax, step);
                float snapped = SnapValue(slider.value);
                slider.SetValueWithoutNotify(snapped);
                readoutText.text = FormatValue(snapped);
            }
            finally
            {
                suppressCallback = false;
            }
        };
    }

    public static void CreatePowerRow(
        Transform parent, 
        string labelText, 
        string initialBadge, 
        Color initialBadgeColor, 
        string initialButtonText, 
        Action onButtonClicked, 
        out Action<string, Color, string, bool> updateVisual)
    {
        CreateSettingRow(parent, labelText, out Transform controlContainer);

        GameObject badgeObj = new GameObject("PowerBadge", typeof(RectTransform), typeof(TextMeshProUGUI), typeof(LayoutElement), typeof(WardTextElement));
        badgeObj.transform.SetParent(controlContainer, false);
        badgeObj.GetComponent<RectTransform>().sizeDelta = new Vector2(130f, 28f);
        badgeObj.SetLayoutDimensions(130f, 28f);

        TextMeshProUGUI badgeText = badgeObj.GetComponent<TextMeshProUGUI>();
        badgeText.text = initialBadge;
        badgeText.font = UIFonts.GetPrimaryFont();
        badgeText.fontSize = 13f;
        badgeText.color = initialBadgeColor;
        badgeText.alignment = TextAlignmentOptions.MidlineRight;
        badgeText.overflowMode = TextOverflowModes.Overflow;
        badgeText.textWrappingMode = TextWrappingModes.NoWrap;

        WardTextElement badgeWte = badgeObj.GetComponent<WardTextElement>();
        badgeWte.baseFontSize = 13;

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

        TextMeshProUGUI btnText = SetupButtonText(btnObj, initialButtonText, 13f);

        btnObj.GetComponent<Button>().onClick.AddListener(() =>
        {
            onButtonClicked?.Invoke();
        });

        updateVisual = (text, color, buttonLabel, canPower) =>
        {
            badgeText.text = text;
            badgeText.color = color;
            btnText.text = buttonLabel;
            btnText.color = (buttonLabel == "$wmb_deactivate".Localize() || buttonLabel == "DEACTIVATE") ? WardColorPalette.DangerRed : WardColorPalette.RunicGreen;
            btnObj.SetActive(canPower);
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

        TextMeshProUGUI btnText = SetupButtonText(btnObj, initialRoleText, 12f, initialRoleColor);
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

    public static GameObject CreateBadgeRow(Transform parent, string labelText, string initialBadge, Color badgeColor, out Action<string, Color> setBadge)
    {
        GameObject rowObj = CreateSettingRow(parent, labelText, out Transform controlContainer);

        GameObject badgeObj = new GameObject("Badge", typeof(RectTransform), typeof(TextMeshProUGUI), typeof(LayoutElement), typeof(WardTextElement));
        badgeObj.transform.SetParent(controlContainer, false);
        badgeObj.GetComponent<RectTransform>().sizeDelta = new Vector2(380f, 28f);
        badgeObj.SetLayoutDimensions(380f, 28f);

        TextMeshProUGUI badgeText = badgeObj.GetComponent<TextMeshProUGUI>();
        badgeText.text = initialBadge;
        badgeText.font = UIFonts.GetPrimaryFont();
        badgeText.fontSize = 13f;
        badgeText.color = badgeColor;
        badgeText.alignment = TextAlignmentOptions.MidlineRight;
        badgeText.overflowMode = TextOverflowModes.Overflow;
        badgeText.textWrappingMode = TextWrappingModes.NoWrap;

        WardTextElement badgeWte = badgeObj.GetComponent<WardTextElement>();
        badgeWte.baseFontSize = 13;

        setBadge = (text, color) =>
        {
            badgeText.text = text;
            badgeText.color = color;
        };

        return rowObj;
    }

    public static void CreateInputRow(
        Transform parent, 
        string labelText, 
        string initialValue, 
        string buttonText, 
        Action<string> onCommit, 
        out Action<string, bool> updateVisual)
    {
        CreateSettingRow(parent, labelText, out Transform controlContainer);

        GameObject badgeObj = new GameObject("StaticBadge", typeof(RectTransform), typeof(TextMeshProUGUI), typeof(LayoutElement), typeof(WardTextElement));
        badgeObj.transform.SetParent(controlContainer, false);
        badgeObj.GetComponent<RectTransform>().sizeDelta = new Vector2(330f, 28f);
        badgeObj.SetLayoutDimensions(330f, 28f);

        TextMeshProUGUI badgeText = badgeObj.GetComponent<TextMeshProUGUI>();
        badgeText.text = initialValue;
        badgeText.font = UIFonts.GetPrimaryFont();
        badgeText.fontSize = 13f;
        badgeText.color = WardColorPalette.ValheimBeige;
        badgeText.alignment = TextAlignmentOptions.MidlineRight;
        badgeText.overflowMode = TextOverflowModes.Overflow;
        badgeText.textWrappingMode = TextWrappingModes.NoWrap;

        WardTextElement badgeWte = badgeObj.GetComponent<WardTextElement>();
        badgeWte.baseFontSize = 13;

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

        TextMeshProUGUI btnText = SetupButtonText(btnObj, buttonText, 13f);

        btnObj.GetComponent<Button>().onClick.AddListener(() =>
        {
            onCommit?.Invoke(input.text);
        });

        updateVisual = (val, canEdit) =>
        {
            badgeText.text = val;
            badgeObj.SetActive(!canEdit);
            inputObj.SetActive(canEdit);
            btnObj.SetActive(canEdit);
            if (canEdit && input.text != val)
            {
                input.text = val;
            }
        };
    }
}
