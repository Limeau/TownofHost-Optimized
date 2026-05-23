using System.Linq;
using AmongUs.GameOptions;
using TOHO.Modules;
using TOHO.Roles.Core;
using UnityEngine;
using static TOHO.Options;
namespace TOHO.Roles.Neutral;

internal class SecretAgent : RoleBase
{
    //===========================SETUP================================\\
    private const int Id = 42400;
    public static bool HasEnabled => CustomRoleManager.HasEnabled(CustomRoles.SecretAgent);
    public override CustomRoles Role => CustomRoles.SecretAgent;
    public override CustomRoles ThisRoleBase => CustomRoles.Crewmate;
    public override Custom_RoleType ThisRoleType => Custom_RoleType.NeutralEvil;
    //==================================================================\\

    public static OptionItem SecretAgentDistance;
    
    public override void SetupCustomOption()
    {
        SetupRoleOptions(Id, TabGroup.NeutralRoles, CustomRoles.SecretAgent);
        SecretAgentDistance = FloatOptionItem.Create(Id + 10, "SecretAgentDistance", (1f, 5f, 0.5f), 3f, TabGroup.NeutralRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.SecretAgent])
            .SetValueFormat(OptionFormat.Multiplier);
        OverrideTasksData.Create(Id + 11, TabGroup.NeutralRoles, CustomRoles.SecretAgent);
    }

    public override bool HasTasks(NetworkedPlayerInfo player, CustomRoles role, bool ForRecompute)
    {
        return true;
    }

    public override bool OnTaskComplete(PlayerControl tasker, int completedTaskCount, int totalTaskCount)
    {
        foreach (var player in Main.AllAlivePlayerControls.Where(x => Vector2.Distance(x.transform.position, tasker.transform.position) < SecretAgentDistance.GetFloat() && x != tasker))
        {
            if (tasker.IsAlive()) tasker.RpcMurderPlayer(tasker);
            player.KillFlash();
        }

        if (tasker.IsAlive() && completedTaskCount == totalTaskCount)
        {
            CustomWinnerHolder.WinnerIds.Add(tasker.PlayerId);
            CustomWinnerHolder.ResetAndSetWinner(CustomWinner.SecretAgent);
        }
        return true;
    }
}