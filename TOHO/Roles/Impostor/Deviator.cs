using AmongUs.GameOptions;
using static TOHO.Options;
namespace TOHO.Roles.Impostor;

internal class Deviator : RoleBase
{
    //===========================SETUP================================\\
    private const int Id = 40100;
    public override CustomRoles Role => CustomRoles.Deviator;
    public override CustomRoles ThisRoleBase => CustomRoles.Impostor;

    public override Custom_RoleType ThisRoleType => Custom_RoleType.ImpostorSupport;
    //==================================================================\\

    public static OptionItem DeviatorShieldDuration;
    public static OptionItem DeviatorShieldCooldown;

    public override void SetupCustomOption()
    {
        SetupRoleOptions(Id, TabGroup.ImpostorRoles, CustomRoles.Deviator);
        DeviatorShieldDuration = FloatOptionItem.Create(Id + 10, "DeviatorShieldDuration", (10f, 40f, 1f), 20f,
                TabGroup.ImpostorRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Deviator])
            .SetValueFormat(OptionFormat.Seconds);
        DeviatorShieldCooldown = FloatOptionItem.Create(Id + 11, "DeviatorShieldCooldown", (10f, 40f, 1f), 20f,
                TabGroup.ImpostorRoles, false)
            .SetParent((CustomRoleSpawnChances[CustomRoles.Deviator])
            .SetValueFormat(OptionFormat.Seconds));
    }
    
    public static bool IsAlert = false;

    public override void UnShapeShiftButton(PlayerControl shapeshifter)
    {
        IsAlert = true;
        shapeshifter.RpcResetAbilityCooldown();
        new LateTask(() =>
        {
            IsAlert = false;
        }, DeviatorShieldDuration.GetFloat(), "Remove alert Deviator");
    }

    public override bool OnCheckMurderAsTarget(PlayerControl killer, PlayerControl target)
    {
        if (IsAlert)
        {
            killer.RpcMurderPlayer(killer);
            IsAlert = false;
            return false;
        }
        return true;
    }

    public override void ApplyGameOptions(IGameOptions opt, byte playerId)
    {
        AURoleOptions.ShapeshifterCooldown = DeviatorShieldCooldown.GetFloat();
    }
}

