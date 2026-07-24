using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using TOHO;
using TOHO.Modules;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static TOHO.Options;
using static TOHO.Translator;
using Object = UnityEngine.Object;

namespace TOHO.Patches;

[HarmonyPatch(typeof(LobbyViewSettingsPane))]
public static class LobbyViewSettingsPanePatch
{
    private static int SelectedIdx { get; set; }

    private static bool ShowModifiers { get; set; }

    private static PassiveButton ModifiersTabButton { get; set; }

    private static Dictionary<OptionItem, CustomRoles> RoleBySpawnOption =>
        _roleBySpawnOption ??= CustomRoleSpawnChances.ToDictionary(kv => (OptionItem)kv.Value, kv => kv.Key);
    private static Dictionary<OptionItem, CustomRoles> _roleBySpawnOption;

    private static readonly Sprite NextModeIconNormal = Utils.LoadSprite("TOHO.Resources.Images.NextModeArrow.png", 100f);
    private static readonly Sprite NextModeIconHover = Utils.LoadSprite("TOHO.Resources.Images.NextModeArrow_Hover.png", 100f);
    private static readonly Sprite PrevModeIconNormal = Utils.LoadSprite("TOHO.Resources.Images.PrevModeArrow.png", 100f);
    private static readonly Sprite PrevModeIconHover = Utils.LoadSprite("TOHO.Resources.Images.PrevModeArrow_Hover.png", 100f);

    // Same purple used for the "TOHO" mode label, reused here so the arrow
    // hover tint matches it instead of introducing a second purple.
    private static readonly Color ArrowHoverColor = new Color32(0x80, 0x00, 0x80, 0xFF);
	
	
	/// <summary>
	/// Clones rolesTabButton - the same template ModifiersTabButton already
	/// uses without any issues - and only repaints its sprites and hides its
	/// label text. Nothing about PassiveButton's own click/hover/collider/
	/// sound wiring is touched at all this time, since that's exactly what
	/// kept breaking when building one from scratch.
	/// </summary>
	private static PassiveButton CreateTabIconButton(
		PassiveButton template,
		Transform parent,
		string name,
		Vector3 position,
		Sprite icon,
		Sprite hoverIcon,
		UnityAction onClick)
	{
		var clone = Object.Instantiate(template, parent);
		clone.name = name;
		clone.transform.localPosition = position;
		clone.transform.localScale = new Vector3(0.2f, 0.8f, 1f);

		if (icon != null)
		{
			foreach (var renderer in clone.GetComponentsInChildren<SpriteRenderer>(true))
			{
				renderer.sprite = icon;
			}
		}
		else
		{
			Logger.Error(
				$"{name}: custom icon missing - check the PNG exists under TOHO/Resources/Images and the project was rebuilt.",
				"LobbyViewSettingsPanePatch");
		}

		// The hover-state child is actually named "Highlight" (not
		// "Active"/"Hover" like the PassiveButton field names implied) -
		// repaint just that subtree with the hover icon, after the blanket
		// pass above already put the normal icon everywhere.
		if (hoverIcon != null)
		{
			var highlight = clone.GetComponentsInChildren<Transform>(true)
				.FirstOrDefault(t => t.name == "Highlight");

			if (highlight != null)
			{
				foreach (var renderer in highlight.GetComponentsInChildren<SpriteRenderer>(true))
				{
					renderer.sprite = hoverIcon;
				}
			}
			else
			{
				Logger.Error($"{name}: no \"Highlight\" child found to apply the hover icon to.", "LobbyViewSettingsPanePatch");
			}
		}

		// Tab buttons carry a decorative "Shine" child per state (see the
		// Icon/Shine layout in MainMenuPatch.cs) - we don't want that
		// highlight sweeping across a plain icon button, so switch it off
		// wherever it shows up (Inactive, Active, Selected, ...).
		foreach (var shine in clone.GetComponentsInChildren<Transform>(true))
		{
			if (shine.name == "Shine")
				shine.gameObject.SetActive(false);
		}

		// It's a tab button, so it comes with label text ("Roles" etc.) -
		// we only want the icon, not a leftover word under/behind it.
		if (clone.buttonText != null)
		{
			var translator = clone.buttonText.GetComponent<TextTranslatorTMP>();
			if (translator != null) Object.Destroy(translator);
			clone.buttonText.text = "";
		}

		clone.OnClick = new Button.ButtonClickedEvent();
		clone.OnClick.AddListener(onClick);

		// Purple tint applied only while the pointer is actually over the
		// button - captured per-renderer up front so OnMouseOut can restore
		// each one to whatever it started as (icon vs. Highlight child may
		// not share the same base color) rather than assuming white.
		var normalColors = clone.GetComponentsInChildren<SpriteRenderer>(true)
			.Where(r => r != null)
			.ToDictionary(r => r, r => r.color);

		clone.OnMouseOver.AddListener((UnityAction)(() =>
		{
			foreach (var renderer in normalColors.Keys)
			{
				if (renderer != null) renderer.color = ArrowHoverColor;
			}
		}));

		clone.OnMouseOut.AddListener((UnityAction)(() =>
		{
			foreach (var kvp in normalColors)
			{
				if (kvp.Key != null) kvp.Key.color = kvp.Value;
			}
		}));

		return clone;
	}

