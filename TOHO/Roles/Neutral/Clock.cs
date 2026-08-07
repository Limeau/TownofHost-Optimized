using System.Linq;
using AmongUs.GameOptions;
using TOHO.Modules;
using TOHO.Roles.Core;
using UnityEngine;
using static TOHO.Options;
namespace TOHO.Roles.Neutral;

internal class Clock : RoleBase
{
    //===========================SETUP================================\\
    private const int Id = 46500;
    public static bool HasEnabled => CustomRoleManager.HasEnabled(CustomRoles.Clock);
    public override CustomRoles Role => CustomRoles.Clock;
    public override CustomRoles ThisRoleBase => CustomRoles.Shapeshifter;
    public override Custom_RoleType ThisRoleType => Custom_RoleType.NeutralKilling;
    public override bool TOHORole => true;
    public override string IdeaRole => "nespalyer";
    //==================================================================\\
    
    private static OptionItem KillCooldown;
    private static OptionItem UnshiftCooldown;
    private static OptionItem SpeedUpDuration;

    
    public override void SetupCustomOption()
    {
        SetupRoleOptions(Id, TabGroup.NeutralRoles, CustomRoles.Clock);
        KillCooldown = IntegerOptionItem.Create(Id + 10, GeneralOption.KillCooldown, new(1, 60, 1), 20, TabGroup.NeutralRoles, false)
            .SetParent(Options.CustomRoleSpawnChances[CustomRoles.Clock])
            .SetValueFormat(OptionFormat.Seconds);
        UnshiftCooldown = IntegerOptionItem.Create(Id + 11, GeneralOption.AbilityCooldown, new(1, 60, 1), 20, TabGroup.NeutralRoles, false)
            .SetParent(Options.CustomRoleSpawnChances[CustomRoles.Clock])
            .SetValueFormat(OptionFormat.Seconds);
        SpeedUpDuration = IntegerOptionItem.Create(Id + 12, "ClockSpeedUpDuration", new(1, 30, 1), 10, TabGroup.NeutralRoles, false)
            .SetParent(Options.CustomRoleSpawnChances[CustomRoles.Clock])
            .SetValueFormat(OptionFormat.Seconds);
    }

    public override void SetKillCooldown(byte id)
    {
        Main.AllPlayerKillCooldown[id] = KillCooldown.GetInt();
    }

    public override void ApplyGameOptions(IGameOptions opt, byte playerId)
    {
        AURoleOptions.ShapeshifterCooldown = UnshiftCooldown.GetInt();
    }

    public override bool CanUseKillButton(PlayerControl pc)
    {
        return true;
    }

    public override void UnShapeShiftButton(PlayerControl shapeshifter)
    {
        foreach (var player in Main.AllAlivePlayerControls)
        {
            if (player.HasKillButton())
            {
                var newkcd = player.killTimer;
                if (newkcd < 0) newkcd = 0;
                player.SetKillCooldown(newkcd);
            }

            var tmpSpeed = Main.AllPlayerSpeed[player.PlayerId];
            Main.AllPlayerSpeed[player.PlayerId] = Main.AllPlayerSpeed[player.PlayerId] * 2;
            player.MarkDirtySettings();
            new LateTask(() =>
            {
                Main.AllPlayerSpeed[player.PlayerId] = tmpSpeed;
                player.MarkDirtySettings();
            }, SpeedUpDuration.GetInt(), "Clock Reset Speed");
        }

        var elder = Main.AllAlivePlayerControls.RandomElement();
        elder.SetDeathReason(PlayerState.DeathReason.OldAge);
        elder.RpcMurderPlayer(elder);
    }
}