using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using static TOHO.Options;

namespace TOHO.Roles.Neutral;

internal class Spider : RoleBase
{
    //===========================SETUP================================\\
    public override CustomRoles Role => CustomRoles.Spider;
    private const int Id = 47200;
    public override CustomRoles ThisRoleBase => CustomRoles.Impostor;
    public override Custom_RoleType ThisRoleType => Custom_RoleType.NeutralKilling;
    public override bool TOHORole => true;
    public override bool NewRole => true;

    //==================================================================\\
    public static bool IsWebActive = false;
    public static Dictionary<PlayerControl, float> PlayersInWeb = [];
    public override bool CanUseKillButton(PlayerControl pc)
    {
        return true;
    }

    private static OptionItem KillCooldown;

    public override void SetupCustomOption()
    {
        SetupRoleOptions(Id, TabGroup.NeutralRoles, CustomRoles.Spider);
        KillCooldown = FloatOptionItem.Create(Id + 2, GeneralOption.KillCooldown, new(1f, 60f, 1f), 20f, TabGroup.NeutralRoles, false)
            .SetParent(Options.CustomRoleSpawnChances[CustomRoles.Spider])
            .SetValueFormat(OptionFormat.Seconds);
    }

    public override void SetKillCooldown(byte id) => Main.AllPlayerKillCooldown[id] = KillCooldown.GetFloat();

    public override void Add(byte playerId)
    {
        IsWebActive = false;
        PlayersInWeb.Clear();
    }

    public static void ResetSpeed()
    {
        IsWebActive = false;
        foreach (var kvp in PlayersInWeb)
        {
            Main.AllPlayerSpeed[kvp.Key.PlayerId] = kvp.Value;
            kvp.Key.MarkDirtySettings();
            PlayersInWeb.Remove(kvp.Key);
        }
    }
    
    public override bool OnCheckMurderAsKiller(PlayerControl killer, PlayerControl target)
    {
        if (IsWebActive && PlayersInWeb.ContainsKey(target))
        {
            foreach (var player in PlayersInWeb.Keys)
            {
                killer.RpcMurderPlayer(player);
            }
            ResetSpeed();
            killer.RpcGuardAndKill();
            return false;
        }

        if (IsWebActive) return false;
        
        PlayersInWeb[target] = Main.AllPlayerSpeed[target.PlayerId];
        Main.AllPlayerSpeed[target.PlayerId] = 0;
        target.MarkDirtySettings();
        killer.RpcGuardAndKill();
        IsWebActive = true;
        return false;
    }

    public override void OnFixedUpdate(PlayerControl player, bool lowLoad, long nowTime, int timerLowLoad)
    {
        foreach (var player2 in Main.AllAlivePlayerControls.Where(x => !x.Is(CustomRoles.Spider)))
        {
            if (IsWebActive)
            {
                foreach (var webbed in PlayersInWeb.Keys.Where(x => x != player2))
                {
                    if (Utils.GetDistance(webbed.GetTruePosition(), player2.GetTruePosition()) <= 1f)
                    {
                        PlayersInWeb[player2] = Main.AllPlayerSpeed[player2.PlayerId];
                        Main.AllPlayerSpeed[player2.PlayerId] = 0;
                        player2.MarkDirtySettings();
                        return;
                    }
                }
            }
        }
    }

    public override void AfterMeetingTasks()
    {
        ResetSpeed();
    }
}
