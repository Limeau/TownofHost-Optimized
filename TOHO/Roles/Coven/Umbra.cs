using System.Collections.Generic;
using System.Linq;
using Rewired;
using TOHO.Roles.Modifiers;
using static TOHO.Options;

namespace TOHO.Roles.Coven;

internal class Umbra : CovenManager
{
    //===========================SETUP================================\\
    public override CustomRoles Role => CustomRoles.Umbra;
    private const int Id = 39600;
    public override bool IsDesyncRole => true;
    public override CustomRoles ThisRoleBase => CustomRoles.Impostor;
    public override Custom_RoleType ThisRoleType => Custom_RoleType.CovenPower;
    public override bool TOHORole => true;
    //==================================================================\\

    private static int Shields = 0;
    private static PlayerControl SelectedPlayer;

    public override void SetupCustomOption()
    {
        SetupSingleRoleOptions(Id, TabGroup.CovenRoles, CustomRoles.Umbra, 1, zeroOne: false);
    }
    public override bool CanUseKillButton(PlayerControl pc) => true;

    public override bool OnCheckMurderAsKiller(PlayerControl killer, PlayerControl target)
    {
        if (HasNecronomicon(killer))
        {
            Shields++;
            return true;
        }
        SelectedPlayer = target;
        killer.RpcGuardAndKill(killer);
        killer.ResetKillCooldown();
        return false;
    }

    public override bool CheckMurderOnOthersTarget(PlayerControl killer, PlayerControl target)
    {
        if (target.IsPlayerCoven() && SelectedPlayer != null && !HasNecronomicon(_Player))
        {
            var tarpos = target.GetCustomPosition();
            target.RpcTeleport(SelectedPlayer.GetCustomPosition());
            SelectedPlayer.RpcTeleport(tarpos);
            killer.RpcMurderPlayer(SelectedPlayer);
            return true;
        }
        if (HasNecronomicon(_Player) && Shields >= 1)
        {
            Shields--;
            killer.RpcGuardAndKill(killer);
            killer.ResetKillCooldown();
            return true;
        }
        return false;
    }

    public override void AfterMeetingTasks()
    {
        SelectedPlayer = null;
    }
}
