using static TOHO.Options;
namespace TOHO.Roles.Modifiers.Impostor;
public class Lag : IModifier
{
    public CustomRoles Role => CustomRoles.Lag;
    private const int Id = 39200;
    public ModifierTypes Type => ModifierTypes.Impostor;
    public void SetupCustomOption()
    { SetupAdtRoleOptions(Id, CustomRoles.Lag, canSetNum: true, tab: TabGroup.Modifiers); }
    public void Init() { }
    public void Add(byte playerId, bool gameIsLoading = true) { }
    public void Remove(byte playerId) { }
    public static bool OnCheckMurder(PlayerControl killer, PlayerControl target)
    { new LateTask(() => { killer.RpcMurderPlayer(target); }, 1f, "Lag Modifier"); return false; }
}