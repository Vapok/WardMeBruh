using UnityEngine;

namespace WardMeBruh.Components;

public class FloatingRune : MonoBehaviour
{
    private Vector3 _basePos;
    private Vector3 _baseScale;
    private float _phaseOffset;

    private void Awake()
    {
        _basePos = transform.localPosition;
        _baseScale = transform.localScale;
        _phaseOffset = transform.position.x * 2.5f + transform.position.z * 2.5f;
    }

    private void LateUpdate()
    {
        float bob = Mathf.Sin(Time.time * 2.0f + _phaseOffset) * 0.04f;
        transform.localPosition = new Vector3(_basePos.x, _basePos.y + bob, _basePos.z);

        float pulse = 1f + Mathf.Sin(Time.time * 2.8f + _phaseOffset) * 0.04f;
        transform.localScale = _baseScale * pulse;

        Camera cam = GameCamera.instance != null && GameCamera.instance.m_camera != null 
            ? GameCamera.instance.m_camera 
            : Camera.main;

        if (cam != null)
        {
            transform.rotation = cam.transform.rotation;
        }
    }
}
