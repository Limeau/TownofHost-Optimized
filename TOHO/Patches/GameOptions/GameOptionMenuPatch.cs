using System;
using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using UnityEngine;
using TMPro;
using TOHO;
using UnityEngine.Events;
using Object = UnityEngine.Object;

[HarmonyPatch(typeof(GameSettingMenu))]
public static class GameSettingMenuPatch
{
    public static GameOptionsMenu ModSettingsTab;
    public static GameOptionsMenu VanillaSettingsTab;
    public static GameOptionsMenu RoleSettingsTab;

    public static PassiveButton ModSettingsButton;
    public static PassiveButton VanillaSettingsButton;
    public static PassiveButton RoleSettingsButton;

    public static List<GameObject> CrewmateObjects = [];
    public static List<GameObject> ImpostorObjects = [];
    public static List<GameObject> NeutralObjects = [];
    public static List<GameObject> CovenObjects = [];
    public static List<GameObject> ModifierObjects = [];
    public static List<GameObject> ModSettingObjects = [];
    
    public static List<BooleanOptionItem> BoolCrewmateSettings = [];
    public static List<BooleanOptionItem> BoolImpostorSettings = [];
    public static List<BooleanOptionItem> BoolNeutralSettings = [];
    public static List<BooleanOptionItem> BoolCovenSettings = [];
    public static List<BooleanOptionItem> BoolModifierSettings = [];
    public static List<BooleanOptionItem> BoolModSettings = [];
    
    public static List<NumberOptionItem> NumberCrewmateSettings = [];
    public static List<NumberOptionItem> NumberImpostorSettings = [];
    public static List<NumberOptionItem> NumberNeutralSettings = [];
    public static List<NumberOptionItem> NumberCovenSettings = [];
    public static List<NumberOptionItem> NumberModifierSettings = [];
    public static List<NumberOptionItem> NumberModSettings = [];
    
    public static List<StringOptionItem> StringCrewmateSettings = [];
    public static List<StringOptionItem> StringImpostorSettings = [];
    public static List<StringOptionItem> StringNeutralSettings = [];
    public static List<StringOptionItem> StringCovenSettings = [];
    public static List<StringOptionItem> StringModifierSettings = [];
    public static List<StringOptionItem> StringModSettings = [];
    
    private static bool ModSettingsInitialized;

    [HarmonyPatch(nameof(GameSettingMenu.Start)), HarmonyPostfix]
    public static void StartPostfix(GameSettingMenu __instance)
    {
        Main.Logger.LogInfo("[Settings] StartPostfix entered!");
        
        __instance.GamePresetsButton.gameObject.SetActive(false);
        __instance.GameSettingsButton.gameObject.SetActive(false);
        __instance.RoleSettingsButton.gameObject.SetActive(false);

        __instance.PresetsTab.gameObject.SetActive(false);
        __instance.RoleSettingsTab.gameObject.SetActive(false);
        __instance.GameSettingsTab.gameObject.SetActive(false);
        
        VanillaSettingsButton = Object.Instantiate(__instance.GamePresetsButton, __instance.GamePresetsButton.transform.parent);
        ModSettingsButton = Object.Instantiate(__instance.GameSettingsButton, __instance.GameSettingsButton.transform.parent);
        RoleSettingsButton = Object.Instantiate(__instance.RoleSettingsButton, __instance.RoleSettingsButton.transform.parent);
                
        ModSettingsButton.GetComponentInChildren<TextMeshPro>().DestroyTranslator();
        VanillaSettingsButton.GetComponentInChildren<TextMeshPro>().DestroyTranslator();
        RoleSettingsButton.GetComponentInChildren<TextMeshPro>().DestroyTranslator();
        ModSettingsButton.GetComponentInChildren<TextMeshPro>().text = "Mod Settings";
        VanillaSettingsButton.GetComponentInChildren<TextMeshPro>().text = "Vanilla Settings";
        RoleSettingsButton.GetComponentInChildren<TextMeshPro>().text = "Role Settings";
        ModSettingsButton.gameObject.SetActive(true);
        VanillaSettingsButton.gameObject.SetActive(true);
        RoleSettingsButton.gameObject.SetActive(true);
        
        __instance.ControllerSelectable.Add(VanillaSettingsButton);
        __instance.ControllerSelectable.Add(ModSettingsButton);
        __instance.ControllerSelectable.Add(RoleSettingsButton);
        
        ModSettingsTab = Object.Instantiate(__instance.GameSettingsTab, __instance.GameSettingsTab.transform.parent);

        VanillaSettingsTab = Object.Instantiate(__instance.GameSettingsTab, __instance.GameSettingsTab.transform.parent);
        RoleSettingsTab = Object.Instantiate(__instance.GameSettingsTab, __instance.GameSettingsTab.transform.parent);

        ChangeCustomTab(VanillaSettingsTab, VanillaSettingsButton);

        ModSettingsButton.OnClick.AddListener((UnityEngine.Events.UnityAction)(() => ChangeCustomTab(ModSettingsTab, ModSettingsButton)));
        VanillaSettingsButton.OnClick.AddListener((UnityEngine.Events.UnityAction)(() => ChangeCustomTab(VanillaSettingsTab, VanillaSettingsButton)));
        RoleSettingsButton.OnClick.AddListener((UnityEngine.Events.UnityAction)(() => ChangeCustomTab(RoleSettingsTab, RoleSettingsButton)));

    }

