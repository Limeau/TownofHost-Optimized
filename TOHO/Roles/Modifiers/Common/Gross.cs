using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static TOHO.Options;

namespace TOHO.Roles.Modifiers.Common;

public class Gross : IModifier
{
    public CustomRoles Role => CustomRoles.Gross;
    public static bool IsEnable = false;
    private const int Id = 43200;
    public ModifierTypes Type => ModifierTypes.Mixed;
    private static Dictionary<PlayerControl, Vector3> Position = [];
    public static Dictionary<PlayerControl, bool> HasBeenMurdered = [];
    public void SetupCustomOption()
    {
        SetupAdtRoleOptions(Id, CustomRoles.Gross, canSetNum: true, teamSpawnOptions: true);
    }

    public void Init()
    {
        IsEnable = false;
        Position.Clear();
    }

    public void Add(byte playerId, bool gameIsLoading = true)
    {
        foreach (var player in Main.AllPlayerControls)
        {
            HasBeenMurdered[player] = false;
        }
        IsEnable = true;
    }
    public void Remove(byte playerId)
    { }

    public static void CheckMurder(PlayerControl killer, PlayerControl target)
    {
        Position[target] = target.transform.position;
    }

    public static void AfterMeeting()
    {
        foreach (var entry in Position.Where(x => !x.Key.IsAlive()))
        {
            HasBeenMurdered[entry.Key] = true;
            _ = new LateTask(() =>
            {
                entry.Key.RpcTeleport(entry.Value);
                entry.Key.RpcMurderPlayer(entry.Key);
            }, 1f, "Gross");
        }
    }
}