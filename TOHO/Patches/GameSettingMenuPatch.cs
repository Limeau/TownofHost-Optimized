using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using TMPro;
using UnityEngine;
using static TOHO.Translator;
using Object = UnityEngine.Object;

namespace TOHO;

// Thanks: https://github.com/Yumenopai/TownOfHost_Y/blob/main/Patches/GameSettingMenuPatch.cs
[HarmonyPatch(typeof(GameSettingMenu))]
public class GameSettingMenuPatch
{
    private static readonly Vector3 ButtonPositionLeft = new(-3.9f, -0.4f, 0f);
    private static readonly Vector3 ButtonPositionRight = new(-2.4f, -0.4f, 0f);

    private static readonly Vector3 ButtonSize = new(0.45f, 0.6f, 1f);

    private static GameOptionsMenu TemplateGameOptionsMenu;
    private static PassiveButton TemplateGameSettingsButton;

    static Dictionary<TabGroup, PassiveButton> ModSettingsButtons = [];
    static Dictionary<TabGroup, GameOptionsMenu> ModSettingsTabs = [];
    public static GameSettingMenu Instance;

    [HarmonyPatch(nameof(GameSettingMenu.Start)), HarmonyPrefix]
    [HarmonyPriority(Priority.First)]
    public static void StartPostfix(GameSettingMenu __instance)
    {
        Instance = __instance;

        TabGroup[] ExludeList = Options.CurrentGameMode switch
        {
            CustomGameMode.HidenSeekTOHO => Enum.GetValues<TabGroup>().Skip(3).ToArray(),
            CustomGameMode.FFA => Enum.GetValues<TabGroup>().Skip(2).ToArray(),
            CustomGameMode.KOTH => Enum.GetValues<TabGroup>().Skip(2).ToArray(),
            CustomGameMode.UltimateTeam => Enum.GetValues<TabGroup>().Skip(2).ToArray(),
            CustomGameMode.FourCorners => Enum.GetValues<TabGroup>().Skip(2).ToArray(),
            CustomGameMode.CandR => Enum.GetValues<TabGroup>().Skip(2).ToArray(),
            _ => []
        };
        
        SetUpPresetPickerButtons(__instance);
        
        var presetButton = __instance.GamePresetsButton;
        var pos = presetButton.transform.localPosition;
        pos = __instance.GamePresetsButton.transform.parent.parent.FindChild("GameSettingsLabel").transform.localPosition;
        pos.x = 2.4f;
        pos.y = -2.5f;
        pos.z = -5f;
        presetButton.transform.localPosition = pos;
        
        var plabel = presetButton.GetComponentInChildren<TextMeshPro>();
        plabel.DestroyTranslator();
        plabel.fontStyle = FontStyles.UpperCase;
        plabel.text = "<color=#ffffff>" + GetString("PresetButton") + "</color>";
        presetButton.inactiveSprites.GetComponent<SpriteRenderer>().color = new Color(0, 0, 255);
        presetButton.activeSprites.GetComponent<SpriteRenderer>().color = new Color(0, 0, 255);
        presetButton.selectedSprites.GetComponent<SpriteRenderer>().color = new Color(0, 0, 255);
        presetButton.transform.localScale = ButtonSize;
        presetButton.OnClick.AddListener((UnityEngine.Events.UnityAction)(() => __instance.ChangeTab(1, false)));

        // https://gyazo.com/a8f6ec93e44eca8e6febb7d2e91c3750 So much empy space, I think this definetevly calls for HnS roles 😼

        ModSettingsButtons = [];
        foreach (var tab in EnumHelper.GetAllValues<TabGroup>().Except(ExludeList))
        {
            var button = Object.Instantiate(TemplateGameSettingsButton, __instance.GameSettingsButton.transform.parent);
            button.gameObject.SetActive(true);
            button.name = "Button_" + tab;
            var label = button.GetComponentInChildren<TextMeshPro>();
            label.DestroyTranslator();
            string htmlcolor = tab switch
            {
                TabGroup.SystemSettings => Main.ModColor,
                TabGroup.ModSettings => "#59ef83",
                TabGroup.ImpostorRoles => "#f74631",
                TabGroup.CrewmateRoles => "#8cffff",
                TabGroup.NeutralRoles => "#7f8c8d",
                TabGroup.CovenRoles => "#ac42f2",
                TabGroup.Modifiers => "#ffff00",
                _ => "#ffffff",
            };
            label.fontStyle = FontStyles.UpperCase;
            label.text = $"<color={htmlcolor}>{GetString("TabGroup." + tab)}</color>";

            _ = ColorUtility.TryParseHtmlString(htmlcolor, out Color tabColor);
            button.inactiveSprites.GetComponent<SpriteRenderer>().color = tabColor;
            button.activeSprites.GetComponent<SpriteRenderer>().color = tabColor;
            button.selectedSprites.GetComponent<SpriteRenderer>().color = tabColor;

            Vector3 offset = new(0.0f, 0.5f * (((int)tab + 1) / 2), 0.0f);
            button.transform.localPosition = ((((int)tab + 1) % 2 == 0) ? ButtonPositionLeft : ButtonPositionRight) - offset;
            button.transform.localScale = ButtonSize;

            var buttonComponent = button.GetComponent<PassiveButton>();
            buttonComponent.OnClick = new();
            buttonComponent.OnClick.AddListener(
                (UnityEngine.Events.UnityAction)(() => __instance.ChangeTab((int)tab + 3, false)));

            ModSettingsButtons.Add(tab, button);
        }

        ModSettingsTabs = [];
        foreach (var tab in EnumHelper.GetAllValues<TabGroup>())
        {
            GameOptionsMenu setTab;
            setTab = Object.Instantiate(TemplateGameOptionsMenu, __instance.GameSettingsTab.transform.parent);
            setTab.name = "tab_" + tab;
            setTab.gameObject.SetActive(false);

            ModSettingsTabs.Add(tab, setTab);
        }

        foreach (var tab in EnumHelper.GetAllValues<TabGroup>())
        {
            if (ModSettingsButtons.TryGetValue(tab, out var button))
            {
                __instance.ControllerSelectable.Add(button);
            }
        }

        HiddenBySearch.Do(x => x.SetHidden(false));
        HiddenBySearch.Clear();

        SetupAdittionalButtons(__instance);
    }
    private static void SetDefaultButton(GameSettingMenu __instance)
    {
        var gameSettingButton = __instance.GameSettingsButton;
        gameSettingButton.transform.localPosition = new(-3f, -0.5f, 0f);

        var textLabel = gameSettingButton.GetComponentInChildren<TextMeshPro>();
        textLabel.DestroyTranslator();
        textLabel.fontStyle = FontStyles.UpperCase;
        textLabel.text = GetString("TabVanilla.GameSettings");

        var optionMenu = GameObject.Find("PlayerOptionsMenu(Clone)");
        var menuDescription = optionMenu?.transform.FindChild("What Is This?");

        var infoImage = menuDescription.transform.FindChild("InfoImage");
        infoImage.transform.localPosition = new(-4.65f, 0.16f, -1f);
        infoImage.transform.localScale = new(0.2202f, 0.2202f, 0.3202f);

        var infoText = menuDescription.transform.FindChild("InfoText");
        infoText.transform.localPosition = new(-3.5f, 0.83f, -2f);
        infoText.transform.localScale = new(1f, 1f, 1f);

        var cubeObject = menuDescription.transform.FindChild("Cube");
        cubeObject.transform.localPosition = new(-3.2f, 0.55f, -0.1f);
        cubeObject.transform.localScale = new(0.61f, 0.64f, 1f);

        __instance.MenuDescriptionText.m_marginWidth = 2.5f;

        gameSettingButton.transform.localPosition = ButtonPositionLeft;
        gameSettingButton.transform.localScale = ButtonSize;

        __instance.RoleSettingsButton.gameObject.SetActive(false);

        __instance.DefaultButtonSelected = gameSettingButton;
        __instance.ControllerSelectable = new();
        __instance.ControllerSelectable.Add(gameSettingButton);
    }
    public static StringOption PresetBehaviour;
    public static FreeChatInputField InputField;
    public static List<OptionItem> HiddenBySearch = [];
    public static Action _SearchForOptions;

