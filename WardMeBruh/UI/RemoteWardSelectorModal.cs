using System;
using System.Collections.Generic;
using Jotunn.Managers;
using TMPro;
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
    private PrivateArea _physicalWard;

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

        GameObject closeBtn = GUIManager.Instance.CreateButton(
            "X", 
            _panel.transform, 
            new Vector2(1f, 1f), 
            new Vector2(1f, 1f), 
            new Vector2(-30f, -32f), 
            34f, 
            32f);
        closeBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(34f, 32f);
        WardUIFactory.SetupButtonText(closeBtn, "X", 14f, WardColorPalette.DangerRed);
        closeBtn.GetComponent<Button>().onClick.AddListener(Close);

        GameObject scrollAreaObj = WardUIFactory.CreateScrollArea(_panel.transform, out _listContainer);
        RectTransform scrollRt = scrollAreaObj.GetComponent<RectTransform>();
        scrollRt.offsetMin = new Vector2(25f, 25f);
        scrollRt.offsetMax = new Vector2(-25f, -60f);

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

        for (int i = 0; i < wards.Count; i++)
        {
            KnownWard ward = wards[i];
            bool isManaging = ward.Id == activeWardId;
            bool isPhysicalLocation = ward.Id == physicalWardId;

            float dist = Utils.DistanceXZ(playerPos, ward.Position);
            string distStr = dist < 1000f ? $"{Mathf.RoundToInt(dist)}{"$wmb_meters_away".Localize()}" : $"{dist / 1000f:F1}{"$wmb_km_away".Localize()}";
            if (isPhysicalLocation)
            {
                distStr = "$wmb_modal_current_location".Localize();
            }

            string typeBadge = ward.Type == WardType.SubWard ? "<color=#93c5fd>[Sub-Ward]</color> " : string.Empty;
            string labelText = $"{typeBadge}{ward.CustomName}  <color=#fcd34d>[r: {Mathf.RoundToInt(ward.Radius)}{"$wmb_meters".Localize()}]</color>\n<size={subSize}><color=#9ca3af>{"$wmb_owner_label".Localize()} {ward.CreatorName}  •  {distStr}</color></size>";

            WardUIFactory.CreateSettingRow(card, labelText, out Transform ctrlContainer, rowHeight);

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

            if (isManaging)
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
