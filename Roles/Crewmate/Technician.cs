﻿using static TOHE.Options;
using static TOHE.Utils;
using TOHE.Roles.Neutral;

namespace TOHE.Roles.Crewmate;

internal class Technician : RoleBase
{
    //===========================SETUP================================\\
    private const int Id = 29600;
    private static readonly HashSet<byte> playerIdList = [];
    public static bool HasEnabled => playerIdList.Any();
    
    public override CustomRoles ThisRoleBase => CustomRoles.Crewmate;
    public override Custom_RoleType ThisRoleType => Custom_RoleType.CrewmatePower;
    //==================================================================\\

    private static OptionItem SeeAllIDs;
    private static OptionItem SeePlayerInteractions;

    public override void SetupCustomOption()
    {
        SetupRoleOptions(Id, TabGroup.CrewmateRoles, CustomRoles.Technician);
        SeeAllIDs = BooleanOptionItem.Create(Id + 2, "TechnicianSeeAllIds", false, TabGroup.CrewmateRoles, false).SetParent(Options.CustomRoleSpawnChances[CustomRoles.Technician]);
        SeePlayerInteractions = BooleanOptionItem.Create(Id + 3, "TechnicianSeePlayerInteractions", false, TabGroup.CrewmateRoles, false).SetParent(Options.CustomRoleSpawnChances[CustomRoles.Technician]);
    }
    
    public override void Init()
    {
        playerIdList.Clear();
    }
    public override void Add(byte playerId)
    {
        playerIdList.Add(playerId);
    }

    public override string GetMark(PlayerControl seer, PlayerControl seen, bool isForMeeting = false)
    {
        if (!seer.IsAlive() || (!SeeAllIDs.GetBool() || !seen.IsAlive())) return string.Empty;
        
        if (Doppelganger.CheckDoppelVictim(seen.PlayerId))
            seen = Doppelganger.GetDoppelControl(seen);
        {
        return ColorString(GetRoleColor(CustomRoles.Technician), $" {seen.Data.PlayerId}");
        }
        
    }
    public static bool ActivateGuardAnimation(byte killerId, PlayerControl target, int colorId)
    {
        foreach (var technicianId in playerIdList.ToArray())
        {   
            if (SeePlayerInteractions.GetBool())
            {
                if (SeePlayerInteractions.GetBool())
                if (technicianId == killerId) continue;
                var technician = Utils.GetPlayerById(technicianId);
                if (technician == null) continue;
            

                technician.RpcGuardAndKill(target, colorId, true);
            }
        }
        return false;
    }
}
