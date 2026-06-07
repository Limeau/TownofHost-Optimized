using System.Linq;
using AmongUs.GameOptions;
using TOHO.Modules;
using TOHO.Roles.Core;
using UnityEngine;
using static TOHO.Options;
namespace TOHO.Roles.Neutral;

internal class Swan : RoleBase
{
    //===========================SETUP================================\\
    private const int Id = 43000;
    public static bool HasEnabled => CustomRoleManager.HasEnabled(CustomRoles.Swan);
    public override CustomRoles Role => CustomRoles.Swan;
    public override CustomRoles ThisRoleBase => CustomRoles.Impostor;
    public override Custom_RoleType ThisRoleType => Custom_RoleType.NeutralKilling;
    public override bool TOHORole => true;

    public override string IdeaRole => "the_little_pelican";
    //==================================================================\\

    public static OptionItem KillCooldown;
    
    public override void SetupCustomOption()
    {
        SetupRoleOptions(Id, TabGroup.NeutralRoles, CustomRoles.Swan);
        KillCooldown = FloatOptionItem.Create(Id + 10, GeneralOption.KillCooldown, (1f, 60f, 1f), 20f, TabGroup.NeutralRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Swan])
            .SetValueFormat(OptionFormat.Seconds);
    }

    public override bool CanUseKillButton(PlayerControl pc)
    {
        return true;
    }

    public override void SetKillCooldown(byte id)
    {
        Main.AllPlayerKillCooldown[id] = KillCooldown.GetFloat();
    }

    public override bool OnCheckReportDeadBody(PlayerControl reporter, NetworkedPlayerInfo deadBody,
        PlayerControl killer)
    {
        if (Main.UnreportableBodies.Contains(deadBody.PlayerId)) return false;

        if (reporter.Is(CustomRoles.Swan))
        {
            Main.UnreportableBodies.Add(deadBody.PlayerId);
            reporter.SetKillCooldownV3(KillCooldown.GetFloat(), forceAnime: true);
            return false;
        }

        return true;
    }
}