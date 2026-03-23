using static TOHO.Options;

namespace TOHO.Roles.Modifiers.Common;

public class Stubborn : IModifier
{
    public CustomRoles Role => CustomRoles.Stubborn;
    private const int Id = 22500;
    public ModifierTypes Type => ModifierTypes.Mixed;

    public void SetupCustomOption()
    {
        SetupAdtRoleOptions(Id, CustomRoles.Stubborn, canSetNum: true, teamSpawnOptions: true);
    }
    public void Init()
    { }
    public void Add(byte playerId, bool gameIsLoading = true)
    { }
    public void Remove(byte playerId)
    { }
}

