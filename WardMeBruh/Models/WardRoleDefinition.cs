using System;
using System.Collections.Generic;

namespace WardMeBruh.Models;

public class WardRoleDefinition
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int Level { get; set; }
    public string ColorHex { get; set; }
    public HashSet<string> ExplicitPermissions { get; } = new(StringComparer.OrdinalIgnoreCase);

    public WardRoleDefinition(int id, string name, int level, string colorHex)
    {
        Id = id;
        Name = name;
        Level = level;
        ColorHex = colorHex;
    }

    public bool HasExplicitPermission(string permissionId)
    {
        if (string.IsNullOrEmpty(permissionId))
            return false;

        return ExplicitPermissions.Contains(permissionId);
    }

    public void AddPermission(string permissionId)
    {
        if (!string.IsNullOrEmpty(permissionId))
            ExplicitPermissions.Add(permissionId);
    }

    public void RemovePermission(string permissionId)
    {
        if (!string.IsNullOrEmpty(permissionId))
            ExplicitPermissions.Remove(permissionId);
    }
}
