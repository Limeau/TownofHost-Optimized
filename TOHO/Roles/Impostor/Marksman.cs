using System.Collections.Generic;
using System.Linq;
using static TOHO.Options;

namespace TOHO.Roles.Impostor;

internal class Marksman : RoleBase
{
    //===========================SETUP================================\\
    public override CustomRoles Role => CustomRoles.Marksman;
    private const int Id = 44800;
    public override CustomRoles ThisRoleBase => CustomRoles.Impostor;
    public override Custom_RoleType ThisRoleType => Custom_RoleType.ImpostorSupport;
    public override bool TOHORole => true;
    public override string IdeaRole => "den6211epic";

    public override bool NewRole => true;
    //==================================================================\\
    private static List<PlayerControl> Mark = [];
    private static OptionItem KillCooldown;
    public override void SetupCustomOption()
    {
        SetupSingleRoleOptions(Id, TabGroup.ImpostorRoles, CustomRoles.Marksman);
        KillCooldown = FloatOptionItem.Create(Id + 10, GeneralOption.KillCooldown, new(1f, 60f, 1f), 20f, TabGroup.ImpostorRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Marksman])
            .SetValueFormat(OptionFormat.Seconds);
    }
    public override void Add(byte playerId)
    {
        Mark.Clear();
        // Double Trigger
        var pc = Utils.GetPlayerById(playerId);
        pc.AddDoubleTrigger();
    }
    public override void SetKillCooldown(byte id) => Main.AllPlayerKillCooldown[id] = KillCooldown.GetFloat();
    public override bool CheckMurderOnOthersTarget(PlayerControl killer, PlayerControl target)
    {
        if (killer.IsPlayerImpostorTeam() && Mark.Contains(target) && !killer.Is(CustomRoles.Marksman))
        {
            target.RpcMurderPlayer(target);
            target.SetRealKiller(killer);
            killer.RpcTeleport(target.GetTruePosition());
            return true;
        }

        if (!killer.IsPlayerImpostorTeam() && Mark.Contains(target))
        {
            var tmpcd = Main.AllPlayerKillCooldown[killer.PlayerId];
            Main.AllPlayerKillCooldown[killer.PlayerId] = tmpcd * 2;
            killer.RpcMurderPlayer(target);
            Main.AllPlayerKillCooldown[killer.PlayerId] = tmpcd;
            return true;
        }
        
        return false;
    }

    public override bool OnCheckMurderAsKiller(PlayerControl killer, PlayerControl target)
    {
        if (killer.CheckDoubleTrigger(target, () => { }) || !Main.AllAlivePlayerControls.Where(x => x.IsPlayerImpostorTeam() && x != killer).Any())
        {
            return true;
        }

        if (!Mark.Contains(target)) Mark.Add(target);
        killer.RpcGuardAndKill(); 
        killer.ResetKillCooldown();
        
        return false;
    }
}