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

        WardType type = (WardType)zdo.GetInt(WardZdoKeys.WardType, (int)WardType.Main);
        if (type == WardType.SubWard)
            return zdo;

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

        if (type == WardType.Extension && current.m_uid == zdo.m_uid && current.GetZDOID(WardZdoKeys.ParentWard) == ZDOID.None)
        {
            Vector3 pos = current.GetPosition();
            float rad = current.GetFloat(WardZdoKeys.Radius, 15f);
            if (PrivateArea.m_allAreas != null)
            {
                for (int i = 0; i < PrivateArea.m_allAreas.Count; i++)
                {
                    PrivateArea area = PrivateArea.m_allAreas[i];
                    if (area == null || !area.IsEnabled() || area.m_nview == null || !area.m_nview.IsValid())
                        continue;

                    ZDO aZdo = area.m_nview.GetZDO();
                    if (aZdo.m_uid == current.m_uid)
                        continue;

                    WardType aType = (WardType)aZdo.GetInt(WardZdoKeys.WardType, (int)WardType.Main);
                    if (aType != WardType.Main)
                        continue;

                    float aRadius = WardQueryService.GetWardRadius(area);
                    if (Utils.DistanceXZ(area.transform.position, pos) <= aRadius + rad)
                    {
                        current.Set(WardZdoKeys.ParentWard, aZdo.m_uid);
                        if (WardRegistryManager.Instance != null)
                        {
                            WardRegistryManager.Instance.RegisterOrUpdate(current);
                        }
                        return aZdo;
                    }
                }
            }
        }

        return current;
    }

    public static ZDO GetEffectiveDefenseWardZdo(ZDO zdo)
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
            ZDO defenseZdo = GetEffectiveDefenseWardZdo(zdo) ?? zdo;
            if (defenseZdo != null && HasFeatureEnabled(defenseZdo, WardZdoKeys.WaterDrain))
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
            ZDO defenseZdo = GetEffectiveDefenseWardZdo(zdo) ?? zdo;
            if (defenseZdo != null && HasFeatureEnabled(defenseZdo, WardZdoKeys.GhostBase))
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
        PrivateArea bestArea = null;
        float bestRadius = float.MaxValue;
        bool foundSubWard = false;

        for (int i = 0; i < allAreas.Count; i++)
        {
            PrivateArea area = allAreas[i];
            if (area == null)
                continue;

            if (IsInsideWard(area, point, radius))
            {
                PrivateArea effective = GetEffectiveWard(area);
                if (effective == null || effective.m_nview == null || !effective.m_nview.IsValid())
                    continue;

                ZDO effectiveZdo = effective.m_nview.GetZDO();
                WardType effectiveType = (WardType)effectiveZdo.GetInt(WardZdoKeys.WardType, (int)WardType.Main);
                float effectiveRadius = GetWardRadius(effective);

                if (effectiveType == WardType.SubWard)
                {
                    if (!foundSubWard || effectiveRadius < bestRadius)
                    {
                        bestArea = effective;
                        bestRadius = effectiveRadius;
                        foundSubWard = true;
                    }
                }
                else if (!foundSubWard && (bestArea == null || effectiveRadius < bestRadius))
                {
                    bestArea = effective;
                    bestRadius = effectiveRadius;
                }
            }
        }

        _cachedCoveringWard = bestArea;
        return _cachedCoveringWard;
    }

    public static PrivateArea GetActiveSubWardCovering(Vector3 pos)
    {
        if (PrivateArea.m_allAreas == null || PrivateArea.m_allAreas.Count == 0)
            return null;

        PrivateArea bestSubWard = null;
        float bestDist = float.MaxValue;

        for (int i = 0; i < PrivateArea.m_allAreas.Count; i++)
        {
            PrivateArea area = PrivateArea.m_allAreas[i];
            if (area == null || area.m_nview == null || !area.m_nview.IsValid())
                continue;

            if (!area.IsEnabled())
                continue;

            ZDO zdo = area.m_nview.GetZDO();
            WardType type = (WardType)zdo.GetInt(WardZdoKeys.WardType, (int)WardType.Main);
            if (type != WardType.SubWard)
                continue;

            if (IsInsideWard(area, pos))
            {
                float dist = Utils.DistanceXZ(area.transform.position, pos);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    bestSubWard = area;
                }
            }
        }

        return bestSubWard;
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

        WardType type = (WardType)parentWardZdo.GetInt(WardZdoKeys.WardType, (int)WardType.Main);
        if (type == WardType.SubWard)
        {
            float subRadius = Mathf.Clamp(parentWardZdo.GetFloat(WardZdoKeys.Radius, 30.0f), 0.0f, 100.0f);
            return Utils.DistanceXZ(parentWardZdo.GetPosition(), point) <= subRadius;
        }

        ZDO effectiveZdo = GetEffectiveWardZdo(parentWardZdo);
        if (effectiveZdo == null || !effectiveZdo.IsValid())
            return false;

        float parentRadius = Mathf.Clamp(effectiveZdo.GetFloat(WardZdoKeys.Radius, 30.0f), 0.0f, 100.0f);
        if (Utils.DistanceXZ(effectiveZdo.GetPosition(), point) <= parentRadius)
            return true;

        List<KnownExtensionWard> extensions = WardRegistryManager.Instance.GetExtensionsForParent(effectiveZdo.m_uid);
        for (int i = 0; i < extensions.Count; i++)
        {
            KnownExtensionWard ext = extensions[i];
            if (!ext.IsEnabled)
                continue;

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

        float defaultRadius = (ConfigRegistry.EnableProgressionSystem != null && ConfigRegistry.EnableProgressionSystem.Value) ? WardProgressionService.BaselineRadius : 30.0f;
        float maxAllowed = WardProgressionService.GetMaxAllowedRadius(zdo);
        return Mathf.Clamp(zdo.GetFloat(WardZdoKeys.Radius, defaultRadius), 0.0f, maxAllowed);
    }

    public static bool GetDefaultFeatureState(int featureKey)
    {
        if (featureKey == WardZdoKeys.FoodFreeze)
            return true;
        if (featureKey == WardZdoKeys.RegenBoost)
            return true;
        if (featureKey == WardZdoKeys.DeathSkillShield)
            return true;

        return false;
    }

    public static WardFeature? GetFeatureForZdoKey(int featureKey)
    {
        if (featureKey == WardZdoKeys.PushbackForce) return WardFeature.PushbackForce;
        if (featureKey == WardZdoKeys.ShockCreatureDmg) return WardFeature.CreatureShock;
        if (featureKey == WardZdoKeys.ShockPlayerDmg) return WardFeature.PlayerShock;
        if (featureKey == WardZdoKeys.GhostBase) return WardFeature.GhostBase;
        if (featureKey == WardZdoKeys.RaidSuppress) return WardFeature.RaidSuppression;
        if (featureKey == WardZdoKeys.SpawnSuppress) return WardFeature.SpawnSuppression;
        if (featureKey == WardZdoKeys.WeatherRoof) return WardFeature.WeatherRoof;
        if (featureKey == WardZdoKeys.AshShield) return WardFeature.AshShield;
        if (featureKey == WardZdoKeys.LavaShield) return WardFeature.LavaShield;
        if (featureKey == WardZdoKeys.SnowShield) return WardFeature.SnowShield;
        if (featureKey == WardZdoKeys.WaterDrain) return WardFeature.WaterDrain;
        if (featureKey == WardZdoKeys.MaxComfort) return WardFeature.MaxComfort;
        if (featureKey == WardZdoKeys.RegenBoost) return WardFeature.RegenBoost;
        if (featureKey == WardZdoKeys.DeathSkillShield) return WardFeature.DeathSkillShield;
        if (featureKey == WardZdoKeys.FoodFreeze) return WardFeature.FoodFreeze;
        if (featureKey == WardZdoKeys.PortalAccess) return WardFeature.PortalAccess;
        if (featureKey == WardZdoKeys.RepairEnabled) return WardFeature.AreaRepair;
        if (featureKey == WardZdoKeys.AutoFuelEnabled) return WardFeature.AutoFuel;
        if (featureKey == WardZdoKeys.AutoCloseDoors) return WardFeature.AutoCloseDoors;
        if (featureKey == WardZdoKeys.ProtectTames) return WardFeature.ProtectTames;
        if (featureKey == WardZdoKeys.ProtectCrops) return WardFeature.ProtectCrops;
        if (featureKey == WardZdoKeys.ProtectTerrain) return WardFeature.ProtectTerrain;
        if (featureKey == WardZdoKeys.LockVehicles) return WardFeature.VehicleLocking;
        if (featureKey == WardZdoKeys.FastTravel) return WardFeature.FastTravel;
        return null;
    }

    public static bool HasFeatureEnabled(Vector3 point, int featureKey, long playerId = 0L)
    {
        if (ConfigRegistry.Enabled != null && !ConfigRegistry.Enabled.Value)
            return false;

        ZDO wardZdo = GetCoveringWardZdo(point);
        if (wardZdo == null)
            return false;

        return HasFeatureEnabled(wardZdo, featureKey, playerId);
    }

    public static bool HasFeatureEnabled(ZDO wardZdo, int featureKey, long playerId = 0L)
    {
        if (wardZdo == null)
            return false;

        if (ConfigRegistry.Enabled != null && !ConfigRegistry.Enabled.Value)
            return false;

        ZDO defenseZdo = GetEffectiveDefenseWardZdo(wardZdo) ?? wardZdo;
        if (defenseZdo == null || !defenseZdo.GetBool(ZDOVars.s_enabled, true))
            return false;

        WardFeature? feature = GetFeatureForZdoKey(featureKey);
        if (feature.HasValue && !WardProgressionService.IsFeatureUnlocked(defenseZdo, feature.Value, playerId))
            return false;

        if (featureKey == WardZdoKeys.PushbackForce || featureKey == WardZdoKeys.ShockCreatureDmg || featureKey == WardZdoKeys.ShockPlayerDmg)
        {
            if (defenseZdo.GetFloat(featureKey, 0f) <= 0f)
                return false;
        }
        else
        {
            bool defaultValue = GetDefaultFeatureState(featureKey);
            if (!defenseZdo.GetBool(featureKey, defaultValue))
                return false;
        }

        if (playerId != 0L && !WardPermissions.HasFeatureAccess(defenseZdo, featureKey, playerId))
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

    public static float GetMaxAllowedSubWardRadius(PrivateArea subWard)
    {
        if (subWard == null)
            return 100f;

        ZDO zdo = subWard.m_nview != null && subWard.m_nview.IsValid() ? subWard.m_nview.GetZDO() : null;
        return GetMaxAllowedSubWardRadius(zdo, subWard.transform.position);
    }

    public static float GetMaxAllowedSubWardRadius(ZDO subWardZdo, Vector3 subWardPos, ZDO hintParentZdo = null)
    {
        float maxRadius = 100f;
        if (ConfigRegistry.MaxWardRadius != null && ConfigRegistry.MaxWardRadius.Value > 10f)
        {
            maxRadius = Mathf.Min(100f, (float)ConfigRegistry.MaxWardRadius.Value);
        }

        ZDOID parentId = ZDOID.None;
        if (subWardZdo != null)
        {
            parentId = subWardZdo.GetZDOID(WardZdoKeys.ParentWard);
        }

        if (parentId == ZDOID.None && hintParentZdo != null && hintParentZdo.IsValid())
        {
            ZDO effectiveHint = GetEffectiveWardZdo(hintParentZdo) ?? hintParentZdo;
            parentId = effectiveHint.m_uid;
        }

        if (parentId == ZDOID.None && PrivateArea.m_allAreas != null)
        {
            for (int i = 0; i < PrivateArea.m_allAreas.Count; i++)
            {
                PrivateArea area = PrivateArea.m_allAreas[i];
                if (area == null || !area.IsEnabled() || area.m_nview == null || !area.m_nview.IsValid())
                    continue;

                if (subWardZdo != null && area.m_nview.GetZDO().m_uid == subWardZdo.m_uid)
                    continue;

                float radius = GetWardRadius(area);
                if (Utils.DistanceXZ(subWardPos, area.transform.position) <= radius)
                {
                    ZDO effective = GetEffectiveWardZdo(area.m_nview.GetZDO()) ?? area.m_nview.GetZDO();
                    parentId = effective.m_uid;
                    break;
                }
            }
        }

        if (parentId == ZDOID.None)
        {
            List<KnownWard> allKnown = WardRegistryManager.Instance.GetAllWards();
            for (int i = 0; i < allKnown.Count; i++)
            {
                KnownWard kw = allKnown[i];
                if (kw.IsEnabled && Utils.DistanceXZ(subWardPos, kw.Position) <= kw.Radius)
                {
                    parentId = kw.Id;
                    break;
                }
            }
        }

        if (parentId != ZDOID.None && subWardZdo != null && subWardZdo.IsValid())
        {
            WardType currentType = (WardType)subWardZdo.GetInt(WardZdoKeys.WardType, (int)WardType.Main);
            if (currentType == WardType.SubWard && subWardZdo.GetZDOID(WardZdoKeys.ParentWard) == ZDOID.None)
            {
                subWardZdo.Set(WardZdoKeys.ParentWard, parentId);
                WardRegistryManager.Instance.RegisterOrUpdate(subWardZdo);
                Networking.WardNetworkRouter.SendRegisterWard(subWardZdo.m_uid);
            }
        }

        List<WardCoverageCircle> baseCircles = new();
        HashSet<ZDOID> seenWards = new();

        if (PrivateArea.m_allAreas != null)
        {
            for (int i = 0; i < PrivateArea.m_allAreas.Count; i++)
            {
                PrivateArea area = PrivateArea.m_allAreas[i];
                if (area == null || !area.IsEnabled() || area.m_nview == null || !area.m_nview.IsValid())
                    continue;

                ZDO areaZdo = area.m_nview.GetZDO();
                if (subWardZdo != null && areaZdo.m_uid == subWardZdo.m_uid)
                    continue;

                WardType areaType = (WardType)areaZdo.GetInt(WardZdoKeys.WardType, (int)WardType.Main);
                if (areaType == WardType.SubWard)
                    continue;

                ZDO effectiveParent = GetEffectiveWardZdo(areaZdo) ?? areaZdo;
                bool isThisBase = (parentId != ZDOID.None && (areaZdo.m_uid == parentId || effectiveParent.m_uid == parentId));
                if (isThisBase)
                {
                    seenWards.Add(areaZdo.m_uid);
                    baseCircles.Add(new WardCoverageCircle(area.transform.position, GetWardRadius(area)));
                }
            }
        }

        if (parentId != ZDOID.None)
        {
            if (!seenWards.Contains(parentId))
            {
                KnownWard parentKnown = WardRegistryManager.Instance.GetKnownWard(parentId);
                if (parentKnown != null && parentKnown.IsEnabled)
                {
                    seenWards.Add(parentId);
                    baseCircles.Add(new WardCoverageCircle(parentKnown.Position, parentKnown.Radius));
                }
            }

            List<KnownExtensionWard> exts = WardRegistryManager.Instance.GetExtensionsForParent(parentId);
            for (int i = 0; i < exts.Count; i++)
            {
                KnownExtensionWard ext = exts[i];
                if (ext.IsEnabled && !seenWards.Contains(ext.Id))
                {
                    seenWards.Add(ext.Id);
                    baseCircles.Add(new WardCoverageCircle(ext.Position, ext.Radius));
                }
            }
        }

        if (baseCircles.Count == 0)
        {
            return 15f;
        }

        Vector2 subPos2D = new(subWardPos.x, subWardPos.z);
        bool isInsideBase = false;
        for (int i = 0; i < baseCircles.Count; i++)
        {
            if (Vector2.Distance(subPos2D, baseCircles[i].Center) <= baseCircles[i].Radius + 0.1f)
            {
                isInsideBase = true;
                break;
            }
        }

        if (!isInsideBase)
        {
            return 1f;
        }

        const int SampleCount = 72;
        const float AngleStep = 360f / SampleCount;
        float smallestBoundaryDist = float.MaxValue;
        List<CoverageInterval> intervals = new(baseCircles.Count);

        for (int s = 0; s < SampleCount; s++)
        {
            float rad = (s * AngleStep) * Mathf.Deg2Rad;
            Vector2 dir = new(Mathf.Cos(rad), Mathf.Sin(rad));

            intervals.Clear();
            for (int c = 0; c < baseCircles.Count; c++)
            {
                WardCoverageCircle circle = baseCircles[c];
                Vector2 v = subPos2D - circle.Center;
                float b = Vector2.Dot(v, dir);
                float cVal = Vector2.Dot(v, v) - (circle.Radius * circle.Radius);
                float disc = (b * b) - cVal;
                if (disc >= 0f)
                {
                    float sqrtDisc = Mathf.Sqrt(disc);
                    float tEnter = -b - sqrtDisc;
                    float tExit = -b + sqrtDisc;
                    if (tExit > 0f)
                    {
                        intervals.Add(new CoverageInterval(Mathf.Max(0f, tEnter), tExit));
                    }
                }
            }

            float maxT = 0f;
            bool expanded = true;
            while (expanded)
            {
                expanded = false;
                for (int i = 0; i < intervals.Count; i++)
                {
                    CoverageInterval interval = intervals[i];
                    if (interval.Start <= maxT + 0.05f && interval.End > maxT)
                    {
                        maxT = interval.End;
                        expanded = true;
                    }
                }
            }

            if (maxT < smallestBoundaryDist)
            {
                smallestBoundaryDist = maxT;
            }
        }

        ZDOID currentSubWardId = subWardZdo != null ? subWardZdo.m_uid : ZDOID.None;
        HashSet<ZDOID> seenSiblings = new();

        if (PrivateArea.m_allAreas != null)
        {
            for (int i = 0; i < PrivateArea.m_allAreas.Count; i++)
            {
                PrivateArea area = PrivateArea.m_allAreas[i];
                if (area == null || !area.IsEnabled() || area.m_nview == null || !area.m_nview.IsValid())
                    continue;

                ZDO areaZdo = area.m_nview.GetZDO();
                if (currentSubWardId != ZDOID.None && areaZdo.m_uid == currentSubWardId)
                    continue;

                WardType areaType = (WardType)areaZdo.GetInt(WardZdoKeys.WardType, (int)WardType.Main);
                if (areaType == WardType.SubWard)
                {
                    seenSiblings.Add(areaZdo.m_uid);
                    float siblingRadius = GetWardRadius(area);
                    float distToSibling = Utils.DistanceXZ(subWardPos, area.transform.position);
                    float allowedFromSibling = Mathf.Max(1f, distToSibling - siblingRadius);
                    if (allowedFromSibling < smallestBoundaryDist)
                    {
                        smallestBoundaryDist = allowedFromSibling;
                    }
                }
            }
        }

        List<KnownWard> allWards = WardRegistryManager.Instance.GetAllWards();
        for (int i = 0; i < allWards.Count; i++)
        {
            KnownWard ward = allWards[i];
            if (currentSubWardId != ZDOID.None && ward.Id == currentSubWardId)
                continue;

            if (ward.Type == WardType.SubWard && !seenSiblings.Contains(ward.Id))
            {
                seenSiblings.Add(ward.Id);
                float distToSibling = Utils.DistanceXZ(subWardPos, ward.Position);
                float allowedFromSibling = Mathf.Max(1f, distToSibling - ward.Radius);
                if (allowedFromSibling < smallestBoundaryDist)
                {
                    smallestBoundaryDist = allowedFromSibling;
                }
            }
        }

        return Mathf.Clamp(smallestBoundaryDist, 1f, maxRadius);
    }
}

