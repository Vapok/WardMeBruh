using System;
using System.Collections.Generic;
using UnityEngine;
using WardMeBruh.Configuration;
using WardMeBruh.Models;
using WardMeBruh.Networking;
using WardMeBruh.Services;

namespace WardMeBruh.UI.Tabs;

public class TabUpkeep
{
    private GameObject _root;
    private PrivateArea _ward;

    private Action<bool> _setRepair;
    private Action<float> _setRepairThresholdSlider;

    private Action<bool> _setAutoFuel;

    private Action<bool> _setAutoClose;
    private Action<float> _setAutoCloseSlider;

    private readonly List<GameObject> _standardUpkeepCards = new();
    private GameObject _delegatedCard;
    private Action<string, Color> _setDelegatedBadge;

    private GameObject _cardRepairObj;
    private GameObject _cardFuelObj;
    private GameObject _cardDoorsObj;

    public GameObject Root => _root;

    public void Build(Transform parent)
    {
        _root = new GameObject("Tab_Upkeep", typeof(RectTransform));
        _root.transform.SetParent(parent, false);
        RectTransform rt = _root.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        WardUIFactory.CreateScrollArea(_root.transform, out Transform listContainer);

        Transform delegatedCard = WardUIFactory.CreateCard(listContainer, "$wmb_tab_upkeep");
        _delegatedCard = delegatedCard.parent.parent.gameObject;
        WardUIFactory.CreateBadgeRow(delegatedCard, "$wmb_territory_label", "...", WardColorPalette.GoldAmber, out _setDelegatedBadge);
        _delegatedCard.SetActive(false);

        Transform card1 = WardUIFactory.CreateCard(listContainer, "$wmb_upkeep_card_repair");
        _cardRepairObj = card1.parent.parent.gameObject;
        _standardUpkeepCards.Add(_cardRepairObj);
        WardUIFactory.CreateToggleRow(
            card1, 
            "$wmb_upkeep_repair", 
            false, 
            v => SetZdoBool(WardZdoKeys.RepairEnabled, v, "upkeep.repair"), 
            out _setRepair);

        WardUIFactory.CreateSliderRow(
            card1, 
            "$wmb_upkeep_repair_threshold", 
            20f, 
            100f, 
            100f, 
            true, 
            "%", 
            v => SetZdoFloat(WardZdoKeys.RepairThreshold, v / 100f, "upkeep.repair"), 
            out _setRepairThresholdSlider);

        WardUIFactory.CreateBadgeRow(card1, "$wmb_upkeep_repair_cost", "$wmb_upkeep_repair_cost_val".Localize(), WardColorPalette.ValheimBeige, out _);

        Transform card2 = WardUIFactory.CreateCard(listContainer, "$wmb_upkeep_card_fuel");
        _cardFuelObj = card2.parent.parent.gameObject;
        _standardUpkeepCards.Add(_cardFuelObj);
        WardUIFactory.CreateToggleRow(
            card2, 
            "$wmb_upkeep_fuel", 
            false, 
            v => SetZdoBool(WardZdoKeys.AutoFuelEnabled, v, "upkeep.autofuel"), 
            out _setAutoFuel);

        WardUIFactory.CreateBadgeRow(card2, "$wmb_upkeep_fuel_source", "$wmb_upkeep_fuel_source_val".Localize(), WardColorPalette.GoldAmber, out _);
        WardUIFactory.CreateBadgeRow(card2, "$wmb_upkeep_appliances", "$wmb_upkeep_appliances_val".Localize(), WardColorPalette.ValheimBeige, out _);

        Transform card3 = WardUIFactory.CreateCard(listContainer, "$wmb_upkeep_card_doors");
        _cardDoorsObj = card3.parent.parent.gameObject;
        _standardUpkeepCards.Add(_cardDoorsObj);
        WardUIFactory.CreateToggleRow(
            card3, 
            "$wmb_upkeep_auto_close_doors", 
            false, 
            v => SetZdoBool(WardZdoKeys.AutoCloseDoors, v, "upkeep.autodoors"), 
            out _setAutoClose);

        WardUIFactory.CreateSliderRow(
            card3, 
            "$wmb_upkeep_auto_close_delay", 
            5f, 
            30f, 
            5f, 
            true, 
            "s", 
            v => SetZdoFloat(WardZdoKeys.AutoCloseDelay, v, "upkeep.autodoors"), 
            out _setAutoCloseSlider);
    }

    public void Bind(PrivateArea ward)
    {
        _ward = ward;
        Refresh();
    }

