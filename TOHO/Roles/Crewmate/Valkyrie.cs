using static TOHO.Options;

namespace TOHO.Roles.Crewmate;

internal class Valkyrie : RoleBase
{
    private const int Id = 31100;
    public override CustomRoles ThisRoleBase => CustomRoles.Crewmate;
    public override Custom_RoleType ThisRoleType => Custom_RoleType.CrewmateKilling;
    public override CustomRoles Role => CustomRoles.Valkyrie;
    public override bool TOHORole => true;

    public static OptionItem RevengeTime;
    public static bool IsRevenge = false;

    public override void SetupCustomOption()
    {
        SetupRoleOptions(Id, TabGroup.CrewmateRoles, CustomRoles.Valkyrie); 
        RevengeTime = FloatOptionItem.Create(Id + 13, "ValkyrieRevengeTime", new(5f, 25f, 2.5f), 15f, TabGroup.CrewmateRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Valkyrie])
            .SetValueFormat(OptionFormat.Seconds);
    }

    public override bool CanUseKillButton(PlayerControl pc)
    {
        return true;
    }

    public override void SetKillCooldown(byte id)
    {
        Main.AllPlayerKillCooldown[id] = 1;
    }

    public override bool OnCheckMurderAsTarget(PlayerControl killer, PlayerControl target)
    {
        if (IsRevenge) return true;
        
        IsRevenge = true;
        target.RpcChangeRoleBasis(CustomRoles.ImpostorTOHO); 
        target.RpcRandomVentTeleport(); 
        killer.RpcGuardAndKill(); 
        target.SetKillCooldownV3(1);
            
        new LateTask(() => 
        { 
            if (killer.IsAlive()) target.RpcMurderPlayer(target);
            else 
            { 
                target.RpcSetCustomRole(CustomRoles.Valkyrie); 
                target.RpcChangeRoleBasis(CustomRoles.Valkyrie); 
                target.Notify(Translator.GetString("ValkyrieSuccess"));
            }

            IsRevenge = false;
        }, RevengeTime.GetFloat(), "Valkyrie Revenge");
            
        killer.Notify(Translator.GetString("ValkyrieHide"));
        target.Notify(string.Format(Translator.GetString("ValkyrieRevenge"), RevengeTime.GetFloat(), killer.GetRealName().RemoveHtmlTags()));
        return false;
    }
}