using System.Linq;
using HarmonyLib;
using static TOHO.Options;
namespace TOHO.Roles.Crewmate;
internal class Drone : RoleBase
{ 
    private const int Id = 45000;
    public override CustomRoles Role => CustomRoles.Drone;
    public override CustomRoles ThisRoleBase => CustomRoles.Crewmate;
    public override Custom_RoleType ThisRoleType => Custom_RoleType.CrewmatePower;
    public override bool TOHORole => true;
    public override string IdeaRole => "bxogamesyt";

    public static PlayerControl PCDrone;
    public static bool IsInProtect = false;
    public static float SwitchTime = 0;
    public static OptionItem SkillDuration;
    public static OptionItem ProtectRadiusOpt;
    
    public override void SetupCustomOption()
    {        
        SetupRoleOptions(Id, TabGroup.CrewmateRoles, CustomRoles.Drone);
        SkillDuration = FloatOptionItem.Create(Id + 4, GeneralOption.AbilityDuration, new(1f, 30f, 1f), 10f, TabGroup.CrewmateRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.Drone])
            .SetValueFormat(OptionFormat.Seconds);
        ProtectRadiusOpt = FloatOptionItem.Create(Id + 5, "ProtectRadius450", new(0.5f, 5f, 0.5f), 2f, TabGroup.CrewmateRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Drone])
            .SetValueFormat(OptionFormat.Multiplier);
        OverrideTasksData.Create(Id + 10, TabGroup.CrewmateRoles, CustomRoles.Drone);
    }

    public override void Add(byte playerId)
    {
        PCDrone = Utils.GetPlayerById(playerId);
        IsInProtect = false;
    }

    public override bool CheckMurderOnOthersTarget(PlayerControl killer, PlayerControl target)
    {
        if (IsInProtect && Utils.GetDistance(target.transform.position, PCDrone.transform.position) <= ProtectRadiusOpt.GetFloat())
        {
            killer.RpcGuardAndKill();
            return true;
        }
        return false;
    }

    public override void OnOthersTaskComplete(PlayerControl pc, PlayerTask task, bool playerIsOverridden, PlayerControl realPlayer)
    {
        if (IsInProtect && Utils.GetDistance(pc.transform.position, PCDrone.transform.position) <= ProtectRadiusOpt.GetFloat()) SwitchTime += 5;
    }

    public override bool OnTaskComplete(PlayerControl player, int completedTaskCount, int totalTaskCount)
    {
        if (!player.Is(CustomRoles.Drone)) return true;
        if (IsInProtect) return false;
        SwitchTime = ProtectRadiusOpt.GetFloat();
        IsInProtect = true;
        return true;
    }
    
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
    class UpdateInGameModeKothPatch
    {
        private static long LastFixedUpdate;
        public static void Postfix()
        {
            var now = Utils.GetTimeStamp();
            
            if (!AmongUsClient.Instance.AmHost || !IsInProtect) return;
            
            if (LastFixedUpdate == now) return;
            LastFixedUpdate = now;
            SwitchTime--;
            if (SwitchTime <= 0)
            {
                IsInProtect = false;
            }

            if (IsInProtect)
            {
                foreach (var player in Main.AllAlivePlayerControls.Where(x => Utils.GetDistance(x.transform.position, PCDrone.transform.position) <= ProtectRadiusOpt.GetFloat()))
                {
                    player.Notify($"You are protected for {SwitchTime} seconds while close to the Drone!");
                }
            }
            else
            {
                foreach (var player in Main.AllAlivePlayerControls.Where(x => Utils.GetDistance(x.transform.position, PCDrone.transform.position) <= ProtectRadiusOpt.GetFloat()))
                {
                    player.Notify($"Protection has ended.");
                }
            }
        }
    }
}