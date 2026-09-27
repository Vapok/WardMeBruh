using UnityEngine;

namespace WardMeBruh.Models;

public class KnownExtensionWard
{
    public ZDOID Id { get; set; }
    public ZDOID ParentId { get; set; }
    public Vector3 Position { get; set; }
    public float Radius { get; set; }
    public bool IsEnabled { get; set; }

    public KnownExtensionWard(ZDOID id, ZDOID parentId, Vector3 position, float radius, bool isEnabled)
    {
        Id = id;
        ParentId = parentId;
        Position = position;
        Radius = radius;
        IsEnabled = isEnabled;
    }

    public void Pack(ZPackage pkg)
    {
        pkg.Write(Id);
        pkg.Write(ParentId);
        pkg.Write(Position);
        pkg.Write(Radius);
        pkg.Write(IsEnabled);
    }

    public static KnownExtensionWard Unpack(ZPackage pkg)
    {
        ZDOID id = pkg.ReadZDOID();
        ZDOID parentId = pkg.ReadZDOID();
        Vector3 position = pkg.ReadVector3();
        float radius = pkg.ReadSingle();
        bool isEnabled = pkg.ReadBool();
        return new KnownExtensionWard(id, parentId, position, radius, isEnabled);
    }
}

