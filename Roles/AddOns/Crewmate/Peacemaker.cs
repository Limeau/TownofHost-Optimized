using AmongUs.GameOptions;
using static TOHO.Options;

namespace TOHO.Roles.AddOns.Crewmate;

public class Peacemaker : IAddon
{
    public CustomRoles Role => CustomRoles.Peacemaker;
    private const int Id = 36900;
    public AddonTypes Type => AddonTypes.Helpful;

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
