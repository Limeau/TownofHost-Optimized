using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TOHO;

//来源：https://github.com/tukasa0001/TownOfHost/pull/1265
public class ClientOptionItem
{
    public ConfigEntry<bool> Config;
    public ToggleButtonBehaviour ToggleButton;

    public static SpriteRenderer CustomBackground;

    // Pagination — same layout constants and Prev/Next pattern used by
    // AmongUsQoLMod's QoLOptionsFeature.cs, so this panel no longer just
    // stacks toggles downward forever (which used to run new rows off
    // the bottom of the panel once there were more than ~9-10 of them).
    private const int PageSize = 12; // 2 columns x 6 rows
    private const int Columns = 2;
    private const float ColGap = 2.6f;
    private const float RowGap = 0.5f;
    private const float StartY = 2.2f;
    private const float RowZ = -6f;

    // Same purple used everywhere else in this file for "active"/accent
    // state - the TOHOOptions open button, and a toggle's own on-color in
    // UpdateToggle() below. Reused here so Back/Prev/Next visually match
    // instead of sitting there as leftover neutral grey.
    private static readonly Color32 ActiveColor = new(180, 126, 222, byte.MaxValue);

    private static readonly List<ClientOptionItem> _items = new();
    private static ToggleButtonBehaviour _rowTemplate;
    private static GameObject _prevBtn;
    private static GameObject _nextBtn;
    private static int _page;

    private ClientOptionItem(
        string name,
        ConfigEntry<bool> config,
        OptionsMenuBehaviour optionsMenuBehaviour,
        Action additionalOnClickAction = null)
    {
        Config = config;

        var mouseMoveToggle = optionsMenuBehaviour.DisableMouseMovement;

        // 1つ目のボタンの生成時に背景も生成
        if (CustomBackground == null)
        {
            _items.Clear();
            _rowTemplate = mouseMoveToggle;
            _prevBtn = null;
            _nextBtn = null;
            _page = 0;

            CustomBackground = Object.Instantiate(optionsMenuBehaviour.Background, optionsMenuBehaviour.transform);
            CustomBackground.name = "CustomBackground";
            CustomBackground.transform.localScale = new(0.9f, 0.9f, 1f);
            CustomBackground.transform.localPosition += Vector3.back * 8;
            CustomBackground.gameObject.SetActive(false);

            var closeButton = Object.Instantiate(mouseMoveToggle, CustomBackground.transform);
            closeButton.transform.localPosition = new(1.3f, -2.3f, -6f);
            closeButton.name = "Back";
            closeButton.Text.text = Translator.GetString("Back");
            closeButton.Background.color = ActiveColor;
            closeButton.Rollover?.ChangeOutColor(ActiveColor);
            var closePassiveButton = closeButton.GetComponent<PassiveButton>();
            closePassiveButton.OnClick = new();
            closePassiveButton.OnClick.AddListener((UnityEngine.Events.UnityAction)(() =>
            {
                CustomBackground.gameObject.SetActive(false);
            }));

            UiElement[] selectableButtons = optionsMenuBehaviour.ControllerSelectable.ToArray();
            PassiveButton leaveButton = null;
            PassiveButton returnButton = null;
            foreach (var button in selectableButtons)
            {
                if (button == null) continue;

                if (button.name == "LeaveGameButton")
                    leaveButton = button.GetComponent<PassiveButton>();
                else if (button.name == "ReturnToGameButton")
                    returnButton = button.GetComponent<PassiveButton>();
            }
            var generalTab = mouseMoveToggle.transform.parent.parent.parent;

            var modOptionsButton = Object.Instantiate(mouseMoveToggle, generalTab);
            modOptionsButton.transform.localPosition = new(1.2f, -1.8f, 1f);
            modOptionsButton.name = "TOHOOptions";
            modOptionsButton.Text.text = Translator.GetString("TOHOOptions");
            modOptionsButton.Background.color = new Color32(180, 126, 222, byte.MaxValue);
            var modOptionsPassiveButton = modOptionsButton.GetComponent<PassiveButton>();
            modOptionsPassiveButton.OnClick = new();
            modOptionsPassiveButton.OnClick.AddListener((UnityEngine.Events.UnityAction)(() =>
            {
                CustomBackground.gameObject.SetActive(true);
            }));

            if (leaveButton != null)
                leaveButton.transform.localPosition = new(-1.35f, -2.411f, -1f);
            if (returnButton != null)
                returnButton.transform.localPosition = new(1.35f, -2.411f, -1f);
        }

        // ボタン生成 — positioned later by ShowPage() based on this
        // item's slot within the current page, not by creation order.
        ToggleButton = Object.Instantiate(mouseMoveToggle, CustomBackground.transform);
        ToggleButton.name = name;
        ToggleButton.Text.text = Translator.GetString(name);
        var passiveButton = ToggleButton.GetComponent<PassiveButton>();
        passiveButton.OnClick = new();
        passiveButton.OnClick.AddListener((UnityEngine.Events.UnityAction)(() =>
        {
            if (config != null) config.Value = !config.Value;
            UpdateToggle();
            additionalOnClickAction?.Invoke();
        }));
        UpdateToggle();

        _items.Add(this);
    }

