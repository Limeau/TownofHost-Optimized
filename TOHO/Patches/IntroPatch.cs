using AmongUs.GameOptions;
using System.Reflection;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes;
using InnerNet;
using UnityEngine;

namespace TOHO;

[HarmonyPatch(typeof(HudManager), nameof(HudManager.CoShowIntro))]
class CoShowIntroPatch
{
    public static void Prefix()
    {
        if (!AmongUsClient.Instance.AmHost) return;

        _ = new LateTask(() =>
        {
            if (AmongUsClient.Instance.GameState == InnerNetClient.GameStates.Ended) return;

            DestroyableSingleton<HudManager>.Instance.SetHudActive(true);
        }, 0.6f, "Set Disconnected");

        _ = new LateTask(() =>
        {
            try
            {
                if (AmongUsClient.Instance.GameState == InnerNetClient.GameStates.Ended) return;
                // Assign tasks after assign all Roles, as it should be
                ShipStatus.Instance.Begin();
            }
            catch
            {
                Main.Logger.LogInfo($"Game ended? {AmongUsClient.Instance.GameState == InnerNetClient.GameStates.Ended}");
            }
        }, 4f, "Assign Task For All");
    }
}
[HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.CoBegin))]
class CoBeginPatch
{
    public static void Prefix()
    {
    }
}
[HarmonyPatch(typeof(IntroCutscene_ShowRole), "MoveNext")]
class SetUpRoleTextPatch
{
    public static bool IsInIntro = false;

    public static void Postfix(IntroCutscene_ShowRole __instance, ref bool __result)
    {
        if (__instance.__1__state == 1 && __result) // while wait for 2.5s
        {
            IntroCutscene introCutscene = __instance.__4__this;

            if (!AmongUsClient.Instance.AmHost) return;
            // After showing team for non-modded clients update player names.
            IsInIntro = false; 

            GameData.Instance.RecomputeTaskCounts();
            _ = new LateTask(() =>
            {
                PlayerControl localPlayer = PlayerControl.LocalPlayer;
                CustomRoles role = localPlayer.GetCustomRole();
                ColorUtility.TryParseHtmlString(role.RoleColor, out var c);
                introCutscene.YouAreText.color = c; 
                introCutscene.RoleText.text = role.RoleName; 
                introCutscene.RoleText.color = c; 
                introCutscene.RoleBlurbText.color = c; 
                introCutscene.RoleBlurbText.text = role.ShortDescription;
            }, 0.0001f, "Override Role Text");
        }
    }
}

[HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.BeginCrewmate))]
class BeginCrewmatePatch
{
    public static bool Prefix(IntroCutscene __instance,
        ref Il2CppSystem.Collections.Generic.List<PlayerControl> teamToDisplay)
    {
        var role = PlayerControl.LocalPlayer.GetCustomRole();

        if (PlayerControl.LocalPlayer.IsPlayerCoven())
        {
            var covTeam = new Il2CppSystem.Collections.Generic.List<PlayerControl>();
            covTeam.Add(PlayerControl.LocalPlayer);
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (pc.Data.IsDead) continue;

                if (pc.IsPlayerCoven() && pc != PlayerControl.LocalPlayer) covTeam.Add(pc);
            }

            teamToDisplay = covTeam;
        }

        if (PlayerControl.LocalPlayer.IsPlayerImpostor())
        {
            var impTeam = new Il2CppSystem.Collections.Generic.List<PlayerControl>();
            impTeam.Add(PlayerControl.LocalPlayer);
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (pc.Data.IsDead) continue;

                if (pc.IsPlayerImpostor() && pc != PlayerControl.LocalPlayer) impTeam.Add(pc);
            }

