using UnityEngine;
using UnityEngine.EventSystems;
using WardMeBruh.Models;
using WardMeBruh.UI.Tabs;

namespace WardMeBruh.UI.Elements;

public class RowHoverTooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler
{
    public FeaturePermission Permission { get; set; }
    public TabManagement Tab { get; set; }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (ZInput.IsGamepadActive())
            return;

        if (Tab != null && Permission != null)
        {
            Tab.ShowHoverTooltip(Permission, GetComponent<RectTransform>());
        }
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        if (ZInput.IsGamepadActive())
            return;

        if (Tab != null && Permission != null)
        {
            Tab.UpdateHoverTooltipPosition(GetComponent<RectTransform>());
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (ZInput.IsGamepadActive())
            return;

        if (Tab != null)
        {
            Tab.HideHoverTooltip();
        }
    }

    private void OnDisable()
    {
        if (Tab != null)
        {
            Tab.HideHoverTooltip();
        }
    }
}
