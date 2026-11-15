using UnityEngine;
using System.Linq;
using HarmonyLib;
using InnerNet;
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

[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.CastVote))]
class CastVotePatch
{
    public static bool Prefix(MeetingHud __instance, PlayerId srcPlayerId, PlayerId suspectPlayerId)
    {
        var target = Utils.GetPlayerById(suspectPlayerId);
        var voter = Utils.GetPlayerById(srcPlayerId);
        
        if (target == null || voter == null) return true;
        
        var a = target.GetCustomRole().OnVoteAsTarget(voter, target);
        var b = voter.GetCustomRole().OnVote(voter, target);

        foreach (var modifier in Main.Modifiers[voter.PlayerId])
        {
            var xa = modifier.OnVote(voter, target);
            if (!xa && a) a = false;
        }
        foreach (var modifier in Main.Modifiers[target.PlayerId])
        {
            var xa = modifier.OnVoteAsTarget(voter, target);
            if (!xa && a) a = false;
        }
        
        if (!a || !b) return false;

        var c = true;
        foreach (var spect in PlayerControl.AllPlayerControls)
        {
            var d = spect.GetCustomRole().OnVoteAsSpect(voter, target);
            if (!d) c = false;
            
            foreach (var modifier in Main.Modifiers[spect.PlayerId])
            {
                var xc = modifier.OnVoteAsSpect(voter, target);
                if (!xc && c) c = false;
            }
        }

        return c;
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
                name += $"{Main.PlayerNames[player.PlayerId]} was a {player.GetCustomRole().GetCustomRoleTeamFromRole()}";
                break;
            
            case 1:
                name += $"{Main.PlayerNames[player.PlayerId]} was the {player.GetCustomRole().RoleName}";
                break;
            
            case 2:
                name += $"{Main.PlayerNames[player.PlayerId]} was ejected";
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