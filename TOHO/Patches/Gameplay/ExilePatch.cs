using System;
using HarmonyLib;

namespace TOHO;

class ExileControllerWrapUpPatch
{
    [HarmonyPatch(typeof(ExileController), nameof(ExileController.WrapUp))]
    class BaseExileControllerPatch
    {
        public static void Postfix(ExileController __instance)
        {
            var pc = __instance.initData.networkedPlayer.PlayerId.GetPlayerById();
            pc.RpcSetName(Main.PlayerNames[pc]);
            pc.SetNameWithRole();
            foreach (var player in PlayerControl.AllPlayerControls)
            {
                if (player == pc) player.GetCustomRole().OnEjectAsTarget(pc);
                player.GetCustomRole().OnEjectAsSpect(pc);
            }
        }
    }
}