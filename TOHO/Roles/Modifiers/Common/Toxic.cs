using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using UnityEngine;

namespace TOHO.Roles.Modifiers.Common;

public class Toxic : IModifier
{
    public CustomRoles Role => CustomRoles.Toxic;
    private const int Id = 42600;
    public ModifierTypes Type => ModifierTypes.Harmful;
    public static bool IsEnable = false;

    private static OptionItem ToxicTime;
    private static OptionItem ToxicRadius;

    private static readonly Dictionary<byte, float> TimePlayer = [];

    public void SetupCustomOption()
    {
        Options.SetupAdtRoleOptions(Id, CustomRoles.Toxic, canSetNum: true, tab: TabGroup.Modifiers, teamSpawnOptions: true);
        ToxicTime = FloatOptionItem.Create(Id + 10, "ToxicTime", new(1f, 10f, 0.5f), 5f, TabGroup.Modifiers, false).SetParent(Options.CustomRoleSpawnChances[CustomRoles.Toxic])
             .SetValueFormat(OptionFormat.Seconds);
        ToxicRadius = FloatOptionItem.Create(Id + 11, "ToxicRadius", new(1f, 3f, 0.5f), 1.5f, TabGroup.Modifiers, false).SetParent(Options.CustomRoleSpawnChances[CustomRoles.Toxic])
             .SetValueFormat(OptionFormat.Multiplier);
    }

    public void Init()
    {
        TimePlayer.Clear();
        IsEnable = false;
    }
    public void Remove(byte playerId)
    {
        TimePlayer.Clear();
        IsEnable = false;
    }

    public void Add(byte playerId, bool gameIsLoading = true)
    {
        var speed = Main.AllPlayerSpeed[playerId];
        foreach (var player in Main.AllPlayerControls)
        {
            TimePlayer[player.PlayerId] = 0;
        }
    }

    public void OnFixedUpdate(PlayerControl victim)
    {
        if (!victim.Is(CustomRoles.Toxic) || GameStates.IsMeeting || (!victim.IsAlive() && victim == null)) return;

        foreach (var PVC in Main.AllPlayerControls.Where(x => !x.Is(CustomRoles.Toxic)))
        {
            if (PVC.IsAlive() && Utils.GetDistance(PVC.transform.position, victim.transform.position) < ToxicRadius.GetFloat())
            {   
                TimePlayer[PVC.PlayerId] += Time.deltaTime;
            }
            else if (TimePlayer[PVC.PlayerId] > 0) TimePlayer[PVC.PlayerId] = 0;

            if (TimePlayer[PVC.PlayerId] >= ToxicTime.GetFloat())
            {
                PVC.RpcTeleport(PVC.GetClosestVent().transform.position);
                TimePlayer[PVC.PlayerId] = 0;
            }
        }
    }
}