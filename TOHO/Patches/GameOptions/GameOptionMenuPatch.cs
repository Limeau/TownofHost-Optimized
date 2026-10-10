using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;
using TMPro;
using UnityEngine.Events;
using Object = UnityEngine.Object;

namespace TOHO;

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
    
    public static List<RoleOptionItem> CrewmateSettings = [];
    public static List<RoleOptionItem> ImpostorSettings = [];
    public static List<RoleOptionItem> NeutralSettings = [];
    public static List<RoleOptionItem> CovenSettings = [];
    public static List<RoleOptionItem> ModifierSettings = [];
    
    public static List<BooleanOptionItem> BoolModSettings = [];
    public static List<NumberOptionItem> NumberModSettings = [];
    public static List<StringOptionItem> StringModSettings = [];
    
    private static bool ModSettingsInitialized;
    private static bool RoleSettingsInitialized;

    public static void InitializeSettings()
    {
        if (!ModSettingsInitialized)
        {
            Options.InitModSettings();
            SetupModSettingsTab();

            // Build the UI, but don't show it yet.
            foreach (var obj in ModSettingObjects)
                obj.SetActive(false);

            ModSettingsInitialized = true;
        }

        if (!RoleSettingsInitialized)
        {
            InitializeRoleSettings();

            // Start with every role option hidden.
            foreach (var obj in CrewmateObjects
                         .Concat(ImpostorObjects)
                         .Concat(NeutralObjects)
                         .Concat(CovenObjects)
                         .Concat(ModifierObjects))
            {
                obj.SetActive(false);
            }

            RoleSettingsInitialized = true;
        }
    }
    
    private static void InitializeRoleSettings()
    {
        // Move the five button setup blocks here:
        // CrewmateButton, ImpostorButton, NeutralButton,
        // CovenButton, ModifierButton.

        // Keep their existing SetImage and OnClick code.

        SetupCrewmateTab();
        SetupImpostorTab();
        SetupNeutralTab();
        SetupModifierTab();
        SetupCovenTab();
    }
    
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
        
        InitializeSettings();
    }

    public static void ChangeCustomTab(GameOptionsMenu menu, PassiveButton button)
    {
        ModSettingsTab.gameObject.SetActive(false);
        VanillaSettingsTab.gameObject.SetActive(false);
        RoleSettingsTab.gameObject.SetActive(false);
        GameSettingMenu.Instance.RoleSettingsTab.gameObject.SetActive(false);
        
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
            Options.InitModSettings();
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


        if (!RoleSettingsInitialized)
        {

            // Crewmate Tab
            CrewmateButton = RoleSettingsTab.MapPicker.mapButtons[0];
            CrewmateButton.SetImage(Utils.LoadSprite("TOHO.Resources.Images.TabIcon_CrewmateRoles.png", 120f),
                CrewmateButton.GetComponentInChildren<SpriteRenderer>().material.GetInt(PlayerMaterial.MaskLayer));
            CrewmateButton.Button.OnClick.RemoveAllListeners();
            CrewmateButton.Button.OnClick.AddListener((UnityEngine.Events.UnityAction)(() => SwitchToCrewmateTab()));

            // Impostor Tab
            ImpostorButton = RoleSettingsTab.MapPicker.mapButtons[1];
            ImpostorButton.SetImage(Utils.LoadSprite("TOHO.Resources.Images.TabIcon_ImpostorRoles.png", 120f),
                ImpostorButton.GetComponentInChildren<SpriteRenderer>().material.GetInt(PlayerMaterial.MaskLayer));
            ImpostorButton.Button.OnClick.RemoveAllListeners();
            ImpostorButton.Button.OnClick.AddListener((UnityEngine.Events.UnityAction)(() => SwitchToImpostorTab()));

            // Neutral Tab
            NeutralButton = RoleSettingsTab.MapPicker.mapButtons[2];
            NeutralButton.SetImage(Utils.LoadSprite("TOHO.Resources.Images.TabIcon_NeutralRoles.png", 120f),
                NeutralButton.GetComponentInChildren<SpriteRenderer>().material.GetInt(PlayerMaterial.MaskLayer));
            NeutralButton.Button.OnClick.RemoveAllListeners();
            NeutralButton.Button.OnClick.AddListener((UnityEngine.Events.UnityAction)(() => SwitchToNeutralTab()));

            // Coven Tab
            CovenButton = RoleSettingsTab.MapPicker.mapButtons[3];
            CovenButton.SetImage(Utils.LoadSprite("TOHO.Resources.Images.TabIcon_CovenRoles.png", 120f),
                CovenButton.GetComponentInChildren<SpriteRenderer>().material.GetInt(PlayerMaterial.MaskLayer));
            CovenButton.Button.OnClick.RemoveAllListeners();
            CovenButton.Button.OnClick.AddListener((UnityEngine.Events.UnityAction)(() => SwitchToCovenTab()));

            // Modifier Tab
            ModifierButton = RoleSettingsTab.MapPicker.mapButtons[4];
            ModifierButton.SetImage(Utils.LoadSprite("TOHO.Resources.Images.TabIcon_Modifiers.png", 120f),
                ModifierButton.GetComponentInChildren<SpriteRenderer>().material.GetInt(PlayerMaterial.MaskLayer));
            ModifierButton.Button.OnClick.RemoveAllListeners();
            ModifierButton.Button.OnClick.AddListener((UnityEngine.Events.UnityAction)(() => SwitchToModifierTab()));
            
            SetupCrewmateTab();
            SetupImpostorTab();
            SetupNeutralTab();
            SetupModifierTab();
            SetupCovenTab();
            
            RoleSettingsInitialized = true;
        }

        new LateTask(() => { SwitchToCrewmateTab(); }, 0.01f);
        
        
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

        var y = 0.9f;
        
        foreach (var obj in CrewmateObjects)
        {
            obj.SetActive(true);
            y -= 0.6f;
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
        
        RoleSettingsTab.scrollBar.SetYBoundsMin(-2f + 2f);
        RoleSettingsTab.scrollBar.SetYBoundsMax(-y - 1f);
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

    public static void SwitchToAdvancedRoleSettings(CustomRoles role)
    {
        var menu = GameSettingMenu.Instance; 
        var advanced = menu.RoleSettingsTab; 
        advanced.gameObject.SetActive(true);
        RoleSettingsTab.gameObject.SetActive(false);
        advanced.AllButton.ReceiveClickDown();
        advanced.roleTabs[1].ReceiveClickDown();

        foreach (var obj in advanced.roleTabs)
        {
            obj.gameObject.SetActive(false);
        }

        foreach (var obj in advanced.advancedSettingChildren)
        {
            obj.gameObject.SetActive(false);
        }

        var newallbutton = Object.Instantiate(advanced.AllButton, advanced.AllButton.transform.parent);
        
        newallbutton.OnClick.RemoveAllListeners();
        newallbutton.OnClick.AddListener((UnityAction)(() =>
        {
            newallbutton.gameObject.SetActive(false);
            ChangeCustomTab(RoleSettingsTab, RoleSettingsButton);
            switch (role.Category)
            {
                case RoleCategories.CrewmateVanilla:
                case RoleCategories.CrewmateVanillaGhost:
                    new LateTask(() =>
                    {
                        SwitchToCrewmateTab();
                    }, 0.02f);                   
                    break;
                case RoleCategories.ImpostorVanilla:
                    new LateTask(() =>
                    {
                        SwitchToImpostorTab();
                    }, 0.02f); 
                    break;
            }
        }));
        newallbutton.gameObject.SetActive(true);
        
        advanced.roleHeaderText.text = role.RoleName;
        advanced.roleScreenshot.sprite = Utils.LoadSprite(role.ScreenshotPath, 500f);
        advanced.roleDescriptionText.text = role.LongDescription;
        ColorUtility.TryParseHtmlString(role.RoleColor, out var c);
        advanced.roleHeaderText.color = c;
        _ = ColorUtility.TryParseHtmlString("#000000", out var c2);
        advanced.roleHeaderSprite.color = Color.Lerp(c, c2, 0.5f);

        foreach (var oldrole in Main.AllRoles.Values.Where(x => x != role))
        {
            if (oldrole.IsOptionIninialized) oldrole.StopAdvancedOptions();
        }
        
        if (!role.IsOptionIninialized)
        {
            role.SetupAdvancedOptions(-0.9f);
            role.IsOptionIninialized = true;
        }

        role.StartAdvancedOptions();
    }


    public static void SetupModSettingsTab()
    {
        var y = 1.9f;
        
        OptionManager.SetupHeader(y, "Ejection Settings", ModSettingObjects, ModSettingsTab, Color.green);

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

        y -= 0.6f;
        OptionManager.SetupHeader(y, "Game Settings", ModSettingObjects, ModSettingsTab, Color.green);

        foreach (var option in BoolModSettings.Where(x => x.OptionTab == OptionTabs.ModSettingsGame))
        {
            y -= 0.6f;
            option.SetupBooleanOption(y: y);
        }        
        foreach (var option in NumberModSettings.Where(x => x.OptionTab == OptionTabs.ModSettingsGame))
        {
            y -= 0.6f;
            option.SetupNumberOption(y: y);
        }
        foreach (var option in StringModSettings.Where(x => x.OptionTab == OptionTabs.ModSettingsGame))
        {
            y -= 0.6f;
            option.SetupStringOption(y: y);
        }
        
        y -= 0.6f;
        OptionManager.SetupHeader(y, "Lobby Settings", ModSettingObjects, ModSettingsTab, Color.green);

        foreach (var option in BoolModSettings.Where(x => x.OptionTab == OptionTabs.ModSettingsLobby))
        {
            y -= 0.6f;
            option.SetupBooleanOption(y: y);
        }        
        foreach (var option in NumberModSettings.Where(x => x.OptionTab == OptionTabs.ModSettingsLobby))
        {
            y -= 0.6f;
            option.SetupNumberOption(y: y);
        }
        foreach (var option in StringModSettings.Where(x => x.OptionTab == OptionTabs.ModSettingsLobby))
        {
            y -= 0.6f;
            option.SetupStringOption(y: y);
        }
        ModSettingsTab.scrollBar.SetYBoundsMin(-2f + 2f);
        ModSettingsTab.scrollBar.SetYBoundsMax(-y - 1f);
    }

    public static void SetupCrewmateTab()
    {
        var y = 0.9f;

        OptionManager.SetupHeader(y, "Crewmate Vanilla", CrewmateObjects, RoleSettingsTab, Color.cyan);
        
        foreach (var option in CrewmateSettings.Where(x => x.OptionTab == RoleCategories.CrewmateVanilla))
        {
            y -= 0.6f;
            option.SetupRoleOption(CrewmateObjects, y);
        }
        y -= 0.6f;
        OptionManager.SetupHeader(y, "Crewmate Vanilla Ghost", CrewmateObjects, RoleSettingsTab, Color.cyan);
        
        foreach (var option in CrewmateSettings.Where(x => x.OptionTab == RoleCategories.CrewmateVanillaGhost))
        {
            y -= 0.6f;
            option.SetupRoleOption(CrewmateObjects, y);
        }

        if (CrewmateSettings.Any(x => x.OptionTab == RoleCategories.CrewmateHindering))
        {       
            y -= 0.6f;
            OptionManager.SetupHeader(y, "Crewmate Hindering", CrewmateObjects, RoleSettingsTab, Color.cyan);
            foreach (var option in CrewmateSettings.Where(x => x.OptionTab == RoleCategories.CrewmateHindering))
            {
                y -= 0.6f;
                option.SetupRoleOption(CrewmateObjects, y);
            }
        }
        if (CrewmateSettings.Any(x => x.OptionTab == RoleCategories.CrewmateInvestigative))
        {       
            y -= 0.6f;
            OptionManager.SetupHeader(y, "Crewmate Investigative", CrewmateObjects, RoleSettingsTab, Color.cyan);
            foreach (var option in CrewmateSettings.Where(x => x.OptionTab == RoleCategories.CrewmateInvestigative))
            {
                y -= 0.6f;
                option.SetupRoleOption(CrewmateObjects, y);
            }
        }
        if (CrewmateSettings.Any(x => x.OptionTab == RoleCategories.CrewmateKilling))
        {       
            y -= 0.6f;
            OptionManager.SetupHeader(y, "Crewmate Killing", CrewmateObjects, RoleSettingsTab, Color.cyan);
            foreach (var option in CrewmateSettings.Where(x => x.OptionTab == RoleCategories.CrewmateKilling))
            {
                y -= 0.6f;
                option.SetupRoleOption(CrewmateObjects, y);
            }
        }
        if (CrewmateSettings.Any(x => x.OptionTab == RoleCategories.CrewmatePower))
        {       
            y -= 0.6f;
            OptionManager.SetupHeader(y, "Crewmate Power", CrewmateObjects, RoleSettingsTab, Color.cyan);
            foreach (var option in CrewmateSettings.Where(x => x.OptionTab == RoleCategories.CrewmatePower))
            {
                y -= 0.6f;
                option.SetupRoleOption(CrewmateObjects, y);
            }
        }
        if (CrewmateSettings.Any(x => x.OptionTab == RoleCategories.CrewmateSupport))
        {       
            y -= 0.6f;
            OptionManager.SetupHeader(y, "Crewmate Support", CrewmateObjects, RoleSettingsTab, Color.cyan);
            foreach (var option in CrewmateSettings.Where(x => x.OptionTab == RoleCategories.CrewmateSupport))
            {
                y -= 0.6f;
                option.SetupRoleOption(CrewmateObjects, y);
            }
        }
        
        if (CrewmateSettings.Any(x => x.OptionTab == RoleCategories.CrewmateGhosts))
        {       
            y -= 0.6f;
            OptionManager.SetupHeader(y, "Crewmate Ghosts", CrewmateObjects, RoleSettingsTab, Color.cyan);
            foreach (var option in CrewmateSettings.Where(x => x.OptionTab == RoleCategories.CrewmateGhosts))
            {
                y -= 0.6f;
                option.SetupRoleOption(CrewmateObjects, y);
            }
        }
    }

    public static void SetupImpostorTab()
    {
        var y = 0.9f;
        
        OptionManager.SetupHeader(y, "Impostor Vanilla", ImpostorObjects, RoleSettingsTab, Color.red);
        
        foreach (var option in ImpostorSettings.Where(x => x.OptionTab == RoleCategories.ImpostorVanilla))
        {
            y -= 0.6f;
            option.SetupRoleOption(ImpostorObjects, y);
        }
        if (ImpostorSettings.Any(x => x.OptionTab == RoleCategories.ImpostorConcealing))
        {       
            y -= 0.6f;
            OptionManager.SetupHeader(y, "Impostor Concealing", ImpostorObjects, RoleSettingsTab, Color.red);
            foreach (var option in ImpostorSettings.Where(x => x.OptionTab == RoleCategories.ImpostorConcealing))
            {
                y -= 0.6f;
                option.SetupRoleOption(ImpostorObjects, y);
            }
        }
        if (ImpostorSettings.Any(x => x.OptionTab == RoleCategories.ImpostorHindering))
        {       
            y -= 0.6f;
            OptionManager.SetupHeader(y, "Impostor Hindering", ImpostorObjects, RoleSettingsTab, Color.red);
            foreach (var option in ImpostorSettings.Where(x => x.OptionTab == RoleCategories.ImpostorHindering))
            {
                y -= 0.6f;
                option.SetupRoleOption(ImpostorObjects, y);
            }
        }
        if (ImpostorSettings.Any(x => x.OptionTab == RoleCategories.ImpostorKilling))
        {       
            y -= 0.6f;
            OptionManager.SetupHeader(y, "Impostor Killing", ImpostorObjects, RoleSettingsTab, Color.red);
            foreach (var option in ImpostorSettings.Where(x => x.OptionTab == RoleCategories.ImpostorKilling))
            {
                y -= 0.6f;
                option.SetupRoleOption(ImpostorObjects, y);
            }
        }
        if (ImpostorSettings.Any(x => x.OptionTab == RoleCategories.ImpostorSupport))
        {                   
            y -= 0.6f;
            OptionManager.SetupHeader(y, "Impostor Support", ImpostorObjects, RoleSettingsTab, Color.red);
            foreach (var option in ImpostorSettings.Where(x => x.OptionTab == RoleCategories.ImpostorSupport))
            {
                y -= 0.6f;
                option.SetupRoleOption(ImpostorObjects, y);
            }
        }
        if (ImpostorSettings.Any(x => x.OptionTab == RoleCategories.ImpostorGhosts))
        {       
            y -= 0.6f;
            OptionManager.SetupHeader(y, "Impostor Ghosts", ImpostorObjects, RoleSettingsTab, Color.red);
            foreach (var option in ImpostorSettings.Where(x => x.OptionTab == RoleCategories.ImpostorGhosts))
            {
                y -= 0.6f;
                option.SetupRoleOption(ImpostorObjects, y);
            }
        }
    }
    public static void SetupNeutralTab()
    {
        var y = 1.5f;
        
        if (NeutralSettings.Any(x => x.OptionTab == RoleCategories.NeutralBenign))
        {       
            y -= 0.6f;
            OptionManager.SetupHeader(y, "Neutral Benign", NeutralObjects, RoleSettingsTab, Color.gray);
            foreach (var option in NeutralSettings.Where(x => x.OptionTab == RoleCategories.NeutralBenign))
            {
                y -= 0.6f;
                option.SetupRoleOption(NeutralObjects, y);
            }
        }
        if (NeutralSettings.Any(x => x.OptionTab == RoleCategories.NeutralChaos))
        {       
            y -= 0.6f;
            OptionManager.SetupHeader(y, "Neutral Chaos", NeutralObjects, RoleSettingsTab, Color.gray);
            foreach (var option in NeutralSettings.Where(x => x.OptionTab == RoleCategories.NeutralChaos))
            {
                y -= 0.6f;
                option.SetupRoleOption(NeutralObjects, y);
            }
        }
        if (NeutralSettings.Any(x => x.OptionTab == RoleCategories.NeutralEvil))
        {       
            y -= 0.6f;
            OptionManager.SetupHeader(y, "Neutral Evil", NeutralObjects, RoleSettingsTab, Color.gray);
            foreach (var option in NeutralSettings.Where(x => x.OptionTab == RoleCategories.NeutralEvil))
            {
                y -= 0.6f;
                option.SetupRoleOption(NeutralObjects, y);
            }
        }
        if (NeutralSettings.Any(x => x.OptionTab == RoleCategories.NeutralKilling))
        {       
            y -= 0.6f;
            OptionManager.SetupHeader(y, "Neutral Killing", NeutralObjects, RoleSettingsTab, Color.gray);
            foreach (var option in NeutralSettings.Where(x => x.OptionTab == RoleCategories.NeutralKilling))
            {
                y -= 0.6f;
                option.SetupRoleOption(NeutralObjects, y);
            }
        }
        if (NeutralSettings.Any(x => x.OptionTab == RoleCategories.NeutralGhosts))
        {       
            y -= 0.6f;
            OptionManager.SetupHeader(y, "Neutral Ghosts", NeutralObjects, RoleSettingsTab, Color.gray);
            foreach (var option in NeutralSettings.Where(x => x.OptionTab == RoleCategories.NeutralGhosts))
            {
                y -= 0.6f;
                option.SetupRoleOption(NeutralObjects, y);
            }
        }
    }
    public static void SetupCovenTab()
    {
        var y = 1.5f;

        if (CovenSettings.Any(x => x.OptionTab == RoleCategories.CovenKilling))
        {       
            y -= 0.6f;
            OptionManager.SetupHeader(y, "Coven Killing", CovenObjects, RoleSettingsTab, Color.magenta);
            foreach (var option in CovenSettings.Where(x => x.OptionTab == RoleCategories.CovenKilling))
            {
                y -= 0.6f;
                option.SetupRoleOption(CovenObjects, y);
            }
        }   

        if (CovenSettings.Any(x => x.OptionTab == RoleCategories.CovenPower))
        {       
            y -= 0.6f;
            OptionManager.SetupHeader(y, "Coven Power", CovenObjects, RoleSettingsTab, Color.magenta);
            foreach (var option in CovenSettings.Where(x => x.OptionTab == RoleCategories.CovenPower))
            {
                y -= 0.6f;
                option.SetupRoleOption(CovenObjects, y);
            }
        }   

        if (CovenSettings.Any(x => x.OptionTab == RoleCategories.CovenTrickery))
        {       
            y -= 0.6f;
            OptionManager.SetupHeader(y, "Coven Trickery", CovenObjects, RoleSettingsTab, Color.magenta);
            foreach (var option in CovenSettings.Where(x => x.OptionTab == RoleCategories.CovenTrickery))
            {
                y -= 0.6f;
                option.SetupRoleOption(CovenObjects, y);
            }
        }   

        if (CovenSettings.Any(x => x.OptionTab == RoleCategories.CovenUtility))
        {       
            y -= 0.6f;
            OptionManager.SetupHeader(y, "Coven Utility", CovenObjects, RoleSettingsTab, Color.magenta);
            foreach (var option in CovenSettings.Where(x => x.OptionTab == RoleCategories.CovenUtility))
            {
                y -= 0.6f;
                option.SetupRoleOption(CovenObjects, y);
            }
        }    
    }
    public static void SetupModifierTab()
    {
        var y = 0.9f;
        
        OptionManager.SetupHeader(y, "Modifiers", ModifierObjects, RoleSettingsTab, Color.yellow);
        foreach (var option in ModifierSettings.Where(x => x.OptionTab == RoleCategories.Modifier))
        {
            y -= 0.6f;
            option.SetupRoleOption(ModifierObjects, y);
        }
    }
}
public enum OptionTabs
{
    ModSettingsEjection,
    ModSettingsGame,
    ModSettingsLobby,
    RoleSetting
}
public enum RoleCategories
{
    CrewmateVanilla,
    CrewmateVanillaGhost,
    CrewmateHindering,
    CrewmateInvestigative,
    CrewmateSupport,
    CrewmateKilling,
    CrewmatePower,
    CrewmateGhosts,
    
    ImpostorVanilla,
    ImpostorKilling,
    ImpostorSupport,
    ImpostorConcealing,
    ImpostorHindering,
    ImpostorGhosts,
    
    NeutralBenign,
    NeutralEvil,
    NeutralChaos,
    NeutralKilling,
    NeutralGhosts,
    
    CovenPower,
    CovenKilling,
    CovenTrickery,
    CovenUtility,
    
    Modifier
}