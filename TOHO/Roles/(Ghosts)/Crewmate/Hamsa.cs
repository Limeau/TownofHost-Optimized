using AmongUs.GameOptions;
using TOHO.Modules;
using TOHO.Roles.Core;
using static TOHO.Options;

namespace TOHO.Roles._Ghosts_.Crewmate;

internal class Hamsa : RoleBase
{
    //===========================SETUP================================\\
    private const int Id = 46400;
    public static bool HasEnabled => CustomRoleManager.HasEnabled(CustomRoles.Hamsa);
    public override CustomRoles Role => CustomRoles.Hamsa;
    public override CustomRoles ThisRoleBase => CustomRoles.GuardianAngel;
    public override Custom_RoleType ThisRoleType => Custom_RoleType.CrewmateGhosts;
    public override bool TOHORole => true;
    public override bool NewRole => true;
    public override string IdeaRole => "den6211epic";
    //==================================================================\\

    public static OptionItem RevealCooldown;
    public int KeepCount = 0;
    public bool KnowTargetRole = false;
    public override void SetupCustomOption()
    {
        SetupSingleRoleOptions(Id, TabGroup.CrewmateRoles, CustomRoles.Hamsa);
        RevealCooldown = FloatOptionItem.Create(Id + 10, GeneralOption.GuardianAngelBase_ProtectCooldown, new(0f, 120f, 2.5f), 25f, TabGroup.CrewmateRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.Hamsa])
            .SetValueFormat(OptionFormat.Seconds);
    }
    public override void ApplyGameOptions(IGameOptions opt, byte playerId)
    {
        AURoleOptions.GuardianAngelCooldown = RevealCooldown.GetFloat();
        AURoleOptions.ProtectionDurationSeconds = 0f;
    }
    public override bool OnCheckProtect(PlayerControl killer, PlayerControl target)
    {
        if (target.GetAbilityUseLimit() > 0)
        {
            if (target.IsPlayerCrewmateTeam()) target.RpcIncreaseAbilityUseLimitBy(1);
            else target.RpcRemoveAbilityUse();
        }
        killer.RpcResetAbilityCooldown();
        return false;
    }
}
