using UnityEngine;
using System.Linq;
using HarmonyLib;
using TOHO;


[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start))]
class MeetingHudStartPatch
{
    public static void Postfix(MeetingHud __instance)
    {
        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (player.OwnerId != AmongUsClient.Instance.HostId) player.SetNameWithRole();
        }
    }
}


[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.CoIntro))]
class MeetingHudIntroPatch
{
    public static void Postfix(MeetingHud __instance)
    {
        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (player.OwnerId == AmongUsClient.Instance.HostId)
            {
                CustomRoles role = null;
                role = player.Data.IsDead ? player.GetGhostRole() : player.GetCustomRole();
                if (role != null)
                {
                    new LateTask(() =>
                    {
                        __instance.TitleText.SetText($"Your role is: <color={role.RoleColor}>{role.RoleName}</color>");
                    }, 10f);
                }
            }
        }
    }
}

[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.CheckForEndVoting))]
class CheckForEndVotingPatch
{
    public static bool Prefix(MeetingHud __instance)
    {
        if (!AmongUsClient.Instance.AmHost) return true;

        var votes = __instance.playerStates.Where(x => x != null && x.DidVote && x.VotedForId < 253).GroupBy(x => x.VotedForId).Select(g => new { PlayerId = g.Key, VoteCount = g.Count() }).OrderByDescending(x => x.VoteCount).ToList();

        if (votes.Count == 0) return true;

        var highest = votes[0].VoteCount;
        var winners = votes.Where(x => x.VoteCount == highest).ToList();

        if (winners.Count != 1)
        {
            return true;
        }

        byte exiledPlayerId = winners[0].PlayerId;
        var player = exiledPlayerId.GetPlayerById();
        
        var name = $"<size=75%>";
        
        switch (Options.ConfirmEjectionsMode.Value)
        {
            case 0:
                name += $"{Main.PlayerNames[player]} was a {player.GetCustomRole().GetCustomRoleTeamFromRole()}";
                break;
            
            case 1:
                name += $"{Main.PlayerNames[player]} was the {player.GetCustomRole().RoleName}";
                break;
            
            case 2:
                name += $"{Main.PlayerNames[player]} was ejected";
                break;
            
        }
        name += Options.EjectionImpostors.Value ? $"\n{Utils.GetImpostorCount()} Impostors remain." : "";
        name += Options.EjectionNeutralKillers.Value ? $"\n{Utils.GetNKcount()} Neutral Killers remain." : "";
        name += Options.EjectionCoven.Value ? $"\n{Utils.GetCovenCount()} Coven remain." : "";

        name += "<size=0%>";

        player.RpcSetName(name);
        
        return true;
    }
}