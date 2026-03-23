using static TOHO.Options;

namespace TOHO.Roles.Modifiers.Crewmate;

public class Peacemaker : IModifier
{
    public CustomRoles Role => CustomRoles.Peacemaker;
    private const int Id = 36900;
    public ModifierTypes Type => ModifierTypes.Helpful;

    public void SetupCustomOption()
    {
        SetupAdtRoleOptions(Id, CustomRoles.Peacemaker, canSetNum: true);
    }
    public void Init()
    { }
    public void Add(byte playerId, bool gameIsLoading = true)
    { }
    public void Remove(byte playerId)
    { }
}
