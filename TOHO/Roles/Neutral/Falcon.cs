using System.Collections.Generic;
using AmongUs.GameOptions;
using static TOHO.Options;
namespace TOHO.Roles.Neutral;

internal class Falcon : RoleBase
{
    //===========================SETUP================================\\
    private const int Id = 43700;
    public override CustomRoles Role => CustomRoles.Falcon;
    public override CustomRoles ThisRoleBase => CustomRoles.Shapeshifter;
    public override bool TOHORole => true;
    public override bool NewRole => true;
    public override string IdeaRole => "the_little_pelican";
    public override Custom_RoleType ThisRoleType => Custom_RoleType.NeutralKilling;
    //==================================================================\\

    public static OptionItem AttackModeDuration;
    public static OptionItem AttackModeCooldown;
    public static OptionItem SpeedIncrease;
    public static OptionItem KillCooldown;

    public static bool AttackMode = false;
    public static List<PlayerControl> Scars = [];
    public static float tmpSpeed;
    
    public override void SetupCustomOption()
    {
        SetupRoleOptions(Id, TabGroup.NeutralRoles, CustomRoles.Falcon); 
        AttackModeDuration = FloatOptionItem.Create(Id + 10, "FalconAttackModeDuration", (1f, 20f, 1f), 10f, TabGroup.NeutralRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Falcon])
            .SetValueFormat(OptionFormat.Seconds);
        AttackModeCooldown = FloatOptionItem.Create(Id + 11, "FalconAttackModeCooldown", (1f, 60f, 1f), 20f, TabGroup.NeutralRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Falcon])
            .SetValueFormat(OptionFormat.Seconds);
        SpeedIncrease = FloatOptionItem.Create(Id + 12, "FalconSpeedIncrease", (0.1f, 2f, 0.1f), 1f, TabGroup.NeutralRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Falcon])
            .SetValueFormat(OptionFormat.Multiplier);
        KillCooldown = FloatOptionItem.Create(Id + 13, GeneralOption.KillCooldown, (1f, 60f, 1f), 20f, TabGroup.NeutralRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Falcon])
            .SetValueFormat(OptionFormat.Seconds);
    }

    public override void UnShapeShiftButton(PlayerControl shapeshifter)
    {
        if (AttackMode) return;
        tmpSpeed = Main.AllPlayerSpeed[shapeshifter.PlayerId];
        Main.AllPlayerSpeed[shapeshifter.PlayerId] += SpeedIncrease.GetFloat();
        shapeshifter.MarkDirtySettings();
        AttackMode = true;
        new LateTask(() =>
        {
            Main.AllPlayerSpeed[shapeshifter.PlayerId] = tmpSpeed;
            shapeshifter.MarkDirtySettings();
            AttackMode = false;
        }, AttackModeDuration.GetFloat(), "Falcon Attack Finish");
    }

    public override bool CanUseKillButton(PlayerControl pc)
    {
        return true;
    }

    public override void ApplyGameOptions(IGameOptions opt, byte playerId)
    {
        AURoleOptions.ShapeshifterCooldown = AttackModeCooldown.GetFloat();
    }

    public override void Add(byte playerId)
    {
        tmpSpeed = Main.AllPlayerSpeed[playerId];
        Scars.Clear();
        AttackMode = false;
    }

    public override void SetKillCooldown(byte id)
    {
        Main.AllPlayerKillCooldown[id] = KillCooldown.GetFloat();
    }

    public override bool CheckMurderOnOthersTarget(PlayerControl killer, PlayerControl target)
    {
        if (Scars.Contains(killer))
        {
            killer.RpcGuardAndKill();
            return true;
        }
        return false;
    }

    public override bool OnCheckReportDeadBody(PlayerControl reporter, NetworkedPlayerInfo deadBody, PlayerControl killer)
    {
        if (Scars.Contains(reporter)) return false;
        return true;    
    }

    public override bool OnCheckMurderAsKiller(PlayerControl killer, PlayerControl target)
    {
        if (!AttackMode) return true;
        Scars.Add(target);
        AttackMode = false;
        Main.AllPlayerSpeed[killer.PlayerId] = tmpSpeed;
        Main.AllPlayerSpeed[target.PlayerId] -= SpeedIncrease.GetFloat();
        killer.MarkDirtySettings();
        target.MarkDirtySettings();
        killer.RpcGuardAndKill();
        return false;
    }

    public override string GetProgressText(byte playerId, bool comms)
    {
        if (AttackMode) return "You are in Attack Mode!";
        return string.Empty;
    }

    public override void OnMeetingHudStart(PlayerControl pc)
    {
        new LateTask(() =>
        {
            foreach (var player in Scars)
            {
                player.SetDeathReason(PlayerState.DeathReason.Scarred);
                player.SetRealKiller(pc);
                Main.PlayersDiedInMeeting.Add(player.PlayerId);
                MurderPlayerPatch.AfterPlayerDeathTasks(pc, player, true);
                GuessManager.RpcGuesserMurderPlayer(player);
            }
        }, 5f, "Kill Falcon Scars");
    }
}