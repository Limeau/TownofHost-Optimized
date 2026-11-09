using AmongUs.GameOptions;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using Hazel;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;

namespace TOHO;

[HarmonyPatch(typeof(GameManager), nameof(GameManager.CheckEndGameViaTasks))]
class CheckEndGameViaTasksForNormalPatch
{
    public static bool Prefix(ref bool __result)
    {
        __result = false;
        return false;
    }
}
[HarmonyPatch(typeof(GameManager), nameof(GameManager.CheckTaskCompletion))]
class CheckTaskCompletionPatch
{
    public static bool Prefix(ref bool __result)
    {
        if (Options.DisableTaskWin.Value)
        {
            __result = false;
            return false;
        }
        
        GameEndCheckerForNormal.Prefix();

        __result = false;
        return false;
    }
}
[HarmonyPatch(typeof(LogicGameFlowNormal), nameof(LogicGameFlowNormal.CheckEndCriteria))]
class GameEndCheckerForNormal
{
    public static List<string> Results = [];
    public static List<PlayerControl> Winners = [];
    public static bool IsGameEnd = false;
    public static bool Prefix()
    {
        if (!AmongUsClient.Instance.AmHost) return true;

        if (Options.NoGameEnd.Value) return false;

        var crewWin = true;
        var impWin = true;
        var covWin = true;
        var count = 0;
        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (!player.Data.IsDead)
            {
                count++;
                if (!player.IsPlayerCrewmate()) crewWin = false;
                if (!player.IsPlayerImpostor()) impWin = false;
                if (!player.IsPlayerCoven()) covWin = false;
            }
        }
        
        if (!IsGameEnd && !crewWin && !impWin && !covWin && count > 1) return false;

        foreach (var kvp in Main.PlayerNames)
        {
            Utils.GetPlayerById(kvp.Key).RpcSetNameToAll(kvp.Value);
        }
        
        IsGameEnd = false;

        if (crewWin)
        {
            foreach (var player in PlayerControl.AllPlayerControls)
            {
                if (player.IsPlayerCrewmate()) Winners.Add(player);
            }
        }
        if (impWin)
        {
            foreach (var player in PlayerControl.AllPlayerControls)
            {
                if (player.IsPlayerImpostor()) Winners.Add(player);
            }
        }
        if (covWin)
        {
            foreach (var player in PlayerControl.AllPlayerControls)
            {
                if (player.IsPlayerCoven()) Winners.Add(player);
            }
        }

        if (count == 1)
        {
            foreach (var player in PlayerControl.AllPlayerControls)
            {
                if (!player.Data.IsDead && player.IsPlayerNeutral()) Winners.Add(player);
            }
        }
        
        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (Winners.Contains(player))
            {
                var w = "★ ";
                w += Main.PlayerNames[player.PlayerId];
                w += " - ";
                w += $"<color={player.GetCustomRole().RoleColor}>";
                w += player.GetCustomRole().RoleName;
                w += "</color>";
                w += " - (";
                w += $"<color={player.GetGhostRole().RoleColor}>";
                w += player.GetGhostRole().RoleName;
                w += "</color>)";
                Results.Add(w);
                Main.GhostRoles[player.PlayerId] = Main.AllRoles["Impostor Ghost"];
                player.MurderPlayer(player);
            }
        }

        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (!Winners.Contains(player))
            {
                var w = "";
                w += Main.PlayerNames[player.PlayerId];
                w += " - ";
                w += $"<color={player.GetCustomRole().RoleColor}>";
                w += player.GetCustomRole().RoleName;
                w += "</color>";
                w += " - (";
                w += $"<color={player.GetGhostRole().RoleColor}>";
                w += player.GetGhostRole().RoleName;
                w += "</color>)";
                Results.Add(w);
                Main.GhostRoles[player.PlayerId] = Main.AllRoles["Crewmate Ghost"];
                player.MurderPlayer(player);
            }
        }

        Main.PlayerNames.Clear();

        RoleAssign.RoleSet = false;
        
        GameManager.Instance.RpcEndGame(GameOverReason.ImpostorsByKill, false);
        
        return true;
    }
}