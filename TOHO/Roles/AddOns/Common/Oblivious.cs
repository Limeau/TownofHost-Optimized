using static TOHO.Options;

namespace TOHO.Roles.Modifiers.Common;

public class Oblivious : IModifier
{
    public CustomRoles Role => CustomRoles.Oblivious;
    private const int Id = 20700;
    public ModifierTypes Type => ModifierTypes.Harmful;

    public static OptionItem ObliviousBaitImmune;

    public void SetupCustomOption()
    {
        SetupAdtRoleOptions(Id, CustomRoles.Oblivious, canSetNum: true, teamSpawnOptions: true);
        ObliviousBaitImmune = BooleanOptionItem.Create(Id + 13, "ObliviousBaitImmune", false, TabGroup.Modifiers, false).SetParent(CustomRoleSpawnChances[CustomRoles.Oblivious]);
    }
    public void Init()
    { }
    public void Add(byte playerId, bool gameIsLoading = true)
    { }
    public void Remove(byte playerId)
    { }

}
