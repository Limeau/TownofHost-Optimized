@ -0,0 +1,174 @@
using System.Collections.Generic;
using AmongUs.GameOptions;
using Hazel;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using static TOHO.Options;

namespace TOHO.Roles.Modifiers.Crewmate;

public class Rage : IModifier
{
    public CustomRoles Role => CustomRoles.Rage;
    private const int Id = 44900;
    public ModifierTypes Type => ModifierTypes.Mixed;

    private static OptionItem ExtraTasks;
    private static OptionItem KillCooldown;

    // Public: read by CustomRolesHelper.CheckModifierConfilct to decide whether
    // task-reliant-ability roles (Merchant, Alchemist, etc.) are allowed to roll Rage.
    public static OptionItem RageAffectTaskRole;

    private static OptionItem AllowShortTasks;
    private static OptionItem AllowCommonTasks;
    private static OptionItem AllowLongTasks;

    // Players who have finished ALL of their tasks (base + Rage's extra) and have
    // therefore earned a kill button. Cleared on Init/Remove.
    private static readonly HashSet<byte> RageKillers = [];

    public void SetupCustomOption()
    {
        SetupAdtRoleOptions(Id, CustomRoles.Rage, canSetNum: true);

        ExtraTasks = IntegerOptionItem.Create(Id + 10, "RageExtraTasks", new(1, 10, 1), 4, TabGroup.Modifiers, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Rage])
            .SetValueFormat(OptionFormat.Pieces);

        KillCooldown = IntegerOptionItem.Create(Id + 11, "RageKillCooldown", new(10, 60, 5), 15, TabGroup.Modifiers, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Rage])
            .SetValueFormat(OptionFormat.Seconds);

        RageAffectTaskRole = BooleanOptionItem.Create(Id + 12, "RageAffectTaskRole", false, TabGroup.Modifiers, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Rage]);

        // NOTE: defaulted to true/true/false (not all-false like the stub) so the
        // modifier actually has a task pool to pull extra tasks from out of the box.
        AllowShortTasks = BooleanOptionItem.Create(Id + 13, "RageAllowShortTasks", true, TabGroup.Modifiers, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Rage]);
        AllowCommonTasks = BooleanOptionItem.Create(Id + 14, "RageAllowCommonTasks", true, TabGroup.Modifiers, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Rage]);
        AllowLongTasks = BooleanOptionItem.Create(Id + 15, "RageAllowLongTasks", false, TabGroup.Modifiers, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Rage]);
    }

    public void Init()
    {
        RageKillers.Clear();
    }

    public void Add(byte playerId, bool gameIsLoading = true)
    {
        // Modifiers are attached to the player AFTER the game's own initial task
        // assignment already ran (ModifierAssign runs after the vanilla RpcSetRole
        // flow), so the very first RpcSetTasksPatch pass never saw pc.Is(Rage) as
        // true and never added the extra tasks. Forcing a redistribution here -
        // the same trick Workhorse uses - makes RpcSetTasksPatch run again, and
        // this time it correctly picks up ApplyExtraTasks().
        if (!AmongUsClient.Instance.AmHost) return;

        var pc = playerId.GetPlayer();
        if (pc?.Data == null) return;

        pc.Data.RpcSetTasks(new Il2CppStructArray<byte>(0));
    }

    public void Remove(byte playerId)
    {
        RageKillers.Remove(playerId);
    }

    /// <summary>
    /// Called from RpcSetTasksPatch (TaskAssignPatch.cs) after the base role/other
    /// modifiers have already decided this player's task counts. Adds Rage's extra
    /// tasks on top, split across whichever pools the host has enabled.
    /// Common tasks are all-or-nothing at the engine level, so RageAllowCommonTasks
    /// just gates whether this player keeps the standard common-task set at all.
    /// </summary>
    public static (bool hasCommonTasks, int numLongTasks, int numShortTasks) ApplyExtraTasks(bool hasCommonTasks, int numLongTasks, int numShortTasks)
    {
        if (!AllowCommonTasks.GetBool())
            hasCommonTasks = false;

        var extra = ExtraTasks.GetInt();
        var allowShort = AllowShortTasks.GetBool();
        var allowLong = AllowLongTasks.GetBool();

        if (allowShort && allowLong)
        {
            var half = extra / 2;
            numShortTasks += half + (extra % 2); // odd leftover goes to short tasks
            numLongTasks += half;
        }
        else if (allowShort)
        {
            numShortTasks += extra;
        }
        else if (allowLong)
        {
            numLongTasks += extra;
        }
        // If the host disabled both pools, there's nowhere to put the extra tasks -
        // that's a host misconfiguration, not something to silently patch around.

        return (hasCommonTasks, numLongTasks, numShortTasks);
    }

    public static bool HasRageKill(byte playerId) => RageKillers.Contains(playerId);

    public static float GetKillCooldown() => KillCooldown.GetInt();

    /// <summary>
    /// Hooked from PlayerControlCompleteTaskPatch (PlayerControlPatch.cs), only when
    /// taskState.CompletedTasksCount >= taskState.AllTasksCount - i.e. every task,
    /// including Rage's extras, is done. Grants the kill button.
    /// </summary>
    public static void OnTaskComplete(PlayerControl player)
    {
        if (RageKillers.Contains(player.PlayerId)) return;

        GrantKill(player.PlayerId);

        Main.AllPlayerKillCooldown[player.PlayerId] = KillCooldown.GetInt();

        // Non-modded clients only see a kill button if their own client's real vanilla
        // RoleTypes says Impostor/Shapeshifter - HudPatch.cs (which drives the button off
        // CanUseKillButton()) only runs for players with the mod installed. Sending a
        // self-only desync (visible to nobody else) is the same trick Corrupted.cs uses
        // when it mid-game-converts a Crewmate into a real Impostor.
        // NOTE: this will also visually show vent/sabotage icons on their own client -
        // those stay correctly blocked server-side (RoleBase.CanUseImpostorVentButton /
        // CanUseSabotage both gate on Custom_Team.Impostor, which Rage never changes) but
        // it's a cosmetic wrinkle worth testing for.
        player.RpcSetRoleDesync(RoleTypes.Impostor, player.GetClientId());

        player.Notify(Utils.ColorString(Utils.GetRoleColor(CustomRoles.Rage), Translator.GetString("RageKillButtonGranted")));

        // RageKillers is static state that only ever gets updated here, and this whole
        // method only ever runs on the HOST (PlayerControlCompleteTaskPatch is wrapped in
        // an AmHost check). HudPatch.cs, which shows/hides the kill button, always
        // evaluates CanUseKillButton() against PlayerControl.LocalPlayer - i.e. every
        // client independently checks its OWN local copy of RageKillers. That means a
        // modded client who ISN'T the host would never see their own entry get added,
        // and their own kill button would never show. Broadcasting this RPC to the
        // affected player's own client (host excluded - it already has this locally)
        // fixes that.
        if (!player.IsHost())
        {
            var writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.RageKillGranted, SendOption.Reliable, player.GetClientId());
            writer.Write(player.PlayerId);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }
    }

    /// <summary>
    /// Handles the incoming RageKillGranted RPC on the receiving client - just mirrors
    /// the RageKillers state locally so that client's own CanUseKillButton() check
    /// (evaluated only for PlayerControl.LocalPlayer) sees it too. Cooldown and the
    /// vanilla role desync are already delivered through their own RPCs, so this only
    /// needs to update the one piece of state HudPatch actually reads locally.
    /// </summary>
    public static void ReceiveKillGranted(byte playerId) => GrantKill(playerId);

    private static void GrantKill(byte playerId) => RageKillers.Add(playerId);
}