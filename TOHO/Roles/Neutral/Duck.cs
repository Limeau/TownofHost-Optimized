using System.Linq;
using AmongUs.GameOptions;
using TOHO.Modules;
using TOHO.Roles.Core;
using UnityEngine;
using static TOHO.Options;
namespace TOHO.Roles.Neutral;

internal class Duck : RoleBase
{
    //===========================SETUP================================\\
    private const int Id = 42900;
    public static bool HasEnabled => CustomRoleManager.HasEnabled(CustomRoles.Duck);
    public override CustomRoles Role => CustomRoles.Duck;
    public override CustomRoles ThisRoleBase => CustomRoles.Crewmate;
    public override Custom_RoleType ThisRoleType => Custom_RoleType.NeutralEvil;
    public override bool NewRole => true;
    public override bool TOHORole => true;

    public override string IdeaRole => "the_little_pelican";
    //==================================================================\\

    public static OptionItem DuckTime;
    
    public override void SetupCustomOption()
    {
        SetupRoleOptions(Id, TabGroup.NeutralRoles, CustomRoles.Duck);
        DuckTime = FloatOptionItem.Create(Id + 10, "DuckTime", (1f, 10f, 0.5f), 5f, TabGroup.NeutralRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Duck])
            .SetValueFormat(OptionFormat.Seconds);
        OverrideTasksData.Create(Id + 11, TabGroup.NeutralRoles, CustomRoles.Duck);
    }

    public override bool HasTasks(NetworkedPlayerInfo player, CustomRoles role, bool ForRecompute)
    {
        return true;
    }

    public override bool OnTaskComplete(PlayerControl tasker, int completedTaskCount, int totalTaskCount)
    {
        var target = Main.AllAlivePlayerControls.Where(x => !x.Is(CustomRoles.Duck)).RandomElement();
        if (!target) return true;
        Main.PlayerStates[target.PlayerId].IsBlackOut = true;
        target.MarkDirtySettings();
        new LateTask(() =>
        {
            Main.PlayerStates[target.PlayerId].IsBlackOut = false;
            target.MarkDirtySettings();
            target.RpcGuardAndKill();
        }, DuckTime.GetFloat(), "Duck");
        return true;
    }
}