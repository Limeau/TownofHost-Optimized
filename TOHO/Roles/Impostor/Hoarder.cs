using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using Rewired;
using TOHO.Modules;
using TOHO.Roles.Core;

namespace TOHO.Roles.Impostor;

internal class Hoarder : RoleBase
{
    //===========================SETUP================================\\
    private const int Id = 44200;
    public override CustomRoles Role => CustomRoles.Hoarder;
    public override CustomRoles ThisRoleBase => CustomRoles.Shapeshifter;
    public override Custom_RoleType ThisRoleType => Custom_RoleType.ImpostorSupport;
    public override bool TOHORole => true;
    public override bool NewRole => true;
    public override string IdeaRole => "runsterv";
    //==================================================================\\

    private static OptionItem HoarderKillCooldown;
    private static OptionItem AbilityUses;
    public static OptionItem HoarderAbilityCooldown;
    public static OptionItem HoarderAbilityDuration;

    public static bool IsTaskHarmful = false;
    
    public override void SetupCustomOption()
    {
        Options.SetupRoleOptions(Id, TabGroup.ImpostorRoles, CustomRoles.Hoarder);
        HoarderKillCooldown = FloatOptionItem.Create(Id + 2, GeneralOption.KillCooldown, new(1f, 180f, 1f), 30f, TabGroup.ImpostorRoles, false)
            .SetParent(Options.CustomRoleSpawnChances[CustomRoles.Hoarder])
            .SetValueFormat(OptionFormat.Seconds);
        HoarderAbilityCooldown = FloatOptionItem.Create(Id + 3, GeneralOption.AbilityCooldown, new(1f, 60f, 1f), 20f, TabGroup.ImpostorRoles, false)
            .SetParent(Options.CustomRoleSpawnChances[CustomRoles.Hoarder])
            .SetValueFormat(OptionFormat.Seconds);
        HoarderAbilityDuration = FloatOptionItem.Create(Id + 4, GeneralOption.AbilityDuration, new(1f, 60f, 1f), 20f, TabGroup.ImpostorRoles, false)
            .SetParent(Options.CustomRoleSpawnChances[CustomRoles.Hoarder])
            .SetValueFormat(OptionFormat.Seconds);
        AbilityUses = IntegerOptionItem.Create(Id + 5, "AbilityUses337", new(1, 10, 1), 3, TabGroup.ImpostorRoles, false)
            .SetParent(Options.CustomRoleSpawnChances[CustomRoles.Hoarder]);
    }

    public override void Add(byte playerId)
    {            
        IsTaskHarmful = false;
        Utils.GetPlayerById(playerId).SetAbilityUseLimit(AbilityUses.GetInt());
    }

    public override void UnShapeShiftButton(PlayerControl shapeshifter)
    {
        if (shapeshifter.GetAbilityUseLimit() < 1) return;
        
        shapeshifter.RpcRemoveAbilityUse();
        IsTaskHarmful = true;
        _ = new LateTask(() =>
        {
            IsTaskHarmful = false;
        }, HoarderAbilityDuration.GetFloat(), "Hoarder Ability End");
    }

    public static void OnTask()
    {
        foreach (var player in Main.AllAlivePlayerControls.Where(x => x.Is(CustomRoles.Hoarder) && Main.AllPlayerKillCooldown[x.PlayerId] > 10))
        {
            Main.AllPlayerKillCooldown[player.PlayerId] -= 1;
            player.MarkDirtySettings();
        }
    }

    public override void SetKillCooldown(byte id) => Main.AllPlayerKillCooldown[id] = HoarderKillCooldown.GetFloat();

    public override void ApplyGameOptions(IGameOptions opt, byte playerId)
    {
        AURoleOptions.ShapeshifterCooldown = HoarderAbilityCooldown.GetFloat();
    }
}