using System;
using System.Collections.Generic;
using UnityEngine;
using WardMeBruh.Configuration;
using WardMeBruh.Models;
using WardMeBruh.UI;

namespace WardMeBruh.Services;

public sealed class WardRegistryManager
{
    private static readonly Lazy<WardRegistryManager> _lazy = new(() => new WardRegistryManager());
    public static WardRegistryManager Instance => _lazy.Value;

    private readonly Dictionary<ZDOID, KnownWard> _knownWards = new();
    private readonly Dictionary<ZDOID, KnownExtensionWard> _extensionWards = new();

    public int Count => _knownWards.Count;
    public int ExtensionCount => _extensionWards.Count;

    private WardRegistryManager()
    {
    }

    public bool RegisterOrUpdate(ZDO zdo)
    {
        if (zdo == null || !zdo.IsValid())
            return false;

        ZDOID id = zdo.m_uid;
        string name = zdo.GetString(WardZdoKeys.CustomName, "Ward");
        Vector3 position = zdo.GetPosition();
        long creatorId = zdo.GetLong(ZDOVars.s_creator, 0L);
        string creatorName = zdo.GetString(ZDOVars.s_creatorName, "Unknown");
        float radius = Mathf.Clamp(zdo.GetFloat(WardZdoKeys.Radius, 30.0f), 1.0f, 100.0f);
        bool isEnabled = zdo.GetBool(ZDOVars.s_enabled, true);

        ZDOID parentId = zdo.GetZDOID(WardZdoKeys.ParentWard);
        WardType wardType = (WardType)zdo.GetInt(WardZdoKeys.WardType, (int)WardType.Main);
        if (wardType == WardType.Extension)
        {
            if (parentId == ZDOID.None)
            {
                List<KnownWard> allBases = GetLivingMainBases();
                KnownWard covering = FindCoveringMainBase(position, allBases);
                if (covering == null)
                {
                    for (int b = 0; b < allBases.Count; b++)
                    {
                        KnownWard baseWard = allBases[b];
                        if (Utils.DistanceXZ(baseWard.Position, position) <= baseWard.Radius + radius)
                        {
                            covering = baseWard;
                            break;
                        }
                    }
                }

                if (covering != null)
                {
                    parentId = covering.Id;
                    zdo.Set(WardZdoKeys.ParentWard, parentId);
                }
            }

            if (parentId != ZDOID.None)
            {
                if (_knownWards.ContainsKey(id))
                {
                    _knownWards.Remove(id);
                }

                bool hasExistingExt = _extensionWards.TryGetValue(id, out KnownExtensionWard existingExt);
                if (hasExistingExt &&
                    existingExt.ParentId == parentId &&
                    existingExt.Position == position &&
                    Mathf.Approximately(existingExt.Radius, radius) &&
                    existingExt.IsEnabled == isEnabled)
                {
                    return false;
                }

                KnownExtensionWard ext = new(id, parentId, position, radius, isEnabled);
                _extensionWards[id] = ext;

                if (ZDOMan.instance != null)
                {
                    ZDO parentZdo = ZDOMan.instance.GetZDO(parentId);
                    if (parentZdo != null && parentZdo.IsValid())
                    {
                        int parentMask = parentZdo.GetInt(WardZdoKeys.AttunedRunes, 0);
                        int parentProg = parentZdo.GetInt(WardZdoKeys.ProgressionLevel, 0);
                        if (parentMask != 0 && zdo.GetInt(WardZdoKeys.AttunedRunes, 0) != parentMask)
                        {
                            zdo.Set(WardZdoKeys.AttunedRunes, parentMask);
                            zdo.Set(WardZdoKeys.ProgressionLevel, parentProg);
                        }
                    }
                }

                WardMeBruh.Log.Debug($"[Registry] Registered Extension Ward {id} -> Parent Base: {parentId}. Total Extensions: {_extensionWards.Count}");
                return true;
            }
        }

        if (_extensionWards.ContainsKey(id))
        {
            _extensionWards.Remove(id);
        }

        bool hasExistingWard = _knownWards.TryGetValue(id, out KnownWard existingWard);
        if (hasExistingWard &&
            existingWard.Name == name &&
            existingWard.Position == position &&
            existingWard.CreatorId == creatorId &&
            existingWard.CreatorName == creatorName &&
            Mathf.Approximately(existingWard.Radius, radius) &&
            existingWard.IsEnabled == isEnabled &&
            existingWard.Type == wardType &&
            existingWard.ParentId == parentId)
        {
            return false;
        }

        KnownWard ward = new(id, name, position, creatorId, creatorName, radius, isEnabled, wardType, parentId);
        _knownWards[id] = ward;
        WardMeBruh.Log.Debug($"[Registry] Registered/Updated Ward {id} ('{name}', Type: {wardType}). Total Bases: {_knownWards.Count}");
        return true;
    }

