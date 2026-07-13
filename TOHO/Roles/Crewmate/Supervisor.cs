using static TOHO.Options;
using static TOHO.Utils;

namespace TOHO.Roles.Crewmate;

internal class Supervisor : RoleBase
{
    //===========================SETUP================================\\
    public override CustomRoles Role => CustomRoles.Supervisor;
    private const int Id = 44600;
    public override CustomRoles ThisRoleBase => CustomRoles.Crewmate;
    public override Custom_RoleType ThisRoleType => Custom_RoleType.CrewmateInvestigative;

    public override bool TOHORole => true;
    public override string IdeaRole => "bearyyy_cocoa";
    //==================================================================\\

    public override void SetupCustomOption()
    {
        SetupRoleOptions(Id, TabGroup.CrewmateRoles, CustomRoles.Supervisor);
        OverrideTasksData.Create(Id + 10, TabGroup.CrewmateRoles, CustomRoles.Supervisor);
    }

    public override string GetMark(PlayerControl seer, PlayerControl seen, bool isForMeeting = false)
    {
        if (!seer.IsAlive() || !seen.IsAlive() || !seer.AllTasksCompleted() || seer == seen) return string.Empty;

        if (seen.GetPlayerTaskState().hasTasks) return ColorString(GetRoleColor(CustomRoles.Supervisor), $" ({seen.GetPlayerTaskState().CompletedTasksCount}/{seen.GetPlayerTaskState().AllTasksCount})");
        return ColorString(GetRoleColor(CustomRoles.Supervisor), $" (No Tasks)");
    }
}