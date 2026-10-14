using System;
using UnityEngine;
using WardMeBruh.Configuration;
using WardMeBruh.Models;
using WardMeBruh.Services;

namespace WardMeBruh.Components;

public class WardAttunementStones : MonoBehaviour
{
    private PrivateArea _area;
    private readonly GameObject[] _stoneInstances = new GameObject[8];
    private int _lastMask = -1;
    private bool _lastProgressionEnabled;
    private bool _lastShowStones;
    private float _checkTimer;

    private void Awake()
    {
        _area = GetComponent<PrivateArea>();
        if (ConfigRegistry.ShowPhysicalAttunementStones != null)
        {
            ConfigRegistry.ShowPhysicalAttunementStones.SettingChanged += OnSettingChanged;
        }
        if (ConfigRegistry.EnableProgressionSystem != null)
        {
            ConfigRegistry.EnableProgressionSystem.SettingChanged += OnSettingChanged;
        }
    }

    private void Start()
    {
        if (_area != null && _area.m_nview != null && _area.m_nview.IsValid())
        {
            int mask = WardProgressionService.GetAttunedMask(_area.m_nview.GetZDO());
            _lastMask = mask;
            _lastProgressionEnabled = ConfigRegistry.EnableProgressionSystem != null && ConfigRegistry.EnableProgressionSystem.Value;
            _lastShowStones = ConfigRegistry.ShowPhysicalAttunementStones != null && ConfigRegistry.ShowPhysicalAttunementStones.Value;
            UpdateStones(mask);
        }
    }

    private void OnDestroy()
    {
        if (ConfigRegistry.ShowPhysicalAttunementStones != null)
        {
            ConfigRegistry.ShowPhysicalAttunementStones.SettingChanged -= OnSettingChanged;
        }
        if (ConfigRegistry.EnableProgressionSystem != null)
        {
            ConfigRegistry.EnableProgressionSystem.SettingChanged -= OnSettingChanged;
        }
        ClearStones();
    }

    private void OnSettingChanged(object sender, EventArgs args)
    {
        _lastMask = -1;
        if (_area != null && _area.m_nview != null && _area.m_nview.IsValid())
        {
            int mask = WardProgressionService.GetAttunedMask(_area.m_nview.GetZDO());
            _lastMask = mask;
            _lastProgressionEnabled = ConfigRegistry.EnableProgressionSystem != null && ConfigRegistry.EnableProgressionSystem.Value;
            _lastShowStones = ConfigRegistry.ShowPhysicalAttunementStones != null && ConfigRegistry.ShowPhysicalAttunementStones.Value;
            UpdateStones(mask);
        }
    }

    private void Update()
    {
        _checkTimer += Time.deltaTime;
        if (_checkTimer < 0.5f)
            return;

        _checkTimer = 0f;

        if (_area == null || _area.m_nview == null || !_area.m_nview.IsValid())
            return;

        bool progressionEnabled = ConfigRegistry.EnableProgressionSystem != null && ConfigRegistry.EnableProgressionSystem.Value;
        bool showStonesConfig = ConfigRegistry.ShowPhysicalAttunementStones != null && ConfigRegistry.ShowPhysicalAttunementStones.Value;

        ZDO zdo = _area.m_nview.GetZDO();
        int mask = WardProgressionService.GetAttunedMask(zdo);
        if (mask != _lastMask || progressionEnabled != _lastProgressionEnabled || showStonesConfig != _lastShowStones)
        {
            _lastMask = mask;
            _lastProgressionEnabled = progressionEnabled;
            _lastShowStones = showStonesConfig;
            UpdateStones(mask);
        }
    }

