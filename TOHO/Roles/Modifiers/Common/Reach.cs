using AmongUs.GameOptions;
using static TOHO.Options;

namespace TOHO.Roles.Modifiers.Common;

public class Reach : IModifier
{
    public CustomRoles Role => CustomRoles.Reach;
    private const int Id = 23700;
    public ModifierTypes Type => ModifierTypes.Helpful;
    public static CustomRoles IsReach => CustomRoles.Reach; // Used to find "references" of this Modifier.

    public void SetupCustomOption()
    {
        SetupAdtRoleOptions(Id, CustomRoles.Reach, canSetNum: true);
    }
    public void Init()
    { }
    public void Add(byte playerId, bool gameIsLoading = true)
    { }
    public void Remove(byte playerId)
    { }
    public static void ApplyGameOptions(IGameOptions opt) => opt.SetInt(Int32OptionNames.KillDistance, 2);
}
