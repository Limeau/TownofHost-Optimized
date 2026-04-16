using System.Collections.Generic;
using AmongUs.GameOptions;
using TOHO.Modules;
using TOHO.Roles.Core;
using static TOHO.Options;
namespace TOHO.Roles.Neutral;

internal class Abzorbaloff : RoleBase
{
    //===========================SETUP================================\\
    private const int Id = 39000;
    public static bool HasEnabled => CustomRoleManager.HasEnabled(CustomRoles.Abzorbaloff);
    public override CustomRoles Role => CustomRoles.Abzorbaloff;
    public override CustomRoles ThisRoleBase => CustomRoles.Impostor;
    public override Custom_RoleType ThisRoleType => Custom_RoleType.NeutralKilling;
    //==================================================================\\

    public static OptionItem AbzorbaloffMaxPlayers;
    public static OptionItem AbzorbaloffRange;
    public static List<PlayerControl> AbzPlayers = [];
    
    public override void SetupCustomOption()
    {
        SetupRoleOptions(Id, TabGroup.NeutralRoles, CustomRoles.Abzorbaloff);
        AbzorbaloffMaxPlayers = IntegerOptionItem.Create(Id + 10, "AbzMaxPlayers", (1, 5, 1), 3, TabGroup.NeutralRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Abzorbaloff]);
        AbzorbaloffRange = FloatOptionItem.Create(Id + 10, "AbzRange", (0.1f, 5f, 0.1f), 3f, TabGroup.NeutralRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Abzorbaloff])
            .SetValueFormat(OptionFormat.Multiplier);
    }

    public override bool CanUseKillButton(PlayerControl pc)
    {
        return true;
    }

    public override bool OnCheckMurderAsKiller(PlayerControl killer, PlayerControl target)
    {
        List<PlayerControl> candidates = [];

        foreach (var player in Main.AllAlivePlayerControls)
        {
            if (player == killer) continue;
            if (player == target) continue;
            candidates.Add(player);
        }

        foreach (var soul in AbzPlayers)
        {
            PlayerControl currentTarget = null;
            foreach (var player in candidates)
            {
                if (Utils.GetDistance(killer.transform.position, player.transform.position) <=
                    AbzorbaloffRange.GetFloat())
                {
                    if (currentTarget == null)
                    {
                        currentTarget = player;
                    }
                    else if (Utils.GetDistance(killer.transform.position, player.transform.position) <=
                             Utils.GetDistance(killer.transform.position, currentTarget.transform.position))
                    {
                        currentTarget = player;
                    }
                }
            }

            candidates.Remove(currentTarget);
            soul.RpcMurderPlayer(currentTarget);
        }

        if (AbzPlayers.Count < AbzorbaloffMaxPlayers.GetInt()) AbzPlayers.Add(target);
        return true;
    }
}