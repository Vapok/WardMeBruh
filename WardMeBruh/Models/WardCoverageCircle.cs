using UnityEngine;

namespace WardMeBruh.Models;

public readonly struct WardCoverageCircle
{
    public readonly Vector2 Center;
    public readonly float Radius;

    public WardCoverageCircle(Vector2 center, float radius)
    {
        Center = center;
        Radius = radius;
    }

    public WardCoverageCircle(Vector3 position, float radius)
    {
        Center = new Vector2(position.x, position.z);
        Radius = radius;
    }
}
