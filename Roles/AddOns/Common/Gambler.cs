﻿using static TOHE.Options;

namespace TOHE.Roles.AddOns.Common;

public class Gambler : IAddon
{
    public CustomRoles Role => CustomRoles.Gambler;
    private const int Id = 33100;
    public AddonTypes Type => AddonTypes.Mixed;

    public static Dictionary<byte, int> Gambles = [];
    private static readonly Dictionary<byte, bool> Gamble = [];

    public static OptionItem GambleUses;

    public void SetupCustomOption()
    {
        SetupAdtRoleOptions(Id, CustomRoles.Gambler, canSetNum: true, teamSpawnOptions: true);
        GambleUses = IntegerOptionItem.Create(Id + 10, "GambleUses", new(1, 4, 1), 1, TabGroup.Addons, false).SetParent(CustomRoleSpawnChances[CustomRoles.Gambler])
           .SetValueFormat(OptionFormat.Times);
    }
    public void Init()
    {
        Gambles.Clear();
        Gamble.Clear();
    }
    public void Add(byte playerId, bool gameIsLoading = true)
    {
        Gambles[playerId] = GambleUses.GetInt();
        Gamble[playerId] = false;
    }
    public void Remove(byte player)
    {
        Gambles.Remove(player);
        Gamble.Remove(player);
    }
    private static void AvoidDeathChance(PlayerControl killer, PlayerControl target)
    {
        var rd = IRandom.Instance;
        if (rd.Next(1, 3) <= 1)
        {
            killer.RpcGuardAndKill(target);
            Gamble[target.PlayerId] = true;
        }
    }
    public static bool OnCheckMurder(PlayerControl killer, PlayerControl target)
    {
        AvoidDeathChance(killer, target);
        if (Gamble[target.PlayerId])
        {
            Gamble[target.PlayerId] = false;
            return false;
        }
        return true;
    }
}

