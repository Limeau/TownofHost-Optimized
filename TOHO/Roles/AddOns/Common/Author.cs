/* WILLS - v1.6.0
using static TOHO.Options;
namespace TOHO.Roles.Modifiers.Common;
internal class Author : IModifier
{
    public CustomRoles Role => CustomRoles.Author;
    private const int Id = 33900;
    public ModifierTypes Type => ModifierTypes.Helpful;

    public void SetupCustomOption()
    {
        SetupAdtRoleOptions(Id, CustomRoles.Author, canSetNum: true, teamSpawnOptions: true);
    }
    public void Init()
    { }
    public void Add(byte playerId, bool gameIsLoading = true)
    { }
    public void Remove(byte playerId)
    { }
}
*/
