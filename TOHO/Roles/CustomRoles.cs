using System.Collections.Generic;
using AmongUs.GameOptions;
using UnityEngine;
using static TOHO.OptionManager;

namespace TOHO;

public class CustomRoles
{
    public bool IsOptionIninialized;
    public int RoleID;
    public string RoleName;
    public string ShortDescription;
    public string ScreenshotPath;
    public string LongDescription;
    public string RoleColor;
    public RoleTypes Basis;
    public RoleCategories Category;
    
    public List<BooleanOptionItem> SubBooleanOptions = [];
    public List<NumberOptionItem> SubNumberOptions = [];
    public List<StringOptionItem> SubStringOptions = [];
    
    public RoleOptionItem Option;
    public bool Enabled;

    public CustomRoles(int roleID, string roleName, string shortDescription, string longDescription, string roleColor, RoleTypes basis, RoleCategories category)
    {
        RoleID = roleID;
        RoleName = roleName;
        ShortDescription = shortDescription;
        LongDescription = longDescription;
        RoleColor = roleColor;
        Basis = basis;
        Category = category;
        IsOptionIninialized = false;
        
        Main.AllRoles.Add(roleName, this);
    }

    public void SetupCustomOption()
    {
        Option = new RoleOptionItem(this, RoleID, $"<color={RoleColor}>{RoleName}</color>", Category);
        Utils.GetRoleOptionItemFromCategory(Category).Add(Option);
        SubNumberOptions.Add(new NumberOptionItem(RoleID + 1, "Max", OptionTabs.RoleSetting, 1f, 1f, 10f, 1f));
    }

    public void StopAdvancedOptions()
    {
        foreach (var option in SubBooleanOptions)
        {
            option.obj.SetActive(false);
        }
        foreach (var option in SubNumberOptions)
        {
            option.obj.SetActive(false);
        }
        foreach (var option in SubStringOptions)
        {
            option.obj.SetActive(false);
        }
    }
    public void StartAdvancedOptions()
    {
        foreach (var option in SubBooleanOptions)
        {
            option.obj.SetActive(true);
        }
        foreach (var option in SubNumberOptions)
        {
            option.obj.SetActive(true);
        }
        foreach (var option in SubStringOptions)
        {
            option.obj.SetActive(true);
        }
    }
    
    public void SetupAdvancedOptions(float y)
    {
        foreach (var option in SubBooleanOptions)
        {
            option.SetupBooleanOption(y);
            y -= 0.6f;
        }
        foreach (var option in SubNumberOptions)
        {
            option.SetupNumberOption(y);
            y -= 0.6f;
        }
        foreach (var option in SubStringOptions)
        {
            option.SetupStringOption(y);
            y -= 0.6f;
        }
        
        GameSettingMenu.Instance.RoleSettingsTab.scrollBar.SetYBoundsMin(-2f + 2f);
        GameSettingMenu.Instance.RoleSettingsTab.scrollBar.SetYBoundsMax(-y - 1f);
    }
    
    public void SetupAdvancedBooleanOption(BooleanOptionItem option)
    {
        SubBooleanOptions.Add(option);
    }
    public void SetupAdvancedNumberOption(NumberOptionItem option)
    {
        SubNumberOptions.Add(option);
    }
    public void SetupAdvancedStringOption(StringOptionItem option)
    {
        SubStringOptions.Add(option);
    }

    public void SetupScreenshot(string path)
    {
        ScreenshotPath = path;
    }
}