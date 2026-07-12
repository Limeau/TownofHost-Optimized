using System.Collections.Generic;
using AmongUs.GameOptions;
using Hazel;
using UnityEngine;
using TOHO.Modules;
using TOHO.Roles.Core;
using static TOHO.Options;
using static TOHO.Translator;

namespace TOHO.Roles.Crewmate;

internal class Empath : RoleBase
{
    //===========================SETUP================================\\
    public override CustomRoles Role => CustomRoles.Empath;
    private const int Id = 45500;
    public override CustomRoles ThisRoleBase => CustomRoles.Shapeshifter;
    public override Custom_RoleType ThisRoleType => Custom_RoleType.CrewmateSupport;
	
	public override bool TOHORole => true;
	public override bool NewRole => true;
	
	public override string CodedRole => ".angel24.";
    public override string IdeaRole => ".angel24.";
    //==================================================================\\

    private static OptionItem SenseCooldown;
    private static OptionItem BurdenMultiplier;
	
	public static bool HasBurden(byte playerId)
	{
		return BurdenedKillers.Contains(playerId);
	}

	public static float GetBurdenMultiplier()
	{
		return BurdenMultiplier.GetFloat();
	}

    private static readonly Dictionary<byte, byte> SelectedTarget = [];

    private static readonly HashSet<byte> BurdenedKillers = [];

    private static readonly Dictionary<byte, float> LastSeenTimer = [];

    public override void SetupCustomOption()
    {
        SetupRoleOptions(Id, TabGroup.CrewmateRoles, CustomRoles.Empath);

        SenseCooldown = FloatOptionItem.Create(Id + 10, "EmpathSenseCooldown", new(2.5f, 60f, 2.5f), 20f, TabGroup.CrewmateRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Empath])
            .SetValueFormat(OptionFormat.Seconds);

        BurdenMultiplier = FloatOptionItem.Create(Id + 11, "EmpathBurdenCooldownMultiplier", new(1f, 5f, 0.5f), 2f, TabGroup.CrewmateRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Empath])
            .SetValueFormat(OptionFormat.Multiplier);
    }

    public override void Add(byte playerId)
    {
        if (AmongUsClient.Instance.AmHost)
        {
        }
    }

    public override void Init()
    {
        SelectedTarget.Clear();
        BurdenedKillers.Clear();
        LastSeenTimer.Clear();
    }

    public override void Remove(byte playerId)
    {
        SelectedTarget.Remove(playerId);
    }

    public override void ApplyGameOptions(IGameOptions opt, byte playerId)
    {
        AURoleOptions.ShapeshifterCooldown = 1f;
        AURoleOptions.ShapeshifterDuration = 1f;
        opt.SetVision(false);
    }

    public override bool OnCheckShapeshift(PlayerControl empath, PlayerControl target, ref bool resetCooldown, ref bool shouldAnimate)
    {
        shouldAnimate = false;
        resetCooldown = true;

        if (empath.PlayerId == target.PlayerId) return false;

        SelectedTarget[empath.PlayerId] = target.PlayerId;
        empath.Notify(string.Format(GetString("EmpathTargetSelected"), target.GetRealName()), time: 2.5f);

        return false;
    }

    public override bool CanUseKillButton(PlayerControl pc) => pc.IsAlive();

    public override void SetKillCooldown(byte id)
	{
		float cooldown = SenseCooldown.GetFloat();

		if (BurdenedKillers.Contains(id))
			cooldown *= BurdenMultiplier.GetFloat();

		Main.AllPlayerKillCooldown[id] = cooldown;
	}

    public override void SetAbilityButtonText(HudManager hud, byte playerId)
    {
        hud.KillButton.OverrideText(GetString("EmpathAbilitySense"));
		hud.AbilityButton.OverrideText(GetString("EmpathAbilityChoose"));
    }

    public override Sprite GetKillButtonSprite(PlayerControl player, bool shapeshifting) => CustomButton.Get("Examine");

    public override bool ForcedCheckMurderAsKiller(PlayerControl killer, PlayerControl target)
    {
        if (killer == null) return false;

        if (!SelectedTarget.TryGetValue(killer.PlayerId, out var targetId))
        {
            killer.Notify(GetString("EmpathNoTargetSelected"), time: 2.5f);
            return false;
        }

        var sensed = Utils.GetPlayerById(targetId);
        if (sensed == null || !sensed.IsAlive())
        {
            killer.Notify(GetString("EmpathTargetUnavailable"), time: 2.5f);
            return false;
        }

        string reading =
            Main.AllKillers.ContainsKey(sensed.PlayerId) ? GetString("EmpathReadingGuilty") :
            sensed.Is(Custom_Team.Impostor) ? GetString("EmpathReadingAgitated") :
            GetString("EmpathReadingCalm");

        killer.Notify(string.Format(GetString("EmpathSenseResult"), sensed.GetRealName(), reading), time: 4f);

        killer.MarkDirtySettings();
		killer.SetKillCooldown();

        return false;
    }

    public override void OnMurderPlayerAsTarget(PlayerControl killer, PlayerControl target, bool inMeeting, bool isSuicide)
	{
		if (isSuicide || killer == null)
			return;

		if (BurdenedKillers.Add(killer.PlayerId))
		{
			killer.Notify(GetString("EmpathBurdenApplied"), time: 4f);

			// Recalculate their cooldown immediately.
			killer.ResetKillCooldown();
			killer.SetKillCooldown();
			killer.MarkDirtySettings();
		}
	}
}