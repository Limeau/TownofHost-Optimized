using System;
using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using TOHO.Modules;
using TOHO.Roles.Core;
using TOHO.Roles.Impostor;
using UnityEngine.SocialPlatforms;
using static TOHO.Options;
namespace TOHO.Roles.Neutral;

internal class Developer : RoleBase
{
    //===========================SETUP================================\\
    private const int Id = 40000;
    public static bool HasEnabled => CustomRoleManager.HasEnabled(CustomRoles.Developer);
    public override CustomRoles Role => CustomRoles.Developer;
    public override CustomRoles ThisRoleBase => CustomRoles.Impostor;
    public override Custom_RoleType ThisRoleType => Custom_RoleType.NeutralBenign;
    //==================================================================\\

    public static OptionItem Deviator;
    public static OptionItem DeviatorShieldDuration;
    public static OptionItem DeviatorShieldCooldown;
    public static OptionItem Reckless;
    public static OptionItem RecklessChanceToKill;
    public static OptionItem RecklessChanceToAlert;
    public static OptionItem Gravedigger;
    public static OptionItem Villager;
    public static OptionItem Plumber;
    public static OptionItem PlumberVentCooldown;
    public static OptionItem Firefighter;
    public static OptionItem Jury;
    public static OptionItem Extremist;
    public static OptionItem Communist;
    public static OptionItem CommunistRecruitCooldown;
    public static OptionItem Prototype;
    public static OptionItem PrototypeKillCooldown;
    public static OptionItem PrototypeSuicideChance;

    public static List<PlayerControl> Customers = [];
    public static List<CustomRoles> ImpRoles = [];
    public static List<CustomRoles> CrewRoles = [];
    public static List<CustomRoles> NeutralRoles = [];

    public override void Add(byte playerId)
    {
        if (Deviator.GetBool()) ImpRoles.Add(CustomRoles.Deviator);
        if (Reckless.GetBool()) ImpRoles.Add(CustomRoles.Reckless);
        if (Gravedigger.GetBool()) ImpRoles.Add(CustomRoles.Gravedigger);
        
        if (Villager.GetBool()) CrewRoles.Add(CustomRoles.Villager);
        if (Plumber.GetBool()) CrewRoles.Add(CustomRoles.Plumber);
        if (Firefighter.GetBool()) CrewRoles.Add(CustomRoles.Firefighter);
        if (Jury.GetBool()) CrewRoles.Add(CustomRoles.Jury);
        
        if (Extremist.GetBool()) NeutralRoles.Add(CustomRoles.Extremist);
        if (Communist.GetBool()) NeutralRoles.Add(CustomRoles.Communist);
        if (Prototype.GetBool()) NeutralRoles.Add(CustomRoles.Prototype);
    }

    public override void SetupCustomOption()
    {
        SetupRoleOptions(Id, TabGroup.NeutralRoles, CustomRoles.Developer);
        Deviator =  BooleanOptionItem.Create(Id + 10, "Deviator", true, TabGroup.NeutralRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Developer])
            .SetColor(Utils.GetRoleColor(CustomRoles.Deviator));
        DeviatorShieldDuration = FloatOptionItem.Create(Id + 20, "DeviatorShieldDuration", (10f, 40f, 1f), 20f, TabGroup.NeutralRoles, false)
            .SetParent(Deviator)
            .SetColor(Utils.GetRoleColor(CustomRoles.Deviator))
            .SetValueFormat(OptionFormat.Seconds);        
        DeviatorShieldCooldown = FloatOptionItem.Create(Id + 21, "DeviatorShieldCooldown", (10f, 40f, 1f), 20f, TabGroup.NeutralRoles, false)
            .SetParent(Deviator)
            .SetColor(Utils.GetRoleColor(CustomRoles.Deviator))
            .SetValueFormat(OptionFormat.Seconds);
        
