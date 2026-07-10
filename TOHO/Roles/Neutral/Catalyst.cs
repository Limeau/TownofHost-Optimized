using System.Linq;
using AmongUs.GameOptions;
using TOHO.Modules;
using TOHO.Roles.Core;
using UnityEngine;
using static TOHO.Options;
namespace TOHO.Roles.Neutral;

internal class Catalyst : RoleBase
{
    //===========================SETUP================================\\
    private const int Id = 45100;
    public static bool HasEnabled => CustomRoleManager.HasEnabled(CustomRoles.Catalyst);
    public override CustomRoles Role => CustomRoles.Catalyst;
    public override CustomRoles ThisRoleBase => CustomRoles.Impostor;
    public override Custom_RoleType ThisRoleType => Custom_RoleType.NeutralBenign;
    public override bool TOHORole => true;
    public override bool NewRole => true;
    public override string IdeaRole => "bxogamesyt";
    //==================================================================\\
    public override void SetupCustomOption()
    {
        SetupRoleOptions(Id, TabGroup.NeutralRoles, CustomRoles.Catalyst);
    }

    public override bool OnCheckMurderAsKiller(PlayerControl killer, PlayerControl target)
    {
        if (target.HasKillButton()) target.SetKillCooldownV3(0.1f);
        killer.RpcGuardAndKill();
        return false;
    }

    public override bool CanUseKillButton(PlayerControl pc)
    {
        return true;
    }
}