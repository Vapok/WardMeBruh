using System.Collections.Generic;
using UnityEngine;
using WardMeBruh.Configuration;
using WardMeBruh.Models;
using WardMeBruh.Services;
using WardMeBruh.UI;

namespace WardMeBruh.Components;

public class WardForcefieldDome : MonoBehaviour, IHitProjectile
{
    public static readonly List<WardForcefieldDome> Instances = new();
    public static readonly HashSet<ShieldGenerator> SubWardGenerators = new();

    private static ShieldDomeImageEffect _shieldDomeEffect;
    private static float _lastDomeEffectSearchTime = -1f;

    private PrivateArea _area;
    private ShieldGenerator _proxyGenerator;
    private CircleProjector _areaMarker;
    private float _lastRadius = -1f;
    private float _lastHitTime;
    private bool _isDomeRegistered;
    private float _lastRegisteredRadius = -1f;
    private Vector3 _lastRegisteredPos = Vector3.negativeInfinity;
    private float _lastRegisteredHitTime = -1f;
    private int _lastTintedSegmentCount = -1;
    private Color _lastAppliedColor = Color.clear;

    public bool IsActive => _area != null && _area.IsEnabled() && (ConfigRegistry.Enabled == null || ConfigRegistry.Enabled.Value);
    public float CurrentRadius => _lastRadius > 0f ? _lastRadius : (_area != null ? WardQueryService.GetWardRadius(_area) : 0f);
    public PrivateArea Area => _area;

    private void Awake()
    {
        Instances.Add(this);
        _area = GetComponent<PrivateArea>();
        _areaMarker = _area != null ? _area.m_areaMarker : GetComponentInChildren<CircleProjector>(true);
        SetupProxyGenerator();
    }

    private void SetupProxyGenerator()
    {
        _proxyGenerator = GetComponent<ShieldGenerator>();
        if (_proxyGenerator == null)
        {
            _proxyGenerator = gameObject.AddComponent<ShieldGenerator>();
            _proxyGenerator.enabled = false;
        }
    }

    private void Start()
    {
        UpdateVisuals();
    }

    private void Update()
    {
        UpdateVisuals();
    }

    private void OnDestroy()
    {
        Instances.Remove(this);
        if (_proxyGenerator != null)
        {
            SubWardGenerators.Remove(_proxyGenerator);
        }
        UnregisterDome();
        if (_areaMarker != null && _areaMarker.gameObject != null)
        {
            _areaMarker.gameObject.SetActive(false);
        }
        _lastTintedSegmentCount = -1;
        _lastAppliedColor = Color.clear;
    }

    private void UpdateVisuals()
    {
        if (_area == null)
            return;

        bool modEnabled = ConfigRegistry.Enabled == null || ConfigRegistry.Enabled.Value;
        bool isWardActive = _area.IsEnabled() && modEnabled;

        float currentRadius = WardQueryService.GetWardRadius(_area);
        if (!Mathf.Approximately(_lastRadius, currentRadius))
        {
            _lastRadius = currentRadius;
            if (_areaMarker != null)
            {
                _areaMarker.m_radius = currentRadius;
                _areaMarker.m_nrOfSegments = Mathf.Clamp((int)(currentRadius * 1.5f), 30, 180);
            }
        }

        ZDO zdo = (_area.m_nview != null && _area.m_nview.IsValid()) ? _area.m_nview.GetZDO() : null;
        bool showDome = isWardActive && (zdo == null || zdo.GetBool(WardZdoKeys.ShowShieldDome, true));
        bool showRing = isWardActive && (zdo == null || zdo.GetBool(WardZdoKeys.ShowPerimeterRing, true));

        Update3DDome(showDome, currentRadius);
        UpdatePerimeterRing(showRing, currentRadius);
    }

