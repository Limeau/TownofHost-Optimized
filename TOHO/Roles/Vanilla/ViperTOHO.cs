using AmongUs.GameOptions;

namespace TOHO.Roles.Vanilla;

internal class ViperTOHO : RoleBase
{
    //===========================SETUP================================\\
    public override CustomRoles Role => CustomRoles.ViperTOHO;
    private const int Id = 39300;
    public override CustomRoles ThisRoleBase => CustomRoles.Viper;
    public override Custom_RoleType ThisRoleType => Custom_RoleType.ImpostorVanilla;
    //==================================================================\\

    private static OptionItem ViperDissolveTime;

    public override void SetupCustomOption()
    {
        Options.SetupRoleOptions(Id, TabGroup.ImpostorRoles, CustomRoles.ViperTOHO);
        ViperDissolveTime = FloatOptionItem.Create(Id + 4, "DissolveTime393", new(10, 30, 1), 15, TabGroup.ImpostorRoles, false)
            .SetParent(Options.CustomRoleSpawnChances[CustomRoles.ViperTOHO])
            .SetValueFormat(OptionFormat.Seconds);
    }

    public override void ApplyGameOptions(IGameOptions opt, byte playerId)
    {
        AURoleOptions.ViperDissolveTime = ViperDissolveTime.GetFloat();
    }
}