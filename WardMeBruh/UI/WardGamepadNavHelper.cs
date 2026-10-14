using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace WardMeBruh.UI;

public static class WardGamepadNavHelper
{
    private static float _lastNavTime;
    private static float _lastTriggerTime;
    private const float NavInitialDelay = 0.32f;
    private const float NavRepeatRate = 0.12f;
    private const float TriggerDelay = 0.25f;
    private static string _activeNavDirection = string.Empty;

    public static readonly Color SelectedGlowColor = new Color(1f, 0.78f, 0.25f, 1f);
    public static readonly Color HighlightedGlowColor = new Color(1.15f, 0.9f, 0.4f, 1f);

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

            if (s is Scrollbar)
                continue;

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

        bool dUp = ZInput.GetButton("JoyDPadUp") || ZInput.GetButton("JoyLStickUp");
        bool dDown = ZInput.GetButton("JoyDPadDown") || ZInput.GetButton("JoyLStickDown");
        bool dLeft = ZInput.GetButton("JoyDPadLeft") || ZInput.GetButton("JoyLStickLeft");
        bool dRight = ZInput.GetButton("JoyDPadRight") || ZInput.GetButton("JoyLStickRight");

        string currentDir = string.Empty;
        if (dUp) currentDir = "up";
        else if (dDown) currentDir = "down";
        else if (dLeft) currentDir = "left";
        else if (dRight) currentDir = "right";

        if (string.IsNullOrEmpty(currentDir))
        {
            _activeNavDirection = string.Empty;
            return false;
        }

        float now = Time.unscaledTime;
        bool shouldFire = false;

        if (_activeNavDirection != currentDir)
        {
            _activeNavDirection = currentDir;
            _lastNavTime = now;
            shouldFire = true;
        }
        else
        {
            float elapsed = now - _lastNavTime;
            if (elapsed >= NavInitialDelay)
            {
                if (elapsed >= NavInitialDelay + NavRepeatRate)
                {
                    _lastNavTime = now - NavInitialDelay;
                    shouldFire = true;
                }
            }
        }

        if (shouldFire)
        {
            if (currentDir == "up") up = true;
            else if (currentDir == "down") down = true;
            else if (currentDir == "left") left = true;
            else if (currentDir == "right") right = true;
            return true;
        }

        return false;
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

        bool lt = ZInput.GetButtonDown("JoyLTrigger");
        bool rt = ZInput.GetButtonDown("JoyRTrigger");

        float now = Time.unscaledTime;
        if ((lt || rt) && (now - _lastTriggerTime >= TriggerDelay))
        {
            _lastTriggerTime = now;
            leftTrigger = lt;
            rightTrigger = rt;
            return true;
        }

        return false;
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
