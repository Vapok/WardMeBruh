using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using WardMeBruh.Components;

namespace WardMeBruh.Patches;

[HarmonyPatch(typeof(ShieldDomeImageEffect))]
internal static class Patch_ShieldDomeImageEffect
{
    private static readonly int _DomeBuffer = Shader.PropertyToID("_DomeBuffer");
    private static readonly int _DomeCount = Shader.PropertyToID("_DomeCount");
    private static readonly int _TopLeft = Shader.PropertyToID("_TopLeft");
    private static readonly int _TopRight = Shader.PropertyToID("_TopRight");
    private static readonly int _BottomLeft = Shader.PropertyToID("_BottomLeft");
    private static readonly int _BottomRight = Shader.PropertyToID("_BottomRight");
    private static readonly int _Smoothing = Shader.PropertyToID("_Smoothing");
    private static readonly int _DepthFade = Shader.PropertyToID("_DepthFade");
    private static readonly int _EdgeGlow = Shader.PropertyToID("_EdgeGlow");
    private static readonly int _DrawDistance = Shader.PropertyToID("_DrawDistance");
    private static readonly int _ShieldColorGradient = Shader.PropertyToID("_ShieldColorGradient");
    private static readonly int _ShieldTime = Shader.PropertyToID("_ShieldTime");
    private static readonly int _NoiseTexture = Shader.PropertyToID("_NoiseTexture");
    private static readonly int _CurlNoiseTexture = Shader.PropertyToID("_CurlNoiseTexture");
    private static readonly int _NoiseSize = Shader.PropertyToID("_NoiseSize");
    private static readonly int _CurlNoiseSize = Shader.PropertyToID("_CurlNoiseSize");
    private static readonly int _CurlNoiseStrength = Shader.PropertyToID("_CurlNoiseStrength");
    private static readonly int _MaxSteps = Shader.PropertyToID("_MaxSteps");
    private static readonly int _SurfaceDistance = Shader.PropertyToID("_SurfaceDistance");
    private static readonly int _NormalBias = Shader.PropertyToID("_NormalBias");
    private static readonly int _RefractStrength = Shader.PropertyToID("_RefractStrength");

    private static readonly FieldInfo _effectMaterialField = AccessTools.Field(typeof(ShieldDomeImageEffect), "m_effectMaterial");
    private static readonly FieldInfo _shieldDomeDataField = AccessTools.Field(typeof(ShieldDomeImageEffect), "m_shieldDomeData");
    private static readonly FieldInfo _gradientTexField = AccessTools.Field(typeof(ShieldDomeImageEffect), "m_gradientTex");
    private static readonly FieldInfo _camField = AccessTools.Field(typeof(ShieldDomeImageEffect), "m_cam");

    private static ComputeBuffer _mainDomeBuffer;
    private static ComputeBuffer _subDomeBuffer;

