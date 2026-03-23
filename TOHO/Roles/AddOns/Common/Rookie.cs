using static TOHO.Options;

namespace TOHO.Roles.Modifiers.Common;

public class Rookie : IModifier
{
    public CustomRoles Role => CustomRoles.Rookie;
    private const int Id = 38800;
    public ModifierTypes Type => ModifierTypes.Harmful;
    public static OptionItem RookieChance;
    public void SetupCustomOption()
    {
        SetupAdtRoleOptions(Id, CustomRoles.Rookie, canSetNum: true, teamSpawnOptions: true);
        RookieChance = IntegerOptionItem.Create(Id + 10, "RookieChance", (5, 100, 5), 50, TabGroup.Modifiers, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Rookie])
            .SetValueFormat(OptionFormat.Percent);
    }
    public void Init()
    { }
    public void Add(byte playerId, bool gameIsLoading = true)
    { }
    public void Remove(byte playerId)
    { }

    public static void OnTaskComplete(PlayerControl player)
    {
        var rand = IRandom.Instance;

        if (rand.Next(1, 100) <= RookieChance.GetInt()) player.RpcResetTasks();
        return;
    }
}
