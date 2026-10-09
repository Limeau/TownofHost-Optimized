using System.Collections.Generic;
using AmongUs.GameOptions;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using UnityEngine;
using TMPro;
using TOHO;
using Object = UnityEngine.Object;

namespace TOHO;

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

        if (option.OptionTab != OptionTabs.RoleSetting) option.obj = Object.Instantiate(template.gameObject, GameSettingMenuPatch.ModSettingsTab.settingsContainer);
        else option.obj = Object.Instantiate(template.gameObject, GameSettingMenu.Instance.RoleSettingsTab.AdvancedRolesSettings.transform);

        var toggle = option.obj.GetComponent<ToggleOption>();
        toggle.boolOptionName = BoolOptionNames.Invalid;

        option.obj.transform.localPosition = option.OptionTab != OptionTabs.RoleSetting ? new Vector3(-0.4f, y, -2f) : new Vector3(2f, y, -2f);
        option.obj.SetActive(true);

        var text = toggle.TitleText;
        
        new LateTask(() =>
        {
            text.DestroyTranslator();
            text.text = option.OptionName;

            toggle.CheckMark.enabled = option.Value;
            
            if (option.OptionTab != OptionTabs.RoleSetting) GameSettingMenuPatch.ModSettingObjects.Add(option.obj);
        }, 0.01f);
    }
    public static void SetupRoleOption(this RoleOptionItem option, List<GameObject> team, float y)
    {
        ToggleOption template = null;
        option.SetInitialRoleValue();
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

        option.obj = Object.Instantiate(template.gameObject, GameSettingMenuPatch.RoleSettingsTab.settingsContainer);

        var toggle = option.obj.GetComponent<ToggleOption>();
        toggle.boolOptionName = BoolOptionNames.Invalid;
        _ = ColorUtility.TryParseHtmlString("#000000", out var rclr2);
        ColorUtility.TryParseHtmlString(option.Role.RoleColor, out var rc);
        toggle.LabelBackground.color = Color.Lerp(rc, rclr2, 0.5f);
        option.obj.transform.localPosition = new Vector3(-0.4f, y, -2f);
        option.obj.SetActive(true);

        var text = toggle.TitleText;
        
        new LateTask(() =>
        {
            text.DestroyTranslator();
            text.text = option.OptionName;

            toggle.CheckMark.enabled = option.Value;
            
            team.Add(option.obj);
            
            SetupHelpIcon(option);
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

        if (option.OptionTab != OptionTabs.RoleSetting) option.obj = Object.Instantiate(template.gameObject, GameSettingMenuPatch.ModSettingsTab.settingsContainer);
        else option.obj = Object.Instantiate(template.gameObject, GameSettingMenu.Instance.RoleSettingsTab.AdvancedRolesSettings.transform);

        var number = option.obj.GetComponent<NumberOption>();
        number.floatOptionName = FloatOptionNames.Invalid;

        number.Increment = option.Step;
        number.ValidRange = new FloatRange(option.MinValue, option.MaxValue);
        
        option.obj.transform.localPosition = option.OptionTab != OptionTabs.RoleSetting ? new Vector3(-0.4f, y, -2f) : new Vector3(2f, y, -2f);
        option.obj.SetActive(true);

        var text = number.TitleText;

        new LateTask(() =>
        {
            text.DestroyTranslator();
            text.text = option.OptionName;

            number.Value = option.Value;
            number.ValueText.text = $"{number.Value}";
            if (option.OptionTab != OptionTabs.RoleSetting) GameSettingMenuPatch.ModSettingObjects.Add(option.obj);
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

        if (option.OptionTab != OptionTabs.RoleSetting) option.obj = Object.Instantiate(template.gameObject, GameSettingMenuPatch.ModSettingsTab.settingsContainer);
        else option.obj = Object.Instantiate(template.gameObject, GameSettingMenu.Instance.RoleSettingsTab.AdvancedRolesSettings.transform);

        var stringo = option.obj.GetComponent<StringOption>();
        StringOptionItem.OptionMap[stringo] = option;
        
        stringo.stringOptionName = Int32OptionNames.Invalid;

        option.obj.transform.localPosition = option.OptionTab != OptionTabs.RoleSetting ? new Vector3(-0.4f, y, -2f) : new Vector3(2f, y, -2f);
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
            
            if (option.OptionTab != OptionTabs.RoleSetting) GameSettingMenuPatch.ModSettingObjects.Add(option.obj);
        }, 0.01f);
    }

    public static void SetupHeader(float y, string title, List<GameObject> list, GameOptionsMenu tab, Color color)
    {
        CategoryHeaderMasked header = Object.Instantiate(tab.categoryHeaderOrigin, Vector3.zero, Quaternion.identity, tab.settingsContainer);
        header.SetHeader(StringNames.RolesCategory, 20);
        header.Title.text = title;
        header.Background.color = header.Divider.color = color;
        header.transform.localScale = Vector3.one * 0.68f;
        header.transform.localPosition = new(-0.9f, y, 2f);
        var chmText = header.transform.FindChild("HeaderText").GetComponent<TextMeshPro>();
        chmText.fontStyle = FontStyles.Bold;
        chmText.outlineWidth = 0.17f;
        header.gameObject.SetActive(false);
        list.Add(header.gameObject);
    }
    private static void SetupHelpIcon(RoleOptionItem option)
    {
        NumberOption template = null;
        
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
        
        template.gameObject.SetActive(true);
        
        var icontemplate = template.transform.FindChild("MinusButton");
        
        // Help Button
        
        var help = Object.Instantiate(icontemplate, option.obj.transform, true);
        help.gameObject.SetActive(true);
        help.name = $"{option.Role.RoleName}HelpIcon";
        var htext = help.GetComponentInChildren<TextMeshPro>();
        htext.text = "?";
        _ = ColorUtility.TryParseHtmlString("#000000", out var hclr2);
        ColorUtility.TryParseHtmlString(option.Role.RoleColor, out var hc);
        var hGameOptionsButton = help.GetComponent<GameOptionButton>();
        hGameOptionsButton.OnClick = new();
        hGameOptionsButton.OnClick.AddListener((UnityEngine.Events.UnityAction)(() =>
        {
            var str = $"{option.Role.LongDescription}";
            int size = str.Length > 500 ? str.Length > 550 ? 65 : 70 : 100;
            var ColorRole = $"<color={option.Role.RoleColor}>{option.Role.RoleName}</color>";
            var info = $"<size={size}%>{ColorRole}: {str}</size>"; 
            GameSettingMenu.Instance.MenuDescriptionText.text = info;
        }));
        
        
        help.FindChild("ButtonSprite").GetComponent<SpriteRenderer>().color = hGameOptionsButton.interactableClickColor;
        help.FindChild("ButtonSprite").GetComponent<SpriteRenderer>().color = Color.Lerp(hc, hclr2, 0.75f);
        hGameOptionsButton.interactableColor = Color.Lerp(hc, hclr2, 0.75f);
        hGameOptionsButton.interactableHoveredColor = Color.Lerp(hGameOptionsButton.interactableColor, hclr2, 0.5f);
        hGameOptionsButton.interactableClickColor = hclr2;
        help.localPosition = new Vector3(1.55f, 0f, 0f);
        htext.color = hc;
        help.SetAsLastSibling();
        
        // Settings Button
        
        var settings = Object.Instantiate(icontemplate, option.obj.transform, true);
        settings.gameObject.SetActive(true);
        settings.name = $"{option.Role.RoleName}HelpIcon";
        var text = settings.GetComponentInChildren<TextMeshPro>();
        text.text = "!";
        _ = ColorUtility.TryParseHtmlString("#000000", out var clr2);
        ColorUtility.TryParseHtmlString(option.Role.RoleColor, out var c);
        var GameOptionsButton = settings.GetComponent<GameOptionButton>();
        GameOptionsButton.OnClick = new();
        GameOptionsButton.OnClick.AddListener((UnityEngine.Events.UnityAction)(() =>
        { 
            GameSettingMenuPatch.SwitchToAdvancedRoleSettings(option.Role);
        }));

        settings.FindChild("ButtonSprite").GetComponent<SpriteRenderer>().color = Color.Lerp(c, clr2, 0.75f);
        GameOptionsButton.interactableColor = Color.Lerp(c, clr2, 0.75f);
        GameOptionsButton.interactableHoveredColor = Color.Lerp(GameOptionsButton.interactableColor, clr2, 0.5f);
        GameOptionsButton.interactableClickColor = clr2;
        settings.localPosition = new Vector3(2.1f, 0f, 0f);
        settings.SetAsLastSibling();        
        text.color = c;
        
        template.gameObject.SetActive(false);
    }
}