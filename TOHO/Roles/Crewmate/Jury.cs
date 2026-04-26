using static TOHO.Options;
namespace TOHO.Roles.Crewmate;

internal class Jury : RoleBase
{ 
    private const int Id = 40700;
    public override CustomRoles Role => CustomRoles.Jury;
    public override CustomRoles ThisRoleBase => CustomRoles.Crewmate;
    public override Custom_RoleType ThisRoleType => Custom_RoleType.CrewmatePower;

    public static PlayerControl TrialPlayer = null;
    public static int uses = 1;
    
    public override void SetupCustomOption()
    {        
        SetupRoleOptions(Id, TabGroup.CrewmateRoles, CustomRoles.Jury);
        OverrideTasksData.Create(Id + 10, TabGroup.CrewmateRoles, CustomRoles.Jury);
    }
    
    public override bool CheckVote(PlayerControl voter, PlayerControl target)
    {
        if (uses <= 0) return true;
        uses -= 1;
        TrialPlayer = target;
        return true;
    }

    public override void AfterMeetingTasks()
    {
        TrialPlayer = null;
    }

    public override bool OnTaskComplete(PlayerControl player, int completedTaskCount, int totalTaskCount)
    {
        if (completedTaskCount == totalTaskCount) uses += 1;
        return true;
    }
}