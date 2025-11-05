using AmongUs.GameOptions;
using static TOHO.Options;

namespace TOHO.Roles.Neutral;

internal class Widow : RoleBase
{
    //===========================SETUP================================\\
    public override CustomRoles Role => CustomRoles.Widow;
    public override bool IsDesyncRole => true;
    public override CustomRoles ThisRoleBase => CustomRoles.Impostor;
    public override Custom_RoleType ThisRoleType => Custom_RoleType.NeutralKilling;
    //==================================================================\\
    
    public override void SetKillCooldown(byte id) => Main.AllPlayerKillCooldown[id] = DefaultKillCooldown;
    public override bool CanUseKillButton(PlayerControl pc) => true;
    public override bool CanUseImpostorVentButton(PlayerControl pc) => true;
}

