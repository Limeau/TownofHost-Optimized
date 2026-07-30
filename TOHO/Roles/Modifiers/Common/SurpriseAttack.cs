using System.Linq;
using System.Reflection;
using static TOHO.Options;

namespace TOHO.Roles.Modifiers.Common;

public class SurpriseAttack : IModifier
{
    public CustomRoles Role => CustomRoles.SurpriseAttack;
    private const int Id = 46200;
    public ModifierTypes Type => ModifierTypes.Misc;

    public void SetupCustomOption()
    {
        SetupAdtRoleOptions(Id, CustomRoles.SurpriseAttack, canSetNum: true, teamSpawnOptions: true);
    }
    public void Init()
    { }
    public void Add(byte playerId, bool gameIsLoading = true)
    { }
    public void Remove(byte playerId)
    { }

    public static void AfterActionTasks(PlayerControl player)
    {
        var pc1 = Main.AllAlivePlayerControls.Where(x => x != player).RandomElement();
        var pc2 = Main.AllAlivePlayerControls.Where(x => x != player || x != pc1).RandomElement();
        if (!pc2) return;

        var pos = pc1.GetCustomPosition();
        pc1.RpcTeleport(pc2.GetCustomPosition());
        pc2.RpcTeleport(pos);
    }
}