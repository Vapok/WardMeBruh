using System.Collections.Generic;
using UnityEngine;
using WardMeBruh.Configuration;
using WardMeBruh.Models;

namespace WardMeBruh.Services;

public static class WardQueryService
{
    public static ZDO GetEffectiveWardZdo(ZDO zdo)
    {
        if (zdo == null || !zdo.IsValid())
            return null;

        ZDO current = zdo;
        int depth = 0;
        while (depth < 10)
        {
            ZDOID parentId = current.GetZDOID(WardZdoKeys.ParentWard);
            if (parentId == ZDOID.None || parentId == current.m_uid)
                break;

            ZDO parentZdo = (ZDOMan.instance != null) ? ZDOMan.instance.GetZDO(parentId) : null;
            if (parentZdo == null || !parentZdo.IsValid())
                break;

            current = parentZdo;
            depth++;
        }

        return current;
    }

    public static PrivateArea GetEffectiveWard(PrivateArea ward)
    {
        if (ward == null || ward.m_nview == null || !ward.m_nview.IsValid())
            return ward;

        ZDO zdo = ward.m_nview.GetZDO();
        if (zdo == null || !zdo.IsValid())
            return ward;

        ZDO effectiveZdo = GetEffectiveWardZdo(zdo);
        if (effectiveZdo == null || effectiveZdo.m_uid == zdo.m_uid)
            return ward;

        if (PrivateArea.m_allAreas != null)
        {
            for (int i = 0; i < PrivateArea.m_allAreas.Count; i++)
            {
                PrivateArea area = PrivateArea.m_allAreas[i];
                if (area != null && area.m_nview != null && area.m_nview.IsValid())
                {
                    if (area.m_nview.GetZDO().m_uid == effectiveZdo.m_uid)
                        return area;
                }
            }
        }

        return ward;
    }

    private static int _lastWardFrame = -1;
    private static Vector3 _lastWardPoint;
    private static float _lastWardRadius;
    private static PrivateArea _cachedCoveringWard;

    private static int _lastZdoFrame = -1;
    private static Vector3 _lastZdoPoint;
    private static float _lastZdoRadius;
    private static ZDO _cachedCoveringZdo;

    private static float _lastWaterDrainCheckTime = -1f;
    private static bool _hasAnyActiveWaterDrain;

    private static float _lastGhostBaseCheckTime = -1f;
    private static bool _hasAnyActiveGhostBase;

    public static bool HasAnyActiveWaterDrain()
    {
        float now = Time.time;
        if (now - _lastWaterDrainCheckTime < 1.0f)
            return _hasAnyActiveWaterDrain;

        _lastWaterDrainCheckTime = now;
        _hasAnyActiveWaterDrain = false;

        if (PrivateArea.m_allAreas == null || PrivateArea.m_allAreas.Count == 0)
            return false;

        for (int i = 0; i < PrivateArea.m_allAreas.Count; i++)
        {
            PrivateArea area = PrivateArea.m_allAreas[i];
            if (area == null || !area.IsEnabled() || area.m_nview == null || !area.m_nview.IsValid())
                continue;

            ZDO zdo = area.m_nview.GetZDO();
            if (zdo != null && zdo.GetBool(WardZdoKeys.WaterDrain, false))
            {
                _hasAnyActiveWaterDrain = true;
                break;
            }
        }

        return _hasAnyActiveWaterDrain;
    }

    public static bool HasAnyActiveGhostBase()
    {
        float now = Time.time;
        if (now - _lastGhostBaseCheckTime < 1.0f)
            return _hasAnyActiveGhostBase;

        _lastGhostBaseCheckTime = now;
        _hasAnyActiveGhostBase = false;

        if (PrivateArea.m_allAreas == null || PrivateArea.m_allAreas.Count == 0)
            return false;

        for (int i = 0; i < PrivateArea.m_allAreas.Count; i++)
        {
            PrivateArea area = PrivateArea.m_allAreas[i];
            if (area == null || !area.IsEnabled() || area.m_nview == null || !area.m_nview.IsValid())
                continue;

            ZDO zdo = area.m_nview.GetZDO();
            if (zdo != null && zdo.GetBool(WardZdoKeys.GhostBase, false))
            {
                _hasAnyActiveGhostBase = true;
                break;
            }
        }

        return _hasAnyActiveGhostBase;
    }

