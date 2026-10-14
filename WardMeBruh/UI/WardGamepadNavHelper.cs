using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Valheim.SettingsGui;

namespace WardMeBruh.UI;

public static class WardGamepadNavHelper
{
    private static float _stickCooldown;
    private const float StickRepeatInterval = 0.20f;

    public static readonly Color SelectedGlowColor = new Color(1f, 0.78f, 0.25f, 1f);
    public static readonly Color HighlightedGlowColor = new Color(1.15f, 0.9f, 0.4f, 1f);

    public static string GetGamepadGlyph(string key)
    {
        if (ZInput.instance != null)
        {
            string bindingKey = key.StartsWith("Joy") ? key : "Joy" + key;
            string bound = ZInput.instance.GetBoundKeyString(bindingKey, true);
            if (!string.IsNullOrEmpty(bound))
            {
                return bound;
            }
        }

        string asset = "xbox";
        switch (ZInput.CurrentGlyph)
        {
            case GamepadGlyphs.Playstation:
                asset = "ps5";
                break;
            case GamepadGlyphs.Switch:
                asset = "switch2";
                break;
            case GamepadGlyphs.Auto:
                switch (ZInput.ConnectedGamepadType)
                {
                    case GamepadType.DualSense:
                    case GamepadType.DualShock:
                        asset = "ps5";
                        break;
                    case GamepadType.SwitchPro:
                    case GamepadType.JoyCon:
                    case GamepadType.JoyCon2Mouse:
                        asset = "switch2";
                        break;
                    default:
                        asset = "xbox";
                        break;
                }
                break;
            default:
                asset = "xbox";
                break;
        }

        string spriteName = key switch
        {
            "A" or "ButtonA" => asset == "ps5" ? "button_cross" : "button_a",
            "B" or "ButtonB" => asset == "ps5" ? "button_circle" : "button_b",
            "X" or "ButtonX" => asset == "ps5" ? "button_square" : "button_x",
            "Y" or "ButtonY" => asset == "ps5" ? "button_triangle" : "button_y",
            "LB" or "BumperL" => asset == "ps5" ? "button_l1" : (asset == "switch2" ? "lshoulder" : "button_lb"),
            "RB" or "BumperR" => asset == "ps5" ? "button_r1" : (asset == "switch2" ? "rshoulder" : "button_rb"),
            "LT" or "TriggerL" => asset == "ps5" ? "button_l2" : (asset == "switch2" ? "ltrigger" : "button_lt"),
            "RT" or "TriggerR" => asset == "ps5" ? "button_r2" : (asset == "switch2" ? "rtrigger" : "button_rt"),
            "RS" or "R3" => asset == "ps5" ? "button_r3" : (asset == "switch2" ? "rstick_pressed" : "button_rs"),
            "LS" or "L3" => asset == "ps5" ? "button_l3" : (asset == "switch2" ? "lstick_pressed" : "button_ls"),
            "DPad" => asset == "switch2" ? "dpad_blank" : "dpad",
            "RStick" => "rstick",
            "LStick" => "lstick",
            _ => key
        };

        return $"<sprite=\"{asset}\" name=\"{spriteName}\">";
    }

    public static void ApplyGamepadColorBlock(Selectable selectable)
    {
        if (selectable == null)
            return;

        ColorBlock cb = selectable.colors;
        cb.selectedColor = SelectedGlowColor;
        cb.highlightedColor = HighlightedGlowColor;
        selectable.colors = cb;
    }

    public static List<Transform> FindCards(Transform listContainer)
    {
        List<Transform> cards = new List<Transform>();
        if (listContainer == null)
            return cards;

        int childCount = listContainer.childCount;
        for (int i = 0; i < childCount; i++)
        {
            Transform child = listContainer.GetChild(i);
            if (child != null && child.gameObject.activeInHierarchy)
            {
                cards.Add(child);
            }
        }
        return cards;
    }

    public static List<Selectable> FindSelectables(GameObject root)
    {
        List<Selectable> result = new List<Selectable>();
        if (root == null)
            return result;

        Selectable[] all = root.GetComponentsInChildren<Selectable>(false);
        for (int i = 0; i < all.Length; i++)
        {
            Selectable s = all[i];
            if (s == null || !s.gameObject.activeInHierarchy || !s.interactable)
                continue;

            if (s is Scrollbar || s is InputField || s is TMP_InputField)
                continue;

            Navigation nav = s.navigation;
            nav.mode = Navigation.Mode.None;
            s.navigation = nav;

            result.Add(s);
            ApplyGamepadColorBlock(s);
        }
        return result;
    }

