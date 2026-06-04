using AmongUs.GameOptions;
using static TOHO.Options;

namespace TOHO.Roles.Crewmate;

internal class Valkyrie : RoleBase
{

    private const int Id = 31100;
    public override CustomRoles ThisRoleBase => CustomRoles.Crewmate;
    public override Custom_RoleType ThisRoleType => Custom_RoleType.CrewmateKilling;
    public override CustomRoles Role => CustomRoles.Valkyrie;
    public override bool TOHORole => true;

    public static OptionItem GhostKillCooldown;
    public static OptionItem RevengeTime;
    public static OptionItem Legacy;

    public override void SetupCustomOption()
    {
        SetupRoleOptions(Id, TabGroup.CrewmateRoles, CustomRoles.Valkyrie);
        GhostKillCooldown = FloatOptionItem.Create(Id + 11, "ValkyrieGhostCooldown", new(5f, 25f, 2.5f), 15f, TabGroup.CrewmateRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.Valkyrie])
            .SetValueFormat(OptionFormat.Seconds);
        Legacy = BooleanOptionItem.Create(Id + 12, "UseLegacyValkyrie", false, TabGroup.CrewmateRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Valkyrie]);
        RevengeTime = FloatOptionItem.Create(Id + 13, "ValkyrieRevengeTime", new(5f, 25f, 2.5f), 15f, TabGroup.CrewmateRoles, false)
            .SetParent(Legacy)
            .SetValueFormat(OptionFormat.Seconds);
        
    }

    public override bool CanUseKillButton(PlayerControl pc)
    {
        return true;
    }

    public override void SetKillCooldown(byte id)
    {
        Main.AllPlayerKillCooldown[id] = 1;
    }

    public override bool OnCheckMurderAsTarget(PlayerControl killer, PlayerControl target)
    {
        if (Legacy.GetBool())
        {
            target.RpcChangeRoleBasis(CustomRoles.ImpostorTOHO);
            target.RpcRandomVentTeleport();
            killer.RpcGuardAndKill();
            target.SetKillCooldownV3(1);
            
            new LateTask(() =>
            {
                if (killer.IsAlive()) target.RpcMurderPlayer(target);
                else
                {
                    target.RpcSetCustomRole(CustomRoles.Valkyrie);
                    target.RpcChangeRoleBasis(CustomRoles.Valkyrie);
                    target.Notify(Translator.GetString("ValkyrieSuccess"));
                }
            }, RevengeTime.GetFloat(), "Valkyrie Revenge");
            
            killer.Notify(Translator.GetString("ValkyrieHide"));
            target.Notify(string.Format(Translator.GetString("ValkyrieRevenge"), RevengeTime.GetFloat(), killer.GetRealName().RemoveHtmlTags()));
            
            return false;
        }
        else new LateTask(() =>
        {
            target.RpcSetCustomRole(CustomRoles.ValkyrieB, true);
        }, 1f, "Valkyrie Set Role");
        return true;
    }
}

internal class ValkyrieB : RoleBase
{
    public override CustomRoles Role => CustomRoles.ValkyrieB;
    public override bool IsDesyncRole => true;
    public override CustomRoles ThisRoleBase => CustomRoles.GuardianAngel;
    public override Custom_RoleType ThisRoleType => Custom_RoleType.CrewmateKilling;
    public override void ApplyGameOptions(IGameOptions opt, byte playerId)
    {
        AURoleOptions.GuardianAngelCooldown = Valkyrie.GhostKillCooldown.GetFloat();
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
