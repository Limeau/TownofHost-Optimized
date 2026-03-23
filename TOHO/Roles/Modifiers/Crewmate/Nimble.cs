using static TOHO.Options;

namespace TOHO.Roles.Modifiers.Crewmate;

public class Nimble : IModifier
{
    public CustomRoles Role => CustomRoles.Nimble;
    private const int Id = 19700;
    public ModifierTypes Type => ModifierTypes.Helpful;

    public void SetupCustomOption()
    {
        SetupAdtRoleOptions(Id, CustomRoles.Nimble, canSetNum: true, tab: TabGroup.Modifiers);
    }
    public void Init()
    { }
    public void Add(byte playerId, bool gameIsLoading = true)
    { }
    public void Remove(byte playerId)
    { }
}
