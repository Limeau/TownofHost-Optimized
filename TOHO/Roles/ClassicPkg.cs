using System.Collections.Generic;
using AmongUs.GameOptions;
using TOHO;
using UnityEngine.Events;

public static class ClassicPkg
{
    public static NumberOptionItem SheriffCooldown;
    
    public static NumberOptionItem VampireCooldown;
    public static NumberOptionItem VampireTimer;
    
    public static int VectorCount;
    public static NumberOptionItem VectorVentsRequired;
    public static NumberOptionItem VectorCooldown;
    public static void SetupCustomRoles()
    {
        var sheriff = new CustomRoles("Limeau.Sheriff", "Sheriff", "Kill evils as a Crewmate", "The Sheriff can use their kill button to kill a player. However, if the Sheriff tries to kill a Crewmate, they will die instead.", "#f5bf42", RoleTypes.Impostor, RoleCategories.CrewmateKilling);
        SheriffCooldown = new NumberOptionItem("Limeau.Sheriff.Cooldown", "Kill cooldown", OptionTabs.RoleSetting, 20f, 1f, 60f, 1f);
        sheriff.SetupAdvancedNumberOption(SheriffCooldown);
        sheriff.AddOnGameStart((UnityAction<PlayerControl>)((PlayerControl a) =>
        {
            a.SetKillCooldown(SheriffCooldown.Value);
        }));
        sheriff.AddOnMurder((UnityAction<PlayerControl, PlayerControl>)((PlayerControl killer, PlayerControl target) =>
        {
            Main.Logger.LogInfo($"Target: {target.Data.PlayerName}");
            
            var targetRole = Main.CustomRoles[target.PlayerId];

            Main.Logger.LogInfo($"Role: {targetRole.RoleName}");
            Main.Logger.LogInfo($"Category: {targetRole.Category}");

            if (targetRole.Category == RoleCategories.CrewmateVanilla ||
                targetRole.Category == RoleCategories.CrewmateVanillaGhost ||
                targetRole.Category == RoleCategories.CrewmateHindering ||
                targetRole.Category == RoleCategories.CrewmateInvestigative ||
                targetRole.Category == RoleCategories.CrewmateKilling ||
                targetRole.Category == RoleCategories.CrewmatePower ||
                targetRole.Category == RoleCategories.CrewmateSupport ||
                targetRole.Category == RoleCategories.CrewmateGhosts)
            {
                killer.MurderPlayer(killer);
            }
            else
            {
                killer.MurderPlayer(target);
            }
        }), isKiller: true, allow: false);
        
        var doctor = new CustomRoles("Limeau.Doctor", "Doctor", "See which role killed players", "The Doctor can see the role that dead players were killed by.", "#32a89d", RoleTypes.Scientist, RoleCategories.CrewmateInvestigative);
        doctor.AddOnCallMeeting((UnityAction<PlayerControl>)((PlayerControl seer) =>
        {
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (pc.Data.IsDead)
                {
                    var s = Main.PlayerNames[pc.PlayerId];
                    s += "\n<size=70%>";
                    if (Main.RealKillers.ContainsKey(pc.PlayerId))
                    {
                        var role = Utils.GetPlayerById(Main.RealKillers[pc.PlayerId]).GetCustomRole();
                        s += $"<color={role.RoleColor}>";
                        s += role.RoleName;
                        s += "</color>";
                    }
                    else s += "(No killer)";
                    s += "</size>";
                    seer.RpcSetNameSpecific(pc, s);
                }
            }
        }));
        doctor.AddOnAfterMeeting((UnityAction<PlayerControl>)((PlayerControl seer) =>
        {
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (pc.Data.IsDead)
                {
                    seer.RpcSetNameSpecific(pc, Main.PlayerNames[pc.PlayerId]);
                }
            }
        }));

        VampireCooldown = new NumberOptionItem("Limeau.Vampire.Cooldown", "Kill cooldown", OptionTabs.RoleSetting, 20f, 1f, 60f, 1f);
        VampireTimer = new NumberOptionItem("Limeau.Vampire.Timer", "Delay Timer", OptionTabs.RoleSetting, 10f, 1f, 20f, 1f);
        var vampire = new CustomRoles("Limeau.Vampire", "Vampire", "Kill with a delay", "When the Vampire kills, their kill will be delayed by a certain amount of time before it takes place.", "#ff1919", RoleTypes.Impostor, RoleCategories.ImpostorKilling);
        vampire.AddOnMurder((UnityAction<PlayerControl, PlayerControl>)((PlayerControl killer, PlayerControl target) =>
        {
            killer.ProtectPlayer(killer, killer.CurrentOutfit.ColorId);
            killer.SetKillCooldown(VampireCooldown.Value);
            _ = new LateTask(() =>
            {
                target.MurderPlayer(target);
                Main.RealKillers[target.PlayerId] = killer.PlayerId;
            }, VampireTimer.Value, "Vampire Kill");
        }), isKiller: true, allow: false);
        
        var jester = new CustomRoles("Limeau.Jester", "Jester", "Get ejected to win",
                "The Jester wins by being ejected from a meeting.", "#ff3385", RoleTypes.Crewmate, RoleCategories.NeutralEvil);
        jester.AddOnEject((UnityAction<PlayerControl>)((PlayerControl a) =>
            {
                List<PlayerControl> win = [];
                win.Add(a);
                CustomRoleExtentions.WinTheGame(win);
            }), true);
        
        var vector = new CustomRoles("Limeau.Vector", "Vector", "Vent enough times to win",
                "The Vector wins by venting a certain number times.", "#E38214", RoleTypes.Engineer, RoleCategories.NeutralEvil);
        VectorVentsRequired = new NumberOptionItem("Limeau.Vector.VentsRequired", "Vents required to win", OptionTabs.RoleSetting, 50f, 5f, 100f, 5f);
        VectorCooldown = new NumberOptionItem("Limeau.Vector.Cooldown", "Vent cooldown", OptionTabs.RoleSetting, 1f, 0f, 10f, 1f);
        vector.SetupAdvancedNumberOption(VectorVentsRequired);
        vector.SetupAdvancedNumberOption(VectorCooldown);
        vector.AddOnEnterVent((UnityAction<PlayerControl>)((PlayerControl a) =>
            {
                VectorCount++;

                if (VectorCount >= VectorVentsRequired.Value)
                {
                    List<PlayerControl> win = [];
                    win.Add(a);
                    VectorCount = 0;
                    CustomRoleExtentions.WinTheGame(win);
                }
            }), true);
        vector.AddOnGameStart((UnityAction<PlayerControl>)((PlayerControl a) =>
        {
            a.SetEngineerCooldown(VectorCooldown.Value);
        }));
        
        var bait = new CustomRoles("Limeau.Bait", "Bait", "Have your body instantly reported when someone kills you",
            "When the Bait is killed, the killer will instantly self report.", "#00ffff", RoleTypes.Crewmate, RoleCategories.Modifier);

    }
}