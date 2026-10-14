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
    public static List<PlayerControl> Winners = [];
    public static Custom_Team WinnerTeam;
    public static bool IsGameEnd = false;
    public static bool Prefix()
    {
        if (!AmongUsClient.Instance.AmHost) return true;

        if (Options.NoGameEnd.Value) return false;
        if (!IsGameEnd) return false;

        foreach (var kvp in Main.PlayerNames)
        {
            kvp.Key.RpcSetNameLocal(kvp.Value);
        }
        Main.PlayerNames.Clear();
        
        IsGameEnd = false;

        return true;
    }
}