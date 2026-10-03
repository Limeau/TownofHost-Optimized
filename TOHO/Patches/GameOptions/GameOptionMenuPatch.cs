using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;
using TMPro;
using TOHO;
using Object = UnityEngine.Object;

[HarmonyPatch(typeof(GameSettingMenu))]
public class GameSettingMenuPatch
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
        
        if (menu == RoleSettingsTab) SetRoleSettingsTab(); 
    }

    public static MapSelectButton CrewmateButton;
    public static MapSelectButton ImpostorButton;
    public static MapSelectButton NeutralButton;
    public static MapSelectButton CovenButton;
    public static MapSelectButton ModifierButton;

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
        
        SetupTabs();
        
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

    public static void SetupTabs()
    {
        CategoryHeaderMasked crewmateVanilla = Object.Instantiate(RoleSettingsTab.categoryHeaderOrigin, Vector3.zero, Quaternion.identity, RoleSettingsTab.settingsContainer);
        crewmateVanilla.SetHeader(StringNames.RolesCategory, 20);
        crewmateVanilla.Title.text = "Crewmate Vanilla";
        crewmateVanilla.Background.color = crewmateVanilla.Divider.color = Color.cyan;
        crewmateVanilla.transform.localScale = Vector3.one * 0.68f;
        crewmateVanilla.transform.localPosition = new(-0.9f, 0.9f, -2f);
        var chmText = crewmateVanilla.transform.FindChild("HeaderText").GetComponent<TextMeshPro>();
        chmText.fontStyle = FontStyles.Bold;
        chmText.outlineWidth = 0.17f;
        crewmateVanilla.gameObject.SetActive(false);
        CrewmateObjects.Add(crewmateVanilla.gameObject);
        
        CategoryHeaderMasked impostorVanilla = Object.Instantiate(RoleSettingsTab.categoryHeaderOrigin, Vector3.zero, Quaternion.identity, RoleSettingsTab.settingsContainer);
        impostorVanilla.SetHeader(StringNames.RolesCategory, 20);
        impostorVanilla.Title.text = "Impostor Vanilla";
        impostorVanilla.Background.color = impostorVanilla.Divider.color = Color.red;
        impostorVanilla.transform.localScale = Vector3.one * 0.68f;
        impostorVanilla.transform.localPosition = new(-0.9f, 0.9f, -2f);
        var ihmText = impostorVanilla.transform.FindChild("HeaderText").GetComponent<TextMeshPro>();
        ihmText.fontStyle = FontStyles.Bold;
        ihmText.outlineWidth = 0.17f;
        impostorVanilla.gameObject.SetActive(false);
        ImpostorObjects.Add(impostorVanilla.gameObject);
    }
}