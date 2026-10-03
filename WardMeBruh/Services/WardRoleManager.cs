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

    private static readonly Dictionary<string, FeaturePermission> _registeredPermissions = new(StringComparer.OrdinalIgnoreCase);
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
        if (string.IsNullOrEmpty(data))
        {
            List<WardRoleDefinition> defaultRoles = CreateDefaultRoleDefinitions();
            SaveRoleDefinitions(wardZdo, defaultRoles);
            return defaultRoles;
        }

        List<WardRoleDefinition> roles = DeserializeRoles(data);
        if (roles.Count == 0)
        {
            roles = CreateDefaultRoleDefinitions();
            SaveRoleDefinitions(wardZdo, roles);
        }
        else
        {
            bool modified = false;

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

            if (modified)
            {
                SaveRoleDefinitions(wardZdo, roles);
            }
        }

        roles.Sort((a, b) => a.Level.CompareTo(b.Level));
        return roles;
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
    }

    public static WardRoleAssignments GetRoleAssignments(ZDO wardZdo)
    {
        if (wardZdo == null)
            return new WardRoleAssignments();

        wardZdo = WardQueryService.GetEffectiveWardZdo(wardZdo);
        if (wardZdo == null)
            return new WardRoleAssignments();

        string data = wardZdo.GetString(WardZdoKeys.RoleAssignments, string.Empty);
        if (!string.IsNullOrEmpty(data))
        {
            return WardRoleAssignments.Deserialize(data);
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
    }

    public static int GetPlayerRoleId(ZDO wardZdo, long playerId)
    {
        if (wardZdo == null || playerId == 0L)
            return (int)WardRole.Guest;

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

        if (WardPermissions.IsAdmin(playerId))
            return true;

        int roleId = GetPlayerRoleId(wardZdo, playerId);
        if (roleId == (int)WardRole.Owner)
            return true;

        List<WardRoleDefinition> roles = GetRoleDefinitions(wardZdo);
        return HasPermission(roles, roleId, permissionId);
    }

    public static List<FeaturePermission> GetGrantedPermissions(ZDO wardZdo, long playerId)
    {
        EnsureInitialized();
        List<FeaturePermission> granted = new();
        if (wardZdo == null)
            return granted;

        bool isAdmin = WardPermissions.IsAdmin(playerId);
        int roleId = GetPlayerRoleId(wardZdo, playerId);
        List<WardRoleDefinition> roles = GetRoleDefinitions(wardZdo);

        foreach (KeyValuePair<string, FeaturePermission> kvp in _registeredPermissions)
        {
            FeaturePermission perm = kvp.Value;
            if (isAdmin || roleId == (int)WardRole.Owner || HasPermission(roles, roleId, perm.Id))
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
        RegisterPermission(new FeaturePermission(WardPermissionIds.CoreDomeAccess, "$wmb_perm_core_dome_access_name", WardCapabilityCategory.Core, "$wmb_perm_core_dome_access_desc", WardRole.Guest, 0));
        RegisterPermission(new FeaturePermission(WardPermissionIds.CoreBenefits, "$wmb_perm_core_benefits_name", WardCapabilityCategory.Core, "$wmb_perm_core_benefits_desc", WardRole.Resident, 1));
        RegisterPermission(new FeaturePermission(WardPermissionIds.CoreBuilding, "$wmb_perm_core_building_name", WardCapabilityCategory.Core, "$wmb_perm_core_building_desc", WardRole.Resident, 2));
        RegisterPermission(new FeaturePermission(WardPermissionIds.CoreFarming, "$wmb_perm_core_farming_name", WardCapabilityCategory.Core, "$wmb_perm_core_farming_desc", WardRole.Resident, 3));
        RegisterPermission(new FeaturePermission(WardPermissionIds.CoreHarvest, "$wmb_perm_core_harvest_name", WardCapabilityCategory.Core, "$wmb_perm_core_harvest_desc", WardRole.Resident, 4));
        RegisterPermission(new FeaturePermission(WardPermissionIds.CoreViewGui, "$wmb_perm_core_view_gui_name", WardCapabilityCategory.Core, "$wmb_perm_core_view_gui_desc", WardRole.Resident, 5));
        RegisterPermission(new FeaturePermission(WardPermissionIds.CorePower, "$wmb_perm_core_power_name", WardCapabilityCategory.Core, "$wmb_perm_core_power_desc", WardRole.Manager, 6));
        RegisterPermission(new FeaturePermission(WardPermissionIds.CoreRename, "$wmb_perm_core_rename_name", WardCapabilityCategory.Core, "$wmb_perm_core_rename_desc", WardRole.Manager, 7));

        RegisterPermission(new FeaturePermission(WardPermissionIds.AccessDoors, "$wmb_perm_access_doors_name", WardCapabilityCategory.Access, "$wmb_perm_access_doors_desc", WardRole.Manager, 10));
        RegisterPermission(new FeaturePermission(WardPermissionIds.AccessChests, "$wmb_perm_access_chests_name", WardCapabilityCategory.Access, "$wmb_perm_access_chests_desc", WardRole.Manager, 11));
        RegisterPermission(new FeaturePermission(WardPermissionIds.AccessPortals, "$wmb_perm_access_portals_name", WardCapabilityCategory.Access, "$wmb_perm_access_portals_desc", WardRole.Manager, 12));

        RegisterPermission(new FeaturePermission(WardPermissionIds.DefensesDomeSize, "$wmb_perm_defenses_dome_size_name", WardCapabilityCategory.Defenses, "$wmb_perm_defenses_dome_size_desc", WardRole.Owner, 19));
        RegisterPermission(new FeaturePermission(WardPermissionIds.DefensesCombat, "$wmb_perm_defenses_combat_name", WardCapabilityCategory.Defenses, "$wmb_perm_defenses_combat_desc", WardRole.Manager, 20));
        RegisterPermission(new FeaturePermission(WardPermissionIds.DefensesCamouflage, "$wmb_perm_defenses_camouflage_name", WardCapabilityCategory.Defenses, "$wmb_perm_defenses_camouflage_desc", WardRole.Manager, 21));
        RegisterPermission(new FeaturePermission(WardPermissionIds.DefensesBiome, "$wmb_perm_defenses_biome_name", WardCapabilityCategory.Defenses, "$wmb_perm_defenses_biome_desc", WardRole.Manager, 22));
        RegisterPermission(new FeaturePermission(WardPermissionIds.DefensesTames, "$wmb_perm_defenses_tames_name", WardCapabilityCategory.Defenses, "$wmb_perm_defenses_tames_desc", WardRole.Manager, 23));

        RegisterPermission(new FeaturePermission(WardPermissionIds.UpkeepRepair, "$wmb_perm_upkeep_repair_name", WardCapabilityCategory.Upkeep, "$wmb_perm_upkeep_repair_desc", WardRole.Manager, 30));
        RegisterPermission(new FeaturePermission(WardPermissionIds.UpkeepAutoFuel, "$wmb_perm_upkeep_autofuel_name", WardCapabilityCategory.Upkeep, "$wmb_perm_upkeep_autofuel_desc", WardRole.Manager, 31));
        RegisterPermission(new FeaturePermission(WardPermissionIds.UpkeepAutoDoors, "$wmb_perm_upkeep_autodoors_name", WardCapabilityCategory.Upkeep, "$wmb_perm_upkeep_autodoors_desc", WardRole.Manager, 32));
        RegisterPermission(new FeaturePermission(WardPermissionIds.UpkeepPerks, "$wmb_perm_upkeep_perks_name", WardCapabilityCategory.Upkeep, "$wmb_perm_upkeep_perks_desc", WardRole.Manager, 33));
        RegisterPermission(new FeaturePermission(WardPermissionIds.UpkeepWaterDrain, "$wmb_perm_upkeep_waterdrain_name", WardCapabilityCategory.Upkeep, "$wmb_perm_upkeep_waterdrain_desc", WardRole.Manager, 34));
        RegisterPermission(new FeaturePermission(WardPermissionIds.UpkeepSecurity, "$wmb_perm_upkeep_security_name", WardCapabilityCategory.Upkeep, "$wmb_perm_upkeep_security_desc", WardRole.Manager, 35));

        RegisterPermission(new FeaturePermission(WardPermissionIds.PersonnelRoster, "$wmb_perm_personnel_roster_name", WardCapabilityCategory.Personnel, "$wmb_perm_personnel_roster_desc", WardRole.Manager, 40));
        RegisterPermission(new FeaturePermission(WardPermissionIds.PersonnelGovernance, "$wmb_perm_personnel_governance_name", WardCapabilityCategory.Personnel, "$wmb_perm_personnel_governance_desc", WardRole.Owner, 41));
    }
}