    [HarmonyPatch("OnRenderImage")]
    [HarmonyPrefix]
    private static bool OnRenderImagePrefix(ShieldDomeImageEffect __instance, RenderTexture src, RenderTexture dest)
    {
        if (WardForcefieldDome.SubWardGenerators.Count == 0)
            return true;

        Dictionary<ShieldGenerator, ShieldDomeImageEffect.ShieldDome> domeData =
            _shieldDomeDataField?.GetValue(__instance) as Dictionary<ShieldGenerator, ShieldDomeImageEffect.ShieldDome>;

        if (domeData == null || domeData.Count == 0)
            return true;

        int mainCount = 0;
        int subCount = 0;
        foreach (KeyValuePair<ShieldGenerator, ShieldDomeImageEffect.ShieldDome> kvp in domeData)
        {
            if (WardForcefieldDome.SubWardGenerators.Contains(kvp.Key))
            {
                subCount++;
            }
            else
            {
                mainCount++;
            }
        }

        if (subCount == 0)
            return true;

        Material mat = _effectMaterialField?.GetValue(__instance) as Material;
        if (mat == null)
            return true;

        Camera cam = _camField?.GetValue(__instance) as Camera;
        if (cam == null)
        {
            cam = Camera.main;
            _camField?.SetValue(__instance, cam);
        }
        if (cam == null)
            return true;

        List<ShieldDomeImageEffect.ShieldDome> mainDomes = new(mainCount);
        Dictionary<Color, List<ShieldDomeImageEffect.ShieldDome>> subDomesByColor = new();

        foreach (KeyValuePair<ShieldGenerator, ShieldDomeImageEffect.ShieldDome> kvp in domeData)
        {
            if (WardForcefieldDome.SubWardGenerators.Contains(kvp.Key))
            {
                Color domeColor = WardForcefieldDome.GetDomeColor(kvp.Key);
                if (!subDomesByColor.TryGetValue(domeColor, out List<ShieldDomeImageEffect.ShieldDome> list))
                {
                    list = new List<ShieldDomeImageEffect.ShieldDome>();
                    subDomesByColor[domeColor] = list;
                }
                list.Add(kvp.Value);
            }
            else
            {
                mainDomes.Add(kvp.Value);
            }
        }

        Vector3 direction = cam.ViewportPointToRay(new Vector3(0f, 1f, 0f)).direction;
        Vector3 direction2 = cam.ViewportPointToRay(new Vector3(1f, 1f, 0f)).direction;
        Vector3 direction3 = cam.ViewportPointToRay(new Vector3(0f, 0f, 0f)).direction;
        Vector3 direction4 = cam.ViewportPointToRay(new Vector3(1f, 0f, 0f)).direction;
        mat.SetVector(_TopLeft, direction);
        mat.SetVector(_TopRight, direction2);
        mat.SetVector(_BottomLeft, direction3);
        mat.SetVector(_BottomRight, direction4);
        mat.SetFloat(_Smoothing, __instance.m_smoothing);
        mat.SetFloat(_DepthFade, __instance.m_depthFadeDistance);
        mat.SetFloat(_EdgeGlow, __instance.m_edgeGlowDistance);
        mat.SetFloat(_DrawDistance, __instance.m_drawDistance);
        mat.SetFloat(_ShieldTime, Time.time);
        mat.SetTexture(_NoiseTexture, __instance.m_noiseTexture);
        mat.SetTexture(_CurlNoiseTexture, __instance.m_curlNoiseTexture);
        mat.SetFloat(_NoiseSize, __instance.m_noiseSize);
        mat.SetFloat(_CurlNoiseSize, __instance.m_curlSize);
        mat.SetFloat(_CurlNoiseStrength, __instance.m_curlStrength);
        int maxSteps = 32 + (GraphicsSettingsManager.Instance != null
            ? GraphicsSettingsManager.Instance.ActiveSettings.m_lod * 32
            : 32);
        mat.SetInt(_MaxSteps, maxSteps);
        mat.SetFloat(_SurfaceDistance, __instance.m_surfaceDistance);
        mat.SetFloat(_NormalBias, __instance.m_normalBias);
        mat.SetFloat(_RefractStrength, __instance.m_refractStrength);

        Texture2D defaultGradTex = _gradientTexField?.GetValue(__instance) as Texture2D;

        int totalPasses = (mainDomes.Count > 0 ? 1 : 0) + subDomesByColor.Count;
        if (totalPasses == 1)
        {
            if (mainDomes.Count > 0)
            {
                if (defaultGradTex != null)
                {
                    mat.SetTexture(_ShieldColorGradient, defaultGradTex);
                }
                SetBufferData(ref _mainDomeBuffer, mainDomes.ToArray(), mat);
            }
            else
            {
                foreach (KeyValuePair<Color, List<ShieldDomeImageEffect.ShieldDome>> kvp in subDomesByColor)
                {
                    mat.SetTexture(_ShieldColorGradient, GetOrCreateGradientTexture(kvp.Key));
                    SetBufferData(ref _subDomeBuffer, kvp.Value.ToArray(), mat);
                    break;
                }
            }
            Graphics.Blit(src, dest, mat, 0);
            return false;
        }

        RenderTexture currentSource = RenderTexture.GetTemporary(src.width, src.height, 0, src.format);
        Graphics.Blit(src, currentSource);

        if (mainDomes.Count > 0)
        {
            if (defaultGradTex != null)
            {
                mat.SetTexture(_ShieldColorGradient, defaultGradTex);
            }
            SetBufferData(ref _mainDomeBuffer, mainDomes.ToArray(), mat);

            RenderTexture temp = RenderTexture.GetTemporary(src.width, src.height, 0, src.format);
            Graphics.Blit(currentSource, temp, mat, 0);
            RenderTexture.ReleaseTemporary(currentSource);
            currentSource = temp;
        }

        int subIndex = 0;
        foreach (KeyValuePair<Color, List<ShieldDomeImageEffect.ShieldDome>> kvp in subDomesByColor)
        {
            subIndex++;
            bool isLast = subIndex == subDomesByColor.Count;

            mat.SetTexture(_ShieldColorGradient, GetOrCreateGradientTexture(kvp.Key));
            SetBufferData(ref _subDomeBuffer, kvp.Value.ToArray(), mat);

            if (isLast)
            {
                Graphics.Blit(currentSource, dest, mat, 0);
            }
            else
            {
                RenderTexture temp = RenderTexture.GetTemporary(src.width, src.height, 0, src.format);
                Graphics.Blit(currentSource, temp, mat, 0);
                RenderTexture.ReleaseTemporary(currentSource);
                currentSource = temp;
            }
        }

        RenderTexture.ReleaseTemporary(currentSource);
        return false;
    }

