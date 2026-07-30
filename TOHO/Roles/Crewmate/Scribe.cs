using TOHO.Modules;
using static TOHO.Options;

namespace TOHO.Roles.Crewmate;

internal class Scribe : RoleBase
{
    //===========================SETUP================================\\
    public override CustomRoles Role => CustomRoles.Scribe;
    private const int Id = 46100;
    public override CustomRoles ThisRoleBase => CustomRoles.Crewmate;
    public override Custom_RoleType ThisRoleType => Custom_RoleType.CrewmateKilling;
    public override bool TOHORole => true;
    public override bool NewRole => true;
    //==================================================================\\
    
    public static OptionItem ScribeCanBeGuessed;

    public override void SetupCustomOption()
    {
        SetupRoleOptions(Id, TabGroup.CrewmateRoles, CustomRoles.Scribe);
        ScribeCanBeGuessed = BooleanOptionItem.Create(Id + 10, "ScribeGuessed", false, TabGroup.CrewmateRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Scribe]);
    }
    public override void Add(byte playerId)
    {
        Utils.GetPlayerById(playerId).SetChatVisibleSpecific();
    }
    public override void AfterMeetingTasks()
    {
        _Player.SetChatVisibleSpecific();
        AntiBlackout.SetIsDead();
    }
}