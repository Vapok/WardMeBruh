using UnityEngine;
using WardMeBruh.Configuration;
using WardMeBruh.Models;
using WardMeBruh.Services;
using WardMeBruh.UI;

namespace WardMeBruh.Components;

public class WardForcefieldDome : MonoBehaviour, IHitProjectile
{
    private static ShieldDomeImageEffect _shieldDomeEffect;

    private PrivateArea _area;
    private SphereCollider _domeCollider;
    private ShieldGenerator _proxyGenerator;
    private CircleProjector _areaMarker;
    private float _lastRadius = -1f;
    private float _lastHitTime;
    private bool _isDomeRegistered;
    private float _lastTintTime;

    private void Awake()
    {
        _area = GetComponent<PrivateArea>();
        _areaMarker = _area != null ? _area.m_areaMarker : GetComponentInChildren<CircleProjector>(true);
        SetupCollider();
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

    private void SetupCollider()
    {
        _domeCollider = gameObject.AddComponent<SphereCollider>();
        _domeCollider.isTrigger = true;
        _domeCollider.center = Vector3.zero;
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
        UnregisterDome();
        if (_areaMarker != null && _areaMarker.gameObject != null)
        {
            _areaMarker.gameObject.SetActive(false);
        }
    }

    private void UpdateVisuals()
    {
        if (_area == null || _domeCollider == null)
            return;

        bool modEnabled = ConfigRegistry.Enabled == null || ConfigRegistry.Enabled.Value;
        bool isWardActive = _area.IsEnabled() && modEnabled;

        float currentRadius = WardQueryService.GetWardRadius(_area);
        if (!Mathf.Approximately(_lastRadius, currentRadius))
        {
            _lastRadius = currentRadius;
            _domeCollider.radius = currentRadius;
            if (_areaMarker != null)
            {
                _areaMarker.m_radius = currentRadius;
                _areaMarker.m_nrOfSegments = Mathf.Clamp((int)(currentRadius * 1.5f), 30, 180);
            }
        }

        _domeCollider.enabled = isWardActive;

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
            _shieldDomeEffect = Object.FindFirstObjectByType<ShieldDomeImageEffect>();
        }

        if (_shieldDomeEffect == null || _proxyGenerator == null)
            return;

        if (active)
        {
            _shieldDomeEffect.SetShieldData(_proxyGenerator, transform.position, radius, 1.0f, _lastHitTime);
            _isDomeRegistered = true;
        }
        else if (_isDomeRegistered)
        {
            UnregisterDome();
        }
    }

    private void UnregisterDome()
    {
        if (_shieldDomeEffect != null && _proxyGenerator != null)
        {
            _shieldDomeEffect.RemoveShield(_proxyGenerator);
        }
        _isDomeRegistered = false;
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

            if (Time.time - _lastTintTime >= 1.0f)
            {
                _lastTintTime = Time.time;
                ApplyAetherIceTint();
            }
        }
        else
        {
            if (_areaMarker.gameObject.activeSelf)
            {
                _areaMarker.gameObject.SetActive(false);
            }
        }
    }

    private void ApplyAetherIceTint()
    {
        if (_areaMarker == null)
            return;

        Renderer[] renderers = _areaMarker.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer r = renderers[i];
            if (r == null)
                continue;

            Material mat = r.material;
            if (mat != null)
            {
                mat.color = WardColorPalette.AetherIce;
                if (mat.HasProperty("_EmissionColor"))
                {
                    mat.SetColor("_EmissionColor", WardColorPalette.AetherIceGlow);
                }
            }
        }

        Light[] lights = _areaMarker.GetComponentsInChildren<Light>(true);
        for (int i = 0; i < lights.Length; i++)
        {
            if (lights[i] != null)
            {
                lights[i].color = WardColorPalette.AetherIce;
            }
        }
    }

    public void TriggerHit()
    {
        _lastHitTime = Time.time;
        if (_isDomeRegistered && _shieldDomeEffect != null && _proxyGenerator != null)
        {
            _shieldDomeEffect.SetShieldData(_proxyGenerator, transform.position, _lastRadius, 1.0f, _lastHitTime);
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
