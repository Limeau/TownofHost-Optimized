using Hazel;
using TOHO.Modules;
using static TOHO.Options;
using static TOHO.Translator;
using UnityEngine;
using static TOHO.MeetingHudStartPatch;
using AmongUs.GameOptions;
using TOHO.Roles.Core;
using TOHO.Roles.Coven;
using TOHO.Roles.Impostor;

namespace TOHO.Roles.Crewmate;

internal class ForensicScientist : RoleBase
{
    //===========================SETUP================================\\
    public override CustomRoles Role => CustomRoles.ForensicScientist;
    private const int Id = 35800;
    public override CustomRoles ThisRoleBase => CustomRoles.Scientist;
    public override Custom_RoleType ThisRoleType => Custom_RoleType.CrewmateSupport;
    //==================================================================\\

    private static OptionItem SampleFailChance;
    private static OptionItem VitalsDuration;
    private static OptionItem CanRemoveCurses;
    private static OptionItem CursesRemoveMax;
    private static OptionItem ShowArrows;
    private static OptionItem VitalsCooldownAfterTasks;

    private static readonly Dictionary<byte, byte> ReportedBodies = [];
    private static readonly Dictionary<byte, byte> IdentifiedKillers = [];
    private static readonly Dictionary<byte, string> AnalysisResults = [];
    private static readonly Dictionary<byte, bool> SamplesCollected = [];



