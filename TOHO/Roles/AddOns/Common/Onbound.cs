using static TOHO.Options;

namespace TOHO.Roles.Modifiers.Common;

public class Onbound : IModifier
{
    public CustomRoles Role => CustomRoles.Onbound;
    private const int Id = 25800;
    public ModifierTypes Type => ModifierTypes.Guesser;

    public void SetupCustomOption()
    {
        SetupAdtRoleOptions(Id, CustomRoles.Onbound, canSetNum: true, tab: TabGroup.Modifiers, teamSpawnOptions: true);
    }
    public void Init()
    { }
    public void Add(byte playerId, bool gameIsLoading = true)
    { }
    public void Remove(byte playerId)
    { }
}

