using static TOHO.Options;
namespace TOHO.Roles.Neutral;

internal class Extremist : RoleBase
{     
    private const int Id = 40800;
    public override CustomRoles Role => CustomRoles.Extremist;
    public override CustomRoles ThisRoleBase => CustomRoles.Crewmate;
    public override Custom_RoleType ThisRoleType => Custom_RoleType.NeutralBenign;
    public override void SetupCustomOption()
    {
        SetupRoleOptions(Id, TabGroup.NeutralRoles, CustomRoles.Extremist); 
    }
}