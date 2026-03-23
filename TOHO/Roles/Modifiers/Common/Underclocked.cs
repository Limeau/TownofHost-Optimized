using static TOHO.Options;

namespace TOHO.Roles.Modifiers.Common;

public class Underclocked : IModifier
{
    public CustomRoles Role => CustomRoles.Underclocked;
    private const int Id = 35900;
    public ModifierTypes Type => ModifierTypes.Harmful;

    public static OptionItem UnderclockedIncrease;

    public void SetupCustomOption()
    {
        SetupAdtRoleOptions(Id, CustomRoles.Underclocked, canSetNum: true);
        UnderclockedIncrease = FloatOptionItem.Create(Id + 10, "UnderclockedIncrease", new(5f, 100f, 2.5f), 40f, TabGroup.Modifiers, false).SetParent(CustomRoleSpawnChances[CustomRoles.Underclocked])
            .SetValueFormat(OptionFormat.Percent);
    }
    public void Init()
    { }
    public void Add(byte playerId, bool gameIsLoading = true)
    { }
    public void Remove(byte playerId)
    { }
}
