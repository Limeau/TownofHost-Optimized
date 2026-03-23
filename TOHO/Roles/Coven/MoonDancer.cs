using System.Collections.Generic;
using System.Linq;
using Hazel;
using InnerNet;
using TOHO.Modules;
using TOHO.Roles.Modifiers;
using TOHO.Roles.Crewmate;
using TOHO.Roles.Double;
using TOHO.Roles.Impostor;
using TOHO.Roles.Neutral;
using static TOHO.Options;
using static TOHO.Translator;
using static TOHO.Utils;

namespace TOHO.Roles.Coven;

internal class MoonDancer : CovenManager
{
    //===========================SETUP================================\\
    public override CustomRoles Role => CustomRoles.MoonDancer;
    private const int Id = 30500;
    public override bool IsDesyncRole => true;
    public override CustomRoles ThisRoleBase => CustomRoles.Impostor;
    public override Custom_RoleType ThisRoleType => Custom_RoleType.CovenUtility;
    //==================================================================\\

    private static OptionItem BatonPassCooldown;
    private static OptionItem BlastOffChance;
    private static OptionItem BatonPassEnabledModifiers;

    private static List<CustomRoles> Modifiers = [];
    private static readonly Dictionary<byte, HashSet<byte>> BatonPassList = [];
    private static readonly Dictionary<byte, HashSet<byte>> BlastedOffList = [];
    private static readonly Dictionary<byte, float> originalSpeed = [];

