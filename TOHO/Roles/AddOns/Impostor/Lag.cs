using static TOHO.Options;
namespace TOHO.Roles.AddOns.Impostor;
public class Lag : IAddon
{
    public CustomRoles Role => CustomRoles.Lag;
    private const int Id = 39200;
    public AddonTypes Type => AddonTypes.Impostor;
    public void SetupCustomOption()
    { SetupAdtRoleOptions(Id, CustomRoles.Lag, canSetNum: true, tab: TabGroup.Addons); }
    public void Init() { }
    public void Add(byte playerId, bool gameIsLoading = true) { }
    public void Remove(byte playerId) { }
    public static bool OnCheckMurder(PlayerControl killer, PlayerControl target)
    { new LateTask(() => { killer.RpcMurderPlayer(target); }, 1f, "Lag addon"); return false; }
}