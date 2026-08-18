using System.Linq;
using AmongUs.GameOptions;
using static TOHO.Options;

namespace TOHO.Roles.Neutral;

internal class WeepingAngel : RoleBase
{
    //===========================SETUP================================\\
    public override CustomRoles Role => CustomRoles.WeepingAngel;
    private const int Id = 46900;
    public override CustomRoles ThisRoleBase => CustomRoles.Shapeshifter;
    public override Custom_RoleType ThisRoleType => Custom_RoleType.NeutralKilling;
    public override bool TOHORole => true;
    public override bool NewRole => true;
    //==================================================================\\
    private static bool IsAbility;

    private static OptionItem AbilityCooldown;
    private static OptionItem AbilityDuration;
    public static OptionItem SpeedIncrease;
    public override void SetupCustomOption()
    {
        SetupSingleRoleOptions(Id, TabGroup.NeutralRoles, CustomRoles.WeepingAngel, 1, zeroOne: false);
        AbilityCooldown = FloatOptionItem.Create(Id + 10, GeneralOption.AbilityCooldown, new(0f, 180f, 2.5f), 35f, TabGroup.NeutralRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.WeepingAngel])
            .SetValueFormat(OptionFormat.Seconds);
        AbilityDuration = FloatOptionItem.Create(Id + 11, GeneralOption.AbilityDuration, new(0f, 60f, 1f), 10f, TabGroup.NeutralRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.WeepingAngel])
            .SetValueFormat(OptionFormat.Seconds);
        SpeedIncrease = FloatOptionItem.Create(Id + 12, "SpeedIncrease", new(2f, 5f, 0.2f), 3f, TabGroup.NeutralRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.WeepingAngel]) 
            .SetValueFormat(OptionFormat.Multiplier);
    }

    public override void Add(byte playerId)
    {
        IsAbility = false;
    }

    public override void ApplyGameOptions(IGameOptions opt, byte playerId)
    {
        AURoleOptions.ShapeshifterCooldown = AbilityCooldown.GetFloat();
    }

    public override void SetKillCooldown(byte id) => Main.AllPlayerKillCooldown[id] = DefaultKillCooldown;
    public override bool CanUseKillButton(PlayerControl pc) => false;
    public override bool CanUseImpostorVentButton(PlayerControl pc) => true;
    public override void UnShapeShiftButton(PlayerControl shapeshifter)
    {
        IsAbility = true;
        foreach (var player in Main.AllAlivePlayerControls.Where(x => x != shapeshifter))
        {
            Main.PlayerStates[player.PlayerId].IsBlackOut = true;
            player.MarkDirtySettings();
        }

        var tmpspeed = Main.AllPlayerSpeed[shapeshifter.PlayerId];
        Main.AllPlayerSpeed[shapeshifter.PlayerId] = SpeedIncrease.GetFloat();
        shapeshifter.MarkDirtySettings();

        _ = new LateTask(() =>
        {
            IsAbility = false;
            Main.AllPlayerSpeed[shapeshifter.PlayerId] = tmpspeed;
            shapeshifter.MarkDirtySettings();
            foreach (var player in Main.AllAlivePlayerControls.Where(x => x != shapeshifter))
            {
                Main.PlayerStates[player.PlayerId].IsBlackOut = false;
                player.MarkDirtySettings();
            }
        }, AbilityDuration.GetFloat(), "Weeping Angel");
    }

    public override void OnFixedUpdate(PlayerControl angel, bool lowLoad, long nowTime, int timerLowLoad)
    {
        if (angel.Is(CustomRoles.WeepingAngel) && IsAbility)
        {
            foreach (var player in Main.AllAlivePlayerControls.Where(x => x != angel))
            {
                if (Utils.GetDistance(player.transform.position, angel.transform.position) <= 1)
                {
                    angel.RpcMurderPlayer(player);
                }
            }
        }
    }
}