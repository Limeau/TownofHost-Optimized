using TOHO.Roles.Core;
using static TOHO.Options;
namespace TOHO.Roles.Neutral;

internal class Sonic : RoleBase
{
    //===========================SETUP================================\\
    private const int Id = 32100;
    public static bool HasEnabled => CustomRoleManager.HasEnabled(CustomRoles.Sonic);
    public override CustomRoles Role => CustomRoles.Sonic;
    public override CustomRoles ThisRoleBase => CustomRoles.Impostor;
    public override Custom_RoleType ThisRoleType => Custom_RoleType.NeutralKilling;
    //==================================================================\\

    public static OptionItem SonicKillCooldown;
    public override void SetupCustomOption()
    {
        SetupRoleOptions(Id, TabGroup.NeutralRoles, CustomRoles.Sonic);
        SonicKillCooldown = FloatOptionItem.Create(Id + 2, GeneralOption.KillCooldown, new(1f, 60f, 1f), 20f, TabGroup.NeutralRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.Sonic])
            .SetValueFormat(OptionFormat.Seconds);
    }
    public override void SetKillCooldown(byte id)
    {
        Main.AllPlayerKillCooldown[id] = SonicKillCooldown.GetFloat();
    }

    public override bool CanUseKillButton(PlayerControl pc) => true;
}

