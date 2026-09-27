using UnityEngine;

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

    public KnownWard()
    {
        Name = string.Empty;
        CreatorName = string.Empty;
        Radius = 32f;
        IsEnabled = true;
    }

    public KnownWard(ZDOID id, string name, Vector3 position, long creatorId, string creatorName, float radius, bool isEnabled)
    {
        Id = id;
        Name = string.IsNullOrEmpty(name) ? "Unnamed Ward" : name;
        Position = position;
        CreatorId = creatorId;
        CreatorName = string.IsNullOrEmpty(creatorName) ? "Unknown" : creatorName;
        Radius = radius;
        IsEnabled = isEnabled;
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

        return new KnownWard(id, name, position, creatorId, creatorName, radius, isEnabled);
    }
}
