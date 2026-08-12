using System.Collections.Generic;
using AmongUs.GameOptions;
using static TOHO.Utils;

namespace TOHO.Roles.Impostor;

internal class Samurai : RoleBase
{
    //===========================SETUP================================\\
    public override CustomRoles Role => CustomRoles.Samurai;
    private const int Id = 46800;
    public override CustomRoles ThisRoleBase => CustomRoles.Shapeshifter;
    public override Custom_RoleType ThisRoleType => Custom_RoleType.ImpostorKilling;
    public override bool TOHORole => true;
    public override bool NewRole => true;
    //==================================================================\\

    private static OptionItem SamuraiCD;
    private static OptionItem BlindRadius;
    private static OptionItem SamuraiDuration;
    private static List<PlayerControl> BlindedPlayers = [];

    public override void SetupCustomOption()
    {
        Options.SetupRoleOptions(Id, TabGroup.ImpostorRoles, CustomRoles.Samurai);
        BlindRadius = FloatOptionItem.Create(Id + 10, "BlindRadius350", new(1f, 5f, 1f), 3f, TabGroup.ImpostorRoles, false)
            .SetParent(Options.CustomRoleSpawnChances[CustomRoles.Samurai])
            .SetValueFormat(OptionFormat.Multiplier);
        SamuraiCD = FloatOptionItem.Create(Id + 11, GeneralOption.AbilityCooldown, new(1f, 60f, 1f), 20f,
                TabGroup.ImpostorRoles, false)
            .SetParent(Options.CustomRoleSpawnChances[CustomRoles.Samurai])
            .SetValueFormat(OptionFormat.Seconds);
        SamuraiDuration = FloatOptionItem.Create(Id + 12, GeneralOption.AbilityDuration, new(1f, 30f, 1f), 10f,
                TabGroup.ImpostorRoles, false)
            .SetParent(Options.CustomRoleSpawnChances[CustomRoles.Samurai])
            .SetValueFormat(OptionFormat.Seconds);
    }

    public override void SetKillCooldown(byte id) => Main.AllPlayerKillCooldown[id] = 0.1f;

    public override void ApplyGameOptions(IGameOptions opt, byte playerId)
    {
        AURoleOptions.ShapeshifterCooldown = SamuraiCD.GetFloat();
    }

    public override void Add(byte playerId)
    {
        BlindedPlayers.Clear();
    }

    public override bool OnCheckMurderAsKiller(PlayerControl killer, PlayerControl target)
    {
        return (BlindedPlayers.Contains(target));
    }

    public override void UnShapeShiftButton(PlayerControl shapeshifter)
    {
        foreach (var player in Main.AllAlivePlayerControls)
        {
            if (Utils.GetDistance(shapeshifter.GetTruePosition(), player.GetTruePosition()) <= BlindRadius.GetFloat())
            {
                Main.PlayerStates[player.PlayerId].IsBlackOut = true;
                player.MarkDirtySettings();
                BlindedPlayers.Add(player);
                _ = new LateTask(() =>
                {
                    Main.PlayerStates[player.PlayerId].IsBlackOut = false;
                    player.MarkDirtySettings();
                    BlindedPlayers.Remove(player);
                }, SamuraiDuration.GetFloat(), "Remove Samurai Blinding");
            }
        }
    }
}