    private static void SetBufferData(ref ComputeBuffer buffer, ShieldDomeImageEffect.ShieldDome[] data, Material mat)
    {
        if (buffer == null || buffer.count != data.Length)
        {
            buffer?.Release();
            buffer = new ComputeBuffer(data.Length, 24, ComputeBufferType.Structured);
        }
        buffer.SetData(data);
        mat.SetBuffer(_DomeBuffer, buffer);
        mat.SetInt(_DomeCount, data.Length);
    }

    private static readonly Dictionary<Color, Texture2D> _gradientTextureCache = new();

    public static Texture2D GetOrCreateGradientTexture(Color baseColor)
    {
        if (_gradientTextureCache.TryGetValue(baseColor, out Texture2D cached) && cached != null)
        {
            return cached;
        }

        Texture2D tex = new(256, 1, TextureFormat.RGB24, false);
        tex.name = $"ShieldGradient_{ColorUtility.ToHtmlStringRGB(baseColor)}";
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;

        Color highlight = Color.Lerp(baseColor, Color.white, 0.45f);
        Color mid = baseColor;
        Color rim = Color.Lerp(baseColor, Color.black, 0.25f);

        for (int i = 0; i < 256; i++)
        {
            float t = (float)i / 255f;
            Color c;
            if (t < 0.4f)
            {
                c = Color.Lerp(highlight, mid, t / 0.4f);
            }
            else
            {
                c = Color.Lerp(mid, rim, (t - 0.4f) / 0.6f);
            }
            tex.SetPixel(i, 0, c);
        }
        tex.Apply(false, true);
        _gradientTextureCache[baseColor] = tex;
        return tex;
    }

    [HarmonyPatch("OnDestroy")]
    [HarmonyPostfix]
    private static void OnDestroyPostfix()
    {
        _mainDomeBuffer?.Release();
        _mainDomeBuffer = null;
        _subDomeBuffer?.Release();
        _subDomeBuffer = null;

        foreach (KeyValuePair<Color, Texture2D> kvp in _gradientTextureCache)
        {
            if (kvp.Value != null)
            {
                Object.Destroy(kvp.Value);
            }
        }
        _gradientTextureCache.Clear();
    }
}
