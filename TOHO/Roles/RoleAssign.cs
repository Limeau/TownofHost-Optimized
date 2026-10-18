using System;
using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using Hazel;

namespace TOHO;

public static class RoleAssign
{
    public static bool RoleSet = false;
    
    public static List<CustomRoles> Crewmates = [];
    public static List<CustomRoles> Impostors = [];
    public static List<CustomRoles> NKs = [];
    public static List<CustomRoles> NNKs = [];
    public static List<CustomRoles> Covens = [];
    
    public static List<CustomRoles> ThisGameRoles = [];
    public static void AssignRoles()
    {
        RoleSet = true;
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
                    if (role.Category != RoleCategories.CrewmateVanillaGhost && role.Category != RoleCategories.CrewmateGhosts) Crewmates.Add(role);
                    break;
                case Custom_Team.Impostor:
                    if (role.Category != RoleCategories.ImpostorVanillaGhosts && role.Category != RoleCategories.ImpostorGhosts) Impostors.Add(role);
                    break;
                case Custom_Team.Neutral:
                    if (role.Category == RoleCategories.NeutralKilling) NKs.Add(role);
                    else if (role.Category != RoleCategories.NeutralGhosts) NNKs.Add(role);
                    break;
                case Custom_Team.Coven:
                    if (role.Category != RoleCategories.CovenGhosts) Covens.Add(role);
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
        
        GhostRoleAssign.AssignGhostRoles();
    }
}

public static class GhostRoleAssign
{
    
    public static List<CustomRoles> Crewmates = [];
    public static List<CustomRoles> Impostors = [];
    public static List<CustomRoles> Neutrals = [];
    public static List<CustomRoles> Covens = [];
    public static void AssignGhostRoles()
    {
        Crewmates.Clear();
        Impostors.Clear();
        Neutrals.Clear();
        Covens.Clear();
        
        Main.GhostRoles.Clear();
        
        foreach (var kvp in Main.AllRoles.Where(x => x.Value.Enabled))
        {
            var role = kvp.Value;
            switch (role.GetCustomRoleTeamFromRole())
            {
                case Custom_Team.Crewmate:
                    if (role.Category == RoleCategories.CrewmateVanillaGhost || role.Category == RoleCategories.CrewmateGhosts) Crewmates.Add(role);
                    break;
                case Custom_Team.Impostor:
                    if (role.Category == RoleCategories.ImpostorVanillaGhosts || role.Category == RoleCategories.ImpostorGhosts) Impostors.Add(role);
                    break;
                case Custom_Team.Neutral:
                    if (role.Category == RoleCategories.NeutralGhosts) Neutrals.Add(role);
                    break;
                case Custom_Team.Coven:
                    if (role.Category == RoleCategories.CovenGhosts) Covens.Add(role);
                    break;
            }
        }

        var srandom = new Random();

        foreach (var kvp in Main.CustomRoles)
        {
            switch (kvp.Value.GetCustomRoleTeamFromRole())
            {
                case Custom_Team.Crewmate:
                    if (!Crewmates.Any())
                    {
                        var ncm = Main.AllRoles["Crewmate Ghost"];
                        Main.GhostRoles.Add(kvp.Key, ncm);
                        break;
                    }
                    var i = srandom.Next(Crewmates.Count);
                    var cm = Crewmates[i];
                    Crewmates.Remove(cm);
                    Main.GhostRoles.Add(kvp.Key, cm);
                    break;
                case Custom_Team.Impostor:
                    if (!Impostors.Any())
                    {
                        var ncm = Main.AllRoles["Impostor Ghost"];
                        Main.GhostRoles.Add(kvp.Key, ncm);
                        break;
                    }
                    var n = srandom.Next(Impostors.Count);
                    var im = Impostors[n];
                    Impostors.Remove(im); 
                    Main.GhostRoles.Add(kvp.Key, im);
                    break;
                case Custom_Team.Neutral:
                    if (!Neutrals.Any())
                    {
                        var ncm = Main.AllRoles["Neutral Ghost"];
                        Main.GhostRoles.Add(kvp.Key, ncm);
                        break;
                    }
                    var x = srandom.Next(Neutrals.Count);
                    var nt = Neutrals[x]; 
                    Neutrals.Remove(nt);                    
                    Main.GhostRoles.Add(kvp.Key, nt);
                    break;
                case Custom_Team.Coven:
                    if (!Covens.Any())
                    {
                        var ncm = Main.AllRoles["Coven Ghost"];
                        Main.GhostRoles.Add(kvp.Key, ncm);
                        break;
                    }
                    var y = srandom.Next(Covens.Count);
                    var cv = Covens[y]; 
                    Covens.Remove(cv);               
                    Main.GhostRoles.Add(kvp.Key, cv);
                    break;
            }
        }
    }
}