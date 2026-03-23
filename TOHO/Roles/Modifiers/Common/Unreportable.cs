using static TOHO.Options;

namespace TOHO.Roles.Modifiers.Common;

public class Unreportable : IModifier
{
    public CustomRoles Role => CustomRoles.Unreportable;
    private const int Id = 20500;
    public ModifierTypes Type => ModifierTypes.Harmful;

    public void SetupCustomOption()
    {
        SetupAdtRoleOptions(Id, CustomRoles.Unreportable, canSetNum: true, teamSpawnOptions: true);
    }
    public void Init()
    { }
    public void Add(byte playerId, bool gameIsLoading = true)
    { }
    public void Remove(byte playerId)
    { }
}
