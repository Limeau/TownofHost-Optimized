using Hazel;
using System;
using System.Text.RegularExpressions;
using TOHE.Modules.ChatManager;
using TOHE.Roles.AddOns.Crewmate;
using TOHE.Roles.AddOns.Impostor;
using TOHE.Roles.Crewmate;
using TOHE.Roles.Double;
using TOHE.Roles.Neutral;
using UnityEngine;
using static TOHE.Options;
using static TOHE.Translator;
using static TOHE.Utils;

namespace TOHE.Roles.Coven;

internal class Ritualist : CovenManager
{
    //===========================SETUP================================\\
    public override CustomRoles Role => CustomRoles.Ritualist;
    private const int Id = 30800;
    public override bool IsDesyncRole => true;
    public override CustomRoles ThisRoleBase => CustomRoles.Impostor;
    public override Custom_RoleType ThisRoleType => Custom_RoleType.CovenPower;
    //==================================================================\\

    private static OptionItem MaxRitsPerRound;
    public static OptionItem TryHideMsg;
    public static OptionItem EnchantedKnowsCoven;
    public static OptionItem EnchantedKnowsEnchanted;


    private static readonly Dictionary<byte, int> RitualLimit = [];
    private static readonly Dictionary<byte, List<byte>> EnchantedPlayers = [];

