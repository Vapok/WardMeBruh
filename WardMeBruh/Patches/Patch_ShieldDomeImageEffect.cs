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

        if (mainCount == 0 || subCount == 0)
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

        ShieldDomeImageEffect.ShieldDome[] mainDomes = new ShieldDomeImageEffect.ShieldDome[mainCount];
        ShieldDomeImageEffect.ShieldDome[] subDomes = new ShieldDomeImageEffect.ShieldDome[subCount];
        int mIdx = 0;
        int sIdx = 0;
        foreach (KeyValuePair<ShieldGenerator, ShieldDomeImageEffect.ShieldDome> kvp in domeData)
        {
            if (WardForcefieldDome.SubWardGenerators.Contains(kvp.Key))
            {
                subDomes[sIdx++] = kvp.Value;
            }
            else
            {
                mainDomes[mIdx++] = kvp.Value;
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
        Texture2D gradTex = _gradientTexField?.GetValue(__instance) as Texture2D;
        if (gradTex != null)
        {
            mat.SetTexture(_ShieldColorGradient, gradTex);
        }
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

        if (_mainDomeBuffer == null || _mainDomeBuffer.count != mainDomes.Length)
        {
            _mainDomeBuffer?.Release();
            _mainDomeBuffer = new ComputeBuffer(mainDomes.Length, 24, ComputeBufferType.Structured);
        }
        _mainDomeBuffer.SetData(mainDomes);
        mat.SetBuffer(_DomeBuffer, _mainDomeBuffer);
        mat.SetInt(_DomeCount, mainDomes.Length);

        RenderTexture temp = RenderTexture.GetTemporary(src.width, src.height, 0, src.format);
        Graphics.Blit(src, temp, mat, 0);

        if (_subDomeBuffer == null || _subDomeBuffer.count != subDomes.Length)
        {
            _subDomeBuffer?.Release();
            _subDomeBuffer = new ComputeBuffer(subDomes.Length, 24, ComputeBufferType.Structured);
        }
        _subDomeBuffer.SetData(subDomes);
        mat.SetBuffer(_DomeBuffer, _subDomeBuffer);
        mat.SetInt(_DomeCount, subDomes.Length);

        Graphics.Blit(temp, dest, mat, 0);
        RenderTexture.ReleaseTemporary(temp);

        return false;
    }

    [HarmonyPatch("OnDestroy")]
    [HarmonyPostfix]
    private static void OnDestroyPostfix()
    {
        _mainDomeBuffer?.Release();
        _mainDomeBuffer = null;
        _subDomeBuffer?.Release();
        _subDomeBuffer = null;
    }
}
