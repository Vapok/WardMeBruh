using UnityEngine;
using UnityEngine.UI;

namespace WardMeBruh.UI;

public static class WardUIExtensions
{
    public static RectTransform SetSize(this RectTransform rect, float width, float height)
    {
        rect.sizeDelta = new Vector2(width, height);
        return rect;
    }

    public static RectTransform SetAnchoredPos(this RectTransform rect, float x, float y)
    {
        rect.anchoredPosition = new Vector2(x, y);
        return rect;
    }

    public static RectTransform SetAnchors(this RectTransform rect, Vector2 min, Vector2 max)
    {
        rect.anchorMin = min;
        rect.anchorMax = max;
        return rect;
    }

    public static RectTransform SetPivot(this RectTransform rect, Vector2 pivot)
    {
        rect.pivot = pivot;
        return rect;
    }

    public static LayoutElement SetLayoutDimensions(this GameObject obj, float preferredWidth, float preferredHeight, float minWidth = -1f, float minHeight = -1f)
    {
        LayoutElement layoutElement = obj.GetComponent<LayoutElement>();
        if (layoutElement == null)
        {
            layoutElement = obj.AddComponent<LayoutElement>();
        }

        layoutElement.preferredWidth = preferredWidth;
        layoutElement.preferredHeight = preferredHeight;
        if (minWidth >= 0f)
            layoutElement.minWidth = minWidth;
        if (minHeight >= 0f)
            layoutElement.minHeight = minHeight;
        layoutElement.flexibleWidth = 0f;
        layoutElement.flexibleHeight = 0f;

        return layoutElement;
    }

    public static GameObject CreateValheimSlider(Transform parent, float min, float max, float current, bool wholeNumbers, float width, float height)
    {
        GameObject sliderObj = DefaultControls.CreateSlider(Jotunn.Managers.GUIManager.Instance.ValheimControlResources);
        sliderObj.transform.SetParent(parent, false);
        RectTransform rt = sliderObj.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(width, height);
        Slider slider = sliderObj.GetComponent<Slider>();
        slider.minValue = min;
        slider.maxValue = max;
        slider.wholeNumbers = wholeNumbers;
        slider.value = current;
        return sliderObj;
    }

    public static GameObject CreateValheimDropdown(Transform parent, float width, float height)
    {
        GameObject dropdownObj = DefaultControls.CreateDropdown(Jotunn.Managers.GUIManager.Instance.ValheimControlResources);
        dropdownObj.transform.SetParent(parent, false);
        RectTransform rt = dropdownObj.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(width, height);

        Dropdown dropdown = dropdownObj.GetComponent<Dropdown>();
        ColorBlock ddColors = dropdown.colors;
        ddColors.normalColor = WardColorPalette.ButtonDark;
        ddColors.highlightedColor = WardColorPalette.RowHover;
        ddColors.pressedColor = WardColorPalette.RowOdd;
        ddColors.disabledColor = new Color(0.12f, 0.10f, 0.08f, 0.85f);
        dropdown.colors = ddColors;

        Image bg = dropdownObj.GetComponent<Image>();
        if (bg != null)
        {
            bg.color = WardColorPalette.ButtonDark;
        }

        Transform label = dropdownObj.transform.Find("Label");
        if (label != null)
        {
            Text labelText = label.GetComponent<Text>();
            if (labelText != null)
            {
                labelText.font = Jotunn.Managers.GUIManager.Instance.AveriaSerifBold;
                labelText.fontSize = 13;
                labelText.color = WardColorPalette.ValheimBeige;
                labelText.alignment = TextAnchor.MiddleLeft;
            }
        }

        Transform arrow = dropdownObj.transform.Find("Arrow");
        if (arrow != null)
        {
            Image arrowImg = arrow.GetComponent<Image>();
            if (arrowImg != null)
            {
                arrowImg.color = WardColorPalette.GoldAmber;
            }
        }

        Transform template = dropdownObj.transform.Find("Template");
        if (template != null)
        {
            Image templateBg = template.GetComponent<Image>();
            if (templateBg != null)
            {
                templateBg.color = WardColorPalette.WoodpanelDark;
            }

            Transform viewport = template.Find("Viewport");
            if (viewport != null)
            {
                Image vpImg = viewport.GetComponent<Image>();
                if (vpImg != null)
                {
                    vpImg.color = WardColorPalette.WoodpanelDark;
                }
            }

            Transform item = template.Find("Viewport/Content/Item");
            if (item != null)
            {
                Toggle toggle = item.GetComponent<Toggle>();
                if (toggle != null)
                {
                    ColorBlock tcb = toggle.colors;
                    tcb.normalColor = WardColorPalette.RowEven;
                    tcb.highlightedColor = WardColorPalette.RowHover;
                    tcb.pressedColor = WardColorPalette.RowOdd;
                    tcb.selectedColor = WardColorPalette.RowHover;
                    toggle.colors = tcb;
                }

                Transform itemBg = item.Find("Item Background");
                if (itemBg != null)
                {
                    Image ibImg = itemBg.GetComponent<Image>();
                    if (ibImg != null)
                    {
                        ibImg.color = WardColorPalette.RowEven;
                    }
                }

                Transform itemCheck = item.Find("Item Checkmark");
                if (itemCheck != null)
                {
                    Image icImg = itemCheck.GetComponent<Image>();
                    if (icImg != null)
                    {
                        icImg.color = WardColorPalette.GoldAmber;
                    }
                }

                Transform itemLabel = item.Find("Item Label");
                if (itemLabel != null)
                {
                    Text ilText = itemLabel.GetComponent<Text>();
                    if (ilText != null)
                    {
                        ilText.font = Jotunn.Managers.GUIManager.Instance.AveriaSerifBold;
                        ilText.fontSize = 13;
                        ilText.color = WardColorPalette.ValheimBeige;
                        ilText.alignment = TextAnchor.MiddleLeft;
                    }
                }
            }

            Transform scrollbar = template.Find("Scrollbar");
            if (scrollbar != null)
            {
                Image sbImg = scrollbar.GetComponent<Image>();
                if (sbImg != null)
                {
                    sbImg.color = new Color(0.04f, 0.03f, 0.02f, 0.95f);
                }

                Transform handle = scrollbar.Find("Sliding Area/Handle");
                if (handle != null)
                {
                    Image handleImg = handle.GetComponent<Image>();
                    if (handleImg != null)
                    {
                        handleImg.color = WardColorPalette.GoldAmber;
                    }
                }
            }
        }

        return dropdownObj;
    }
}

public static class WardInputHelper
{
    private static readonly System.Reflection.MethodInfo ResetInputBlockMethod = HarmonyLib.AccessTools.Method(typeof(Jotunn.Managers.GUIManager), "ResetInputBlock");

    public static void ForceUnblockInput()
    {
        Jotunn.Managers.GUIManager.BlockInput(false);
        try
        {
            ResetInputBlockMethod?.Invoke(null, null);
        }
        catch (System.Exception ex)
        {
            Jotunn.Logger.LogWarning($"Failed to reset input block: {ex.Message}");
        }
    }
}