        Reckless =  BooleanOptionItem.Create(Id + 11, "Reckless", true, TabGroup.NeutralRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Developer])
            .SetColor(Utils.GetRoleColor(CustomRoles.Reckless));
        RecklessChanceToKill = IntegerOptionItem.Create(Id + 22, "RecklessChanceToKill", (1, 100, 5), 25, TabGroup.NeutralRoles, false)
            .SetParent(Reckless)
            .SetColor(Utils.GetRoleColor(CustomRoles.Reckless))
            .SetValueFormat(OptionFormat.Percent);
        RecklessChanceToAlert = IntegerOptionItem.Create(Id + 23, "RecklessChanceToAlert", (1, 100, 5), 25, TabGroup.NeutralRoles, false)
            .SetParent(Reckless)
            .SetColor(Utils.GetRoleColor(CustomRoles.Reckless))
            .SetValueFormat(OptionFormat.Percent);
        
        Gravedigger =  BooleanOptionItem.Create(Id + 12, "Gravedigger", true, TabGroup.NeutralRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Developer])
            .SetColor(Utils.GetRoleColor(CustomRoles.Gravedigger));
        
        Villager =  BooleanOptionItem.Create(Id + 13, "Villager", true, TabGroup.NeutralRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Developer])
            .SetColor(Utils.GetRoleColor(CustomRoles.Villager));
        
        Plumber =  BooleanOptionItem.Create(Id + 14, "Plumber", true, TabGroup.NeutralRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Developer])
            .SetColor(Utils.GetRoleColor(CustomRoles.Plumber));
        PlumberVentCooldown = FloatOptionItem.Create(Id + 24, "PlumberVentCooldown", (10f, 40f, 1f), 20f, TabGroup.NeutralRoles, false)
            .SetParent(Plumber)
            .SetColor(Utils.GetRoleColor(CustomRoles.Plumber))
            .SetValueFormat(OptionFormat.Seconds);
        
        Firefighter =  BooleanOptionItem.Create(Id + 15, "Firefighter", true, TabGroup.NeutralRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Developer])
            .SetColor(Utils.GetRoleColor(CustomRoles.Firefighter));
        OverrideTasksData.Create(Id + 25, TabGroup.NeutralRoles, CustomRoles.Firefighter, Firefighter);
        
        Jury =  BooleanOptionItem.Create(Id + 16, "Jury", true, TabGroup.NeutralRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Developer])
            .SetColor(Utils.GetRoleColor(CustomRoles.Jury));
        OverrideTasksData.Create(Id + 26, TabGroup.NeutralRoles, CustomRoles.Jury, Jury);
        
        Extremist =  BooleanOptionItem.Create(Id + 17, "Extremist", true, TabGroup.NeutralRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Developer])
            .SetColor(Utils.GetRoleColor(CustomRoles.Extremist));
        
        Communist =  BooleanOptionItem.Create(Id + 18, "Communist", true, TabGroup.NeutralRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Developer])
            .SetColor(Utils.GetRoleColor(CustomRoles.Communist));
        CommunistRecruitCooldown = FloatOptionItem.Create(Id + 27, "CommunistRecruitCooldown", (10f, 40f, 1f), 20f, TabGroup.NeutralRoles, false)
            .SetParent(Communist)
            .SetColor(Utils.GetRoleColor(CustomRoles.Communist))
            .SetValueFormat(OptionFormat.Seconds);
        
        Prototype =  BooleanOptionItem.Create(Id + 19, "Prototype", true, TabGroup.NeutralRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Developer])
            .SetColor(Utils.GetRoleColor(CustomRoles.Prototype));
        PrototypeKillCooldown = FloatOptionItem.Create(Id + 28, "PrototypeKillCooldown", (10f, 40f, 1f), 20f, TabGroup.NeutralRoles, false)
            .SetParent(Prototype)
            .SetColor(Utils.GetRoleColor(CustomRoles.Prototype))
            .SetValueFormat(OptionFormat.Seconds);
        PrototypeSuicideChance = IntegerOptionItem.Create(Id + 29, "PrototypeSuicideChance", (1, 100, 5), 20, TabGroup.NeutralRoles, false)
            .SetParent(Prototype)
            .SetColor(Utils.GetRoleColor(CustomRoles.Prototype))
            .SetValueFormat(OptionFormat.Percent);
    }

    public override bool OnCheckMurderAsKiller(PlayerControl killer, PlayerControl target)
    {
        if (target.IsPlayerImpostorTeam() && ImpRoles.Any())
        {
            Customers.Add(target);
            var role = ImpRoles.RandomElement();
            target.RpcSetCustomRole(role);
            target.RpcChangeRoleBasis(role);
            ImpRoles.Remove(role);
        }

        if (target.IsPlayerCrewmateTeam() && CrewRoles.Any())
        {
            Customers.Add(target);
            var role = CrewRoles.RandomElement();
            target.RpcSetCustomRole(role);
            target.RpcChangeRoleBasis(role);
            CrewRoles.Remove(role);
        }

        if (target.IsPlayerNeutralTeam() && NeutralRoles.Any())
        {
            Customers.Add(target);
            var role = NeutralRoles.RandomElement();
            target.RpcSetCustomRole(role);
            target.RpcChangeRoleBasis(role);
            NeutralRoles.Remove(role);
        }
        
        killer.RpcGuardAndKill(fromSetKCD: true);
        return false;
    }

    public override bool CanUseKillButton(PlayerControl pc)
    {
        return true;
    }
}

