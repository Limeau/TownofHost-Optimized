using System.Collections.Generic;
using AmongUs.GameOptions;
using TOHO;
using UnityEngine.Events;

public static class ClassicPkg
{
    public static int VectorCount;
    public static NumberOptionItem VectorVentsRequired;
    public static NumberOptionItem VectorCooldown;
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
                    CustomRoleExtentions.WinTheGame(Custom_Team.Neutral, win);
                }
            }), true);
        vector.AddOnGameStart((UnityAction<PlayerControl>)((PlayerControl a) =>
        {
            a.SetEngineerCooldown(VectorCooldown.Value);
        }));
    }
}