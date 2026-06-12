using System.Collections.Generic;
using TOHO.Modules;
using static TOHO.Options;

namespace TOHO.Roles.Crewmate;

internal class Director : RoleBase
{
    //===========================SETUP================================\\
    public override CustomRoles Role => CustomRoles.Director;
    private const int Id = 43600;
    public override CustomRoles ThisRoleBase => CustomRoles.Impostor;
    public override Custom_RoleType ThisRoleType => Custom_RoleType.CrewmateSupport;
    
    public override bool TOHORole => true;
    
    public override bool NewRole => true;
    public override string IdeaRole => "the_little_pelican";
    //==================================================================\\

    private static OptionItem StarCooldown;
    private static OptionItem AbilityUses;

    private static Dictionary<PlayerControl, CustomRoles> PastRoles = [];
    
    public override void SetupCustomOption()
    {
        SetupRoleOptions(Id, TabGroup.CrewmateRoles, CustomRoles.Director);
        StarCooldown = FloatOptionItem.Create(Id + 10, "DirectorStarCooldown", new(0f, 60f, 1f), 20f, TabGroup.CrewmateRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Director])
            .SetValueFormat(OptionFormat.Seconds);
        AbilityUses = IntegerOptionItem
            .Create(Id + 11, "DirectorAbilityUses", new(1, 10, 1), 5, TabGroup.CrewmateRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Director]);
    }
    public override void SetKillCooldown(byte id)
    {
        Main.AllPlayerKillCooldown[id] = StarCooldown.GetFloat();
    }

    public override void Add(byte playerId)
    {
        PastRoles.Clear();
        var player = Utils.GetPlayerById(playerId);
        player.SetAbilityUseLimit(AbilityUses.GetInt());
    }

    public override bool OnCheckMurderAsKiller(PlayerControl killer, PlayerControl target)
    {
        if (killer.GetAbilityUseLimit() <= 0) return false;
        killer.RpcRemoveAbilityUse();
        
        killer.RpcGuardAndKill();
        killer.SetKillCooldownV3(StarCooldown.GetFloat());
        
        PastRoles[target] = target.GetCustomRole();
        
        target.RpcSetCustomRole(CustomRoles.SuperStar);
        target.RpcChangeRoleBasis(CustomRoles.SuperStar);
        
        return false;
    }

    public override bool CanUseKillButton(PlayerControl pc)
    {
        return true;
    }

    public override void AfterMeetingTasks()
    {
        foreach (var entry in PastRoles)
        {
            entry.Key.RpcSetCustomRole(entry.Value);
            entry.Key.RpcChangeRoleBasis(entry.Value);
        }
        PastRoles.Clear();
    }
}
