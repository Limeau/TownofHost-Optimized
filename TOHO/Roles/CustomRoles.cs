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

    /*****----- Triggers -----*****/

    /* Non-Cancellable */

    // After Meeting
    public UnityAction<PlayerControl> AfterMeetingAction = (UnityAction<PlayerControl>)((PlayerControl a) => { });

    public void OnAfterMeeting(PlayerControl target)
    {
        AfterMeetingAction.Invoke(target);
    }
    public void AddOnAfterMeeting(UnityAction<PlayerControl> onAfterMeeting)
    {
        AfterMeetingAction = onAfterMeeting;
    }
    
    // Fixed Update

    public UnityAction<PlayerControl> FixedUpdateAction = (UnityAction<PlayerControl>)((PlayerControl a) => { });

    public void OnFixedUpdate(PlayerControl target)
    {
        FixedUpdateAction.Invoke(target);
    }
    public void AddOnFixedUpdate(UnityAction<PlayerControl> onFixedUpdate)
    {
        FixedUpdateAction = onFixedUpdate;
    }

    /* Cancellable */

    // Vote
    public UnityAction<PlayerControl> VoteAction = (UnityAction<PlayerControl>)((PlayerControl a) => { });
    public UnityAction<PlayerControl> VoteAsSpectAction = (UnityAction<PlayerControl>)((PlayerControl a) => { });

    public bool VoteBool = true;
    public bool VoteAsSpectBool = true;
    public bool OnVote(PlayerControl target)
    {
        VoteAction.Invoke(target);
        return VoteBool;
    }
    public bool OnVoteAsSpect(PlayerControl target)
    {
        VoteAsSpectAction.Invoke(target);
        return VoteAsSpectBool;
    }

    public void AddOnVote(UnityAction<PlayerControl> onVote, bool isTarget = false, bool allow = true)
    {
        if (isTarget)
        {
            VoteAction = onVote;
            VoteBool = allow;
        }
        else
        {
            VoteAsSpectAction = onVote;
            VoteAsSpectBool = allow;
        }
    }

    // Report Dead Body / Start Meeting
    public UnityAction<PlayerControl> ReportDeadBodyAction = (UnityAction<PlayerControl>)((PlayerControl a) => { });
    public UnityAction<PlayerControl> ReportDeadBodyAsSpectAction = (UnityAction<PlayerControl>)((PlayerControl a) => { });

    public bool ReportDeadBodyBool = true;
    public bool ReportDeadBodyAsSpectBool = true;
    public bool OnReportDeadBody(PlayerControl target)
    {
        ReportDeadBodyAction.Invoke(target);
        return ReportDeadBodyBool;
    }
    public bool OnReportDeadBodyAsSpect(PlayerControl target)
    {
        ReportDeadBodyAsSpectAction.Invoke(target);
        return ReportDeadBodyAsSpectBool;
    }

    public void AddOnReportDeadBody(UnityAction<PlayerControl> onReportDeadBody, bool isTarget = false, bool allow = true)
    {
        if (isTarget)
        {
            ReportDeadBodyAction = onReportDeadBody;
            ReportDeadBodyBool = allow;
        }
        else
        {
            ReportDeadBodyAsSpectAction = onReportDeadBody;
            ReportDeadBodyAsSpectBool = allow;
        }
    }
    
    public UnityAction<PlayerControl> CallMeetingAction = (UnityAction<PlayerControl>)((PlayerControl a) => { });
    public UnityAction<PlayerControl> CallMeetingAsSpectAction = (UnityAction<PlayerControl>)((PlayerControl a) => { });

    public bool CallMeetingBool = true;
    public bool CallMeetingAsSpectBool = true;
    public bool OnCallMeeting(PlayerControl target)
    {
        CallMeetingAction.Invoke(target);
        return CallMeetingBool;
    }
    public bool OnCallMeetingAsSpect(PlayerControl target)
    {
        CallMeetingAsSpectAction.Invoke(target);
        return CallMeetingAsSpectBool;
    }

    public void AddOnCallMeeting(UnityAction<PlayerControl> onCallMeeting, bool isTarget = false, bool allow = true)
    {
        if (isTarget)
        {
            CallMeetingAction = onCallMeeting;
            CallMeetingBool = allow;
        }
        else
        {
            CallMeetingAsSpectAction = onCallMeeting;
            CallMeetingAsSpectBool = allow;
        }
    }

    // Sabotage
    public UnityAction<PlayerControl> SabotageAction = (UnityAction<PlayerControl>)((PlayerControl a) => { });
    public UnityAction<PlayerControl> SabotageAsSpectAction = (UnityAction<PlayerControl>)((PlayerControl a) => { });

    public bool SabotageBool = true;
    public bool SabotageAsSpectBool = true;
    public bool OnSabotage(PlayerControl target)
    {
        SabotageAction.Invoke(target);
        return SabotageBool;
    }
    public bool OnSabotageAsSpect(PlayerControl target)
    {
        SabotageAsSpectAction.Invoke(target);
        return SabotageAsSpectBool;
    }

    public void AddOnSabotage(UnityAction<PlayerControl> onSabotage, bool isTarget = false, bool allow = true)
    {
        if (isTarget)
        {
            SabotageAction = onSabotage;
            SabotageBool = allow;
        }
        else
        {
            SabotageAsSpectAction = onSabotage;
            SabotageAsSpectBool = allow;
        }
    }

    // Phantom Vanish & Appear

    public UnityAction<PlayerControl> VanishAction = (UnityAction<PlayerControl>)((PlayerControl a) => { });
    public UnityAction<PlayerControl> VanishAsSpectAction = (UnityAction<PlayerControl>)((PlayerControl a) => { });

    public bool VanishBool = true;
    public bool VanishAsSpectBool = true;
    public bool OnVanish(PlayerControl target)
    {
        VanishAction.Invoke(target);
        return VanishBool;
    }
    public bool OnVanishAsSpect(PlayerControl target)
    {
        VanishAsSpectAction.Invoke(target);
        return VanishAsSpectBool;
    }

    public void AddOnVanish(UnityAction<PlayerControl> onVanish, bool isTarget = false, bool allow = true)
    {
        if (isTarget)
        {
            VanishAction = onVanish;
            VanishBool = allow;
        }
        else
        {
            VanishAsSpectAction = onVanish;
            VanishAsSpectBool = allow;
        }
    }
        
    public UnityAction<PlayerControl> AppearAction = (UnityAction<PlayerControl>)((PlayerControl a) => { });
    public UnityAction<PlayerControl> AppearAsSpectAction = (UnityAction<PlayerControl>)((PlayerControl a) => { });

    public bool AppearBool = true;
    public bool AppearAsSpectBool = true;
    public bool OnAppear(PlayerControl target)
    {
        AppearAction.Invoke(target);
        return AppearBool;
    }
    public bool OnAppearAsSpect(PlayerControl target)
    {
        AppearAsSpectAction.Invoke(target);
        return AppearAsSpectBool;
    }

    public void AddOnAppear(UnityAction<PlayerControl> onAppear, bool isTarget = false, bool allow = true)
    {
        if (isTarget)
        {
            AppearAction = onAppear;
            AppearBool = allow;
        }
        else
        {
            AppearAsSpectAction = onAppear;
            AppearAsSpectBool = allow;
        }
    }

    // Shapeshift

    public UnityAction<PlayerControl> ShapeshiftAction = (UnityAction<PlayerControl>)((PlayerControl a) => { });
    public UnityAction<PlayerControl> ShapeshiftAsSpectAction = (UnityAction<PlayerControl>)((PlayerControl a) => { });

    public bool ShapeshiftBool = true;
    public bool ShapeshiftAsSpectBool = true;
    public bool OnShapeshift(PlayerControl target)
    {
        ShapeshiftAction.Invoke(target);
        return ShapeshiftBool;
    }
    public bool OnShapeshiftAsSpect(PlayerControl target)
    {
        ShapeshiftAsSpectAction.Invoke(target);
        return ShapeshiftAsSpectBool;
    }

    public void AddOnShapeshift(UnityAction<PlayerControl> onShapeshift, bool isTarget = false, bool allow = true)
    {
        if (isTarget)
        {
            ShapeshiftAction = onShapeshift;
            ShapeshiftBool = allow;
        }
        else
        {
            ShapeshiftAsSpectAction = onShapeshift;
            ShapeshiftAsSpectBool = allow;
        }
    }  
    
    public UnityAction<PlayerControl> UnshiftAction = (UnityAction<PlayerControl>)((PlayerControl a) => { });
    public UnityAction<PlayerControl> UnshiftAsSpectAction = (UnityAction<PlayerControl>)((PlayerControl a) => { });

    public bool UnshiftBool = true;
    public bool UnshiftAsSpectBool = true;
    public bool OnUnshift(PlayerControl target)
    {
        UnshiftAction.Invoke(target);
        return UnshiftBool;
    }
    public bool OnUnshiftAsSpect(PlayerControl target)
    {
        UnshiftAsSpectAction.Invoke(target);
        return UnshiftAsSpectBool;
    }

    public void AddOnUnshift(UnityAction<PlayerControl> onUnshift, bool isTarget = false, bool allow = true)
    {
        if (isTarget)
        {
            UnshiftAction = onUnshift;
            UnshiftBool = allow;
        }
        else
        {
            UnshiftAsSpectAction = onUnshift;
            UnshiftAsSpectBool = allow;
        }
    }

    // Complete Task
    public UnityAction<PlayerControl> CompleteTaskAction = (UnityAction<PlayerControl>)((PlayerControl a) => { });
    public UnityAction<PlayerControl> CompleteTaskAsSpectAction = (UnityAction<PlayerControl>)((PlayerControl a) => { });

    public bool CompleteTaskBool = true;
    public bool CompleteTaskAsSpectBool = true;
    public bool OnCompleteTask(PlayerControl target)
    {
        CompleteTaskAction.Invoke(target);
        return CompleteTaskBool;
    }
    public bool OnCompleteTaskAsSpect(PlayerControl target)
    {
        CompleteTaskAsSpectAction.Invoke(target);
        return CompleteTaskAsSpectBool;
    }

    public void AddOnCompleteTask(UnityAction<PlayerControl> onCompleteTask, bool isTarget = false, bool allow = true)
    {
        if (isTarget)
        {
            CompleteTaskAction = onCompleteTask;
            CompleteTaskBool = allow;
        }
        else
        {
            CompleteTaskAsSpectAction = onCompleteTask;
            CompleteTaskAsSpectBool = allow;
        }
    }

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
    public UnityAction<PlayerControl> EnterVentAction = (UnityAction<PlayerControl>)((PlayerControl a) => { });
    public UnityAction<PlayerControl> EnterVentAsSpectAction = (UnityAction<PlayerControl>)((PlayerControl a) => { });
    public bool EnterVentBool = true;
    public bool EnterVentAsSpectBool = true;
    public bool OnEnterVent(PlayerControl target)
    {
        EnterVentAction.Invoke(target);
        return EnterVentBool;
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
            EnterVentAction = onEnterVent;
            EnterVentBool = allow;
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