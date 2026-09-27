namespace WardMeBruh.Models;

public class FeaturePermission
{
    public string Id { get; }
    public string DisplayName { get; }
    public WardCapabilityCategory Category { get; }
    public string Description { get; }
    public WardRole DefaultRole { get; }
    public int SortOrder { get; }

    public FeaturePermission(string id, string displayName, WardCapabilityCategory category, string description, WardRole defaultRole, int sortOrder = 0)
    {
        Id = id;
        DisplayName = displayName;
        Category = category;
        Description = description;
        DefaultRole = defaultRole;
        SortOrder = sortOrder;
    }
}