    public static void SetUpPresetPickerButtons(GameSettingMenu instance)
    {
        // Beginner
        var BpresetPickerButton = Object.Instantiate(instance.GamePresetsButton, instance.GamePresetsButton.transform.parent);
        var posB = BpresetPickerButton.transform.localPosition;
        posB = instance.GamePresetsButton.transform.parent.parent.FindChild("GameSettingsLabel").transform.localPosition;
        posB.x = -3f;
        posB.y = 2.5f;
        posB.z = -5f;
        BpresetPickerButton.transform.localPosition = posB;
        var pkblabel = BpresetPickerButton.GetComponentInChildren<TextMeshPro>();
        pkblabel.DestroyTranslator();
        pkblabel.fontStyle = FontStyles.UpperCase;
        pkblabel.text = "<color=#ffffff>" + GetString("EasyButton") + "</color>";
        BpresetPickerButton.inactiveSprites.GetComponent<SpriteRenderer>().color = new Color(0, 255, 0);
        BpresetPickerButton.activeSprites.GetComponent<SpriteRenderer>().color = new Color(0, 100, 0);
        BpresetPickerButton.selectedSprites.GetComponent<SpriteRenderer>().color = new Color(0, 150, 0);
        BpresetPickerButton.transform.localScale = ButtonSize;
        BpresetPickerButton.OnClick.AddListener((UnityEngine.Events.UnityAction)(() =>
                {
                    foreach (var option in Options.CustomRoleSpawnChances)
                    {
                        if (BeginnerRoles.Contains(option.Key)) option.Value.SetValue(100, false);
                        else option.Value.SetValue(0, false);                    }
                    GameOptionsMenuPatch.ReOpenSettings();
                }
            ));
        // Intermediate
        var IpresetPickerButton = Object.Instantiate(instance.GamePresetsButton, instance.GamePresetsButton.transform.parent);
        var posI = IpresetPickerButton.transform.localPosition;
        posI = instance.GamePresetsButton.transform.parent.parent.FindChild("GameSettingsLabel").transform.localPosition;
        posI.x = 0f;
        posI.y = 2.5f;
        posI.z = -5f;
        IpresetPickerButton.transform.localPosition = posI;
        var pkilabel = IpresetPickerButton.GetComponentInChildren<TextMeshPro>();
        pkilabel.DestroyTranslator();
        pkilabel.fontStyle = FontStyles.UpperCase;
        pkilabel.text = "<color=#ffffff>" + GetString("ModerateButton") + "</color>";
        IpresetPickerButton.inactiveSprites.GetComponent<SpriteRenderer>().color = new Color(0, 0, 255);
        IpresetPickerButton.activeSprites.GetComponent<SpriteRenderer>().color = new Color(0, 0, 100);
        IpresetPickerButton.selectedSprites.GetComponent<SpriteRenderer>().color = new Color(0, 0, 150);
        IpresetPickerButton.transform.localScale = ButtonSize;
        IpresetPickerButton.OnClick.AddListener((UnityEngine.Events.UnityAction)(() =>
                {
                    foreach (var option in Options.CustomRoleSpawnChances)
                    {
                        if (BeginnerRoles.Contains(option.Key) || IntermediateRoles.Contains(option.Key)) option.Value.SetValue(100, false);
                        else option.Value.SetValue(0, false);                    }
                    GameOptionsMenuPatch.ReOpenSettings();
                }
            ));
        // Advanced
        var ApresetPickerButton = Object.Instantiate(instance.GamePresetsButton, instance.GamePresetsButton.transform.parent);
        var posA = ApresetPickerButton.transform.localPosition;
        posA = instance.GamePresetsButton.transform.parent.parent.FindChild("GameSettingsLabel").transform.localPosition;
        posA.x = 3f;
        posA.y = 2.5f;
        posA.z = -5f;
        ApresetPickerButton.transform.localPosition = posA;
        var pkalabel = ApresetPickerButton.GetComponentInChildren<TextMeshPro>();
        pkalabel.DestroyTranslator();
        pkalabel.fontStyle = FontStyles.UpperCase;
        pkalabel.text = "<color=#ffffff>" + GetString("HardButton") + "</color>";
        ApresetPickerButton.inactiveSprites.GetComponent<SpriteRenderer>().color = new Color(255, 0, 0);
        ApresetPickerButton.activeSprites.GetComponent<SpriteRenderer>().color = new Color(100, 0, 0);
        ApresetPickerButton.selectedSprites.GetComponent<SpriteRenderer>().color = new Color(150, 0, 0);
        ApresetPickerButton.transform.localScale = ButtonSize;
        ApresetPickerButton.OnClick.AddListener((UnityEngine.Events.UnityAction)(() =>
                {
                    foreach (var option in Options.CustomRoleSpawnChances)
                    {
                        if (BeginnerRoles.Contains(option.Key) || IntermediateRoles.Contains(option.Key) || AdvancedRoles.Contains(option.Key)) option.Value.SetValue(100, false);
                        else option.Value.SetValue(0, false);
                    }
                    GameOptionsMenuPatch.ReOpenSettings();
                }
            ));
        
    }

