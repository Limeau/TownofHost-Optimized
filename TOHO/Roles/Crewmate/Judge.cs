using Hazel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using HarmonyLib;
using TOHO.Modules;
using TOHO.Modules.ChatManager;
using TOHO.Roles.Coven;
using TOHO.Roles.Double;
using UnityEngine;
using static TOHO.Translator;
using static TOHO.Utils;

namespace TOHO.Roles.Crewmate;

internal class Justice : RoleBase
{
    //===========================SETUP================================\\
    public override CustomRoles Role => CustomRoles.Justice;
    private const int Id = 10700;
    public override CustomRoles ThisRoleBase => CustomRoles.Crewmate;
    public override Custom_RoleType ThisRoleType => Custom_RoleType.CrewmateKilling;
    //==================================================================\\

    public static OptionItem TrialLimitPerMeeting;
    private static OptionItem TrialLimitPerGame;
    private static OptionItem TryHideMsg;
    private static OptionItem CanTrialMadmate;
    private static OptionItem CanTrialCharmed;
    private static OptionItem CanTrialSidekick;
    private static OptionItem CanTrialInfected;
    private static OptionItem CanTrialContagious;
    private static OptionItem CanTrialEnchanted;
    private static OptionItem CanTrialCrewKilling;
    private static OptionItem CanTrialNeutralB;
    private static OptionItem CanTrialNeutralK;
    private static OptionItem CanTrialNeutralE;
    private static OptionItem CanTrialNeutralC;
    private static OptionItem CanTrialNeutralA;
    private static OptionItem CanTrialCoven;

    private static readonly Dictionary<byte, int> TrialLimitMeeting = [];
    private static readonly Dictionary<byte, int> TrialLimitGame = [];