    public void UpdateWardEnabled(ZDOID id, bool isEnabled)
    {
        if (_knownWards.TryGetValue(id, out KnownWard ward))
        {
            ward.IsEnabled = isEnabled;
        }

        if (_extensionWards.TryGetValue(id, out KnownExtensionWard ext))
        {
            ext.IsEnabled = isEnabled;
        }
    }

    public void Unregister(ZDOID id)
    {
        WardRoleManager.InvalidateWard(id);
        WardComfortService.InvalidateCache(id);

        if (_knownWards.Remove(id))
        {
            WardMeBruh.Log.Debug($"[Registry] Unregistered Ward {id}. Remaining Wards: {_knownWards.Count}");

            List<ZDOID> orphanedExtensions = null;
            foreach (KeyValuePair<ZDOID, KnownExtensionWard> kvp in _extensionWards)
            {
                if (kvp.Value.ParentId == id)
                {
                    orphanedExtensions ??= new List<ZDOID>();
                    orphanedExtensions.Add(kvp.Key);
                }
            }

            if (orphanedExtensions != null)
            {
                for (int i = 0; i < orphanedExtensions.Count; i++)
                {
                    ZDOID extId = orphanedExtensions[i];
                    _extensionWards.Remove(extId);
                    ZDO extZdo = (ZDOMan.instance != null) ? ZDOMan.instance.GetZDO(extId) : null;
                    if (extZdo != null && extZdo.IsValid())
                    {
                        extZdo.Set(WardZdoKeys.ParentWard, ZDOID.None);
                        RegisterOrUpdate(extZdo);
                    }
                }
            }
        }

        if (_extensionWards.Remove(id))
        {
            WardMeBruh.Log.Debug($"[Registry] Unregistered Extension Ward {id}. Remaining Extensions: {_extensionWards.Count}");
        }

        if (WardMainWindow.HasInstanceAndOpen)
        {
            if (WardMainWindow.Instance.ActiveWardId == id)
            {
                WardMainWindow.Instance.Close();
            }
            else
            {
                WardMainWindow.Instance.RefreshRemoteSelector();
            }
        }
    }

    public void PruneDeadWards()
    {
        if (ZNet.instance == null || !ZNet.instance.IsServer() || ZDOMan.instance == null)
            return;

        List<ZDOID> toRemove = null;
        List<KnownWard> snapshot = new(_knownWards.Values);
        for (int i = 0; i < snapshot.Count; i++)
        {
            KnownWard ward = snapshot[i];
            ZDO zdo = ZDOMan.instance.GetZDO(ward.Id);
            if (zdo == null)
            {
                toRemove ??= new List<ZDOID>();
                toRemove.Add(ward.Id);
            }
        }

        if (toRemove != null)
        {
            for (int i = 0; i < toRemove.Count; i++)
            {
                Unregister(toRemove[i]);
            }
            Networking.WardNetworkRouter.BroadcastRegistry();
        }
    }

