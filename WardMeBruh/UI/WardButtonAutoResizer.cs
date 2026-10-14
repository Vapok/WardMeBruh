using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace WardMeBruh.UI;

public class WardButtonAutoResizer : MonoBehaviour
{
    private RectTransform _rectTransform;
    private LayoutElement _layoutElement;
    private TextMeshProUGUI _tmp;
    private float _minWidth;
    private float _padding = 24f;
    private string _lastText;
    private float _lastFontSize;

    public float MinWidth
    {
        get => _minWidth;
        set
        {
            _minWidth = value;
            RefreshWidth();
        }
    }

    public float Padding
    {
        get => _padding;
        set
        {
            _padding = value;
            RefreshWidth();
        }
    }

    public void Initialize(RectTransform rt, TextMeshProUGUI tmp, float minWidth, float padding = 24f)
    {
        _rectTransform = rt;
        _tmp = tmp;
        _minWidth = minWidth;
        _padding = padding;

        if (_tmp != null)
        {
            _lastText = _tmp.text;
            _lastFontSize = _tmp.fontSize;
        }

        RefreshWidth();
    }

    public void RefreshWidth()
    {
        if (_rectTransform == null)
        {
            _rectTransform = GetComponent<RectTransform>();
            if (_rectTransform == null)
                return;
        }

        if (_tmp == null)
        {
            _tmp = GetComponentInChildren<TextMeshProUGUI>(true);
            if (_tmp == null)
                return;
        }

        if (_layoutElement == null)
        {
            _layoutElement = GetComponent<LayoutElement>();
            if (_layoutElement == null && transform.parent != null && transform.parent.GetComponent<LayoutGroup>() != null)
            {
                _layoutElement = gameObject.AddComponent<LayoutElement>();
                _layoutElement.flexibleWidth = 0f;
                _layoutElement.flexibleHeight = 0f;
                _layoutElement.preferredHeight = _rectTransform.sizeDelta.y;
                _layoutElement.minHeight = _rectTransform.sizeDelta.y;
            }
        }

        float textWidth = _tmp.GetPreferredValues(_tmp.text, float.PositiveInfinity, float.PositiveInfinity).x;
        float requiredWidth = Mathf.Ceil(textWidth + _padding);
        float targetWidth = Mathf.Max(_minWidth, requiredWidth);

        bool changed = false;
        if (Mathf.Abs(_rectTransform.sizeDelta.x - targetWidth) > 0.5f)
        {
            _rectTransform.sizeDelta = new Vector2(targetWidth, _rectTransform.sizeDelta.y);
            changed = true;
        }

        if (_layoutElement != null)
        {
            if (Mathf.Abs(_layoutElement.preferredWidth - targetWidth) > 0.5f || Mathf.Abs(_layoutElement.minWidth - targetWidth) > 0.5f)
            {
                _layoutElement.preferredWidth = targetWidth;
                _layoutElement.minWidth = targetWidth;
                changed = true;
            }
        }

        if (changed && transform.parent != null)
        {
            RectTransform parentRt = transform.parent.GetComponent<RectTransform>();
            if (parentRt != null)
            {
                LayoutRebuilder.MarkLayoutForRebuild(parentRt);
            }
        }
    }

    private void LateUpdate()
    {
        if (_tmp == null || _rectTransform == null)
            return;

        if (_tmp.text != _lastText || !Mathf.Approximately(_tmp.fontSize, _lastFontSize))
        {
            _lastText = _tmp.text;
            _lastFontSize = _tmp.fontSize;
            RefreshWidth();
        }
    }
}
