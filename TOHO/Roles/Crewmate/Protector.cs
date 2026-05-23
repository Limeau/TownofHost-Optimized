using System.Collections.Generic;
using System.Linq;
using System.Text;
using TOHO.Modules;
using static TOHO.Options;
using static TOHO.Translator;

namespace TOHO.Roles.Crewmate;

internal class Protector : RoleBase
{
    //===========================SETUP================================\\
    private const int Id = 31200;
    private static readonly HashSet<byte> playerIdList = [];
    public static bool HasEnabled => playerIdList.Any();
    public override CustomRoles Role => CustomRoles.Protector;
    public override CustomRoles ThisRoleBase => CustomRoles.Crewmate;
    public override Custom_RoleType ThisRoleType => Custom_RoleType.CrewmatePower;
    public override bool TOHORole => true;
    //==================================================================\\

    private static OptionItem MaxShields;


    public override void SetupCustomOption()
    {
        Options.SetupRoleOptions(Id, TabGroup.CrewmateRoles, CustomRoles.Protector);
        MaxShields = IntegerOptionItem.Create(Id + 10, "ProtectorMaxShields", new(1, 14, 1), 3, TabGroup.CrewmateRoles, false).SetParent(Options.CustomRoleSpawnChances[CustomRoles.Protector])
            .SetValueFormat(OptionFormat.Votes);
        Options.OverrideTasksData.Create(Id + 13, TabGroup.CrewmateRoles, CustomRoles.Protector);
    }
    public override void Add(byte playerId)
    {
        playerId.SetAbilityUseLimit(MaxShields.GetInt());

        if (!Main.ResetCamPlayerList.Contains(playerId))
            Main.ResetCamPlayerList.Add(playerId);
    }
    private Dictionary<byte, bool> ProtectorInProtect = [];

    public override bool OnTaskComplete(PlayerControl player, int completedTaskCount, int totalTaskCount)
    {
        if (player.GetAbilityUseLimit() <= 0) return true;
        player.RpcRemoveAbilityUse();
        ProtectorInProtect[player.PlayerId] = true;
        return true;
    }

    public override bool OnCheckMurderAsTarget(PlayerControl killer, PlayerControl target)
    {
        if (ProtectorInProtect[target.PlayerId])
        {
            killer.RpcGuardAndKill(target);
            if (!DisableShieldAnimations.GetBool()) target.RpcGuardAndKill();
            target.Notify(GetString("ProtectorShield"));
            ProtectorInProtect[target.PlayerId] = false;
            Logger.Info($"{target.GetNameWithRole()} shield broken", "ProtectorShieldBroken");
            return false;
        }
        return true;
    }
    public override string GetLowerText(PlayerControl pc, PlayerControl seen = null, bool isForMeeting = false, bool isForHud = false)
    {
        if (pc == null || isForMeeting || !isForHud || !pc.IsAlive()) return string.Empty;

        var str = new StringBuilder();
        if (ProtectorInProtect[pc.PlayerId])
        {
            str.Append(string.Format(GetString("ProtectorSkillTimeRemain")));
        }
        return str.ToString();
    }
}