    private static List<CustomRoles> BeginnerRoles =
    [
// IMPOSTORS
CustomRoles.Agent,
CustomRoles.Bane,
CustomRoles.Crewpostor,
CustomRoles.Kidnapper,
CustomRoles.Lunger,
CustomRoles.Lurker,
CustomRoles.Saboteur,
CustomRoles.Witch,
CustomRoles.Cleaner,
CustomRoles.Gangster,
CustomRoles.Kamikaze,
CustomRoles.TimeThief,
CustomRoles.Scavenger,
CustomRoles.Shapetricker,
CustomRoles.Vampire,

// CREWMATES
CustomRoles.Deputy,
CustomRoles.Sheriff,
CustomRoles.Grenadier,
CustomRoles.Medic,
CustomRoles.Firefighter,
CustomRoles.Oracle,
CustomRoles.Snitch,
CustomRoles.Villager,
CustomRoles.Benefactor,
CustomRoles.Chameleon,
CustomRoles.Exorcist,
CustomRoles.Retributionist,
CustomRoles.Sentinel,
CustomRoles.Veteran,
CustomRoles.Dictator,

// NEUTRALS
CustomRoles.Jester,
CustomRoles.Jackal,
CustomRoles.Cultist,
CustomRoles.Opportunist,
CustomRoles.Innocent,
CustomRoles.Executioner,
CustomRoles.Amnesiac,
CustomRoles.Lawyer,
CustomRoles.Revenant,
CustomRoles.Arsonist,
CustomRoles.Doomsayer,
CustomRoles.PunchingBag,
CustomRoles.God,
CustomRoles.Workaholic,
CustomRoles.Specter,

// MODIFIERS
CustomRoles.Bait,
CustomRoles.Autopsy,
CustomRoles.Fragile,
CustomRoles.Diseased,
CustomRoles.Gravestone,
CustomRoles.Oblivious,
CustomRoles.Unreportable,
CustomRoles.Susceptible,
CustomRoles.Trapper,
CustomRoles.Bewilder,
CustomRoles.Cyber,
CustomRoles.Evader,
CustomRoles.Reach,
CustomRoles.Overclocked,
CustomRoles.Underclocked
    ];
    
    private static List<CustomRoles> IntermediateRoles =
    [
// IMPOSTORS
CustomRoles.Bomber,
CustomRoles.BountyHunter,
CustomRoles.Councillor,
CustomRoles.Diviner,
CustomRoles.Fury,
CustomRoles.Rogue,
CustomRoles.WildShot,
CustomRoles.Zombie,
CustomRoles.Blackmailer,
CustomRoles.Consigliere,
CustomRoles.Detonator,
CustomRoles.Vindicator,
CustomRoles.Blinder,
CustomRoles.Butcher,
CustomRoles.Pathogen,

// CREWMATES
CustomRoles.Valkyrie,
CustomRoles.Admirer,
CustomRoles.Savior,
CustomRoles.Constable,
CustomRoles.Guardian,
CustomRoles.Protector,
CustomRoles.Bastion,
CustomRoles.Jailer,
CustomRoles.Webweaver,
CustomRoles.Crusader,
CustomRoles.ForensicScientist,
CustomRoles.Technician,
CustomRoles.Judge,
CustomRoles.Vigilante,
CustomRoles.Reverie,

// NEUTRALS
CustomRoles.Skeleton,
CustomRoles.Pixie,
CustomRoles.Specter,
CustomRoles.Gunslinger,
CustomRoles.Godzilla,
CustomRoles.DarkFairy,
CustomRoles.Narc,
CustomRoles.Seeker,
CustomRoles.Communist,
CustomRoles.Developer,
CustomRoles.Quizmaster,
CustomRoles.Terrorist,
CustomRoles.Vector,
CustomRoles.Vulture,
CustomRoles.Doppelganger,

// MODIFIERS
CustomRoles.FragileHunter,
CustomRoles.Burst,
CustomRoles.Egoist,
CustomRoles.Tiebreaker,
CustomRoles.Windy,
CustomRoles.Fool,
CustomRoles.Unlucky,
CustomRoles.Hurried,
CustomRoles.Influenced,
CustomRoles.VoidBallot,
CustomRoles.Rainbow,
CustomRoles.Randomizer,
CustomRoles.Glow,
CustomRoles.Youtuber,
CustomRoles.Oiiai
    ];
    
