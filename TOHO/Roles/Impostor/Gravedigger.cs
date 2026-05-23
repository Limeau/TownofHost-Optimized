using System.Collections.Generic;
using System.Linq;
using static TOHO.Options;
namespace TOHO.Roles.Impostor;

internal class Gravedigger : RoleBase
{ 
    public override CustomRoles Role => CustomRoles.Gravedigger;
    private const int Id = 40300;
    public override CustomRoles ThisRoleBase => CustomRoles.Impostor;
    public override Custom_RoleType ThisRoleType => Custom_RoleType.ImpostorConcealing;
    public override bool TOHORole => true;

    public override void SetupCustomOption()
    {
        SetupRoleOptions(Id, TabGroup.ImpostorRoles, CustomRoles.Gravedigger);
    }
    
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