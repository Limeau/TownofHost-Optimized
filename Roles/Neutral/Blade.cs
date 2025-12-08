using AmongUs.GameOptions;
using Rewired.ComponentControls;
using TOHO.Modules;
using TOHO.Roles.Double;
using static TOHO.Options;
using static TOHO.Translator;

namespace TOHO.Roles.Neutral;

internal class Blade : RoleBase
{
    //===========================SETUP================================\\
    public override CustomRoles Role => CustomRoles.Blade;
    private const int Id = 37900;
    public override CustomRoles ThisRoleBase => CustomRoles.Shapeshifter;
    public override Custom_RoleType ThisRoleType => Custom_RoleType.NeutralKilling;
    //==================================================================\\

    private static Dictionary<PlayerControl, bool> IsBladeActive = [];
    private static bool Unfreeze;
    private static float tmpSpeed;
    
    private static OptionItem KillCooldown;
    private static OptionItem UnfreezeTime;
    private static OptionItem BladeRadius;

    public override void SetupCustomOption()
    {
        SetupSingleRoleOptions(Id, TabGroup.NeutralRoles, CustomRoles.Blade);
        KillCooldown = FloatOptionItem.Create(Id + 10, GeneralOption.KillCooldown, new(1f, 60f, 1f), 20f, TabGroup.NeutralRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Blade])
            .SetValueFormat(OptionFormat.Seconds);
        UnfreezeTime = FloatOptionItem.Create(Id + 11, "UnfreezeTime379", new(0f, 5f, 0.1f), 3f, TabGroup.NeutralRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Blade])
            .SetValueFormat(OptionFormat.Seconds);
        BladeRadius = FloatOptionItem.Create(Id + 12, "BladeRadius379", new(0.5f, 1.5f, 0.1f), 1f, TabGroup.NeutralRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Blade])
            .SetValueFormat(OptionFormat.Multiplier);
    }
    
    public override void SetKillCooldown(byte id) => Main.AllPlayerKillCooldown[id] = KillCooldown.GetFloat();

    public override void UnShapeShiftButton(PlayerControl shapeshifter)
    {
        Unfreeze = false;
        IsBladeActive[shapeshifter] = true; 
        tmpSpeed = Main.AllPlayerSpeed[shapeshifter.PlayerId];
        Main.AllPlayerSpeed[shapeshifter.PlayerId] = 0f;
    }

    public override void OnFixedUpdate(PlayerControl varr, bool lowLoad, long nowTime, int timerLowLoad)
    {
        if (!IsBladeActive[_Player] && Unfreeze) return;
        if (GameStates.IsMeeting) return;
        _ = new LateTask(() =>
        {
            foreach (var player in Main.AllAlivePlayerControls)
            {
                if (player == _Player) continue;

                if (player.IsTransformedNeutralApocalypse()) continue; 
                if ((player.Is(CustomRoles.NiceMini) || player.Is(CustomRoles.EvilMini)) && Mini.Age < 18) continue;

                if (Utils.GetDistance(_Player.transform.position, player.transform.position) <= BladeRadius.GetFloat())
                {
                    _Player.KillWithoutBody(player);
                    player.SetRealKiller(_Player);
                }
            }

            new LateTask(() =>
            {
                IsBladeActive[_Player] = false;
                Main.AllPlayerSpeed[_Player.PlayerId] = tmpSpeed;
            }, UnfreezeTime.GetFloat(), "Blade Unfreeze");
        }, 0.1f, "Blade Kill Bug Fix");
    }

    public override void AfterMeetingTasks()
    {
        IsBladeActive[_Player] = false;
        Main.AllPlayerSpeed[_Player.PlayerId] = tmpSpeed;
    }
}
