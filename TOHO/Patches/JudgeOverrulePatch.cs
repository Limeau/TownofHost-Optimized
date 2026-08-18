using HarmonyLib;
using System.Collections.Generic;
using InnerNet;

namespace TOHO.Patches;

[HarmonyPatch(typeof(JudgeRole), nameof(JudgeRole.TryOverrule))]
public class JudgeOverrulePatch
{
    public static void Prefix(PlayerId overruledPlayerId)
    {
        List<MeetingHud.VoterState> statesList = [];

        if (MeetingHud.Instance) MeetingHud.Instance.VotingComplete(statesList.ToArray(), Utils.GetPlayerInfoById(overruledPlayerId.Value), false, true, byte.MaxValue);
    }
}