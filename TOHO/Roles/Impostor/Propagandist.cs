using System.Collections.Generic;
using Rewired;
using static TOHO.Options;

namespace TOHO.Roles.Impostor;

internal class Propagandist : RoleBase
{
    //===========================SETUP================================\\
    public override CustomRoles Role => CustomRoles.Propagandist;
    private const int Id = 36100;
    public override CustomRoles ThisRoleBase => CustomRoles.Impostor;
    public override Custom_RoleType ThisRoleType => Custom_RoleType.ImpostorSupport;
    public override bool TOHORole => true;
    public override string IdeaRole => "ragavrulez65";
    //==================================================================\\

    public static OptionItem KillCooldown;

    public static HashSet<byte> Players = [];
    public override void SetupCustomOption()
    {
        SetupRoleOptions(Id, TabGroup.ImpostorRoles, CustomRoles.Propagandist);
        KillCooldown = FloatOptionItem.Create(Id + 10, GeneralOption.KillCooldown, new(0f, 60f, 1f), 20f, TabGroup.ImpostorRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Propagandist])
            .SetValueFormat(OptionFormat.Seconds);
    }
    
    
    public override void Add(byte playerId)
    {
        playerId.GetPlayer()?.AddDoubleTrigger();
    }
    
    public override void SetKillCooldown(byte id) => Main.AllPlayerKillCooldown[id] = KillCooldown.GetFloat();

    public override bool OnCheckMurderAsKiller(PlayerControl killer, PlayerControl target)
    {
        // Use double trigger system
        if (killer.CheckDoubleTrigger(target, () => { }))
        {
            return true;
        }
        Players.Add(target.PlayerId);
        killer.RpcGuardAndKill();
        return false;
    }

    public override int AddRealVotesNum(PlayerVoteArea PVA) => Players.Count;
    
    public override string GetMarkOthers(PlayerControl seer, PlayerControl target, bool isForMeeting = false)
    {
        if ((!seer.IsAlive() || seer.Is(CustomRoles.Propagandist)) && Players.Contains(target.PlayerId))
        {
            return Utils.ColorString(Utils.GetRoleColor(CustomRoles.Propagandist), "♦");
        }
        return string.Empty;
    }

    public override void AfterMeetingTasks()
    {
        Players.Clear();
    }
}
