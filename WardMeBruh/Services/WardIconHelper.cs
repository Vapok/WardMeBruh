using UnityEngine;

namespace WardMeBruh.Services;

public static class WardIconHelper
{
    private static Sprite _cachedIcon;

    public static Sprite GetWardIcon()
    {
        if (_cachedIcon != null)
            return _cachedIcon;

        if (PrivateArea.m_allAreas != null && PrivateArea.m_allAreas.Count > 0)
        {
            for (int i = 0; i < PrivateArea.m_allAreas.Count; i++)
            {
                PrivateArea area = PrivateArea.m_allAreas[i];
                if (area != null)
                {
                    Piece piece = area.GetComponent<Piece>();
                    if (piece != null && piece.m_icon != null)
                    {
                        _cachedIcon = piece.m_icon;
                        return _cachedIcon;
                    }
                }
            }
        }

        if (ZNetScene.instance != null && ZNetScene.instance.m_prefabs != null)
        {
            for (int i = 0; i < ZNetScene.instance.m_prefabs.Count; i++)
            {
                GameObject go = ZNetScene.instance.m_prefabs[i];
                if (go == null)
                    continue;

                PrivateArea area = go.GetComponent<PrivateArea>();
                if (area != null)
                {
                    Piece piece = go.GetComponent<Piece>();
                    if (piece != null && piece.m_icon != null)
                    {
                        _cachedIcon = piece.m_icon;
                        return _cachedIcon;
                    }
                }
            }
        }

        return null;
    }
}
