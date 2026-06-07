using AmongUs.GameOptions;
using UnityEngine;
using static TOHO.Options;
using static TOHO.Translator;

namespace TOHO.Roles.Crewmate;

internal class Archivist : RoleBase
{
    //===========================SETUP================================\\

    public static bool InRevival = false;
    public override CustomRoles Role => CustomRoles.Archivist;
    private const int Id = 42500;
    public override CustomRoles ThisRoleBase => CustomRoles.Shapeshifter;
    public override Custom_RoleType ThisRoleType => Custom_RoleType.CrewmatePower;
    public override bool TOHORole => true;
    public override string IdeaRole => "the_little_pelican";
    //==================================================================\\
    public static SystemTypes ActiveRoom = SystemTypes.HeliSabotage;
    public static int Kills = 0;
    public static int Shifts = 0;
    public static int Tasks = 0;
    public static int Vents = 0;

    public override void Add(byte playerId)
    {
        ActiveRoom = SystemTypes.HeliSabotage;
        Kills = 0;
        Shifts = 0; 
        Tasks = 0;
        Vents = 0;
    }
    public override void ApplyGameOptions(IGameOptions opt, byte playerId)
    {
        AURoleOptions.ShapeshifterCooldown = 1;
    }

    public override bool CanUseKillButton(PlayerControl pc)
    {
        return false;
    }

    public override void SetupCustomOption()
    {
        SetupRoleOptions(Id, TabGroup.CrewmateRoles, CustomRoles.Archivist);
    }

    public override void UnShapeShiftButton(PlayerControl shapeshifter)
    {
        if (ActiveRoom == SystemTypes.HeliSabotage)
        {
            ActiveRoom = shapeshifter.GetPlainShipRoom().RoomId;
        }
        else shapeshifter.Notify("You already selected a room this round.");
    }

    public override void OnOthersShapeshift(PlayerControl shapeshifter)
    {
        if (ActiveRoom != SystemTypes.HeliSabotage && ActiveRoom == shapeshifter.GetPlainShipRoom().RoomId) Shifts++;
    }
    public override bool CheckMurderOnOthersTarget(PlayerControl killer, PlayerControl target)
    {
        if (ActiveRoom != SystemTypes.HeliSabotage && ActiveRoom == killer.GetPlainShipRoom().RoomId)
        {
            Kills++;
        }
        return false;
    }

    public override bool OnCoEnterVentOthers(PlayerPhysics physics, int ventId)
    {
        if (ActiveRoom != SystemTypes.HeliSabotage && ActiveRoom == physics.myPlayer.GetPlainShipRoom().RoomId) Vents++;
        return true;
    }
    public override void OnOthersTaskComplete(PlayerControl pc, PlayerTask task, bool playerIsOverridden, PlayerControl realPlayer)
    {   
        if (ActiveRoom != SystemTypes.HeliSabotage && ActiveRoom == pc.GetPlainShipRoom().RoomId) Tasks++;
    }

    public override void OnMeetingHudStart(PlayerControl pc)
    {
        Utils.SendMessage($"In the room {GetString(ActiveRoom.ToString())}, the following events happened:\n{Kills} kills\n{Shifts} shapeshifts\n{Tasks} tasks completed\n{Vents} vents entered", pc.PlayerId, title: Utils.ColorString(Utils.GetRoleColor(CustomRoles.Archivist), "ARCHIVIST INFORMATION"));
    }

    public override void AfterMeetingTasks()
    {
        ActiveRoom = SystemTypes.HeliSabotage;
        Kills = 0;
        Shifts = 0; 
        Tasks = 0;
        Vents = 0;
    }
}