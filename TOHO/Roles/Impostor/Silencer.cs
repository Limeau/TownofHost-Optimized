using System.Collections.Generic;
using System.Linq;
using TOHO.Modules;
using static TOHO.Options;

namespace TOHO.Roles.Impostor;

internal class Silencer : RoleBase
{
    //===========================SETUP================================\\
    public override CustomRoles Role => CustomRoles.Silencer;
    private const int Id = 46600;
    public override CustomRoles ThisRoleBase => CustomRoles.Impostor;
    public override Custom_RoleType ThisRoleType => Custom_RoleType.ImpostorConcealing;
    public override bool TOHORole => true;
    public override bool NewRole => true;
    public override string IdeaRole => "zanyfee";
    //==================================================================\\

    private static OptionItem KillCooldown;
    private static OptionItem SilencerReportDelay;
    public static bool IsSilence;

    public override void SetupCustomOption()
    {
        SetupRoleOptions(Id, TabGroup.ImpostorRoles, CustomRoles.Silencer);
        KillCooldown = FloatOptionItem.Create(Id + 10, GeneralOption.KillCooldown, new(0f, 180f, 2.5f), 20f, TabGroup.ImpostorRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Silencer])
            .SetValueFormat(OptionFormat.Seconds);
        SilencerReportDelay = FloatOptionItem.Create(Id + 11, "SilencerReportDelay", new(1f, 15f, 1f), 5f, TabGroup.ImpostorRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Silencer])
            .SetValueFormat(OptionFormat.Seconds);
    }

    public override void Add(byte playerId)
    {
        IsSilence = false;
    }

    public override void SetKillCooldown(byte id) => Main.AllPlayerKillCooldown[id] = KillCooldown.GetFloat();
    public override bool CanUseKillButton(PlayerControl pc) => true;

    public override bool OnCheckReportDeadBody(PlayerControl reporter, NetworkedPlayerInfo deadBody, PlayerControl killer)
    {
        if (IsSilence) return false;
        return true;
    }

    public override bool OnCheckStartMeeting(PlayerControl reporter)
    {
        if (IsSilence) return false;
        return true;
    }

    public override bool OnCheckMurderAsKiller(PlayerControl killer, PlayerControl target)
    {
        IsSilence = true;
        _ = new LateTask(() =>
        {
            IsSilence = false;
        }, SilencerReportDelay.GetFloat(), "Silencer Report Delay");
        return true;
    }
}