    public static ClientOptionItem Create(
        string name,
        ConfigEntry<bool> config,
        OptionsMenuBehaviour optionsMenuBehaviour,
        Action additionalOnClickAction = null)
    {
        return new(name, config, optionsMenuBehaviour, additionalOnClickAction);
    }

    public void UpdateToggle()
    {
        if (ToggleButton == null) return;

        var color = (Config != null && Config.Value) ? new Color32(180, 126, 222, byte.MaxValue) : new Color32(77, 77, 77, byte.MaxValue);
        ToggleButton.Background.color = color;
        ToggleButton.Rollover?.ChangeOutColor(color);
    }

    // ------------------------------------------------------------------
    // Pagination — call once after all ClientOptionItem.Create() calls
    // for a given menu open (see OptionsMenuBehaviourStartPatch.Postfix).
    // Builds the Prev/Next buttons the first time there's more than one
    // page, then lays out whichever page is currently selected.
    // ------------------------------------------------------------------
    public static void RefreshPaging()
    {
        if (CustomBackground == null || _rowTemplate == null) return;

        BuildPagerButtonsIfNeeded();
        ShowPage(_page);
    }

    private static void BuildPagerButtonsIfNeeded()
    {
        if (_prevBtn != null || _nextBtn != null) return; // already built
        if (TotalPages() <= 1) return;

        // Fixed at the bottom of a full page (6 rows) rather than the
        // current item count, so the nav row doesn't jump around as
        // items are added/removed between menu opens.
        float y = StartY - PageSize / Columns * RowGap - (RowGap * 0.5f);

        _prevBtn = BuildNavButton("QoLPrevBtn", "< Prev", new Vector3(-ColGap / 2f, y, RowZ), () => ShowPage(_page - 1));
        _nextBtn = BuildNavButton("QoLNextBtn", "Next >", new Vector3(ColGap / 2f, y, RowZ), () => ShowPage(_page + 1));
    }

    private static GameObject BuildNavButton(string name, string text, Vector3 pos, Action onClick)
    {
        if (_rowTemplate == null || CustomBackground == null) return null;

        var obj = Object.Instantiate(_rowTemplate.gameObject, CustomBackground.transform, false);
        obj.name = name;
        obj.transform.localPosition = pos;

        var toggle = obj.GetComponent<ToggleButtonBehaviour>();
        if (toggle != null)
        {
            toggle.Text.text = text;
            var navColor = ActiveColor;
            toggle.Background.color = navColor;
            toggle.Rollover?.ChangeOutColor(navColor);
        }

        var button = obj.GetComponent<PassiveButton>();
        button.OnClick = new();
        button.OnClick.AddListener((UnityEngine.Events.UnityAction)(() => onClick()));

        return obj;
    }

    private static int TotalPages() =>
        _items.Count == 0 ? 1 : Mathf.CeilToInt(_items.Count / (float)PageSize);