    public static void InvalidateFeatureCaches()
    {
        _lastWaterDrainCheckTime = -1f;
        _lastGhostBaseCheckTime = -1f;
    }

    public static PrivateArea GetCoveringWard(Vector3 point, float radius = 0f)
    {
        if (ConfigRegistry.Enabled != null && !ConfigRegistry.Enabled.Value)
            return null;

        if (Time.frameCount == _lastWardFrame && _lastWardPoint == point && Mathf.Approximately(_lastWardRadius, radius))
            return _cachedCoveringWard;

        _lastWardFrame = Time.frameCount;
        _lastWardPoint = point;
        _lastWardRadius = radius;

        if (PrivateArea.m_allAreas == null)
        {
            _cachedCoveringWard = null;
            return null;
        }

        List<PrivateArea> allAreas = PrivateArea.m_allAreas;
        for (int i = 0; i < allAreas.Count; i++)
        {
            PrivateArea area = allAreas[i];
            if (area == null)
                continue;

            if (IsInsideWard(area, point, radius))
            {
                _cachedCoveringWard = GetEffectiveWard(area);
                return _cachedCoveringWard;
            }
        }

        _cachedCoveringWard = null;
        return null;
    }

    public static ZDO GetCoveringWardZdo(Vector3 point, float radius = 0f)
    {
        if (Time.frameCount == _lastZdoFrame && _lastZdoPoint == point && Mathf.Approximately(_lastZdoRadius, radius))
            return _cachedCoveringZdo;

        _lastZdoFrame = Time.frameCount;
        _lastZdoPoint = point;
        _lastZdoRadius = radius;

        PrivateArea area = GetCoveringWard(point, radius);
        if (area != null && area.m_nview != null && area.m_nview.IsValid())
        {
            _cachedCoveringZdo = GetEffectiveWardZdo(area.m_nview.GetZDO());
            return _cachedCoveringZdo;
        }

        KnownWard known = WardRegistryManager.Instance.GetWardCoveringPoint(point);
        if (known != null && ZDOMan.instance != null)
        {
            ZDO zdo = ZDOMan.instance.GetZDO(known.Id);
            if (zdo != null && zdo.IsValid())
            {
                _cachedCoveringZdo = GetEffectiveWardZdo(zdo);
                return _cachedCoveringZdo;
            }
        }

        _cachedCoveringZdo = null;
        return null;
    }

    public static bool IsCoveredByWardOrExtensions(ZDO parentWardZdo, Vector3 point)
    {
        if (parentWardZdo == null || !parentWardZdo.IsValid())
            return false;

        ZDO effectiveZdo = GetEffectiveWardZdo(parentWardZdo);
        if (effectiveZdo == null || !effectiveZdo.IsValid())
            return false;

        float parentRadius = Mathf.Clamp(effectiveZdo.GetFloat(WardZdoKeys.Radius, 30.0f), 10.0f, 100.0f);
        if (Utils.DistanceXZ(effectiveZdo.GetPosition(), point) <= parentRadius)
            return true;

        List<KnownExtensionWard> extensions = WardRegistryManager.Instance.GetExtensionsForParent(effectiveZdo.m_uid);
        for (int i = 0; i < extensions.Count; i++)
        {
            KnownExtensionWard ext = extensions[i];
            if (Utils.DistanceXZ(ext.Position, point) <= ext.Radius)
                return true;
        }

        return false;
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

        return Mathf.Clamp(zdo.GetFloat(WardZdoKeys.Radius, 30.0f), 10.0f, 100.0f);
    }

    public static bool HasFeatureEnabled(Vector3 point, int featureKey, long playerId = 0L)
    {
        if (ConfigRegistry.Enabled != null && !ConfigRegistry.Enabled.Value)
            return false;

        ZDO wardZdo = GetCoveringWardZdo(point);
        if (wardZdo == null)
            return false;

        if (!wardZdo.GetBool(ZDOVars.s_enabled, true))
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