    private static List<CustomRoles> AdvancedRoles =
    [
// IMPOSTORS
CustomRoles.Propagandist,
CustomRoles.Anonymous,
CustomRoles.DollMaster,
CustomRoles.Chronomancer,
CustomRoles.Dragon,
CustomRoles.Hangman,
CustomRoles.Incinerator,
CustomRoles.Harbourer,
CustomRoles.Meteor,
CustomRoles.Nuancer,
CustomRoles.Gravedigger,
CustomRoles.Lifestealer,
CustomRoles.Lightning,
CustomRoles.Puppeteer,
CustomRoles.Staller,

// CREWMATES
CustomRoles.Raven,
CustomRoles.Archivist,
CustomRoles.Overseer,
CustomRoles.Supervisor,
CustomRoles.Mayor,
CustomRoles.Altruist,
CustomRoles.Keeper,
CustomRoles.Medium,
CustomRoles.Merchant,
CustomRoles.Survivalist,
CustomRoles.TimeMaster,
CustomRoles.Mage,
CustomRoles.Protester,
CustomRoles.Cursebearer,
CustomRoles.Hawk,

// NEUTRALS
CustomRoles.Abzorbaloff,
CustomRoles.ShadowKing,
CustomRoles.Wight,
CustomRoles.Slaad,
CustomRoles.Bandit,
CustomRoles.Contaminator,
CustomRoles.Falcon,
CustomRoles.Glitch,
CustomRoles.Hacker,
CustomRoles.Pelican,
CustomRoles.Shroud,
CustomRoles.Virus,
CustomRoles.Werewolf,
CustomRoles.Predator,
CustomRoles.Keymaster,

// MODIFIERS
CustomRoles.Avanger,
CustomRoles.Bloodthirst,
CustomRoles.Chronos,
CustomRoles.Gross,
CustomRoles.Radiator,
CustomRoles.Circumvent,
CustomRoles.Quota,
CustomRoles.Swift,
CustomRoles.Identifier,
CustomRoles.Necroview,
CustomRoles.Nimble,
CustomRoles.Sleuth,
CustomRoles.Watcher,
CustomRoles.Spurt,
CustomRoles.Flash
    ];
    
