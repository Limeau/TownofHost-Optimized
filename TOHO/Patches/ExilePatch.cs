using System;
using HarmonyLib;

namespace TOHO;

class ExileControllerWrapUpPatch
{
    public static NetworkedPlayerInfo AntiBlackout_LastExiled;
    [HarmonyPatch(typeof(ExileController), nameof(ExileController.WrapUp))]
    class BaseExileControllerPatch
    {
        public static void Postfix(ExileController __instance)
        {
            var pc = __instance.initData.networkedPlayer.PlayerId.GetPlayerById();

            foreach (var player in PlayerControl.AllPlayerControls)
            {
                if (player == pc) player.GetCustomRole().OnEjectAsTarget(pc);
                player.GetCustomRole().OnEjectAsSpect(pc);
            }
        }
    }
}