    public static void ChangeCustomTab(GameOptionsMenu menu, PassiveButton button)
    {
        ModSettingsTab.gameObject.SetActive(false);
        VanillaSettingsTab.gameObject.SetActive(false);
        RoleSettingsTab.gameObject.SetActive(false);
        
        ModSettingsButton.SelectButton(false);
        VanillaSettingsButton.SelectButton(false);
        RoleSettingsButton.SelectButton(false);

        button.SelectButton(true);
        
        menu.gameObject.SetActive(true);
        
        if (menu == ModSettingsTab) SetModSettingsTab(); 
        if (menu == RoleSettingsTab) SetRoleSettingsTab(); 
    }

    public static MapSelectButton CrewmateButton;
    public static MapSelectButton ImpostorButton;
    public static MapSelectButton NeutralButton;
    public static MapSelectButton CovenButton;
    public static MapSelectButton ModifierButton;

    public static void SetModSettingsTab()
    {
        foreach (var child in ModSettingsTab.Children)
        {
            child.gameObject.SetActive(false);
        }
        ModSettingsTab.MapPicker.gameObject.SetActive(false);
        foreach (var obj in Object.FindObjectsOfType<CategoryHeaderMasked>())
        {
            obj.gameObject.SetActive(false);
        }

        if (!ModSettingsInitialized)
        {
            GameOptionMenuConfig.InitModSettings();
            SetupModSettingsTab();
            ModSettingsInitialized = true;
        }

        foreach (var obj in ModSettingObjects)
        {
            obj.SetActive(true);
        }
    }