    private void Update3DDome(bool active, float radius)
    {
        if (Jotunn.Managers.GUIManager.IsHeadless())
            return;

        if (_shieldDomeEffect == null)
        {
            float now = Time.time;
            if (now - _lastDomeEffectSearchTime >= 2.0f)
            {
                _lastDomeEffectSearchTime = now;
                if (Camera.main != null)
                {
                    _shieldDomeEffect = Camera.main.GetComponent<ShieldDomeImageEffect>();
                }
                if (_shieldDomeEffect == null)
                {
                    _shieldDomeEffect = Object.FindFirstObjectByType<ShieldDomeImageEffect>();
                }
            }
        }

        if (_shieldDomeEffect == null || _proxyGenerator == null)
            return;

        if (active)
        {
            ZDO zdo = (_area.m_nview != null && _area.m_nview.IsValid()) ? _area.m_nview.GetZDO() : null;
            WardType wardType = (zdo != null) ? (WardType)zdo.GetInt(WardZdoKeys.WardType, (int)WardType.Main) : WardType.Main;
            if (wardType == WardType.SubWard)
            {
                SubWardGenerators.Add(_proxyGenerator);
            }
            else
            {
                SubWardGenerators.Remove(_proxyGenerator);
            }

            Vector3 currentPos = transform.position;
            if (!_isDomeRegistered ||
                !Mathf.Approximately(_lastRegisteredRadius, radius) ||
                _lastRegisteredPos != currentPos ||
                !Mathf.Approximately(_lastRegisteredHitTime, _lastHitTime))
            {
                _shieldDomeEffect.SetShieldData(_proxyGenerator, currentPos, radius, 1.0f, _lastHitTime);
                _lastRegisteredRadius = radius;
                _lastRegisteredPos = currentPos;
                _lastRegisteredHitTime = _lastHitTime;
                _isDomeRegistered = true;
            }
        }
        else if (_isDomeRegistered)
        {
            UnregisterDome();
        }
    }

    private void UnregisterDome()
    {
        if (_proxyGenerator != null)
        {
            SubWardGenerators.Remove(_proxyGenerator);
        }

        if (_shieldDomeEffect != null && _proxyGenerator != null)
        {
            _shieldDomeEffect.RemoveShield(_proxyGenerator);
        }
        _isDomeRegistered = false;
        _lastRegisteredRadius = -1f;
        _lastRegisteredPos = Vector3.negativeInfinity;
        _lastRegisteredHitTime = -1f;
    }

    private void UpdatePerimeterRing(bool active, float radius)
    {
        if (_areaMarker == null)
            return;

        if (active)
        {
            _area.CancelInvoke("HideMarker");
            if (!_areaMarker.gameObject.activeSelf)
            {
                _areaMarker.gameObject.SetActive(true);
            }

            UpdatePerimeterTint();
        }
        else
        {
            if (_areaMarker.gameObject.activeSelf)
            {
                _areaMarker.gameObject.SetActive(false);
            }
        }
    }

    private void UpdatePerimeterTint()
    {
        if (_areaMarker == null)
            return;

        Renderer[] renderers = _areaMarker.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0)
            return;

        ZDO zdo = (_area.m_nview != null && _area.m_nview.IsValid()) ? _area.m_nview.GetZDO() : null;
        WardType wardType = (zdo != null) ? (WardType)zdo.GetInt(WardZdoKeys.WardType, (int)WardType.Main) : WardType.Main;

        Color ringColor = (wardType == WardType.SubWard) ? WardColorPalette.AetherIce : WardColorPalette.GoldAmber;
        Color ringGlow = (wardType == WardType.SubWard) ? WardColorPalette.AetherIceGlow : new Color(1.2f, 0.85f, 0.2f, 1.0f);

        if (_lastTintedSegmentCount == renderers.Length && _lastAppliedColor == ringColor)
            return;

        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer r = renderers[i];
            if (r == null)
                continue;

            Material mat = r.material;
            if (mat != null)
            {
                mat.color = ringColor;
                if (mat.HasProperty("_EmissionColor"))
                {
                    mat.SetColor("_EmissionColor", ringGlow);
                }
            }
        }

        Light[] lights = _areaMarker.GetComponentsInChildren<Light>(true);
        for (int i = 0; i < lights.Length; i++)
        {
            if (lights[i] != null)
            {
                lights[i].color = ringColor;
            }
        }

        _lastTintedSegmentCount = renderers.Length;
        _lastAppliedColor = ringColor;
    }

    public void TriggerHit()
    {
        _lastHitTime = Time.time;
        if (_isDomeRegistered && _shieldDomeEffect != null && _proxyGenerator != null)
        {
            Vector3 currentPos = transform.position;
            _shieldDomeEffect.SetShieldData(_proxyGenerator, currentPos, _lastRadius, 1.0f, _lastHitTime);
            _lastRegisteredRadius = _lastRadius;
            _lastRegisteredPos = currentPos;
            _lastRegisteredHitTime = _lastHitTime;
        }
    }

    public bool OnProjectileHit(Character owner, ItemDrop.ItemData weapon, Projectile projectile, Collider collider, Vector3 hitPoint, bool water, Vector3 normal)
    {
        if (ConfigRegistry.Enabled != null && !ConfigRegistry.Enabled.Value)
            return true;

        if (_area == null || !_area.IsEnabled())
            return true;

        if (owner is Player player)
        {
            if (WardPermissions.HasWardAccess(_area, player.GetPlayerID()))
            {
                return true;
            }
        }

        TriggerHit();

        if (projectile != null && projectile.gameObject != null)
        {
            Destroy(projectile.gameObject);
        }

        return false;
    }
}
