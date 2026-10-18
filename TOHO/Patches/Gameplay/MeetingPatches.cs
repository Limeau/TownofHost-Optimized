using System.Linq;
using HarmonyLib;
using TOHO;

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
        
        player.RpcSetName($"{Main.PlayerNames[player]} was the {player.GetCustomRole().RoleName}");
        
        return true;
    }
}