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

    public static GameObject CreateFocusReticle(Transform parent)
    {
        GameObject reticle = new("FocusReticle", typeof(RectTransform));
        reticle.transform.SetParent(parent, false);
        RectTransform rt = reticle.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = Vector2.zero;

        Color gold = new(1f, 0.82f, 0.2f, 1f);
        float thickness = 3.5f;

        CreateReticleBorder(reticle.transform, "Top", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -thickness), Vector2.zero, gold);
        CreateReticleBorder(reticle.transform, "Bottom", new Vector2(0f, 0f), new Vector2(1f, 0f), Vector2.zero, new Vector2(0f, thickness), gold);
        CreateReticleBorder(reticle.transform, "Left", new Vector2(0f, 0f), new Vector2(0f, 1f), Vector2.zero, new Vector2(thickness, 0f), gold);
        CreateReticleBorder(reticle.transform, "Right", new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(-thickness, 0f), Vector2.zero, gold);

        reticle.SetActive(false);
        return reticle;
    }

    private static void CreateReticleBorder(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, Color color)
    {
        GameObject border = new(name, typeof(RectTransform), typeof(Image));
        border.transform.SetParent(parent, false);
        RectTransform rt = border.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = offsetMin;
        rt.offsetMax = offsetMax;
        Image img = border.GetComponent<Image>();
        img.color = color;
        img.raycastTarget = false;
    }

    public static void AttachFocusReticle(GameObject reticle, Selectable selectable)
    {
        if (reticle == null)
            return;

        if (selectable == null || !selectable.gameObject.activeInHierarchy)
        {
            reticle.SetActive(false);
            return;
        }

        RectTransform selRt = selectable.transform as RectTransform;
        RectTransform reticleRt = reticle.GetComponent<RectTransform>();
        if (selRt == null || reticleRt == null)
        {
            reticle.SetActive(false);
            return;
        }

        RectTransform parentRt = reticle.transform.parent as RectTransform;
        if (parentRt == null)
        {
            reticle.SetActive(false);
            return;
        }

        Vector3[] corners = new Vector3[4];
        selRt.GetWorldCorners(corners);

        float minX = corners[0].x;
        float maxX = corners[2].x;
        float minY = corners[0].y;
        float maxY = corners[2].y;

        ScrollRect sr = selectable.GetComponentInParent<ScrollRect>();
        if (sr != null)
        {
            RectTransform vp = sr.viewport != null ? sr.viewport : (RectTransform)sr.transform;
            Vector3[] vpCorners = new Vector3[4];
            vp.GetWorldCorners(vpCorners);

            if (maxY <= vpCorners[0].y + 2f || minY >= vpCorners[2].y - 2f || maxX <= vpCorners[0].x || minX >= vpCorners[2].x)
            {
                reticle.SetActive(false);
                return;
            }

            minY = Mathf.Max(minY, vpCorners[0].y);
            maxY = Mathf.Min(maxY, vpCorners[2].y);
        }

        Vector3 worldMin = new Vector3(minX, minY, corners[0].z);
        Vector3 worldMax = new Vector3(maxX, maxY, corners[2].z);

        Vector3 localMin = parentRt.InverseTransformPoint(worldMin);
        Vector3 localMax = parentRt.InverseTransformPoint(worldMax);

        if (localMax.y <= parentRt.rect.yMin || localMin.y >= parentRt.rect.yMax)
        {
            reticle.SetActive(false);
            return;
        }

        reticleRt.anchorMin = new Vector2(0.5f, 0.5f);
        reticleRt.anchorMax = new Vector2(0.5f, 0.5f);
        reticleRt.pivot = new Vector2(0.5f, 0.5f);
        reticleRt.anchoredPosition = (localMin + localMax) * 0.5f;
        reticleRt.sizeDelta = new Vector2(Mathf.Abs(localMax.x - localMin.x) + 8f, Mathf.Abs(localMax.y - localMin.y) + 8f);
        reticle.transform.SetAsLastSibling();
        reticle.SetActive(true);
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
        List<Transform> cards = new();
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
        List<Selectable> result = new();
        if (root == null)
            return result;

        Selectable[] all = root.GetComponentsInChildren<Selectable>(false);
        for (int i = 0; i < all.Length; i++)
        {
            Selectable s = all[i];
            if (s == null || !s.gameObject.activeInHierarchy || !s.interactable)
                continue;

            if (s is Scrollbar)
                continue;

            Navigation nav = s.navigation;
            nav.mode = Navigation.Mode.None;
            s.navigation = nav;

            result.Add(s);
            ApplyGamepadColorBlock(s);
        }
        return result;
    }

    public static bool IsGamepadActive()
    {
        return ZInput.IsGamepadActive();
    }

    public static bool IsButtonDownA()
    {
        bool zDown = ZInput.GetButtonDown("JoyButtonA") || ZInput.GetButtonDown("JoyUse");
        bool key = Input.GetKeyDown(KeyCode.JoystickButton0);
        bool gpDown = UnityEngine.InputSystem.Gamepad.current != null && UnityEngine.InputSystem.Gamepad.current.buttonSouth.wasPressedThisFrame;
        return zDown || key || gpDown;
    }

    public static bool IsButtonDownB()
    {
        bool zDown = ZInput.GetButtonDown("JoyButtonB") || ZInput.GetButtonDown("JoyRadialClose");
        bool key = Input.GetKeyDown(KeyCode.JoystickButton1);
        bool gpDown = UnityEngine.InputSystem.Gamepad.current != null && UnityEngine.InputSystem.Gamepad.current.buttonEast.wasPressedThisFrame;
        return zDown || key || gpDown;
    }

    public static bool IsButtonDownX()
    {
        bool zDown = ZInput.GetButtonDown("JoyButtonX");
        bool key = Input.GetKeyDown(KeyCode.JoystickButton2);
        bool gpDown = UnityEngine.InputSystem.Gamepad.current != null && UnityEngine.InputSystem.Gamepad.current.buttonWest.wasPressedThisFrame;
        return zDown || key || gpDown;
    }

    public static bool IsButtonDownY()
    {
        bool zDown = ZInput.GetButtonDown("JoyButtonY") || ZInput.GetButtonDown("JoyInventory");
        bool key = Input.GetKeyDown(KeyCode.JoystickButton3);
        bool gpDown = UnityEngine.InputSystem.Gamepad.current != null && UnityEngine.InputSystem.Gamepad.current.buttonNorth.wasPressedThisFrame;
        return zDown || key || gpDown;
    }

    public static bool IsButtonDownLB()
    {
        bool zDown = ZInput.GetButtonDown("JoyLBumper") || ZInput.GetButtonDown("JoyTabLeft");
        bool key = Input.GetKeyDown(KeyCode.JoystickButton4);
        bool gpDown = UnityEngine.InputSystem.Gamepad.current != null && UnityEngine.InputSystem.Gamepad.current.leftShoulder.wasPressedThisFrame;
        return zDown || key || gpDown;
    }

    public static bool IsButtonDownRB()
    {
        bool zDown = ZInput.GetButtonDown("JoyRBumper") || ZInput.GetButtonDown("JoyTabRight");
        bool key = Input.GetKeyDown(KeyCode.JoystickButton5);
        bool gpDown = UnityEngine.InputSystem.Gamepad.current != null && UnityEngine.InputSystem.Gamepad.current.rightShoulder.wasPressedThisFrame;
        return zDown || key || gpDown;
    }

    public static bool IsButtonDownRS()
    {
        bool zDown = ZInput.GetButtonDown("JoyRStick");
        bool key = Input.GetKeyDown(KeyCode.JoystickButton9);
        bool gpDown = UnityEngine.InputSystem.Gamepad.current != null && UnityEngine.InputSystem.Gamepad.current.rightStickButton.wasPressedThisFrame;
        return zDown || key || gpDown;
    }

    public static bool IsButtonDownLS()
    {
        bool zDown = ZInput.GetButtonDown("JoyLStick");
        bool key = Input.GetKeyDown(KeyCode.JoystickButton8);
        bool gpDown = UnityEngine.InputSystem.Gamepad.current != null && UnityEngine.InputSystem.Gamepad.current.leftStickButton.wasPressedThisFrame;
        return zDown || key || gpDown;
    }

    public static bool GetDirectionInput(out bool up, out bool down, out bool left, out bool right)
    {
        up = false;
        down = false;
        left = false;
        right = false;

        bool dUp = ZInput.GetButtonDown("JoyDPadUp") || ZInput.GetButtonDown("JoyLStickUp");
        bool dDown = ZInput.GetButtonDown("JoyDPadDown") || ZInput.GetButtonDown("JoyLStickDown");
        bool dLeft = ZInput.GetButtonDown("JoyDPadLeft") || ZInput.GetButtonDown("JoyLStickLeft");
        bool dRight = ZInput.GetButtonDown("JoyDPadRight") || ZInput.GetButtonDown("JoyLStickRight");

        float stickX = ZInput.GetJoyLeftStickX();
        float stickY = ZInput.GetJoyLeftStickY();

        if (UnityEngine.InputSystem.Gamepad.current != null)
        {
            dUp |= UnityEngine.InputSystem.Gamepad.current.dpad.up.wasPressedThisFrame;
            dDown |= UnityEngine.InputSystem.Gamepad.current.dpad.down.wasPressedThisFrame;
            dLeft |= UnityEngine.InputSystem.Gamepad.current.dpad.left.wasPressedThisFrame;
            dRight |= UnityEngine.InputSystem.Gamepad.current.dpad.right.wasPressedThisFrame;

            Vector2 s = UnityEngine.InputSystem.Gamepad.current.leftStick.ReadValue();
            if (Mathf.Abs(s.x) > Mathf.Abs(stickX)) stickX = s.x;
            if (Mathf.Abs(s.y) > Mathf.Abs(stickY)) stickY = -s.y;
        }

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

    private static bool _lastLtPressed;
    private static bool _lastRtPressed;

    public static bool GetTriggerInput(out bool leftTrigger, out bool rightTrigger)
    {
        leftTrigger = false;
        rightTrigger = false;

        bool zLt = ZInput.GetButtonDown("JoyLTrigger") || ZInput.GetButtonDown("JoyAltKeys");
        bool zRt = ZInput.GetButtonDown("JoyRTrigger");

        bool gpLt = false;
        bool gpRt = false;
        if (UnityEngine.InputSystem.Gamepad.current != null)
        {
            bool currLt = UnityEngine.InputSystem.Gamepad.current.leftTrigger.ReadValue() > 0.4f;
            if (currLt && !_lastLtPressed)
            {
                gpLt = true;
            }
            _lastLtPressed = currLt;

            bool currRt = UnityEngine.InputSystem.Gamepad.current.rightTrigger.ReadValue() > 0.4f;
            if (currRt && !_lastRtPressed)
            {
                gpRt = true;
            }
            _lastRtPressed = currRt;
        }

        leftTrigger = zLt || gpLt;
        rightTrigger = zRt || gpRt;

        return leftTrigger || rightTrigger;
    }

    public static float GetRightStickScrollDelta()
    {
        if (UnityEngine.InputSystem.Gamepad.current != null)
        {
            Vector2 stick = UnityEngine.InputSystem.Gamepad.current.rightStick.ReadValue();
            if (Mathf.Abs(stick.y) > 0.15f)
            {
                return stick.y;
            }
        }

        if (ZInput.instance != null)
        {
            Vector2 joyStick = ZInput.GetJoyRightStick();
            if (Mathf.Abs(joyStick.y) > 0.15f)
            {
                return joyStick.y;
            }
        }

        return 0f;
    }

    public static void EnsureVisible(ScrollRect scrollRect, RectTransform target)
    {
        if (scrollRect == null || target == null || scrollRect.content == null)
            return;

        RectTransform viewport = scrollRect.viewport != null ? scrollRect.viewport : (RectTransform)scrollRect.transform;
        Vector3[] targetCorners = new Vector3[4];
        target.GetWorldCorners(targetCorners);

        Vector3 localTargetMin = viewport.InverseTransformPoint(targetCorners[0]);
        Vector3 localTargetMax = viewport.InverseTransformPoint(targetCorners[1]);

        float vpMinY = viewport.rect.yMin;
        float vpMaxY = viewport.rect.yMax;

        float contentHeight = scrollRect.content.rect.height;
        float vpHeight = viewport.rect.height;
        if (contentHeight <= vpHeight)
            return;

        float scrollableRange = contentHeight - vpHeight;
        if (scrollableRange <= 0.01f)
            return;

        if (localTargetMin.y < vpMinY)
        {
            float diff = (vpMinY - localTargetMin.y) + 16f;
            float normDiff = diff / scrollableRange;
            scrollRect.verticalNormalizedPosition = Mathf.Clamp01(scrollRect.verticalNormalizedPosition - normDiff);
        }
        else if (localTargetMax.y > vpMaxY)
        {
            float diff = (localTargetMax.y - vpMaxY) + 16f;
            float normDiff = diff / scrollableRange;
            scrollRect.verticalNormalizedPosition = Mathf.Clamp01(scrollRect.verticalNormalizedPosition + normDiff);
        }
    }
}
