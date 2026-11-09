using HarmonyLib;
using Il2CppSystem.Numerics;

namespace TOHO;

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CheckMurder))]
class CheckMurderPatch
{
    public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target)
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

        if (c) Main.RealKillers[target.PlayerId] = __instance.PlayerId;
        
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
        var a = __instance.GetCustomRole().OnShapeshift(__instance);

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

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.ReportDeadBody))]
class ReportDeadBodyPatch
{
    public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] NetworkedPlayerInfo target)
    {
        var a = __instance.GetCustomRole().OnReportDeadBody(__instance, target.Object);

        var b = target.Object.GetCustomRole().OnReportDeadBodyAsTarget(__instance, target.Object);
        if (!a) return false;
        if (!b) return false;

        var c = true;
        foreach (var spect in PlayerControl.AllPlayerControls)
        {
            var d = spect.GetCustomRole().OnReportDeadBodyAsSpect(__instance, target.Object);
            if (!d) c = false;
        }

        return c;
    }
}
[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.StartMeeting))]
class CallMeetingPatch
{
    public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] NetworkedPlayerInfo target)
    {
        var a = __instance.GetCustomRole().OnCallMeeting(__instance);

        if (!a) return false;

        var c = true;
        foreach (var spect in PlayerControl.AllPlayerControls)
        {
            var d = spect.GetCustomRole().OnCallMeetingAsSpect(__instance);
            if (!d) c = false;
        }

        return c;
    }
}
[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CheckVanish))]
class VanishPatch
{
    public static bool Prefix(PlayerControl __instance)
    {
        var a = __instance.GetCustomRole().OnVanish(__instance);

        if (!a) return false;

        var c = true;
        foreach (var spect in PlayerControl.AllPlayerControls)
        {
            var d = spect.GetCustomRole().OnVanishAsSpect(__instance);
            if (!d) c = false;
        }

        return c;
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CheckAppear))]
class AppearPatch
{
    public static bool Prefix(PlayerControl __instance)
    {
        var a = __instance.GetCustomRole().OnAppear(__instance);

        if (!a) return false;

        var c = true;
        foreach (var spect in PlayerControl.AllPlayerControls)
        {
            var d = spect.GetCustomRole().OnAppearAsSpect(__instance);
            if (!d) c = false;
        }

        return c;
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CompleteTask))]
class CompleteTaskPatch
{
    public static bool Prefix(PlayerControl __instance)
    {
        var a = __instance.GetCustomRole().OnCompleteTask(__instance);

        if (!a) return false;

        var c = true;
        foreach (var spect in PlayerControl.AllPlayerControls)
        {
            var d = spect.GetCustomRole().OnCompleteTaskAsSpect(__instance);
            if (!d) c = false;
        }

        return c;
    }
}

[HarmonyPatch(typeof(Vent), nameof(Vent.EnterVent))]
class EnterVentPatch
{
    public static bool Prefix(Vent __instance, [HarmonyArgument(0)] PlayerControl pc)
    {
        var a = pc.GetCustomRole().OnEnterVent(pc);

        if (!a) return false;

        var c = true;
        foreach (var spect in PlayerControl.AllPlayerControls)
        {
            var d = spect.GetCustomRole().OnEnterVentAsSpect(pc);
            if (!d) c = false;
        }

        return c;
    }
}