    public List<KnownExtensionWard> GetExtensionsForParent(ZDOID parentId)
    {
        List<KnownExtensionWard> result = new();
        List<KnownExtensionWard> snapshot = new(_extensionWards.Values);
        for (int i = 0; i < snapshot.Count; i++)
        {
            if (snapshot[i].ParentId == parentId)
            {
                result.Add(snapshot[i]);
            }
        }
        return result;
    }


    public KnownWard GetKnownWard(ZDOID id)
    {
        if (_knownWards.TryGetValue(id, out KnownWard ward))
            return ward;

        if (_extensionWards.TryGetValue(id, out KnownExtensionWard ext))
        {
            if (_knownWards.TryGetValue(ext.ParentId, out KnownWard parentWard))
            {
                return parentWard;
            }
        }

        return null;
    }

    public List<KnownWard> GetAllWards()
    {
        PruneDeadWards();
        return new List<KnownWard>(_knownWards.Values);
    }

    public KnownWard GetWardCoveringPoint(Vector3 point, bool requireEnabled = false)
    {
        KnownWard bestWard = null;
        foreach (KeyValuePair<ZDOID, KnownWard> kvp in _knownWards)
        {
            KnownWard ward = kvp.Value;
            if (ward == null)
                continue;

            if (requireEnabled && !ward.IsEnabled)
                continue;

            float dist = Utils.DistanceXZ(ward.Position, point);
            if (dist <= ward.Radius)
            {
                if (bestWard == null)
                {
                    bestWard = ward;
                }
                else if (ward.Type == WardType.SubWard && bestWard.Type != WardType.SubWard)
                {
                    bestWard = ward;
                }
                else if (ward.Radius < bestWard.Radius)
                {
                    bestWard = ward;
                }
            }
        }

        if (bestWard != null)
            return bestWard;

        foreach (KeyValuePair<ZDOID, KnownExtensionWard> kvp in _extensionWards)
        {
            KnownExtensionWard ext = kvp.Value;
            if (ext == null)
                continue;

            if (requireEnabled && !ext.IsEnabled)
                continue;

            float dist = Utils.DistanceXZ(ext.Position, point);
            if (dist <= ext.Radius)
            {
                if (_knownWards.TryGetValue(ext.ParentId, out KnownWard parentWard))
                {
                    return parentWard;
                }
            }
        }

        return null;
    }

    public bool IsBaseWardRegistered(ZDOID id)
    {
        if (_knownWards.Count == 0)
            return true;
        return _knownWards.TryGetValue(id, out KnownWard ward) && ward.Type == WardType.Main;
    }