    private static void ShowPage(int page)
    {
        int totalPages = TotalPages();
        _page = Mathf.Clamp(page, 0, totalPages - 1);

        int start = _page * PageSize;
        int end = Math.Min(start + PageSize, _items.Count);

        for (int i = 0; i < _items.Count; i++)
        {
            var item = _items[i];
            if (item.ToggleButton == null) continue;

            bool visible = i >= start && i < end;
            item.ToggleButton.gameObject.SetActive(visible);
            if (!visible) continue;

            int idxInPage = i - start;
            int col = idxInPage % Columns;
            int rowInPage = idxInPage / Columns;

            float x = col == 0 ? -ColGap / 2f : ColGap / 2f;
            float y = StartY - rowInPage * RowGap;
            item.ToggleButton.transform.localPosition = new Vector3(x, y, RowZ);
        }

        _prevBtn?.SetActive(_page > 0);
        _nextBtn?.SetActive(_page < totalPages - 1);
    }
}

public class ThemeOptionItem
{
    public ConfigEntry<bool> Config;
    public ToggleButtonBehaviour modOptionsButton;
    public static int ThemeID = 1;
    
    public static SpriteRenderer CustomBackground;

    private ThemeOptionItem(
        ConfigEntry<int> config,
        OptionsMenuBehaviour optionsMenuBehaviour
        )
    {
        var mouseMoveToggle = optionsMenuBehaviour.DisableMouseMovement;
        var generalTab = mouseMoveToggle.transform.parent.parent.parent;
        PassiveButton leaveButton = null;
        foreach (var button in optionsMenuBehaviour.ControllerSelectable.ToArray())
        {
            if (button == null) continue;

            if (button.name == "LeaveGameButton")
                leaveButton = button.GetComponent<PassiveButton>();
        }        
        modOptionsButton = Object.Instantiate(mouseMoveToggle, generalTab);

        modOptionsButton.transform.localPosition = new(-1.2f, -1.8f, 1f);
        modOptionsButton.name = "TOHOTheme";

        var theme = "None";
        switch (ThemeID)
        {
            case 1:
                theme = "Classic";
                break;
            case 2:
                theme = "Dark";
                break;
            case 3:
                theme = "Mars Red";
                break;
            case 4:
                theme = "Golden Yellow";
                break;
            case 5:
                theme = "Forest Green";
                break;
            case 6:
                theme = "Deep Sea Blue";
                break;
        }
        modOptionsButton.Text.text = "Theme: " + theme;
        modOptionsButton.Background.color = new Color32(180, 126, 222, byte.MaxValue);
        var modOptionsPassiveButton = modOptionsButton.GetComponent<PassiveButton>();
        modOptionsPassiveButton.OnClick = new();
        modOptionsPassiveButton.OnClick.AddListener((UnityEngine.Events.UnityAction)(() =>
        {
            if (ThemeID >= 6) ThemeID = 0;
            else ThemeID++;
            UpdateToggle();
        }));
        if (leaveButton != null)
            leaveButton.transform.localPosition = new(-1.35f, -2.411f, -1f);
        UpdateToggle();
    }

    public static ThemeOptionItem Create(
        ConfigEntry<int> config,
        OptionsMenuBehaviour optionsMenuBehaviour)
    {
        return new(config, optionsMenuBehaviour);
    }

    public void UpdateToggle()
    {
        if (modOptionsButton == null) return;

        var color = new Color(0, 0, 0);
        
        switch (ThemeID)
        {
            case 1: 
                color = new Color32(225, 225, 225, byte.MaxValue);
                break;

            case 2: 
                color = new Color32(55, 55, 55, byte.MaxValue);
                break;

            case 3: 
                color = new Color32(112, 33, 25, byte.MaxValue);
                break;

            case 4: 
                color = new Color32(117, 83, 11, byte.MaxValue);
                break;

            case 5: 
                color = new Color32(36, 69, 25, byte.MaxValue);
                break;

            case 6: 
                color = new Color32(6, 13, 56, byte.MaxValue);
                break;
        }
        modOptionsButton.Background.color = color;
        modOptionsButton.Rollover?.ChangeOutColor(color);
        var theme = "None";
        switch (ThemeID)
        {
            case 1:
                theme = "Classic";
                break;
            case 2:
                theme = "Dark";
                break;
            case 3:
                theme = "Mars Red";
                break;
            case 4:
                theme = "Golden Yellow";
                break;
            case 5:
                theme = "Forest Green";
                break;
            case 6:
                theme = "Deep Sea Blue";
                break;
        }
        modOptionsButton.Text.text = "Theme: " + theme;
    }
}