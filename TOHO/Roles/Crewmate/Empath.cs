using System.Collections.Generic;
using AmongUs.GameOptions;
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
    private static OptionItem BloodlustMultiplier;

    // <EmpathId, chosen target PlayerId> - set by the shapeshift panel, read by the kill button.
    private static readonly Dictionary<byte, byte> SelectedTarget = [];

    // Killers who have been cursed by dying to an Empath. Persists for the rest of
    // the game regardless of whether the Empath who applied it is still alive.
    private static readonly HashSet<byte> BloodlustedKillers = [];

    // Used by EnforceBloodlust to detect "this player's cooldown was just refreshed
    // by a new kill" so we only double it once per kill, not every tick.
    private static readonly Dictionary<byte, float> LastSeenTimer = [];

    public override void SetupCustomOption()
    {
        SetupRoleOptions(Id, TabGroup.CrewmateRoles, CustomRoles.Empath);

        SenseCooldown = FloatOptionItem.Create(Id + 10, "EmpathSenseCooldown", new(2.5f, 60f, 2.5f), 20f, TabGroup.CrewmateRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Empath])
            .SetValueFormat(OptionFormat.Seconds);

        BloodlustMultiplier = FloatOptionItem.Create(Id + 11, "EmpathBloodlustCooldownMultiplier", new(1f, 5f, 0.5f), 2f, TabGroup.CrewmateRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Empath])
            .SetValueFormat(OptionFormat.Multiplier);
    }

    public override void Add(byte playerId)
    {
        // Static/idempotent - HashSet.Add just no-ops if this is already registered
        // from a previous round, same pattern Witness uses for its own tick hook.
        if (AmongUsClient.Instance.AmHost)
        {
            CustomRoleManager.OnFixedUpdateOthers.Add(EnforceBloodlust);
        }
    }

    public override void Init()
    {
        SelectedTarget.Clear();
        BloodlustedKillers.Clear();
        LastSeenTimer.Clear();
    }

    public override void Remove(byte playerId)
    {
        SelectedTarget.Remove(playerId);
    }

    public override void ApplyGameOptions(IGameOptions opt, byte playerId)
    {
        // The shift panel is only ever used as a picker - it never actually
        // transforms - so keep its own cooldown/duration negligible. The real
        // Sense Intent cooldown lives on the kill button via SetKillCooldown below.
        AURoleOptions.ShapeshifterCooldown = 1f;
        AURoleOptions.ShapeshifterDuration = 1f;
        opt.SetVision(false);
    }

    // === Target selection: shapeshift panel as picker, no transform/animation ===
    public override bool OnCheckShapeshift(PlayerControl empath, PlayerControl target, ref bool resetCooldown, ref bool shouldAnimate)
    {
        shouldAnimate = false;
        resetCooldown = true; // picking/re-picking a target should feel instant, not gated

        if (empath.PlayerId == target.PlayerId) return false;

        SelectedTarget[empath.PlayerId] = target.PlayerId;
        empath.Notify(string.Format(GetString("EmpathTargetSelected"), target.GetRealName()), time: 2.5f);

        // Always reject the actual shapeshift - CanDesyncShapeshift is left at its
        // default (false), so this is a *global* reject: nobody, including the
        // Empath's own client, ever sees a shapeshift animation or skin change.
        return false;
    }

    // === Activation: kill button fires Sense Intent on the selected target ===
    public override bool CanUseKillButton(PlayerControl pc) => pc.IsAlive();

    public override void SetKillCooldown(byte id) => Main.AllPlayerKillCooldown[id] = SenseCooldown.GetFloat();

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

        // "Guilty" takes priority over "Agitated" - a killer Impostor who just
        // killed reads as Guilty, not Agitated. This is a deliberate design
        // choice, not a hard rule from the spec - flip the order if you'd rather
        // team always win over recent-kill state.
        string reading =
            Main.AllKillers.ContainsKey(sensed.PlayerId) ? GetString("EmpathReadingGuilty") :
            sensed.Is(Custom_Team.Impostor) ? GetString("EmpathReadingAgitated") :
            GetString("EmpathReadingCalm");

        killer.Notify(string.Format(GetString("EmpathSenseResult"), sensed.GetRealName(), reading), time: 4f);

        killer.SetKillCooldown();

        return false; // never an actual murder
    }

    // === Emotional Bond passive: curse whoever kills the Empath ===
    public override void OnMurderPlayerAsTarget(PlayerControl killer, PlayerControl target, bool inMeeting, bool isSuicide)
    {
        if (isSuicide || killer == null) return;

        if (BloodlustedKillers.Add(killer.PlayerId))
        {
            killer.Notify(GetString("EmpathBloodlustApplied"), time: 4f);
        }

        // SetKillTimer() for THIS kill already ran earlier in the same
        // murder-resolution call, so KillTimerManager.AllKillTimers already holds
        // the fresh, undoubled base value here. Double it directly rather than
        // relying on EnforceBloodlust's next-tick jump detection, which would
        // otherwise miss this very first kill (there's no prior LastSeenTimer
        // baseline yet to compare against).
        if (KillTimerManager.AllKillTimers.TryGetValue(killer.PlayerId, out var current))
        {
            current *= BloodlustMultiplier.GetFloat();
            KillTimerManager.AllKillTimers[killer.PlayerId] = current;

            // Keep EnforceBloodlust's baseline in sync so it doesn't see this same
            // value next tick and think it's ANOTHER fresh kill to double again.
            LastSeenTimer[killer.PlayerId] = current;
        }
    }

    // Registered once into CustomRoleManager.OnFixedUpdateOthers, so this ticks
    // for every player every frame regardless of their own role class - the only
    // way to keep affecting a killer's cooldown after the Empath who cursed them
    // is dead, since RoleBase hooks are otherwise scoped to your own role instance.
    //
    // Important: Main.AllPlayerKillCooldown is just the CONFIGURED cooldown length
    // for a player's *next* kill (whatever their own role's SetKillCooldown(byte)
    // override set it to) - it isn't a live countdown, and the killer's own
    // SetKillCooldown call overwrites it fresh on every kill regardless of what we
    // write there, so doubling it does nothing. The actual enforced, ticking timer
    // lives in KillTimerManager.AllKillTimers, refreshed via SetKillTimer() right
    // when a kill resolves (PlayerControlPatch.cs) and decremented every
    // FixedUpdate from there. That's the dictionary we need to double.
    private static void EnforceBloodlust(PlayerControl player, bool lowLoad, long nowTime)
    {
        if (!BloodlustedKillers.Contains(player.PlayerId)) return;
        if (!KillTimerManager.AllKillTimers.TryGetValue(player.PlayerId, out var current)) return;

        // A fresh kill just refreshed this player's live timer back up from
        // near-zero - double it exactly once here rather than every tick while it
        // counts down.
        if (LastSeenTimer.TryGetValue(player.PlayerId, out var last) && current > last + 1f)
        {
            current *= BloodlustMultiplier.GetFloat();
            KillTimerManager.AllKillTimers[player.PlayerId] = current;
        }

        LastSeenTimer[player.PlayerId] = current;
    }
}