internal class Deviator : RoleBase
{ 
    public override CustomRoles Role => CustomRoles.Deviator;
    public override CustomRoles ThisRoleBase => CustomRoles.Shapeshifter;
    public override Custom_RoleType ThisRoleType => Custom_RoleType.ImpostorSupport;

    public static bool IsAlert = false;

    public override void UnShapeShiftButton(PlayerControl shapeshifter)
    {
        IsAlert = true;
        shapeshifter.RpcResetAbilityCooldown();
        new LateTask(() =>
        {
            IsAlert = false;
        }, Developer.DeviatorShieldDuration.GetFloat(), "Remove alert Deviator");
    }

    public override bool OnCheckMurderAsTarget(PlayerControl killer, PlayerControl target)
    {
        if (IsAlert)
        {
            killer.RpcMurderPlayer(killer);
            IsAlert = false;
            return false;
        }
        return true;
    }

    public override void ApplyGameOptions(IGameOptions opt, byte playerId)
    {
        AURoleOptions.ShapeshifterCooldown = Developer.DeviatorShieldCooldown.GetFloat();
    }
}

internal class Reckless : RoleBase
{ 
    public override CustomRoles Role => CustomRoles.Reckless;
    public override CustomRoles ThisRoleBase => CustomRoles.Impostor;
    public override Custom_RoleType ThisRoleType => Custom_RoleType.ImpostorKilling;

    public override bool OnCheckMurderAsKiller(PlayerControl killer, PlayerControl target)
    {
        if (IRandom.Instance.Next(1, 100) <= Developer.RecklessChanceToKill.GetInt())
        {
            PlayerControl closest = Main.AllAlivePlayerControls.Where(x => x.PlayerId != killer.PlayerId).MinBy(x => Utils.GetDistance(killer.GetCustomPosition(), x.GetCustomPosition()));
            closest.RpcMurderPlayer(closest);
            closest.SetRealKiller(killer);
            return true;
        }
        if (IRandom.Instance.Next(1, 100) <= Developer.RecklessChanceToAlert.GetInt())
        {
            target.Notify(Translator.GetString("RecklessAlert"));
            killer.ResetKillCooldown();
            killer.RpcGuardAndKill();
            return false;
        }
        
        return true;
    }
}

internal class Gravedigger : RoleBase
{ 
    public override CustomRoles Role => CustomRoles.Gravedigger;
    public override CustomRoles ThisRoleBase => CustomRoles.Impostor;
    public override Custom_RoleType ThisRoleType => Custom_RoleType.ImpostorConcealing;

    public override bool OnCheckMurderAsKiller(PlayerControl killer, PlayerControl target)
    {
        var shipRoom = ShipStatus.Instance.AllRooms.ToArray();

        List<PlainShipRoom> validRooms = [];
        
        foreach (var room in shipRoom)
        {
            if (room.RoomId != SystemTypes.HeliSabotage) validRooms.Add(room);
        }

        var roomToSend = validRooms[IRandom.Instance.Next(0, validRooms.Count)];

        target.RpcTeleport(roomToSend.transform.position);
        new LateTask(() =>
        {
            target.RpcMurderPlayer(target);
            target.SetRealKiller(killer);
        }, 1f, "Gravedigger Kill");
        
        return false;
    }
}