    private void ValidateAndRepairChildWardParents()
    {
        List<KnownWard> allBases = null;

        List<KnownWard> knownSnapshot = new(_knownWards.Values);
        for (int i = 0; i < knownSnapshot.Count; i++)
        {
            KnownWard ward = knownSnapshot[i];
            if (ward.Type == WardType.Extension)
            {
                allBases ??= GetLivingMainBases();
                KnownWard covering = FindCoveringMainBase(ward.Position, allBases);
                if (covering == null)
                {
                    for (int b = 0; b < allBases.Count; b++)
                    {
                        KnownWard baseWard = allBases[b];
                        if (Utils.DistanceXZ(baseWard.Position, ward.Position) <= baseWard.Radius + ward.Radius)
                        {
                            covering = baseWard;
                            break;
                        }
                    }
                }

                if (covering != null)
                {
                    ward.ParentId = covering.Id;
                    UpdateZdoParent(ward.Id, covering.Id);
                    _knownWards.Remove(ward.Id);
                    KnownExtensionWard ext = new(ward.Id, covering.Id, ward.Position, ward.Radius, ward.IsEnabled);
                    _extensionWards[ward.Id] = ext;

                    if (ZDOMan.instance != null)
                    {
                        ZDO parentZdo = ZDOMan.instance.GetZDO(covering.Id);
                        ZDO extZdo = ZDOMan.instance.GetZDO(ward.Id);
                        if (parentZdo != null && parentZdo.IsValid() && extZdo != null && extZdo.IsValid())
                        {
                            int parentMask = parentZdo.GetInt(WardZdoKeys.AttunedRunes, 0);
                            int parentProg = parentZdo.GetInt(WardZdoKeys.ProgressionLevel, 0);
                            extZdo.Set(WardZdoKeys.AttunedRunes, parentMask);
                            extZdo.Set(WardZdoKeys.ProgressionLevel, parentProg);
                        }
                    }
                    WardMeBruh.Log.Info($"[Registry] Repaired & linked Extension Ward {ward.Id} to Parent Base {covering.Id}");
                }
                continue;
            }

            if (ward.Type != WardType.SubWard)
                continue;

            bool parentValid = ward.ParentId != ZDOID.None && _knownWards.ContainsKey(ward.ParentId);
            if (!parentValid)
            {
                allBases ??= GetLivingMainBases();
                KnownWard covering = FindCoveringMainBase(ward.Position, allBases);
                if (covering != null)
                {
                    ward.ParentId = covering.Id;
                    UpdateZdoParent(ward.Id, covering.Id);
                }
                else if (ward.ParentId != ZDOID.None)
                {
                    ward.ParentId = ZDOID.None;
                    UpdateZdoParent(ward.Id, ZDOID.None);
                }
            }
        }

        List<KnownExtensionWard> extSnapshot = new(_extensionWards.Values);
        for (int i = 0; i < extSnapshot.Count; i++)
        {
            KnownExtensionWard ext = extSnapshot[i];
            bool parentValid = ext.ParentId != ZDOID.None && _knownWards.ContainsKey(ext.ParentId);
            if (!parentValid)
            {
                allBases ??= GetLivingMainBases();
                KnownWard covering = FindCoveringMainBase(ext.Position, allBases);
                if (covering == null)
                {
                    for (int b = 0; b < allBases.Count; b++)
                    {
                        KnownWard baseWard = allBases[b];
                        if (Utils.DistanceXZ(baseWard.Position, ext.Position) <= baseWard.Radius + ext.Radius)
                        {
                            covering = baseWard;
                            break;
                        }
                    }
                }

                if (covering != null)
                {
                    ext.ParentId = covering.Id;
                    UpdateZdoParent(ext.Id, covering.Id);
                }
                else if (ext.ParentId != ZDOID.None)
                {
                    ext.ParentId = ZDOID.None;
                    UpdateZdoParent(ext.Id, ZDOID.None);
                }
            }

            if (ext.ParentId != ZDOID.None && ZDOMan.instance != null)
            {
                ZDO parentZdo = ZDOMan.instance.GetZDO(ext.ParentId);
                ZDO extZdo = ZDOMan.instance.GetZDO(ext.Id);
                if (parentZdo != null && parentZdo.IsValid() && extZdo != null && extZdo.IsValid())
                {
                    int parentMask = parentZdo.GetInt(WardZdoKeys.AttunedRunes, 0);
                    int parentProg = parentZdo.GetInt(WardZdoKeys.ProgressionLevel, 0);
                    if (parentMask != 0 && extZdo.GetInt(WardZdoKeys.AttunedRunes, 0) != parentMask)
                    {
                        extZdo.Set(WardZdoKeys.AttunedRunes, parentMask);
                        extZdo.Set(WardZdoKeys.ProgressionLevel, parentProg);
                    }
                }
            }
        }
    }

    private List<KnownWard> GetLivingMainBases()
    {
        List<KnownWard> list = new();
        List<KnownWard> snapshot = new(_knownWards.Values);
        for (int i = 0; i < snapshot.Count; i++)
        {
            if (snapshot[i].Type == WardType.Main)
            {
                list.Add(snapshot[i]);
            }
        }
        return list;
    }

