using AmongUs.GameOptions;
using static TOHO.Options;

namespace TOHO.Roles.Crewmate;

internal class Paranormal : RoleBase
{
    private const int Id = 43100;
    public override CustomRoles ThisRoleBase => CustomRoles.Crewmate;
    public override Custom_RoleType ThisRoleType => Custom_RoleType.CrewmateKilling;
    public override CustomRoles Role => CustomRoles.Paranormal;
    public override bool TOHORole => true;
    public override bool NewRole => true;
    public static OptionItem GhostKillCooldown;

    public override bool OnCheckMurderAsTarget(PlayerControl killer, PlayerControl target)
    {
        new LateTask(() =>
        {
            target.RpcSetCustomRole(CustomRoles.ParanormalB, true);
        }, 1f, "Paranormal Set Role");
        return true;
    }
    
    
    public override void SetupCustomOption()
    {
        SetupRoleOptions(Id, TabGroup.CrewmateRoles, CustomRoles.Paranormal);
        GhostKillCooldown = FloatOptionItem.Create(Id + 11, "ValkyrieGhostCooldown", new(5f, 25f, 2.5f), 15f, TabGroup.CrewmateRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Paranormal])
            .SetValueFormat(OptionFormat.Seconds);
    }
}

internal class ParanormalB : RoleBase
{
    public override CustomRoles Role => CustomRoles.ParanormalB;
    public override bool IsDesyncRole => true;
    public override CustomRoles ThisRoleBase => CustomRoles.GuardianAngel;
    public override Custom_RoleType ThisRoleType => Custom_RoleType.CrewmateKilling;
    public override void ApplyGameOptions(IGameOptions opt, byte playerId)
    {
        AURoleOptions.GuardianAngelCooldown = Paranormal.GhostKillCooldown.GetFloat();
        AURoleOptions.ProtectionDurationSeconds = 0f;
    }
    public override bool OnCheckProtect(PlayerControl killer, PlayerControl target)
    {
        killer.RpcMurderPlayer(target);
        killer.RpcRevive();
        killer.RpcChangeRoleBasis(CustomRoles.Crewmate);
        killer.RpcSetCustomRole(CustomRoles.Crewmate, true);
        return false;
    }
}