    public override void SetupCustomOption()
    {
        Options.SetupRoleOptions(Id, TabGroup.CrewmateRoles, CustomRoles.Justice);
        TrialLimitPerMeeting = IntegerOptionItem.Create(Id + 10, "JusticeTrialLimitPerMeeting", new(1, 30, 1), 1, TabGroup.CrewmateRoles, false).SetParent(Options.CustomRoleSpawnChances[CustomRoles.Justice])
            .SetValueFormat(OptionFormat.Times);
        TrialLimitPerGame = IntegerOptionItem.Create(Id + 25, "JusticeTrialLimitPerGame", new(1, 30, 1), 1, TabGroup.CrewmateRoles, false).SetParent(Options.CustomRoleSpawnChances[CustomRoles.Justice])
            .SetValueFormat(OptionFormat.Times);
        CanTrialMadmate = BooleanOptionItem.Create(Id + 12, "JusticeCanTrialMadmate", true, TabGroup.CrewmateRoles, false).SetParent(Options.CustomRoleSpawnChances[CustomRoles.Justice]);
        CanTrialCharmed = BooleanOptionItem.Create(Id + 16, "JusticeCanTrialCharmed", true, TabGroup.CrewmateRoles, false).SetParent(Options.CustomRoleSpawnChances[CustomRoles.Justice]);
        CanTrialSidekick = BooleanOptionItem.Create(Id + 19, "JusticeCanTrialSidekick", true, TabGroup.CrewmateRoles, false).SetParent(Options.CustomRoleSpawnChances[CustomRoles.Justice]);
        CanTrialInfected = BooleanOptionItem.Create(Id + 20, "JusticeCanTrialInfected", true, TabGroup.CrewmateRoles, false).SetParent(Options.CustomRoleSpawnChances[CustomRoles.Justice]);
        CanTrialContagious = BooleanOptionItem.Create(Id + 21, "JusticeCanTrialContagious", true, TabGroup.CrewmateRoles, false).SetParent(Options.CustomRoleSpawnChances[CustomRoles.Justice]);
        CanTrialEnchanted = BooleanOptionItem.Create(Id + 24, "JusticeCanTrialEnchanted", true, TabGroup.CrewmateRoles, false).SetParent(Options.CustomRoleSpawnChances[CustomRoles.Justice]);
        CanTrialCrewKilling = BooleanOptionItem.Create(Id + 13, "JusticeCanTrialnCrewKilling", true, TabGroup.CrewmateRoles, false).SetParent(Options.CustomRoleSpawnChances[CustomRoles.Justice]);
        CanTrialNeutralB = BooleanOptionItem.Create(Id + 14, "JusticeCanTrialNeutralB", false, TabGroup.CrewmateRoles, false).SetParent(Options.CustomRoleSpawnChances[CustomRoles.Justice]);
        CanTrialNeutralE = BooleanOptionItem.Create(Id + 17, "JusticeCanTrialNeutralE", false, TabGroup.CrewmateRoles, false).SetParent(Options.CustomRoleSpawnChances[CustomRoles.Justice]);
        CanTrialNeutralC = BooleanOptionItem.Create(Id + 18, "JusticeCanTrialNeutralC", false, TabGroup.CrewmateRoles, false).SetParent(Options.CustomRoleSpawnChances[CustomRoles.Justice]);
        CanTrialNeutralK = BooleanOptionItem.Create(Id + 15, "JusticeCanTrialNeutralK", true, TabGroup.CrewmateRoles, false).SetParent(Options.CustomRoleSpawnChances[CustomRoles.Justice]);
        CanTrialNeutralA = BooleanOptionItem.Create(Id + 22, "JusticeCanTrialNeutralA", true, TabGroup.CrewmateRoles, false).SetParent(Options.CustomRoleSpawnChances[CustomRoles.Justice]);
        CanTrialCoven = BooleanOptionItem.Create(Id + 23, "JusticeCanTrialCoven", true, TabGroup.CrewmateRoles, false).SetParent(Options.CustomRoleSpawnChances[CustomRoles.Justice]);
        TryHideMsg = BooleanOptionItem.Create(Id + 11, "JusticeTryHideMsg", true, TabGroup.CrewmateRoles, false).SetParent(Options.CustomRoleSpawnChances[CustomRoles.Justice])
            .SetColor(Color.green);
    }
    public override void Init()
    {
        TrialLimitMeeting.Clear();
        TrialLimitGame.Clear();
    }
    public override void Add(byte playerId)
    {
        TrialLimitMeeting[playerId] = TrialLimitPerMeeting.GetInt();
        TrialLimitGame[playerId] = TrialLimitPerGame.GetInt();
        playerId.SetAbilityUseLimit(TrialLimitPerGame.GetInt());
    }
    public override void Remove(byte playerId)
    {
        TrialLimitMeeting.Remove(playerId);
        TrialLimitGame.Remove(playerId);
    }
    public override void OnReportDeadBody(PlayerControl party, NetworkedPlayerInfo dinosaur)
    {
        if (!_Player) return;

        TrialLimitMeeting[_Player.PlayerId] = TrialLimitPerMeeting.GetInt();

        if (TrialLimitGame[_Player.PlayerId] <= TrialLimitPerMeeting.GetInt())
        {
            _Player.SetAbilityUseLimit(TrialLimitGame[_Player.PlayerId]);
        }
        else
        {
            _Player.SetAbilityUseLimit(TrialLimitPerMeeting.GetInt());
        }
    }
    public override void AfterMeetingTasks()
    {
        if (!_Player) return;
        _Player.SetAbilityUseLimit(TrialLimitGame[_Player.PlayerId]);
    }
    public static bool TrialMsg(PlayerControl pc, string msg, bool isUI = false)
    {
        var originMsg = msg;

        if (!AmongUsClient.Instance.AmHost) return false;
        if (!GameStates.IsMeeting || pc == null || GameStates.IsExilling) return false;
        if (!pc.Is(CustomRoles.Justice)) return false;

        int operate = 0;
        msg = msg.ToLower().TrimStart().TrimEnd();
        if (CheckCommond(ref msg, "id|guesslist|gl编号|玩家编号|玩家id|id列表|玩家列表|列表|所有id|全部id||編號|玩家編號")) operate = 1;
        else if (CheckCommond(ref msg, "sp|jj|tl|trial|审判|判|审|審判|審", false)) operate = 2;
        else return false;

        if (!pc.IsAlive())
        {
            pc.ShowInfoMessage(isUI, GetString("JusticeDead"));
            return true;
        }

        if (operate == 1)
        {
            SendMessage(GuessManager.GetFormatString(), pc.PlayerId);
            return true;
        }
        else if (operate == 2)
        {

            if (TryHideMsg.GetBool())
            {
                //if (Options.NewHideMsg.GetBool()) ChatManager.SendPreviousMessagesToAll();
                //else GuessManager.TryHideMsg();
                GuessManager.TryHideMsg();
                ChatManager.SendPreviousMessagesToAll();
            }
            else if (pc.AmOwner) SendMessage(originMsg, 255, pc.GetRealName());

            if (!MsgToPlayerAndRole(msg, out byte targetId, out string error))
            {
                SendMessage(error, pc.PlayerId);
                return true;
            }
            var target = GetPlayerById(targetId);
            if (target != null)
            {
                Logger.Info($"{pc.GetNameWithRole()} try trial {target.GetNameWithRole()}", "Justice");
                bool JusticeSuicide = true;
                if (TrialLimitMeeting[pc.PlayerId] < 1)
                {
                    pc.ShowInfoMessage(isUI, GetString("JusticeTrialMaxMeetingMsg"));
                    return true;
                }
                if (pc.GetAbilityUseLimit() < 1)
                {
                    pc.ShowInfoMessage(isUI, GetString("JusticeTrialMaxGameMsg"));
                }
                if (target.Is(CustomRoles.VoodooMaster) && VoodooMaster.Dolls[target.PlayerId].Count > 0)
                {
                    target = GetPlayerById(VoodooMaster.Dolls[target.PlayerId].Where(x => GetPlayerById(x).IsAlive()).ToList().RandomElement());
                    SendMessage(string.Format(GetString("VoodooMasterTargetInMeeting"), target.GetRealName()), Utils.GetPlayerListByRole(CustomRoles.VoodooMaster).First().PlayerId);
                }
                if (Jailer.IsTarget(target.PlayerId))
                {
                    pc.ShowInfoMessage(isUI, GetString("CanNotTrialJailed"), ColorString(GetRoleColor(CustomRoles.Jailer), GetString("JailerTitle")));
                    return true;
                }
                if (pc.PlayerId == target.PlayerId)
                {
                    pc.ShowInfoMessage(isUI, GetString("Justice_LaughToWhoTrialSelf"), ColorString(Color.cyan, GetString("MessageFromKPD")));
                    goto SkipToPerform;
                }
                if (target.Is(CustomRoles.NiceMini) && Mini.Age < 18)
                {
                    pc.ShowInfoMessage(isUI, GetString("GuessMini"));
                    return true;
                }
                if (target.Is(CustomRoles.PunchingBag))
                {
                    pc.ShowInfoMessage(isUI, GetString("EradicatePunchingBag"));
                    return true;
                }

                if (target.Is(CustomRoles.Rebound))
                {
                    Logger.Info($"{pc.GetNameWithRole()} Justiced {target.GetNameWithRole()}, Justice sucide = true because target rebound", "JusticeTrialMsg");
                    JusticeSuicide = true;
                }
                else if (target.Is(CustomRoles.Solsticer))
                {
                    pc.ShowInfoMessage(isUI, GetString("GuessSolsticer"));
                    return true;
                }
                else if (target.IsTransformedNeutralApocalypse()) JusticeSuicide = true;
                else if (target.Is(CustomRoles.Trickster)) JusticeSuicide = true;
                else if (Medic.IsProtected(target.PlayerId) && !Medic.GuesserIgnoreShield.GetBool())
                {
                    pc.ShowInfoMessage(isUI, GetString("GuessShielded"));
                    return true;
                }
                else if (Guardian.CannotBeKilled(target))
                {
                    pc.ShowInfoMessage(isUI, GetString("GuessGuardianTask"));
                    return true;
                }
                else if (pc.IsAnySubRole(x => x.IsConverted())) JusticeSuicide = false;
                else if (target.Is(CustomRoles.Rascal)) JusticeSuicide = false;
                else if ((target.Is(CustomRoles.Sidekick) || target.Is(CustomRoles.Recruit)) && CanTrialSidekick.GetBool()) JusticeSuicide = false;
                else if ((target.GetCustomRole().IsMadmate() || target.Is(CustomRoles.Madmate)) && CanTrialMadmate.GetBool()) JusticeSuicide = false;
                else if (target.Is(CustomRoles.Infected) && CanTrialInfected.GetBool()) JusticeSuicide = false;
                else if (target.Is(CustomRoles.Contagious) && CanTrialContagious.GetBool()) JusticeSuicide = false;
                else if (target.Is(CustomRoles.Charmed) && CanTrialCharmed.GetBool()) JusticeSuicide = false;
                else if (target.Is(CustomRoles.Enchanted) && CanTrialEnchanted.GetBool()) JusticeSuicide = false;
                else if (target.GetCustomRole().IsCrewKiller() && CanTrialCrewKilling.GetBool()) JusticeSuicide = false;
                else if (target.GetCustomRole().IsNK() && CanTrialNeutralK.GetBool()) JusticeSuicide = false;
                else if (target.GetCustomRole().IsNB() && CanTrialNeutralB.GetBool()) JusticeSuicide = false;
                else if (target.GetCustomRole().IsNE() && CanTrialNeutralE.GetBool()) JusticeSuicide = false;
                else if (target.GetCustomRole().IsNC() && CanTrialNeutralC.GetBool()) JusticeSuicide = false;
                else if (target.GetCustomRole().IsNA() && CanTrialNeutralA.GetBool()) JusticeSuicide = false;
                else if (target.GetCustomRole().IsCoven() && CanTrialCoven.GetBool()) JusticeSuicide = false;
                else if (target.GetCustomRole().IsImpostor()) JusticeSuicide = false;
                else
                {
                    Logger.Warn("Impossibe to reach here!", "JusticeTrial");
                    JusticeSuicide = true;
                }

            SkipToPerform:
                var dp = JusticeSuicide ? pc : target;
                target = dp;

                string Name = dp.GetRealName();

                TrialLimitMeeting[pc.PlayerId]--;
                TrialLimitGame[pc.PlayerId]--;
                pc.RpcRemoveAbilityUse();

                if (!GameStates.IsProceeding)
                    _ = new LateTask(() =>
                    {
                        dp.SetDeathReason(PlayerState.DeathReason.Trialed);
                        dp.SetRealKiller(pc);
                        GuessManager.RpcGuesserMurderPlayer(dp);

                        Main.PlayersDiedInMeeting.Add(dp.PlayerId);
                        MurderPlayerPatch.AfterPlayerDeathTasks(pc, dp, true);

                        _ = new LateTask(() => { SendMessage(string.Format(GetString("Justice_TrialKill"), Name), 255, ColorString(GetRoleColor(CustomRoles.Justice), GetString("Justice_TrialKillTitle")), true); }, 0.6f, "Guess Msg");

                    }, 0.2f, "Trial Kill");
            }
        }
        return true;
    }
    private static bool MsgToPlayerAndRole(string msg, out byte id, out string error)
    {
        if (msg.StartsWith("/")) msg = msg.Replace("/", string.Empty);

        Regex r = new("\\d+");
        MatchCollection mc = r.Matches(msg);
        string result = string.Empty;
        for (int i = 0; i < mc.Count; i++)
        {
            result += mc[i];
        }

        if (int.TryParse(result, out int num))
        {
            id = Convert.ToByte(num);
        }
        else
        {
            id = byte.MaxValue;
            error = GetString("Justice_TrialHelp");
            return false;
        }

        PlayerControl target = GetPlayerById(id);
        if (target == null || target.Data.IsDead)
        {
            error = GetString("Justice_TrialNull");
            return false;
        }

        error = string.Empty;
        return true;
    }
    public static bool CheckCommond(ref string msg, string command, bool exact = true)
    {
        var comList = command.Split('|');
        foreach (var comm in comList)
        {
            if (exact)
            {
                if (msg == "/" + comm) return true;
            }
            else
            {
                if (msg.StartsWith("/" + comm))
                {
                    msg = msg.Replace("/" + comm, string.Empty);
                    return true;
                }
            }
        }
        return false;
    }

