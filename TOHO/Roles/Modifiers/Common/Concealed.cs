using static TOHO.Options;

namespace TOHO.Roles.Modifiers.Common;

public class Concealed : IModifier
{
    public CustomRoles Role => CustomRoles.Concealed;
    private const int Id = 43400;
    public ModifierTypes Type => ModifierTypes.Harmful;

    public void SetupCustomOption()
    {
        SetupAdtRoleOptions(Id, CustomRoles.Concealed, canSetNum: true, teamSpawnOptions: true);
    }
    public void Init()
    { }
    public void Add(byte playerId, bool gameIsLoading = true)
    { }
    public void Remove(byte playerId)
    { }
}