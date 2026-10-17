using UnityEngine;
using WardMeBruh.UI;

namespace WardMeBruh.Models;

public class KnownWard
{
    public ZDOID Id { get; set; }
    public ZDOID ZdoId => Id;
    public string Name { get; set; }
    public string CustomName => Name;
    public Vector3 Position { get; set; }
    public long CreatorId { get; set; }
    public string CreatorName { get; set; }
    public float Radius { get; set; }
    public bool IsEnabled { get; set; }
    public WardType Type { get; set; } = WardType.Main;
    public ZDOID ParentId { get; set; } = ZDOID.None;

    public KnownWard()
    {
        Name = string.Empty;
        CreatorName = string.Empty;
        Radius = 32f;
        IsEnabled = true;
        Type = WardType.Main;
        ParentId = ZDOID.None;
    }

    public KnownWard(ZDOID id, string name, Vector3 position, long creatorId, string creatorName, float radius, bool isEnabled, WardType type = WardType.Main, ZDOID parentId = default)
    {
        Id = id;
        Name = string.IsNullOrEmpty(name) ? "$wmb_unnamed_ward".Localize() : name;
        Position = position;
        CreatorId = creatorId;
        CreatorName = string.IsNullOrEmpty(creatorName) ? "$wmb_creator_unknown".Localize() : creatorName;
        Radius = radius;
        IsEnabled = isEnabled;
        Type = type;
        ParentId = parentId;
    }

    public void Pack(ZPackage pkg)
    {
        pkg.Write(Id);
        pkg.Write(Name ?? string.Empty);
        pkg.Write(Position);
        pkg.Write(CreatorId);
        pkg.Write(CreatorName ?? string.Empty);
        pkg.Write(Radius);
        pkg.Write(IsEnabled);
        pkg.Write((int)Type);
        pkg.Write(ParentId);
    }

    public static KnownWard Unpack(ZPackage pkg)
    {
        ZDOID id = pkg.ReadZDOID();
        string name = pkg.ReadString();
        Vector3 position = pkg.ReadVector3();
        long creatorId = pkg.ReadLong();
        string creatorName = pkg.ReadString();
        float radius = pkg.ReadSingle();
        bool isEnabled = pkg.ReadBool();
        WardType type = WardType.Main;
        ZDOID parentId = ZDOID.None;
        if (pkg.GetArray() != null && pkg.Size() > pkg.GetPos())
        {
            type = (WardType)pkg.ReadInt();
            parentId = pkg.ReadZDOID();
        }

        return new KnownWard(id, name, position, creatorId, creatorName, radius, isEnabled, type, parentId);
    }
}