            teamToDisplay = impTeam;
        }
        else if (PlayerControl.LocalPlayer.IsPlayerNeutral())
        {
            teamToDisplay = new Il2CppSystem.Collections.Generic.List<PlayerControl>();
            teamToDisplay.Add(PlayerControl.LocalPlayer);
        }

        return true;
    }

    public static void Postfix(IntroCutscene __instance)
    {
        CustomRoles role = PlayerControl.LocalPlayer.GetCustomRole();

        __instance.ImpostorText.gameObject.SetActive(false);

        switch (PlayerControl.LocalPlayer.GetCustomRoleTeam())
        {
            case Custom_Team.Impostor:
                __instance.TeamTitle.text = "Impostor";
                __instance.TeamTitle.color =
                    __instance.BackgroundBar.material.color = new Color32(255, 25, 25, byte.MaxValue);
                PlayerControl.LocalPlayer.Data.Role.IntroSound = RoleManager.Instance.AllRoles
                    .Find((Il2CppSystem.Predicate<RoleBehaviour>)((roletype) => roletype.Role == RoleTypes.Impostor))
                    ?.IntroSound;
                __instance.ImpostorText.gameObject.SetActive(true);
                __instance.ImpostorText.text = "Sabotage and kill all Crewmates";
                break;
            case Custom_Team.Crewmate:
                __instance.TeamTitle.text = "Crewmate";
                __instance.TeamTitle.color =
                    __instance.BackgroundBar.material.color = new Color32(140, 255, 255, byte.MaxValue);
                PlayerControl.LocalPlayer.Data.Role.IntroSound = RoleManager.Instance.AllRoles
                    .Find((Il2CppSystem.Predicate<RoleBehaviour>)((roletype) => roletype.Role == RoleTypes.Crewmate))
                    ?.IntroSound;
                __instance.ImpostorText.gameObject.SetActive(true);
                __instance.ImpostorText.text = "Find and exile all Impostors";
                break;
            case Custom_Team.Neutral:
                __instance.TeamTitle.text = role.RoleName;
                ColorUtility.TryParseHtmlString(role.RoleColor, out var c);
                __instance.TeamTitle.color = __instance.BackgroundBar.material.color = c;
                PlayerControl.LocalPlayer.Data.Role.IntroSound = RoleManager.Instance.AllRoles
                    .Find((Il2CppSystem.Predicate<RoleBehaviour>)((roletype) =>
                        roletype.Role == RoleTypes.Shapeshifter))?.IntroSound;
                __instance.ImpostorText.gameObject.SetActive(true);
                __instance.ImpostorText.text = role.ShortDescription;
                break;
            case Custom_Team.Coven:
                __instance.TeamTitle.text = "Coven";
                __instance.TeamTitle.color =
                    __instance.BackgroundBar.material.color = new Color32(172, 66, 242, byte.MaxValue);
                PlayerControl.LocalPlayer.Data.Role.IntroSound = RoleManager.Instance.AllRoles
                    .Find((Il2CppSystem.Predicate<RoleBehaviour>)((roletype) => roletype.Role == RoleTypes.Phantom))
                    ?.IntroSound;
                __instance.ImpostorText.gameObject.SetActive(true);
                __instance.ImpostorText.text = "Win on your own with your fellow Coven";
                break;
        }
    }
}

[HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.BeginImpostor))]
class BeginImpostorPatch
{
    public static bool Prefix(IntroCutscene __instance, ref Il2CppSystem.Collections.Generic.List<PlayerControl> yourTeam)
    {
        var role = PlayerControl.LocalPlayer.GetCustomRole();

        if (PlayerControl.LocalPlayer.GetCustomRoleTeam() == Custom_Team.Crewmate)
        {
            yourTeam = new();
            yourTeam.Add(PlayerControl.LocalPlayer);
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (pc.AmOwner) continue;
                yourTeam.Add(pc);
            }
            __instance.BeginCrewmate(yourTeam);
            __instance.overlayHandle.color = Palette.CrewmateBlue;
            return false;
        }

        if (PlayerControl.LocalPlayer.GetCustomRoleTeam() == Custom_Team.Neutral)
        {
            yourTeam = new();
            yourTeam.Add(PlayerControl.LocalPlayer);
            __instance.BeginCrewmate(yourTeam);
            ColorUtility.TryParseHtmlString(role.RoleColor, out var c);
            __instance.overlayHandle.color = c;
            return false;
        }
        if (PlayerControl.LocalPlayer.GetCustomRoleTeam() == Custom_Team.Impostor)
        {
            yourTeam = new();
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (pc.AmOwner) continue;
                if (pc.GetCustomRoleTeam() == Custom_Team.Impostor) yourTeam.Add(pc);
            }
            yourTeam.Add(PlayerControl.LocalPlayer);
            __instance.BeginCrewmate(yourTeam);
            __instance.overlayHandle.color = Palette.ImpostorRed;
            return false;
        }
        
        if (PlayerControl.LocalPlayer.GetCustomRoleTeam() == Custom_Team.Coven)
        {
            yourTeam = new();
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (pc.AmOwner) continue;
                if (pc.GetCustomRoleTeam() == Custom_Team.Coven) yourTeam.Add(pc);
            }
            yourTeam.Add(PlayerControl.LocalPlayer);
            __instance.BeginCrewmate(yourTeam);
            __instance.overlayHandle.color = new Color32(172, 66, 242, byte.MaxValue);
            return false;
        }
        BeginCrewmatePatch.Prefix(__instance, ref yourTeam);
        return true;
    }

    public static void Postfix(IntroCutscene __instance)
    {
        BeginCrewmatePatch.Postfix(__instance);
    }
}
[HarmonyPatch]
class IntroCutsceneDestroyPatch
{
    public static MethodBase TargetMethod()
    {
        if (AccessTools.Method(typeof(IntroCutscene), "OnDestroy") is { } methodInfo)
        {
            return methodInfo;
        }

        return AccessTools.Method(typeof(IntroCutscene_CoBegin), "MoveNext");
    }

    public static void Prefix(Il2CppObjectBase __instance)
    {
        if (__instance.TryCast<IntroCutscene_CoBegin>() is { __1__state: >= 0 })
        {
            // return if using enumerator movenext and we are not at last state
            return;
        }
    }

    public static void Postfix(Il2CppObjectBase __instance)
    {
        if (__instance.TryCast<IntroCutscene_CoBegin>() is { __1__state: >= 0 })
        {
            // return if using enumerator movenext and we are not at last state
            return;
        }

        if (AmongUsClient.Instance.GameState != InnerNetClient.GameStates.Started) return;

        foreach (var pc in PlayerControl.AllPlayerControls)
        {
            pc.roleAssigned = false;
        }
    }
}