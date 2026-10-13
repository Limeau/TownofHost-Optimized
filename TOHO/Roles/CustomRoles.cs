using System.Collections.Generic;
using AmongUs.GameOptions;
using UnityEngine.Events;

namespace TOHO;

public class CustomRoles
{
    public bool IsOptionIninialized;
    public string RoleID;
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

    public CustomRoles(string roleID, string roleName, string shortDescription, string longDescription, string roleColor, RoleTypes basis, RoleCategories category)
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

    /* Settings */

    public void SetupCustomOption()
    {
        Option = new RoleOptionItem(this, RoleID, $"<color={RoleColor}>{RoleName}</color>", Category);
        Utils.GetRoleOptionItemFromCategory(Category).Add(Option);
        SubNumberOptions.Add(new NumberOptionItem(RoleID + ".Max", "Max", OptionTabs.RoleSetting, 1f, 1f, 10f, 1f));
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
    public void StartAdvancedOptions(float y)
    {
        foreach (var option in SubBooleanOptions)
        {
            option.obj.SetActive(true);
            y -= 0.6f;
        }
        foreach (var option in SubNumberOptions)
        {
            option.obj.SetActive(true);
            y -= 0.6f;
        }
        foreach (var option in SubStringOptions)
        {
            option.obj.SetActive(true);
            y -= 0.6f;
        }
        
        GameSettingMenu.Instance.RoleSettingsTab.scrollBar.SetYBoundsMin(-2f + 2f);
        GameSettingMenu.Instance.RoleSettingsTab.scrollBar.SetYBoundsMax(-y - 1f);
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

    /* Triggers */

    // Murder
    public UnityAction<PlayerControl, PlayerControl> MurderAsKillerAction = (UnityAction<PlayerControl, PlayerControl>)((PlayerControl a, PlayerControl b) => { });
    public UnityAction<PlayerControl, PlayerControl> MurderAsTargetAction = (UnityAction<PlayerControl, PlayerControl>)((PlayerControl a, PlayerControl b) => { });
    public UnityAction<PlayerControl, PlayerControl> MurderAsSpectAction = (UnityAction<PlayerControl, PlayerControl>)((PlayerControl a, PlayerControl b) => { });
    public bool MurderAsKillerBool = true;
    public bool MurderAsTargetBool = true;
    public bool MurderAsSpectBool = true;
    public bool OnMurderAsKiller(PlayerControl killer, PlayerControl target)
    {
        MurderAsKillerAction.Invoke(killer, target);
        return MurderAsKillerBool;
    }
    public bool OnMurderAsTarget(PlayerControl killer, PlayerControl target)
    {
        MurderAsTargetAction.Invoke(killer, target);
        return MurderAsTargetBool;
    }
    public bool OnMurderAsSpect(PlayerControl killer, PlayerControl target)
    {
        MurderAsSpectAction.Invoke(killer, target);
        return MurderAsSpectBool;
    }

    public void AddOnMurder(UnityAction<PlayerControl, PlayerControl> onMurder, bool isKiller = false, bool isTarget = false, bool allow = true)
    {
        if (isKiller)
        {
            MurderAsKillerAction = onMurder;
            MurderAsKillerBool = allow;
        }
        else if (isTarget)
        {
            MurderAsTargetAction = onMurder;
            MurderAsTargetBool = allow;
        }
        else
        {
            MurderAsSpectAction = onMurder;
            MurderAsSpectBool = allow;
        }
    }

    // Enter, Exit Vent
    public UnityAction<PlayerControl> EnterVentAsTargetAction = (UnityAction<PlayerControl>)((PlayerControl a) => { });
    public UnityAction<PlayerControl> EnterVentAsSpectAction = (UnityAction<PlayerControl>)((PlayerControl a) => { });
    public bool EnterVentAsTargetBool = true;
    public bool EnterVentAsSpectBool = true;
    public bool OnEnterVentAsTarget(PlayerControl target)
    {
        EnterVentAsTargetAction.Invoke(target);
        return EnterVentAsTargetBool;
    }
    public bool OnEnterVentAsSpect(PlayerControl target)
    {
        EnterVentAsSpectAction.Invoke(target);
        return EnterVentAsSpectBool;
    }

    public void AddOnEnterVent(UnityAction<PlayerControl> onEnterVent, bool isTarget = false, bool allow = true)
    {
        if (isTarget)
        {
            EnterVentAsTargetAction = onEnterVent;
            EnterVentAsTargetBool = allow;
        }
        else
        {
            EnterVentAsSpectAction = onEnterVent;
            EnterVentAsSpectBool = allow;
        }
    }
   
    public UnityAction<PlayerControl> ExitVentAsTargetAction = (UnityAction<PlayerControl>)((PlayerControl a) => { });
    public UnityAction<PlayerControl> ExitVentAsSpectAction = (UnityAction<PlayerControl>)((PlayerControl a) => { });
    public bool ExitVentAsTargetBool = true;
    public bool ExitVentAsSpectBool = true;
    public bool OnExitVentAsTarget(PlayerControl target)
    {
        ExitVentAsTargetAction.Invoke(target);
        return ExitVentAsTargetBool;
    }
    public bool OnExitVentAsSpect(PlayerControl target)
    {
        ExitVentAsSpectAction.Invoke(target);
        return ExitVentAsSpectBool;
    }

    public void AddOnExitVent(UnityAction<PlayerControl> onExitVent, bool isTarget = false, bool allow = true)
    {
        if (isTarget)
        {
            ExitVentAsTargetAction = onExitVent;
            ExitVentAsTargetBool = allow;
        }
        else
        {
            ExitVentAsSpectAction = onExitVent;
            ExitVentAsSpectBool = allow;
        }
    }


    // Ejection
    public UnityAction<PlayerControl> EjectAsTargetAction = (UnityAction<PlayerControl>)((PlayerControl a) => { });
    public UnityAction<PlayerControl> EjectAsSpectAction = (UnityAction<PlayerControl>)((PlayerControl a) => { });
    public bool EjectAsTargetBool = true;
    public bool EjectAsSpectBool = true;
    public bool OnEjectAsTarget(PlayerControl target)
    {
        EjectAsTargetAction.Invoke(target);
        return EjectAsTargetBool;
    }
    public bool OnEjectAsSpect(PlayerControl target)
    {
        EjectAsSpectAction.Invoke(target);
        return EjectAsSpectBool;
    }

    public void AddOnEject(UnityAction<PlayerControl> onEject, bool isTarget = false, bool allow = true)
    {
        if (isTarget)
        {
            EjectAsTargetAction = onEject;
            EjectAsTargetBool = allow;
        }
        else
        {
            EjectAsSpectAction = onEject;
            EjectAsSpectBool = allow;
        }
    }
}

public static class CustomRoleExtentions
{
    public static void MurderPlayer(PlayerControl killer, PlayerControl target)
    {
        killer.RpcMurderPlayer(target, true);
    }

    public static void WinTheGame(List<PlayerControl> winners, Custom_Team winnerTeam)
    {
        GameEndCheckerForNormal.IsGameEnd = true;
        GameEndCheckerForNormal.Winners = winners;
        GameEndCheckerForNormal.WinnerTeam = winnerTeam;
    }
}