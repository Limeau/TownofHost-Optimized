using static TOHO.Options;

namespace TOHO.Roles.Modifiers.Crewmate;

public class Rascal : IModifier
{
    public CustomRoles Role => CustomRoles.Rascal;
    private const int Id = 20800;
    public ModifierTypes Type => ModifierTypes.Harmful;

    private static OptionItem RascalAppearAsMadmate;

    public void SetupCustomOption()
    {
        SetupAdtRoleOptions(Id, CustomRoles.Rascal, canSetNum: true, tab: TabGroup.Modifiers);
        RascalAppearAsMadmate = BooleanOptionItem.Create(Id + 10, "RascalAppearAsMadmate", true, TabGroup.Modifiers, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Rascal]);
    }
    public void Init()
    { }
    public void Add(byte playerId, bool gameIsLoading = true)
    { }
    public void Remove(byte playerId)
    { }
    public static bool AppearAsMadmate(PlayerControl player) => RascalAppearAsMadmate.GetBool() && player.Is(CustomRoles.Rascal);
}
