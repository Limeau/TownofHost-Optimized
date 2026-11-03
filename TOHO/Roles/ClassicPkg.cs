using System.Collections.Generic;
using AmongUs.GameOptions;
using TOHO;
using UnityEngine.Events;

public static class ClassicPkg
{
    public static int VectorCount;
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
        
        new CustomRoles("Limeau.Vector", "Vector", "Vent enough times to win",
                "The Vector wins by venting 50 times.", "#E38214", RoleTypes.Engineer, RoleCategories.NeutralEvil)
            .AddOnEnterVent((UnityAction<PlayerControl>)((PlayerControl a) =>
            {
                if (VectorCount < 50)
                {
                    VectorCount++;
                }
                else
                {
                    List<PlayerControl> win = [];
                    win.Add(a);
                    VectorCount = 0;
                    CustomRoleExtentions.WinTheGame(Custom_Team.Neutral, win);
                }
            }), true);
    }
}