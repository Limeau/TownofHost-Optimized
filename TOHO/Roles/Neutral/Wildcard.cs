using TOHO.Modules;
using static TOHO.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using TOHO.Roles.Core;

namespace TOHO.Roles.Neutral;

internal class Wildcard : RoleBase
{
    //===========================SETUP================================\\
    public override CustomRoles Role => CustomRoles.Wildcard;
    private const int Id = 45800;
    public override bool IsDesyncRole => true;
    public override CustomRoles ThisRoleBase => CustomRoles.Impostor;
    public override Custom_RoleType ThisRoleType => Custom_RoleType.NeutralChaos;
    public override bool TOHORole => true;
    public override bool NewRole => true;
    public override string IdeaRole => "puffyxavy";
    //==================================================================\\

    private static OptionItem KillCooldown;
    
    public override void SetupCustomOption()
    {
        SetupSingleRoleOptions(Id, TabGroup.NeutralRoles, CustomRoles.Wildcard, 1, zeroOne: false);
        KillCooldown = FloatOptionItem.Create(Id + 10, GeneralOption.KillCooldown, new(0f, 180f, 2.5f), 20f, TabGroup.NeutralRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.Wildcard])
            .SetValueFormat(OptionFormat.Seconds);
    }

    public override void SetKillCooldown(byte id) => Main.AllPlayerKillCooldown[id] = KillCooldown.GetFloat();
    public override bool CanUseKillButton(PlayerControl pc) => true;

    public static CustomRoles BetrayerRole = CustomRoles.NotAssigned;
    
    public override void Add(byte playerId)
    {
        List<CustomRoles> AllBetrayals = [];
        foreach (var role in EnumHelper.GetAllValues<CustomRoles>().Where(x => x.IsBetrayalModifierV2()))
        {
            AllBetrayals.Add(role);
        }
        BetrayerRole = AllBetrayals.RandomElement();
        Utils.GetPlayerById(playerId).RpcSetCustomRole(BetrayerRole);
    }

    public override bool OnCheckMurderAsKiller(PlayerControl killer, PlayerControl target)
    {
        killer.RpcGuardAndKill();
        target.RpcSetCustomRole(BetrayerRole);
        return false;
    }
}