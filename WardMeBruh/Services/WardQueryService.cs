using System.Collections.Generic;
using UnityEngine;
using WardMeBruh.Configuration;
using WardMeBruh.Models;

namespace WardMeBruh.Services;

public static class WardQueryService
{
    public static PrivateArea GetCoveringWard(Vector3 point, float radius = 0f)
    {
        if (ConfigRegistry.Enabled != null && !ConfigRegistry.Enabled.Value)
            return null;

        if (PrivateArea.m_allAreas == null)
            return null;

        List<PrivateArea> allAreas = PrivateArea.m_allAreas;
        for (int i = 0; i < allAreas.Count; i++)
        {
            PrivateArea area = allAreas[i];
            if (area == null || !area.IsEnabled())
                continue;

            if (IsInsideWard(area, point, radius))
                return area;
        }

        return null;
    }

    public static ZDO GetCoveringWardZdo(Vector3 point, float radius = 0f)
    {
        PrivateArea area = GetCoveringWard(point, radius);
        if (area == null || area.m_nview == null || !area.m_nview.IsValid())
            return null;

        return area.m_nview.GetZDO();
    }

    public static bool IsInsideWard(PrivateArea area, Vector3 point, float radius = 0f)
    {
        if (area == null)
            return false;

        float effectiveRadius = GetWardRadius(area);
        float distance = Utils.DistanceXZ(area.transform.position, point);

        return distance <= effectiveRadius + radius;
    }

    public static bool IsInsideWardSphere(PrivateArea area, Vector3 point, float radius = 0f)
    {
        if (area == null)
            return false;

        float effectiveRadius = GetWardRadius(area);
        float distance = Vector3.Distance(area.transform.position, point);

        return distance <= effectiveRadius + radius;
    }

    public static float GetWardRadius(PrivateArea area)
    {
        if (ConfigRegistry.Enabled != null && !ConfigRegistry.Enabled.Value)
            return 32.0f;

        if (area == null || area.m_nview == null || !area.m_nview.IsValid())
            return 32.0f;

        ZDO zdo = area.m_nview.GetZDO();
        if (zdo == null)
            return 32.0f;

        return zdo.GetFloat(WardZdoKeys.Radius, 32.0f);
    }

    public static bool HasFeatureEnabled(Vector3 point, int featureKey, long playerId = 0L)
    {
        if (ConfigRegistry.Enabled != null && !ConfigRegistry.Enabled.Value)
            return false;

        ZDO wardZdo = GetCoveringWardZdo(point);
        if (wardZdo == null)
            return false;

        if (!wardZdo.GetBool(featureKey, false))
            return false;

        if (playerId != 0L && !WardPermissions.HasFeatureAccess(wardZdo, featureKey, playerId))
            return false;

        return true;
    }

    public static bool IsTamedOrTaming(Character character)
    {
        if (character == null)
            return false;

        if (character.IsTamed())
            return true;

        Tameable tameable = character.GetComponent<Tameable>();
        if (tameable != null && character.m_nview != null && character.m_nview.IsValid())
        {
            ZDO zdo = character.m_nview.GetZDO();
            if (zdo != null && tameable.m_tamingTime > 0f)
            {
                float remainingTime = zdo.GetFloat(ZDOVars.s_tameTimeLeft, tameable.m_tamingTime);
                if (remainingTime < tameable.m_tamingTime)
                {
                    int tameness = (int)((1f - Mathf.Clamp01(remainingTime / tameable.m_tamingTime)) * 100f);
                    if (tameness > 0)
                        return true;
                }
            }
        }

        return false;
    }
}
