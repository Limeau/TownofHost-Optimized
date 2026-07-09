using System.Collections.Generic;
using AmongUs.GameOptions;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using static TOHO.Options;

namespace TOHO.Roles.Modifiers.Crewmate;

public class Rage : IModifier
{
    public CustomRoles Role => CustomRoles.Rage;
    private const int Id = 44900;
    public ModifierTypes Type => ModifierTypes.Mixed;

    private static OptionItem ExtraTasks;
    private static OptionItem KillCooldown;

    public static OptionItem RageAffectTaskRole;

    private static OptionItem AllowShortTasks;
    private static OptionItem AllowCommonTasks;
    private static OptionItem AllowLongTasks;

    private static readonly HashSet<byte> RageKillers = [];

    public void SetupCustomOption()
    {
        SetupAdtRoleOptions(Id, CustomRoles.Rage, canSetNum: true);

        ExtraTasks = IntegerOptionItem.Create(Id + 10, "RageExtraTasks", new(1, 10, 1), 4, TabGroup.Modifiers, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Rage])
            .SetValueFormat(OptionFormat.Pieces);

        KillCooldown = IntegerOptionItem.Create(Id + 11, "RageKillCooldown", new(10, 60, 5), 15, TabGroup.Modifiers, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Rage])
            .SetValueFormat(OptionFormat.Seconds);

        RageAffectTaskRole = BooleanOptionItem.Create(Id + 12, "RageAffectTaskRole", false, TabGroup.Modifiers, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Rage]);

        AllowShortTasks = BooleanOptionItem.Create(Id + 13, "RageAllowShortTasks", true, TabGroup.Modifiers, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Rage]);
        AllowCommonTasks = BooleanOptionItem.Create(Id + 14, "RageAllowCommonTasks", true, TabGroup.Modifiers, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Rage]);
        AllowLongTasks = BooleanOptionItem.Create(Id + 15, "RageAllowLongTasks", false, TabGroup.Modifiers, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Rage]);
    }

    public void Init()
    {
        RageKillers.Clear();
    }

    public void Add(byte playerId, bool gameIsLoading = true)
    {
        if (!AmongUsClient.Instance.AmHost) return;

        var pc = playerId.GetPlayer();
        if (pc?.Data == null) return;

        pc.Data.RpcSetTasks(new Il2CppStructArray<byte>(0));
    }

    public void Remove(byte playerId)
    {
        RageKillers.Remove(playerId);
    }

    public static (bool hasCommonTasks, int numLongTasks, int numShortTasks) ApplyExtraTasks(bool hasCommonTasks, int numLongTasks, int numShortTasks)
    {
        if (!AllowCommonTasks.GetBool())
            hasCommonTasks = false;

        var extra = ExtraTasks.GetInt();
        var allowShort = AllowShortTasks.GetBool();
        var allowLong = AllowLongTasks.GetBool();

        if (allowShort && allowLong)
        {
            var half = extra / 2;
            numShortTasks += half + (extra % 2);
            numLongTasks += half;
        }
        else if (allowShort)
        {
            numShortTasks += extra;
        }
        else if (allowLong)
        {
            numLongTasks += extra;
        }

        return (hasCommonTasks, numLongTasks, numShortTasks);
    }

    public static bool HasRageKill(byte playerId) => RageKillers.Contains(playerId);

    public static float GetKillCooldown() => KillCooldown.GetInt();

    public static void OnTaskComplete(PlayerControl player)
    {
        if (RageKillers.Contains(player.PlayerId)) return;

        RageKillers.Add(player.PlayerId);
        Main.AllPlayerKillCooldown[player.PlayerId] = KillCooldown.GetInt();

        player.RpcSetRoleDesync(RoleTypes.Impostor, player.GetClientId());

        player.Notify(Utils.ColorString(Utils.GetRoleColor(CustomRoles.Rage), Translator.GetString("RageKillButtonGranted")));
    }
}