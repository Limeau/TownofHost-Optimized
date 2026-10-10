using System;
using System.Collections.Generic;
using System.Linq;
using Hazel;
using LibCpp2IL;

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
        Main.CustomRoles.Clear();
        
        Crewmates.Clear();
        Impostors.Clear();
        NKs.Clear();
        NNKs.Clear();
        Covens.Clear();
        ThisGameRoles.Clear();
        foreach (var kvp in Main.AllRoles.Where(x => x.Value.Enabled))
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
        
        // logs here
        
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
        
        // does not log here
        
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
        
        try
        {
            foreach (var player in PlayerControl.AllPlayerControls)
            {
                var role = ThisGameRoles[srandom.Next(ThisGameRoles.Count)];
                Main.CustomRoles.Add(player, role);
                ThisGameRoles.Remove(role);
            }
        }
        catch (Exception e)
        {
            Main.Logger.LogInfo(e);
        }

        new LateTask(() => { SetRoleBasisOfPlayers(); }, 5f, "Set Initial Role Basis");
    }

    public static void SetRoleBasisOfPlayers()
    {
        foreach (var kvp in Main.CustomRoles)
        {
            var player = kvp.Key;
            var role = kvp.Value;
            
            RoleManager.Instance.SetRole(player, role.Basis);
        }
    }
}