    public static void SetRoleSettingsTab()
    { 
        foreach (var child in RoleSettingsTab.Children) 
        { 
            child.gameObject.SetActive(false);
        }

        RoleSettingsTab.MapPicker.gameObject.SetActive(true);
        RoleSettingsTab.MapPicker.Labeltext.DestroyTranslator();
        RoleSettingsTab.MapPicker.Labeltext.text = "Roles";
        
        // Crewmate Tab
        CrewmateButton = RoleSettingsTab.MapPicker.mapButtons[0];
        CrewmateButton.SetImage(Utils.LoadSprite("TOHO.Resources.Images.TabIcon_CrewmateRoles.png", 120f), CrewmateButton.GetComponentInChildren<SpriteRenderer>().material.GetInt(PlayerMaterial.MaskLayer));
        CrewmateButton.Button.OnClick.RemoveAllListeners();
        CrewmateButton.Button.OnClick.AddListener((UnityEngine.Events.UnityAction)(() => SwitchToCrewmateTab()));
        
        // Impostor Tab
        ImpostorButton = RoleSettingsTab.MapPicker.mapButtons[1];
        ImpostorButton.SetImage(Utils.LoadSprite("TOHO.Resources.Images.TabIcon_ImpostorRoles.png", 120f), ImpostorButton.GetComponentInChildren<SpriteRenderer>().material.GetInt(PlayerMaterial.MaskLayer));
        ImpostorButton.Button.OnClick.RemoveAllListeners();
        ImpostorButton.Button.OnClick.AddListener((UnityEngine.Events.UnityAction)(() => SwitchToImpostorTab()));
        
        // Neutral Tab
        NeutralButton = RoleSettingsTab.MapPicker.mapButtons[2];
        NeutralButton.SetImage(Utils.LoadSprite("TOHO.Resources.Images.TabIcon_NeutralRoles.png", 120f), NeutralButton.GetComponentInChildren<SpriteRenderer>().material.GetInt(PlayerMaterial.MaskLayer));
        NeutralButton.Button.OnClick.RemoveAllListeners();
        NeutralButton.Button.OnClick.AddListener((UnityEngine.Events.UnityAction)(() => SwitchToNeutralTab()));
        
        // Coven Tab
        CovenButton = RoleSettingsTab.MapPicker.mapButtons[3];
        CovenButton.SetImage(Utils.LoadSprite("TOHO.Resources.Images.TabIcon_CovenRoles.png", 120f), CovenButton.GetComponentInChildren<SpriteRenderer>().material.GetInt(PlayerMaterial.MaskLayer));
        CovenButton.Button.OnClick.RemoveAllListeners();
        CovenButton.Button.OnClick.AddListener((UnityEngine.Events.UnityAction)(() => SwitchToCovenTab()));
        
        // Modifier Tab
        ModifierButton = RoleSettingsTab.MapPicker.mapButtons[4];
        ModifierButton.SetImage(Utils.LoadSprite("TOHO.Resources.Images.TabIcon_Modifiers.png", 120f), ModifierButton.GetComponentInChildren<SpriteRenderer>().material.GetInt(PlayerMaterial.MaskLayer));
        ModifierButton.Button.OnClick.RemoveAllListeners();
        ModifierButton.Button.OnClick.AddListener((UnityEngine.Events.UnityAction)(() => SwitchToModifierTab()));

        new LateTask(() => { SwitchToCrewmateTab(); }, 0.01f);
        
        SetupCrewmateTab();
        SetupImpostorTab();
        SetupNeutralTab();
        SetupModifierTab();
        SetupCovenTab();
        
        foreach (var obj in Object.FindObjectsOfType<CategoryHeaderMasked>())
        {
            obj.gameObject.SetActive(false);
        }
    }

