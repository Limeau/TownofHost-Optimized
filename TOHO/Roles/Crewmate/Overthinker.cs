using AmongUs.GameOptions;
using System;
using System.Collections.Generic;
using TOHO.Modules;
using TOHO.Roles.Core;
using UnityEngine;
using static TOHO.Options;
using static TOHO.Translator;
using static TOHO.Utils;

namespace TOHO.Roles.Crewmate;

internal class Overthinker : RoleBase
{
    //===========================SETUP================================\\
    public override CustomRoles Role => CustomRoles.Overthinker;
    private const int Id = 41900;
    public static bool HasEnabled => CustomRoleManager.HasEnabled(CustomRoles.Overthinker);
    public override CustomRoles ThisRoleBase => CustomRoles.Crewmate;
    public override Custom_RoleType ThisRoleType => Custom_RoleType.CrewmateSupport;
    public override bool BlockMoveInVent(PlayerControl pc) => true;
    //==================================================================\\

    private static OptionItem OverthinkerMaxUses;

    private static readonly Dictionary<byte, PlainShipRoom> OverthinkerBackTrack = [];

    public override void SetupCustomOption()
    {
        SetupRoleOptions(Id, TabGroup.CrewmateRoles, CustomRoles.Overthinker);
        OverthinkerMaxUses = IntegerOptionItem.Create(Id + 10, "OverthinkerMaxUses", new(0, 20, 1), 1, TabGroup.CrewmateRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.Overthinker])
            .SetValueFormat(OptionFormat.Times);
        OverthinkerAbilityUseGainWithEachTaskCompleted = FloatOptionItem.Create(Id + 11, "AbilityUseGainWithEachTaskCompleted", new(0f, 5f, 0.1f), 1f, TabGroup.CrewmateRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.Overthinker])
            .SetValueFormat(OptionFormat.Times);
        Options.OverrideTasksData.Create(Id + 12, TabGroup.CrewmateRoles, CustomRoles.Overthinker);
    }
    public override void Init()
    {
        OverthinkerBackTrack.Clear();
    }
    public override void Add(byte playerId)
    {
        playerId.SetAbilityUseLimit(OverthinkerMaxUses.GetInt());
    }
    public override void OnFixedUpdate(PlayerControl player, bool lowLoad, long nowTime, int timerLowLoad)
    {
        _ = new LateTask(() =>
        {
            foreach (var player2 in Main.AllAlivePlayerControls)
            {
                OverthinkerBackTrack[player2.PlayerId] = player2.GetPlainShipRoom();
            }
        }, 10f, "Overthinker Positions");
    }

    public override void OnMeetingHudStart(PlayerControl pc)
    {
        string list = "Each alive player was seen in these rooms 10 seconds ago:";
        foreach (var player in Main.AllAlivePlayerControls)
        {
            list += $"\n{player.GetRealName().RemoveHtmlTags()}: ";
            list += GetString(OverthinkerBackTrack[player.PlayerId].RoomId.ToString());
        }
        if (pc.GetAbilityUseLimit() >= 1)
        {
            SendMessage(list, pc.PlayerId, title: ColorString(Utils.GetRoleColor(CustomRoles.Overthinker), GetString("OverthinkerMeetingTitle")));
            pc.RpcRemoveAbilityUse();
        }
    }
}
