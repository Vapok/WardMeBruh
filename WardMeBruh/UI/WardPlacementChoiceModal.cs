using Jotunn.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using WardMeBruh.Components;
using WardMeBruh.Configuration;
using WardMeBruh.Models;
using WardMeBruh.Networking;
using WardMeBruh.Services;

namespace WardMeBruh.UI;

public class WardPlacementChoiceModal : MonoBehaviour
{
    private static WardPlacementChoiceModal _instance;
    private GameObject _panel;
    private TextMeshProUGUI _titleText;
    private TextMeshProUGUI _descText;
    private PrivateArea _pendingWard;
    private PrivateArea _coveringWard;
    private Button _extendBtn;
    private Button _subwardBtn;
    private TextMeshProUGUI _extendBtnText;
    private TextMeshProUGUI _subwardBtnText;
    private GameObject _gamepadLegend;
    private TextMeshProUGUI _legendText;
    private bool _wasGamepadActive;
    private bool _isInputBlocked;
    private GameObject _focusReticle;
    private int _lastSubmitFrame = -1;

    public static WardPlacementChoiceModal Instance
    {
        get
        {
            if (_instance == null && !GUIManager.IsHeadless() && GUIManager.CustomGUIFront != null)
            {
                GameObject root = new GameObject("WardPlacementChoiceModal_Host");
                root.transform.SetParent(GUIManager.CustomGUIFront.transform, false);
                _instance = root.AddComponent<WardPlacementChoiceModal>();
            }
            return _instance;
        }
    }

    public bool IsOpen => _panel != null && _panel.activeSelf;
    public static bool HasInstanceAndOpen => _instance != null && _instance.IsOpen;

    private void Awake()
    {
        Build();
    }

    private void OnDestroy()
    {
        SetInputBlocked(false);
        if (_instance == this)
        {
            _instance = null;
        }
    }

    private void SetInputBlocked(bool block)
    {
        if (block)
        {
            if (!_isInputBlocked)
            {
                _isInputBlocked = true;
                GUIManager.BlockInput(true);
            }
        }
        else
        {
            _isInputBlocked = false;
            WardInputHelper.ForceUnblockInput();
        }
    }

    private void Update()
    {
        if (!IsOpen)
            return;

        bool isGamepad = WardGamepadNavHelper.IsGamepadActive();
        if (isGamepad != _wasGamepadActive)
        {
            UpdateKeyhints(isGamepad);
        }

        bool closeRequested = Input.GetKeyDown(KeyCode.Escape) ||
            ZInput.GetKeyDown(KeyCode.Escape) ||
            (UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.escapeKey.wasPressedThisFrame) ||
            WardGamepadNavHelper.IsButtonDownB() ||
            (ZInput.instance != null && (ZInput.GetButtonDown("JoyMenu") || ZInput.GetButtonDown("JoyRadialClose")));

        if (closeRequested)
        {
            OnSubWardClicked();
            return;
        }

        if (isGamepad)
        {
            if (WardGamepadNavHelper.IsButtonDownY() && _subwardBtn != null && _subwardBtn.interactable)
            {
                OnSubWardClicked();
                return;
            }

            if (WardGamepadNavHelper.GetDirectionInput(out int navX, out int navY))
            {
                if (navX < 0 && _extendBtn != null)
                {
                    _extendBtn.Select();
                    WardGamepadNavHelper.AttachFocusReticle(_focusReticle, _extendBtn);
                }
                else if (navX > 0 && _subwardBtn != null && _subwardBtn.interactable)
                {
                    _subwardBtn.Select();
                    WardGamepadNavHelper.AttachFocusReticle(_focusReticle, _subwardBtn);
                }
            }

            if (WardGamepadNavHelper.IsButtonDownA() && Time.frameCount != _lastSubmitFrame)
            {
                _lastSubmitFrame = Time.frameCount;
                GameObject currentObj = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
                if (currentObj != null)
                {
                    Button btn = currentObj.GetComponent<Button>();
                    if (btn != null && btn.interactable)
                    {
                        btn.onClick.Invoke();
                        return;
                    }
                }
                OnExtendClicked();
            }
        }
    }

