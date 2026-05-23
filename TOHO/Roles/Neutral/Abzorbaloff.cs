using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using TOHO.Modules;
using TOHO.Roles.Core;
using UnityEngine.SocialPlatforms;
using static TOHO.Options;
namespace TOHO.Roles.Neutral;

internal class Abzorbaloff : RoleBase
{
    //===========================SETUP================================\\
    private const int Id = 39700;
    public static bool HasEnabled => CustomRoleManager.HasEnabled(CustomRoles.Abzorbaloff);
    public override CustomRoles Role => CustomRoles.Abzorbaloff;
    public override CustomRoles ThisRoleBase => CustomRoles.Impostor;
    public override Custom_RoleType ThisRoleType => Custom_RoleType.NeutralKilling;
    public override bool TOHORole => true;

    public override string LikedRole => "limeau";
    //==================================================================\\

    public static OptionItem AbzorbaloffMaxPlayers;
    public static OptionItem AbzorbaloffRange;
    public static List<PlayerControl> AbzPlayers = [];
    
    public override void SetupCustomOption()
    {
        SetupRoleOptions(Id, TabGroup.NeutralRoles, CustomRoles.Abzorbaloff);
        AbzorbaloffMaxPlayers = IntegerOptionItem.Create(Id + 10, "AbzMaxPlayers", new(1, 5, 1), 3, TabGroup.NeutralRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Abzorbaloff]);
        AbzorbaloffRange = FloatOptionItem.Create(Id + 11, "AbzRange", new(0.1f, 5f, 0.1f), 3f, TabGroup.NeutralRoles, false)
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
            if (player == null) continue;
            if (player == target || player == killer) continue;

            candidates.Add(player);
        }

        var souls = AbzPlayers.ToList(); // copy snapshot

        foreach (var soul in AbzPlayers)
        {
            PlayerControl soultarget = null;

            foreach (var player in candidates)
            {
                if (Utils.GetDistance(killer.transform.position, player.transform.position) <= AbzorbaloffRange.GetFloat())
                {
                    if (soultarget == null ||
                        Utils.GetDistance(killer.transform.position, player.transform.position) <
                        Utils.GetDistance(killer.transform.position, soultarget.transform.position))
                    {
                        soultarget = player;
                    }
                }
            }

            if (soultarget != null)
            {
                soul.RpcMurderPlayer(soultarget);
            }

            souls.Remove(soul);
        }
        
        if (!AbzPlayers.Contains(target) && AbzPlayers.Count < AbzorbaloffMaxPlayers.GetInt())
        {                
            AbzPlayers.Add(target);
        }
        
        return true;
    }
}