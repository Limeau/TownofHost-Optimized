using static TOHO.Options;

namespace TOHO.Roles.Modifiers.Common;
internal class FragileHunter : IModifier
{
    public CustomRoles Role => CustomRoles.FragileHunter;
    private const int Id = 33500;
    public ModifierTypes Type => ModifierTypes.Misc;

    public void SetupCustomOption()
    {
        SetupAdtRoleOptions(Id, CustomRoles.FragileHunter, canSetNum: true);
    }
    public void Init()
    { }
    public void Add(byte playerId, bool gameIsLoading = true)
    { }
    public void Remove(byte playerId)
    { }
}