internal class Villager : RoleBase
{ 
    public override CustomRoles Role => CustomRoles.Villager;
    public override CustomRoles ThisRoleBase => CustomRoles.Crewmate;
    public override Custom_RoleType ThisRoleType => Custom_RoleType.CrewmateBasic;

    public override void OnMeetingHudStart(PlayerControl player)
    {
        var pc = Main.AllAlivePlayerControls.RandomElement();
        MeetingHudStartPatch.AddMsg($"The player {pc.GetRealName()} was in the room {Translator.GetString(pc.GetPlainShipRoom().RoomId.ToString())} before the meeting was called.", title: Utils.ColorString(Utils.GetRoleColor(CustomRoles.Residue), "RESIDUE INFORMATION"));
    }
}

internal class Plumber : RoleBase
{ 
    public override CustomRoles Role => CustomRoles.Plumber;
    public override CustomRoles ThisRoleBase => CustomRoles.Engineer;
    public override Custom_RoleType ThisRoleType => Custom_RoleType.CrewmateHindering;

    public override void ApplyGameOptions(IGameOptions opt, byte playerId)
    {
        AURoleOptions.EngineerCooldown = Developer.PlumberVentCooldown.GetFloat();
    }

    public override void OnEnterVent(PlayerControl pc, Vent vent)
    {

        foreach (var player in Main.AllAlivePlayerControls)
        {
            if (player == pc) return;
            foreach (var bvent in ShipStatus.Instance.AllVents.ToList())
            {
                CustomRoleManager.BlockedVentsList[player.PlayerId].Add(bvent.Id);
                CustomRoleManager.DoNotUnlockVentsList[player.PlayerId].Add(bvent.Id);
            }
        }
    }

    public override void OnExitVent(PlayerControl pc, int ventId)
    {
        foreach (var player in Main.AllAlivePlayerControls)
        {
            foreach (var bvent in ShipStatus.Instance.AllVents.ToList())
            {
                if (CustomRoleManager.BlockedVentsList[player.PlayerId].Contains(bvent.Id)) CustomRoleManager.BlockedVentsList[player.PlayerId].Remove(bvent.Id);
                if (CustomRoleManager.DoNotUnlockVentsList[player.PlayerId].Contains(bvent.Id)) CustomRoleManager.DoNotUnlockVentsList[player.PlayerId].Remove(bvent.Id);
            }
        }
    }

    public override void AfterMeetingTasks()
    {
        foreach (var player in Main.AllAlivePlayerControls)
        {
            foreach (var bvent in ShipStatus.Instance.AllVents.ToList())
            {
                if (CustomRoleManager.BlockedVentsList[player.PlayerId].Contains(bvent.Id)) CustomRoleManager.BlockedVentsList[player.PlayerId].Remove(bvent.Id);
                if (CustomRoleManager.DoNotUnlockVentsList[player.PlayerId].Contains(bvent.Id)) CustomRoleManager.DoNotUnlockVentsList[player.PlayerId].Remove(bvent.Id);
            }
        }    
    }
}

internal class Firefighter : RoleBase
{ 
    public override CustomRoles Role => CustomRoles.Firefighter;
    public override CustomRoles ThisRoleBase => CustomRoles.Crewmate;
    public override Custom_RoleType ThisRoleType => Custom_RoleType.CrewmateSupport;
    
