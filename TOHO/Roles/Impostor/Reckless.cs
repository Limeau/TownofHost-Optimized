using System.Linq;
using static TOHO.Options;
namespace TOHO.Roles.Impostor;
internal class Reckless : RoleBase
{ 
    public override CustomRoles Role => CustomRoles.Reckless; 
    private const int Id = 40200; 
    public override CustomRoles ThisRoleBase => CustomRoles.Impostor; 
    public override Custom_RoleType ThisRoleType => Custom_RoleType.ImpostorKilling;
    
    public static OptionItem RecklessChanceToKill;
    public static OptionItem RecklessChanceToAlert;
    
    public override void SetupCustomOption()
    {
        SetupRoleOptions(Id, TabGroup.ImpostorRoles, CustomRoles.Reckless);
        RecklessChanceToKill = IntegerOptionItem.Create(Id + 10, "RecklessChanceToKill", (0, 100, 5), 25, TabGroup.ImpostorRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Reckless])
            .SetValueFormat(OptionFormat.Percent);
        RecklessChanceToAlert = IntegerOptionItem.Create(Id + 11, "RecklessChanceToAlert", (0, 100, 5), 25, TabGroup.ImpostorRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Reckless])
            .SetValueFormat(OptionFormat.Percent);
    }
    
    public override bool OnCheckMurderAsKiller(PlayerControl killer, PlayerControl target)
    {
        if (IRandom.Instance.Next(1, 100) <= RecklessChanceToKill.GetInt())
        {
            PlayerControl closest = Main.AllAlivePlayerControls.Where(x => x.PlayerId != killer.PlayerId).MinBy(x => Utils.GetDistance(killer.GetCustomPosition(), x.GetCustomPosition()));
            closest.RpcMurderPlayer(closest);
            closest.SetRealKiller(killer);
            return true;
        }
        if (IRandom.Instance.Next(1, 100) <= RecklessChanceToAlert.GetInt())
        {
            target.Notify(Translator.GetString("RecklessAlert"));
            killer.ResetKillCooldown();
            killer.RpcGuardAndKill();
            return false;
        }
        
        return true;
    }
}