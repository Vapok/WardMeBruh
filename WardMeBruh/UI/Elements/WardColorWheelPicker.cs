using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace WardMeBruh.UI.Elements;

public class WardColorWheelPicker : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    private static Texture2D _wheelTexture;

    public Action<Color> OnColorChanged;

    private RectTransform _rt;
    private RectTransform _crosshairRt;
    private float _currentHue = 0.55f;
    private float _currentSat = 0.8f;
    private float _currentVal = 1.0f;
    private float _wheelRadius = 75f;

    public float CurrentBrightness => _currentVal;

    public static Texture2D GetOrCreateWheelTexture()
    {
        if (_wheelTexture != null)
            return _wheelTexture;

        int size = 160;
        float radius = size * 0.5f;
        _wheelTexture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        _wheelTexture.wrapMode = TextureWrapMode.Clamp;
        _wheelTexture.filterMode = FilterMode.Bilinear;

        Color[] pixels = new Color[size * size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = x - radius + 0.5f;
                float dy = y - radius + 0.5f;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);

                if (dist > radius)
                {
                    pixels[y * size + x] = Color.clear;
                }
                else
                {
                    float sat = dist / radius;
                    float angle = Mathf.Atan2(dy, dx);
                    float hue = Mathf.Repeat((angle / (2f * Mathf.PI)) + 0.5f, 1f);
                    Color rgb = Color.HSVToRGB(hue, sat, 1.0f);

                    float alpha = Mathf.Clamp01(radius - dist + 0.5f);
                    pixels[y * size + x] = new Color(rgb.r, rgb.g, rgb.b, alpha);
                }
            }
        }

        _wheelTexture.SetPixels(pixels);
        _wheelTexture.Apply(false, true);
        return _wheelTexture;
    }

    public static GameObject Create(Transform parent, float size, Action<Color> onColorChanged, out WardColorWheelPicker pickerComponent)
    {
        GameObject wheelObj = new("ColorWheel", typeof(RectTransform), typeof(RawImage), typeof(WardColorWheelPicker));
        wheelObj.transform.SetParent(parent, false);

        RectTransform wheelRt = wheelObj.GetComponent<RectTransform>();
        wheelRt.sizeDelta = new Vector2(size, size);
        wheelRt.pivot = new Vector2(0.5f, 0.5f);

        RawImage rawImg = wheelObj.GetComponent<RawImage>();
        rawImg.texture = GetOrCreateWheelTexture();
        rawImg.raycastTarget = true;

        GameObject crosshairObj = new("Crosshair", typeof(RectTransform), typeof(Image));
        crosshairObj.transform.SetParent(wheelObj.transform, false);

        RectTransform chRt = crosshairObj.GetComponent<RectTransform>();
        chRt.sizeDelta = new Vector2(14f, 14f);
        chRt.pivot = new Vector2(0.5f, 0.5f);
        chRt.anchoredPosition = Vector2.zero;

        Image chImg = crosshairObj.GetComponent<Image>();
        chImg.color = Color.white;
        chImg.raycastTarget = false;

        GameObject innerObj = new("InnerDot", typeof(RectTransform), typeof(Image));
        innerObj.transform.SetParent(crosshairObj.transform, false);
        RectTransform inRt = innerObj.GetComponent<RectTransform>();
        inRt.anchorMin = Vector2.zero;
        inRt.anchorMax = Vector2.one;
        inRt.offsetMin = new Vector2(2f, 2f);
        inRt.offsetMax = new Vector2(-2f, -2f);

        Image inImg = innerObj.GetComponent<Image>();
        inImg.color = Color.black;
        inImg.raycastTarget = false;

        pickerComponent = wheelObj.GetComponent<WardColorWheelPicker>();
        pickerComponent._rt = wheelRt;
        pickerComponent._crosshairRt = chRt;
        pickerComponent._wheelRadius = size * 0.5f;
        pickerComponent.OnColorChanged = onColorChanged;

        return wheelObj;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        HandlePointer(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        HandlePointer(eventData);
    }

    private void HandlePointer(PointerEventData eventData)
    {
        if (_rt == null)
            return;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_rt, eventData.position, eventData.pressEventCamera, out Vector2 localPoint))
        {
            float dist = localPoint.magnitude;
            float sat = Mathf.Clamp01(dist / _wheelRadius);
            float angle = Mathf.Atan2(localPoint.y, localPoint.x);
            float hue = Mathf.Repeat((angle / (2f * Mathf.PI)) + 0.5f, 1f);

            _currentHue = hue;
            _currentSat = sat;

            float clampedDist = Mathf.Min(dist, _wheelRadius);
            Vector2 crosshairPos = new(Mathf.Cos(angle) * clampedDist, Mathf.Sin(angle) * clampedDist);
            if (_crosshairRt != null)
            {
                _crosshairRt.anchoredPosition = crosshairPos;
            }

            TriggerColorChanged();
        }
    }

    public void SetColor(Color c)
    {
        Color.RGBToHSV(c, out float h, out float s, out float v);
        _currentHue = h;
        _currentSat = s;
        _currentVal = Mathf.Max(0.2f, v);

        UpdateCrosshairPosition();
    }

    public void SetBrightness(float brightness)
    {
        _currentVal = Mathf.Clamp(brightness, 0.2f, 1.0f);
        TriggerColorChanged();
    }

    public Color GetColor()
    {
        return Color.HSVToRGB(_currentHue, _currentSat, _currentVal);
    }

    private void UpdateCrosshairPosition()
    {
        if (_crosshairRt == null)
            return;

        float angle = (_currentHue - 0.5f) * (2f * Mathf.PI);
        float dist = _currentSat * _wheelRadius;
        _crosshairRt.anchoredPosition = new Vector2(Mathf.Cos(angle) * dist, Mathf.Sin(angle) * dist);
    }

    private void TriggerColorChanged()
    {
        Color result = Color.HSVToRGB(_currentHue, _currentSat, _currentVal);
        OnColorChanged?.Invoke(result);
    }
}