    public void UpdateStones(int mask)
    {
        bool showStones = ConfigRegistry.ShowPhysicalAttunementStones != null &&
                          ConfigRegistry.ShowPhysicalAttunementStones.Value &&
                          ConfigRegistry.EnableProgressionSystem != null &&
                          ConfigRegistry.EnableProgressionSystem.Value;

        if (!showStones)
        {
            ClearStones();
            return;
        }

        const float ringRadius = 0.55f;
        GameObject prefab = GetTraderRunePrefab();

        for (int r = 0; r <= 7; r++)
        {
            bool isAttuned = (mask & (1 << r)) != 0;
            if (isAttuned)
            {
                if (_stoneInstances[r] == null)
                {
                    float angleDeg = r * (360f / 8f);
                    float angleRad = angleDeg * Mathf.Deg2Rad;
                    Vector3 offset = new Vector3(Mathf.Sin(angleRad) * ringRadius, 0.02f, Mathf.Cos(angleRad) * ringRadius);

                    GameObject stone = null;

                    if (prefab != null)
                    {
                        stone = UnityEngine.Object.Instantiate(prefab, transform, false);
                        stone.name = $"AttunementStone_{r}";

                        Component[] components = stone.GetComponentsInChildren<Component>(true);
                        for (int c = 0; c < components.Length; c++)
                        {
                            Component comp = components[c];
                            if (comp == null || comp is Transform || comp is MeshFilter || comp is MeshRenderer || comp is Renderer)
                                continue;
                            UnityEngine.Object.Destroy(comp);
                        }

                        stone.transform.localPosition = offset;
                        stone.transform.localRotation = Quaternion.Euler(0f, angleDeg + 180f, 0f);
                        stone.transform.localScale = new Vector3(0.35f, 0.35f, 0.35f);
                    }
                    else
                    {
                        stone = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                        stone.name = $"AttunementStone_{r}";
                        stone.transform.SetParent(transform, false);
                        stone.transform.localPosition = offset;
                        stone.transform.localRotation = Quaternion.Euler(0f, angleDeg, 0f);
                        stone.transform.localScale = new Vector3(0.16f, 0.22f, 0.16f);

                        Collider col = stone.GetComponent<Collider>();
                        if (col != null) UnityEngine.Object.Destroy(col);

                        Renderer rend = stone.GetComponent<Renderer>();
                        if (rend != null)
                        {
                            Renderer wardRend = _area != null ? _area.GetComponentInChildren<Renderer>() : null;
                            if (wardRend != null && wardRend.sharedMaterial != null)
                            {
                                rend.material = new Material(wardRend.sharedMaterial.shader);
                            }
                        }
                    }

                    Color runeCol = GetRuneColor((WardRune)r);
                    Renderer[] renderers = stone.GetComponentsInChildren<Renderer>(true);
                    for (int i = 0; i < renderers.Length; i++)
                    {
                        Renderer rend = renderers[i];
                        if (rend != null && rend.material != null)
                        {
                            Material mat = rend.material;
                            if (mat.HasProperty("_EmissionColor"))
                            {
                                mat.SetColor("_EmissionColor", runeCol * 3.5f);
                                mat.EnableKeyword("_EMISSION");
                            }
                        }
                    }

                    GameObject floatObj = GameObject.CreatePrimitive(PrimitiveType.Quad);
                    floatObj.name = $"FloatingRune_{(WardRune)r}";
                    Collider floatCol = floatObj.GetComponent<Collider>();
                    if (floatCol != null) UnityEngine.Object.Destroy(floatCol);

                    floatObj.transform.SetParent(stone.transform, false);
                    floatObj.transform.localPosition = new Vector3(0f, 1.80f, 0f);
                    floatObj.transform.localScale = new Vector3(0.78f, 0.78f, 0.78f);

                    Renderer floatRend = floatObj.GetComponent<Renderer>();
                    if (floatRend != null)
                    {
                        Shader runeShader = GetRuneShader();
                        Material runeMat = new Material(runeShader);
                        runeMat.mainTexture = RuneTextureFactory.GetRuneTexture((WardRune)r);
                        runeMat.color = Color.white;
                        floatRend.material = runeMat;
                    }

                    floatObj.AddComponent<FloatingRune>();

                    Light light = stone.AddComponent<Light>();
                    light.type = LightType.Point;
                    light.range = 2.0f;
                    light.intensity = 1.6f;
                    light.color = runeCol;

                    _stoneInstances[r] = stone;
                }
            }
            else
            {
                if (_stoneInstances[r] != null)
                {
                    Destroy(_stoneInstances[r]);
                    _stoneInstances[r] = null;
                }
            }
        }
    }

