using System.Collections.Generic;
using UnityEngine;
using WardMeBruh.Models;

namespace WardMeBruh.Components;

public static class RuneTextureFactory
{
    private static readonly Dictionary<WardRune, Texture2D> _cache = new();

    public static Texture2D GetRuneTexture(WardRune rune)
    {
        if (_cache.TryGetValue(rune, out Texture2D cached) && cached != null)
        {
            return cached;
        }

        const int size = 256;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.name = $"RuneTexture_{rune}";
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;

        Vector4[] segments = GetRuneSegments(rune);
        Color baseColor = WardAttunementStones.GetRuneColor(rune);

        const float coreRadius = 0.012f;
        const float bodyRadius = 0.055f;
        const float glowRadius = 0.095f;
        const float shadowRadius = 0.145f;

        Color coreHighlight = Color.Lerp(baseColor, Color.white, 0.40f);
        Color bodyColor = baseColor;
        Color glowColor = new Color(baseColor.r, baseColor.g, baseColor.b, 0.35f);
        Color shadowInner = new Color(0.02f, 0.02f, 0.02f, 0.60f);
        Color shadowOuter = new Color(0.01f, 0.01f, 0.01f, 0.0f);

        Color[] pixels = new Color[size * size];

        for (int y = 0; y < size; y++)
        {
            float v = (y + 0.5f) / size;
            for (int x = 0; x < size; x++)
            {
                float u = (x + 0.5f) / size;
                float minDist = float.MaxValue;

                for (int s = 0; s < segments.Length; s++)
                {
                    Vector4 seg = segments[s];
                    float d = DistToSegment(u, v, seg.x, seg.y, seg.z, seg.w);
                    if (d < minDist)
                    {
                        minDist = d;
                    }
                }

                Color col;
                if (minDist < coreRadius)
                {
                    float t = minDist / coreRadius;
                    col = Color.Lerp(coreHighlight, bodyColor, t);
                }
                else if (minDist < bodyRadius)
                {
                    col = bodyColor;
                }
                else if (minDist < glowRadius)
                {
                    float t = (minDist - bodyRadius) / (glowRadius - bodyRadius);
                    col = Color.Lerp(bodyColor, glowColor, t);
                }
                else if (minDist < shadowRadius)
                {
                    float t = (minDist - glowRadius) / (shadowRadius - glowRadius);
                    col = Color.Lerp(shadowInner, shadowOuter, t);
                }
                else
                {
                    col = Color.clear;
                }

                pixels[y * size + x] = col;
            }
        }

        tex.SetPixels(pixels);
        tex.Apply(false, true);

        _cache[rune] = tex;
        return tex;
    }

    private static float DistToSegment(float px, float py, float ax, float ay, float bx, float by)
    {
        float abx = bx - ax;
        float aby = by - ay;
        float dot = (px - ax) * abx + (py - ay) * aby;
        float lenSq = abx * abx + aby * aby;
        float t = Mathf.Clamp01(dot / lenSq);
        float cx = ax + t * abx;
        float cy = ay + t * aby;
        float dx = px - cx;
        float dy = py - cy;
        return Mathf.Sqrt(dx * dx + dy * dy);
    }

    private static Vector4[] GetRuneSegments(WardRune rune) => rune switch
    {
        WardRune.Fehu => new[]
        {
            new Vector4(0.35f, 0.12f, 0.35f, 0.88f),
            new Vector4(0.35f, 0.65f, 0.72f, 0.88f),
            new Vector4(0.35f, 0.42f, 0.70f, 0.65f)
        },
        WardRune.Uruz => new[]
        {
            new Vector4(0.30f, 0.12f, 0.30f, 0.88f),
            new Vector4(0.30f, 0.88f, 0.70f, 0.55f),
            new Vector4(0.70f, 0.55f, 0.70f, 0.12f)
        },
        WardRune.Thurisaz => new[]
        {
            new Vector4(0.35f, 0.12f, 0.35f, 0.88f),
            new Vector4(0.35f, 0.68f, 0.72f, 0.50f),
            new Vector4(0.72f, 0.50f, 0.35f, 0.32f)
        },
        WardRune.Isa => new[]
        {
            new Vector4(0.50f, 0.12f, 0.50f, 0.88f)
        },
        WardRune.Sowilo => new[]
        {
            new Vector4(0.68f, 0.88f, 0.32f, 0.64f),
            new Vector4(0.32f, 0.64f, 0.68f, 0.36f),
            new Vector4(0.68f, 0.36f, 0.32f, 0.12f)
        },
        WardRune.Laguz => new[]
        {
            new Vector4(0.35f, 0.12f, 0.35f, 0.88f),
            new Vector4(0.35f, 0.88f, 0.70f, 0.52f)
        },
        WardRune.Dagaz => new[]
        {
            new Vector4(0.30f, 0.12f, 0.30f, 0.88f),
            new Vector4(0.70f, 0.12f, 0.70f, 0.88f),
            new Vector4(0.30f, 0.88f, 0.70f, 0.12f),
            new Vector4(0.30f, 0.12f, 0.70f, 0.88f)
        },
        WardRune.Hagalaz => new[]
        {
            new Vector4(0.30f, 0.12f, 0.30f, 0.88f),
            new Vector4(0.70f, 0.12f, 0.70f, 0.88f),
            new Vector4(0.30f, 0.62f, 0.70f, 0.38f)
        },
        _ => new[]
        {
            new Vector4(0.50f, 0.12f, 0.50f, 0.88f)
        }
    };
}