    private static void SetupAdittionalButtons(GameSettingMenu __instance)
    {
       /* CreateButton("", new Vector3(0f, 2f, 1f), new Color32(0, 0, 255, 200), Color32.Lerp(new Color32(0, 0, 255, 200), new Color32(0, 0, 0, 200), 0.5f), (UnityEngine.Events.UnityAction)(
            () =>
            {
                foreach (var option in Options.CustomRoleSpawnChances)
                {
                    option.Value.SetValue(0, false);
                }
            }), "Set Basic Preset");
        */
        
        if (__instance == null) return;
        
        var ParentLeftPanel = __instance.GamePresetsButton.transform.parent;

        var labeltag = GameObject.Find("ModeValue");
        var preset = Object.Instantiate(labeltag, ParentLeftPanel);
        preset.transform.localPosition = new Vector3(-3.33f, -0.45f, -2f);

        preset.transform.localScale = new Vector3(0.65f, 0.63f, 1f);
        var SpriteRenderer = preset.GetComponentInChildren<SpriteRenderer>();
        SpriteRenderer.color = Color.white;
        SpriteRenderer.sprite = Utils.LoadSprite("TOHO.Resources.Images.PresetBox.png", 55f);

        Color clr = new(-1, -1, -1);
        var PLabel = preset.GetComponentInChildren<TextMeshPro>();
        PLabel.DestroyTranslator();
        PLabel.text = GetString($"Preset_{OptionItem.CurrentPreset + 1}");
        float size = DestroyableSingleton<TranslationController>.Instance.currentLanguage.languageID switch
        {
            SupportedLangs.Russian => 1.45f,
            _ => 2.45f,
        };
        (PLabel.fontSizeMax, PLabel.fontSizeMin) = (size, size);

        var TempMinus = GameObject.Find("MinusButton").gameObject;
        var GMinus = Object.Instantiate(__instance.RoleSettingsButton.gameObject, preset.transform);
        GMinus.gameObject.SetActive(true);
        GMinus.transform.localScale = new Vector3(0.08f, 0.4f, 1f);


        var MLabel = GMinus.transform.Find("FontPlacer/Text_TMP").GetComponent<TextMeshPro>();
        MLabel.alignment = TextAlignmentOptions.Center;
        MLabel.DestroyTranslator();
        MLabel.text = "-";
        MLabel.transform.localPosition = new Vector3(MLabel.transform.localPosition.x, MLabel.transform.localPosition.y + 0.26f, MLabel.transform.localPosition.z);
        MLabel.color = new Color(255f, 255f, 255f);
        MLabel.SetFaceColor(new Color(255f, 255f, 255f));
        MLabel.transform.localScale = new Vector3(12f, 4f, 1f);


        var Minus = GMinus.GetComponent<PassiveButton>();
        Minus.OnClick.RemoveAllListeners();
        Minus.OnClick.AddListener(
                (UnityEngine.Events.UnityAction)(() =>
                {
                    if (PresetBehaviour == null) __instance.ChangeTab(3, false);
                    PresetBehaviour.Decrease();
                }));
        Minus.activeTextColor = new Color(255f, 255f, 255f);
        Minus.inactiveTextColor = new Color(255f, 255f, 255f);
        Minus.disabledTextColor = new Color(255f, 255f, 255f);
        Minus.selectedTextColor = new Color(255f, 255f, 255f);

        Minus.transform.localPosition = new Vector3(-2f, -3.37f, -4f);
        Minus.inactiveSprites.GetComponent<SpriteRenderer>().sprite = TempMinus.GetComponentInChildren<SpriteRenderer>().sprite;
        Minus.activeSprites.GetComponent<SpriteRenderer>().sprite = TempMinus.GetComponentInChildren<SpriteRenderer>().sprite;
        Minus.selectedSprites.GetComponent<SpriteRenderer>().sprite = TempMinus.GetComponentInChildren<SpriteRenderer>().sprite;

        Minus.inactiveSprites.GetComponent<SpriteRenderer>().color = new Color32(55, 59, 60, 255);
        Minus.activeSprites.GetComponent<SpriteRenderer>().color = new Color32(61, 62, 63, 255);
        Minus.selectedSprites.GetComponent<SpriteRenderer>().color = new Color32(55, 59, 60, 255);



        var PlusFab = Object.Instantiate(GMinus, preset.transform);
        var PLuLabel = PlusFab.transform.Find("FontPlacer/Text_TMP").GetComponent<TextMeshPro>();
        PLuLabel.alignment = TextAlignmentOptions.Center;
        PLuLabel.DestroyTranslator();
        PLuLabel.text = "+";
        PLuLabel.color = new Color(255f, 255f, 255f);
        PLuLabel.transform.localPosition = new Vector3(PLuLabel.transform.localPosition.x, PLuLabel.transform.localPosition.y + 0.26f, PLuLabel.transform.localPosition.z);
        PLuLabel.transform.localScale = new Vector3(12f, 4f, 1f);

        var plus = PlusFab.GetComponent<PassiveButton>();
        plus.OnClick.RemoveAllListeners();
        plus.OnClick.AddListener(
                (UnityEngine.Events.UnityAction)(() =>
                {
                    if (PresetBehaviour == null) __instance.ChangeTab(3, false);
                    PresetBehaviour.Increase();
                }));
        plus.activeTextColor = new Color(255f, 255f, 255f);
        plus.inactiveTextColor = new Color(255f, 255f, 255f);
        plus.disabledTextColor = new Color(255f, 255f, 255f);
        plus.selectedTextColor = new Color(255f, 255f, 255f);

        plus.transform.localPosition = new Vector3(-0.4f, -3.37f, -4f);

        var GameSettingsLabel = __instance.GameSettingsButton.transform.parent.parent.FindChild("GameSettingsLabel").GetComponent<TextMeshPro>();
        GameSettingsLabel.DestroyTranslator();
        GameSettingsLabel.text = GetString($"{Options.CurrentGameMode}");

        var FreeChatField = DestroyableSingleton<ChatController>.Instance.freeChatField;
        var TextField = Object.Instantiate(FreeChatField, ParentLeftPanel.parent);
        TextField.transform.localScale = new Vector3(0.3f, 0.59f, 1);
        TextField.transform.localPosition = new Vector3(-2.07f, -2.57f, -5f);
        TextField.textArea.outputText.transform.localScale = new Vector3(3.5f, 2f, 1f);
        TextField.textArea.outputText.font = PLuLabel.font;
        TextField.name = "InputField";

        InputField = TextField;


        var button = TextField.transform.FindChild("ChatSendButton");

        Object.Destroy(button.FindChild("Normal").FindChild("Icon").GetComponent<SpriteRenderer>());
        Object.Destroy(button.FindChild("Hover").FindChild("Icon").GetComponent<SpriteRenderer>());
        Object.Destroy(button.FindChild("Disabled").FindChild("Icon").GetComponent<SpriteRenderer>());
        Object.Destroy(button.transform.FindChild("Text").GetComponent<TextMeshPro>());

        button.FindChild("Normal").FindChild("Background").GetComponent<SpriteRenderer>().sprite = Utils.LoadSprite("TOHO.Resources.Images.SearchIconActive.png", 100f);
        button.FindChild("Hover").FindChild("Background").GetComponent<SpriteRenderer>().sprite = Utils.LoadSprite("TOHO.Resources.Images.SearchIconHover.png", 100f);
        button.FindChild("Disabled").FindChild("Background").GetComponent<SpriteRenderer>().sprite = Utils.LoadSprite("TOHO.Resources.Images.SearchIcon.png", 100f);

        if (DestroyableSingleton<TranslationController>.Instance.currentLanguage.languageID == SupportedLangs.Russian)
        {
            Vector3 FixedScale = new(0.7f, 1f, 1f);
            button.FindChild("Normal").FindChild("Background").transform.localScale = FixedScale;
            button.FindChild("Hover").FindChild("Background").transform.localScale = FixedScale;
            button.FindChild("Disabled").FindChild("Background").transform.localScale = FixedScale;
        }

        PassiveButton passiveButton = button.GetComponent<PassiveButton>();

        passiveButton.OnClick = new();
        passiveButton.OnClick.AddListener(
                (UnityEngine.Events.UnityAction)(() =>
                {
                    SearchForOptions(TextField);
                }));

        _SearchForOptions = (() =>
        {
            if (TextField.textArea.text == string.Empty)
                return;

            passiveButton.ReceiveClickDown();
        });

        static void SearchForOptions(FreeChatInputField textField)
        {
            if (ModGameOptionsMenu.TabIndex < 3) return;

            HiddenBySearch.Do(x => x.SetHidden(false));
            string text = textField.textArea.text.Trim().ToLower();
            var Result = OptionItem.AllOptions.Where(x => x.Parent == null && !x.IsHiddenOn(Options.CurrentGameMode)
            && !GetString($"{x.Name}").ToLower().Contains(text) && x.Tab == (TabGroup)(ModGameOptionsMenu.TabIndex - 3)).ToList();
            HiddenBySearch = Result;
            var SearchWinners = OptionItem.AllOptions.Where(x => x.Parent == null && !x.IsHiddenOn(Options.CurrentGameMode) && x.Tab == (TabGroup)(ModGameOptionsMenu.TabIndex - 3) && !Result.Contains(x)).ToList();
            if (!SearchWinners.Any() || !ModSettingsTabs.TryGetValue((TabGroup)(ModGameOptionsMenu.TabIndex - 3), out var settingsTab) || settingsTab == null)
            {
                HiddenBySearch.Clear();
                Logger.SendInGame(GetString("SearchNoResult")); // okay so showpopup nor this will overlay the menu, but I use this just for the sound lol
                return;
            }

            Result.Do(x => x.SetHidden(true));

            GameOptionsMenuPatch.ReCreateSettings(settingsTab);
            textField.Clear();
        }
    }

