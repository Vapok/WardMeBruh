using System;
using System.Collections.Generic;
using UnityEngine;
using WardMeBruh.Configuration;
using WardMeBruh.Models;

namespace WardMeBruh.Services;

public sealed class WardRegistryManager
{
    private static readonly Lazy<WardRegistryManager> _lazy = new(() => new WardRegistryManager());
    public static WardRegistryManager Instance => _lazy.Value;

    private readonly Dictionary<ZDOID, KnownWard> _knownWards = new();

    public int Count => _knownWards.Count;

    private WardRegistryManager()
    {
    }

    public void RegisterOrUpdate(ZDO zdo)
    {
        if (zdo == null || !zdo.IsValid())
            return;

        ZDOID id = zdo.m_uid;
        string name = zdo.GetString(WardZdoKeys.CustomName, "Ward");
        Vector3 position = zdo.GetPosition();
        long creatorId = zdo.GetLong(ZDOVars.s_creator, 0L);
        string creatorName = zdo.GetString(ZDOVars.s_creatorName, "Unknown");
        float radius = zdo.GetFloat(WardZdoKeys.Radius, 32.0f);
        bool isEnabled = zdo.GetBool(ZDOVars.s_enabled, true);

        KnownWard ward = new(id, name, position, creatorId, creatorName, radius, isEnabled);
        _knownWards[id] = ward;
        WardMeBruh.Log.Debug($"[Registry] Registered/Updated Ward {id} ('{name}'). Total Wards: {_knownWards.Count}");
    }

    public void Unregister(ZDOID id)
    {
        if (_knownWards.ContainsKey(id))
        {
            _knownWards.Remove(id);
            WardMeBruh.Log.Debug($"[Registry] Unregistered Ward {id}. Remaining Wards: {_knownWards.Count}");
        }
    }

    public KnownWard GetKnownWard(ZDOID id)
    {
        if (_knownWards.TryGetValue(id, out KnownWard ward))
            return ward;

        return null;
    }

    public List<KnownWard> GetAllWards()
    {
        return new List<KnownWard>(_knownWards.Values);
    }

    public KnownWard GetWardCoveringPoint(Vector3 point)
    {
        foreach (KnownWard ward in _knownWards.Values)
        {
            if (!ward.IsEnabled)
                continue;

            float dist = Utils.DistanceXZ(ward.Position, point);
            if (dist <= ward.Radius)
            {
                return ward;
            }
        }
        return null;
    }

    public List<KnownWard> GetPlayerWards(long playerId) => GetWardsForPlayer(playerId);

    public List<KnownWard> GetPlayerOwnedWards(long playerId)
    {
        List<KnownWard> result = new();
        if (playerId == 0L)
            return result;

        foreach (KnownWard ward in _knownWards.Values)
        {
            long creatorId = ward.CreatorId;
            if (creatorId == 0L && ZDOMan.instance != null)
            {
                ZDO zdo = ZDOMan.instance.GetZDO(ward.Id);
                if (zdo != null)
                {
                    creatorId = zdo.GetLong(ZDOVars.s_creator, 0L);
                    if (creatorId != ward.CreatorId)
                    {
                        RegisterOrUpdate(zdo);
                    }
                }
            }

            if (creatorId == playerId)
            {
                result.Add(ward);
            }
        }

        return result;
    }

    public List<KnownWard> GetWardsForPlayer(long playerId)
    {
        List<KnownWard> result = new();
        if (playerId == 0L)
            return result;

        foreach (KnownWard ward in _knownWards.Values)
        {
            if (ward.CreatorId == playerId)
            {
                result.Add(ward);
                continue;
            }

            if (ZDOMan.instance != null)
            {
                ZDO zdo = ZDOMan.instance.GetZDO(ward.Id);
                if (zdo != null && WardPermissions.HasRequiredRole(zdo, playerId, WardRole.Resident))
                {
                    result.Add(ward);
                }
            }
        }

        return result;
    }

    public bool ValidateWardPlacement(Vector3 newPos, float newRadius, long playerId, ZDOID currentWardId, out string failureReason)
    {
        float separationDistance = ConfigRegistry.WardSeparationDistance != null ? ConfigRegistry.WardSeparationDistance.Value : 50.0f;
        float maxAllowedRadius = ConfigRegistry.MaxWardRadius != null ? ConfigRegistry.MaxWardRadius.Value : 400.0f;

        if (newRadius > maxAllowedRadius)
        {
            failureReason = $"Ward radius ({newRadius:F0}m) exceeds the server maximum allowed ({maxAllowedRadius:F0}m).";
            return false;
        }

        foreach (KnownWard otherWard in _knownWards.Values)
        {
            if (otherWard.Id == currentWardId)
                continue;

            if (otherWard.CreatorId == playerId)
                continue;

            if (ZDOMan.instance != null)
            {
                ZDO otherZdo = ZDOMan.instance.GetZDO(otherWard.Id);
                if (otherZdo != null && WardPermissions.CanManageWard(otherZdo, playerId))
                    continue;
            }

            float distance = Utils.DistanceXZ(newPos, otherWard.Position);
            float minRequiredDistance = otherWard.Radius + newRadius + separationDistance;

            if (distance < minRequiredDistance)
            {
                failureReason = $"Encroaches on {otherWard.Name}'s territory. Distance is {distance:F0}m, but {minRequiredDistance:F0}m is required.";
                WardMeBruh.Log.Debug($"[Registry] Ward placement rejected for Player {playerId}: {failureReason}");
                return false;
            }
        }

        failureReason = string.Empty;
        return true;
    }

    public ZPackage Pack()
    {
        ZPackage pkg = new();
        List<KnownWard> allWards = GetAllWards();
        pkg.Write(allWards.Count);

        for (int i = 0; i < allWards.Count; i++)
        {
            allWards[i].Pack(pkg);
        }

        return pkg;
    }

    public void Unpack(ZPackage pkg)
    {
        if (pkg == null)
            return;

        int count = pkg.ReadInt();
        _knownWards.Clear();

        for (int i = 0; i < count; i++)
        {
            KnownWard ward = KnownWard.Unpack(pkg);
            _knownWards[ward.Id] = ward;
        }

        WardMeBruh.Log.Debug($"[Registry] Unpacked {count} wards from server into registry");
    }
}
