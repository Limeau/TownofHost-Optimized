using System.Collections.Generic;
using System.Linq;
using TOHO.Modules;
using UnityEngine;
using static TOHO.Options;
using static TOHO.Translator;

namespace TOHO.Roles.Crewmate;

internal class Visitor : RoleBase
{
    //===========================SETUP================================\\
    public override CustomRoles Role => CustomRoles.Visitor;
    private const int Id = 44700;
    public override CustomRoles ThisRoleBase => CustomRoles.Crewmate;
    public override Custom_RoleType ThisRoleType => Custom_RoleType.CrewmateInvestigative;
	public override bool TOHORole => true;
	public override bool NewRole => true;
	
	public override string CodedRole => ".angel24.";
    public override string IdeaRole => "the_little_pelican";
    //==================================================================\\
	
    private static OptionItem VisitAmount;
    private static OptionItem VisitType;
    private static OptionItem CanKnowKillerIdentity;
    private static OptionItem WarnKillerAboutVisitor;
    private static OptionItem Reveal;

    private static readonly Color RoleColor = Utils.GetRoleColor(CustomRoles.Visitor);

    private static string RoleTitle => Utils.ColorString(RoleColor, GetString("VisitorVisitLogs447"));

    private static readonly Dictionary<string, string> RoomColors = new()
	{
		// Shared across multiple maps
		{ "Admin", "FFA500" },
		{ "Cafeteria", "00AAFF" },
		{ "Communications", "AA00FF" },
		{ "Electrical", "FFFF00" },
		{ "MedBay", "FF69B4" },
		{ "Reactor", "FF0000" },
		{ "Security", "808080" },
		{ "Storage", "8B4513" },
		{ "O2", "00FFFF" },
		{ "Weapons", "FF4500" },
		{ "Decontamination", "7FFFD4" },
		{ "Laboratory", "9ACD32" },

		// The Skeld only
		{ "Navigation", "1E90FF" },
		{ "Shields", "00CED1" },
		{ "UpperEngine", "DC143C" },
		{ "LowerEngine", "B22222" },

		// MIRA HQ only
		{ "Balcony", "FFD700" },
		{ "Launchpad", "FF8C00" },
		{ "Records", "6A5ACD" },
		{ "Office", "20B2AA" },
		{ "Greenhouse", "228B22" },

		// Polus only
		{ "Boiler Room", "8B0000" },
		{ "Specimen Room", "556B2F" },
		{ "Outside", "708090" },

		// The Airship only
		{ "Main Hall", "4682B4" },
		{ "Meeting Room", "9932CC" },
		{ "Cockpit", "483D8B" },
		{ "Viewing Deck", "40E0D0" },
		{ "Kitchen", "CD853F" },
		{ "Cargo Bay", "A0522D" },
		{ "Armory", "696969" },
		{ "Vault", "DAA520" },
		{ "Brig", "800000" },
	};

    private static Color32 HexToColor(string hex)
    {
        if (!hex.StartsWith('#')) hex = "#" + hex;
        return ColorUtility.TryParseHtmlString(hex, out Color color) ? color : Color.white;
    }

    private static string ColorizeRoom(string roomName)
        => RoomColors.TryGetValue(roomName, out var hex) ? Utils.ColorString(HexToColor(hex), roomName) : roomName;

    public override void SetupCustomOption()
    {
        SetupSingleRoleOptions(Id, TabGroup.CrewmateRoles, CustomRoles.Visitor);

        VisitAmount = IntegerOptionItem.Create(Id + 10, "VisitorVisitAmount447", new(1, 5, 1), 1, TabGroup.CrewmateRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Visitor]);

