using System.Linq;
using static TOHO.Options;
namespace TOHO.Roles.Impostor;

internal class Pathogen : RoleBase
{
    public override CustomRoles Role => CustomRoles.Pathogen;
    private const int Id = 45200;
    public override CustomRoles ThisRoleBase => CustomRoles.Shapeshifter;
    public override Custom_RoleType ThisRoleType => Custom_RoleType.ImpostorHindering;
    public override bool TOHORole => true;
    public override void SetupCustomOption()
    {
        SetupRoleOptions(Id, TabGroup.ImpostorRoles, CustomRoles.Pathogen);
    }

    public override bool OnCheckShapeshift(PlayerControl shapeshifter, PlayerControl target, ref bool resetCooldown,
        ref bool shouldAnimate)
    {
        RepeatShift(target);
        shapeshifter.RpcResetAbilityCooldown();
        return false;
    }

    public static void RepeatShift(PlayerControl target)
    {
        if (GameStates.IsMeeting) return;
        target.RpcShapeshift(target, true);
        _ = new LateTask(() =>
        {
            RepeatShift(target);
        }, 1f, "Repeat Shift");
    }
}