    private static GameObject _cachedTraderRunePrefab;

    private static GameObject GetTraderRunePrefab()
    {
        if (_cachedTraderRunePrefab != null)
            return _cachedTraderRunePrefab;

        foreach (AssetBundle loadedBundle in AssetBundle.GetAllLoadedAssetBundles())
        {
            if (loadedBundle.Contains("Assets/world/Props/TraderRune/TraderRune.prefab") || loadedBundle.Contains("TraderRune"))
            {
                _cachedTraderRunePrefab = loadedBundle.LoadAsset<GameObject>("Assets/world/Props/TraderRune/TraderRune.prefab") ??
                                          loadedBundle.LoadAsset<GameObject>("TraderRune");
                if (_cachedTraderRunePrefab != null)
                    return _cachedTraderRunePrefab;
            }
        }

        string bundlePath = System.IO.Path.Combine(Application.streamingAssetsPath, "SoftRef", "Bundles", "c32ede71");
        if (System.IO.File.Exists(bundlePath))
        {
            try
            {
                AssetBundle bundle = AssetBundle.LoadFromFile(bundlePath);
                if (bundle != null)
                {
                    _cachedTraderRunePrefab = bundle.LoadAsset<GameObject>("Assets/world/Props/TraderRune/TraderRune.prefab") ??
                                              bundle.LoadAsset<GameObject>("TraderRune");
                    if (_cachedTraderRunePrefab != null)
                        return _cachedTraderRunePrefab;
                }
            }
            catch (Exception ex)
            {
                WardMeBruh.Log.Debug($"[TraderRune] Bundle load: {ex.Message}");
            }
        }

        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        for (int i = 0; i < allObjects.Length; i++)
        {
            if (allObjects[i].name == "TraderRune")
            {
                _cachedTraderRunePrefab = allObjects[i];
                return _cachedTraderRunePrefab;
            }
        }

        if (ObjectDB.instance != null)
        {
            _cachedTraderRunePrefab = ObjectDB.instance.GetItemPrefab("Thunderstone");
        }

        return _cachedTraderRunePrefab;
    }

    private static Shader _cachedRuneShader;

    private static Shader GetRuneShader()
    {
        if (_cachedRuneShader != null)
            return _cachedRuneShader;

        Shader shader = Shader.Find("Sprites/Default");
        if (shader != null)
        {
            _cachedRuneShader = shader;
            return _cachedRuneShader;
        }

        shader = Shader.Find("Legacy Shaders/Particles/Alpha Blended") ??
                 Shader.Find("Unlit/Transparent") ??
                 Shader.Find("Standard");

        _cachedRuneShader = shader;
        return _cachedRuneShader;
    }

    private void ClearStones()
    {
        for (int i = 0; i < _stoneInstances.Length; i++)
        {
            if (_stoneInstances[i] != null)
            {
                Destroy(_stoneInstances[i]);
                _stoneInstances[i] = null;
            }
        }
    }

    public static Color GetRuneColor(WardRune rune) => rune switch
    {
        WardRune.Fehu => new Color(1.00f, 0.15f, 0.15f),
        WardRune.Uruz => new Color(1.00f, 0.52f, 0.05f),
        WardRune.Thurisaz => new Color(1.00f, 0.85f, 0.10f),
        WardRune.Isa => new Color(0.20f, 0.95f, 0.25f),
        WardRune.Sowilo => new Color(0.10f, 0.85f, 1.00f),
        WardRune.Laguz => new Color(0.25f, 0.48f, 1.00f),
        WardRune.Dagaz => new Color(0.68f, 0.25f, 1.00f),
        WardRune.Hagalaz => new Color(1.00f, 0.22f, 0.75f),
        _ => Color.white
    };
}