    private static KnownWard FindCoveringMainBase(Vector3 pos, List<KnownWard> bases)
    {
        KnownWard closest = null;
        float closestDist = float.MaxValue;
        for (int i = 0; i < bases.Count; i++)
        {
            KnownWard b = bases[i];
            float dist = Utils.DistanceXZ(b.Position, pos);
            if (dist <= b.Radius && dist < closestDist)
            {
                closest = b;
                closestDist = dist;
            }
        }
        return closest;
    }

    private static void UpdateZdoParent(ZDOID wardId, ZDOID parentId)
    {
        if (ZDOMan.instance != null)
        {
            ZDO zdo = ZDOMan.instance.GetZDO(wardId);
            if (zdo != null && zdo.IsValid())
            {
                zdo.Set(WardZdoKeys.ParentWard, parentId);
            }
        }
    }

    public List<KnownWard> GetPlayerWards(long playerId) => GetWardsForPlayer(playerId);

    public List<KnownWard> GetPlayerOwnedWards(long playerId)
    {
        PruneDeadWards();
        ValidateAndRepairChildWardParents();
        List<KnownWard> result = new();
        if (playerId == 0L)
            return result;

        if (PrivateArea.m_allAreas != null)
        {
            for (int i = 0; i < PrivateArea.m_allAreas.Count; i++)
            {
                PrivateArea area = PrivateArea.m_allAreas[i];
                if (area != null && area.m_nview != null && area.m_nview.IsValid())
                {
                    ZDO z = area.m_nview.GetZDO();
                    if (z != null && !_knownWards.ContainsKey(z.m_uid) && !_extensionWards.ContainsKey(z.m_uid))
                    {
                        RegisterOrUpdate(z);
                    }
                }
            }
        }

        List<KnownWard> snapshot = new(_knownWards.Values);
        for (int i = 0; i < snapshot.Count; i++)
        {
            KnownWard ward = snapshot[i];
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

        List<KnownExtensionWard> extSnapshot = new(_extensionWards.Values);
        for (int i = 0; i < extSnapshot.Count; i++)
        {
            KnownExtensionWard ext = extSnapshot[i];
            KnownWard parent = null;
            if (ext.ParentId != ZDOID.None)
            {
                _knownWards.TryGetValue(ext.ParentId, out parent);
            }

            long parentCreatorId = parent != null ? parent.CreatorId : 0L;
            string parentName = parent != null ? parent.Name : "Ward";
            string parentCreatorName = parent != null ? parent.CreatorName : "Unknown";

            ZDO extZdo = (ZDOMan.instance != null) ? ZDOMan.instance.GetZDO(ext.Id) : null;
            long extCreatorId = extZdo != null ? extZdo.GetLong(ZDOVars.s_creator, 0L) : 0L;
            string extName = extZdo != null ? extZdo.GetString(WardZdoKeys.CustomName, null) : null;
            string extCreatorName = extZdo != null ? extZdo.GetString(ZDOVars.s_creatorName, null) : null;

            if (parentCreatorId == 0L && ext.ParentId != ZDOID.None && ZDOMan.instance != null)
            {
                ZDO parentZdo = ZDOMan.instance.GetZDO(ext.ParentId);
                if (parentZdo != null)
                {
                    parentCreatorId = parentZdo.GetLong(ZDOVars.s_creator, 0L);
                    parentName = parentZdo.GetString(WardZdoKeys.CustomName, parentName);
                    parentCreatorName = parentZdo.GetString(ZDOVars.s_creatorName, parentCreatorName);
                }
            }

            bool isOwned = (extCreatorId != 0L && extCreatorId == playerId) ||
                           (parentCreatorId != 0L && parentCreatorId == playerId);

            if (isOwned)
            {
                string displayName = !string.IsNullOrEmpty(extName)
                    ? extName
                    : $"{parentName} {"$wmb_ward_type_extension".Localize()}";
                string creatorName = !string.IsNullOrEmpty(extCreatorName)
                    ? extCreatorName
                    : parentCreatorName;
                long creatorId = extCreatorId != 0L ? extCreatorId : parentCreatorId;

                result.Add(new KnownWard(ext.Id, displayName, ext.Position, creatorId, creatorName, ext.Radius, ext.IsEnabled, WardType.Extension, ext.ParentId));
            }
        }

        HashSet<ZDOID> existingIds = new();
        for (int i = 0; i < result.Count; i++)
        {
            existingIds.Add(result[i].Id);
        }

        List<ZDOID> missingParentIds = null;
        for (int i = 0; i < result.Count; i++)
        {
            KnownWard w = result[i];
            if (w.ParentId != ZDOID.None && !existingIds.Contains(w.ParentId))
            {
                missingParentIds ??= new List<ZDOID>();
                if (!missingParentIds.Contains(w.ParentId))
                {
                    missingParentIds.Add(w.ParentId);
                }
            }
        }

        if (missingParentIds != null)
        {
            for (int i = 0; i < missingParentIds.Count; i++)
            {
                ZDOID parentId = missingParentIds[i];
                if (_knownWards.TryGetValue(parentId, out KnownWard parent))
                {
                    result.Add(parent);
                    existingIds.Add(parentId);
                }
            }
        }

        return result;
    }

    public List<KnownWard> GetWardsForPlayer(long playerId)
    {
        PruneDeadWards();
        ValidateAndRepairChildWardParents();
        List<KnownWard> result = new();
        if (playerId == 0L)
            return result;

        List<KnownWard> snapshot = new(_knownWards.Values);
        for (int i = 0; i < snapshot.Count; i++)
        {
            KnownWard ward = snapshot[i];
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

        List<KnownExtensionWard> extSnapshot = new(_extensionWards.Values);
        for (int i = 0; i < extSnapshot.Count; i++)
        {
            KnownExtensionWard ext = extSnapshot[i];
            KnownWard parent = null;
            if (ext.ParentId != ZDOID.None)
            {
                _knownWards.TryGetValue(ext.ParentId, out parent);
            }

            long parentCreatorId = parent != null ? parent.CreatorId : 0L;
            string parentName = parent != null ? parent.Name : "Ward";
            string parentCreatorName = parent != null ? parent.CreatorName : "Unknown";

            ZDO extZdo = (ZDOMan.instance != null) ? ZDOMan.instance.GetZDO(ext.Id) : null;
            long extCreatorId = extZdo != null ? extZdo.GetLong(ZDOVars.s_creator, 0L) : 0L;
            string extName = extZdo != null ? extZdo.GetString(WardZdoKeys.CustomName, null) : null;
            string extCreatorName = extZdo != null ? extZdo.GetString(ZDOVars.s_creatorName, null) : null;

            if (parentCreatorId == 0L && ext.ParentId != ZDOID.None && ZDOMan.instance != null)
            {
                ZDO parentZdo = ZDOMan.instance.GetZDO(ext.ParentId);
                if (parentZdo != null)
                {
                    parentCreatorId = parentZdo.GetLong(ZDOVars.s_creator, 0L);
                    parentName = parentZdo.GetString(WardZdoKeys.CustomName, parentName);
                    parentCreatorName = parentZdo.GetString(ZDOVars.s_creatorName, parentCreatorName);
                }
            }

            bool isAccessible = (extCreatorId != 0L && extCreatorId == playerId) ||
                                (parentCreatorId != 0L && parentCreatorId == playerId);

            if (!isAccessible && ZDOMan.instance != null && ext.ParentId != ZDOID.None)
            {
                ZDO parentZdo = ZDOMan.instance.GetZDO(ext.ParentId);
                if (parentZdo != null && WardPermissions.HasRequiredRole(parentZdo, playerId, WardRole.Resident))
                {
                    isAccessible = true;
                }
            }

            if (isAccessible)
            {
                string displayName = !string.IsNullOrEmpty(extName)
                    ? extName
                    : $"{parentName} {"$wmb_ward_type_extension".Localize()}";
                string creatorName = !string.IsNullOrEmpty(extCreatorName)
                    ? extCreatorName
                    : parentCreatorName;
                long creatorId = extCreatorId != 0L ? extCreatorId : parentCreatorId;

                result.Add(new KnownWard(ext.Id, displayName, ext.Position, creatorId, creatorName, ext.Radius, ext.IsEnabled, WardType.Extension, ext.ParentId));
            }
        }

        HashSet<ZDOID> existingIds = new();
        for (int i = 0; i < result.Count; i++)
        {
            existingIds.Add(result[i].Id);
        }

        List<ZDOID> missingParentIds = null;
        for (int i = 0; i < result.Count; i++)
        {
            KnownWard w = result[i];
            if (w.ParentId != ZDOID.None && !existingIds.Contains(w.ParentId))
            {
                missingParentIds ??= new List<ZDOID>();
                if (!missingParentIds.Contains(w.ParentId))
                {
                    missingParentIds.Add(w.ParentId);
                }
            }
        }

        if (missingParentIds != null)
        {
            for (int i = 0; i < missingParentIds.Count; i++)
            {
                ZDOID parentId = missingParentIds[i];
                if (_knownWards.TryGetValue(parentId, out KnownWard parent))
                {
                    result.Add(parent);
                    existingIds.Add(parentId);
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

        KnownWard covering = GetWardCoveringPoint(newPos);
        if (covering != null)
        {
            if (covering.CreatorId == playerId)
            {
                failureReason = string.Empty;
                return true;
            }
            if (ZDOMan.instance != null)
            {
                ZDO coveringZdo = ZDOMan.instance.GetZDO(covering.Id);
                if (coveringZdo != null && WardPermissions.CanManageWard(coveringZdo, playerId))
                {
                    failureReason = string.Empty;
                    return true;
                }
            }
        }

        List<KnownWard> snapshot = new(_knownWards.Values);
        for (int i = 0; i < snapshot.Count; i++)
        {
            KnownWard otherWard = snapshot[i];
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

    public float GetMaxAllowableSpatialRadius(Vector3 pos, ZDOID currentWardId, long playerId)
    {
        float separationDistance = ConfigRegistry.WardSeparationDistance != null ? ConfigRegistry.WardSeparationDistance.Value : 50.0f;
        float maxAllowedRadius = ConfigRegistry.MaxWardRadius != null ? ConfigRegistry.MaxWardRadius.Value : 100.0f;

        List<KnownWard> snapshot = new(_knownWards.Values);
        for (int i = 0; i < snapshot.Count; i++)
        {
            KnownWard otherWard = snapshot[i];
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

            float distance = Utils.DistanceXZ(pos, otherWard.Position);
            float availableForRadius = distance - otherWard.Radius - separationDistance;
            if (availableForRadius < maxAllowedRadius)
            {
                maxAllowedRadius = availableForRadius;
            }
        }

        return Mathf.Max(10.0f, maxAllowedRadius);
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

        List<KnownExtensionWard> extSnapshot = new(_extensionWards.Values);
        pkg.Write(extSnapshot.Count);
        for (int i = 0; i < extSnapshot.Count; i++)
        {
            extSnapshot[i].Pack(pkg);
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

        _extensionWards.Clear();
        if (pkg.GetArray() != null && pkg.Size() > pkg.GetPos())
        {
            int extCount = pkg.ReadInt();
            for (int i = 0; i < extCount; i++)
            {
                KnownExtensionWard ext = KnownExtensionWard.Unpack(pkg);
                _extensionWards[ext.Id] = ext;
            }
        }

        WardMeBruh.Log.Debug($"[Registry] Unpacked {count} wards and {_extensionWards.Count} extensions from server into registry");
    }
}
