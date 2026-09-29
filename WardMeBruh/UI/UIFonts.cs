using TMPro;
using UnityEngine;
using Jotunn.Managers;

namespace WardMeBruh.UI;

public static class UIFonts
{
    private static TMP_FontAsset _primaryFont;

    public static TMP_FontAsset GetPrimaryFont()
    {
        if (_primaryFont != null)
        {
            return _primaryFont;
        }

        if (Hud.instance != null && Hud.instance.m_hoverName != null && Hud.instance.m_hoverName.font != null)
        {
            _primaryFont = Hud.instance.m_hoverName.font;
        }

        if (_primaryFont == null)
        {
            TMP_FontAsset[] fonts = Resources.FindObjectsOfTypeAll<TMP_FontAsset>();
            for (int i = 0; i < fonts.Length; i++)
            {
                TMP_FontAsset candidate = fonts[i];
                if (candidate != null && candidate.name.Contains("Valheim-Averia-Serif-Bold"))
                {
                    _primaryFont = candidate;
                    break;
                }
            }
        }

        if (_primaryFont == null)
        {
            TMP_FontAsset[] fonts = Resources.FindObjectsOfTypeAll<TMP_FontAsset>();
            for (int i = 0; i < fonts.Length; i++)
            {
                TMP_FontAsset candidate = fonts[i];
                if (candidate != null && candidate.name.Contains("Averia"))
                {
                    _primaryFont = candidate;
                    break;
                }
            }
        }

        if (_primaryFont == null && GUIManager.Instance != null && GUIManager.Instance.TMP_AveriaSansLibre != null)
        {
            _primaryFont = GUIManager.Instance.TMP_AveriaSansLibre;
        }

        if (_primaryFont == null)
        {
            _primaryFont = TMP_Settings.defaultFontAsset;
        }

        if (_primaryFont != null)
        {
            TMP_Settings.defaultFontAsset = _primaryFont;
        }

        return _primaryFont;
    }
}