    public override void SetupCustomOption()
    {
        SetupSingleRoleOptions(Id, TabGroup.CovenRoles, CustomRoles.MoonDancer, 1, zeroOne: false);
        BatonPassCooldown = FloatOptionItem.Create(Id + 10, "MoonDancerBatonPassCooldown", new(0f, 180f, 2.5f), 30f, TabGroup.CovenRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.MoonDancer])
            .SetValueFormat(OptionFormat.Seconds);
        BlastOffChance = IntegerOptionItem.Create(Id + 11, "MoonDancerBlastOffChance", new(0, 100, 1), 50, TabGroup.CovenRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.MoonDancer])
            .SetValueFormat(OptionFormat.Percent);
        BatonPassEnabledModifiers = BooleanOptionItem.Create(Id + 12, "MoonDancerPassEnabledModifiers", false, TabGroup.CovenRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.MoonDancer]);
    }
    public override void Init()
    {
        BatonPassList.Clear();
        Modifiers.Clear();
        BlastedOffList.Clear();
        originalSpeed.Clear();

        Modifiers.AddRange(GroupedModifiers[ModifierTypes.Helpful]);
        Modifiers.AddRange(GroupedModifiers[ModifierTypes.Harmful]);
        if (BatonPassEnabledModifiers.GetBool())
        {
            Modifiers = Modifiers.Where(role => role.GetMode() != 0).ToList();
        }
    }
    public override void Add(byte playerId)
    {
        BatonPassList[playerId] = [];
        BlastedOffList[playerId] = [];
    }
    private void SyncBlastList()
    {
        SendRPC(byte.MaxValue);
        foreach (var bl in BlastedOffList)
            SendRPC(bl.Key);
    }
    private void SendRPC(byte playerId)
    {
        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SyncRoleSkill, SendOption.Reliable, -1);
        writer.WriteNetObject(_Player);
        writer.Write(playerId);
        if (playerId != byte.MaxValue)
        {
            writer.Write(BlastedOffList[playerId].Count);
            foreach (var bl in BlastedOffList[playerId])
                writer.Write(bl);
        }
        AmongUsClient.Instance.FinishRpcImmediately(writer);
    }
    public override void ReceiveRPC(MessageReader reader, PlayerControl NaN)
    {
        byte playerId = reader.ReadByte();
        if (playerId == byte.MaxValue)
        {
            BlastedOffList.Clear();
        }
        else
        {
            int blastNum = reader.ReadInt32();
            BlastedOffList.Remove(playerId);
            HashSet<byte> list = [];
            for (int i = 0; i < blastNum; i++)
                list.Add(reader.ReadByte());
            BlastedOffList.Add(playerId, list);
        }
    }
    public override bool CanUseKillButton(PlayerControl pc) => true;
    public override void SetKillCooldown(byte id) => Main.AllPlayerKillCooldown[id] = BatonPassCooldown.GetFloat();

    public static bool CanBlast(PlayerControl pc, byte id)
    {
        if (!pc.Is(CustomRoles.MoonDancer) || GameStates.IsMeeting) return false;

        var target = GetPlayerById(id);

        var penguins = GetRoleBasesByType<Penguin>()?.ToList();
        if (penguins != null)
        {
            if (penguins.Any(pg => target.PlayerId == pg.AbductVictim?.PlayerId))
            {
                return false;
            }
        }

        return target != null && target.CanBeTeleported() && !target.Is(CustomRoles.Stubborn) && !target.IsTransformedNeutralApocalypse() && !Medic.IsProtected(target.PlayerId) && !target.Is(CustomRoles.GM) && !IsBlasted(pc, id) && !IsBlasted(id);
    }
    private static bool IsBlasted(PlayerControl pc, byte id) => BlastedOffList.TryGetValue(pc.PlayerId, out var list) && list.Contains(id);
    public static bool IsBlasted(byte id)
    {
        foreach (var bl in BlastedOffList)
            if (bl.Value.Contains(id))
                return true;
        return false;
    }
    private void BlastPlayer(PlayerControl pc, PlayerControl target)
    {
        if (pc == null || target == null || !target.CanBeTeleported()) return;
        if (Mini.Age < 18 && (target.Is(CustomRoles.NiceMini) || target.Is(CustomRoles.EvilMini)))
        {
            pc.Notify(Utils.ColorString(Utils.GetRoleColor(CustomRoles.NiceMini), GetString("CantEat")));
            return;
        }

        if (!BlastedOffList.ContainsKey(pc.PlayerId)) BlastedOffList.Add(pc.PlayerId, []);
        BlastedOffList[pc.PlayerId].Add(target.PlayerId);

        SyncBlastList();

        originalSpeed.Remove(target.PlayerId);
        originalSpeed.Add(target.PlayerId, Main.AllPlayerSpeed[target.PlayerId]);

        target.RpcTeleport(Pelican.GetBlackRoomPSForPelican());
        Main.AllPlayerSpeed[target.PlayerId] = 0.5f;
        ReportDeadBodyPatch.CanReport[target.PlayerId] = false;
        target.MarkDirtySettings();

        //NotifyRoles(SpecifySeer: pc);
        //NotifyRoles(SpecifySeer: target);

        Logger.Info($"{pc.GetRealName()} Blasted Off {target.GetRealName()}", "MoonDancer");
    }

    public override bool OnCheckMurderAsKiller(PlayerControl killer, PlayerControl target)
    {
        if (killer == null || target == null) return false;
        if (HasNecronomicon(killer))
        {
            var rd = IRandom.Instance;
            if (target.GetCustomRole().IsCovenTeam())
            {
                killer.Notify(GetString("MoonDancerCantBlastOff"));
                return false;
            }
            if (rd.Next(0, 101) < BlastOffChance.GetInt())
            {
                if (CanBlast(killer, target.PlayerId))
                {
                    BlastPlayer(killer, target);
                    if (!DisableShieldAnimations.GetBool()) killer.RpcGuardAndKill(killer);
                    killer.ResetKillCooldown();
                    killer.SetKillCooldown();
                    killer.RPCPlayCustomSound("BlastOff");
                    target.RPCPlayCustomSound("BlastOff");
                }
                else
                {
                    killer.ResetKillCooldown();
                    killer.SetKillCooldown();
                    killer.Notify(GetString("MoonDancerCantBlastOff"));
                }
                return false;
            }
            else
            {
                killer.Notify(GetString("MoonDancerNormalKill"));
                return true;
            }
        }
        if (target.Is(CustomRoles.Stubborn))
        {
            killer.Notify(GetString("StubbornNotify"));
            return false;
        }
        if (target.GetCustomRole().IsCovenTeam())
        {
            BatonPassList[killer.PlayerId].Add(target.PlayerId);
            killer.Notify(GetString("MoonDancerGiveHelpfulModifier"));
        }
        else
        {
            BatonPassList[killer.PlayerId].Add(target.PlayerId);
            killer.Notify(GetString("MoonDancerGiveHarmfulModifier"));
        }
        killer.ResetKillCooldown();
        killer.SetKillCooldown();
        return false;
    }

    public override void OnReportDeadBody(PlayerControl reporter, NetworkedPlayerInfo target)
    {
        if (_Player == null) return;
        
        KillBlastedOff();
        foreach (var md in BatonPassList.Keys)
        {
            var player = GetPlayerById(md);
            if (player == null) continue;
        
            DistributeModifiers(player);
        }
    }
    private static void DistributeModifiers(PlayerControl md)
    {
        var rd = IRandom.Instance;
        foreach (var pc in BatonPassList[md.PlayerId])
        {
            var player = GetPlayerById(pc);

            if (player == null)
            {
                BatonPassList[md.PlayerId].Remove(pc);
                continue;
            }
            
            var Modifier = Modifiers.RandomElement();
            var helpful = GroupedModifiers[ModifierTypes.Helpful].Where(x => Modifiers.Contains(x)).ToList();
            var harmful = GroupedModifiers[ModifierTypes.Harmful].Where(x => Modifiers.Contains(x)).ToList();
            if (player.GetCustomRole().IsCovenTeam() || (player.Is(CustomRoles.Lovers) && md.Is(CustomRoles.Lovers)))
            {
                if (helpful.Count <= 0)
                {
                    SendMessage(string.Format(GetString("MoonDancerNoModifiers"), player.GetRealName()), md.PlayerId);
                    Logger.Info("No Modifiers to pass.", "MoonDancer");
                    continue;
                }
                Modifier = helpful.RandomElement();
            }
            else
            {
                if (harmful.Count <= 0)
                {
                    SendMessage(string.Format(GetString("MoonDancerNoModifiers"), player.GetRealName()), md.PlayerId);
                    Logger.Info("No Modifiers to pass.", "MoonDancer");
                    continue;
                }
                Modifier = harmful.RandomElement();
            }
            player.RpcSetCustomRole(Modifier);
            player.AddInSwitchModifiers(player, Modifier);
            Logger.Info("Modifier Passed.", "MoonDancer");
        }
        BatonPassList[md.PlayerId].Clear();
    }
    private void KillBlastedOff()
    {
        foreach (var pc in BlastedOffList)
        {
            foreach (var tar in pc.Value)
            {
                var target = GetPlayerById(tar);
                var killer = GetPlayerById(pc.Key);
                if (killer == null || target == null) continue;
                Main.AllPlayerSpeed[tar] = Main.AllPlayerSpeed[tar] - 0.5f + originalSpeed[tar];
                ReportDeadBodyPatch.CanReport[tar] = true;
                target.RpcExileV2();
                target.SetRealKiller(killer);
                tar.SetDeathReason(PlayerState.DeathReason.BlastedOff);
                Main.PlayerStates[tar].SetDead();
                MurderPlayerPatch.AfterPlayerDeathTasks(killer, target, true);
                Logger.Info($"{killer.GetRealName()} Blasted Off {target.GetRealName()}", "MoonDancer");
            }
            BlastedOffList[pc.Key].Clear();
        }
        SyncBlastList();
    }
    public override void OnMurderPlayerAsTarget(PlayerControl killer, PlayerControl moonDancer, bool inMeeting, bool isSuicide)
    {
        if (inMeeting || !BlastedOffList.TryGetValue(moonDancer.PlayerId, out var blastedOff)) return;

        foreach (var bl in blastedOff)
        {
            var pc = GetPlayerById(bl);
            pc.SetRealKiller(moonDancer);
            pc.RpcExileV2();
            pc.SetDeathReason(PlayerState.DeathReason.BlastedOff);
        }
    }
    public override void SetAbilityButtonText(HudManager hud, byte playerId)
    {
        if (HasNecronomicon(playerId))
        {
            hud.KillButton.OverrideText(GetString("MoonDancerNecroButtonText"));
        }
        else
        {
            hud.KillButton.OverrideText(GetString("MoonDancerButtonText"));
        }
    }
}
