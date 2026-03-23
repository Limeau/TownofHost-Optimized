using static TOHO.Options;

namespace TOHO.Roles.Modifiers.Common;

public class ExtraLife : IModifier
{
    public CustomRoles Role => CustomRoles.ExtraLife;
    private const int Id = 37300;
    public static bool IsEnable = false;
    public ModifierTypes Type => ModifierTypes.Helpful;

    private static OptionItem ExtraLifeNum;

    private static int LivesDown = 0;

    public void SetupCustomOption()
    {
        SetupAdtRoleOptions(Id, CustomRoles.ExtraLife, canSetNum: true, teamSpawnOptions: true);
        ExtraLifeNum = IntegerOptionItem.Create(Id + 10, "ExtraLifeNum", new(1, 3, 1), 1, TabGroup.Modifiers, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.ExtraLife]);
    }

    public void Init()
    { }
    public void Add(byte playerId, bool gameIsLoading = true)
    { }
    public void Remove(byte playerId)
    { }

    public static bool CheckMurder(PlayerControl killer, PlayerControl target)
    {
        if (LivesDown < ExtraLifeNum.GetInt())
        {
            LivesDown += 1;
            killer.RpcGuardAndKill();
            killer.ResetKillCooldown();
            target.RpcGuardAndKill();
            if (LivesDown >= ExtraLifeNum.GetInt())
            {
                Main.PlayerStates[target.PlayerId].RemoveSubRole(CustomRoles.ExtraLife);
            }
            return false;
        }
        return true;
    }
}
