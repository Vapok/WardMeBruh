using System;
using System.Collections.Generic;
using Jotunn.Managers;
using UnityEngine;
using UnityEngine.UI;
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

    public bool IsOpen => _panel != null && _panel.activeSelf;

    public void Build(Transform parent, Action<KnownWard> onWardSelected)
    {
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

        GameObject titleObj = new GameObject("TitleText", typeof(RectTransform), typeof(Text));
        titleObj.transform.SetParent(_panel.transform, false);
        RectTransform titleRt = titleObj.GetComponent<RectTransform>();
        titleRt.anchorMin = new Vector2(0f, 1f);
        titleRt.anchorMax = new Vector2(0f, 1f);
        titleRt.pivot = new Vector2(0f, 1f);
        titleRt.anchoredPosition = new Vector2(30f, -20f);
        titleRt.sizeDelta = new Vector2(450f, 32f);

        Text titleText = titleObj.GetComponent<Text>();
        titleText.text = "$wmb_modal_territory_registry".Localize();
        titleText.font = GUIManager.Instance.AveriaSerifBold;
        titleText.fontSize = 18;
        titleText.color = WardColorPalette.ValheimOrange;
        titleText.alignment = TextAnchor.MiddleLeft;

        GameObject closeBtn = GUIManager.Instance.CreateButton(
            "X", 
            _panel.transform, 
            new Vector2(1f, 1f), 
            new Vector2(1f, 1f), 
            new Vector2(-30f, -32f), 
            34f, 
            32f);
        closeBtn.GetComponent<Button>().onClick.AddListener(Close);

        GameObject scrollAreaObj = WardUIFactory.CreateScrollArea(_panel.transform, out _listContainer);
        RectTransform scrollRt = scrollAreaObj.GetComponent<RectTransform>();
        scrollRt.offsetMin = new Vector2(25f, 25f);
        scrollRt.offsetMax = new Vector2(-25f, -60f);

        _panel.SetActive(false);
    }

    public void Open(PrivateArea activeWard = null)
    {
        if (_panel == null)
            return;

        _activeWard = activeWard;
        _panel.SetActive(true);
        Refresh();
    }

    public void Close()
    {
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
        ZDOID currentWardId = (_activeWard != null && _activeWard.m_nview != null && _activeWard.m_nview.IsValid()) 
            ? _activeWard.m_nview.GetZDO().m_uid 
            : ZDOID.None;

        for (int i = 0; i < wards.Count; i++)
        {
            KnownWard ward = wards[i];
            bool isCurrent = ward.Id == currentWardId;

            float dist = Utils.DistanceXZ(playerPos, ward.Position);
            string distStr = dist < 1000f ? $"{Mathf.RoundToInt(dist)}{"$wmb_meters_away".Localize()}" : $"{dist / 1000f:F1}{"$wmb_km_away".Localize()}";
            if (isCurrent)
            {
                distStr = "$wmb_modal_current_location".Localize();
            }

            string labelText = $"{ward.CustomName}  <color=#fcd34d>[r: {Mathf.RoundToInt(ward.Radius)}{"$wmb_meters".Localize()}]</color>\n<size={subSize}><color=#9ca3af>{"$wmb_owner_label".Localize()} {ward.CreatorName}  •  {distStr}</color></size>";

            WardUIFactory.CreateSettingRow(card, labelText, out Transform ctrlContainer, rowHeight);

            GameObject statusBadgeObj = new GameObject("StatusBadge", typeof(RectTransform), typeof(Text), typeof(LayoutElement));
            statusBadgeObj.transform.SetParent(ctrlContainer, false);
            statusBadgeObj.GetComponent<RectTransform>().sizeDelta = new Vector2(100f, 28f);
            statusBadgeObj.SetLayoutDimensions(100f, 28f);

            Text statusText = statusBadgeObj.GetComponent<Text>();
            statusText.text = ward.IsEnabled ? $"● {"$wmb_active".Localize()}" : $"○ {"$wmb_inactive".Localize()}";
            statusText.font = GUIManager.Instance.AveriaSerifBold;
            statusText.fontSize = 12;
            statusText.color = ward.IsEnabled ? WardColorPalette.RunicGreen : WardColorPalette.TextMuted;
            statusText.alignment = TextAnchor.MiddleRight;

            if (isCurrent)
            {
                GameObject currentBadgeObj = new GameObject("CurrentBadge", typeof(RectTransform), typeof(Text), typeof(LayoutElement));
                currentBadgeObj.transform.SetParent(ctrlContainer, false);
                currentBadgeObj.GetComponent<RectTransform>().sizeDelta = new Vector2(100f, 28f);
                currentBadgeObj.SetLayoutDimensions(100f, 28f);

                Text currentBadgeText = currentBadgeObj.GetComponent<Text>();
                currentBadgeText.text = "$wmb_modal_current_badge".Localize();
                currentBadgeText.font = GUIManager.Instance.AveriaSerifBold;
                currentBadgeText.fontSize = 16;
                currentBadgeText.color = WardColorPalette.GoldAmber;
                currentBadgeText.alignment = TextAnchor.MiddleCenter;
                currentBadgeText.horizontalOverflow = HorizontalWrapMode.Overflow;
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

                Text btnTxt = manageBtn.GetComponentInChildren<Text>();
                if (btnTxt != null)
                {
                    btnTxt.font = GUIManager.Instance.AveriaSerifBold;
                    btnTxt.fontSize = 12;
                    btnTxt.color = WardColorPalette.ValheimBeige;
                }

                manageBtn.GetComponent<Button>().onClick.AddListener(() =>
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
    }
}