    public override void SetupCustomOption()
    {
        SetupRoleOptions(Id, TabGroup.CrewmateRoles, CustomRoles.ForensicScientist);
        SampleFailChance = IntegerOptionItem.Create(Id + 2, "ForensicSampleFailChance", new(0, 100, 5), 50, TabGroup.CrewmateRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.ForensicScientist])
            .SetValueFormat(OptionFormat.Percent);
        VitalsDuration = FloatOptionItem.Create(Id + 3, "ForensicVitalsDuration", new(5f, 60f, 2.5f), 20f, TabGroup.CrewmateRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.ForensicScientist])
            .SetValueFormat(OptionFormat.Seconds);
        CanRemoveCurses = BooleanOptionItem.Create(Id + 4, "ForensicCanRemoveCurses", true, TabGroup.CrewmateRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.ForensicScientist]);
        CursesRemoveMax = IntegerOptionItem.Create(Id + 7, "CursesRemoveMax", new(1, 999, 1), 1, TabGroup.CrewmateRoles, false).SetParent(CanRemoveCurses)
            .SetValueFormat(OptionFormat.Times);
        ShowArrows = BooleanOptionItem.Create(Id + 5, "ForensicShowArrows", true, TabGroup.CrewmateRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.ForensicScientist]);
        VitalsCooldownAfterTasks = FloatOptionItem.Create(Id + 6, "ForensicVitalsCooldown", new(0f, 60f, 5f), 15f, TabGroup.CrewmateRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.ForensicScientist])
            .SetValueFormat(OptionFormat.Seconds);
    }

    public override void Init()
    {
        ReportedBodies.Clear();
        AnalysisResults.Clear();
        SamplesCollected.Clear();
    }

    public override void Add(byte playerId)
    {
        playerId.SetAbilityUseLimit(CursesRemoveMax.GetInt());
        if (ShowArrows.GetBool())
        {
            CustomRoleManager.CheckDeadBodyOthers.Add(CheckDeadBody);
        }
    }

    public override void Remove(byte playerId)
    {
        if (ShowArrows.GetBool())
        {
            CustomRoleManager.CheckDeadBodyOthers.Remove(CheckDeadBody);
        }
    }

    public override bool OnTaskComplete(PlayerControl player, int completedTaskCount, int totalTaskCount)
    {
        if (!player.IsAlive()) return true;
        if (player.GetAbilityUseLimit() < 3)
        {
            player.SetAbilityUseLimit(player.GetAbilityUseLimit() + 1);
        }
        return true;
    }


    public override void ApplyGameOptions(IGameOptions opt, byte playerId)
    {
        // Set vitals duration
        AURoleOptions.ScientistCooldown = VitalsCooldownAfterTasks.GetFloat();
        AURoleOptions.ScientistBatteryCharge = VitalsDuration.GetFloat();
    }

    private void CheckDeadBody(PlayerControl killer, PlayerControl target, bool inMeeting)
    {
        if (inMeeting || target.IsDisconnected()) return;

        var player = _Player;
        if (player == null || !player.IsAlive()) return;

        LocateArrow.Add(player.PlayerId, target.Data.GetDeadBody().transform.position);
    }

    public override void OnReportDeadBody(PlayerControl reporter, NetworkedPlayerInfo target)
    {
        var player = _Player;
        if (player == null || !player.IsAlive() || reporter == null || target == null) return;

        if (ShowArrows.GetBool())
        {
            LocateArrow.RemoveAllTarget(player.PlayerId);
        }

        if (reporter.PlayerId != player.PlayerId) return;
        if (target.PlayerId == player.PlayerId) return;

        ReportedBodies[player.PlayerId] = target.PlayerId;
        SamplesCollected[player.PlayerId] = true;
    }


    public override void OnMeetingHudStart(PlayerControl pc)
    {
        var player = _Player;
        if (player == null) return;

        if (ReportedBodies.TryGetValue(player.PlayerId, out var bodyId) &&
            SamplesCollected.TryGetValue(player.PlayerId, out var collected) && collected)
        {
            bool sampleFailed = IRandom.Instance.Next(0, 100) < SampleFailChance.GetInt();

            if (!sampleFailed)
            {
                var killerId = bodyId.GetRealKillerById();
                if (killerId != null)
                {

                    IdentifiedKillers[player.PlayerId] = killerId.PlayerId;
                    AnalysisResults[player.PlayerId] = string.Format(GetString("ForensicAnalysisSuccess"), killerId.GetRealName());

                }
                else
                {
                    AnalysisResults[player.PlayerId] = GetString("ForensicAnalysisNoKiller");
                }
            }
            else
            {
                AnalysisResults[player.PlayerId] = GetString("ForensicAnalysisFailed");
            }

            AddMsg(AnalysisResults[player.PlayerId], player.PlayerId,
                Utils.ColorString(Utils.GetRoleColor(CustomRoles.ForensicScientist), GetString("ForensicAnalysisTitle")));
            SamplesCollected[player.PlayerId] = false;
        }
    }

    public override void AfterMeetingTasks()
    {

        IdentifiedKillers.Clear();
    }

    public override void OnVote(PlayerControl voter, PlayerControl target)
    {
        var player = _Player;
        if (player == null || !player.IsAlive() || voter.PlayerId != player.PlayerId || target == null) return;

        if (CanRemoveCurses.GetBool() && player.GetAbilityUseLimit() > 0)
        {
            bool removedAnyCurse = false;

            // Remove Witch spells
            foreach (var witch in Witch.SpelledPlayer)
            {
                if (witch.Value.Contains(target.PlayerId))
                {
                    witch.Value.Remove(target.PlayerId);
                    Witch.SendRPC(true, witch.Key, target.PlayerId);
                    removedAnyCurse = true;
                }
            }

            // Remove Hex Master hexes
            foreach (var hexMaster in HexMaster.HexedPlayer)
            {
                if (hexMaster.Value.Contains(target.PlayerId))
                {
                    hexMaster.Value.Remove(target.PlayerId);
                    HexMaster.SendRPC(hexMaster.Key, target.PlayerId);
                    removedAnyCurse = true;
                }
            }

            if (removedAnyCurse)
            {
                player.RpcRemoveAbilityUse();
                voter.Notify(string.Format(GetString("ForensicCurseRemoved"), target.GetRealName(), player.GetAbilityUseLimit()));
            }
        }

    }


    public override string GetSuffix(PlayerControl seer, PlayerControl seen, bool isForMeeting = false)
    {
        if (!ShowArrows.GetBool() || isForMeeting || seer.PlayerId != seen.PlayerId) return string.Empty;

        return Utils.ColorString(Color.white, LocateArrow.GetArrows(seer));
    }

    public override void SetAbilityButtonText(HudManager hud, byte playerId)
    {
        hud.AbilityButton.buttonLabelText.text = GetString("ForensicVitalsText");
    }

}