    public override bool OnTaskComplete(PlayerControl player, int completedTaskCount, int totalTaskCount)
    {
        if (Utils.IsActive(SystemTypes.Laboratory))
        {
            ShipStatus.Instance.RpcUpdateSystem(SystemTypes.Laboratory, 67);
            ShipStatus.Instance.RpcUpdateSystem(SystemTypes.Laboratory, 66);
        }
        if (Utils.IsActive(SystemTypes.LifeSupp)) 
        {
            ShipStatus.Instance.RpcUpdateSystem(SystemTypes.LifeSupp, 67);
            ShipStatus.Instance.RpcUpdateSystem(SystemTypes.LifeSupp, 66);
        }
        if (Utils.IsActive(SystemTypes.Reactor))  
        {
            ShipStatus.Instance.RpcUpdateSystem(SystemTypes.Reactor, 16);
            ShipStatus.Instance.RpcUpdateSystem(SystemTypes.Reactor, 17);
        }
        if (Utils.IsActive(SystemTypes.Comms)) 
        {
            ShipStatus.Instance.RpcUpdateSystem(SystemTypes.Comms, 16);
            ShipStatus.Instance.RpcUpdateSystem(SystemTypes.Comms, 17);
        }
        
        return true;
    }
}

internal class Jury : RoleBase
{ 
    public override CustomRoles Role => CustomRoles.Jury;
    public override CustomRoles ThisRoleBase => CustomRoles.Crewmate;
    public override Custom_RoleType ThisRoleType => Custom_RoleType.CrewmatePower;

    public static PlayerControl TrialPlayer = null;
    public static int uses = 1;
    public override bool CheckVote(PlayerControl voter, PlayerControl target)
    {
        if (uses <= 0) return true;
        uses -= 1;
        TrialPlayer = target;
        return true;
    }

    public override void AfterMeetingTasks()
    {
        TrialPlayer = null;
    }

    public override bool OnTaskComplete(PlayerControl player, int completedTaskCount, int totalTaskCount)
    {
        if (completedTaskCount == totalTaskCount) uses += 1;
        return true;
    }
}

internal class Extremist : RoleBase
{ 
    public override CustomRoles Role => CustomRoles.Extremist;
    public override CustomRoles ThisRoleBase => CustomRoles.Crewmate;
    public override Custom_RoleType ThisRoleType => Custom_RoleType.NeutralBenign;
}

internal class Communist : RoleBase
{
    public override CustomRoles Role => CustomRoles.Communist;
    public override CustomRoles ThisRoleBase => CustomRoles.Impostor;
    public override Custom_RoleType ThisRoleType => Custom_RoleType.NeutralChaos;

    public static List<byte> Communists = [];
    public static Dictionary<byte, CustomRoles> CommunistRoles = [];

    public override bool OnCheckMurderAsKiller(PlayerControl killer, PlayerControl target)
    {
        Communists.Add(target.PlayerId);
        CommunistRoles.Add(target.PlayerId, target.GetCustomRole());
        target.RpcSetCustomRole(CustomRoles.Communist);
        target.RpcChangeRoleBasis(CustomRoles.Communist);
        killer.ResetKillCooldown();
        return false;
    }

    public override bool OnCheckMurderAsTarget(PlayerControl killer, PlayerControl target)
    {
        Communists.Remove(target.PlayerId);
        var revolutionary = Utils.GetPlayerById(Communists.RandomElement());
        revolutionary.RpcSetCustomRole(CommunistRoles[revolutionary.PlayerId]);
        revolutionary.RpcChangeRoleBasis(CommunistRoles[revolutionary.PlayerId]);
        Communists.Remove(revolutionary.PlayerId);
        return true;
    }

    public override bool CanUseKillButton(PlayerControl pc)
    {
        return true;
    }
    
    public override void SetKillCooldown(byte id) => Developer.CommunistRecruitCooldown.GetFloat();
}

internal class Prototype : RoleBase
{ 
    public override CustomRoles Role => CustomRoles.Prototype;
    public override CustomRoles ThisRoleBase => CustomRoles.Impostor;
    public override Custom_RoleType ThisRoleType => Custom_RoleType.NeutralKilling;
    
    
    public override bool CanUseKillButton(PlayerControl pc)
    {
        return true;
    }

    public override bool OnCheckMurderAsKiller(PlayerControl killer, PlayerControl target)
    {
        if (Developer.PrototypeSuicideChance.GetInt() <= IRandom.Instance.Next(1, 100)) killer.RpcMurderPlayer(killer);
        return true;
    }

    public override void SetKillCooldown(byte id) => Developer.PrototypeKillCooldown.GetFloat();
}
