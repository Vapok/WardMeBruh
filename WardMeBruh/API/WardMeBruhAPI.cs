using System;
using System.Collections.Generic;
using UnityEngine;
using WardMeBruh.Models;
#if ! API
using WardMeBruh.Configuration;
using WardMeBruh.Services;
#endif

namespace WardMeBruh.API;

/// <summary>
/// Public developer API for WardMeBruh. Provides programmatic registration for external role features, custom role templates, and ward status/permission queries.
/// </summary>
public static class WardMeBruhAPI
{
    /// <summary>
    /// Notifies whether WardMeBruh is loaded and active in the current game session.
    /// </summary>
    /// <returns>True if WardMeBruh is loaded; otherwise false.</returns>
    public static bool IsLoaded()
    {
#if ! API
        return true;
#else
        return false;
#endif
    }

    /// <summary>
    /// Retrieves all registered permission IDs currently tracked by WardMeBruh, including built-in and external mod permissions.
    /// </summary>
    /// <returns>Collection of permission ID strings.</returns>
    public static IEnumerable<string> GetAllPermissionIds()
    {
#if ! API
        return WardRoleManager.GetAllPermissionIds();
#else
        return Array.Empty<string>();
#endif
    }

    /// <summary>
    /// Registers a custom external role feature permission into the WardMeBruh permission matrix.
    /// </summary>
    /// <param name="id">Unique identifier for the permission (e.g. "com.author.mod.feature").</param>
    /// <param name="displayName">Human-readable label displayed in the Management matrix.</param>
    /// <param name="category">Capability category under which the feature is grouped.</param>
    /// <param name="description">Tooltip explanation of what this permission grants.</param>
    /// <param name="defaultRole">Default minimum role level required to access this feature.</param>
    /// <param name="sortOrder">Visual sort ordering within the category.</param>
    public static void RegisterRoleFeature(
        string id, 
        string displayName, 
        WardCapabilityCategory category, 
        string description, 
        WardRole defaultRole = WardRole.Resident, 
        int sortOrder = 100)
    {
#if ! API
        if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(displayName))
            return;

