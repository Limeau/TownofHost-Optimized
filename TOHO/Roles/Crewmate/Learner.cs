using System.Collections.Generic;
using TOHO.Modules;
using static TOHO.Options;
using static TOHO.Translator;

namespace TOHO.Roles.Crewmate;

internal class Learner : RoleBase
{
    //===========================SETUP================================\\
    public override CustomRoles Role => CustomRoles.Learner;
    private const int Id = 47500;
    public override CustomRoles ThisRoleBase => CustomRoles.Crewmate;
    public override Custom_RoleType ThisRoleType => Custom_RoleType.CrewmatePower;
    public override bool TOHORole => true;
    public override bool NewRole => true;
    //==================================================================\\

    private static List<PlayerControl> Offenders = [];
    private static OptionItem LearnerAbilityUses;
    private static bool IsVoted = false;
    private static PlayerControl Suspect;

    public override void SetupCustomOption()
    {
        Options.SetupRoleOptions(Id, TabGroup.CrewmateRoles, CustomRoles.Learner);
        LearnerAbilityUses = IntegerOptionItem.Create(Id + 10, GeneralOption.SkillLimitTimes, new(1, 5, 1), 2, TabGroup.CrewmateRoles, false)
            .SetParent(Options.CustomRoleSpawnChances[CustomRoles.Learner])
            .SetValueFormat(OptionFormat.Times);
        LearnerAbilityUseGainWithEachTaskCompleted = FloatOptionItem.Create(Id + 11, "AbilityUseGainWithEachTaskCompleted", new(0f, 2f, 0.5f), 1f, TabGroup.CrewmateRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Learner])
            .SetValueFormat(OptionFormat.Times);
        Options.OverrideTasksData.Create(Id + 12, TabGroup.CrewmateRoles, CustomRoles.Learner);
    }

    public override void Add(byte playerId)
    {
        Offenders.Clear();
        IsVoted = false;
        Suspect = null;
        playerId.SetAbilityUseLimit(LearnerAbilityUses.GetInt());
    }

    public override void OnMeetingHudStart(PlayerControl pc)
    {
        Suspect = null;
    }

    public override bool CheckMurderOnOthersTarget(PlayerControl killer, PlayerControl target)
    {
        if (killer == Suspect)
        {
            Offenders.Add(killer);
            Utils.NotifyRoles();
            Logger.Info("Calling", "Learner");
        }
        return false;
    }

    public override bool CheckVote(PlayerControl voter, PlayerControl target)
    {
        if (voter.GetAbilityUseLimit() <= 0) return true;
        if (IsVoted) return true;
        voter.RpcRemoveAbilityUse();
        IsVoted = true;
        Suspect = target;
        MeetingHud.Instance.RpcClearVote(voter.PlayerId);
        return false;
    }

    public override void AfterMeetingTasks()
    {
        IsVoted = false;
    }

    public override string PlayerKnowTargetColor(PlayerControl seer, PlayerControl target)
    {
        if (seer == null || target == null) return string.Empty;
        if (!seer.Is(CustomRoles.Learner) || !Offenders.Contains(target)) return string.Empty;
        return "#FF1919";
    }
}
