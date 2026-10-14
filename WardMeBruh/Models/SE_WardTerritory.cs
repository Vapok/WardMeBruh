using System.Text;
using UnityEngine;
using WardMeBruh.Configuration;
using WardMeBruh.Services;
using WardMeBruh.UI;

namespace WardMeBruh.Models;

public class SE_WardTerritory : StatusEffect
{
    public static readonly int EffectHash = "SE_WardTerritory".GetStableHashCode();

    private WardRole _currentRole = WardRole.Guest;
    private int _currentRoleId = (int)WardRole.Guest;
    private ZDOID _effectiveWardId = ZDOID.None;
    private bool _isAdmin;
    private string _baseName = "$wmb_ward_territory".Localize();
    private string _creatorName = "Unknown";
    private float _wardRadius = 32f;

    public override void Setup(Character character)
    {
        base.Setup(character);
        name = "SE_WardTerritory";
        m_name = "$wmb_ward_territory".Localize();
        m_icon = WardIconHelper.GetWardIcon();
        UpdateTerritoryData();
    }

    private float _timeSinceLastUpdate;
    private const float UpdateIntervalSeconds = 0.5f;

    public override void UpdateStatusEffect(float dt)
    {
        base.UpdateStatusEffect(dt);

        if (m_character == null)
            return;

        _timeSinceLastUpdate += dt;
        if (_timeSinceLastUpdate < UpdateIntervalSeconds)
            return;

        _timeSinceLastUpdate = 0f;
        UpdateTerritoryData();
    }

    private void UpdateTerritoryData()
    {
        if (m_character == null)
            return;

        PrivateArea ward = WardQueryService.GetCoveringWard(m_character.transform.position);
        long playerId = (m_character is Player player) ? player.GetPlayerID() : 0L;
        if (ward == null || ward.m_nview == null || !ward.m_nview.IsValid())
        {
            if (m_character.GetSEMan() != null)
            {
                m_character.GetSEMan().RemoveStatusEffect(EffectHash);
            }
            return;
        }

        ZDO zdo = ward.m_nview.GetZDO();
        ZDO effectiveZdo = WardQueryService.GetEffectiveWardZdo(zdo);
        _effectiveWardId = effectiveZdo != null ? effectiveZdo.m_uid : ZDOID.None;
        _baseName = effectiveZdo.GetString(WardZdoKeys.CustomName, ward.m_name);
        if (string.IsNullOrEmpty(_baseName))
        {
            _baseName = "$wmb_default_ward_name".Localize();
        }

        _currentRole = WardPermissions.GetPlayerRole(effectiveZdo, playerId);
        _currentRoleId = WardRoleManager.GetPlayerRoleId(effectiveZdo, playerId);
        _isAdmin = WardPermissions.IsAdmin(playerId);
        _creatorName = effectiveZdo.GetString(ZDOVars.s_creatorName, "Unknown");
        _wardRadius = WardQueryService.GetWardRadius(ward);

        m_name = _baseName;

        if (m_icon == null)
        {
            m_icon = WardIconHelper.GetWardIcon();
        }
    }

    public override string GetIconText()
    {
        ZDO wardZdo = (ZDOMan.instance != null && _effectiveWardId != ZDOID.None) ? ZDOMan.instance.GetZDO(_effectiveWardId) : null;
        string roleName = WardRoleManager.GetRoleDisplayName(wardZdo, _currentRoleId);
        if (_isAdmin)
        {
            return $"{roleName}\n[Server Admin]";
        }
        return roleName;
    }

    public override string GetTooltipString()
    {
        ZDO wardZdo = (ZDOMan.instance != null && _effectiveWardId != ZDOID.None) ? ZDOMan.instance.GetZDO(_effectiveWardId) : null;
        string roleName = WardRoleManager.GetRoleDisplayName(wardZdo, _currentRoleId);
        StringBuilder sb = new();
        sb.AppendLine($"<color=#fcd34d>{"$wmb_territory_label".Localize()}</color> {_baseName.Localize()}");
        sb.AppendLine($"<color=#38bdf8>{"$wmb_clearance_label".Localize()}</color> {roleName}");
        if (_isAdmin)
        {
            sb.AppendLine("<color=#f87171>[Server Admin]</color>");
        }
        sb.AppendLine($"<color=#9ca3af>{"$wmb_owner_label".Localize()}</color> {_creatorName}");
        sb.AppendLine($"<color=#9ca3af>{"$wmb_perimeter_label".Localize()}</color> {_wardRadius:F0}{"$wmb_meters".Localize()}");
        PrivateArea ward = (m_character != null) ? WardQueryService.GetCoveringWard(m_character.transform.position) : null;
        if (ward != null && ward.IsEnabled())
        {
            sb.AppendLine($"<color=#4ade80>{"$wmb_shield_active_desc".Localize()}</color>");
        }
        else
        {
            sb.AppendLine($"<color=#ef4444>{"$wmb_shield_inactive_desc".Localize()}</color>");
        }
        return sb.ToString();
    }
}
