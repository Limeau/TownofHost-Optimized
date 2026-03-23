
namespace TOHO.Roles.Modifiers.Impostor;

public class Circumvent : IModifier
{
    public CustomRoles Role => CustomRoles.Circumvent;
    private const int Id = 22600;
    public ModifierTypes Type => ModifierTypes.Impostor;

    public void SetupCustomOption()
    {
        Options.SetupAdtRoleOptions(Id, CustomRoles.Circumvent, canSetNum: true, tab: TabGroup.Modifiers);
    }
    public void Init()
    { }
    public void Add(byte playerId, bool gameIsLoading = true)
    { }
    public void Remove(byte playerId)
    { }
    public static bool CantUseVent(PlayerControl player) => player.Is(CustomRoles.Circumvent);
}
