using System.Collections.Generic;
using System.Linq;
using TOHO.Modules;
using static TOHO.Options;

namespace TOHO.Roles.Impostor;

internal class Staller : RoleBase
{
    //===========================SETUP================================\\
    public override CustomRoles Role => CustomRoles.Staller;
    private const int Id = 43300;
    public override CustomRoles ThisRoleBase => CustomRoles.Impostor;
    public override Custom_RoleType ThisRoleType => Custom_RoleType.ImpostorConcealing;
    public override bool TOHORole => true;
    //==================================================================\\

    private static OptionItem KillCooldown;
    private static OptionItem StallerReportDelay;

    public override void SetupCustomOption()
    {
        SetupRoleOptions(Id, TabGroup.ImpostorRoles, CustomRoles.Staller);
        KillCooldown = FloatOptionItem.Create(Id + 10, GeneralOption.KillCooldown, new(0f, 180f, 2.5f), 20f, TabGroup.ImpostorRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Staller])
            .SetValueFormat(OptionFormat.Seconds);
        StallerReportDelay = FloatOptionItem.Create(Id + 11, "StallerReportDelay", new(1f, 15f, 1f), 5f, TabGroup.ImpostorRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Staller])
            .SetValueFormat(OptionFormat.Seconds);
    }

    public override void SetKillCooldown(byte id) => Main.AllPlayerKillCooldown[id] = KillCooldown.GetFloat();
    public override bool CanUseKillButton(PlayerControl pc) => true;

    public override bool OnCheckReportDeadBody(PlayerControl reporter, NetworkedPlayerInfo deadBody, PlayerControl killer)
    {
        if (deadBody._object.GetRealKiller().Is(CustomRoles.Staller) || deadBody._object.Is(CustomRoles.Concealed)) // Concealed code also goes here to make it ez
        {
            _ = new LateTask(() =>
            {
                reporter.NoCheckStartMeeting(deadBody);
            }, StallerReportDelay.GetFloat(), "Staller Report Delay");
            return false;
        }

        return true;
    }
}