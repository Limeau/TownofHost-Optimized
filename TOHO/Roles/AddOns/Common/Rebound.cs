using static TOHO.Options;


namespace TOHO.Roles.Modifiers.Common;

public class Rebound : IModifier
{
    public CustomRoles Role => CustomRoles.Rebound;
    private const int Id = 22300;
    public ModifierTypes Type => ModifierTypes.Guesser;

    public void SetupCustomOption()
    {
        SetupAdtRoleOptions(Id, CustomRoles.Rebound, canSetNum: true, tab: TabGroup.Modifiers, teamSpawnOptions: true);
    }
    public void Init()
    { }
    public void Add(byte playerId, bool gameIsLoading = true)
    { }
    public void Remove(byte playerId)
    { }
}
