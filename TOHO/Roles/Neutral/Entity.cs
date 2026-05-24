using System.Collections.Generic;
using TOHO.Roles.Core;
using static TOHO.Options;
namespace TOHO.Roles.Neutral;

internal class Entity : RoleBase
{
    //===========================SETUP================================\\
    private const int Id = 39900;
    public static bool HasEnabled => CustomRoleManager.HasEnabled(CustomRoles.Entity);
    public override CustomRoles Role => CustomRoles.Entity;
    public override CustomRoles ThisRoleBase => CustomRoles.Impostor;
    public override bool TOHORole => true;
    public override Custom_RoleType ThisRoleType => Custom_RoleType.NeutralEvil;
    public override string IdeaRole => "bearyyy_cocoa";
    //==================================================================\\

    public static OptionItem EntityHauntsToWin;
    public static OptionItem HauntDuration;

    public static List<PlayerControl> Haunted = [];
    
    public override void SetupCustomOption()
    {
        SetupRoleOptions(Id, TabGroup.NeutralRoles, CustomRoles.Entity);
        EntityHauntsToWin = IntegerOptionItem.Create(Id + 10, "EntityHauntsToWin", new(1, 10, 1), 3, TabGroup.NeutralRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Entity]);
        HauntDuration = FloatOptionItem.Create(Id + 11, "HauntDuration", new(30f, 600f, 10f), 90f, TabGroup.NeutralRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Entity])
            .SetValueFormat(OptionFormat.Seconds);
    }

    public override bool OnCheckMurderAsKiller(PlayerControl killer, PlayerControl target)
    {
        Haunted.Add(target);
        new LateTask(() =>
        {
            Haunted.Remove(target);
        }, HauntDuration.GetFloat(), "UnHaunt - Entity");

        if (Haunted.Count >= EntityHauntsToWin.GetFloat())
        {
            if (!CustomWinnerHolder.CheckForConvertedWinner(killer.PlayerId))
            {
                CustomWinnerHolder.ResetAndSetWinner(CustomWinner.Entity);
                CustomWinnerHolder.WinnerIds.Add(killer.PlayerId);
            }
        }
        
        killer.RpcGuardAndKill();
        killer.ResetKillCooldown();
        return false;
    }

    public override bool CanUseKillButton(PlayerControl pc)
    {
        return true;
    }
}