using System;
using UnityEngine;
using WardMeBruh.Configuration;
using WardMeBruh.Models;
using WardMeBruh.Networking;

namespace WardMeBruh.UI.Tabs;

public class TabDefenses
{
    private GameObject _root;
    private PrivateArea _ward;

    private Action<float> _setPushbackSlider;
    private Action<float> _setShockCreatureSlider;
    private Action<float> _setShockPlayerSlider;

    private Action<bool, WardRole, bool> _setGhostBase;
    private Action<bool, WardRole, bool> _setRaidSuppress;
    private Action<bool, WardRole, bool> _setSpawnSuppress;
    private Action<bool, WardRole, bool> _setMaxComfort;
    private Action<bool, WardRole, bool> _setPortalAccess;
    private Action<bool, WardRole, bool> _setRegenBoost;
    private Action<bool, WardRole, bool> _setDeathSkill;
    private Action<bool, WardRole, bool> _setFoodFreeze;

    private Action<bool> _setShowDomeToggle;
    private Action<bool> _setShowRingToggle;

    public GameObject Root => _root;

    public void Build(Transform parent)
    {
        _root = new GameObject("Tab_Defenses", typeof(RectTransform));
        _root.transform.SetParent(parent, false);
        RectTransform rt = _root.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        WardUIFactory.CreateScrollArea(_root.transform, out Transform listContainer);

        Transform card1 = WardUIFactory.CreateCard(listContainer, "Intruder Repulsion & Electrical Shock");
        WardUIFactory.CreateSliderRow(
            card1, 
            "Pushback Impulse Force", 
            0f, 
            50f, 
            0f, 
            true, 
            " N", 
            v => SetZdoFloat(WardZdoKeys.PushbackForce, v), 
            out _setPushbackSlider);

        WardUIFactory.CreateSliderRow(
            card1, 
            "Creature Shock Damage", 
            0f, 
            100f, 
            0f, 
            true, 
            " dmg", 
            v => SetZdoFloat(WardZdoKeys.ShockCreatureDmg, v), 
            out _setShockCreatureSlider);

        WardUIFactory.CreateSliderRow(
            card1, 
            "Intruder Player Shock Damage", 
            0f, 
            100f, 
            0f, 
            true, 
            " dmg", 
            v => SetZdoFloat(WardZdoKeys.ShockPlayerDmg, v), 
            out _setShockPlayerSlider);

        Transform card2 = WardUIFactory.CreateCard(listContainer, "Base Camouflage & Territory Sanctuary");
        WardUIFactory.CreateFeatureToggleRow(
            card2, 
            "Ghost Base (Monster Camouflage)", 
            false, 
            WardRole.Guest,
            v => SetZdoBool(WardZdoKeys.GhostBase, v), 
            r => SetFeatureRole(WardZdoKeys.GhostBaseMinRole, r),
            out _setGhostBase);

        WardUIFactory.CreateFeatureToggleRow(
            card2, 
            "Raid Event Suppression", 
            false, 
            WardRole.Guest,
            v => SetZdoBool(WardZdoKeys.RaidSuppress, v), 
            r => SetFeatureRole(WardZdoKeys.RaidSuppressMinRole, r),
            out _setRaidSuppress);

        WardUIFactory.CreateFeatureToggleRow(
            card2, 
            "Monster Spawn Suppression", 
            false, 
            WardRole.Guest,
            v => SetZdoBool(WardZdoKeys.SpawnSuppress, v), 
            r => SetFeatureRole(WardZdoKeys.SpawnSuppressMinRole, r),
            out _setSpawnSuppress);

        WardUIFactory.CreateFeatureToggleRow(
            card2, 
            "Territory Max Comfort Aura", 
            false, 
            WardRole.Guest,
            v => SetZdoBool(WardZdoKeys.MaxComfort, v), 
            r => SetFeatureRole(WardZdoKeys.MaxComfortMinRole, r),
            out _setMaxComfort);

        WardUIFactory.CreateFeatureToggleRow(
            card2, 
            "Portal Access Control", 
            false, 
            WardRole.Guest,
            v => SetZdoBool(WardZdoKeys.PortalAccess, v), 
            r => SetFeatureRole(WardZdoKeys.PortalMinRole, r),
            out _setPortalAccess);

        Transform card3 = WardUIFactory.CreateCard(listContainer, "Player Sanctuary & Vitality Protections");
        WardUIFactory.CreateFeatureToggleRow(
            card3,
            "Sanctuary Vitality Boost (50% Regen)",
            true,
            WardRole.Guest,
            v => SetZdoBool(WardZdoKeys.RegenBoost, v),
            r => SetFeatureRole(WardZdoKeys.RegenBoostMinRole, r),
            out _setRegenBoost);

        WardUIFactory.CreateFeatureToggleRow(
            card3,
            "Death Skill Loss Protection",
            true,
            WardRole.Guest,
            v => SetZdoBool(WardZdoKeys.DeathSkillShield, v),
            r => SetFeatureRole(WardZdoKeys.DeathSkillShieldMinRole, r),
            out _setDeathSkill);

        WardUIFactory.CreateFeatureToggleRow(
            card3,
            "Food Duration Freeze",
            true,
            WardRole.Guest,
            v => SetZdoBool(WardZdoKeys.FoodFreeze, v),
            r => SetFeatureRole(WardZdoKeys.FoodFreezeMinRole, r),
            out _setFoodFreeze);

        Transform card4 = WardUIFactory.CreateCard(listContainer, "Shield Visuals & Property Boundary");
        WardUIFactory.CreateToggleRow(
            card4,
            "Show 3D Forcefield Dome",
            true,
            v => SetZdoBool(WardZdoKeys.ShowShieldDome, v),
            out _setShowDomeToggle);

        WardUIFactory.CreateToggleRow(
            card4,
            "Show Ground Perimeter Ring",
            true,
            v => SetZdoBool(WardZdoKeys.ShowPerimeterRing, v),
            out _setShowRingToggle);
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
        bool canManage = WardPermissions.CanManageWard(zdo, playerId);

        float push = zdo.GetFloat(WardZdoKeys.PushbackForce, 0f);
        _setPushbackSlider?.Invoke(push);

        float shockC = zdo.GetFloat(WardZdoKeys.ShockCreatureDmg, 0f);
        _setShockCreatureSlider?.Invoke(shockC);

        float shockP = zdo.GetFloat(WardZdoKeys.ShockPlayerDmg, 0f);
        _setShockPlayerSlider?.Invoke(shockP);

        _setGhostBase?.Invoke(zdo.GetBool(WardZdoKeys.GhostBase, false), (WardRole)zdo.GetInt(WardZdoKeys.GhostBaseMinRole, 0), canManage);
        _setRaidSuppress?.Invoke(zdo.GetBool(WardZdoKeys.RaidSuppress, false), (WardRole)zdo.GetInt(WardZdoKeys.RaidSuppressMinRole, 0), canManage);
        _setSpawnSuppress?.Invoke(zdo.GetBool(WardZdoKeys.SpawnSuppress, false), (WardRole)zdo.GetInt(WardZdoKeys.SpawnSuppressMinRole, 0), canManage);
        _setMaxComfort?.Invoke(zdo.GetBool(WardZdoKeys.MaxComfort, false), (WardRole)zdo.GetInt(WardZdoKeys.MaxComfortMinRole, 0), canManage);
        _setPortalAccess?.Invoke(zdo.GetBool(WardZdoKeys.PortalAccess, false), (WardRole)zdo.GetInt(WardZdoKeys.PortalMinRole, 0), canManage);

        _setRegenBoost?.Invoke(zdo.GetBool(WardZdoKeys.RegenBoost, true), (WardRole)zdo.GetInt(WardZdoKeys.RegenBoostMinRole, 0), canManage);
        _setDeathSkill?.Invoke(zdo.GetBool(WardZdoKeys.DeathSkillShield, true), (WardRole)zdo.GetInt(WardZdoKeys.DeathSkillShieldMinRole, 0), canManage);
        _setFoodFreeze?.Invoke(zdo.GetBool(WardZdoKeys.FoodFreeze, true), (WardRole)zdo.GetInt(WardZdoKeys.FoodFreezeMinRole, 0), canManage);

        _setShowDomeToggle?.Invoke(zdo.GetBool(WardZdoKeys.ShowShieldDome, true));
        _setShowRingToggle?.Invoke(zdo.GetBool(WardZdoKeys.ShowPerimeterRing, true));
    }

    private void SetFeatureRole(int roleKey, WardRole role)
    {
        if (_ward != null && _ward.m_nview != null && _ward.m_nview.IsValid())
        {
            ZDO zdo = _ward.m_nview.GetZDO();
            zdo.Set(roleKey, (int)role);
            WardNetworkRouter.SendFeatureRole(zdo.m_uid, roleKey, (int)role);
        }
    }

    private void SetZdoBool(int key, bool val)
    {
        if (_ward != null && _ward.m_nview != null && _ward.m_nview.IsValid())
        {
            _ward.m_nview.GetZDO().Set(key, val);
        }
    }

    private void SetZdoFloat(int key, float val)
    {
        if (_ward != null && _ward.m_nview != null && _ward.m_nview.IsValid())
        {
            _ward.m_nview.GetZDO().Set(key, val);
        }
    }
}
