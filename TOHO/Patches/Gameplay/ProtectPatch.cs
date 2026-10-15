using HarmonyLib;
using Il2CppSystem.Numerics;

namespace TOHO;

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CheckProtect))]
class CheckProtectPatch
{
    public static bool Prefix(PlayerControl __instance, PlayerControl target)
    {
        var a = target.GetCustomRole().OnProtectAsTarget(__instance, target);
        var b = __instance.GetCustomRole().OnProtectAsKiller(__instance, target);

        if (!a || !b) return false;

        var c = true;
        foreach (var spect in PlayerControl.AllPlayerControls)
        {
            var d = spect.GetCustomRole().OnProtectAsSpect(__instance, target);
            if (!d) c = false;
        }

        return c;
    }

}