using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using TOHO.Roles.Core;
using static TOHO.Options;
using static TOHO.Translator;
using static TOHO.Utils;

namespace TOHO.Roles.Coven;

internal class Harvester : CovenManager
{
    //===========================SETUP================================\\
    public override CustomRoles Role => CustomRoles.Harvester;
    private const int Id = 31600;
    public override bool IsDesyncRole => true;
    public override CustomRoles ThisRoleBase => CustomRoles.Shapeshifter;
    public override Custom_RoleType ThisRoleType => Custom_RoleType.CovenUtility;
    //==================================================================\\

    private static OptionItem KillCooldown;
    private static OptionItem SwapCooldown;
    private static OptionItem AmountStolen;
    private static OptionItem MaxModifiersCoven;
    private static OptionItem MaxModifiersSelf;

    private static readonly Dictionary<byte, List<byte>> SwapPlayers = [];

    public override void SetupCustomOption()
    {
        SetupSingleRoleOptions(Id, TabGroup.CovenRoles, CustomRoles.Harvester, 1, zeroOne: false);
        KillCooldown = FloatOptionItem.Create(Id + 10, GeneralOption.KillCooldown, new(0f, 180f, 2.5f), 30f, TabGroup.CovenRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.Harvester])
            .SetValueFormat(OptionFormat.Seconds);
        SwapCooldown = FloatOptionItem.Create(Id + 11, "HarvesterSettings.SwapCooldown", new(0f, 180f, 2.5f), 30f, TabGroup.CovenRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.Harvester])
            .SetValueFormat(OptionFormat.Seconds);
        AmountStolen = IntegerOptionItem.Create(Id + 12, "HarvesterSettings.AmountStolen", new(1, 100, 1), 1, TabGroup.CovenRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.Harvester])
            .SetValueFormat(OptionFormat.Times);
        MaxModifiersCoven = IntegerOptionItem.Create(Id + 13, "HarvesterSettings.MaxModifiersCoven", new(1, 100, 1), 5, TabGroup.CovenRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.Harvester])
            .SetValueFormat(OptionFormat.Times);
        MaxModifiersSelf = IntegerOptionItem.Create(Id + 14, "HarvesterSettings.MaxModifiersSelf", new(1, 100, 1), 5, TabGroup.CovenRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.Harvester])
            .SetValueFormat(OptionFormat.Times);
    }
    public override void Init()
    {
        SwapPlayers.Clear();
        CustomRoleManager.CheckDeadBodyOthers.Add(OnPlayerDead);
    }
    public override void Add(byte playerId)
    {
        SwapPlayers[playerId] = [];
    }
    public override void SetKillCooldown(byte id) => Main.AllPlayerKillCooldown[id] = KillCooldown.GetFloat();
    public override bool CanUseKillButton(PlayerControl pc) => HasNecronomicon(pc);
    public override bool OnCheckMurderAsKiller(PlayerControl killer, PlayerControl target)
    {
        if (!CanUseKillButton(killer)) return false;
        if (HasNecronomicon(killer) && !target.GetCustomRole().IsCovenTeam())
        {
            return true;
        }
        killer.Notify(GetString("CovenDontKillOtherCoven"));
        return false;
    }
    public override bool OnCheckShapeshift(PlayerControl shapeshifter, PlayerControl target, ref bool resetCooldown, ref bool shouldAnimate)
    {
        shouldAnimate = false;
        if (SwapPlayers[shapeshifter.PlayerId].Count() >= 2) {
            shapeshifter.Notify(GetString("Harvester.SwapListFull"));
            return false; 
        }
        if (shapeshifter == null || target == null) return false;

        if (SwapPlayers[shapeshifter.PlayerId].Count() == 0)
        {
            SwapPlayers[shapeshifter.PlayerId].Add(target.PlayerId); 
            Logger.Info($"{target.GetRealName()} is SwapPlayer1", "Harvester");
        }
        else
        {
            SwapPlayers[shapeshifter.PlayerId].Add(target.PlayerId);
            Logger.Info($"{target.GetRealName()} is SwapPlayer2", "Harvester");
        }
        shapeshifter.Notify(string.Format(GetString("Harvester.PlayerAdded"), target.GetRealName()));
        return false;
    }
    private void OnPlayerDead(PlayerControl killer, PlayerControl deadPlayer, bool inMeeting)
    {
        if (!CustomRoles.Harvester.RoleExist()) return;
        if (killer == null || deadPlayer == null || deadPlayer.IsDisconnected()) return;
        var harvester = GetPlayerById(SwapPlayers.Keys.First());
        if (harvester == null) return;
        if (!harvester.IsAlive()) return;
        if (killer.IsPlayerCoven())
        {
            var stolen = 0;
            List<CustomRoles> Modifiers = new(deadPlayer.GetCustomSubRoles());
            foreach (CustomRoles Modifier in Modifiers)
            {
                if (stolen >= AmountStolen.GetInt()) break;
                if (killer.GetCustomSubRoles().Count >= MaxModifiersCoven.GetInt()) break;
                Main.PlayerStates[deadPlayer.PlayerId].RemoveSubRole(Modifier);
                killer.RpcSetCustomRole(Modifier, false, false);
                stolen++;
                Logger.Info($"{Modifier.ToString()} from {deadPlayer.GetNameWithRole()} given to {killer.GetNameWithRole()}", "Harvester");
            }
            Logger.Info($"{deadPlayer.GetNameWithRole()}'s Modifiers given to {killer.GetNameWithRole()}; {stolen} Modifiers stolen total", "Harvester");
        }
        else if (HasNecronomicon(harvester) && !killer.IsPlayerCoven())
        {
            var stolen = 0;
            List<CustomRoles> Modifiers = new(deadPlayer.GetCustomSubRoles());
            foreach (CustomRoles Modifier in Modifiers)
            {
                if (stolen > AmountStolen.GetInt()) break;
                if (harvester.GetCustomSubRoles().Count >= MaxModifiersSelf.GetInt()) break;
                Main.PlayerStates[deadPlayer.PlayerId].RemoveSubRole(Modifier);
                harvester.RpcSetCustomRole(Modifier, false, false);
                stolen++;
                Logger.Info($"{Modifier.ToString()} from {deadPlayer.GetNameWithRole()} given to {harvester.GetNameWithRole()}", "Harvester");
            }
            Logger.Info($"{deadPlayer.GetNameWithRole()}'s Modifiers given to {harvester.GetNameWithRole()}; {stolen} Modifiers stolen total", "Harvester");
        }
    }
    public override void OnReportDeadBody(PlayerControl reporter, NetworkedPlayerInfo target)
    {
        if (!CustomRoles.Harvester.RoleExist()) return;
        if (SwapPlayers[_Player.PlayerId].Count() != 2) return;
        SwapModifiers(GetPlayerById(SwapPlayers[_Player.PlayerId][0]), GetPlayerById(SwapPlayers[_Player.PlayerId][1]));
    }
    public override void AfterMeetingTasks()
    {
        SwapPlayers[_Player.PlayerId].Clear();
    }
    public override void ApplyGameOptions(IGameOptions opt, byte playerId)
    {
        AURoleOptions.ShapeshifterCooldown = SwapCooldown.GetFloat();
        AURoleOptions.ShapeshifterDuration = 1f;
        base.ApplyGameOptions(opt, playerId);
    }
    private void SwapModifiers(PlayerControl player1, PlayerControl player2)
    {
        if (SwapPlayers[_Player.PlayerId].Count() >= 2) return;
        if (player1 == null || player2 == null) return;
        List<CustomRoles> Modifiers1 = new(player1.GetCustomSubRoles());
        List<CustomRoles> Modifiers2 = new(player2.GetCustomSubRoles());
        foreach (CustomRoles Modifier in Modifiers1)
        {
            Main.PlayerStates[player1.PlayerId].RemoveSubRole(Modifier);
            player2.RpcSetCustomRole(Modifier, false, false);
        }
        foreach (CustomRoles Modifier in Modifiers2)
        {
            Main.PlayerStates[player2.PlayerId].RemoveSubRole(Modifier);
            player1.RpcSetCustomRole(Modifier, false, false);
        }
        Logger.Info($"{player1.GetNameWithRole()}'s Modifiers swapped with {player2.GetNameWithRole()}", "Harvester");
    }
    public override void SetAbilityButtonText(HudManager hud, byte playerId) =>
        hud.AbilityButton.OverrideText(GetString("Harvester.ShapeshiftButton"));
}
