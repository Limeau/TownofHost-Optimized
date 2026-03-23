using static TOHO.Options;

namespace TOHO.Roles.Modifiers.Common;

public class Loyal : IModifier
{
    public CustomRoles Role => CustomRoles.Loyal;
    private const int Id = 19400;
    public ModifierTypes Type => ModifierTypes.Helpful;

    public static OptionItem ImpCanBeLoyal;
    public static OptionItem CrewCanBeLoyal;
    public static OptionItem CovenCanBeLoyal;
    public void SetupCustomOption()
    {
        SetupAdtRoleOptions(Id, CustomRoles.Loyal, canSetNum: true);
        ImpCanBeLoyal = BooleanOptionItem.Create(Id + 10, "ImpCanBeLoyal", true, TabGroup.Modifiers, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Loyal]);
        CrewCanBeLoyal = BooleanOptionItem.Create(Id + 11, "CrewCanBeLoyal", true, TabGroup.Modifiers, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Loyal]);
        CovenCanBeLoyal = BooleanOptionItem.Create(Id + 12, "CovenCanBeLoyal", true, TabGroup.Modifiers, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Loyal]);
    }
    public void Init()
    { }
    public void Add(byte playerId, bool gameIsLoading = true)
    { }
    public void Remove(byte playerId)
    { }
}
