using AmongUs.GameOptions;
using static TOHO.Options;

namespace TOHO.Roles.Neutral;

internal class Hacker : RoleBase
{
    //===========================SETUP================================\\
    public override CustomRoles Role => CustomRoles.Hacker;
    private const int Id = 36800;
    public override bool IsDesyncRole => true;
    public override CustomRoles ThisRoleBase => CustomRoles.Shapeshifter;
    public override Custom_RoleType ThisRoleType => Custom_RoleType.NeutralKilling;
    //==================================================================\\

    private static PlayerControl Targeted = null;

    
    private static OptionItem KillCooldown;

    public override void SetupCustomOption()
    {
        SetupSingleRoleOptions(Id, TabGroup.NeutralRoles, CustomRoles.Hacker, 1, zeroOne: false);
        KillCooldown = FloatOptionItem.Create(Id + 10, GeneralOption.KillCooldown, new(0f, 180f, 2.5f), 20f, TabGroup.NeutralRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.Hacker])
            .SetValueFormat(OptionFormat.Seconds);
    }

    public override void Add(byte playerId)
    {
        Targeted = null;
    }

    public override void SetKillCooldown(byte id) => Main.AllPlayerKillCooldown[id] = KillCooldown.GetFloat();
    public override bool CanUseKillButton(PlayerControl pc) => true;
    public override bool CanUseImpostorVentButton(PlayerControl pc) => true;
    public override bool OnCheckMurderAsKiller(PlayerControl killer, PlayerControl target)
    {
        killer.RpcGuardAndKill();
        killer.ResetKillCooldown();
        Targeted = target;
        return false;
    }

    public override void UnShapeShiftButton(PlayerControl shapeshifter)
    {
        if (Targeted != null)
        {
            Targeted.RpcExileV2();
            Main.PlayerStates[Targeted.PlayerId].SetDead();
            Targeted.Data.IsDead = true;
            Targeted.SetDeathReason(PlayerState.DeathReason.Targeted);
            Targeted.SetRealKiller(shapeshifter);
            Targeted = null;
        }
    }
}
