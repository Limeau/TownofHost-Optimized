using AmongUs.GameOptions;

namespace TOHO.Roles.Vanilla;

internal class DetectiveTOHO : RoleBase
{
    //===========================SETUP================================\\
    public override CustomRoles Role => CustomRoles.DetectiveTOHO;
    private const int Id = 39400;
    public override CustomRoles ThisRoleBase => CustomRoles.Detective;
    public override Custom_RoleType ThisRoleType => Custom_RoleType.CrewmateVanilla;
    //==================================================================\\

    private static OptionItem DetectiveSuspectLimit;

    public override void SetupCustomOption()
    {
        Options.SetupRoleOptions(Id, TabGroup.CrewmateRoles, CustomRoles.DetectiveTOHO);
        DetectiveSuspectLimit = IntegerOptionItem.Create(Id + 4, "DetectiveSuspectLimit394", new(10, 30, 1), 15, TabGroup.CrewmateRoles, false)
            .SetParent(Options.CustomRoleSpawnChances[CustomRoles.DetectiveTOHO])
            .SetValueFormat(OptionFormat.Seconds);
    }

    public override void ApplyGameOptions(IGameOptions opt, byte playerId)
    {
        AURoleOptions.DetectiveSuspectLimit = DetectiveSuspectLimit.GetInt();
    }
}