        VisitType = StringOptionItem.Create(Id + 11, "VisitorVisitType447", new string[] { "VisitorVisitByVote447", "VisitorVisitbyCommand447" }, 0, TabGroup.CrewmateRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Visitor]);

        CanKnowKillerIdentity = BooleanOptionItem.Create(Id + 12, "VisitorCanKnowKillerIdentity447", false, TabGroup.CrewmateRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Visitor]);

        WarnKillerAboutVisitor = BooleanOptionItem.Create(Id + 13, "VisitorWarnKillerAboutVisitor447", false, TabGroup.CrewmateRoles, false)
            .SetParent(CanKnowKillerIdentity);

        Reveal = BooleanOptionItem.Create(Id + 14, "VisitorReveal447", false, TabGroup.CrewmateRoles, false)
            .SetParent(CanKnowKillerIdentity);
    }

    private static readonly Dictionary<byte, List<byte>> CurrentTargets = [];
    private static readonly Dictionary<byte, List<string>> VisitedRooms = [];
    private static readonly Dictionary<byte, string> LastRoom = [];
    private static readonly Dictionary<byte, int> TasksAtVisitStart = [];
    private static readonly Dictionary<byte, int> KillsAtVisitStart = [];
    private static readonly Dictionary<byte, int> TotalKillCount = [];
    private static readonly Dictionary<byte, (byte KillerId, CustomRoles KillerRole, string KillerModifier)> VisitKillInfo = [];
    private static readonly Dictionary<byte, List<string>> LastReport = [];
    private static readonly HashSet<byte> RevealedAsVisitor = [];
	private static readonly HashSet<byte> RevealedAsKiller = [];

    public override void Init()
    {
        CurrentTargets.Clear();
        VisitedRooms.Clear();
        LastRoom.Clear();
        TasksAtVisitStart.Clear();
        KillsAtVisitStart.Clear();
        TotalKillCount.Clear();
        VisitKillInfo.Clear();
        LastReport.Clear();
        RevealedAsVisitor.Clear();
		RevealedAsKiller.Clear();
    }

    public override void Add(byte playerId)
	{
		CurrentTargets[playerId] = [];
		playerId.SetAbilityUseLimit(VisitAmount.GetInt());
	}

    public override void Remove(byte playerId)
    {
        CurrentTargets.Remove(playerId);
    }

	public override string GetSuffix(PlayerControl seer, PlayerControl target = null, bool isForMeeting = false)
	{
		if (isForMeeting) return string.Empty;
		if (target != null && seer.PlayerId != target.PlayerId) return string.Empty;
		if (!RevealedAsVisitor.Contains(seer.PlayerId)) return string.Empty;

		return Utils.ColorString(RoleColor, GetString("VisitorVisitorTag447"));
	}

    public override string GetSuffixOthers(PlayerControl seer, PlayerControl target, bool isForMeeting = false)
    {
        if (isForMeeting) return string.Empty;
        if (target != null && seer.PlayerId != target.PlayerId) return string.Empty;
        if (!RevealedAsKiller.Contains(seer.PlayerId)) return string.Empty;

        return Utils.ColorString(RoleColor, GetString("VisitorKillerTag447"));
    }

    public override string GetProgressText(byte playerId, bool comms)
    {
        var taskState = Main.PlayerStates?[playerId].TaskState;
        var normalColor = taskState.IsTaskFinished ? Color.green : Color.yellow;
        var textColor = comms ? Color.gray : normalColor;
        string completed = comms ? "?" : $"{taskState.CompletedTasksCount}";

        var progressText = new System.Text.StringBuilder();
        progressText.Append(Utils.ColorString(textColor, $"({completed}/{taskState.AllTasksCount})"));

        int remaining = (int)playerId.GetAbilityUseLimit();
        var remainingColor = remaining < 1 ? Color.red : Color.white;
        progressText.Append(Utils.ColorString(remainingColor, $" <color=#ffffff>-</color> {remaining}"));

        return progressText.ToString();
    }

    public override void OnFixedUpdate(PlayerControl player, bool lowLoad, long nowTime, int timerLowLoad)
    {
        if (!player.Is(CustomRoles.Visitor)) return;
        if (GameStates.IsMeeting) return;
        if (!CurrentTargets.TryGetValue(player.PlayerId, out var targets) || targets.Count == 0) return;

        foreach (var targetId in targets)
        {
            var target = Utils.GetPlayerById(targetId);
            if (target == null || target.Data.IsDead) continue;

            var room = target.GetPlainShipRoom();
            if (room == null) continue;

            string roomName = GetString(room.RoomId.ToString());

            if (!LastRoom.TryGetValue(targetId, out var last) || last != roomName)
            {
                LastRoom[targetId] = roomName;
                if (!VisitedRooms.ContainsKey(targetId)) VisitedRooms[targetId] = [];
                VisitedRooms[targetId].Add(roomName);
            }
        }
    }

    public override bool CheckMurderOnOthersTarget(PlayerControl killer, PlayerControl target)
    {
        OnGlobalMurder(killer, target);
        return false;
    }

    public static void OnGlobalMurder(PlayerControl killer, PlayerControl target)
	{
		if (target == null) return;

		if (killer != null)
		{
			TotalKillCount.TryAdd(killer.PlayerId, 0);
			TotalKillCount[killer.PlayerId]++;
		}

		foreach (var kvp in CurrentTargets)
		{
			byte visitorId = kvp.Key;

			if (killer == null || !kvp.Value.Contains(target.PlayerId))
				continue;

			VisitKillInfo[target.PlayerId] =
			(
				killer.PlayerId,
				killer.GetCustomRole(),
				GetKillerModifierInfo(killer.PlayerId)
			);

			if (CanKnowKillerIdentity.GetBool())
			{
				if (Reveal.GetBool())
				{
					RevealedAsVisitor.Add(visitorId);
					RevealedAsKiller.Add(killer.PlayerId);
				}

				var visitor = Utils.GetPlayerById(visitorId);

				if (visitor != null)
				{
					string killerName = killer.GetRealName();
					string killerRole = GetString($"{killer.GetCustomRole()}");

					Utils.SendMessage(
						string.Format(
							GetString("VisitorKillerReveal447"),
							target.GetRealName(),
							killerName,
							killerRole,
							GetKillerModifierInfo(killer.PlayerId)),
						visitor.PlayerId,
						title: RoleTitle
					);
				}

				if (WarnKillerAboutVisitor.GetBool() && visitor != null)
				{
					Utils.SendMessage(
						string.Format(
							GetString("VisitorWarnKiller447"),
							visitor.GetRealName()),
						killer.PlayerId,
						title: RoleTitle
					);
				}

				Utils.NotifyRoles();
			}
		}
	}

    private static string GetKillerModifierInfo(byte killerId)
    {
        var subRoles = Main.PlayerStates[killerId].SubRoles;
        if (subRoles == null || subRoles.Count == 0) return "";

        return string.Join(", ", subRoles.ToArray().Select(sr => GetString(sr.ToString())));
    }

	public override bool CheckVote(PlayerControl player, PlayerControl target)
	{
		if (!player.Is(CustomRoles.Visitor)) return true;
		if (VisitType.GetValue() != 0) return true;
		if (target == null) return true;

		if (!CurrentTargets.TryGetValue(player.PlayerId, out var list)) list = CurrentTargets[player.PlayerId] = [];

		if (player.GetAbilityUseLimit() > 0 && !list.Contains(target.PlayerId))
		{
			RegisterVisit(player, target);
			Utils.SendMessage(string.Format(GetString("VisitorTargetSet447"), target.GetRealName()), player.PlayerId, title: RoleTitle);
			Utils.SendMessage(GetString("VoteHasReturned"), player.PlayerId, title: RoleTitle);
			return false;
		}

		return true;
	}

    private static void RegisterVisit(PlayerControl visitor, PlayerControl target)
    {
        if (!CurrentTargets.ContainsKey(visitor.PlayerId)) CurrentTargets[visitor.PlayerId] = [];

        var list = CurrentTargets[visitor.PlayerId];
        if (list.Contains(target.PlayerId)) return;

        list.Add(target.PlayerId);
        TasksAtVisitStart[target.PlayerId] = target.GetPlayerTaskState().CompletedTasksCount;
        KillsAtVisitStart[target.PlayerId] = TotalKillCount.TryGetValue(target.PlayerId, out var kc) ? kc : 0;
        visitor.RpcRemoveAbilityUse();
    }

    public bool VisitMsgCheck(PlayerControl player, string text)
    {
        if (player == null || !player.Is(CustomRoles.Visitor) || !player.IsAlive()) return false;

        var args = text.Trim().Split(' ');

        if (args[0] == "/visits")
        {
            if (LastReport.TryGetValue(player.PlayerId, out var lines) && lines.Count > 0)
            {
                foreach (var line in lines)
                    Utils.SendMessage(line, player.PlayerId, title: RoleTitle);
            }
            else
            {
                Utils.SendMessage(GetString("VisitorNoReportYet447"), player.PlayerId, title: RoleTitle);
            }
            return true;
        }

        if (VisitType.GetValue() != 1) return false;
        if (!GameStates.IsMeeting) return false;
        if (args.Length < 2 || args[0] != "/visit") return false;

        if (!int.TryParse(args[1], out int id))
        {
            Utils.SendMessage(GetString("VisitorInvalidId447"), player.PlayerId, title: RoleTitle);
            return true;
        }

        var target = Utils.GetPlayerById(id);
        if (target == null || target.PlayerId == player.PlayerId || target.Data.IsDead)
        {
            Utils.SendMessage(GetString("VisitorInvalidTarget447"), player.PlayerId, title: RoleTitle);
            return true;
        }

        var list = CurrentTargets.TryGetValue(player.PlayerId, out var existing) ? existing : [];
        if (list.Contains(target.PlayerId))
        {
            Utils.SendMessage(GetString("VisitorAlreadyTargeted447"), player.PlayerId, title: RoleTitle);
            return true;
        }
        if (player.GetAbilityUseLimit() < 1)
        {
            Utils.SendMessage(GetString("VisitorMaxTargetsReached447"), player.PlayerId, title: RoleTitle);
            return true;
        }

        RegisterVisit(player, target);
        Utils.SendMessage(string.Format(GetString("VisitorTargetSet447"), target.GetRealName()), player.PlayerId, title: RoleTitle);
        return true;
    }

    public override void OnMeetingHudStart(PlayerControl pc)
    {
        if (!pc.Is(CustomRoles.Visitor)) return;
        if (!CurrentTargets.TryGetValue(pc.PlayerId, out var targets) || targets.Count == 0)
			return;

        var reportLines = new List<string>();

        foreach (var targetId in targets)
        {
            var target = Utils.GetPlayerById(targetId);
            string targetName = target != null ? target.GetRealName() : GetString("VisitorUnknownPlayer447");

            var rooms = VisitedRooms.TryGetValue(targetId, out var r) ? r : [];
            string roomsStr = rooms.Count > 0 ? string.Join(", ", rooms.Select(ColorizeRoom)) : GetString("VisitorNoRoomsRecorded447");

            int tasksNow = target != null ? target.GetPlayerTaskState().CompletedTasksCount : 0;
            int tasksStart = TasksAtVisitStart.TryGetValue(targetId, out var ts) ? ts : tasksNow;
            int tasksCompleted = System.Math.Max(0, tasksNow - tasksStart);

            int killsNow = TotalKillCount.TryGetValue(targetId, out var kn) ? kn : 0;
            int killsStart = KillsAtVisitStart.TryGetValue(targetId, out var ks) ? ks : killsNow;
            int killsCompleted = System.Math.Max(0, killsNow - killsStart);

            string reportLine = string.Format(GetString("VisitorReport447"), targetName, roomsStr, tasksCompleted, killsCompleted);
            Utils.SendMessage(reportLine, pc.PlayerId, title: RoleTitle);
            reportLines.Add(reportLine);

            if (VisitKillInfo.TryGetValue(targetId, out var killInfo) && CanKnowKillerIdentity.GetBool())
            {
                string killerName = Utils.GetPlayerById(killInfo.KillerId)?.GetRealName() ?? GetString("VisitorUnknownPlayer447");
                string killerRoleName = GetString($"{killInfo.KillerRole}");

                string killerLine = string.Format(GetString("VisitorKillerReveal447"), targetName, killerName, killerRoleName, killInfo.KillerModifier);
                Utils.SendMessage(killerLine, pc.PlayerId, title: RoleTitle);
                reportLines.Add(killerLine);

                if (WarnKillerAboutVisitor.GetBool())
                {
                    Utils.SendMessage(string.Format(GetString("VisitorWarnKiller447"), pc.GetRealName()), killInfo.KillerId, title: RoleTitle);
                }

                if (Reveal.GetBool())
				{
					RevealedAsVisitor.Add(pc.PlayerId);
					RevealedAsKiller.Add(killInfo.KillerId);
				}
            }
        }

        LastReport[pc.PlayerId] = reportLines;

		CurrentTargets[pc.PlayerId].Clear();

		VisitedRooms.Clear();
		LastRoom.Clear();
		VisitKillInfo.Clear();
    }
}