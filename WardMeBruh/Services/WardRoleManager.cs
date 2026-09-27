using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
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

        WardRoleDefinition guest = new WardRoleDefinition((int)WardRole.Guest, "Guest", 0, WardColorPalette.ColorHex(WardColorPalette.TextMuted));
        WardRoleDefinition resident = new WardRoleDefinition((int)WardRole.Resident, "Resident", 1, WardColorPalette.ColorHex(WardColorPalette.ValheimGreen));
        WardRoleDefinition manager = new WardRoleDefinition((int)WardRole.Manager, "Manager", 2, WardColorPalette.ColorHex(WardColorPalette.IceBlue));
        WardRoleDefinition owner = new WardRoleDefinition((int)WardRole.Owner, "Owner", 3, WardColorPalette.ColorHex(WardColorPalette.GoldAmber));

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
        roles.Sort((a, b) => a.Level.CompareTo(b.Level));
        return roles;
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
            EnsurePermissionExists(roles, "core.dome_access", (int)WardRole.Guest, ref modified);
            EnsurePermissionExists(roles, "core.benefits", (int)WardRole.Resident, ref modified);
            EnsurePermissionExists(roles, "core.building", (int)WardRole.Resident, ref modified);
            EnsurePermissionExists(roles, "core.farming", (int)WardRole.Resident, ref modified);
            EnsurePermissionExists(roles, "core.harvest", (int)WardRole.Resident, ref modified);
            EnsurePermissionExists(roles, "defenses.dome_size", (int)WardRole.Owner, ref modified);

            if (modified)
            {
                SaveRoleDefinitions(wardZdo, roles);
            }
        }

        roles.Sort((a, b) => a.Level.CompareTo(b.Level));
        return roles;
    }

    private static void EnsurePermissionExists(List<WardRoleDefinition> roles, string permId, int defaultRoleId, ref bool modified)
    {
        for (int i = 0; i < roles.Count; i++)
        {
            if (roles[i].HasExplicitPermission(permId))
                return;
        }

        for (int i = 0; i < roles.Count; i++)
        {
            if (roles[i].Id == defaultRoleId)
            {
                roles[i].AddPermission(permId);
                modified = true;
                break;
            }
        }
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

        if (targetRole.Id == (int)WardRole.Owner || targetRole.Level >= 3)
            return true;

        if (targetRole.HasExplicitPermission(permissionId))
            return true;

        for (int i = 0; i < roles.Count; i++)
        {
            WardRoleDefinition lowerRole = roles[i];
            if (lowerRole.Level < targetRole.Level)
            {
                if (lowerRole.HasExplicitPermission(permissionId))
                    return true;
            }
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

        if (targetRole.Id == (int)WardRole.Owner || targetRole.Level >= 3)
        {
            inheritedFromRoleName = "Owner Authority";
            return true;
        }

        for (int i = 0; i < roles.Count; i++)
        {
            WardRoleDefinition lowerRole = roles[i];
            if (lowerRole.Level < targetRole.Level)
            {
                if (lowerRole.HasExplicitPermission(permissionId))
                {
                    inheritedFromRoleName = lowerRole.Name;
                    return true;
                }
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

        WardRole resolvedRole = WardPermissions.GetPlayerRole(wardZdo, playerId);
        if (resolvedRole == WardRole.Owner)
            return true;

        List<WardRoleDefinition> roles = GetRoleDefinitions(wardZdo);
        int roleId = (int)resolvedRole;

        return HasPermission(roles, roleId, permissionId);
    }

    public static List<FeaturePermission> GetGrantedPermissions(ZDO wardZdo, long playerId)
    {
        EnsureInitialized();
        List<FeaturePermission> granted = new();
        if (wardZdo == null)
            return granted;

        bool isAdmin = WardPermissions.IsAdmin(playerId);
        WardRole role = WardPermissions.GetPlayerRole(wardZdo, playerId);
        List<WardRoleDefinition> roles = GetRoleDefinitions(wardZdo);
        int roleId = (int)role;

        foreach (KeyValuePair<string, FeaturePermission> kvp in _registeredPermissions)
        {
            FeaturePermission perm = kvp.Value;
            if (isAdmin || role == WardRole.Owner || HasPermission(roles, roleId, perm.Id))
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

    private static void RegisterDefaultPermissions()
    {
        RegisterPermission(new FeaturePermission("core.dome_access", "Dome Access Clearance", WardCapabilityCategory.Core, "Can pass through the ward forcefield dome and move freely inside the perimeter without repulsion or shock.", WardRole.Guest, 0));
        RegisterPermission(new FeaturePermission("core.benefits", "Shield Sanctuary Benefits", WardCapabilityCategory.Core, "Receives territory status and base sanctuary benefits under the shield: maximum comfort, food decay pause, death skill shield, and regen boost.", WardRole.Resident, 1));
        RegisterPermission(new FeaturePermission("core.building", "Building & Construction", WardCapabilityCategory.Core, "Can construct, place, repair, and deconstruct pieces, structures, and furniture within the protected perimeter.", WardRole.Resident, 2));
        RegisterPermission(new FeaturePermission("core.farming", "Farming & Cultivation", WardCapabilityCategory.Core, "Can cultivate soil, plant seeds, plant crops, and tend saplings using the Cultivator.", WardRole.Resident, 3));
        RegisterPermission(new FeaturePermission("core.harvest", "Harvest Crops & Pickables", WardCapabilityCategory.Core, "Can harvest mature crops, berry bushes, mushrooms, and pickable resources within the protected perimeter.", WardRole.Resident, 4));
        RegisterPermission(new FeaturePermission("core.view_gui", "View Ward GUI", WardCapabilityCategory.Core, "Can open and inspect the WardMeBruh interface at the ward stone.", WardRole.Resident, 5));
        RegisterPermission(new FeaturePermission("core.power", "Toggle Ward Power", WardCapabilityCategory.Core, "Can activate or deactivate the ward shield.", WardRole.Manager, 6));
        RegisterPermission(new FeaturePermission("core.rename", "Rename Settlement", WardCapabilityCategory.Core, "Can edit the ward settlement name.", WardRole.Manager, 7));

        RegisterPermission(new FeaturePermission("access.doors", "Configure Doors & Gates", WardCapabilityCategory.Access, "Can rename doors and change door clearance role requirements.", WardRole.Resident, 10));
        RegisterPermission(new FeaturePermission("access.chests", "Configure Containers", WardCapabilityCategory.Access, "Can rename chests, change clearance roles, and toggle Fuel Depots.", WardRole.Resident, 11));
        RegisterPermission(new FeaturePermission("access.portals", "Configure Portals", WardCapabilityCategory.Access, "Can change portal transit clearance roles.", WardRole.Manager, 12));

        RegisterPermission(new FeaturePermission("defenses.dome_size", "Dome Size & Coverage Radius", WardCapabilityCategory.Defenses, "Can adjust the ward forcefield dome size and coverage radius slider.", WardRole.Owner, 19));
        RegisterPermission(new FeaturePermission("defenses.combat", "Forcefield & Repulsion", WardCapabilityCategory.Defenses, "Can adjust pushback force, retaliatory shock damage, and dome visuals.", WardRole.Manager, 20));
        RegisterPermission(new FeaturePermission("defenses.camouflage", "Tactical Camouflage", WardCapabilityCategory.Defenses, "Can toggle Ghost Base, Raid Suppression, and Mob Spawn Suppression.", WardRole.Manager, 21));
        RegisterPermission(new FeaturePermission("defenses.biome", "Biome Hazard Shields", WardCapabilityCategory.Defenses, "Can toggle Virtual Roof, Ashlands Shield, and Deep North Snow Shield.", WardRole.Manager, 22));
        RegisterPermission(new FeaturePermission("defenses.tames", "Protect Tamed Animals", WardCapabilityCategory.Defenses, "Can toggle damage immunity for tamed livestock.", WardRole.Manager, 23));

        RegisterPermission(new FeaturePermission("upkeep.repair", "Area Piece Repair", WardCapabilityCategory.Upkeep, "Can toggle free area repair and adjust weathering threshold.", WardRole.Manager, 30));
        RegisterPermission(new FeaturePermission("upkeep.autofuel", "Auto-Fuel Supply Depots", WardCapabilityCategory.Upkeep, "Can toggle automated refueling for fires and smelters.", WardRole.Manager, 31));
        RegisterPermission(new FeaturePermission("upkeep.autodoors", "Auto-Closing Doors", WardCapabilityCategory.Upkeep, "Can toggle auto-closing doors and delay timer.", WardRole.Manager, 32));
        RegisterPermission(new FeaturePermission("upkeep.perks", "Sanctuary Perks", WardCapabilityCategory.Upkeep, "Can toggle Maximum Comfort, Food Decay Freeze, Death Shield, and Regen Boost.", WardRole.Manager, 33));
        RegisterPermission(new FeaturePermission("upkeep.waterdrain", "Underwater Dome Draining", WardCapabilityCategory.Upkeep, "Can toggle seabed water displacement.", WardRole.Manager, 34));
        RegisterPermission(new FeaturePermission("upkeep.security", "Agricultural & World Security", WardCapabilityCategory.Upkeep, "Can toggle Crop Protection, Terrain Lock, and Vehicle Locking.", WardRole.Manager, 35));

        RegisterPermission(new FeaturePermission("personnel.roster", "Manage Personnel Rosters", WardCapabilityCategory.Personnel, "Can assign, promote, and demote players to roles below their own.", WardRole.Manager, 40));
        RegisterPermission(new FeaturePermission("personnel.governance", "Manage Roles & Governance", WardCapabilityCategory.Personnel, "Can access the MANAGEMENT tab to configure role capabilities and add custom roles.", WardRole.Owner, 41));
    }
}
