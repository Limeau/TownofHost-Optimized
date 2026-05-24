using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using static TOHO.Options;

namespace TOHO.Roles.Impostor;

internal class Ragnarok : RoleBase
{
    //===========================SETUP================================\\
    public override CustomRoles Role => CustomRoles.Ragnarok;
    private const int Id = 41800;
    public override CustomRoles ThisRoleBase => CustomRoles.Shapeshifter;
    public override Custom_RoleType ThisRoleType => Custom_RoleType.ImpostorHindering;
    public override bool TOHORole => true;
    public override string IdeaRole => "zephyrennthegreat";
    //==================================================================\\

    private static OptionItem KillCooldown;
    private static OptionItem AbilityCooldown;
    private static OptionItem AbilityDuration;
    
    public override void SetupCustomOption()
    {
        SetupRoleOptions(Id, TabGroup.ImpostorRoles, CustomRoles.Ragnarok);
        KillCooldown = FloatOptionItem.Create(Id + 10, GeneralOption.KillCooldown, new(0f, 180f, 2.5f), 20f, TabGroup.ImpostorRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Ragnarok])
            .SetValueFormat(OptionFormat.Seconds);
        AbilityCooldown = FloatOptionItem.Create(Id + 11, GeneralOption.AbilityCooldown, new(0f, 180f, 2.5f), 20f, TabGroup.ImpostorRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Ragnarok])
            .SetValueFormat(OptionFormat.Seconds);
        AbilityDuration = FloatOptionItem.Create(Id + 12, GeneralOption.AbilityDuration, new(1f, 30f, 1f), 5f, TabGroup.ImpostorRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Ragnarok])
            .SetValueFormat(OptionFormat.Seconds);
    }

    public override void SetKillCooldown(byte id) => Main.AllPlayerKillCooldown[id] = KillCooldown.GetFloat();

    public override void ApplyGameOptions(IGameOptions opt, byte playerId)
    {
        AURoleOptions.ShapeshifterCooldown = AbilityCooldown.GetFloat();
    }

    public override void UnShapeShiftButton(PlayerControl shapeshifter)
    {
        foreach (var player in Main.AllAlivePlayerControls.Where(x => !x.Is(CustomRoles.Ragnarok)))
        {
            Main.PlayerStates[player.PlayerId].IsBlackOut = true;
            player.MarkDirtySettings();
            _ = new LateTask(() =>
            {
                Main.PlayerStates[player.PlayerId].IsBlackOut = false;
                player.MarkDirtySettings();
            }, AbilityDuration.GetFloat(), "Unblind @ Ragnarok");
        }
        _ = new LateTask(() =>
        {
            if (Main.AllAlivePlayerControls.Where(x => !x.Is(CustomRoles.Ragnarok) && !x.Is(CustomRoles.Valhalla)).Any()) Main.AllAlivePlayerControls.Where(x => !x.Is(CustomRoles.Ragnarok) && !x.Is(CustomRoles.Valhalla)).RandomElement().RpcSetCustomRole(CustomRoles.Valhalla);
        }, AbilityDuration.GetFloat(), "Valhalla Assign");
    }

    public override bool OnCheckMurderAsKiller(PlayerControl killer, PlayerControl target)
    {
        if (!target.Is(CustomRoles.Valhalla)) return true;
        var tempcd = Main.AllPlayerKillCooldown[killer.PlayerId];
        _ = new LateTask(() =>
        {
            Main.AllPlayerKillCooldown[killer.PlayerId] = tempcd;
        }, 5f, "Ragnarok KCD");
        Main.AllPlayerKillCooldown[killer.PlayerId] = 0.1f;
        killer.SetKillCooldownV2();
        return true;
    }
}