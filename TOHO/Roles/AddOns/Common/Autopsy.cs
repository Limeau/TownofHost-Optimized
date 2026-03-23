using static TOHO.Options;

namespace TOHO.Roles.Modifiers.Common;

public class Autopsy : IModifier
{
    public CustomRoles Role => CustomRoles.Autopsy;
    private const int Id = 18600;
    public ModifierTypes Type => ModifierTypes.Helpful;
    public void SetupCustomOption()
    {
        SetupAdtRoleOptions(Id, CustomRoles.Autopsy, canSetNum: true, teamSpawnOptions: true);
    }
    public void Init()
    { }
    public void Add(byte playerId, bool gameIsLoading = true)
    { }
    public void Remove(byte playerId)
    { }
}
