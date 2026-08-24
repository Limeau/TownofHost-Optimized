using AmongUs.GameOptions;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Sentry.Internal.Extensions;
using TOHO.Modules;
using UnityEngine;
using static TOHO.Options;

namespace TOHO.Roles.Crewmate;

internal class Priest : RoleBase
{
    //===========================SETUP================================\\
    public override CustomRoles Role => CustomRoles.Priest;
    private const int Id = 47100;
    public override CustomRoles ThisRoleBase => CustomRoles.Impostor;
    public override Custom_RoleType ThisRoleType => Custom_RoleType.CrewmateSupport;
    public override bool TOHORole => true;
    public override bool NewRole => true;

    //==================================================================\\
    public static OptionItem AbilityUses;
    public static List<PlayerControl> PlayerList = [];
    public override void SetupCustomOption()
    {
        SetupRoleOptions(Id, TabGroup.CrewmateRoles, CustomRoles.Priest);
        AbilityUses = IntegerOptionItem.Create(Id + 10, "PriestAbilityUses", new(1, 5, 1), 3, TabGroup.CrewmateRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Priest]);
    }

    public override bool CanUseKillButton(PlayerControl pc)
    {
        return true;
    }

    public override void Add(byte playerId)
    {
        playerId.SetAbilityUseLimit(AbilityUses.GetInt());
        PlayerList.Clear();
    }

    public override bool OnCheckMurderAsKiller(PlayerControl killer, PlayerControl target)
    {
        if (killer.GetAbilityUseLimit() <= 0) return false;
        killer.RpcRemoveAbilityUse();

        if (target.IsPlayerCrewmateTeam())
        {
            PlayerList.Add(target);
        }
        else
        {
            Main.AllPlayerKillCooldown[target.PlayerId] += 5f;
            target.MarkDirtySettings();
        }
        
        return false;
    }

    public override void AfterMeetingTasks()
    {
        PlayerList.Clear();
    }
}
