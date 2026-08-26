using System.Collections.Generic;
using System.Linq;
using TOHO.Modules;
using static TOHO.Options;

namespace TOHO.Roles.Impostor;

internal class Telephone : RoleBase
{
    //===========================SETUP================================\\
    public override CustomRoles Role => CustomRoles.Telephone;
    private const int Id = 47400;
    public override CustomRoles ThisRoleBase => CustomRoles.Impostor;
    public override Custom_RoleType ThisRoleType => Custom_RoleType.ImpostorKilling;
    public override bool TOHORole => true;
    public override bool NewRole => true;

    //==================================================================\\

    private static OptionItem KillCooldown;
    private static OptionItem TelephoneTimer;

    private static List<PlayerControl> ToBePlayers = [];
    private static PlayerControl ActivePlayer;
    private static PlayerControl TargetPlayer;
    private static PlayerControl TelePlayer;
    
    public override void SetupCustomOption()
    {
        SetupSingleRoleOptions(Id, TabGroup.ImpostorRoles, CustomRoles.Telephone);
        KillCooldown = FloatOptionItem.Create(Id + 10, GeneralOption.KillCooldown, new(0f, 180f, 2.5f), 20f, TabGroup.ImpostorRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Telephone])
            .SetValueFormat(OptionFormat.Seconds);
        TelephoneTimer = FloatOptionItem.Create(Id + 11, "TelephoneTimer", new(0f, 30f, 1f), 15f, TabGroup.ImpostorRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Telephone])
            .SetValueFormat(OptionFormat.Seconds);
    }

    public override void SetKillCooldown(byte id) => Main.AllPlayerKillCooldown[id] = KillCooldown.GetFloat();

    public override void Add(byte playerId)
    {
        ToBePlayers.Clear();
        ActivePlayer = null;
        TargetPlayer = null;
        TelePlayer = Utils.GetPlayerById(playerId);
    }

    public override bool OnCheckMurderAsKiller(PlayerControl killer, PlayerControl target)
    {
        if (ToBePlayers.Any()) return false;
        foreach (var player in Main.AllAlivePlayerControls.Where(x => x != killer && x != target))
        {
            ToBePlayers.Add(player);
        }
        killer.RpcGuardAndKill();
        PassTelephone(target);
        return false;
    }

    public static void PassTelephone(PlayerControl player)
    {
        if (player == TelePlayer)
        {
            ToBePlayers.Clear();
            ActivePlayer = null;
            TargetPlayer = null;
            player.RpcMurderPlayer(player);
            return;
        }
        
        if (ToBePlayers.Any())
        {
            TargetPlayer = ToBePlayers.RandomElement();
            ToBePlayers.Remove(TargetPlayer);
        }
        else
        {
            TargetPlayer = TelePlayer;
        }
        ActivePlayer.Notify("Congratulations! You passed the telephone!");
        ActivePlayer = player;
        player.Notify($"Pass the telephone to <color=#{Utils.ColorText(TargetPlayer.CurrentOutfit.ColorId)}>{TargetPlayer.name}</color>", time: TelephoneTimer.GetFloat());
        _ = new LateTask(() =>
        {
            if (ActivePlayer == player) player.RpcMurderPlayer(player);
            ToBePlayers.Clear();
            ActivePlayer = null;
            TargetPlayer = null;
        }, TelephoneTimer.GetFloat(), "Telephone Check");
    }

    public override void OnMeetingHudStart(PlayerControl pc)
    {
        ToBePlayers.Clear();
        ActivePlayer = null;
        TargetPlayer = null;
    }

    public override void OnFixedUpdate(PlayerControl player, bool lowLoad, long nowTime, int timerLowLoad)
    {
        if (ActivePlayer && TargetPlayer)
        {
            if (Utils.GetDistance(ActivePlayer.GetTruePosition(), TargetPlayer.GetTruePosition()) <= 1f)
            {
                PassTelephone(TargetPlayer);
            }
        }
    }
}