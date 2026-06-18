using System.Collections.Generic;
using UnityEngine;
using static TOHO.Options;

namespace TOHO.Roles.Crewmate;

internal class Raven : RoleBase
{
    //===========================SETUP================================\\
    public override CustomRoles Role => CustomRoles.Raven;
    private const int Id = 44000;
    public override bool IsDesyncRole => true;
    public override CustomRoles ThisRoleBase => CustomRoles.Crewmate;
    public override Custom_RoleType ThisRoleType => Custom_RoleType.CrewmateInvestigative;
    public override bool TOHORole => true;
    public override bool NewRole => true;
    //==================================================================\\

    public static List<RavenWatchKill> KilledPlaces = [];
    
    public override void SetupCustomOption()
    {
        SetupRoleOptions(Id, TabGroup.CrewmateRoles, CustomRoles.Raven);
    }

    public override bool CheckMurderOnOthersTarget(PlayerControl killer, PlayerControl target)
    {
        var raven = new RavenWatchKill();
        raven.position = target.transform.position;
        raven.target = target;
        raven.killer = killer;
        KilledPlaces.Add(raven);
        return false;
    }

    public static void RandomSelection(PlayerControl killer, PlayerControl target, PlayerControl raven)
    {
        switch (IRandom.Instance.Next(4))
        {
            case 0:
                raven.Notify($"The player killed here was {target.GetRealName().RemoveHtmlTags()}");
                break;
            case 1:
                raven.Notify($"The victim's role is {Translator.GetString(target.GetCustomRole().ToString())}");
                break;
            case 2:
                raven.Notify($"The killer's role is {Translator.GetString(killer.GetCustomRole().ToString())}");
                break;
            case 3:
                if (killer.IsAlive()) raven.Notify($"The killer of this site is alive");
                else raven.Notify($"The killer of this site is not alive");
                break;
        }
    }

    public override void OnFixedUpdate(PlayerControl player, bool lowLoad, long nowTime, int timerLowLoad)
    {
        if (!player.Is(CustomRoles.Raven) || !player.IsAlive()) return;

        foreach (var kill in KilledPlaces)
        {
            if (Utils.GetDistance(player.transform.position, kill.position) <= 1f)
            {
                RandomSelection(kill.killer, kill.target, player);
                KilledPlaces.Remove(kill);
            }
        }
    }
}

class RavenWatchKill
{
    public Vector3 position;
    public PlayerControl target;
    public PlayerControl killer;
}