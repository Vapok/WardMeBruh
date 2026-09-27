using System.Text;
using UnityEngine;
using WardMeBruh.Configuration;
using WardMeBruh.Services;

namespace WardMeBruh.Models;

public class SE_WardTerritory : StatusEffect
{
    public static readonly int EffectHash = "SE_WardTerritory".GetStableHashCode();

    private WardRole _currentRole = WardRole.Guest;
    private string _baseName = "Ward Territory";
    private string _creatorName = "Unknown";
    private float _wardRadius = 32f;

    public override void Setup(Character character)
    {
        base.Setup(character);
        name = "SE_WardTerritory";
        m_name = "Ward Territory";
        m_icon = WardIconHelper.GetWardIcon();
        UpdateTerritoryData();
    }

    public override void UpdateStatusEffect(float dt)
    {
        base.UpdateStatusEffect(dt);

        if (m_character == null)
            return;

        UpdateTerritoryData();
    }

    private void UpdateTerritoryData()
    {
        if (m_character == null)
            return;

        PrivateArea ward = WardQueryService.GetCoveringWard(m_character.transform.position);
        long playerId = (m_character is Player player) ? player.GetPlayerID() : 0L;
        if (ward == null || !ward.IsEnabled() || ward.m_nview == null || !ward.m_nview.IsValid() || !WardPermissions.HasWardAccess(ward, playerId))
        {
            if (m_character.GetSEMan() != null)
            {
                m_character.GetSEMan().RemoveStatusEffect(EffectHash);
            }
            return;
        }

        ZDO zdo = ward.m_nview.GetZDO();
        _baseName = zdo.GetString(WardZdoKeys.CustomName, ward.m_name);
        if (string.IsNullOrEmpty(_baseName))
        {
            _baseName = "Ward Stone";
        }

        _currentRole = WardPermissions.GetPlayerRole(zdo, playerId);
        _creatorName = zdo.GetString(ZDOVars.s_creatorName, "Unknown");
        _wardRadius = WardQueryService.GetWardRadius(ward);

        m_name = _baseName;

        if (m_icon == null)
        {
            m_icon = WardIconHelper.GetWardIcon();
        }
    }

    public override string GetIconText()
    {
        return _currentRole.ToString();
    }

    public override string GetTooltipString()
    {
        StringBuilder sb = new();
        sb.AppendLine($"<color=#fcd34d>Territory:</color> {_baseName}");
        sb.AppendLine($"<color=#38bdf8>Clearance:</color> {_currentRole}");
        sb.AppendLine($"<color=#9ca3af>Owner:</color> {_creatorName}");
        sb.AppendLine($"<color=#9ca3af>Perimeter:</color> {_wardRadius:F0}m");
        sb.AppendLine("<color=#4ade80>Protected by Ward Shield</color>");
        return sb.ToString();
    }
}
