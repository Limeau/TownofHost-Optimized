using HarmonyLib;
using Il2CppSystem.Numerics;

namespace TOHO;

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CheckMurder))]
class CheckMurderPatch
{
    public static bool Prefix(PlayerControl __instance, PlayerControl target)
    {
        var a = target.GetCustomRole().OnMurderAsTarget(__instance, target);
        var b = __instance.GetCustomRole().OnMurderAsKiller(__instance, target);

        if (!a || !b) return false;

        var c = true;
        foreach (var spect in PlayerControl.AllPlayerControls)
        {
            var d = spect.GetCustomRole().OnMurderAsSpect(__instance, target);
            if (!d) c = false;
        }

        return c;
    }
}
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
[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CheckShapeshift))]
class CheckShapeshiftPatch
{
    public static bool Prefix(PlayerControl __instance, PlayerControl target, bool shouldAnimate)
    {
        var a = target.GetCustomRole().OnShapeshift(__instance);

        if (!a) return false;

        var c = true;
        foreach (var spect in PlayerControl.AllPlayerControls)
        {
            var d = spect.GetCustomRole().OnShapeshiftAsSpect(__instance);
            if (!d) c = false;
        }

        return c;
    }
}