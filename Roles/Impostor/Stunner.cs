using AmongUs.GameOptions;
using TOHO.Roles.Core;
using TOHO.Roles.Neutral;
using UnityEngine;
using static TOHO.Options;
using static TOHO.Translator;

namespace TOHO.Roles.Impostor;

internal class Stunnner : RoleBase
{
    //===========================SETUP================================\\
    private const int Id = 38000;
    public override CustomRoles Role => CustomRoles.Stunner;
    public override CustomRoles ThisRoleBase => CustomRoles.Shapeshifter;
    public override Custom_RoleType ThisRoleType => Custom_RoleType.ImpostorHindering;
    //==================================================================\\
    public override void SetAbilityButtonText(HudManager hud, byte playerId)
    {
        hud.AbilityButton.OverrideText(GetString("StunnerButtonText"));
    }
    
    private static OptionItem ShapeshiftCooldown;
    private static OptionItem BlindDurationOpt;
    private static OptionItem KillCooldown;

    public override void SetupCustomOption()
    {
        SetupRoleOptions(Id, TabGroup.ImpostorRoles, CustomRoles.Stunner);
        BlindDurationOpt = FloatOptionItem.Create(Id + 12, "BlindDuration380", new(1f, 20f, 1f), 5f, TabGroup.ImpostorRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.Stunner])
            .SetValueFormat(OptionFormat.Seconds);
        ShapeshiftCooldown = FloatOptionItem.Create(Id + 11, GeneralOption.AbilityCooldown, new(1f, 60f, 1f), 20f, TabGroup.ImpostorRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.Stunner])
            .SetValueFormat(OptionFormat.Seconds);
        KillCooldown = FloatOptionItem.Create(Id + 10, GeneralOption.KillCooldown, new(1f, 60f, 1f), 20f, TabGroup.ImpostorRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.Stunner])
            .SetValueFormat(OptionFormat.Seconds);
    }
    
    public override void ApplyGameOptions(IGameOptions opt, byte playerId)
    {
        AURoleOptions.ShapeshifterCooldown = ShapeshiftCooldown.GetFloat();
    }

    public override void SetKillCooldown(byte id) => Main.AllPlayerKillCooldown[id] = KillCooldown.GetFloat();

    public override bool OnCheckShapeshift(PlayerControl shapeshifter, PlayerControl target, ref bool resetCooldown,
        ref bool shouldAnimate)
    {
        target.KillFlash();
        
        shapeshifter.RpcResetAbilityCooldown();
        
        _ = new LateTask(() =>
        {
            Main.PlayerStates[target.PlayerId].IsBlackOut = true;
            target.MarkDirtySettings();
        }, 1, "Stunner Black Out");
        
        _ = new LateTask(() =>
        {
            Main.PlayerStates[target.PlayerId].IsBlackOut = false;
            target.MarkDirtySettings();
        }, BlindDurationOpt.GetFloat() + 1, "Stunner Black Out Removal");
        
        return false;
    }
}
