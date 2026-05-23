using System.Collections.Generic;
using static TOHO.Options;
namespace TOHO.Roles.Neutral;
internal class Communist : RoleBase
{    
    private const int Id = 40900;
    public override CustomRoles Role => CustomRoles.Communist;
    public override CustomRoles ThisRoleBase => CustomRoles.Impostor;
    public override Custom_RoleType ThisRoleType => Custom_RoleType.NeutralChaos;
    public override bool TOHORole => true;

    public static OptionItem CommunistRecruitCooldown;
    
    public static List<byte> Communists = [];
    public static Dictionary<byte, CustomRoles> CommunistRoles = [];

    public override void SetupCustomOption()
    {
        SetupRoleOptions(Id, TabGroup.NeutralRoles, CustomRoles.Communist); 
        CommunistRecruitCooldown = FloatOptionItem.Create(Id + 10, "CommunistRecruitCooldown", (10f, 40f, 1f), 20f, TabGroup.NeutralRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Communist])
            .SetValueFormat(OptionFormat.Seconds);    
    }

    public override bool OnCheckMurderAsKiller(PlayerControl killer, PlayerControl target)
    {
        Communists.Add(target.PlayerId);
        CommunistRoles.Add(target.PlayerId, target.GetCustomRole());
        target.RpcSetCustomRole(CustomRoles.Communist);
        target.RpcChangeRoleBasis(CustomRoles.Communist);
        killer.ResetKillCooldown();
        return false;
    }

    public override bool OnCheckMurderAsTarget(PlayerControl killer, PlayerControl target)
    {
        Communists.Remove(target.PlayerId);
        var revolutionary = Utils.GetPlayerById(Communists.RandomElement());
        revolutionary.RpcSetCustomRole(CommunistRoles[revolutionary.PlayerId]);
        revolutionary.RpcChangeRoleBasis(CommunistRoles[revolutionary.PlayerId]);
        Communists.Remove(revolutionary.PlayerId);
        return true;
    }

    public override bool CanUseKillButton(PlayerControl pc)
    {
        return true;
    }
    
    public override void SetKillCooldown(byte id) => CommunistRecruitCooldown.GetFloat();
}
