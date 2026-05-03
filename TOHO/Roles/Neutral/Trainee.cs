using System.Collections.Generic;
using TOHO.Modules;
using TOHO.Roles.Double;
using static TOHO.Utils;
using static TOHO.Options;
using static TOHO.Translator;

namespace TOHO.Roles.Neutral;
internal class Trainee : RoleBase
{
    //===========================SETUP================================\\
    public override CustomRoles Role => CustomRoles.Trainee;
    private const int Id = 41500;
    public override CustomRoles ThisRoleBase => CustomRoles.Crewmate;
    public override Custom_RoleType ThisRoleType => Custom_RoleType.NeutralKilling;
    //==================================================================\\
    
    public static OptionItem MassacreKillCooldown;

    public override bool HasTasks(NetworkedPlayerInfo player, CustomRoles role, bool ForRecompute) => true;
    
    public override void SetupCustomOption()
    {
        SetupRoleOptions(Id, TabGroup.NeutralRoles, CustomRoles.Trainee);
        MassacreKillCooldown = FloatOptionItem.Create(Id + 10, GeneralOption.KillCooldown, new(0f, 60f, 1f), 20f, TabGroup.NeutralRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Trainee])
            .SetValueFormat(OptionFormat.Seconds);
        Options.OverrideTasksData.Create(Id + 20, TabGroup.NeutralRoles, CustomRoles.Trainee);
    }
    
    public override bool OnTaskComplete(PlayerControl player, int completedTaskCount, int totalTaskCount)
    {
        if (completedTaskCount == totalTaskCount) _ = new LateTask(() =>
        {
            player.RpcChangeRoleBasis(CustomRoles.TraineeB);
            player.RpcSetCustomRole(CustomRoles.TraineeB);
        }, 1f, "Trainee Role Switch");
        return true;
    }
}
internal class TraineeB : RoleBase
{
    public override CustomRoles Role => CustomRoles.TraineeB;
    public override bool IsDesyncRole => true;
    public override CustomRoles ThisRoleBase => CustomRoles.Impostor;
    public override Custom_RoleType ThisRoleType => Custom_RoleType.NeutralKilling;
    public override bool CanUseKillButton(PlayerControl player) => true;
    public override void SetKillCooldown(byte id) => Main.AllPlayerKillCooldown[id] = Trainee.MassacreKillCooldown.GetFloat();
}
