
namespace TOHO.Roles.Modifiers.Common;

public class Silent : IModifier
{
    public CustomRoles Role => CustomRoles.Silent;
    private const int Id = 26600;
    public ModifierTypes Type => ModifierTypes.Helpful;
    public void SetupCustomOption()
    {
        Options.SetupAdtRoleOptions(Id, CustomRoles.Silent, canSetNum: true, tab: TabGroup.Modifiers, teamSpawnOptions: true);
    }
    public void Init()
    { }
    public void Add(byte playerId, bool gameIsLoading = true)
    { }
    public void Remove(byte playerId)
    { }
}