        FeaturePermission perm = new(id, displayName, category, description ?? string.Empty, defaultRole, sortOrder);
        WardRoleManager.RegisterPermission(perm);
        WardMeBruh.Log.Info($"[API] Registered external role feature '{displayName}' ({id}) under {category} category (Default: {defaultRole})");
#endif
    }

    /// <summary>
    /// Registers a custom external role feature permission using a category string name.
    /// </summary>
    /// <param name="id">Unique identifier for the permission (e.g. "com.author.mod.feature").</param>
    /// <param name="displayName">Human-readable label displayed in the Management matrix.</param>
    /// <param name="categoryName">Name of the category to assign or group under.</param>
    /// <param name="description">Tooltip explanation of what this permission grants.</param>
    /// <param name="defaultRole">Default minimum role level required to access this feature.</param>
    /// <param name="sortOrder">Visual sort ordering within the category.</param>
    public static void RegisterRoleFeature(
        string id, 
        string displayName, 
        string categoryName, 
        string description, 
        WardRole defaultRole = WardRole.Resident, 
        int sortOrder = 100)
    {
        WardCapabilityCategory category = WardCapabilityCategory.External;
        if (!string.IsNullOrEmpty(categoryName) && Enum.TryParse(categoryName, true, out WardCapabilityCategory parsed))
        {
            category = parsed;
        }

        RegisterRoleFeature(id, displayName, category, description, defaultRole, sortOrder);
    }

    /// <summary>
    /// Registers a custom external role feature under the default External Mod Integrations category.
    /// </summary>
    /// <param name="id">Unique identifier for the permission (e.g. "com.author.mod.feature").</param>
    /// <param name="displayName">Human-readable label displayed in the Management matrix.</param>
    /// <param name="description">Tooltip explanation of what this permission grants.</param>
    /// <param name="defaultRole">Default minimum role level required to access this feature.</param>
    /// <param name="sortOrder">Visual sort ordering within the category.</param>
    public static void RegisterRoleFeature(
        string id, 
        string displayName, 
        string description, 
        WardRole defaultRole = WardRole.Resident, 
        int sortOrder = 100)
    {
        RegisterRoleFeature(id, displayName, WardCapabilityCategory.External, description, defaultRole, sortOrder);
    }

    /// <summary>
    /// Registers a custom default role template into newly created or unconfigured wards.
    /// </summary>
    /// <param name="name">Display name of the custom role.</param>
    /// <param name="level">Hierarchy level from 0 (Guest equivalent) to 3 (Admin equivalent).</param>
    /// <param name="colorHex">Hex color string (e.g. "#38bdf8") for UI badges.</param>
    /// <param name="defaultPermissions">Optional list of permission IDs granted to this role by default.</param>
    public static void RegisterRoleTemplate(
        string name, 
        int level, 
        string colorHex, 
        IEnumerable<string> defaultPermissions = null)
    {
#if ! API
        if (string.IsNullOrEmpty(name))
            return;

        int safeId = 100 + Math.Abs(name.GetStableHashCode() % 900);
        string safeColor = string.IsNullOrEmpty(colorHex) ? "#38bdf8" : colorHex;
        WardRoleDefinition roleDef = new(safeId, name, Mathf.Clamp(level, 0, 3), safeColor);

        if (defaultPermissions != null)
        {
            foreach (string permId in defaultPermissions)
            {
                if (!string.IsNullOrEmpty(permId))
                {
                    roleDef.AddPermission(permId.Trim());
                }
            }
        }

        WardRoleManager.RegisterRoleTemplate(roleDef);
        WardMeBruh.Log.Info($"[API] Registered external role template '{name}' (Level {level}, Color {safeColor})");
#endif
    }

    /// <summary>
    /// Checks whether a player has access to a specific feature permission at the given world position.
    /// </summary>
    /// <param name="point">World position to check.</param>
    /// <param name="featureId">Permission ID registered with WardMeBruh.</param>
    /// <param name="playerId">Player ID to evaluate, or 0 to query the local player.</param>
    /// <returns>True if access is granted or if outside any ward territory; otherwise false.</returns>
    public static bool HasFeatureAccess(Vector3 point, string featureId, long playerId = 0L)
    {
#if ! API
        if (string.IsNullOrEmpty(featureId))
            return false;

        ZDO wardZdo = WardQueryService.GetCoveringWardZdo(point);
        if (wardZdo == null)
            return false;

        if (playerId == 0L && Player.m_localPlayer != null)
        {
            playerId = Player.m_localPlayer.GetPlayerID();
        }

        return WardRoleManager.CanPerform(wardZdo, playerId, featureId);
#else
        return true;
#endif
    }

    /// <summary>
    /// Checks whether a player has access to a specific feature permission within a specific ward component.
    /// </summary>
    /// <param name="ward">PrivateArea component instance.</param>
    /// <param name="featureId">Permission ID registered with WardMeBruh.</param>
    /// <param name="playerId">Player ID to evaluate, or 0 to query the local player.</param>
    /// <returns>True if access is granted; otherwise false.</returns>
    public static bool HasFeatureAccess(PrivateArea ward, string featureId, long playerId = 0L)
    {
#if ! API
        if (ward == null || ward.m_nview == null || !ward.m_nview.IsValid())
            return false;

        if (string.IsNullOrEmpty(featureId))
            return false;

        if (playerId == 0L && Player.m_localPlayer != null)
        {
            playerId = Player.m_localPlayer.GetPlayerID();
        }

        ZDO zdo = ward.m_nview.GetZDO();
        return WardRoleManager.CanPerform(zdo, playerId, featureId);
#else
        return true;
#endif
    }

    /// <summary>
    /// Checks whether a player has access to a specific feature permission at their current location.
    /// </summary>
    /// <param name="player">Player instance to evaluate.</param>
    /// <param name="featureId">Permission ID registered with WardMeBruh.</param>
    /// <returns>True if access is granted; otherwise false.</returns>
    public static bool HasFeatureAccess(Player player, string featureId)
    {
#if ! API
        if (player == null)
            return false;

        return HasFeatureAccess(player.transform.position, featureId, player.GetPlayerID());
#else
        return true;
#endif
    }

    /// <summary>
    /// Retrieves the assigned WardRole enum of a player at a specific world position.
    /// </summary>
    /// <param name="point">World position to check.</param>
    /// <param name="playerId">Player ID to evaluate, or 0 to query the local player.</param>
    /// <returns>Assigned WardRole enum, or WardRole.Guest if outside any ward.</returns>
    public static WardRole GetPlayerRole(Vector3 point, long playerId = 0L)
    {
#if ! API
        ZDO wardZdo = WardQueryService.GetCoveringWardZdo(point);
        if (wardZdo == null)
            return WardRole.Guest;

        if (playerId == 0L && Player.m_localPlayer != null)
        {
            playerId = Player.m_localPlayer.GetPlayerID();
        }

        return WardPermissions.GetPlayerRole(wardZdo, playerId);
#else
        return WardRole.Guest;
#endif
    }

    /// <summary>
    /// Retrieves the assigned WardRole enum of a player for a specific ward.
    /// </summary>
    /// <param name="ward">PrivateArea component instance.</param>
    /// <param name="playerId">Player ID to evaluate, or 0 to query the local player.</param>
    /// <returns>Assigned WardRole enum, or WardRole.Guest if invalid.</returns>
    public static WardRole GetPlayerRole(PrivateArea ward, long playerId = 0L)
    {
#if ! API
        if (ward == null || ward.m_nview == null || !ward.m_nview.IsValid())
            return WardRole.Guest;

        if (playerId == 0L && Player.m_localPlayer != null)
        {
            playerId = Player.m_localPlayer.GetPlayerID();
        }

        ZDO zdo = ward.m_nview.GetZDO();
        return WardPermissions.GetPlayerRole(zdo, playerId);
#else
        return WardRole.Guest;
#endif
    }

    /// <summary>
    /// Retrieves the display name of the player's role at a specific world position.
    /// </summary>
    /// <param name="point">World position to check.</param>
    /// <param name="playerId">Player ID to evaluate, or 0 to query the local player.</param>
    /// <returns>Role name string, or "None" if outside any ward.</returns>
    public static string GetPlayerRoleName(Vector3 point, long playerId = 0L)
    {
#if ! API
        ZDO wardZdo = WardQueryService.GetCoveringWardZdo(point);
        if (wardZdo == null)
            return "None";

        if (playerId == 0L && Player.m_localPlayer != null)
        {
            playerId = Player.m_localPlayer.GetPlayerID();
        }

        int roleId = WardRoleManager.GetPlayerRoleId(wardZdo, playerId);
        List<WardRoleDefinition> roles = WardRoleManager.GetRoleDefinitions(wardZdo);
        for (int i = 0; i < roles.Count; i++)
        {
            if (roles[i].Id == roleId)
            {
                return roles[i].Name;
            }
        }

        WardRole role = WardPermissions.GetPlayerRole(wardZdo, playerId);
        return role.ToString();
#else
        return "Guest";
#endif
    }

    /// <summary>
    /// Retrieves the display name of the player's role for a specific ward.
    /// </summary>
    /// <param name="ward">PrivateArea component instance.</param>
    /// <param name="playerId">Player ID to evaluate, or 0 to query the local player.</param>
    /// <returns>Role name string, or "None" if invalid.</returns>
    public static string GetPlayerRoleName(PrivateArea ward, long playerId = 0L)
    {
#if ! API
        if (ward == null || ward.m_nview == null || !ward.m_nview.IsValid())
            return "None";

        if (playerId == 0L && Player.m_localPlayer != null)
        {
            playerId = Player.m_localPlayer.GetPlayerID();
        }

        ZDO zdo = ward.m_nview.GetZDO();
        int roleId = WardRoleManager.GetPlayerRoleId(zdo, playerId);
        List<WardRoleDefinition> roles = WardRoleManager.GetRoleDefinitions(zdo);
        for (int i = 0; i < roles.Count; i++)
        {
            if (roles[i].Id == roleId)
            {
                return roles[i].Name;
            }
        }

        WardRole role = WardPermissions.GetPlayerRole(zdo, playerId);
        return role.ToString();
#else
        return "Guest";
#endif
    }

    /// <summary>
    /// Determines whether the specified position is within an active ward territory or network.
    /// </summary>
    /// <param name="point">World position to check.</param>
    /// <returns>True if inside an active ward boundary; otherwise false.</returns>
    public static bool IsInsideWard(Vector3 point)
    {
#if ! API
        return WardQueryService.GetCoveringWard(point) != null || WardRegistryManager.Instance.GetWardCoveringPoint(point) != null;
#else
        return false;
#endif
    }

    /// <summary>
    /// Determines whether the ward covering the specified point has its forcefield dome active.
    /// </summary>
    /// <param name="point">World position to check.</param>
    /// <returns>True if the ward's shield dome is active; otherwise false.</returns>
    public static bool IsWardShieldActive(Vector3 point)
    {
#if ! API
        PrivateArea ward = WardQueryService.GetCoveringWard(point);
        if (ward != null)
            return ward.IsEnabled();

        KnownWard known = WardRegistryManager.Instance.GetWardCoveringPoint(point);
        return known != null && known.IsEnabled;
#else
        return false;
#endif
    }

    /// <summary>
    /// Retrieves the protection radius in meters of the ward covering the specified position.
    /// </summary>
    /// <param name="point">World position to check.</param>
    /// <returns>Ward radius in meters (defaults to 32.0m if not found).</returns>
    public static float GetWardRadius(Vector3 point)
    {
#if ! API
        PrivateArea ward = WardQueryService.GetCoveringWard(point);
        if (ward != null)
            return WardQueryService.GetWardRadius(ward);

        KnownWard known = WardRegistryManager.Instance.GetWardCoveringPoint(point);
        return known != null ? known.Radius : 32.0f;
#else
        return 32.0f;
#endif
    }

    /// <summary>
    /// Retrieves the custom name assigned to the ward covering the specified position.
    /// </summary>
    /// <param name="point">World position to check.</param>
    /// <returns>Custom ward name string, or empty string if not found.</returns>
    public static string GetWardName(Vector3 point)
    {
#if ! API
        ZDO zdo = WardQueryService.GetCoveringWardZdo(point);
        if (zdo != null)
        {
            return zdo.GetString(WardZdoKeys.CustomName, "Ward Stone");
        }
        return string.Empty;
#else
        return string.Empty;
#endif
    }

    /// <summary>
    /// Retrieves the active PrivateArea component covering the specified position if currently loaded.
    /// </summary>
    /// <param name="point">World position to check.</param>
    /// <returns>PrivateArea instance if loaded within scene; otherwise null.</returns>
    public static PrivateArea GetCoveringWard(Vector3 point)
    {
#if ! API
        return WardQueryService.GetCoveringWard(point);
#else
        return null;
#endif
    }
}