    public static void SwitchToCrewmateTab()
    {
        RoleSettingsTab.MapPicker.MapImage.sprite = Utils.LoadSprite("TOHO.Resources.Images.CrewmateImage.png", 100f);
        RoleSettingsTab.MapPicker.MapName.sprite = Utils.LoadSprite("TOHO.Resources.Images.CrewmatesText.png", 50f);
        
        CrewmateButton.Button.SelectButton(true);
        ImpostorButton.Button.SelectButton(false);
        NeutralButton.Button.SelectButton(false);
        CovenButton.Button.SelectButton(false);
        ModifierButton.Button.SelectButton(false);
        
        foreach (var obj in CrewmateObjects)
        {
            obj.SetActive(true);
        }
        foreach (var obj in ImpostorObjects)
        {
            obj.SetActive(false);
        }
        foreach (var obj in NeutralObjects)
        {
            obj.SetActive(false);
        }
        foreach (var obj in CovenObjects)
        {
            obj.SetActive(false);
        }
        foreach (var obj in ModifierObjects)
        {
            obj.SetActive(false);
        }
    }
    public static void SwitchToImpostorTab()
    {
        RoleSettingsTab.MapPicker.MapImage.sprite = Utils.LoadSprite("TOHO.Resources.Images.ImpostorImage.png", 100f);
        RoleSettingsTab.MapPicker.MapName.sprite = Utils.LoadSprite("TOHO.Resources.Images.ImpostorsText.png", 50f);
        
        CrewmateButton.Button.SelectButton(false);
        ImpostorButton.Button.SelectButton(true);
        NeutralButton.Button.SelectButton(false);
        CovenButton.Button.SelectButton(false);
        ModifierButton.Button.SelectButton(false);
        
        
        foreach (var obj in CrewmateObjects)
        {
            obj.SetActive(false);
        }
        foreach (var obj in ImpostorObjects)
        {
            obj.SetActive(true);
        }
        foreach (var obj in NeutralObjects)
        {
            obj.SetActive(false);
        }
        foreach (var obj in CovenObjects)
        {
            obj.SetActive(false);
        }
        foreach (var obj in ModifierObjects)
        {
            obj.SetActive(false);
        }
    }
    public static void SwitchToNeutralTab()
    {
        RoleSettingsTab.MapPicker.MapImage.sprite = Utils.LoadSprite("TOHO.Resources.Images.NeutralImage.png", 100f);
        RoleSettingsTab.MapPicker.MapName.sprite = Utils.LoadSprite("TOHO.Resources.Images.NeutralsText.png", 50f);
        
        CrewmateButton.Button.SelectButton(false);
        ImpostorButton.Button.SelectButton(false);
        NeutralButton.Button.SelectButton(true);
        CovenButton.Button.SelectButton(false);
        ModifierButton.Button.SelectButton(false);
        
        foreach (var obj in CrewmateObjects)
        {
            obj.SetActive(false);
        }
        foreach (var obj in ImpostorObjects)
        {
            obj.SetActive(false);
        }
        foreach (var obj in NeutralObjects)
        {
            obj.SetActive(true);
        }
        foreach (var obj in CovenObjects)
        {
            obj.SetActive(false);
        }
        foreach (var obj in ModifierObjects)
        {
            obj.SetActive(false);
        }
    }
    public static void SwitchToCovenTab()
    {
        RoleSettingsTab.MapPicker.MapImage.sprite = Utils.LoadSprite("TOHO.Resources.Images.CovenImage.png", 100f);
        RoleSettingsTab.MapPicker.MapName.sprite = Utils.LoadSprite("TOHO.Resources.Images.CovenText.png", 50f);
        
        CrewmateButton.Button.SelectButton(false);
        ImpostorButton.Button.SelectButton(false);
        NeutralButton.Button.SelectButton(false);
        CovenButton.Button.SelectButton(true);
        ModifierButton.Button.SelectButton(false);
        
        foreach (var obj in CrewmateObjects)
        {
            obj.SetActive(false);
        }
        foreach (var obj in ImpostorObjects)
        {
            obj.SetActive(false);
        }
        foreach (var obj in NeutralObjects)
        {
            obj.SetActive(false);
        }
        foreach (var obj in CovenObjects)
        {
            obj.SetActive(true);
        }
        foreach (var obj in ModifierObjects)
        {
            obj.SetActive(false);
        }
    }
    public static void SwitchToModifierTab()
    {
        RoleSettingsTab.MapPicker.MapImage.sprite = Utils.LoadSprite("TOHO.Resources.Images.ModifierImage.png", 100f);
        RoleSettingsTab.MapPicker.MapName.sprite = Utils.LoadSprite("TOHO.Resources.Images.ModifiersText.png", 50f);
        
        CrewmateButton.Button.SelectButton(false);
        ImpostorButton.Button.SelectButton(false);
        NeutralButton.Button.SelectButton(false);
        CovenButton.Button.SelectButton(false);
        ModifierButton.Button.SelectButton(true);

        foreach (var obj in CrewmateObjects)
        {
            obj.SetActive(false);
        }
        foreach (var obj in ImpostorObjects)
        {
            obj.SetActive(false);
        }
        foreach (var obj in NeutralObjects)
        {
            obj.SetActive(false);
        }
        foreach (var obj in CovenObjects)
        {
            obj.SetActive(false);
        }
        foreach (var obj in ModifierObjects)
        {
            obj.SetActive(true);
        }
    }
    public static void SetupModSettingsTab()
    {
        var y = 1.9f;

        OptionManager.SetupHeader(y, "Ejection", ModSettingObjects, ModSettingsTab, Color.green);

        foreach (var option in BoolModSettings.Where(x => x.OptionTab == OptionTabs.ModSettingsEjection))
        {
            y -= 0.6f;
            option.SetupBooleanOption(y: y);
        }        
        foreach (var option in NumberModSettings.Where(x => x.OptionTab == OptionTabs.ModSettingsEjection))
        {
            y -= 0.6f;
            option.SetupNumberOption(y: y);
        }
        foreach (var option in StringModSettings.Where(x => x.OptionTab == OptionTabs.ModSettingsEjection))
        {
            y -= 0.6f;
            option.SetupStringOption(y: y);
        }

        ModSettingsTab.scrollBar.ContentYBounds = new FloatRange(y, 2f);
        ModSettingsTab.scrollBar.ScrollToTop();
    }