    public void UpdateKeyhints(bool isGamepad)
    {
        _wasGamepadActive = isGamepad;
        if (_extendBtnText != null)
        {
            string aGlyph = WardGamepadNavHelper.GetGamepadGlyph("A");
            _extendBtnText.text = isGamepad 
                ? $"{aGlyph} " + "$wmb_placement_btn_extend".Localize() 
                : "$wmb_placement_btn_extend".Localize();
        }
        if (_subwardBtnText != null)
        {
            string yGlyph = WardGamepadNavHelper.GetGamepadGlyph("Y");
            _subwardBtnText.text = isGamepad 
                ? $"{yGlyph} " + "$wmb_placement_btn_subward".Localize() 
                : "$wmb_placement_btn_subward".Localize();
        }
        if (_gamepadLegend != null)
        {
            _gamepadLegend.SetActive(isGamepad);
            if (isGamepad)
            {
                TextMeshProUGUI legendText = _gamepadLegend.GetComponent<TextMeshProUGUI>();
                if (legendText != null)
                {
                    string a = WardGamepadNavHelper.GetGamepadGlyph("A");
                    string y = WardGamepadNavHelper.GetGamepadGlyph("Y");
                    string b = WardGamepadNavHelper.GetGamepadGlyph("B");
                    legendText.text = $"<color=#e2d6b5>{a} Extend   {y} Sub-Ward   {b} Cancel</color>";
                }
            }
        }
    }

    private void Build()
    {
        UIFonts.GetPrimaryFont();

        _panel = GUIManager.Instance.CreateWoodpanel(
            transform,
            new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f),
            Vector2.zero,
            520f,
            250f,
            draggable: true);
        _panel.name = "WardPlacementChoiceModal";

        GameObject titleObj = new GameObject("TitleText", typeof(RectTransform), typeof(TextMeshProUGUI), typeof(WardTextElement));
        titleObj.transform.SetParent(_panel.transform, false);
        RectTransform titleRt = titleObj.GetComponent<RectTransform>();
        titleRt.anchorMin = new Vector2(0f, 1f);
        titleRt.anchorMax = new Vector2(1f, 1f);
        titleRt.pivot = new Vector2(0.5f, 1f);
        titleRt.anchoredPosition = new Vector2(0f, -22f);
        titleRt.sizeDelta = new Vector2(460f, 32f);

        _titleText = titleObj.GetComponent<TextMeshProUGUI>();
        _titleText.text = "$wmb_placement_modal_title".Localize();
        _titleText.font = UIFonts.GetPrimaryFont();
        _titleText.fontSize = 20f;
        _titleText.color = WardColorPalette.ValheimOrange;
        _titleText.alignment = TextAlignmentOptions.Center;

        WardTextElement titleWte = titleObj.GetComponent<WardTextElement>();
        titleWte.baseFontSize = 20;

        GameObject descObj = new GameObject("DescText", typeof(RectTransform), typeof(TextMeshProUGUI), typeof(WardTextElement));
        descObj.transform.SetParent(_panel.transform, false);
        RectTransform descRt = descObj.GetComponent<RectTransform>();
        descRt.anchorMin = new Vector2(0f, 1f);
        descRt.anchorMax = new Vector2(1f, 1f);
        descRt.pivot = new Vector2(0.5f, 1f);
        descRt.anchoredPosition = new Vector2(0f, -65f);
        descRt.sizeDelta = new Vector2(460f, 55f);

        _descText = descObj.GetComponent<TextMeshProUGUI>();
        _descText.text = "$wmb_placement_modal_desc".Localize();
        _descText.font = UIFonts.GetPrimaryFont();
        _descText.fontSize = 15f;
        _descText.color = WardColorPalette.TextNormal;
        _descText.alignment = TextAlignmentOptions.Center;

        WardTextElement descWte = descObj.GetComponent<WardTextElement>();
        descWte.baseFontSize = 15;

        GameObject extendBtnObj = GUIManager.Instance.CreateButton(
            "$wmb_placement_btn_extend".Localize(),
            _panel.transform,
            new Vector2(0.5f, 0f),
            new Vector2(0.5f, 0f),
            new Vector2(-110f, 48f),
            200f,
            44f);
        extendBtnObj.GetComponent<RectTransform>().sizeDelta = new Vector2(200f, 44f);
        WardUIFactory.SetupButtonText(extendBtnObj, "$wmb_placement_btn_extend".Localize(), 15f, WardColorPalette.GoldAmber);
        _extendBtn = extendBtnObj.GetComponent<Button>();
        _extendBtnText = extendBtnObj.GetComponentInChildren<TextMeshProUGUI>();
        WardGamepadNavHelper.ApplyGamepadColorBlock(_extendBtn);
        _extendBtn.onClick.AddListener(OnExtendClicked);

