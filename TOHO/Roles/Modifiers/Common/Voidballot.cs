using static TOHO.Options;

namespace TOHO.Roles.Modifiers.Common;

public class VoidBallot : IModifier
{
    public CustomRoles Role => CustomRoles.VoidBallot;
    private const int Id = 21100;
    public ModifierTypes Type => ModifierTypes.Harmful;

    public void SetupCustomOption()
    {
        SetupAdtRoleOptions(Id, CustomRoles.VoidBallot, canSetNum: true, teamSpawnOptions: true);
    }
    public void Init()
    { }
    public void Add(byte playerId, bool gameIsLoading = true)
    { }
    public void Remove(byte playerId)
    { }
}
