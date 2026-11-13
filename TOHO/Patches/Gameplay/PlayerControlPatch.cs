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

        foreach (var modifier in Main.Modifiers[__instance.PlayerId])
        {
            var xa = modifier.OnMurderAsKiller(__instance, target);
            if (!xa && a) a = false;
        }
        foreach (var modifier in Main.Modifiers[target.PlayerId])
        {
            var xa = modifier.OnMurderAsTarget(__instance, target);
            if (!xa && a) a = false;
        }
        
        if (!a || !b) return false;

        var c = true;
        foreach (var spect in PlayerControl.AllPlayerControls)
        {
            var d = spect.GetCustomRole().OnMurderAsSpect(__instance, target);
            if (!d) c = false;
            
            foreach (var modifier in Main.Modifiers[spect.PlayerId])
            {
                var xc = modifier.OnMurderAsSpect(__instance, target);
                if (!xc && c) c = false;
            }
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

        foreach (var modifier in Main.Modifiers[__instance.PlayerId])
        {
            var xa = modifier.OnProtectAsKiller(__instance, target);
            if (!xa && a) a = false;
        }
        foreach (var modifier in Main.Modifiers[target.PlayerId])
        {
            var xa = modifier.OnProtectAsTarget(__instance, target);
            if (!xa && a) a = false;
        }
        
        if (!a || !b) return false;

        var c = true;
        foreach (var spect in PlayerControl.AllPlayerControls)
        {
            var d = spect.GetCustomRole().OnProtectAsSpect(__instance, target);
            if (!d) c = false;
            
            foreach (var modifier in Main.Modifiers[spect.PlayerId])
            {
                var xc = modifier.OnProtectAsSpect(__instance, target);
                if (!xc && c) c = false;
            }
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
        
        foreach (var modifier in Main.Modifiers[__instance.PlayerId])
        {
            var xa = modifier.OnShapeshift(__instance);
            if (!xa && a) a = false;
        }
        
        if (!a) return false;

        var c = true;
        foreach (var spect in PlayerControl.AllPlayerControls)
        {
            var d = spect.GetCustomRole().OnShapeshiftAsSpect(__instance);
            if (!d) c = false;
            
            foreach (var modifier in Main.Modifiers[spect.PlayerId])
            {
                var xc = modifier.OnShapeshiftAsSpect(__instance);
                if (!xc && c) c = false;
            }
        }

        return c;
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.ReportDeadBody))]
class ReportDeadBodyPatch
{
    public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] NetworkedPlayerInfo target)
    {
        if (target == null) return true;
        
        var a = __instance.GetCustomRole().OnReportDeadBody(__instance, target.Object);
        var b = target.Object.GetCustomRole().OnReportDeadBodyAsTarget(__instance, target.Object);
        
        foreach (var modifier in Main.Modifiers[__instance.PlayerId])
        {
            var xa = modifier.OnReportDeadBody(__instance, target.Object);
            if (!xa && a) a = false;
        }
        foreach (var modifier in Main.Modifiers[target.PlayerId])
        {
            var xa = modifier.OnReportDeadBodyAsTarget(__instance, target.Object);
            if (!xa && a) a = false;
        }
        
        if (!a) return false;
        if (!b) return false;

        var c = true;
        foreach (var spect in PlayerControl.AllPlayerControls)
        {
            var d = spect.GetCustomRole().OnReportDeadBodyAsSpect(__instance, target.Object);
            if (!d) c = false;
            
            foreach (var modifier in Main.Modifiers[spect.PlayerId])
            {
                var xc = modifier.OnReportDeadBodyAsSpect(__instance, target.Object);
                if (!xc && c) c = false;
            }
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

        foreach (var modifier in Main.Modifiers[__instance.PlayerId])
        {
            var xa = modifier.OnCallMeeting(__instance);
            if (!xa && a) a = false;
        }
        
        if (!a) return false;

        var c = true;
        foreach (var spect in PlayerControl.AllPlayerControls)
        {
            var d = spect.GetCustomRole().OnCallMeetingAsSpect(__instance);
            if (!d) c = false;
            
            foreach (var modifier in Main.Modifiers[spect.PlayerId])
            {
                var xc = modifier.OnCallMeetingAsSpect(__instance);
                if (!xc && c) c = false;
            }
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
        
        foreach (var modifier in Main.Modifiers[__instance.PlayerId])
        {
            var xa = modifier.OnVanish(__instance);
            if (!xa && a) a = false;
        }
        
        if (!a) return false;

        var c = true;
        foreach (var spect in PlayerControl.AllPlayerControls)
        {
            var d = spect.GetCustomRole().OnVanishAsSpect(__instance);
            if (!d) c = false;
            
            foreach (var modifier in Main.Modifiers[spect.PlayerId])
            {
                var xc = modifier.OnVanishAsSpect(__instance);
                if (!xc && c) c = false;
            }
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
        
        foreach (var modifier in Main.Modifiers[__instance.PlayerId])
        {
            var xa = modifier.OnAppear(__instance);
            if (!xa && a) a = false;
        }
        
        if (!a) return false;

        var c = true;
        foreach (var spect in PlayerControl.AllPlayerControls)
        {
            var d = spect.GetCustomRole().OnAppearAsSpect(__instance);
            if (!d) c = false;
            
            foreach (var modifier in Main.Modifiers[spect.PlayerId])
            {
                var xc = modifier.OnAppearAsSpect(__instance);
                if (!xc && c) c = false;
            }
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

        
        foreach (var modifier in Main.Modifiers[__instance.PlayerId])
        {
            var xa = modifier.OnCompleteTask(__instance);
            if (!xa && a) a = false;
        }
        
        if (!a) return false;

        var c = true;
        foreach (var spect in PlayerControl.AllPlayerControls)
        {
            var d = spect.GetCustomRole().OnCompleteTaskAsSpect(__instance);
            if (!d) c = false;
            
            foreach (var modifier in Main.Modifiers[spect.PlayerId])
            {
                var xc = modifier.OnCompleteTaskAsSpect(__instance);
                if (!xc && c) c = false;
            }
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
        
        foreach (var modifier in Main.Modifiers[pc.PlayerId])
        {
            var xa = modifier.OnEnterVent(pc);
            if (!xa && a) a = false;
        }
        
        if (!a) return false;

        var c = true;
        foreach (var spect in PlayerControl.AllPlayerControls)
        {
            var d = spect.GetCustomRole().OnEnterVentAsSpect(pc);
            if (!d) c = false;
            
            foreach (var modifier in Main.Modifiers[spect.PlayerId])
            {
                var xc = modifier.OnCompleteTaskAsSpect(pc);
                if (!xc && c) c = false;
            }
        }

        return c;
    }
}