    public static void SetupCrewmateTab()
    {
        var y = 0.9f;

        OptionManager.SetupHeader(y, "Crewmate Vanilla", CrewmateObjects, RoleSettingsTab, Color.cyan);
        
        RoleSettingsTab.scrollBar.ContentYBounds = new FloatRange(y, 2f);
        RoleSettingsTab.scrollBar.ScrollToTop();
    }

    public static void SetupImpostorTab()
    {
        var y = 0.9f;
        
        OptionManager.SetupHeader(y, "Impostor Vanilla", ImpostorObjects, RoleSettingsTab, Color.red);
        
        RoleSettingsTab.scrollBar.ContentYBounds = new FloatRange(y, 2f);
        RoleSettingsTab.scrollBar.ScrollToTop();
    }
    public static void SetupNeutralTab()
    {
        var y = 0.9f;
        
        OptionManager.SetupHeader(y, "Coming soon...", NeutralObjects, RoleSettingsTab, Color.gray);
        
        RoleSettingsTab.scrollBar.ContentYBounds = new FloatRange(y, 2f);
        RoleSettingsTab.scrollBar.ScrollToTop();
    }
    public static void SetupCovenTab()
    {
        var y = 0.9f;
        
        OptionManager.SetupHeader(y, "Coming soon...", CovenObjects, RoleSettingsTab, Color.magenta);
        
        RoleSettingsTab.scrollBar.ContentYBounds = new FloatRange(y, 2f);
        RoleSettingsTab.scrollBar.ScrollToTop();
    }
    public static void SetupModifierTab()
    {
        var y = 0.9f;
        
        OptionManager.SetupHeader(y, "Coming soon...", ModifierObjects, RoleSettingsTab, Color.yellow);
        
        RoleSettingsTab.scrollBar.ContentYBounds = new FloatRange(y, 2f);
        RoleSettingsTab.scrollBar.ScrollToTop();
    }
}
public enum OptionTabs
{
    ModSettingsEjection,
    ModSettingsMaps,
    ModSettingsSabotage,
    ModSettingsMeeting,
    ModSettingsLobby,
    ModSettingsGame,
}