        GameObject subwardBtnObj = GUIManager.Instance.CreateButton(
            "$wmb_placement_btn_subward".Localize(),
            _panel.transform,
            new Vector2(0.5f, 0f),
            new Vector2(0.5f, 0f),
            new Vector2(110f, 48f),
            200f,
            44f);
        subwardBtnObj.GetComponent<RectTransform>().sizeDelta = new Vector2(200f, 44f);
        WardUIFactory.SetupButtonText(subwardBtnObj, "$wmb_placement_btn_subward".Localize(), 15f, WardColorPalette.IceBlue);
        _subwardBtn = subwardBtnObj.GetComponent<Button>();
        _subwardBtnText = subwardBtnObj.GetComponentInChildren<TextMeshProUGUI>();
        WardGamepadNavHelper.ApplyGamepadColorBlock(_subwardBtn);
        _subwardBtn.onClick.AddListener(OnSubWardClicked);

        _gamepadLegend = new GameObject("GamepadLegend", typeof(RectTransform), typeof(TextMeshProUGUI));
        _gamepadLegend.transform.SetParent(_panel.transform, false);
        RectTransform legendRt = _gamepadLegend.GetComponent<RectTransform>();
        legendRt.anchorMin = new Vector2(0f, 0f);
        legendRt.anchorMax = new Vector2(1f, 0f);
        legendRt.pivot = new Vector2(0.5f, 1f);
        legendRt.offsetMin = new Vector2(20f, -26f);
        legendRt.offsetMax = new Vector2(-20f, -6f);

        _legendText = _gamepadLegend.GetComponent<TextMeshProUGUI>();
        _legendText.font = UIFonts.GetPrimaryFont();
        _legendText.text = "<color=#e2d6b5><b>[D-Pad]</b> Navigate   <b>[A]</b> Extend   <b>[Y]</b> Sub-Ward   <b>[B]</b> Cancel</color>";
        _legendText.fontSize = 12f;
        _legendText.alignment = TextAlignmentOptions.Center;
        _legendText.color = WardColorPalette.ValheimBeige;

        _focusReticle = WardGamepadNavHelper.CreateFocusReticle(_panel.transform);

