using static TOHO.MeetingHudStartPatch;
using static TOHO.Options;
using static TOHO.Translator;
using static TOHO.Utils;

namespace TOHO.Roles.Crewmate;

internal class Hippie : RoleBase
{
    //===========================SETUP================================\\
    public override CustomRoles Role => CustomRoles.Hippie;
    private const int Id = 37600;
    public override CustomRoles ThisRoleBase => CustomRoles.Crewmate;
    public override Custom_RoleType ThisRoleType => Custom_RoleType.CrewmatePower;
    //==================================================================\\

    public static Dictionary<byte, List<byte>> VoteTargets = [];
    public static List<PlayerControl> Deaths = [];
    
    public override void SetupCustomOption()
    {
        SetupRoleOptions(Id, TabGroup.CrewmateRoles, CustomRoles.Hippie);
    }

    public override bool OnCheckMurderAsTarget(PlayerControl killer, PlayerControl target)
    {
        killer.RpcSetCustomRole(CustomRoles.Admired);
        return true;
    }
    public override void OnPlayerExiled(PlayerControl player, NetworkedPlayerInfo exiled)
    {
        if (exiled == null || (exiled.GetCustomRole() is not CustomRoles.Hippie)) return;
        var exiled2 = Utils.GetPlayerById(exiled.PlayerId);
        Deaths.Add(exiled2);
    }

    public override void AfterMeetingTasks()
    {
        foreach (var target in Deaths)
        {
            target.RpcRevive();
            target.RpcChangeRoleBasis(CustomRoles.Hippie);
            Deaths.Remove(target);
        }
    }

    public override string GetMarkOthers(PlayerControl seer, PlayerControl seen, bool isForMeeting = false)
        => seen.Is(CustomRoles.Hippie) ? ColorString(GetRoleColor(CustomRoles.Lovers), " ❀") : string.Empty;

}