    public static bool GetDirectionInput(out bool up, out bool down, out bool left, out bool right)
    {
        up = false;
        down = false;
        left = false;
        right = false;

        if (!ZInput.IsGamepadActive())
            return false;

        bool dUp = ZInput.GetButtonDown("JoyDPadUp") || ZInput.GetButtonDown("JoyLStickUp");
        bool dDown = ZInput.GetButtonDown("JoyDPadDown") || ZInput.GetButtonDown("JoyLStickDown");
        bool dLeft = ZInput.GetButtonDown("JoyDPadLeft") || ZInput.GetButtonDown("JoyLStickLeft");
        bool dRight = ZInput.GetButtonDown("JoyDPadRight") || ZInput.GetButtonDown("JoyLStickRight");

        float stickX = ZInput.GetJoyLeftStickX();
        float stickY = ZInput.GetJoyLeftStickY();

        bool sUp = stickY < -0.45f;
        bool sDown = stickY > 0.45f;
        bool sLeft = stickX < -0.45f;
        bool sRight = stickX > 0.45f;

        float now = Time.unscaledTime;
        bool stickFired = false;
        if ((sUp || sDown || sLeft || sRight) && now >= _stickCooldown)
        {
            _stickCooldown = now + StickRepeatInterval;
            stickFired = true;
        }
        else if (!sUp && !sDown && !sLeft && !sRight)
        {
            _stickCooldown = 0f;
        }

        up = dUp || (stickFired && sUp);
        down = dDown || (stickFired && sDown);
        left = dLeft || (stickFired && sLeft);
        right = dRight || (stickFired && sRight);

        return up || down || left || right;
    }

    public static bool GetDirectionInput(out int navX, out int navY)
    {
        navX = 0;
        navY = 0;
        if (GetDirectionInput(out bool up, out bool down, out bool left, out bool right))
        {
            if (left) navX = -1;
            else if (right) navX = 1;
            if (down) navY = -1;
            else if (up) navY = 1;
            return true;
        }
        return false;
    }

    public static bool GetTriggerInput(out bool leftTrigger, out bool rightTrigger)
    {
        leftTrigger = false;
        rightTrigger = false;

        if (!ZInput.IsGamepadActive())
            return false;

        leftTrigger = ZInput.GetButtonDown("JoyLTrigger") || ZInput.GetButtonDown("JoyAltKeys");
        rightTrigger = ZInput.GetButtonDown("JoyRTrigger");

        return leftTrigger || rightTrigger;
    }

    public static void EnsureVisible(ScrollRect scrollRect, RectTransform target)
    {
        if (scrollRect == null || target == null || scrollRect.content == null)
            return;

        RectTransform viewport = scrollRect.viewport != null ? scrollRect.viewport : (RectTransform)scrollRect.transform;
        Vector3[] targetCorners = new Vector3[4];
        target.GetWorldCorners(targetCorners);

        Vector3[] viewportCorners = new Vector3[4];
        viewport.GetWorldCorners(viewportCorners);

        float targetMinY = targetCorners[0].y;
        float targetMaxY = targetCorners[1].y;
        float vpMinY = viewportCorners[0].y;
        float vpMaxY = viewportCorners[1].y;

        float contentHeight = scrollRect.content.rect.height;
        float vpHeight = viewport.rect.height;
        if (contentHeight <= vpHeight)
            return;

        float scrollableRange = contentHeight - vpHeight;
        if (scrollableRange <= 0.01f)
            return;

        if (targetMinY < vpMinY)
        {
            float diff = (vpMinY - targetMinY) + 24f;
            float normDiff = diff / scrollableRange;
            scrollRect.verticalNormalizedPosition = Mathf.Clamp01(scrollRect.verticalNormalizedPosition - normDiff);
        }
        else if (targetMaxY > vpMaxY)
        {
            float diff = (targetMaxY - vpMaxY) + 24f;
            float normDiff = diff / scrollableRange;
            scrollRect.verticalNormalizedPosition = Mathf.Clamp01(scrollRect.verticalNormalizedPosition + normDiff);
        }
    }
}