    private static void SendRPC(byte targetId)
    {
        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.Justice, SendOption.Reliable);
        writer.Write(targetId);
        AmongUsClient.Instance.FinishRpcImmediately(writer);
    }
    public static void ReceiveRPC_Custom(MessageReader reader, PlayerControl pc)
    {
        byte targetId = reader.ReadByte();

        TrialMsg(pc, $"/tl {targetId}", true);
    }

    private static void JusticeOnClick(byte targetId /*, MeetingHud __instance*/)
    {
        Logger.Msg($"Click: ID {targetId}", "Justice UI");
        var target = targetId.GetPlayer();
        if (target == null || !target.IsAlive() || !GameStates.IsVoting) return;
        if (AmongUsClient.Instance.AmHost) TrialMsg(PlayerControl.LocalPlayer, $"/tl {targetId}", true);
        else SendRPC(targetId);
    }

    public override string NotifyPlayerName(PlayerControl seer, PlayerControl target, string TargetPlayerName = "", bool IsForMeeting = false)
        => IsForMeeting && seer.IsAlive() && target.IsAlive() ? ColorString(GetRoleColor(CustomRoles.Justice), target.PlayerId.ToString()) + " " + TargetPlayerName : "";
    public override string PVANameText(PlayerVoteArea pva, PlayerControl seer, PlayerControl target)
        => seer.IsAlive() && target.IsAlive() ? ColorString(GetRoleColor(CustomRoles.Justice), target.PlayerId.ToString()) + " " + pva.NameText.text : "";

    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start))]
    class StartMeetingPatch
    {
        public static void Postfix(MeetingHud __instance)
        {
            if (PlayerControl.LocalPlayer.Is(CustomRoles.Justice) && PlayerControl.LocalPlayer.IsAlive())
                CreateJusticeButton(__instance);
        }
    }
    public static void CreateJusticeButton(MeetingHud __instance)
    {
        foreach (var pva in __instance.playerStates)
        {
            var pc = GetPlayerById(pva.PlayerId);
            if (pc == null || !pc.IsAlive()) continue;

            GameObject template = pva.Buttons.transform.Find("CancelButton").gameObject;
            GameObject targetBox = UnityEngine.Object.Instantiate(template, pva.transform);
            targetBox.name = "ShootButton";
            targetBox.transform.localPosition = new Vector3(-0.35f, 0.03f, -1.31f);
            SpriteRenderer renderer = targetBox.GetComponent<SpriteRenderer>();
            renderer.sprite = CustomButton.Get("JusticeIcon");
            PassiveButton button = targetBox.GetComponent<PassiveButton>();
            button.OnClick.RemoveAllListeners();
            button.OnClick.AddListener((UnityEngine.Events.UnityAction)(() => JusticeOnClick(pva.PlayerId/*, __instance*/)));
        }
    }
}
