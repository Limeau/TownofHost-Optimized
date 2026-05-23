using AmongUs.GameOptions;
using TOHO.Roles.Core;
using static TOHO.Options;
namespace TOHO.Roles.Neutral;

internal class Sonic : RoleBase
{
    //===========================SETUP================================\\
    private const int Id = 41600;
    public static bool HasEnabled => CustomRoleManager.HasEnabled(CustomRoles.Sonic);
    public override CustomRoles Role => CustomRoles.Sonic;
    public override CustomRoles ThisRoleBase => CustomRoles.Shapeshifter;
    public override Custom_RoleType ThisRoleType => Custom_RoleType.NeutralKilling;
    public override bool TOHORole => true;
    //==================================================================\\

    public static OptionItem SonicKillCooldown;
    public static OptionItem SonicDashDuration;
    public static OptionItem SonicSpeedIncrease;
    public static OptionItem SonicSSCooldown;

    public static bool InShapeshift = false;

    public override void SetupCustomOption()
    {
        SetupRoleOptions(Id, TabGroup.NeutralRoles, CustomRoles.Sonic);
        SonicKillCooldown = FloatOptionItem.Create(Id + 2, GeneralOption.KillCooldown, new(1f, 60f, 1f), 20f, TabGroup.NeutralRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.Sonic])
            .SetValueFormat(OptionFormat.Seconds);
        SonicDashDuration = FloatOptionItem.Create(Id + 3, GeneralOption.AbilityDuration, new(1f, 60f, 1f), 20f, TabGroup.NeutralRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.Sonic])
            .SetValueFormat(OptionFormat.Seconds);
        SonicSpeedIncrease = FloatOptionItem.Create(Id + 4, "SpeedIncrease416", new(2f, 5f, 0.25f), 3f, TabGroup.NeutralRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.Sonic])
            .SetValueFormat(OptionFormat.Multiplier);
        SonicSSCooldown = FloatOptionItem.Create(Id + 5, GeneralOption.AbilityCooldown, new(1f, 60f, 1f), 10f, TabGroup.NeutralRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.Sonic])
            .SetValueFormat(OptionFormat.Seconds);
    }
    public override void SetKillCooldown(byte id)
    {
        Main.AllPlayerKillCooldown[id] = SonicKillCooldown.GetFloat();
    }

    public override void UnShapeShiftButton(PlayerControl shapeshifter)
    {
        InShapeshift = true;
        var tmpSpeed = Main.AllPlayerSpeed[shapeshifter.PlayerId];
        Main.AllPlayerSpeed[shapeshifter.PlayerId] = SonicKillCooldown.GetFloat();
        shapeshifter.MarkDirtySettings();
        new LateTask(() =>
        {
            InShapeshift = false;
            Main.AllPlayerSpeed[shapeshifter.PlayerId] = tmpSpeed; 
            shapeshifter.MarkDirtySettings();
        }, SonicDashDuration.GetFloat(), "Sonic");
    }

    public override bool OnCheckMurderAsTarget(PlayerControl killer, PlayerControl target)
    {
        if (InShapeshift)
        {
            killer.RpcGuardAndKill();
            return false;
        }
        else return true;
    }
    public override void ApplyGameOptions(IGameOptions opt, byte playerId)
    {
        AURoleOptions.ShapeshifterCooldown = SonicSSCooldown.GetFloat();
        AURoleOptions.ShapeshifterDuration = 1f;
        base.ApplyGameOptions(opt, playerId);
    }

    public override bool CanUseKillButton(PlayerControl pc) => true;
}

