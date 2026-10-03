using System;
using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using HarmonyLib;
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
    
    public static List<CustomOption> CrewmateSettings = [];
    public static List<CustomOption> ImpostorSettings = [];
    public static List<CustomOption> NeutralSettings = [];
    public static List<CustomOption> CovenSettings = [];
    public static List<CustomOption> ModifierSettings = [];
    public static List<CustomOption> ModSettings = [];

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

        var testoption = new CustomOption
        {
            OptionName = "Example",
            OptionType = OptionTypes.Bool,
            OptionTab = OptionTabs.ModSettings,
            Value = false,
            DefaultValue = false,
        
            OnValueChanged = value =>
            {
                Main.Logger.LogInfo($"{value}")
            }
        }
        ModSettings.Add(testoption);
        
        SetupModSettingsTab();
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

        new LateTask(() => { SwitchToCrewmateTab(); }, 0.1f);
        
        SetupCrewmateTab();
        SetupImpostorTab();
        
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
    
    public static void SetupOption(this CustomOption option, float y)
    {
        switch (option.OptionType)
        {
            case OptionTypes.Bool:
                option.obj = Object.Instantiate(GameObject.Find("GameOption_Checkbox"));
                var boolopt = option.obj.GetComponent<ToggleOption>();
                boolopt.boolOptionName = BoolOptionNames.Invalid;
                option.obj.GetComponentInChildren<TextMeshPro>().text = option.OptionName;
                option.obj.transform.localPosition = new(-0.9f, y, -2f);
                break;
            case OptionTypes.Number:
                option.obj = Object.Instantiate(GameObject.Find("GameOption_Number"));
                var numberopt = option.obj.AddComponent<NumberOption>();
                numberopt.floatOptionName = FloatOptionNames.Invalid;
                option.obj.transform.localPosition = new(-0.9f, y, -2f);
                break;
            case OptionTypes.String:
                option.obj = Object.Instantiate(GameObject.Find("GameOption_String"));
                var stringopt = option.obj.AddComponent<StringOption>();
                stringopt.stringOptionName = Int32OptionNames.Invalid;
                option.obj.transform.localPosition = new(-0.9f, y, -2f);
                break;
        }
    }

    public static void SetupModSettingsTab()
    {
        var y = 1.9f;

        CategoryHeaderMasked vanilla = Object.Instantiate(RoleSettingsTab.categoryHeaderOrigin, Vector3.zero,
            Quaternion.identity, RoleSettingsTab.settingsContainer);
        vanilla.SetHeader(StringNames.RolesCategory, 20);
        vanilla.Title.text = "Ejection";
        vanilla.Background.color = vanilla.Divider.color = Color.green;
        vanilla.transform.localScale = Vector3.one * 0.68f;
        vanilla.transform.localPosition = new(-0.9f, y, -2f);
        var chmText = vanilla.transform.FindChild("HeaderText").GetComponent<TextMeshPro>();
        chmText.fontStyle = FontStyles.Bold;
        chmText.outlineWidth = 0.17f;
        vanilla.gameObject.SetActive(false);
        CrewmateObjects.Add(vanilla.gameObject);

        foreach (var option in ModSettings)
        {
            y -= 0.5f;
            option.SetupOption(y);
        }
    }

    public static void SetupCrewmateTab()
    {
        CategoryHeaderMasked vanilla = Object.Instantiate(RoleSettingsTab.categoryHeaderOrigin, Vector3.zero, Quaternion.identity, RoleSettingsTab.settingsContainer);
        vanilla.SetHeader(StringNames.RolesCategory, 20);
        vanilla.Title.text = "Crewmate Vanilla";
        vanilla.Background.color = vanilla.Divider.color = Color.cyan;
        vanilla.transform.localScale = Vector3.one * 0.68f;
        vanilla.transform.localPosition = new(-0.9f, 0.9f, -2f);
        var chmText = vanilla.transform.FindChild("HeaderText").GetComponent<TextMeshPro>();
        chmText.fontStyle = FontStyles.Bold;
        chmText.outlineWidth = 0.17f;
        vanilla.gameObject.SetActive(false);
        CrewmateObjects.Add(vanilla.gameObject);
    }

    public static void SetupImpostorTab()
    {
        CategoryHeaderMasked vanilla = Object.Instantiate(RoleSettingsTab.categoryHeaderOrigin, Vector3.zero, Quaternion.identity, RoleSettingsTab.settingsContainer);
        vanilla.SetHeader(StringNames.RolesCategory, 20);
        vanilla.Title.text = "Impostor Vanilla";
        vanilla.Background.color = vanilla.Divider.color = Color.red;
        vanilla.transform.localScale = Vector3.one * 0.68f;
        vanilla.transform.localPosition = new(-0.9f, 0.9f, -2f);
        var ihmText = vanilla.transform.FindChild("HeaderText").GetComponent<TextMeshPro>();
        ihmText.fontStyle = FontStyles.Bold;
        ihmText.outlineWidth = 0.17f;
        vanilla.gameObject.SetActive(false);
        ImpostorObjects.Add(vanilla.gameObject);
    }
}

public class CustomOption
{
    public OptionTypes OptionType;
    public OptionTabs OptionTab;
    public string OptionName;
    public GameObject obj;
    
    public object Value;
    public object DefaultValue;

    public UnityAction<object> OnValueChanged;
    
    public void SetValue(object value)
    {
        if (Equals(Value, value))
            return;

        Value = value;
        OnValueChanged?.Invoke(Value);
    }
}

public enum OptionTypes
{
    Bool,
    Number,
    String,
}

public enum OptionTabs
{
    Crewmate,
    Impostor,
    Neutral,
    Coven,
    Modifier,
    ModSettings
}

[HarmonyPatch(typeof(ToggleOption), nameof(ToggleOption.UpdateValue))]
public static class BoolTogglePatch
{
    public static void Prefix(ToggleOption __instance)
    {
        foreach (var option in GameSettingMenuPatch.ModSettings)
        {
            if (option.obj == null)
                continue;

            if (!option.obj.TryGetComponent<ToggleOption>(
                    out var toggleOption))
                continue;

            if (toggleOption != __instance)
                continue;

            option.SetValue(__instance.GetBool());

            break;
        }
    }
}