    [HarmonyPostfix]
    [HarmonyPatch(nameof(LobbyViewSettingsPane.Awake))]
    public static void AwakePostfix(LobbyViewSettingsPane __instance)
    {
        SelectedIdx = 0;
        _roleBySpawnOption = null;

        ApplyGameModeTextStyle(__instance);

        CreateTabIconButton(
			__instance.rolesTabButton,
			__instance.BackButton.transform.parent,
			"TOHO_NextModeButton",
			new Vector3(-4.4f, 2.5f, -2f),
			NextModeIconNormal,
			NextModeIconHover,
			(UnityAction)(() =>
			{
				SelectedIdx = SelectedIdx == 0 ? 1 : 0;
				Refresh(__instance);
			}));

        CreateTabIconButton(
			__instance.rolesTabButton,
			__instance.BackButton.transform.parent,
			"TOHO_PrevModeButton",
			new Vector3(-6.6f, 2.5f, -2f),
			PrevModeIconNormal,
			PrevModeIconHover,
			(UnityAction)(() =>
			{
				SelectedIdx = SelectedIdx == 0 ? 1 : 0;
				Refresh(__instance);
			}));

        ModifiersTabButton = Object.Instantiate(__instance.rolesTabButton, __instance.rolesTabButton.transform.parent);
        ModifiersTabButton.name = "TOHO_ModifiersTabButton";
        var modPos = ModifiersTabButton.transform.localPosition;
        modPos.x = 2.1f;
        ModifiersTabButton.transform.localPosition = modPos;
        var translator = ModifiersTabButton.buttonText.GetComponent<TextTranslatorTMP>();
        if (translator != null)
        {
            UnityEngine.Object.Destroy(translator);
        }
        ModifiersTabButton.buttonText.text = "Modifiers";
        ModifiersTabButton.OnClick = new Button.ButtonClickedEvent();
        ModifiersTabButton.OnClick.AddListener((UnityAction)(() =>
        {
            ShowModifiers = true;
            Refresh(__instance);
        }));
        ModifiersTabButton.gameObject.SetActive(false);
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(LobbyViewSettingsPane.ChangeTab))]
    public static void ChangeTabPostfix()
    {
        ShowModifiers = false;
    }

    private static void ApplyGameModeTextStyle(LobbyViewSettingsPane menu)
    {
        menu.gameModeText.text = SelectedIdx == 0 ? "<color=#fc8c03>Classic</color>" : "<color=#800080>TOHO</color>";
        menu.gameModeText.transform.localPosition = new Vector3(
            -3.6586f,
            2.4241f,
            -1.9999f
        );
    }

    private static void Refresh(LobbyViewSettingsPane menu)
    {
        ApplyGameModeTextStyle(menu);
        ModifiersTabButton?.gameObject.SetActive(SelectedIdx == 1);
        ModifiersTabButton?.SelectButton(SelectedIdx == 1 && ShowModifiers);
        if (SelectedIdx == 0) ShowModifiers = false;
        menu.RefreshTab();
        menu.scrollBar.ScrollToTop();
    }

    private static void ClearSettingsInfo(LobbyViewSettingsPane instance)
    {
        var container = instance.settingsContainer;
        if (container != null)
        {
            for (var i = container.childCount - 1; i >= 0; i--)
            {
                try
                {
                    var child = container.GetChild(i);
                    if (child != null) Object.Destroy(child.gameObject);
                }
                catch (System.Exception ex)
                {
                    Logger.Error($"Failed destroying settingsContainer child {i}: {ex}", "LobbyViewSettingsPanePatch");
                }
            }
        }

        foreach (var go in instance.settingsInfo)
        {
            try
            {
                if (go != null) Object.Destroy(go);
            }
            catch (System.Exception ex)
            {
                Logger.Error($"Failed destroying tracked settingsInfo object: {ex}", "LobbyViewSettingsPanePatch");
            }
        }
        instance.settingsInfo.Clear();
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(LobbyViewSettingsPane.RefreshTab))]
    public static bool RefreshTabPrefix(LobbyViewSettingsPane __instance)
    {
        if (SelectedIdx != 1 || !ShowModifiers) return true;

        ClearSettingsInfo(__instance);

        var num = 0.95f;
        num = DrawTabSection(__instance, TabGroup.Modifiers, num, out var advanced);
        num = DrawAdvancedGrid(__instance, advanced, num);
        __instance.scrollBar.SetYBoundsMax(-num);

        return false;
    }

    // ---------------------------------------------------------------
    // Settings tab (non-role options)
    // ---------------------------------------------------------------
    [HarmonyPrefix]
    [HarmonyPatch(nameof(LobbyViewSettingsPane.DrawNormalTab))]
    public static bool DrawNormalTabPatch(LobbyViewSettingsPane __instance)
    {
        if (SelectedIdx == 0) return true;

        DrawTohoOptions(__instance);
        return false;
    }

    private static void DrawTohoOptions(LobbyViewSettingsPane menu)
    {
        ClearSettingsInfo(menu);

        var num = 1.44f;
        var groups = new[] { TabGroup.SystemSettings, TabGroup.ModSettings };

        foreach (var tab in groups)
        {
            num = DrawTabSection(menu, tab, num, out _, isRoleTab: false);
        }

        menu.scrollBar.SetYBoundsMax(-num - 2);
    }

    // ---------------------------------------------------------------
    // Roles tab
    // ---------------------------------------------------------------
    [HarmonyPrefix]
    [HarmonyPatch(nameof(LobbyViewSettingsPane.DrawRolesTab))]
    public static bool DrawRolesTabPatch(LobbyViewSettingsPane __instance)
    {
        if (SelectedIdx == 0) return true;

        DrawTohoRoles(__instance);
        return false;
    }

    private static void DrawTohoRoles(LobbyViewSettingsPane instance)
    {
        ClearSettingsInfo(instance);

        var quotaHeader = Object.Instantiate(instance.categoryHeaderOrigin, instance.settingsContainer, true);
        quotaHeader.SetHeader(StringNames.RoleQuotaLabel, 61);
        quotaHeader.transform.localScale = Vector3.one;
        quotaHeader.transform.localPosition = new Vector3(-9.77f, 1.26f, -2f);
        instance.settingsInfo.Add(quotaHeader.gameObject);

        var teamTabs = new[]
        {
            TabGroup.ImpostorRoles,
            TabGroup.CrewmateRoles,
            TabGroup.NeutralRoles,
            TabGroup.CovenRoles,
        };

        var num = 0.95f;
        var allAdvanced = new List<CustomRoles>();
        foreach (var tab in teamTabs)
        {
            num = DrawTabSection(instance, tab, num, out var advanced);
            allAdvanced.AddRange(advanced);
        }

        num = DrawAdvancedGrid(instance, allAdvanced, num);
        instance.scrollBar.SetYBoundsMax(-num);
    }

    // ---------------------------------------------------------------
    // Shared: walks OptionItem.AllOptions for one TabGroup in registration
    // order. TextOptionItem entries become category headers (colored/named
    // per the option's own Custom_RoleType grouping, exactly like the ESC
    // menu). Anything that's a role's spawn-chance option becomes a role
    // quota card; anything else (isRoleTab: false) becomes a plain
    // checkbox/value row.
    // ---------------------------------------------------------------
    private static float DrawTabSection(
        LobbyViewSettingsPane instance,
        TabGroup tab,
        float num,
        out List<CustomRoles> advancedRoles,
        bool isRoleTab = true)
    {
        advancedRoles = [];
        var roleColX = -6.53f;
        var i = 0;

        List<OptionItem> items;
        try
        {
            items = OptionItem.AllOptions
                .Where(x => x.Tab == tab && x.Parent == null && !x.IsHiddenOn(Options.CurrentGameMode))
                .ToList();
        }
        catch (System.Exception ex)
        {
            Logger.Error($"Failed collecting options for {tab}: {ex}", "LobbyViewSettingsPanePatch");
            return num;
        }

        foreach (var option in items)
        {
            try
            {
                if (option.IsText)
                {
                    var headerType = isRoleTab ? instance.categoryHeaderRoleOrigin : instance.categoryHeaderOrigin;
                    var header = isRoleTab
                        ? (CategoryHeaderMasked)null
                        : Object.Instantiate(instance.categoryHeaderOrigin, instance.settingsContainer, true);

                    if (isRoleTab)
                    {
                        var roleHeader = Object.Instantiate(instance.categoryHeaderRoleOrigin, instance.settingsContainer, true);
                        roleHeader.SetHeader(StringNames.Name, 61);
                        roleHeader.gameObject.DestroyTranslator();
                        roleHeader.Title.text = option.GetName(disableColor: true);
                        if (roleHeader.Background != null) roleHeader.Background.color = option.NameColor;
                        if (roleHeader.Divider != null) roleHeader.Divider.color = option.NameColor;
                        roleHeader.transform.localScale = Vector3.one;
                        roleHeader.transform.localPosition = new Vector3(0.09f, num, -2f);
                        instance.settingsInfo.Add(roleHeader.gameObject);
                        num -= 0.696f;
                    }
                    else
                    {
                        // Headers were landing on the same row as the last
                        // option whenever that row wasn't a fresh, empty one
                        // (i.e. i != 0) - the "start a new row" decrement
                        // only ever fired for a left-column checkbox, never
                        // for a header. Flush the pending row first.
                        if (i != 0) num -= 0.85f;

                        header.SetHeader(StringNames.Name, 61);
                        header.gameObject.DestroyTranslator();
                        header.Title.text = option.GetName(disableColor: true);
                        header.transform.localScale = Vector3.one;
                        header.transform.localPosition = new Vector3(-9.77f, num, -2f);
                        instance.settingsInfo.Add(header.gameObject);
                        num -= 1.35f;
                    }

                    i = 0;
                    continue;
                }

                if (isRoleTab && RoleBySpawnOption.TryGetValue(option, out var role))
                {
                    DrawRoleCard(instance, role, tab, roleColX, num, advancedRoles);
                    num -= 0.664f;
                    continue;
                }

                if (isRoleTab) continue; // unexpected non-role, non-text top-level item on a role tab

                var panel = Object.Instantiate(instance.infoPanelOrigin, instance.settingsContainer, true);
                panel.transform.localScale = Vector3.one;

                const float RowHeight = 0.95f;
                float posX;

                if (i % 2 == 0)
                {
                    posX = -8.95f;

                    if (i > 0)
                        num -= RowHeight;
                }
                else
                {
                    posX = -3f;
                }
                panel.transform.localPosition = new Vector3(posX, num, -2f);

                if (option is BooleanOptionItem)
                {
                    panel.SetInfoCheckbox(StringNames.Name, 61, option.GetBool());
                }
                else
                {
                    panel.SetInfo(StringNames.Name, option.GetString(), 61);
                }
                panel.titleText.gameObject.DestroyTranslator();
                panel.titleText.text = option.GetName(disableColor: true);

                instance.settingsInfo.Add(panel.gameObject);
                i++;
            }
            catch (System.Exception ex)
            {
                Logger.Error($"Failed drawing option {option?.Name} on {tab}: {ex}", "LobbyViewSettingsPanePatch");
            }
        }

        if (!isRoleTab) num -= 1.0f;
        return num;
    }

    private static void DrawRoleCard(
        LobbyViewSettingsPane instance,
        CustomRoles role,
        TabGroup tab,
        float posX,
        float num,
        List<CustomRoles> advancedRoles)
    {
        var count = GetRoleCount(role);
        var chance = GetRoleChance(role);
        var disabled = count == 0;

        var panel = Object.Instantiate(instance.infoPanelRoleOrigin, instance.settingsContainer, true);
        if (panel == null)
        {
            Logger.Error("infoPanelRoleOrigin instantiated null", "LobbyViewSettingsPanePatch");
            return;
        }

        panel.transform.localScale = Vector3.one;
        panel.transform.localPosition = new Vector3(posX, num, -2f);

        if (!disabled) advancedRoles.Add(role);

        panel.SetInfo(
            GetString(role.ToString()),
            count,
            (int)chance,
            61,
            Utils.GetRoleColor(role),
            RoleIconGenerator.GetIcon(role),
            tab == TabGroup.CrewmateRoles,
            disabled);

        panel.titleText.gameObject.DestroyTranslator();
    }

    private static void CollectVisibleOptions(
    OptionItem option,
    CustomRoles role,
    List<OptionItem> output)
    {
        // The count ("Maximum") and, for modifiers, the actual spawn-rate
        // percent ("AdditionRolesSpawnRate" / "Spawn Chance") are already
        // rendered directly on the card via GetRoleCount/GetRoleChance -
        // listing them again as extra rows here just duplicates (and for
        // the rate option, can look like it disagrees with) the card.
        var countOption = CustomRoleCounts.GetValueOrDefault(role);
        var rateOption = CustomAdtRoleSpawnRate.GetValueOrDefault(role);

        foreach (var child in option.Children)
        {
            if (child.IsHiddenOn(Options.CurrentGameMode))
                continue;

            if (ReferenceEquals(child, countOption) || ReferenceEquals(child, rateOption))
                continue;

            output.Add(child);

            CollectVisibleOptions(child, role, output);
        }
    }

    private static float DrawAdvancedGrid(LobbyViewSettingsPane instance, List<CustomRoles> advancedList, float num)
    {
        if (advancedList.Count == 0) return num;

        var settingsHeader = Object.Instantiate(instance.categoryHeaderOrigin, instance.settingsContainer, true);
        settingsHeader.SetHeader(StringNames.RoleSettingsLabel, 61);
        settingsHeader.transform.localScale = Vector3.one;
        settingsHeader.transform.localPosition = new Vector3(-9.77f, num, -2f);
        instance.settingsInfo.Add(settingsHeader.gameObject);
        num -= 2.1f;

        var rowHeight = 0f;
        var placedInRow = 0;
        for (var k = 0; k < advancedList.Count; k++)
        {
            try
            {
                var role = advancedList[k];
                if (!CustomRoleSpawnChances.TryGetValue(role, out var spawnOption)) continue;

                var extras = new List<OptionItem>();
                CollectVisibleOptions(spawnOption, role, extras);

                if (extras.Count == 0) continue;

                float posX;
                if (placedInRow % 2 == 0)
                {
                    posX = -5.8f;
                    if (placedInRow > 0)
                    {
                        num -= rowHeight + 0.85f;
                        rowHeight = 0f;
                    }
                }
                else
                {
                    posX = 0.14999962f;
                }

                var advPanel = Object.Instantiate(instance.advancedRolePanelOrigin, instance.settingsContainer, true);
                advPanel.transform.localScale = Vector3.one;
                advPanel.transform.localPosition = new Vector3(posX, num, -2f);

                var height = SetUpAdvancedRoleViewPanel(advPanel, role, extras, 0.59f, 61);
                if (height > rowHeight) rowHeight = height;

                instance.settingsInfo.Add(advPanel.gameObject);
                placedInRow++;
            }
            catch (System.Exception ex)
            {
                Logger.Error($"Failed drawing advanced panel for {advancedList[k]}: {ex}", "LobbyViewSettingsPanePatch");
            }
        }

        return num - rowHeight;
    }

    private static float SetUpAdvancedRoleViewPanel(
        AdvancedRoleViewPanel viewPanel,
        CustomRoles role,
        List<OptionItem> extraOptions,
        float spacingY,
        int maskLayer)
    {
        viewPanel.header.SetHeader(StringNames.Name, maskLayer);
        viewPanel.header.gameObject.DestroyTranslator();
        viewPanel.header.Title.text = GetString(role.ToString());
        viewPanel.header.Background.color = viewPanel.header.Divider.color = Utils.GetRoleColor(role);
        viewPanel.divider.material.SetInt(PlayerMaterial.MaskLayer, maskLayer);

        // AdvancedRoleViewPanel's header has no icon slot of its own (unlike
        // the role card, which gets one via SetInfo) - add a small icon
        // renderer so the expanded options panel still shows which role
        // it's for, using the same icon source as the card. Match the
        // ribbon's own layer/sorting exactly and render one order in front
        // of it, rather than guessing at a Z offset that could put the icon
        // behind the ribbon depending on how it's drawn.
        //
        // These header panels are pooled/reused, so find-or-create rather
        // than always creating a new one - otherwise repeated refreshes
        // (e.g. scrolling) just keep stacking new icons on top of old ones.
        var headerIconTransform = viewPanel.header.transform.Find("TOHO_RoleIcon");
        GameObject headerIcon;
        if (headerIconTransform != null)
        {
            headerIcon = headerIconTransform.gameObject;
        }
        else
        {
            headerIcon = new GameObject("TOHO_RoleIcon") { layer = viewPanel.header.Background.gameObject.layer };
            headerIcon.transform.SetParent(viewPanel.header.transform, false);
            headerIcon.AddComponent<SpriteRenderer>();
        }

        var headerIconRenderer = headerIcon.GetComponent<SpriteRenderer>();
        headerIconRenderer.sprite = RoleIconGenerator.GetIcon(role);
        headerIconRenderer.sortingLayerID = viewPanel.header.Background.sortingLayerID;
        headerIconRenderer.sortingOrder = viewPanel.header.Background.sortingOrder + 1;

        // The rest of this panel clips to the scroll/panel bounds via a
        // masked shader (see the divider's MaskLayer set below) - a plain
        // default SpriteRenderer material doesn't know about that mask at
        // all, so it can render outside/through panel edges regardless of
        // where it's positioned. Copying the header's own material and
        // mask layer keeps the icon clipped the same way everything else
        // in this header already is.
        headerIconRenderer.material = viewPanel.header.Background.material;
        headerIconRenderer.material.SetInt(PlayerMaterial.MaskLayer, maskLayer);

        // Back to a fixed local offset/scale - the bounds-based world-space
        // version scattered icons across the whole screen instead of onto
        // their headers (Background.bounds evidently isn't what I assumed
        // it was here), which is worse than the original issue it was
        // meant to fix. This fixed version was the one confirmed correctly
        // positioned; bumped up a bit per request.
        headerIcon.transform.localPosition = new Vector3(-2.85f, 0f, -0.1f);
        headerIcon.transform.localScale = Vector3.one * 0.15f;

        var num = viewPanel.yPosStart;
        var num2 = 1.08f;

        for (var i = 0; i < extraOptions.Count; i++)
        {
            var option = extraOptions[i];
            var panel = Object.Instantiate(viewPanel.infoPanelOrigin, viewPanel.transform, true);
            panel.transform.localScale = Vector3.one;
            panel.transform.localPosition = new Vector3(viewPanel.xPosStart, num, -2f);

            if (option is BooleanOptionItem)
            {
                panel.SetInfoCheckbox(StringNames.Name, maskLayer, option.GetBool());
            }
            else
            {
                panel.SetInfo(StringNames.Name, option.GetString(), maskLayer);
            }
            panel.titleText.gameObject.DestroyTranslator();
            panel.titleText.text = option.GetName(disableColor: true);

            num -= spacingY;
            if (i > 0) num2 += 0.8f;
        }

        return num2;
    }
}