        _panel.SetActive(false);
    }

    public void Open(PrivateArea pendingWard, PrivateArea coveringWard)
    {
        if (_panel == null)
            return;

        _pendingWard = pendingWard;
        _coveringWard = coveringWard;

        WardFontSizeMode fontMode = WardMainWindow.Instance != null 
            ? WardMainWindow.Instance.CurrentFontSizeMode 
            : (ConfigRegistry.GuiFontSize != null ? ConfigRegistry.GuiFontSize.Value : WardFontSizeMode.Small);
        WardMainWindow.ApplyFontSizeToHierarchy(_panel, fontMode);

        if (_subwardBtn != null && pendingWard != null && pendingWard.m_nview != null && pendingWard.m_nview.IsValid())
        {
            ZDO coveringZdo = coveringWard != null && coveringWard.m_nview != null && coveringWard.m_nview.IsValid() ? coveringWard.m_nview.GetZDO() : null;
            float maxSubRadius = WardQueryService.GetMaxAllowedSubWardRadius(pendingWard.m_nview.GetZDO(), pendingWard.transform.position, coveringZdo);
            _subwardBtn.interactable = maxSubRadius >= 1.0f;
        }

        bool isGamepad = WardGamepadNavHelper.IsGamepadActive();
        UpdateKeyhints(isGamepad);
        if (isGamepad && _extendBtn != null)
        {
            _extendBtn.Select();
            WardGamepadNavHelper.AttachFocusReticle(_focusReticle, _extendBtn);
        }
        else
        {
            WardGamepadNavHelper.AttachFocusReticle(_focusReticle, null);
        }

        _panel.SetActive(true);
        SetInputBlocked(true);
    }

    public void Close()
    {
        WardGamepadNavHelper.AttachFocusReticle(_focusReticle, null);
        if (_panel != null)
        {
            _panel.SetActive(false);
        }
        SetInputBlocked(false);
        _pendingWard = null;
        _coveringWard = null;
    }

    private void OnExtendClicked()
    {
        if (_pendingWard == null || _coveringWard == null)
        {
            Close();
            return;
        }

        ConfigureAsExtension(_pendingWard, _coveringWard);
        Close();
    }

    private void OnSubWardClicked()
    {
        if (_pendingWard == null || _coveringWard == null)
        {
            Close();
            return;
        }

        ConfigureAsSubWard(_pendingWard, _coveringWard);
        Close();
    }

    public static void ConfigureAsExtension(PrivateArea pendingWard, PrivateArea coveringWard)
    {
        if (pendingWard == null || pendingWard.m_nview == null || !pendingWard.m_nview.IsValid())
            return;

        if (coveringWard == null || coveringWard.m_nview == null || !coveringWard.m_nview.IsValid())
            return;

        pendingWard.m_nview.ClaimOwnership();
        ZDO zdo = pendingWard.m_nview.GetZDO();
        ZDO coveringZdo = coveringWard.m_nview.GetZDO();
        ZDO rootParentZdo = WardQueryService.GetEffectiveWardZdo(coveringZdo);
        if (rootParentZdo == null)
        {
            rootParentZdo = coveringZdo;
        }

        zdo.Set(WardZdoKeys.ParentWard, rootParentZdo.m_uid);
        zdo.Set(WardZdoKeys.WardType, (int)WardType.Extension);

        string parentName = rootParentZdo.GetString(WardZdoKeys.CustomName, coveringWard.m_name);
        string extName = $"{parentName} {"$wmb_ward_type_extension".Localize()}";
        zdo.Set(WardZdoKeys.CustomName, extName);

        float parentRadius = rootParentZdo.GetFloat(WardZdoKeys.Radius, 32.0f);
        zdo.Set(WardZdoKeys.Radius, parentRadius);
        if (pendingWard.m_areaMarker != null)
        {
            pendingWard.m_areaMarker.m_radius = parentRadius;
        }

        bool parentEnabled = coveringWard.IsEnabled();
        zdo.Set(ZDOVars.s_enabled, parentEnabled);

        int parentMask = rootParentZdo.GetInt(WardZdoKeys.AttunedRunes, 0);
        int parentProg = rootParentZdo.GetInt(WardZdoKeys.ProgressionLevel, 0);
        zdo.Set(WardZdoKeys.AttunedRunes, parentMask);
        zdo.Set(WardZdoKeys.ProgressionLevel, parentProg);

        WardAttunementStones stones = pendingWard.GetComponent<WardAttunementStones>();
        if (stones != null)
        {
            stones.UpdateStones(parentMask);
        }

        WardRegistryManager.Instance.RegisterOrUpdate(zdo);
        WardMeBruh.Log.Info($"[Extension Ward] Linked Ward {zdo.m_uid} to Base {rootParentZdo.m_uid} ('{parentName}')");

        if (ZNet.instance != null && !ZNet.instance.IsServer())
        {
            WardNetworkRouter.SendRegisterWard(zdo.m_uid);
        }
    }

    public static void ConfigureAsSubWard(PrivateArea pendingWard, PrivateArea coveringWard)
    {
        if (pendingWard == null || pendingWard.m_nview == null || !pendingWard.m_nview.IsValid())
            return;

        if (coveringWard == null || coveringWard.m_nview == null || !coveringWard.m_nview.IsValid())
            return;

        pendingWard.m_nview.ClaimOwnership();
        ZDO zdo = pendingWard.m_nview.GetZDO();
        ZDO coveringZdo = coveringWard.m_nview.GetZDO();
        ZDO rootParentZdo = WardQueryService.GetEffectiveWardZdo(coveringZdo);
        if (rootParentZdo == null)
        {
            rootParentZdo = coveringZdo;
        }

        zdo.Set(WardZdoKeys.ParentWard, rootParentZdo.m_uid);
        zdo.Set(WardZdoKeys.WardType, (int)WardType.SubWard);

        float maxAllowedSubRadius = WardQueryService.GetMaxAllowedSubWardRadius(zdo, pendingWard.transform.position, rootParentZdo);
        float maxSubRadius = Mathf.Max(1f, Mathf.Floor(maxAllowedSubRadius));
        float minSubRadius = maxSubRadius > 10f ? 5f : 1f;
        float defaultRadius = maxSubRadius < 50f ? Mathf.Min(15f, maxSubRadius) : Mathf.Min(20f, maxSubRadius);
        float initialSubRadius = Mathf.Clamp(defaultRadius, minSubRadius, maxSubRadius);

        zdo.Set(WardZdoKeys.Radius, initialSubRadius);
        zdo.Set(WardZdoKeys.CustomName, "$wmb_ward_type_subward".Localize());

        bool parentEnabled = coveringWard.IsEnabled();
        zdo.Set(ZDOVars.s_enabled, parentEnabled);

        if (pendingWard.m_areaMarker != null)
        {
            pendingWard.m_areaMarker.m_radius = initialSubRadius;
        }

        WardRegistryManager.Instance.RegisterOrUpdate(zdo);
        WardMeBruh.Log.Info($"[Sub-Ward] Created Sub-Ward {zdo.m_uid} inside Base {rootParentZdo.m_uid} with radius {initialSubRadius}m");

        if (ZNet.instance != null && !ZNet.instance.IsServer())
        {
            WardNetworkRouter.SendRegisterWard(zdo.m_uid);
        }
    }
}
