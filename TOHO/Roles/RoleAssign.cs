using System;
using System.Collections.Generic;
using System.Linq;

namespace TOHO;

public static class RoleAssign
{
    public static List<CustomRoles> Crewmates = [];
    public static List<CustomRoles> Impostors = [];
    public static List<CustomRoles> NKs = [];
    public static List<CustomRoles> NNKs = [];
    public static List<CustomRoles> Covens = [];
    
    public static List<CustomRoles> ThisGameRoles = [];
    public static void AssignRoles()
    {
        foreach (var kvp in Main.AllRoles.Where(x => x.Value.Option.Value))
        {
            var role = kvp.Value;
            switch (role.GetCustomRoleTeamFromRole())
            {
                case Custom_Team.Crewmate:
                    Crewmates.Add(role);
                    break;
                case Custom_Team.Impostor:
                    Impostors.Add(role);
                    break;
                case Custom_Team.Neutral:
                    if (role.Category == RoleCategories.NeutralKilling) NKs.Add(role);
                    else NNKs.Add(role);
                    break;
                case Custom_Team.Coven:
                    Covens.Add(role);
                    break;
            }
        }

        var srandom = new Random();
        
        int impCount = 0;
        while (impCount < Options.NumImpostors.Value && Impostors.Count > 0)
        {
            var i = srandom.Next(Impostors.Count);
            ThisGameRoles.Add(Impostors[i]);
            Impostors.Remove(Impostors[i]);
            impCount++;
        }
        int nkCount = 0;
        while (nkCount < Options.NumNeutralKillers.Value && NKs.Count > 0)
        {
            var i = srandom.Next(NKs.Count);
            ThisGameRoles.Add(NKs[i]);
            NKs.Remove(NKs[i]);
            nkCount++;
        }
        int nnkCount = 0;
        while (nnkCount < Options.NumNeutralNonKillers.Value && NNKs.Count > 0)
        {
            var i = srandom.Next(NNKs.Count);
            ThisGameRoles.Add(NNKs[i]);
            NNKs.Remove(NNKs[i]);
            nnkCount++;
        }
        int covCount = 0;
        while (covCount < Options.NumCoven.Value && Covens.Count > 0)
        {
            var i = srandom.Next(Covens.Count);
            ThisGameRoles.Add(Covens[i]);
            Covens.Remove(Covens[i]);
            covCount++;
        }

        var togo = PlayerControl.AllPlayerControls.Count - (covCount + impCount + nnkCount + nkCount);

        while (togo > 0 && Crewmates.Count > 0)
        {
            var i = srandom.Next(Crewmates.Count);
            ThisGameRoles.Add(Crewmates[i]);
            Crewmates.Remove(Crewmates[i]);
            togo--;
        }
        
        while (togo > 0)
        {
            ThisGameRoles.Add(Main.AllRoles["Crewmate"]);
            togo--;
        }

        foreach (var player in PlayerControl.AllPlayerControls)
        {
            var role = ThisGameRoles[srandom.Next(ThisGameRoles.Count)];
            Main.CustomRoles.Add(player, role);
            ThisGameRoles.Remove(role);
        }
    }
}