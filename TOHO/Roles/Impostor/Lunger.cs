using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using UnityEngine;
using static TOHO.Options;

namespace TOHO.Roles.Impostor;

internal class Lunger : RoleBase
{
    //===========================SETUP================================\\
    public override CustomRoles Role => CustomRoles.Lunger;
    private const int Id = 42300;
    public override CustomRoles ThisRoleBase => CustomRoles.Shapeshifter;
    public override Custom_RoleType ThisRoleType => Custom_RoleType.ImpostorKilling;
    public override bool TOHORole => true;
    public override string IdeaRole => "den6211epic";
    //==================================================================\\

    private static OptionItem KillCooldown;
    private static OptionItem Distance;
    
    public override void SetupCustomOption()
    {
        SetupRoleOptions(Id, TabGroup.ImpostorRoles, CustomRoles.Lunger);
        KillCooldown = FloatOptionItem.Create(Id + 10, GeneralOption.KillCooldown, new(0f, 180f, 2.5f), 20f, TabGroup.ImpostorRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Lunger])
            .SetValueFormat(OptionFormat.Seconds);
        Distance = FloatOptionItem.Create(Id + 11, "LungerDistance", new(1f, 5f, 0.5f), 3f, TabGroup.ImpostorRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.Lunger])
            .SetValueFormat(OptionFormat.Multiplier);
    }

    public override void SetKillCooldown(byte id) => Main.AllPlayerKillCooldown[id] = KillCooldown.GetFloat();

    public override void ApplyGameOptions(IGameOptions opt, byte playerId)
    {
        AURoleOptions.ShapeshifterCooldown = KillCooldown.GetFloat();
    }

    public override void UnShapeShiftButton(PlayerControl shapeshifter)
    {
        var pos = shapeshifter.GetTruePosition();
        _ = new LateTask(() =>
        {
            var dir = shapeshifter.GetTruePosition() - pos;
            var halfpos = shapeshifter.GetTruePosition() + (dir/2);
            var newpos = shapeshifter.GetTruePosition() + dir;
            if (newpos == pos)
            {
                shapeshifter.RpcMurderPlayer(shapeshifter);
                return;
            }

            foreach (var player in Main.AllAlivePlayerControls.Where(x => Vector2.Distance(x.transform.position, halfpos) < Distance.GetFloat() && x != shapeshifter))
            {
                player.SetRealKiller(shapeshifter);
                player.SetDeathReason(PlayerState.DeathReason.Execution);
                player.RpcMurderPlayer(player);
            }

            shapeshifter.RpcTeleport(newpos);
        }, 1f, "Lunger");
    }

    public override bool OnCheckMurderAsKiller(PlayerControl killer, PlayerControl target)
    {
        killer.RpcGuardAndKill();
        return false;
    }
}