using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using TOHO.Roles.Core;
using static TOHO.Options;
namespace TOHO.Roles.Neutral;

internal class Developer : RoleBase
{
    //===========================SETUP================================\\
    private const int Id = 40000;
    public static bool HasEnabled => CustomRoleManager.HasEnabled(CustomRoles.Developer);
    public override CustomRoles Role => CustomRoles.Developer;
    public override CustomRoles ThisRoleBase => CustomRoles.Impostor;
    public override Custom_RoleType ThisRoleType => Custom_RoleType.NeutralBenign;
    //==================================================================\\

    public static List<PlayerControl> Customers = [];
    public static List<CustomRoles> ImpRoles = [];
    public static List<CustomRoles> CrewRoles = [];
    public static List<CustomRoles> NeutralRoles = [];

    public override void Add(byte playerId)
    {
        foreach (var role in CustomRolesHelper.AllRoles.Where(x => x.IsEnable() && !x.IsAdditionRole()))
        {
            if (role.IsCrewmate()) CrewRoles.Add(role);
            if (role.IsNeutral()) NeutralRoles.Add(role);
            if (role.IsImpostor()) ImpRoles.Add(role);
        }
    }

    public override void SetupCustomOption()
    {
        SetupRoleOptions(Id, TabGroup.NeutralRoles, CustomRoles.Developer);
    }

    public override bool OnCheckMurderAsKiller(PlayerControl killer, PlayerControl target)
    {
        if (target.IsPlayerImpostorTeam() && ImpRoles.Any())
        {
            Customers.Add(target);
            var role = ImpRoles.RandomElement();
            target.RpcSetCustomRole(role);
            target.RpcChangeRoleBasis(role);
        }

        if (target.IsPlayerCrewmateTeam() && CrewRoles.Any())
        {
            Customers.Add(target);
            var role = CrewRoles.RandomElement();
            target.RpcSetCustomRole(role);
            target.RpcChangeRoleBasis(role);
        }

        if (target.IsPlayerNeutralTeam() && NeutralRoles.Any())
        {
            Customers.Add(target);
            var role = NeutralRoles.RandomElement();
            target.RpcSetCustomRole(role);
            target.RpcChangeRoleBasis(role);
        }
        
        killer.RpcGuardAndKill(fromSetKCD: true);
        return false;
    }

    public override bool CanUseKillButton(PlayerControl pc)
    {
        return true;
    }
}
