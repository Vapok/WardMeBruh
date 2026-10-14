using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using WardMeBruh.API;
using WardMeBruh.Configuration;
using WardMeBruh.Models;
using WardMeBruh.UI;

namespace WardMeBruh.Services;

public static class WardRoleManager
{
    private const char RoleDelimiter = '|';
    private const char FieldDelimiter = ':';
    private const char PermDelimiter = ',';

    private class CachedRoleDefinitions
    {
        public string RawString;
        public List<WardRoleDefinition> Roles;
    }

    private class CachedRoleAssignments
    {
        public string RawString;
        public WardRoleAssignments Assignments;
    }

    private static readonly Dictionary<string, FeaturePermission> _registeredPermissions = new(StringComparer.OrdinalIgnoreCase);
    private static readonly List<string> _orderedPermissionIds = new();
    private static readonly Dictionary<ZDOID, CachedRoleDefinitions> _cachedDefinitions = new();
    private static readonly Dictionary<ZDOID, CachedRoleAssignments> _cachedAssignments = new();
    private static bool _initialized;

    public static void Initialize()
    {
        if (_initialized)
            return;

        _initialized = true;
        RegisterDefaultPermissions();
    }

    public static void RegisterPermission(FeaturePermission permission)
    {
        if (permission == null || string.IsNullOrEmpty(permission.Id))
            return;

        _registeredPermissions[permission.Id] = permission;
        if (!_orderedPermissionIds.Contains(permission.Id))
        {
            _orderedPermissionIds.Add(permission.Id);
        }
    }

    public static IReadOnlyCollection<FeaturePermission> GetAllPermissions()
    {
        EnsureInitialized();
        return _registeredPermissions.Values;
    }

    public static IReadOnlyCollection<string> GetAllPermissionIds()
    {
        EnsureInitialized();
        return _registeredPermissions.Keys;
    }

    public static List<FeaturePermission> GetPermissionsByCategory(WardCapabilityCategory category)
    {
        EnsureInitialized();
        List<FeaturePermission> result = new();
        foreach (KeyValuePair<string, FeaturePermission> kvp in _registeredPermissions)
        {
            if (kvp.Value.Category == category)
            {
                result.Add(kvp.Value);
            }
        }
        result.Sort((a, b) => a.SortOrder.CompareTo(b.SortOrder));
        return result;
    }

    public static List<WardRoleDefinition> CreateDefaultRoleDefinitions()
    {
        EnsureInitialized();

        WardRoleDefinition guest = new WardRoleDefinition((int)WardRole.Guest, "$wmb_role_guest", 0, WardColorPalette.ColorHex(WardColorPalette.TextMuted));
        WardRoleDefinition resident = new WardRoleDefinition((int)WardRole.Resident, "$wmb_role_resident", 1, WardColorPalette.ColorHex(WardColorPalette.ValheimGreen));
        WardRoleDefinition manager = new WardRoleDefinition((int)WardRole.Manager, "$wmb_role_manager", 2, WardColorPalette.ColorHex(WardColorPalette.IceBlue));
        WardRoleDefinition owner = new WardRoleDefinition((int)WardRole.Owner, "$wmb_role_owner", 3, WardColorPalette.ColorHex(WardColorPalette.GoldAmber));

        foreach (KeyValuePair<string, FeaturePermission> kvp in _registeredPermissions)
        {
            FeaturePermission perm = kvp.Value;
            if (perm.DefaultRole == WardRole.Guest)
            {
                guest.AddPermission(perm.Id);
            }
            else if (perm.DefaultRole == WardRole.Resident)
            {
                resident.AddPermission(perm.Id);
            }
            else if (perm.DefaultRole == WardRole.Manager)
            {
                manager.AddPermission(perm.Id);
            }
            else if (perm.DefaultRole == WardRole.Owner)
            {
                owner.AddPermission(perm.Id);
            }
        }

        List<WardRoleDefinition> roles = new() { guest, resident, manager, owner };
        for (int i = 0; i < _registeredRoleTemplates.Count; i++)
        {
            WardRoleDefinition tmpl = _registeredRoleTemplates[i];
            WardRoleDefinition clone = new(tmpl.Id, tmpl.Name, tmpl.Level, tmpl.ColorHex);
            foreach (string p in tmpl.ExplicitPermissions)
            {
                clone.AddPermission(p);
            }
            roles.Add(clone);
        }

        roles.Sort((a, b) => a.Level.CompareTo(b.Level));
        return roles;
    }

    private static readonly List<WardRoleDefinition> _registeredRoleTemplates = new();

    public static void RegisterRoleTemplate(WardRoleDefinition roleDef)
    {
        if (roleDef == null || string.IsNullOrEmpty(roleDef.Name))
            return;

        for (int i = 0; i < _registeredRoleTemplates.Count; i++)
        {
            if (_registeredRoleTemplates[i].Id == roleDef.Id || _registeredRoleTemplates[i].Name.Equals(roleDef.Name, StringComparison.OrdinalIgnoreCase))
            {
                _registeredRoleTemplates[i] = roleDef;
                return;
            }
        }
        _registeredRoleTemplates.Add(roleDef);
    }

