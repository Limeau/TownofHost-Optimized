using System.Collections.Generic;
using AmongUs.GameOptions;
using TOHO;
using UnityEngine.Events;

public static class ClassicPkg
{
    public static void SetupCustomRoles()
    {
        new CustomRoles("Limeau.Jester", "Jester", "Get ejected to win",
            "The Jester wins by being ejected from a meeting.", "#ff3385", RoleTypes.Crewmate, RoleCategories.NeutralEvil)
            .AddOnEject((UnityAction<PlayerControl>)((PlayerControl a) =>
            {
                List<PlayerControl> win = [];
                win.Add(a);
                CustomRoleExtentions.WinTheGame(Custom_Team.Neutral, win);
            }), true);
    }
}