public static class OptionManager
{
    public static void SetupBooleanOption(this BooleanOptionItem option, float y)
    {
        ToggleOption template = null;
        option.SetInitialBooleanValue();
        option.obj = new GameObject();
        
        foreach (var item in Resources.FindObjectsOfTypeAll(Il2CppSystem.Type.GetType("ToggleOption, Assembly-CSharp")))
        {
            template = item.TryCast<ToggleOption>();
            if (template != null) break;
        }

        if (template == null)
        {
            Main.Logger.LogError("[Settings] No ToggleOption template found.");
            return;
        }

        option.obj = Object.Instantiate(template.gameObject, GameSettingMenuPatch.ModSettingsTab.settingsContainer);

        var toggle = option.obj.GetComponent<ToggleOption>();
        toggle.boolOptionName = BoolOptionNames.Invalid;

        option.obj.transform.localPosition = new Vector3(-0.4f, y, -2f);
        option.obj.SetActive(true);

        var text = toggle.TitleText;
        
        new LateTask(() =>
        {
            text.DestroyTranslator();
            text.text = option.OptionName;

            toggle.CheckMark.enabled = option.Value;
            
            GameSettingMenuPatch.ModSettingObjects.Add(option.obj);
        }, 0.01f);
    }
    public static void SetupNumberOption(this NumberOptionItem option, float y)
    {
        NumberOption template = null;
        option.SetInitialNumberValue();
        option.obj = new GameObject();

        foreach (var item in Resources.FindObjectsOfTypeAll(Il2CppSystem.Type.GetType("NumberOption, Assembly-CSharp")))
        {
            template = item.TryCast<NumberOption>();
            if (template != null) break;
        }

        if (template == null)
        {
            Main.Logger.LogError("[Settings] No NumberOption template found.");
            return;
        }

        option.obj = Object.Instantiate(template.gameObject, GameSettingMenuPatch.ModSettingsTab.settingsContainer);

        var number = option.obj.GetComponent<NumberOption>();
        number.floatOptionName = FloatOptionNames.Invalid;

        number.Increment = option.Step;
        number.ValidRange = new FloatRange(option.MinValue, option.MaxValue);
        
        option.obj.transform.localPosition = new Vector3(-0.4f, y, -2f);
        option.obj.SetActive(true);

        var text = number.TitleText;

        new LateTask(() =>
        {
            text.DestroyTranslator();
            text.text = option.OptionName;

            number.Value = option.Value;
            number.ValueText.text = $"{number.Value}";
            GameSettingMenuPatch.ModSettingObjects.Add(option.obj);
        }, 0.01f);
    }
    public static void SetupStringOption(this StringOptionItem option, float y)
    {
        StringOption template = null;
        option.SetInitialStringValue();
        option.obj = new GameObject();

        foreach (var item in Resources.FindObjectsOfTypeAll(Il2CppSystem.Type.GetType("StringOption, Assembly-CSharp")))
        {
            template = item.TryCast<StringOption>();
            if (template != null) break;
        }

        if (template == null)
        {
            Main.Logger.LogError("[Settings] No StringOption template found.");
            return;
        }

        option.obj = Object.Instantiate(template.gameObject, GameSettingMenuPatch.ModSettingsTab.settingsContainer);

        var stringo = option.obj.GetComponent<StringOption>();
        StringOptionItem.OptionMap[stringo] = option;
        
        stringo.stringOptionName = Int32OptionNames.Invalid;

        option.obj.transform.localPosition = new Vector3(-0.4f, y, -2f);
        option.obj.SetActive(true);

        var text = stringo.TitleText;
        StringNames[] arr = [];

        foreach (var item in option.AllValues)
        {
            arr.AddItem<StringNames>(StringNames.Fine);
        }
        
        stringo.Values = new Il2CppStructArray<StringNames>(arr);
        
        new LateTask(() =>
        {
            text.DestroyTranslator();
            text.text = option.OptionName;

            stringo.Value = option.Value;
            stringo.ValueText.text = $"{option.AllValues[stringo.Value]}";
            
            GameSettingMenuPatch.ModSettingObjects.Add(option.obj);
        }, 0.01f);
    }

    public static void SetupHeader(float y, string title, List<GameObject> list, GameOptionsMenu tab, Color color)
    {
        CategoryHeaderMasked header = Object.Instantiate(tab.categoryHeaderOrigin, Vector3.zero, Quaternion.identity, tab.settingsContainer);
        header.SetHeader(StringNames.RolesCategory, 20);
        header.Title.text = "title";
        header.Background.color = header.Divider.color = color;
        header.transform.localScale = Vector3.one * 0.68f;
        header.transform.localPosition = new(-0.9f, y, -2f);
        var chmText = header.transform.FindChild("HeaderText").GetComponent<TextMeshPro>();
        chmText.fontStyle = FontStyles.Bold;
        chmText.outlineWidth = 0.17f;
        header.gameObject.SetActive(false);
        list.Add(header.gameObject);
    }
}