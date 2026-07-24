using AmongUs.GameOptions;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static TOHO.Options;

namespace TOHO.Roles.Crewmate;

internal class Protester : RoleBase
{
    //===========================SETUP================================\\
    public override CustomRoles Role => CustomRoles.Protester;
    private const int Id = 42100;
    public override CustomRoles ThisRoleBase => CustomRoles.Crewmate;
    public override Custom_RoleType ThisRoleType => Custom_RoleType.CrewmateKilling;
    public override bool TOHORole => true;
    //==================================================================\\
    public static OptionItem ProtestRadius;
    public static OptionItem MinPlayers;
    public override void SetupCustomOption()
    {
        SetupRoleOptions(Id, TabGroup.CrewmateRoles, CustomRoles.Protester);
        ProtestRadius = FloatOptionItem.Create(Id + 10, "ProtestRadius", new(1f, 3f, 0.1f), 2f, TabGroup.CrewmateRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Protester])
            .SetValueFormat(OptionFormat.Multiplier);
        MinPlayers = IntegerOptionItem.Create(Id + 11, "MinPlayersProtest", new(3, 10, 1), 5, TabGroup.CrewmateRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Protester]);
    }

    public override void OnFixedUpdate(PlayerControl protester, bool lowLoad, long nowTime, int timerLowLoad)
    {
        List<PlayerControl> Impostors = [];
        List<PlayerControl> CrowdMembers = [];
        foreach (var player in Main.AllAlivePlayerControls)
        {
            if (Utils.GetDistance(protester.transform.position, player.transform.position) <= ProtestRadius.GetFloat())
            {
                if (player.IsPlayerImpostorTeam())
                {
                    Impostors.Add(player);
                }
                CrowdMembers.Add(player);
            }
        }

        if (CrowdMembers.Count >= MinPlayers.GetInt() && Impostors.Any()) foreach (var impostor in Impostors)
        {
            foreach (var member in CrowdMembers.Where(x => !Impostors.Contains(x)))
            {
                member.RpcMurderPlayer(impostor);
            }
        }
    }
}
