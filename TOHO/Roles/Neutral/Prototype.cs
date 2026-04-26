using static TOHO.Options;
namespace TOHO.Roles.Neutral;

internal class Prototype : RoleBase
{ 
    private const int Id = 41000;
    public override CustomRoles Role => CustomRoles.Prototype;
    public override CustomRoles ThisRoleBase => CustomRoles.Impostor;
    public override Custom_RoleType ThisRoleType => Custom_RoleType.NeutralKilling;
    
    public static OptionItem PrototypeKillCooldown;
    public static OptionItem PrototypeSuicideChance;
    
    public override void SetupCustomOption()
    {
        SetupRoleOptions(Id, TabGroup.NeutralRoles, CustomRoles.Prototype);
        
        PrototypeKillCooldown = FloatOptionItem.Create(Id + 10, "PrototypeKillCooldown", (10f, 40f, 1f), 20f, TabGroup.NeutralRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Prototype])
            .SetValueFormat(OptionFormat.Seconds);
        PrototypeSuicideChance = IntegerOptionItem.Create(Id + 11, "PrototypeSuicideChance", (0, 100, 5), 20, TabGroup.NeutralRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Prototype])
            .SetValueFormat(OptionFormat.Percent);
    }
    
    public override bool CanUseKillButton(PlayerControl pc)
    {
        return true;
    }

    public override bool OnCheckMurderAsKiller(PlayerControl killer, PlayerControl target)
    {
        if (PrototypeSuicideChance.GetInt() <= IRandom.Instance.Next(1, 100)) killer.RpcMurderPlayer(killer);
        return true;
    }

    public override void SetKillCooldown(byte id) => PrototypeKillCooldown.GetFloat();
}