    [HarmonyPatch(nameof(GameSettingMenu.ChangeTab)), HarmonyPrefix]
    public static bool ChangeTabPrefix(GameSettingMenu __instance, ref int tabNum, [HarmonyArgument(1)] bool previewOnly)
    {
        if (HiddenBySearch.Any())
        {
            HiddenBySearch.Do(x => x.SetHidden(false));
            if (ModSettingsTabs.TryGetValue((TabGroup)(ModGameOptionsMenu.TabIndex - 3), out var GameSettingsTab) && GameSettingsTab != null)
                GameOptionsMenuPatch.ReCreateSettings(GameSettingsTab);

            HiddenBySearch.Clear();
        }

        if (!previewOnly || tabNum != 1) ModGameOptionsMenu.TabIndex = tabNum;

        GameOptionsMenu settingsTab;
        PassiveButton button;

        if ((previewOnly && Controller.currentTouchType == Controller.TouchType.Joystick) || !previewOnly)
        {
            foreach (var tab in EnumHelper.GetAllValues<TabGroup>())
            {
                if (ModSettingsTabs.TryGetValue(tab, out settingsTab) &&
                    settingsTab != null)
                {
                    settingsTab.gameObject.SetActive(false);
                }
            }
            foreach (var tab in EnumHelper.GetAllValues<TabGroup>())
            {
                if (ModSettingsButtons.TryGetValue(tab, out button) &&
                    button != null)
                {
                    button.SelectButton(false);
                }
            }
        }

        if (tabNum == 1)
        {
            if (__instance.PresetsTab.transform.Find("CustomStandardButton") != null)
                return true;
            
            var original = __instance.PresetsTab.StandardPresetButton.gameObject;

            //Standard
            var standard = Object.Instantiate(original, original.transform.parent);
            standard.transform.localScale = new Vector3(original.transform.localScale.x / 2, original.transform.localScale.y / 2, original.transform.localScale.z);
            standard.name = "CustomStandardButton";
            standard.SetActive(true);
            var standardButton = standard.GetComponent<PassiveButton>();
            var standardText = standard.FindChild<Transform>("ModeText");
            var standardTMP = standardText.GetComponent<TextMeshPro>();
            standardTMP.text = "Standard";
            if (standardButton != null)
            {
                standardButton.OnClick.AddListener((UnityEngine.Events.UnityAction)(() =>
                {
                    Options.GameMode.SetValue(0);
                    GameOptionsMenuPatch.ReOpenSettings();
                }));
            }
            var backgroundsr = standard.AddComponent<SpriteRenderer>();
            backgroundsr.sprite = Utils.LoadSprite("TOHO.Resources.Images.Standard.png", 150f);
            backgroundsr.size = new Vector2(4.48f, 5.23f);
            standard.transform.localPosition = new Vector3(-2.4f, 1f, 0f);
            
            //FFA
            var ffa = Object.Instantiate(original, original.transform.parent);
            ffa.transform.localScale = new Vector3(original.transform.localScale.x / 2, original.transform.localScale.y / 2, original.transform.localScale.z);
            ffa.SetActive(true);
            var ffaButton = ffa.GetComponent<PassiveButton>();
            var ffaText = ffa.FindChild<Transform>("ModeText");
            var ffaTMP = ffaText.GetComponent<TextMeshPro>();
            ffaTMP.text = "Free For All";
            if (ffaButton != null)
            {
                ffaButton.OnClick.AddListener((UnityEngine.Events.UnityAction)(() =>
                {
                    Options.GameMode.SetValue(1);
                    GameOptionsMenuPatch.ReOpenSettings();
                }));
            }
            var backgroundffa = ffa.AddComponent<SpriteRenderer>();
            backgroundffa.sprite = Utils.LoadSprite("TOHO.Resources.Images.FreeForAll.png", 150f);
            backgroundffa.size = new Vector2(4.48f, 5.23f);
            ffa.transform.localPosition = new Vector3(-0.8f, 1f, 0f);

            //CandR
            var candr = Object.Instantiate(original, original.transform.parent);
            candr.transform.localScale = new Vector3(original.transform.localScale.x / 2, original.transform.localScale.y / 2, original.transform.localScale.z);
            candr.SetActive(true);
            var candrButton = candr.GetComponent<PassiveButton>();
            var candrText = candr.FindChild<Transform>("ModeText");
            var candrTMP = candrText.GetComponent<TextMeshPro>();
            candrTMP.text = "Cops and Robbers";
            if (candrButton != null)
            {
                candrButton.OnClick.AddListener((UnityEngine.Events.UnityAction)(() =>
                {
                    Options.GameMode.SetValue(2);
                    GameOptionsMenuPatch.ReOpenSettings();
                }));
            }
            var backgroundcandr = candr.AddComponent<SpriteRenderer>();
            backgroundcandr.sprite = Utils.LoadSprite("TOHO.Resources.Images.CopsAndRobbers.png", 150f);
            backgroundcandr.size = new Vector2(4.48f, 5.23f);
            candr.transform.localPosition = new Vector3(0.8f, 1f, 0f);

            //Ultimate Team
            var ultt = Object.Instantiate(original, original.transform.parent);
            ultt.transform.localScale = new Vector3(original.transform.localScale.x / 2, original.transform.localScale.y / 2, original.transform.localScale.z);
            ultt.SetActive(true);
            var ulttButton = ultt.GetComponent<PassiveButton>();
            var ulttText = ultt.FindChild<Transform>("ModeText");
            var ulttTMP = ulttText.GetComponent<TextMeshPro>();
            ulttTMP.text = "Ultimate Team";
            if (ulttButton != null)
            {
                ulttButton.OnClick.AddListener((UnityEngine.Events.UnityAction)(() =>
                {
                    Options.GameMode.SetValue(3);
                    GameOptionsMenuPatch.ReOpenSettings();
                }));
            }
            var backgroundultt = ultt.AddComponent<SpriteRenderer>();
            backgroundultt.sprite = Utils.LoadSprite("TOHO.Resources.Images.UltimateTeam.png", 150f);
            backgroundultt.size = new Vector2(4.48f, 5.23f);
            ultt.transform.localPosition = new Vector3(2.4f, 1f, 0f);

            //Four Corners
            var frc = Object.Instantiate(original, original.transform.parent);
            frc.transform.localScale = new Vector3(original.transform.localScale.x / 2, original.transform.localScale.y / 2, original.transform.localScale.z);
            frc.SetActive(true);
            var frcButton = frc.GetComponent<PassiveButton>();
            var frcText = frc.FindChild<Transform>("ModeText");
            var frcTMP = frcText.GetComponent<TextMeshPro>();
            frcTMP.text = "Four Corners";
            if (frcButton != null)
            {
                frcButton.OnClick.AddListener((UnityEngine.Events.UnityAction)(() =>
                {
                    Options.GameMode.SetValue(4);
                    GameOptionsMenuPatch.ReOpenSettings();
                }));
            }
            var backgroundfrc = frc.AddComponent<SpriteRenderer>();
            backgroundfrc.sprite = Utils.LoadSprite("TOHO.Resources.Images.FourCorners2.png", 150f);
            backgroundfrc.size = new Vector2(4.48f, 5.23f);
            frc.transform.localPosition = new Vector3(-2.4f, -1f, 0f);
            
            //King Of The Hill
            var cs1 = Object.Instantiate(original, original.transform.parent);
            cs1.transform.localScale = new Vector3(original.transform.localScale.x / 2, original.transform.localScale.y / 2, original.transform.localScale.z);
            cs1.SetActive(true);
            var cs1Button = cs1.GetComponent<PassiveButton>();
            var cs1Text = cs1.FindChild<Transform>("ModeText");
            var cs1TMP = cs1Text.GetComponent<TextMeshPro>();
            cs1TMP.text = "King of the Hill";
            if (cs1Button != null)
            {
                cs1Button.OnClick.AddListener((UnityEngine.Events.UnityAction)(() =>
                {
                    Options.GameMode.SetValue(5);
                    GameOptionsMenuPatch.ReOpenSettings();
                }));
            }
            var backgroundcs1 = cs1.AddComponent<SpriteRenderer>();
            backgroundcs1.sprite = Utils.LoadSprite("TOHO.Resources.Images.KingOfTheHill.png", 150f);
            backgroundcs1.size = new Vector2(4.48f, 5.23f);
            cs1.transform.localPosition = new Vector3(-0.8f, -1f, 0f);
            
            //Coming Soon 2
            var cs2 = Object.Instantiate(original, original.transform.parent);
            cs2.transform.localScale = new Vector3(original.transform.localScale.x / 2, original.transform.localScale.y / 2, original.transform.localScale.z);
            cs2.SetActive(true);
            var cs2Button = cs2.GetComponent<PassiveButton>();
            var cs2Text = cs2.FindChild<Transform>("ModeText");
            var cs2TMP = cs2Text.GetComponent<TextMeshPro>();
            cs2TMP.text = "Coming Soon";
            if (cs2Button != null)
            {
                cs2Button.OnClick.AddListener((UnityEngine.Events.UnityAction)(() =>
                {
                    Options.GameMode.SetValue(0);
                    GameOptionsMenuPatch.ReOpenSettings();
                }));
            }
            var backgroundcs2 = cs2.AddComponent<SpriteRenderer>();
            backgroundcs2.sprite = Utils.LoadSprite("TOHO.Resources.Images.ComingSoon.png", 150f);
            backgroundcs2.size = new Vector2(4.48f, 5.23f);
            cs2.transform.localPosition = new Vector3(0.8f, -1f, 0f);
            
            //Coming Soon 3
            var cs3 = Object.Instantiate(original, original.transform.parent);
            cs3.transform.localScale = new Vector3(original.transform.localScale.x / 2, original.transform.localScale.y / 2, original.transform.localScale.z);
            cs3.SetActive(true);
            var cs3Button = cs3.GetComponent<PassiveButton>();
            var cs3Text = cs3.FindChild<Transform>("ModeText");
            var cs3TMP = cs3Text.GetComponent<TextMeshPro>();
            cs3TMP.text = "Coming Soon";
            if (cs3Button != null)
            {
                cs3Button.OnClick.AddListener((UnityEngine.Events.UnityAction)(() =>
                {
                    Options.GameMode.SetValue(0);
                    GameOptionsMenuPatch.ReOpenSettings();
                }));
            }
            var backgroundcs3 = cs3.AddComponent<SpriteRenderer>();
            backgroundcs3.sprite = Utils.LoadSprite("TOHO.Resources.Images.ComingSoon.png", 150f);
            backgroundcs3.size = new Vector2(4.48f, 5.23f);
            cs3.transform.localPosition = new Vector3(2.4f, -1f, 0f);

            //Disabling Others
            __instance.PresetsTab.StandardPresetButton.gameObject.SetActive(false);
            __instance.PresetsTab.SecondPresetButton.gameObject.SetActive(false);
            __instance.PresetsTab.PresetDescriptionText.gameObject.SetActive(false);
            return true;
        }
        
        if (tabNum < 3) return true;

        var tabGroupId = (TabGroup)(tabNum - 3);
        if ((previewOnly && Controller.currentTouchType == Controller.TouchType.Joystick) || !previewOnly)
        {
            __instance.PresetsTab.gameObject.SetActive(false);
            __instance.GameSettingsTab.gameObject.SetActive(false);
            __instance.RoleSettingsTab.gameObject.SetActive(false);
            __instance.GamePresetsButton.SelectButton(false);
            __instance.GameSettingsButton.SelectButton(false);
            __instance.RoleSettingsButton.SelectButton(false);

            if (ModSettingsTabs.TryGetValue(tabGroupId, out settingsTab) && settingsTab != null)
            {
                settingsTab.gameObject.SetActive(true);
                __instance.MenuDescriptionText.DestroyTranslator();
                switch (tabGroupId)
                {
                    case TabGroup.SystemSettings:
                    case TabGroup.ModSettings:
                        __instance.MenuDescriptionText.text = GetString("TabMenuDescription_General");
                        break;
                    case TabGroup.ImpostorRoles:
                    case TabGroup.CrewmateRoles:
                    case TabGroup.NeutralRoles:
                    case TabGroup.CovenRoles:
                    case TabGroup.Modifiers:
                        __instance.MenuDescriptionText.text = GetString("TabMenuDescription_Roles&Modifiers");
                        break;
                }
            }
        }

        if (previewOnly)
        {
            __instance.ToggleLeftSideDarkener(false);
            __instance.ToggleRightSideDarkener(true);
            return false;
        }
        __instance.ToggleLeftSideDarkener(true);
        __instance.ToggleRightSideDarkener(false);

        if (ModSettingsButtons.TryGetValue(tabGroupId, out button) &&
            button != null)
        {
            button.SelectButton(true);
        }

        return false;
    }