    public override void SetupCustomOption()
    {
        SetupSingleRoleOptions(Id, TabGroup.CovenRoles, CustomRoles.Ritualist, 1, zeroOne: false);
        MaxRitsPerRound = IntegerOptionItem.Create(Id + 10, "RitualistMaxRitsPerRound", new(1, 15, 1), 2, TabGroup.CovenRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.Ritualist])
            .SetValueFormat(OptionFormat.Times);
        TryHideMsg = BooleanOptionItem.Create(Id + 11, "RitualistTryHideMsg", true, TabGroup.CovenRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.Ritualist])
            .SetColor(Color.green);
        EnchantedKnowsCoven = BooleanOptionItem.Create(Id + 12, "RitualistEnchantedKnowsCoven", true, TabGroup.CovenRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.Ritualist]);
        EnchantedKnowsEnchanted = BooleanOptionItem.Create(Id + 13, "RitualistEnchantedKnowsEnchanted", true, TabGroup.CovenRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.Ritualist]);

    }
    public override void Init()
    {
        RitualLimit.Clear();
        EnchantedPlayers.Clear();
    }
    public override void Add(byte PlayerId)
    {
        EnchantedPlayers[PlayerId] = [];
        RitualLimit.Add(PlayerId, MaxRitsPerRound.GetInt());
    }
    public override bool CanUseKillButton(PlayerControl pc) => HasNecronomicon(pc);
    public override void OnReportDeadBody(PlayerControl hatsune, NetworkedPlayerInfo miku)
    {
        foreach (var pid in RitualLimit.Keys)
        {
            RitualLimit[pid] = MaxRitsPerRound.GetInt();
        }
    }
    public override bool OnCheckMurderAsKiller(PlayerControl killer, PlayerControl target)
    {
        if (target.GetCustomRole().IsCovenTeam())
        {
            killer.Notify(GetString("CovenDontKillOtherCoven"));
            return false;
        }
        return true;
    }
    public override string NotifyPlayerName(PlayerControl seer, PlayerControl target, string TargetPlayerName = "", bool IsForMeeting = false)
        => IsForMeeting && seer.IsAlive() && target.IsAlive() ? ColorString(GetRoleColor(CustomRoles.Ritualist), target.PlayerId.ToString()) + " " + TargetPlayerName : "";
    public override string PVANameText(PlayerVoteArea pva, PlayerControl seer, PlayerControl target)
        => seer.IsAlive() && target.IsAlive() ? ColorString(GetRoleColor(CustomRoles.Ritualist), target.PlayerId.ToString()) + " " + pva.NameText.text : "";
    public static bool RitualistMsgCheck(PlayerControl pc, string msg, bool isUI = false)
    {
        var originMsg = msg;

        if (!AmongUsClient.Instance.AmHost) return false;
        if (!GameStates.IsMeeting || pc == null || GameStates.IsExilling) return false;
        if (!pc.Is(CustomRoles.Ritualist)) return false;
        msg = msg.ToLower().TrimStart().TrimEnd();
        
        int operate;
        if (CheckCommond(ref msg, "id|guesslist|gl编号|玩家编号|玩家id|id列表|玩家列表|列表|所有id|全部id||編號|玩家編號")) operate = 1;
        else if (CheckCommond(ref msg, "rt|rit|ritual|bloodritual|鲜血仪式|仪式|献祭|举行|附魔", false)) operate = 2;
        else return false;

        if (!pc.IsAlive())
        {
            pc.ShowInfoMessage(isUI, GetString("GuessDead"));
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
                TryHideMsgForRitual();
                ChatManager.SendPreviousMessagesToAll();
            }
            else if (pc.AmOwner) SendMessage(originMsg, 255, pc.GetRealName());
            if (RitualLimit[pc.PlayerId] <= 0)
            {
                pc.ShowInfoMessage(isUI, GetString("RitualistRitualMax"));
                return true;
            }

            if (!MsgToPlayerAndRole(msg, out byte targetId, out CustomRoles role, out string error))
            {
                pc.ShowInfoMessage(isUI, error);
                return true;
            }
            var target = GetPlayerById(targetId);
            if (role.IsAdditionRole())
            {
                pc.ShowInfoMessage(isUI, GetString("RitualistGuessAddon"));
                return true;
            }
            if (!target.Is(role))
            {
                pc.ShowInfoMessage(isUI, GetString("RitualistRitualFail"));
                RitualLimit[pc.PlayerId] = 0;
                return true;
            }
            if (!CanBeConverted(target))
            {
                pc.ShowInfoMessage(isUI, GetString("RitualistRitualImpossible"));
                return true;
            }

            Logger.Info($"{pc.GetNameWithRole()} enchant {target.GetNameWithRole()}", "Ritualist");

            RitualLimit[pc.PlayerId]--;

            EnchantedPlayers[pc.PlayerId].Add(target.PlayerId);
            SendMessage(string.Format(GetString("RitualistConvertNotif"), CustomRoles.Ritualist.ToColoredString()), target.PlayerId);
            SendMessage(string.Format(GetString("RitualistRitualSuccess"), target.GetRealName()), pc.PlayerId);
            return true;
        }
        return false;
    }
    private static void TryHideMsgForRitual()
    {
        ChatUpdatePatch.DoBlockChat = true;
        if (ChatManager.quickChatSpamMode != QuickChatSpamMode.QuickChatSpam_Disabled)
        {
            ChatManager.SendQuickChatSpam();
            ChatUpdatePatch.DoBlockChat = false;
            return;
        }
        
        List<CustomRoles> roles = CustomRolesHelper.AllRoles.Where(x => x is not CustomRoles.NotAssigned).ToList();
        var rd = IRandom.Instance;
        var msg = new System.Text.StringBuilder();
        string[] command = ["rt", "rit", "ritual", "bloodritual"];
        for (int i = 0; i < 20; i++)
        {
            msg.Clear().Append('/');
            if (rd.Next(1, 100) < 20)
            {
                msg.Append("id");
            }
            else
            {
                msg.Append(command[rd.Next(0, command.Length - 1)]);
                msg.Append(rd.Next(1, 100) < 50 ? string.Empty : " ");
                msg.Append(rd.Next(0, 15));
                msg.Append(rd.Next(1, 100) < 50 ? string.Empty : " ");
                CustomRoles role = roles.RandomElement();
                msg.Append(rd.Next(1, 100) < 50 ? string.Empty : " ");
                msg.Append(GetRoleName(role));
                
            }
            var player = Main.AllAlivePlayerControls.RandomElement();
            FastDestroyableSingleton<HudManager>.Instance.Chat.AddChat(player, msg.ToString());
            var writer = CustomRpcSender.Create("MessagesToSend", SendOption.None);
            writer.StartMessage(-1);
            writer.StartRpc(player.NetId, (byte)RpcCalls.SendChat)
                .Write(msg.ToString())
                .EndRpc();
            writer.EndMessage();
            writer.SendMessage();
        }
        ChatUpdatePatch.DoBlockChat = false;
    }
    public override void AfterMeetingTasks()
    {
        foreach (var rit in EnchantedPlayers.Keys)
        {
            var ritualist = GetPlayerById(rit);
            foreach (var pc in EnchantedPlayers[rit])
            {
                ConvertRole(ritualist, GetPlayerById(pc));
            }
            EnchantedPlayers[rit].Clear();
        }
    }
    private static void ConvertRole(PlayerControl killer, PlayerControl target)
    {
        if (!killer.Is(CustomRoles.Admired) && !killer.Is(CustomRoles.Recruit) && !killer.Is(CustomRoles.Charmed)
                && !killer.Is(CustomRoles.Infected) && !killer.Is(CustomRoles.Contagious) && !killer.Is(CustomRoles.Madmate)
               && CanBeConverted(target))
        {
            Logger.Info("Set converted: " + target.GetNameWithRole().RemoveHtmlTags() + " to " + CustomRoles.Enchanted.ToString(), "Ritualist Assign");
            target.RpcSetCustomRole(CustomRoles.Enchanted, false);
            killer.Notify(CustomRoles.Enchanted.GetColoredTextByRole(GetString("RitualistSuccessfullyRecruited")));
            target.Notify(CustomRoles.Enchanted.GetColoredTextByRole(GetString("BeRecruitedByRitualist")));
        }
        else if (killer.Is(CustomRoles.Admired) && Admirer.CanBeAdmired(target, killer))
        {
            Logger.Info("Set converted: " + target.GetNameWithRole().RemoveHtmlTags() + " to " + CustomRoles.Admired.ToString(), "Ritualist Assign");
            target.RpcSetCustomRole(CustomRoles.Admired, false);
            killer.Notify(CustomRoles.Admired.GetColoredTextByRole(GetString("RitualistSuccessfullyRecruited")));
            target.Notify(CustomRoles.Admired.GetColoredTextByRole(GetString("BeRecruitedByRitualist")));
            Admirer.AdmiredList[killer.PlayerId].Add(target.PlayerId);
            Admirer.SendRPC(killer.PlayerId, target.PlayerId);
        }
        else if (killer.Is(CustomRoles.Recruit) && Jackal.CanBeSidekick(target))
        {
            Logger.Info("Set converted: " + target.GetNameWithRole().RemoveHtmlTags() + " to " + CustomRoles.Recruit.ToString(), "Ritualist Assign");
            target.RpcSetCustomRole(CustomRoles.Recruit, false);
            killer.Notify(CustomRoles.Recruit.GetColoredTextByRole(GetString("RitualistSuccessfullyRecruited")));
            target.Notify(CustomRoles.Recruit.GetColoredTextByRole(GetString("BeRecruitedByRitualist")));
        }
        else if (killer.Is(CustomRoles.Madmate) && target.CanBeMadmate(forAdmirer: true))
        {
            Logger.Info("Set converted: " + target.GetNameWithRole().RemoveHtmlTags() + " to " + CustomRoles.Madmate.ToString(), "Ritualist Assign");
            target.RpcSetCustomRole(CustomRoles.Madmate, false);
            killer.Notify(CustomRoles.Madmate.GetColoredTextByRole(GetString("RitualistSuccessfullyRecruited")));
            target.Notify(CustomRoles.Madmate.GetColoredTextByRole(GetString("BeRecruitedByRitualist")));
        }
        else if (killer.Is(CustomRoles.Charmed) && Cultist.CanBeCharmed(target))
        {
            Logger.Info("Set converted: " + target.GetNameWithRole().RemoveHtmlTags() + " to " + CustomRoles.Charmed.ToString(), "Ritualist Assign");
            target.RpcSetCustomRole(CustomRoles.Charmed, false);
            killer.Notify(CustomRoles.Charmed.GetColoredTextByRole(GetString("RitualistSuccessfullyRecruited")));
            target.Notify(CustomRoles.Charmed.GetColoredTextByRole(GetString("BeRecruitedByRitualist")));
        }
        else if (killer.Is(CustomRoles.Infected) && Infectious.CanBeBitten(target))
        {
            Logger.Info("Set converted: " + target.GetNameWithRole().RemoveHtmlTags() + " to " + CustomRoles.Infected.ToString(), "Ritualist Assign");
            target.RpcSetCustomRole(CustomRoles.Infected, false);
            killer.Notify(CustomRoles.Infected.GetColoredTextByRole(GetString("RitualistSuccessfullyRecruited")));
            target.Notify(CustomRoles.Infected.GetColoredTextByRole(GetString("BeRecruitedByRitualist")));
        }
        else if (killer.Is(CustomRoles.Contagious) && target.CanBeInfected())
        {
            Logger.Info("Set converted: " + target.GetNameWithRole().RemoveHtmlTags() + " to " + CustomRoles.Contagious.ToString(), "Ritualist Assign");
            target.RpcSetCustomRole(CustomRoles.Contagious, false);
            killer.Notify(CustomRoles.Contagious.GetColoredTextByRole(GetString("RitualistSuccessfullyRecruited")));
            target.Notify(CustomRoles.Contagious.GetColoredTextByRole(GetString("BeRecruitedByRitualist")));
        }
    }
    private static bool MsgToPlayerAndRole(string msg, out byte id, out CustomRoles role, out string error)
    {
        if (msg.StartsWith("/")) msg = msg.Replace("/", string.Empty);

        Regex r = new("\\d+");
        MatchCollection mc = r.Matches(msg);
        string result = string.Empty;
        for (int i = 0; i < mc.Count; i++)
        {
            result += mc[i];//匹配结果是完整的数字，此处可以不做拼接的
        }

        if (int.TryParse(result, out int num))
        {
            id = Convert.ToByte(num);
        }
        else
        {
            id = byte.MaxValue;
            error = GetString("RitualistCommandHelp");
            role = new();
            return false;
        }

        PlayerControl target = Utils.GetPlayerById(id);
        if (target == null || target.Data.IsDead)
        {
            error = GetString("GuessNull");
            role = new();
            return false;
        }

        if (!ChatCommands.GetRoleByName(msg, out role))
        {
            error = GetString("RitualistCommandHelp");
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
    public static bool CanBeConverted(PlayerControl pc)
    {
        return pc != null && (!pc.GetCustomRole().IsCovenTeam() && !pc.IsTransformedNeutralApocalypse()) && !pc.Is(CustomRoles.Soulless) && !pc.Is(CustomRoles.Lovers) && !pc.Is(CustomRoles.Loyal)
            && !((pc.Is(CustomRoles.NiceMini) || pc.Is(CustomRoles.EvilMini)) && Mini.Age < 18)
            && !(pc.GetCustomSubRoles().Contains(CustomRoles.Hurried) && !Hurried.CanBeConverted.GetBool());
    }
}
