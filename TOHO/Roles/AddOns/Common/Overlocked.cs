using static TOHO.Options;

namespace TOHO.Roles.Modifiers.Common;

public class Overclocked : IModifier
{
    public CustomRoles Role => CustomRoles.Overclocked;
    private const int Id = 19800;
    public ModifierTypes Type => ModifierTypes.Helpful;

    public static OptionItem OverclockedReduction;

    public void SetupCustomOption()
    {
        SetupAdtRoleOptions(Id, CustomRoles.Overclocked, canSetNum: true);
        OverclockedReduction = FloatOptionItem.Create(Id + 10, "OverclockedReduction", new(0f, 90f, 5f), 40f, TabGroup.Modifiers, false).SetParent(CustomRoleSpawnChances[CustomRoles.Overclocked])
            .SetValueFormat(OptionFormat.Percent);
    }
    public void Init()
    { }
    public void Add(byte playerId, bool gameIsLoading = true)
    { }
    public void Remove(byte playerId)
    { }
}