    [HarmonyPatch(nameof(GameSettingMenu.OnEnable)), HarmonyPrefix]
    private static bool OnEnablePrefix(GameSettingMenu __instance)
    {

        if (TemplateGameOptionsMenu == null)
        {
            TemplateGameOptionsMenu = Object.Instantiate(__instance.GameSettingsTab, __instance.GameSettingsTab.transform.parent);
            TemplateGameOptionsMenu.gameObject.SetActive(false);
        }
        if (TemplateGameSettingsButton == null)
        {
            TemplateGameSettingsButton = Object.Instantiate(__instance.GameSettingsButton, __instance.GameSettingsButton.transform.parent);
            TemplateGameSettingsButton.gameObject.SetActive(false);
        }
        ModGameOptionsMenu.OptionList = new();
        ModGameOptionsMenu.BehaviourList = new();
        ModGameOptionsMenu.CategoryHeaderList = new();

        SetDefaultButton(__instance);

        ControllerManager.Instance.OpenOverlayMenu(__instance.name, __instance.BackButton, __instance.DefaultButtonSelected, __instance.ControllerSelectable, false);
        DestroyableSingleton<HudManager>.Instance.menuNavigationPrompts.SetActive(false);
        if (Controller.currentTouchType != Controller.TouchType.Joystick)
        {
            __instance.ChangeTab(1, Controller.currentTouchType == Controller.TouchType.Joystick);
        }
        __instance.StartCoroutine(__instance.CoSelectDefault());

        return false;
    }
    [HarmonyPatch(nameof(GameSettingMenu.Close)), HarmonyPostfix]
    private static void ClosePostfix(GameSettingMenu __instance)
    {
        foreach (var button in ModSettingsButtons.Values)
            Object.Destroy(button);
        foreach (var tab in ModSettingsTabs.Values)
            Object.Destroy(tab);
        ModSettingsButtons = [];
        ModSettingsTabs = [];
    }
}
[HarmonyPatch(typeof(FreeChatInputField), nameof(FreeChatInputField.UpdateCharCount))]
public static class FixInputChatField
{
    public static bool Prefix(FreeChatInputField __instance)
    {
        if (GameSettingMenuPatch.InputField != null && __instance == GameSettingMenuPatch.InputField)
        {
            Vector2 size = __instance.Background.size;
            size.y = Math.Max(0.62f, __instance.textArea.TextHeight + 0.2f);
            __instance.Background.size = size;
            return false;
        }
        return true;
    }
}
[HarmonyPatch(typeof(ChatController), nameof(ChatController.Update))]
public static class FixDarkThemeForSearchBar
{
    public static void Postfix()
    {
        if (!GameSettingMenu.Instance || (ThemeOptionItem.ThemeID == 1)) return;
        var field = GameSettingMenuPatch.InputField;
        if (field != null)
        {
            field.background.color = new Color32(40, 40, 40, byte.MaxValue);
            field.textArea.compoText.Color(Color.white);
            field.textArea.outputText.color = Color.white;
        }
    }
}
[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcSyncSettings))]
public class RpcSyncSettingsPatch
{
    public static void Postfix()
    {
        OptionItem.SyncAllOptions();
    }
}
