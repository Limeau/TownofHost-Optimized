using System.Collections.Generic;
using TOHO.Modules;
using static TOHO.Options;

namespace TOHO.Roles.Crewmate;

internal class Neutralizer : RoleBase
{
    //===========================SETUP================================\\
    private const int Id = 38500;
    public override CustomRoles Role => CustomRoles.Neutralizer;
    public override CustomRoles ThisRoleBase => CustomRoles.Impostor;
    public override Custom_RoleType ThisRoleType => Custom_RoleType.CrewmateSupport;
    //==================================================================\\
    public static PlayerControl NeutralizedPlayer;
    public static CustomRoles NeutralizedRole;
    public static List<CustomRoles> NeutralizedModifiers = [];
    
    public static OptionItem AbilityUses;
    
    public override void SetupCustomOption()
    {
        SetupRoleOptions(Id, TabGroup.CrewmateRoles, CustomRoles.Neutralizer);
        AbilityUses = IntegerOptionItem.Create(Id + 10, "NeutralizerAbilityUses", new(1, 5, 1), 2, TabGroup.CrewmateRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Neutralizer]);
    }
    
    public override void Add(byte playerId)
    {
        playerId.SetAbilityUseLimit(AbilityUses.GetInt());
    }

    public override bool CanUseKillButton(PlayerControl pc)
    {
        return true;
    }

    public override bool OnCheckMurderAsKiller(PlayerControl killer, PlayerControl target)
    {
        if (killer.GetAbilityUseLimit() <= 0) return false;
        if (NeutralizedPlayer != null) return false;
        killer.RpcRemoveAbilityUse();
        NeutralizedPlayer = target;
        NeutralizedRole = target.GetCustomRole();
        foreach (var Modifier in target.GetCustomSubRoles())
        {
            NeutralizedModifiers.Add(Modifier);
            Main.PlayerStates[target.PlayerId].RemoveSubRole(Modifier);
        }
        target.RpcSetCustomRole(CustomRoles.Neutralized);
        target.RpcChangeRoleBasis(CustomRoles.Neutralized);

        killer.RpcGuardAndKill();
        killer.ResetKillCooldown();
        return false;
    }

    public override void AfterMeetingTasks()
    {
        NeutralizedPlayer.RpcSetCustomRole(NeutralizedRole);
        NeutralizedPlayer.RpcChangeRoleBasis(NeutralizedRole);
        foreach (var Modifier in NeutralizedModifiers) NeutralizedPlayer.RpcSetCustomRole(Modifier);
        NeutralizedPlayer = null;
        NeutralizedRole = CustomRoles.NotAssigned;
        NeutralizedModifiers.Clear();
    }
}

internal class Neutralized : RoleBase
{
    public override CustomRoles Role => CustomRoles.Neutralized;
    public override CustomRoles ThisRoleBase => CustomRoles.Crewmate;
    public override Custom_RoleType ThisRoleType => Custom_RoleType.NeutralBenign;
}
