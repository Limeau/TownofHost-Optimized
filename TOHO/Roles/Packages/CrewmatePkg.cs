using System.Collections.Generic;
using AmongUs.GameOptions;
using TOHO;
using UnityEngine.Events;

public static class CrewmatePkg
{
    public static bool KnightHasKilled;
    public static void SetupCustomRoles()
    {        
        var guardian = new CustomRoles("Limeau.Guardian", "Guardian", "Protect yourself by doing your tasks", "The Guardian cannot be murdered when their tasks are completed.", "#16c421", RoleTypes.Crewmate, RoleCategories.CrewmatePower);
        guardian.AddOnMurder((UnityAction<PlayerControl, PlayerControl>)((PlayerControl killer, PlayerControl target) =>
        {
            if (!target.AllTasksCompleted())
            {
                killer.MurderPlayer(target);
            }
            killer.ResetKillCooldown();
        }), isTarget: true, allow: false);
        
        var knight = new CustomRoles("Limeau.Knight", "Knight", "You can kill, but only once", "The Knight can kill any player in the game, but they can only kill one time.", "#84918e", RoleTypes.Impostor, RoleCategories.CrewmatePower);
        knight.AddOnMurder((UnityAction<PlayerControl, PlayerControl>)((PlayerControl killer, PlayerControl target) =>
        {
            if (!KnightHasKilled)
            {
                killer.MurderPlayer(target);
                KnightHasKilled = true;
            }
        }), isKiller: true, allow: false);
    }
}