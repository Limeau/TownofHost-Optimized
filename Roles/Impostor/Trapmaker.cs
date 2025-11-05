using AmongUs.GameOptions;
using TOHO.Modules;
using TOHO.Roles.Core;
using static TOHO.Options;
using static TOHO.Translator;
using static TOHO.Utils;

namespace TOHO.Roles.Impostor;

internal class Trapmaker : RoleBase
{
    //===========================SETUP================================\\
    public override CustomRoles Role => CustomRoles.Trapmaker;
    private const int Id = 36500;
    public static bool HasEnabled => CustomRoleManager.HasEnabled(CustomRoles.Trapmaker);
    public override CustomRoles ThisRoleBase => CustomRoles.Shapeshifter;
    public override Custom_RoleType ThisRoleType => Custom_RoleType.ImpostorKilling;
    //==================================================================\\

    private static OptionItem ShapeshiftCooldown;
    private static OptionItem ReportFakeBody;
    private static byte TrapBody;
    public override void SetupCustomOption()
    {
        SetupRoleOptions(Id, TabGroup.ImpostorRoles, CustomRoles.Trapmaker);
        ShapeshiftCooldown = FloatOptionItem.Create(Id + 10, "TrapmakerCooldown", new(1f, 180f, 1f), 20f, TabGroup.ImpostorRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Trapmaker])
            .SetValueFormat(OptionFormat.Seconds);
        ReportFakeBody = BooleanOptionItem.Create(Id + 11, "ReportFakeBody", true, TabGroup.ImpostorRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Trapmaker]);
    }

    public override void Add(byte playerId)
    {
        playerId.SetAbilityUseLimit(1);
    }

    public override void ApplyGameOptions(IGameOptions opt, byte playerId)
    {
        AURoleOptions.ShapeshifterCooldown = ShapeshiftCooldown.GetFloat();
    }
    public override void UnShapeShiftButton(PlayerControl player)
    {
        if (player.GetAbilityUseLimit() <= 0) return;
        player.RpcRemoveAbilityUse();
        player.RpcMurderPlayer(player);
        TrapBody = player.PlayerId;
        player.RpcRevive();
        player.RpcChangeRoleBasis(CustomRoles.Trapmaker);
        player.RpcSetCustomRole(CustomRoles.Trapmaker);
    }

    public override void AfterMeetingTasks()
    {
        _Player.RpcIncreaseAbilityUseLimitBy(1);
    }

    public override void SetAbilityButtonText(HudManager hud, byte playerId)
    {
        hud.AbilityButton?.OverrideText(GetString("TrapmakerButtonText"));
    }

    public override bool OnCheckReportDeadBody(PlayerControl reporter, NetworkedPlayerInfo deadBody, PlayerControl killer)
    {
        if (reporter.IsPlayerImpostorTeam()) return false;
        if (TrapBody == deadBody.PlayerId)
        {
            reporter.SetDeathReason(PlayerState.DeathReason.Trap);
            reporter.RpcMurderPlayer(reporter);
            reporter.SetRealKiller(killer);
            return ReportFakeBody.GetBool();
        }
        return true;
    }
}
