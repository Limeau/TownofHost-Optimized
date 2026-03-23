using static TOHO.Options;

namespace TOHO.Roles.Modifiers.Impostor;
public class Mimic : IModifier
{
    public CustomRoles Role => CustomRoles.Mimic;
    private const int Id = 23100;
    public ModifierTypes Type => ModifierTypes.Impostor;

    private static OptionItem CanSeeDeadRolesOpt;
    public void SetupCustomOption()
    {
        SetupAdtRoleOptions(Id, CustomRoles.Mimic, canSetNum: true, tab: TabGroup.Modifiers);
        CanSeeDeadRolesOpt = BooleanOptionItem.Create(Id + 10, "MimicCanSeeDeadRoles", true, TabGroup.Modifiers, false).SetParent(CustomRoleSpawnChances[CustomRoles.Mimic]);
    }
    public void Init()
    { }
    public void Add(byte playerId, bool gameIsLoading = true)
    { }
    public void Remove(byte playerId)
    { }
    public static bool CanSeeDeadRoles(PlayerControl seer, PlayerControl target) => seer.Is(CustomRoles.Mimic) && CanSeeDeadRolesOpt.GetBool() && Main.VisibleTasksCount && !target.IsAlive();
}
