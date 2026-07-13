using System.Linq;
using AmongUs.GameOptions;
using TOHO.Modules;
using TOHO.Roles.Core;
using UnityEngine;
using static TOHO.Options;
namespace TOHO.Roles.Neutral;

internal class Dreadsteed : RoleBase
{
    //===========================SETUP================================\\
    private const int Id = 44300;
    public static bool HasEnabled => CustomRoleManager.HasEnabled(CustomRoles.Dreadsteed);
    public override CustomRoles Role => CustomRoles.Dreadsteed;
    public override CustomRoles ThisRoleBase => CustomRoles.Crewmate;
    public override Custom_RoleType ThisRoleType => Custom_RoleType.NeutralBenign;
    public override bool TOHORole => true;
    public override string IdeaRole => "puffyxavy";
    //==================================================================\\

    public static int Stage = 0;
    public static bool IsDanger;
    public static float TmpSpeed;
    
    public static PlayerControl DreadsteedPlayer;
    
    public override void SetupCustomOption()
    {
        SetupSingleRoleOptions(Id, TabGroup.NeutralRoles, CustomRoles.Dreadsteed);
        OverrideTasksData.Create(Id + 11, TabGroup.NeutralRoles, CustomRoles.Dreadsteed);
    }

    public override void Add(byte playerId)
    {
        DreadsteedPlayer = Utils.GetPlayerById(playerId);
        TmpSpeed = Main.AllPlayerSpeed[playerId];
    }

    public override bool HasTasks(NetworkedPlayerInfo player, CustomRoles role, bool ForRecompute)
    {
        return true;
    }

    public override bool OnTaskComplete(PlayerControl tasker, int completedTaskCount, int totalTaskCount)
    {
        if (completedTaskCount == totalTaskCount && Stage < 4)
        {
            Stage = 0;
        }
        return true;
    }

    public override void AfterMeetingTasks()
    {
        Main.AllPlayerSpeed[DreadsteedPlayer.PlayerId] = TmpSpeed;
        if (DreadsteedPlayer.GetCustomSubRoles().Contains(CustomRoles.Fragile)) Main.PlayerStates[DreadsteedPlayer.PlayerId].RemoveSubRole(CustomRoles.Fragile);
        Main.PlayerStates[DreadsteedPlayer.PlayerId].IsBlackOut = false;
        IsDanger = false;
        if (Stage < 4 && !DreadsteedPlayer.IsAlive())
        {
            Stage += 1;
            DreadsteedPlayer.RpcRevive();
            DreadsteedPlayer.RpcChangeRoleBasis(CustomRoles.Dreadsteed);
            
            if (Stage >= 1)
            {
                Main.AllPlayerSpeed[DreadsteedPlayer.PlayerId] = TmpSpeed - 1f;
            }
            if (Stage >= 2)
            {
                DreadsteedPlayer.RpcSetCustomRole(CustomRoles.Fragile);
            }
            if (Stage >= 3)
            {
                Main.PlayerStates[DreadsteedPlayer.PlayerId].IsBlackOut = true;
            }
            if (Stage >= 4)
            {
                IsDanger = false;
            }
        }
        DreadsteedPlayer.MarkDirtySettings();
    }

    public override void OnFixedUpdate(PlayerControl player, bool lowLoad, long nowTime, int timerLowLoad)
    {
        if (!AmongUsClient.Instance.AmHost) return;
        
        if (IsDanger)
        {
            foreach (var player2 in Main.AllAlivePlayerControls.Where(x => x != DreadsteedPlayer))
            {
                if (Utils.GetDistance(player2.transform.position, DreadsteedPlayer.transform.position) <= 1f)
                {
                    DreadsteedPlayer.RpcMurderPlayer(DreadsteedPlayer);
                }
            }
        }
    }
}