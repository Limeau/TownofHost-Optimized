using AmongUs.GameOptions;
using System.Collections.Generic;
using System.Linq;
using Sentry.Internal.Extensions;
using TOHO.Modules;
using UnityEngine;
using static TOHO.Options;

namespace TOHO.Roles.Crewmate;

internal class Prosecutor : RoleBase
{
    //===========================SETUP================================\\
    public override CustomRoles Role => CustomRoles.Prosecutor;
    private const int Id = 42200;
    public override CustomRoles ThisRoleBase => CustomRoles.Crewmate;
    public override Custom_RoleType ThisRoleType => Custom_RoleType.CrewmatePower;
    //==================================================================\\
    public static OptionItem AbilityUses;
    public static bool IsInTrial;
    public static PlayerControl TrialPlayer;
    public override void SetupCustomOption()
    {
        SetupRoleOptions(Id, TabGroup.CrewmateRoles, CustomRoles.Prosecutor);
        AbilityUses = IntegerOptionItem.Create(Id + 10, "ProsecutorAbilityUses", new(1, 5, 1), 3, TabGroup.CrewmateRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Prosecutor]);
        ProsecutorAbilityUseGainWithEachTaskCompleted = FloatOptionItem.Create(Id + 11, "AbilityUseGainWithEachTaskCompleted", new(0f, 5f, 0.1f), 1f, TabGroup.CrewmateRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Prosecutor])
            .SetValueFormat(OptionFormat.Times);
        OverrideTasksData.Create(Id + 12, TabGroup.CrewmateRoles, CustomRoles.Prosecutor);
    }
    
    
    public override void Add(byte playerId)
    {
        playerId.SetAbilityUseLimit(AbilityUses.GetInt());
        TrialPlayer = null;
        IsInTrial = false;
    }

    public override bool CheckVote(PlayerControl voter, PlayerControl target)
    {
        if (voter.GetAbilityUseLimit() <= 0) return true;
        if (target)
        {
            foreach (var player in Main.AllAlivePlayerControls)
            {
                MeetingHud.Instance.RpcClearVoteDelay(voter.GetClientId());
            }
            TrialPlayer = target;
            IsInTrial = true;
            Utils.SendMessage($"The Prosecutor has chosen to put {target.GetRealName().RemoveHtmlTags()} on trial. You may vote them, or vote skip, but nothing else!", title: Utils.ColorString(Utils.GetRoleColor(CustomRoles.Prosecutor), "PROSECUTOR INFORMATION"));
        }
        return true;
    }

    public override void AfterMeetingTasks()
    {
        TrialPlayer = null;
        IsInTrial = false;
    }
}
