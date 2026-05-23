using static TOHO.Options;
namespace TOHO.Roles.Crewmate;
internal class Villager : RoleBase
{ 
    private const int Id = 40400;
    public override CustomRoles Role => CustomRoles.Villager;
    public override CustomRoles ThisRoleBase => CustomRoles.Crewmate;
    public override Custom_RoleType ThisRoleType => Custom_RoleType.CrewmateBasic;
    public override bool TOHORole => true;

    public override void SetupCustomOption()
    {
        SetupRoleOptions(Id, TabGroup.CrewmateRoles, CustomRoles.Villager);
    }

    public override void OnMeetingHudStart(PlayerControl player)
    {
        var pc = Main.AllAlivePlayerControls.RandomElement(); 
        MeetingHudStartPatch.AddMsg($"The player {pc.GetRealName()} was in the room {Translator.GetString(pc.GetPlainShipRoom().RoomId.ToString())} before the meeting was called.", title: Utils.ColorString(Utils.GetRoleColor(CustomRoles.Villager), "VILLAGER INFORMATION"));
    }
}