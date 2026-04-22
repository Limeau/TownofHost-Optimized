namespace TOHO.Roles.Modifiers.Common;

public class Subversion : IModifier
{
    public CustomRoles Role => CustomRoles.Subversion;
    private const int Id = 38300;
    public ModifierTypes Type => ModifierTypes.Impostor;
    public static bool IsEnable = false;

    public void SetupCustomOption()
    {
        Options.SetupAdtRoleOptions(Id, CustomRoles.Subversion, canSetNum: true, tab: TabGroup.Modifiers, teamSpawnOptions: false);
    }

    public void Init()
    {
    }

    public void Add(byte playerId, bool gameIsLoading = true)
    {
    }
    public void Remove(byte playerId)
    {
    }
}
