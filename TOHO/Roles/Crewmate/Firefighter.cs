using static TOHO.Options;
namespace TOHO.Roles.Crewmate;
internal class Firefighter : RoleBase
{ 
    private const int Id = 40600;
    public override CustomRoles Role => CustomRoles.Firefighter;
    public override CustomRoles ThisRoleBase => CustomRoles.Crewmate;
    public override Custom_RoleType ThisRoleType => Custom_RoleType.CrewmateSupport;
    public override bool TOHORole => true;

    public override void SetupCustomOption()
    {        
        SetupRoleOptions(Id, TabGroup.CrewmateRoles, CustomRoles.Firefighter);
        OverrideTasksData.Create(Id + 10, TabGroup.CrewmateRoles, CustomRoles.Firefighter);
    }

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