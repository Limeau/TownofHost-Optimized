using System.Collections.Generic;
using AmongUs.GameOptions;
using TOHO.Roles.Core;
using static TOHO.Options;

namespace TOHO.Roles._Ghosts_.Crewmate;

internal class InfluencerTOHO : RoleBase
{
    //===========================SETUP================================\\
    public override CustomRoles Role => CustomRoles.InfluencerTOHO;
    private const int Id = 47800;

    public override CustomRoles ThisRoleBase => CustomRoles.Influencer;
    public override Custom_RoleType ThisRoleType => Custom_RoleType.CrewmateVanillaGhosts;
    //==================================================================\\

    private static OptionItem AbilityCooldown;
    public override void SetupCustomOption()
    {
        SetupRoleOptions(Id, TabGroup.CrewmateRoles, CustomRoles.InfluencerTOHO);
        AbilityCooldown = FloatOptionItem.Create(Id + 10, "InfluencerAbilityCooldown", new(1f, 60f, 1f), 10f, TabGroup.CrewmateRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.InfluencerTOHO])
            .SetValueFormat(OptionFormat.Seconds);
    }
    public override void ApplyGameOptions(IGameOptions opt, byte playerId)
    {
        AURoleOptions.SpiritGuideCooldownSeconds = AbilityCooldown.GetFloat();
    }
}
