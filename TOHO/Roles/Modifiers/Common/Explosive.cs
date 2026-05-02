using System.Linq;
using UnityEngine;
using static TOHO.Options;

namespace TOHO.Roles.Modifiers.Common;

public class Explosive : IModifier
{
    public CustomRoles Role => CustomRoles.Explosive;
    private const int Id = 41400;
    public static bool IsEnable = false;
    public ModifierTypes Type => ModifierTypes.Helpful;

    private static OptionItem BombTimer;

    public static bool IsBombActive = false;
    public static PlayerControl BombHolder = null;
    
    public void SetupCustomOption()
    {
        SetupAdtRoleOptions(Id, CustomRoles.Explosive, canSetNum: true, teamSpawnOptions: true);
        BombTimer = FloatOptionItem.Create(Id + 10, "BombTimer414", new(1f,20f, 1f), 10f, TabGroup.Modifiers, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Explosive])
            .SetValueFormat(OptionFormat.Seconds);
    }

    public void Init()
    {
        IsEnable = false;
        IsBombActive = false;
    }

    public void Add(byte playerId, bool gameIsLoading = true)
    {
        IsEnable = true;
        IsBombActive = false;
    }

    public void Remove(byte playerId)
    {
        IsEnable = false;
        IsBombActive = false;
    }

    public static void CheckMurder(PlayerControl killer)
    {
        IsBombActive = true;
        BombHolder = killer;
        BombHolder.Notify(Utils.ColorString(Color.black, "YOU HAVE THE BOMB!!!"));
        _ = new LateTask(() =>
        {
            BombHolder.RpcMurderPlayer(BombHolder);
            BombHolder.SetDeathReason(PlayerState.DeathReason.Bombed);
            BombHolder = null;
        }, BombTimer.GetFloat(), "Explosive Bomb");
    }

    public static void OnFixedUpdate()
    {
        PlayerControl victim = null;
        if (BombHolder == null) return;
        foreach (var player in Main.AllAlivePlayerControls.Where(x => Utils.GetDistance(BombHolder.transform.position, x.transform.position) <= 2))
        {
            victim = player;
        }

        if (victim != null)
        {
            victim.Notify(Utils.ColorString(Color.black, "YOU HAVE THE BOMB!!!"));
            BombHolder.Notify(Utils.ColorString(Color.red, "Bomb passed"));
            BombHolder = victim;
        }
    }
    
}