using System.Collections.Generic;
using System.Linq;
using TOHO.Modules;
using static TOHO.Options;

namespace TOHO.Roles.Impostor;

internal class Threat : RoleBase
{
    //===========================SETUP================================\\
    public override CustomRoles Role => CustomRoles.Threat;
    private const int Id = 41300;
    public override CustomRoles ThisRoleBase => CustomRoles.Impostor;
    public override Custom_RoleType ThisRoleType => Custom_RoleType.ImpostorKilling;
    public override bool TOHORole => true;
    //==================================================================\\

    private static OptionItem KillCooldown;

    private static Dictionary<PlayerControl, PlayerControl> DeathPlayers = [];
    
    public override void SetupCustomOption()
    {
        SetupRoleOptions(Id, TabGroup.ImpostorRoles, CustomRoles.Threat);
        KillCooldown = FloatOptionItem.Create(Id + 10, GeneralOption.KillCooldown, new(0f, 180f, 2.5f), 20f, TabGroup.ImpostorRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Threat])
            .SetValueFormat(OptionFormat.Seconds);
    }

    public override void SetKillCooldown(byte id) => Main.AllPlayerKillCooldown[id] = KillCooldown.GetFloat();

    public override bool OnCheckMurderAsKiller(PlayerControl killer, PlayerControl target)
    {
        DeathPlayers.Add(target, killer);
        killer.RpcGuardAndKill(fromSetKCD: false);
        return false;
    }

    public override void OnMeetingHudStart(PlayerControl pc)
    {
        foreach (var kvp in DeathPlayers)
        {
            kvp.Value.KillWithoutBody(kvp.Key);
            Utils.SendMessage("You have died due to the Threat killing you earlier this round.", sendTo: kvp.Key.PlayerId);
        }
    }
}