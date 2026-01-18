namespace TOHO.Roles.AddOns.Common;

public class Subversion : IAddon
{
    public CustomRoles Role => CustomRoles.Subversion;
    private const int Id = 38300;
    public AddonTypes Type => AddonTypes.Helpful;
    public static bool IsEnable = false;

    public void SetupCustomOption()
    {
        Options.SetupAdtRoleOptions(Id, CustomRoles.Subversion, canSetNum: true, tab: TabGroup.Addons, teamSpawnOptions: true);
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
