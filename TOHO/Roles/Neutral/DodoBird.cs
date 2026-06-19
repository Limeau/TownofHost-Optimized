using System.Linq;
using AmongUs.GameOptions;
using TOHO.Modules;
using TOHO.Roles.Core;
using UnityEngine;
using static TOHO.Options;
namespace TOHO.Roles.Neutral;

internal class DodoBird : RoleBase
{
    //===========================SETUP================================\\
    private const int Id = 43900;
    public static bool HasEnabled => CustomRoleManager.HasEnabled(CustomRoles.DodoBird);
    public override CustomRoles Role => CustomRoles.DodoBird;
    public override CustomRoles ThisRoleBase => CustomRoles.Crewmate;
    public override Custom_RoleType ThisRoleType => Custom_RoleType.NeutralBenign;
    public override bool TOHORole => true;

    public override string IdeaRole => "the_little_pelican";
    //==================================================================\\

    public static PlayerControl Killer = null;
    
    public override void SetupCustomOption()
    {
        SetupRoleOptions(Id, TabGroup.NeutralRoles, CustomRoles.DodoBird);
    }

    public override void Add(byte playerId)
    {
        Killer = null;
    }

    public override bool OnCheckMurderAsTarget(PlayerControl killer, PlayerControl target)
    {
        Killer = killer;
        return true;
    }
}