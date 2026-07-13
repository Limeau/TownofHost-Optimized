using AmongUs.GameOptions;
using System.Linq;
using System.Collections.Generic;
using TOHO.Modules;
using TOHO.Roles.Crewmate;

namespace TOHO.Roles.Neutral;

internal class Gastlighter : RoleBase
{
    //===========================SETUP================================\\
    public override CustomRoles Role => CustomRoles.Gastlighter;
    private const int Id = 44400;
    public override CustomRoles ThisRoleBase => HasSelected ? CustomRoles.Crewmate : CustomRoles.Impostor;
    public override Custom_RoleType ThisRoleType => Custom_RoleType.NeutralEvil;
    public override bool TOHORole => true;
    public override string IdeaRole => "the_little_pelican";
    //==================================================================\\

    public static PlayerControl SelectedPlayer;
    public static bool HasSelected;
    
    public override void SetupCustomOption()
    {
        Options.SetupSingleRoleOptions(Id, TabGroup.NeutralRoles, CustomRoles.Gastlighter);
    }

    public override void Add(byte playerId)
    {
        HasSelected = false;
    }

    public override bool CanUseKillButton(PlayerControl pc) => true;

    public override bool OnCheckMurderAsKiller(PlayerControl killer, PlayerControl target)
    {
        SelectedPlayer = target;
        HasSelected = true;
        killer.RpcChangeRoleBasis(killer.GetCustomRole());
        return false;
    }

    public override string GetMarkOthers(PlayerControl seer, PlayerControl seen, bool isForMeeting = false)
    {
        if (!seer.Is(CustomRoles.Gastlighter) || seen != SelectedPlayer) return string.Empty;
        return Utils.ColorString(Utils.GetRoleColor(CustomRoles.Gastlighter), "■");
    }
}