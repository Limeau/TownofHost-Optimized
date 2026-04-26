using System.Linq;
using AmongUs.GameOptions;
using TOHO.Roles.Core;
using static TOHO.Options;
namespace TOHO.Roles.Crewmate;

internal class Plumber : RoleBase
{ 
    private const int Id = 40500;
    public override CustomRoles Role => CustomRoles.Plumber;
    public override CustomRoles ThisRoleBase => CustomRoles.Engineer;
    public override Custom_RoleType ThisRoleType => Custom_RoleType.CrewmateHindering;

    public static OptionItem PlumberVentCooldown;

    public override void SetupCustomOption()
    {
        SetupRoleOptions(Id, TabGroup.CrewmateRoles, CustomRoles.Plumber);
        PlumberVentCooldown = FloatOptionItem.Create(Id + 10, "PlumberVentCooldown", (10f, 40f, 1f), 20f,
                TabGroup.CrewmateRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Plumber])
            .SetValueFormat(OptionFormat.Seconds);
    }

    public override void ApplyGameOptions(IGameOptions opt, byte playerId)
    {
        AURoleOptions.EngineerCooldown = PlumberVentCooldown.GetFloat();
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