    public void Refresh()
    {
        if (_ward == null || _ward.m_nview == null || !_ward.m_nview.IsValid())
            return;

        ZDO zdo = _ward.m_nview.GetZDO();
        long playerId = Player.m_localPlayer != null ? Player.m_localPlayer.GetPlayerID() : 0L;

        WardType wardType = (WardType)zdo.GetInt(WardZdoKeys.WardType, (int)WardType.Main);
        ZDOID parentId = zdo.GetZDOID(WardZdoKeys.ParentWard);
        string parentName = "Parent Ward";
        if (parentId != ZDOID.None && ZDOMan.instance != null)
        {
            ZDO parentZdo = ZDOMan.instance.GetZDO(parentId);
            if (parentZdo != null)
            {
                parentName = parentZdo.GetString(WardZdoKeys.CustomName, "Parent Ward");
            }
        }

        bool isDelegated = wardType == WardType.SubWard || wardType == WardType.Extension;
        if (_delegatedCard != null)
        {
            _delegatedCard.SetActive(isDelegated);
            if (isDelegated)
            {
                string msg = wardType == WardType.SubWard
                    ? string.Format("$wmb_subward_upkeep_delegated".Localize(), parentName)
                    : string.Format("$wmb_extension_managed_by".Localize(), parentName);
                _setDelegatedBadge?.Invoke(msg, WardColorPalette.GoldAmber);
            }
        }

        bool hasRepair = WardProgressionService.IsFeatureUnlocked(zdo, WardFeature.AreaRepair, playerId);
        bool hasFuel = WardProgressionService.IsFeatureUnlocked(zdo, WardFeature.AutoFuel, playerId);
        bool hasDoors = WardProgressionService.IsFeatureUnlocked(zdo, WardFeature.AutoCloseDoors, playerId);

        if (_cardRepairObj != null) _cardRepairObj.SetActive(!isDelegated && hasRepair);
        if (_cardFuelObj != null) _cardFuelObj.SetActive(!isDelegated && hasFuel);
        if (_cardDoorsObj != null) _cardDoorsObj.SetActive(!isDelegated && hasDoors);

        _setRepair?.Invoke(zdo.GetBool(WardZdoKeys.RepairEnabled, false));

        float repThresh = zdo.GetFloat(WardZdoKeys.RepairThreshold, (float)ConfigRegistry.AutoRepairThresholdDefault.Value);
        if (repThresh > 1.0f)
            repThresh /= 100f;
        _setRepairThresholdSlider?.Invoke(Mathf.Round(repThresh * 100f));

        _setAutoFuel?.Invoke(zdo.GetBool(WardZdoKeys.AutoFuelEnabled, false));

        _setAutoClose?.Invoke(zdo.GetBool(WardZdoKeys.AutoCloseDoors, false));

        float closeDelay = zdo.GetFloat(WardZdoKeys.AutoCloseDelay, (float)ConfigRegistry.AutoCloseDelayDefault.Value);
        _setAutoCloseSlider?.Invoke(closeDelay);

    }

    private static WardFeature? GetFeatureForKey(int key)
    {
        if (key == WardZdoKeys.RepairEnabled) return WardFeature.AreaRepair;
        if (key == WardZdoKeys.AutoFuelEnabled) return WardFeature.AutoFuel;
        if (key == WardZdoKeys.AutoCloseDoors) return WardFeature.AutoCloseDoors;
        return null;
    }

    private void SetZdoBool(int key, bool val, string permissionId)
    {
        if (_ward != null && _ward.m_nview != null && _ward.m_nview.IsValid())
        {
            long playerId = Player.m_localPlayer != null ? Player.m_localPlayer.GetPlayerID() : 0L;
            ZDO zdo = _ward.m_nview.GetZDO();
            if (WardRoleManager.CanPerform(zdo, playerId, permissionId))
            {
                WardFeature? feature = GetFeatureForKey(key);
                if (feature.HasValue && !WardProgressionService.IsFeatureUnlocked(zdo, feature.Value, playerId))
                {
                    if (Player.m_localPlayer != null)
                    {
                        Player.m_localPlayer.Message(MessageHud.MessageType.Center, "$wmb_feature_locked_by_rune".Localize());
                    }
                    Refresh();
                    return;
                }

                WardNetworkRouter.SendWardBool(zdo.m_uid, key, val);
                Refresh();
            }
            else
            {
                Refresh();
            }
        }
    }

    private void SetZdoFloat(int key, float val, string permissionId)
    {
        if (_ward != null && _ward.m_nview != null && _ward.m_nview.IsValid())
        {
            long playerId = Player.m_localPlayer != null ? Player.m_localPlayer.GetPlayerID() : 0L;
            ZDO zdo = _ward.m_nview.GetZDO();
            if (WardRoleManager.CanPerform(zdo, playerId, permissionId))
            {
                WardNetworkRouter.SendWardFloat(zdo.m_uid, key, val);
            }
            else
            {
                Refresh();
            }
        }
    }
}
