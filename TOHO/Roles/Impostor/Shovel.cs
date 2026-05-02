using System.Collections.Generic;
using AmongUs.GameOptions;
using TOHO.Modules;
using TOHO.Roles.Core;
using static TOHO.Options;

namespace TOHO.Roles.Impostor;

internal class Shovel : RoleBase
{
    //===========================SETUP================================\\
    public override CustomRoles Role => CustomRoles.Shovel;
    private const int Id = 41200;
    public static bool HasEnabled => CustomRoleManager.HasEnabled(CustomRoles.Shovel);
    public override CustomRoles ThisRoleBase => CustomRoles.Shapeshifter;
    public override Custom_RoleType ThisRoleType => Custom_RoleType.ImpostorConcealing;
    //==================================================================\\

    private static OptionItem KillCooldown;
    
    public override void SetupCustomOption()
    {
        SetupRoleOptions(Id, TabGroup.ImpostorRoles, CustomRoles.Shovel);
        KillCooldown = FloatOptionItem.Create(Id + 10, GeneralOption.KillCooldown, new(1f, 180f, 1f), 20f, TabGroup.ImpostorRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.Shovel])
            .SetValueFormat(OptionFormat.Seconds);
    }
    
    public override void SetKillCooldown(byte id) => Main.AllPlayerKillCooldown[id] = KillCooldown.GetFloat();

    public override void ApplyGameOptions(IGameOptions opt, byte playerId)
    {
        AURoleOptions.ShapeshifterCooldown = 1;
    }

    public override void UnShapeShiftButton(PlayerControl shapeshifter)
    {
        shapeshifter.RpcTeleport(shapeshifter.GetClosestVent().transform.position);
        shapeshifter.MyPhysics.RpcEnterVent(shapeshifter.GetClosestVent().Id);
    }
}