    public static List<WardRoleDefinition> GetRoleDefinitions(ZDO wardZdo)
    {
        EnsureInitialized();

        if (wardZdo == null)
            return CreateDefaultRoleDefinitions();

        wardZdo = WardQueryService.GetEffectiveWardZdo(wardZdo);
        if (wardZdo == null)
            return CreateDefaultRoleDefinitions();

        string data = wardZdo.GetString(WardZdoKeys.RoleDefinitions, string.Empty);
        if (_cachedDefinitions.TryGetValue(wardZdo.m_uid, out CachedRoleDefinitions cached) &&
            (ReferenceEquals(cached.RawString, data) || string.Equals(cached.RawString, data, StringComparison.Ordinal)))
        {
            if (EnsureRoleDefaultsForUnlockedFeatures(wardZdo, cached.Roles))
            {
                SaveRoleDefinitions(wardZdo, cached.Roles);
            }
            return cached.Roles;
        }

        if (string.IsNullOrEmpty(data))
        {
            List<WardRoleDefinition> defaultRoles = CreateDefaultRoleDefinitions();
            EnsureRoleDefaultsForUnlockedFeatures(wardZdo, defaultRoles);
            SaveRoleDefinitions(wardZdo, defaultRoles);
            return defaultRoles;
        }

        bool modified = false;
        List<WardRoleDefinition> roles = DeserializeRoles(data);
        if (roles.Count == 0)
        {
            roles = CreateDefaultRoleDefinitions();
            modified = true;
        }
        else
        {

            for (int i = 0; i < roles.Count; i++)
            {
                if (roles[i].Id == (int)WardRole.Owner)
                {
                    foreach (string permId in _registeredPermissions.Keys)
                    {
                        if (!roles[i].HasExplicitPermission(permId))
                        {
                            roles[i].AddPermission(permId);
                            modified = true;
                        }
                    }
                    break;
                }
            }

            for (int i = 0; i < roles.Count; i++)
            {
                if (roles[i].Id == (int)WardRole.Guest && (roles[i].Name == "Guest" || roles[i].Name == "$wmb_role_guest"))
                {
                    if (roles[i].Name != "$wmb_role_guest")
                    {
                        roles[i].Name = "$wmb_role_guest";
                        modified = true;
                    }
                }
            }

            WardRoleDefinition residentRole = null;
            WardRoleDefinition managerRole = null;
            for (int i = 0; i < roles.Count; i++)
            {
                if (roles[i].Id == (int)WardRole.Resident)
                    residentRole = roles[i];
                else if (roles[i].Id == (int)WardRole.Manager)
                    managerRole = roles[i];
            }

            if (residentRole != null && managerRole != null)
            {
                if (residentRole.HasExplicitPermission(WardPermissionIds.AccessDoors))
                {
                    residentRole.RemovePermission(WardPermissionIds.AccessDoors);
                    if (!managerRole.HasExplicitPermission(WardPermissionIds.AccessDoors))
                    {
                        managerRole.AddPermission(WardPermissionIds.AccessDoors);
                    }
                    modified = true;
                }

                if (residentRole.HasExplicitPermission(WardPermissionIds.AccessChests))
                {
                    residentRole.RemovePermission(WardPermissionIds.AccessChests);
                    if (!managerRole.HasExplicitPermission(WardPermissionIds.AccessChests))
                    {
                        managerRole.AddPermission(WardPermissionIds.AccessChests);
                    }
                    modified = true;
                }
            }

            for (int i = 0; i < roles.Count; i++)
            {
                WardRoleDefinition role = roles[i];
                if (role.Id == (int)WardRole.Owner)
                    continue;

                List<string> toRemove = null;
                foreach (string permId in role.ExplicitPermissions)
                {
                    for (int j = 0; j < roles.Count; j++)
                    {
                        if (roles[j].Level < role.Level && roles[j].HasExplicitPermission(permId))
                        {
                            toRemove ??= new List<string>();
                            toRemove.Add(permId);
                            break;
                        }
                    }
                }

                if (toRemove != null && toRemove.Count > 0)
                {
                    for (int r = 0; r < toRemove.Count; r++)
                    {
                        role.RemovePermission(toRemove[r]);
                    }
                    modified = true;
                }
            }

            for (int i = 0; i < _registeredRoleTemplates.Count; i++)
            {
                WardRoleDefinition tmpl = _registeredRoleTemplates[i];
                bool roleExists = false;
                for (int j = 0; j < roles.Count; j++)
                {
                    if (roles[j].Id == tmpl.Id || roles[j].Name.Equals(tmpl.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        roleExists = true;
                        break;
                    }
                }
                if (!roleExists)
                {
                    WardRoleDefinition clone = new(tmpl.Id, tmpl.Name, tmpl.Level, tmpl.ColorHex);
                    foreach (string p in tmpl.ExplicitPermissions)
                    {
                        clone.AddPermission(p);
                    }
                    roles.Add(clone);
                    modified = true;
                }
            }
        }

        if (EnsureRoleDefaultsForUnlockedFeatures(wardZdo, roles))
        {
            modified = true;
        }

        roles.Sort((a, b) => a.Level.CompareTo(b.Level));

        if (modified)
        {
            SaveRoleDefinitions(wardZdo, roles);
        }
        else
        {
            _cachedDefinitions[wardZdo.m_uid] = new CachedRoleDefinitions
            {
                RawString = data,
                Roles = roles
            };
        }

        return roles;
    }

    public static bool EnsureRoleDefaultsForUnlockedFeatures(ZDO wardZdo, List<WardRoleDefinition> roles)
    {
        if (wardZdo == null || roles == null || roles.Count == 0)
            return false;

        wardZdo = WardQueryService.GetEffectiveWardZdo(wardZdo);
        if (wardZdo == null)
            return false;

        EnsureInitialized();

        long establishedMask = wardZdo.GetLong(WardZdoKeys.EstablishedRoleDefaults, 0L);
        long originalMask = establishedMask;
        bool modified = false;

        WardRoleDefinition guestRole = null;
        WardRoleDefinition residentRole = null;
        WardRoleDefinition managerRole = null;
        WardRoleDefinition ownerRole = null;

        for (int i = 0; i < roles.Count; i++)
        {
            if (roles[i].Id == (int)WardRole.Guest)
                guestRole = roles[i];
            else if (roles[i].Id == (int)WardRole.Resident)
                residentRole = roles[i];
            else if (roles[i].Id == (int)WardRole.Manager)
                managerRole = roles[i];
            else if (roles[i].Id == (int)WardRole.Owner)
                ownerRole = roles[i];
        }

        bool progressionEnabled = ConfigRegistry.EnableProgressionSystem.Value;

        foreach (KeyValuePair<string, FeaturePermission> kvp in _registeredPermissions)
        {
            FeaturePermission perm = kvp.Value;
            int permIndex = _orderedPermissionIds.IndexOf(perm.Id);
            long bit = (permIndex >= 0 && permIndex < 64) ? (1L << permIndex) : 0L;

            bool isUnlocked = IsPermissionFeatureUnlocked(wardZdo, perm.Id);

            if (isUnlocked)
            {
                if (bit != 0L && (establishedMask & bit) == 0L)
                {
                    WardRoleDefinition targetRole = perm.DefaultRole switch
                    {
                        WardRole.Guest => guestRole,
                        WardRole.Resident => residentRole,
                        WardRole.Manager => managerRole,
                        WardRole.Owner => ownerRole,
                        _ => null
                    };

                    if (targetRole != null && !targetRole.HasExplicitPermission(perm.Id))
                    {
                        bool alreadyInherited = false;
                        for (int j = 0; j < roles.Count; j++)
                        {
                            if (roles[j].Level < targetRole.Level && roles[j].HasExplicitPermission(perm.Id))
                            {
                                alreadyInherited = true;
                                break;
                            }
                        }

                        if (!alreadyInherited)
                        {
                            targetRole.AddPermission(perm.Id);
                            modified = true;
                        }
                    }

                    if (ownerRole != null && !ownerRole.HasExplicitPermission(perm.Id))
                    {
                        ownerRole.AddPermission(perm.Id);
                        modified = true;
                    }

                    establishedMask |= bit;
                }
            }
            else if (progressionEnabled)
            {
                for (int i = 0; i < roles.Count; i++)
                {
                    if (roles[i].Id != (int)WardRole.Owner && roles[i].HasExplicitPermission(perm.Id))
                    {
                        roles[i].RemovePermission(perm.Id);
                        modified = true;
                    }
                }

                if (bit != 0L && (establishedMask & bit) != 0L)
                {
                    establishedMask &= ~bit;
                }
            }
        }

        if (establishedMask != originalMask)
        {
            wardZdo.Set(WardZdoKeys.EstablishedRoleDefaults, establishedMask);
        }

        return modified;
    }

    public static void SaveRoleDefinitions(ZDO wardZdo, List<WardRoleDefinition> roles)
    {
        if (wardZdo == null || roles == null)
            return;

        wardZdo = WardQueryService.GetEffectiveWardZdo(wardZdo);
        if (wardZdo == null)
            return;

        string serialized = SerializeRoles(roles);
        wardZdo.Set(WardZdoKeys.RoleDefinitions, serialized);
        _cachedDefinitions[wardZdo.m_uid] = new CachedRoleDefinitions
        {
            RawString = serialized,
            Roles = roles
        };
    }

    public static WardRoleAssignments GetRoleAssignments(ZDO wardZdo)
    {
        if (wardZdo == null)
            return new WardRoleAssignments();

        wardZdo = WardQueryService.GetEffectiveWardZdo(wardZdo);
        if (wardZdo == null)
            return new WardRoleAssignments();

        string data = wardZdo.GetString(WardZdoKeys.RoleAssignments, string.Empty);
        if (_cachedAssignments.TryGetValue(wardZdo.m_uid, out CachedRoleAssignments cached) &&
            (ReferenceEquals(cached.RawString, data) || string.Equals(cached.RawString, data, StringComparison.Ordinal)))
        {
            return cached.Assignments;
        }

        if (!string.IsNullOrEmpty(data))
        {
            WardRoleAssignments deserialized = WardRoleAssignments.Deserialize(data);
            _cachedAssignments[wardZdo.m_uid] = new CachedRoleAssignments
            {
                RawString = data,
                Assignments = deserialized
            };
            return deserialized;
        }

        WardRoleAssignments assignments = new();
        string mgrData = wardZdo.GetString(WardZdoKeys.RolesManagers, string.Empty);
        WardRoster mgrRoster = !string.IsNullOrEmpty(mgrData) ? WardRoster.Deserialize(mgrData) : null;
        string resData = wardZdo.GetString(WardZdoKeys.RolesResidents, string.Empty);
        WardRoster resRoster = !string.IsNullOrEmpty(resData) ? WardRoster.Deserialize(resData) : null;

        if ((mgrRoster != null && mgrRoster.Entries.Count > 0) || (resRoster != null && resRoster.Entries.Count > 0))
        {
            assignments.SyncFromLegacy(mgrRoster, resRoster);
            SaveRoleAssignments(wardZdo, assignments);
        }
        else
        {
            _cachedAssignments[wardZdo.m_uid] = new CachedRoleAssignments
            {
                RawString = data,
                Assignments = assignments
            };
        }

        return assignments;
    }

    public static void SaveRoleAssignments(ZDO wardZdo, WardRoleAssignments assignments)
    {
        if (wardZdo == null || assignments == null)
            return;

        wardZdo = WardQueryService.GetEffectiveWardZdo(wardZdo);
        if (wardZdo == null)
            return;

        string serialized = assignments.Serialize();
        wardZdo.Set(WardZdoKeys.RoleAssignments, serialized);

        assignments.SyncToLegacy(out WardRoster managers, out WardRoster residents);
        wardZdo.Set(WardZdoKeys.RolesManagers, managers.Serialize());
        wardZdo.Set(WardZdoKeys.RolesResidents, residents.Serialize());

        _cachedAssignments[wardZdo.m_uid] = new CachedRoleAssignments
        {
            RawString = serialized,
            Assignments = assignments
        };
    }

    public static void InvalidateWard(ZDOID wardId)
    {
        _cachedDefinitions.Remove(wardId);
        _cachedAssignments.Remove(wardId);
    }

    public static void ClearCaches()
    {
        _cachedDefinitions.Clear();
        _cachedAssignments.Clear();
    }

    public static int GetPlayerRoleId(ZDO wardZdo, long playerId)
    {
        if (wardZdo == null || playerId == 0L)
            return (int)WardRole.Guest;

        if (WardPermissions.IsAdmin(playerId))
            return (int)WardRole.Owner;

        wardZdo = WardQueryService.GetEffectiveWardZdo(wardZdo);
        if (wardZdo == null)
            return (int)WardRole.Guest;

        long creatorId = wardZdo.GetLong(ZDOVars.s_creator, 0L);
        if (creatorId != 0L && creatorId == playerId)
            return (int)WardRole.Owner;

        WardRoleAssignments assignments = GetRoleAssignments(wardZdo);
        return assignments.GetRoleId(playerId, (int)WardRole.Guest);
    }

    public static bool HasPermission(List<WardRoleDefinition> roles, int roleId, string permissionId)
    {
        if (roles == null || roles.Count == 0 || string.IsNullOrEmpty(permissionId))
            return false;

        WardRoleDefinition targetRole = null;
        for (int i = 0; i < roles.Count; i++)
        {
            if (roles[i].Id == roleId)
            {
                targetRole = roles[i];
                break;
            }
        }

        if (targetRole == null)
            return false;

        if (targetRole.Id == (int)WardRole.Owner || targetRole.Level >= roles.Count - 1)
            return true;

        if (targetRole.HasExplicitPermission(permissionId))
            return true;

        for (int i = 0; i < roles.Count; i++)
        {
            if (roles[i].Level < targetRole.Level && roles[i].HasExplicitPermission(permissionId))
                return true;
        }

        return false;
    }

    public static bool IsPermissionInherited(List<WardRoleDefinition> roles, int roleId, string permissionId, out string inheritedFromRoleName)
    {
        inheritedFromRoleName = string.Empty;

        if (roles == null || roles.Count == 0 || string.IsNullOrEmpty(permissionId))
            return false;

        WardRoleDefinition targetRole = null;
        for (int i = 0; i < roles.Count; i++)
        {
            if (roles[i].Id == roleId)
            {
                targetRole = roles[i];
                break;
            }
        }

        if (targetRole == null)
            return false;

        if (targetRole.Id == (int)WardRole.Owner || targetRole.Level >= roles.Count - 1)
        {
            inheritedFromRoleName = "$wmb_mgmt_owner_authority";
            return true;
        }

        for (int i = 0; i < roles.Count; i++)
        {
            WardRoleDefinition otherRole = roles[i];
            if (otherRole.Level < targetRole.Level && otherRole.HasExplicitPermission(permissionId))
            {
                inheritedFromRoleName = otherRole.Name;
                return true;
            }
        }

        return false;
    }

    public static bool CanPerform(ZDO wardZdo, long playerId, string permissionId)
    {
        if (wardZdo == null)
            return false;

        wardZdo = WardQueryService.GetEffectiveWardZdo(wardZdo);
        if (wardZdo == null)
            return false;

        if (!IsPermissionFeatureUnlocked(wardZdo, permissionId, playerId))
            return false;

        if (WardPermissions.IsAdmin(playerId))
            return true;

        int roleId = GetPlayerRoleId(wardZdo, playerId);
        if (roleId == (int)WardRole.Owner)
            return true;

        WardType wardType = (WardType)wardZdo.GetInt(WardZdoKeys.WardType, (int)WardType.Main);
        if (wardType == WardType.SubWard)
        {
            ZDOID parentId = wardZdo.GetZDOID(WardZdoKeys.ParentWard);
            if (parentId != ZDOID.None)
            {
                ZDO parentZdo = (ZDOMan.instance != null) ? ZDOMan.instance.GetZDO(parentId) : null;
                if (parentZdo != null && parentZdo.IsValid())
                {
                    int parentRoleId = GetPlayerRoleId(parentZdo, playerId);
                    if (parentRoleId == (int)WardRole.Owner)
                        return true;
                }
            }
        }

        List<WardRoleDefinition> roles = GetRoleDefinitions(wardZdo);
        return HasPermission(roles, roleId, permissionId);
    }

    public static bool IsPermissionFeatureUnlocked(ZDO wardZdo, string permissionId, long playerId = 0)
    {
        if (!ConfigRegistry.EnableProgressionSystem.Value)
            return true;

        if (wardZdo == null || !wardZdo.IsValid())
            return false;

        switch (permissionId)
        {
            case WardPermissionIds.CoreDomeAccess:
            case WardPermissionIds.CoreBuilding:
            case WardPermissionIds.CoreViewGui:
            case WardPermissionIds.CorePower:
            case WardPermissionIds.CoreRename:
            case WardPermissionIds.AccessDoors:
            case WardPermissionIds.AccessChests:
            case WardPermissionIds.CoreExtendWard:
            case WardPermissionIds.CoreSubWard:
            case WardPermissionIds.CoreAttuneWard:
            case WardPermissionIds.PersonnelRoster:
            case WardPermissionIds.PersonnelGovernance:
                return true;

            case WardPermissionIds.CoreBenefits:
                return HasAnyPerkUnlocked(wardZdo, playerId);

            case WardPermissionIds.CoreFarming:
                return WardProgressionService.IsFeatureUnlocked(wardZdo, WardFeature.ProtectCrops, playerId);

            case WardPermissionIds.CoreHarvest:
                return WardProgressionService.IsFeatureUnlocked(wardZdo, WardFeature.PickableHarvestProtection, playerId);

            case WardPermissionIds.AccessPortals:
                return WardProgressionService.IsFeatureUnlocked(wardZdo, WardFeature.PortalAccess, playerId);

            case WardPermissionIds.DefensesDomeSize:
                return true;

            case WardPermissionIds.DefensesCombat:
                return WardProgressionService.IsFeatureUnlocked(wardZdo, WardFeature.PushbackForce, playerId) ||
                       WardProgressionService.IsFeatureUnlocked(wardZdo, WardFeature.CreatureShock, playerId);

            case WardPermissionIds.DefensesCamouflage:
                return WardProgressionService.IsFeatureUnlocked(wardZdo, WardFeature.GhostBase, playerId);

            case WardPermissionIds.DefensesBiome:
                return WardProgressionService.IsFeatureUnlocked(wardZdo, WardFeature.WeatherRoof, playerId) ||
                       WardProgressionService.IsFeatureUnlocked(wardZdo, WardFeature.SnowShield, playerId) ||
                       WardProgressionService.IsFeatureUnlocked(wardZdo, WardFeature.AshShield, playerId) ||
                       WardProgressionService.IsFeatureUnlocked(wardZdo, WardFeature.ArcticHearth, playerId);

            case WardPermissionIds.DefensesTames:
                return WardProgressionService.IsFeatureUnlocked(wardZdo, WardFeature.ProtectTames, playerId);

            case WardPermissionIds.UpkeepRepair:
                return WardProgressionService.IsFeatureUnlocked(wardZdo, WardFeature.AreaRepair, playerId);

            case WardPermissionIds.UpkeepAutoFuel:
                return WardProgressionService.IsFeatureUnlocked(wardZdo, WardFeature.AutoFuel, playerId);

            case WardPermissionIds.UpkeepAutoDoors:
                return WardProgressionService.IsFeatureUnlocked(wardZdo, WardFeature.AutoCloseDoors, playerId);

            case WardPermissionIds.UpkeepPerks:
                return WardProgressionService.IsFeatureUnlocked(wardZdo, WardFeature.FoodFreeze, playerId) ||
                       WardProgressionService.IsFeatureUnlocked(wardZdo, WardFeature.MaxComfort, playerId) ||
                       WardProgressionService.IsFeatureUnlocked(wardZdo, WardFeature.RegenBoost, playerId) ||
                       WardProgressionService.IsFeatureUnlocked(wardZdo, WardFeature.DeathSkillShield, playerId);

            case WardPermissionIds.UpkeepWaterDrain:
                return WardProgressionService.IsFeatureUnlocked(wardZdo, WardFeature.WaterDrain, playerId);

            case WardPermissionIds.UpkeepSecurity:
                return WardProgressionService.IsFeatureUnlocked(wardZdo, WardFeature.PackIceShield, playerId) ||
                       WardProgressionService.IsFeatureUnlocked(wardZdo, WardFeature.ProtectTerrain, playerId) ||
                       WardProgressionService.IsFeatureUnlocked(wardZdo, WardFeature.VehicleLocking, playerId);

            default:
                return true;
        }
    }

    private static bool HasAnyPerkUnlocked(ZDO wardZdo, long playerId)
    {
        return WardProgressionService.IsFeatureUnlocked(wardZdo, WardFeature.WeatherRoof, playerId) ||
               WardProgressionService.IsFeatureUnlocked(wardZdo, WardFeature.RegenBoost, playerId) ||
               WardProgressionService.IsFeatureUnlocked(wardZdo, WardFeature.DeathSkillShield, playerId) ||
               WardProgressionService.IsFeatureUnlocked(wardZdo, WardFeature.FoodFreeze, playerId) ||
               WardProgressionService.IsFeatureUnlocked(wardZdo, WardFeature.GhostBase, playerId) ||
               WardProgressionService.IsFeatureUnlocked(wardZdo, WardFeature.RaidSuppression, playerId) ||
               WardProgressionService.IsFeatureUnlocked(wardZdo, WardFeature.SpawnSuppression, playerId) ||
               WardProgressionService.IsFeatureUnlocked(wardZdo, WardFeature.ProtectTames, playerId) ||
               WardProgressionService.IsFeatureUnlocked(wardZdo, WardFeature.AreaRepair, playerId) ||
               WardProgressionService.IsFeatureUnlocked(wardZdo, WardFeature.AutoFuel, playerId) ||
               WardProgressionService.IsFeatureUnlocked(wardZdo, WardFeature.AutoCloseDoors, playerId) ||
               WardProgressionService.IsFeatureUnlocked(wardZdo, WardFeature.MaxComfort, playerId);
    }

    public static List<FeaturePermission> GetGrantedPermissions(ZDO wardZdo, long playerId)
    {
        EnsureInitialized();
        List<FeaturePermission> granted = new();
        if (wardZdo == null)
            return granted;

        bool isAdmin = WardPermissions.IsAdmin(playerId);
        int roleId = GetPlayerRoleId(wardZdo, playerId);
        bool isOwner = roleId == (int)WardRole.Owner;
        if (!isOwner)
        {
            WardType wardType = (WardType)wardZdo.GetInt(WardZdoKeys.WardType, (int)WardType.Main);
            if (wardType == WardType.SubWard)
            {
                ZDOID parentId = wardZdo.GetZDOID(WardZdoKeys.ParentWard);
                if (parentId != ZDOID.None)
                {
                    ZDO parentZdo = (ZDOMan.instance != null) ? ZDOMan.instance.GetZDO(parentId) : null;
                    if (parentZdo != null && parentZdo.IsValid())
                    {
                        isOwner = GetPlayerRoleId(parentZdo, playerId) == (int)WardRole.Owner;
                    }
                }
            }
        }

        List<WardRoleDefinition> roles = GetRoleDefinitions(wardZdo);

        foreach (KeyValuePair<string, FeaturePermission> kvp in _registeredPermissions)
        {
            FeaturePermission perm = kvp.Value;
            if (!IsPermissionFeatureUnlocked(wardZdo, perm.Id, playerId))
                continue;

            if (isAdmin || isOwner || HasPermission(roles, roleId, perm.Id))
            {
                granted.Add(perm);
            }
        }

        granted.Sort((a, b) =>
        {
            int catComp = a.Category.CompareTo(b.Category);
            return catComp != 0 ? catComp : a.SortOrder.CompareTo(b.SortOrder);
        });
        return granted;
    }

    public static bool CanPerform(PrivateArea ward, long playerId, string permissionId)
    {
        if (ward == null || ward.m_nview == null || !ward.m_nview.IsValid())
            return false;

        return CanPerform(ward.m_nview.GetZDO(), playerId, permissionId);
    }

    public static string SerializeRoles(List<WardRoleDefinition> roles)
    {
        StringBuilder sb = new();
        for (int i = 0; i < roles.Count; i++)
        {
            WardRoleDefinition role = roles[i];
            if (i > 0)
                sb.Append(RoleDelimiter);

            string safeName = role.Name.Replace(RoleDelimiter, '_').Replace(FieldDelimiter, '_');
            sb.Append(role.Id).Append(FieldDelimiter);
            sb.Append(safeName).Append(FieldDelimiter);
            sb.Append(role.Level).Append(FieldDelimiter);
            sb.Append(role.ColorHex).Append(FieldDelimiter);

            int pCount = 0;
            foreach (string perm in role.ExplicitPermissions)
            {
                if (pCount > 0)
                    sb.Append(PermDelimiter);
                sb.Append(perm);
                pCount++;
            }
        }
        return sb.ToString();
    }

    public static List<WardRoleDefinition> DeserializeRoles(string data)
    {
        List<WardRoleDefinition> roles = new();
        if (string.IsNullOrEmpty(data))
            return roles;

        string[] roleTokens = data.Split(RoleDelimiter);
        for (int i = 0; i < roleTokens.Length; i++)
        {
            string token = roleTokens[i];
            if (string.IsNullOrEmpty(token))
                continue;

            string[] fields = token.Split(FieldDelimiter);
            if (fields.Length < 4)
                continue;

            if (!int.TryParse(fields[0], out int id))
                continue;

            string name = fields[1];
            if (!int.TryParse(fields[2], out int level))
                continue;

            string colorHex = fields[3];

            WardRoleDefinition roleDef = new WardRoleDefinition(id, name, level, colorHex);

            if (fields.Length >= 5 && !string.IsNullOrEmpty(fields[4]))
            {
                string[] perms = fields[4].Split(PermDelimiter);
                for (int j = 0; j < perms.Length; j++)
                {
                    string p = perms[j].Trim();
                    if (!string.IsNullOrEmpty(p))
                    {
                        roleDef.AddPermission(p);
                    }
                }
            }

            roles.Add(roleDef);
        }

        return roles;
    }

    private static void EnsureInitialized()
    {
        if (!_initialized)
        {
            Initialize();
        }
    }

    public static string GetRoleName(WardRole role)
    {
        return role switch
        {
            WardRole.Guest => "$wmb_role_guest",
            WardRole.Resident => "$wmb_role_resident",
            WardRole.Manager => "$wmb_role_manager",
            WardRole.Owner => "$wmb_role_owner",
            _ => role.ToString()
        };
    }

    public static string GetRoleDisplayName(ZDO wardZdo, int roleId)
    {
        if (wardZdo != null)
        {
            List<WardRoleDefinition> roles = GetRoleDefinitions(wardZdo);
            for (int i = 0; i < roles.Count; i++)
            {
                if (roles[i].Id == roleId)
                {
                    return roles[i].Name.Localize();
                }
            }
        }
        return GetRoleName((WardRole)roleId).Localize();
    }

    public static Color GetRoleColor(ZDO wardZdo, int roleId)
    {
        if (wardZdo != null)
        {
            List<WardRoleDefinition> roles = GetRoleDefinitions(wardZdo);
            for (int i = 0; i < roles.Count; i++)
            {
                if (roles[i].Id == roleId)
                {
                    if (!string.IsNullOrEmpty(roles[i].ColorHex) && ColorUtility.TryParseHtmlString(roles[i].ColorHex, out Color c))
                    {
                        return c;
                    }
                    break;
                }
            }
        }
        return roleId >= (int)WardRole.Manager ? WardColorPalette.GoldAmber : (roleId == (int)WardRole.Resident ? WardColorPalette.IceBlue : WardColorPalette.TextMuted);
    }

    public static int GetNextRoleId(ZDO wardZdo, int currentRoleId)
    {
        List<WardRoleDefinition> roles = GetRoleDefinitions(wardZdo);
        if (roles == null || roles.Count == 0)
            return (currentRoleId + 1) % 4;

        int currentIndex = -1;
        for (int i = 0; i < roles.Count; i++)
        {
            if (roles[i].Id == currentRoleId)
            {
                currentIndex = i;
                break;
            }
        }

        if (currentIndex == -1)
        {
            return roles[0].Id;
        }

        int nextIndex = (currentIndex + 1) % roles.Count;
        return roles[nextIndex].Id;
    }

    public static string GetCategoryDisplayName(WardCapabilityCategory category)
    {
        return category switch
        {
            WardCapabilityCategory.Core => "$wmb_cat_core",
            WardCapabilityCategory.Access => "$wmb_cat_access",
            WardCapabilityCategory.Defenses => "$wmb_cat_defenses",
            WardCapabilityCategory.Upkeep => "$wmb_cat_upkeep",
            WardCapabilityCategory.Personnel => "$wmb_cat_personnel",
            WardCapabilityCategory.External => "$wmb_cat_external",
            _ => category.ToString()
        };
    }

    private static void RegisterDefaultPermissions()
    {
        RegisterPermission(new FeaturePermission(WardPermissionIds.CoreDomeAccess, "$wmb_perm_core_dome_access_name", WardCapabilityCategory.Core, "$wmb_perm_core_dome_access_desc", WardRole.Guest, 0, WardRune.None));
        RegisterPermission(new FeaturePermission(WardPermissionIds.CoreBenefits, "$wmb_perm_core_benefits_name", WardCapabilityCategory.Core, "$wmb_perm_core_benefits_desc", WardRole.Resident, 1, WardRune.Isa));
        RegisterPermission(new FeaturePermission(WardPermissionIds.CoreBuilding, "$wmb_perm_core_building_name", WardCapabilityCategory.Core, "$wmb_perm_core_building_desc", WardRole.Resident, 2, WardRune.None));
        RegisterPermission(new FeaturePermission(WardPermissionIds.CoreFarming, "$wmb_perm_core_farming_name", WardCapabilityCategory.Core, "$wmb_perm_core_farming_desc", WardRole.Resident, 3, WardRune.Uruz));
        RegisterPermission(new FeaturePermission(WardPermissionIds.CoreHarvest, "$wmb_perm_core_harvest_name", WardCapabilityCategory.Core, "$wmb_perm_core_harvest_desc", WardRole.Resident, 4, WardRune.Uruz));
        RegisterPermission(new FeaturePermission(WardPermissionIds.CoreViewGui, "$wmb_perm_core_view_gui_name", WardCapabilityCategory.Core, "$wmb_perm_core_view_gui_desc", WardRole.Resident, 5, WardRune.None));
        RegisterPermission(new FeaturePermission(WardPermissionIds.CorePower, "$wmb_perm_core_power_name", WardCapabilityCategory.Core, "$wmb_perm_core_power_desc", WardRole.Manager, 6, WardRune.None));
        RegisterPermission(new FeaturePermission(WardPermissionIds.CoreRename, "$wmb_perm_core_rename_name", WardCapabilityCategory.Core, "$wmb_perm_core_rename_desc", WardRole.Manager, 7, WardRune.None));
        RegisterPermission(new FeaturePermission(WardPermissionIds.CoreExtendWard, "$wmb_perm_core_extend_ward_name", WardCapabilityCategory.Core, "$wmb_perm_core_extend_ward_desc", WardRole.Owner, 8, WardRune.None));
        RegisterPermission(new FeaturePermission(WardPermissionIds.CoreSubWard, "$wmb_perm_core_sub_ward_name", WardCapabilityCategory.Core, "$wmb_perm_core_sub_ward_desc", WardRole.Manager, 9, WardRune.None));
        RegisterPermission(new FeaturePermission(WardPermissionIds.CoreAttuneWard, "$wmb_perm_core_attune_ward_name", WardCapabilityCategory.Core, "$wmb_perm_core_attune_ward_desc", WardRole.Owner, 10, WardRune.None));

        RegisterPermission(new FeaturePermission(WardPermissionIds.AccessDoors, "$wmb_perm_access_doors_name", WardCapabilityCategory.Access, "$wmb_perm_access_doors_desc", WardRole.Manager, 10, WardRune.None));
        RegisterPermission(new FeaturePermission(WardPermissionIds.AccessChests, "$wmb_perm_access_chests_name", WardCapabilityCategory.Access, "$wmb_perm_access_chests_desc", WardRole.Manager, 11, WardRune.None));
        RegisterPermission(new FeaturePermission(WardPermissionIds.AccessPortals, "$wmb_perm_access_portals_name", WardCapabilityCategory.Access, "$wmb_perm_access_portals_desc", WardRole.Manager, 12, WardRune.Uruz));

        RegisterPermission(new FeaturePermission(WardPermissionIds.DefensesDomeSize, "$wmb_perm_defenses_dome_size_name", WardCapabilityCategory.Defenses, "$wmb_perm_defenses_dome_size_desc", WardRole.Owner, 19, WardRune.None));
        RegisterPermission(new FeaturePermission(WardPermissionIds.DefensesCombat, "$wmb_perm_defenses_combat_name", WardCapabilityCategory.Defenses, "$wmb_perm_defenses_combat_desc", WardRole.Manager, 20, WardRune.Thurisaz));
        RegisterPermission(new FeaturePermission(WardPermissionIds.DefensesCamouflage, "$wmb_perm_defenses_camouflage_name", WardCapabilityCategory.Defenses, "$wmb_perm_defenses_camouflage_desc", WardRole.Manager, 21, WardRune.Sowilo));
        RegisterPermission(new FeaturePermission(WardPermissionIds.DefensesBiome, "$wmb_perm_defenses_biome_name", WardCapabilityCategory.Defenses, "$wmb_perm_defenses_biome_desc", WardRole.Manager, 22, WardRune.Uruz));
        RegisterPermission(new FeaturePermission(WardPermissionIds.DefensesTames, "$wmb_perm_defenses_tames_name", WardCapabilityCategory.Defenses, "$wmb_perm_defenses_tames_desc", WardRole.Manager, 23, WardRune.Fehu));

        RegisterPermission(new FeaturePermission(WardPermissionIds.UpkeepRepair, "$wmb_perm_upkeep_repair_name", WardCapabilityCategory.Upkeep, "$wmb_perm_upkeep_repair_desc", WardRole.Manager, 30, WardRune.Thurisaz));
        RegisterPermission(new FeaturePermission(WardPermissionIds.UpkeepAutoFuel, "$wmb_perm_upkeep_autofuel_name", WardCapabilityCategory.Upkeep, "$wmb_perm_upkeep_autofuel_desc", WardRole.Manager, 31, WardRune.Uruz));
        RegisterPermission(new FeaturePermission(WardPermissionIds.UpkeepAutoDoors, "$wmb_perm_upkeep_autodoors_name", WardCapabilityCategory.Upkeep, "$wmb_perm_upkeep_autodoors_desc", WardRole.Manager, 32, WardRune.Fehu));
        RegisterPermission(new FeaturePermission(WardPermissionIds.UpkeepPerks, "$wmb_perm_upkeep_perks_name", WardCapabilityCategory.Upkeep, "$wmb_perm_upkeep_perks_desc", WardRole.Manager, 33, WardRune.Isa));
        RegisterPermission(new FeaturePermission(WardPermissionIds.UpkeepWaterDrain, "$wmb_perm_upkeep_waterdrain_name", WardCapabilityCategory.Upkeep, "$wmb_perm_upkeep_waterdrain_desc", WardRole.Manager, 34, WardRune.Laguz));
        RegisterPermission(new FeaturePermission(WardPermissionIds.UpkeepSecurity, "$wmb_perm_upkeep_security_name", WardCapabilityCategory.Upkeep, "$wmb_perm_upkeep_security_desc", WardRole.Manager, 35, WardRune.Hagalaz));

        RegisterPermission(new FeaturePermission(WardPermissionIds.PersonnelRoster, "$wmb_perm_personnel_roster_name", WardCapabilityCategory.Personnel, "$wmb_perm_personnel_roster_desc", WardRole.Manager, 40, WardRune.None));
        RegisterPermission(new FeaturePermission(WardPermissionIds.PersonnelGovernance, "$wmb_perm_personnel_governance_name", WardCapabilityCategory.Personnel, "$wmb_perm_personnel_governance_desc", WardRole.Owner, 41, WardRune.None));
    }
}
