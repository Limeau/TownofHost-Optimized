using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using Rewired;
using static TOHO.Options;

namespace TOHO.Roles.Modifiers.Common;

public class Bounty : IModifier
{
    public CustomRoles Role => CustomRoles.Bounty;
    private const int Id = 42800;
    public ModifierTypes Type => ModifierTypes.Harmful;

    private static OptionItem BountyChance;
    private static OptionItem Vision;
    private static OptionItem Speed;
    private static OptionItem Cooldown;

    public static HashSet<PlayerControl> Dimmed = [];
    public static HashSet<PlayerControl> Brightened = [];
    public static bool IsEnable;

    public static bool IsShieldActive = false;
    
    public void SetupCustomOption()
    {
        SetupAdtRoleOptions(Id, CustomRoles.Bounty, canSetNum: true, teamSpawnOptions: true);
        BountyChance = IntegerOptionItem.Create(Id + 11, "BountyChance", new(5, 100, 5), 75, TabGroup.Modifiers, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Bounty])
            .SetValueFormat(OptionFormat.Percent);
        Vision = FloatOptionItem.Create(Id + 12, "BountyVision", new(0.5f, 2f, 0.1f), 1f, TabGroup.Modifiers, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Bounty])
            .SetValueFormat(OptionFormat.Multiplier);
        Speed = FloatOptionItem.Create(Id + 13, "BountySpeed", new(0.5f, 2f, 0.1f), 1f, TabGroup.Modifiers, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Bounty])
            .SetValueFormat(OptionFormat.Multiplier);
        Cooldown = FloatOptionItem.Create(Id + 14, "BountyCooldown", new(1, 20, 1), 5, TabGroup.Modifiers, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Bounty])
            .SetValueFormat(OptionFormat.Seconds);
    }

    public void Init()
    {        
        IsEnable = false;
    }

    public void Add(byte playerId, bool gameIsLoading = true)
    {        
        IsEnable = true;
    }

    public void Remove(byte playerId)
    {
        IsEnable = false;
    }

    public static void ApplyGameOption(PlayerControl player, IGameOptions opt)
    {
        if (Dimmed.Contains(player))
        {
            opt.SetVision(false);
            opt.SetFloat(FloatOptionNames.CrewLightMod, opt.GetFloat(FloatOptionNames.CrewLightMod) - Vision.GetFloat());
            opt.SetFloat(FloatOptionNames.ImpostorLightMod, opt.GetFloat(FloatOptionNames.ImpostorLightMod) - Vision.GetFloat());
            Dimmed.Remove(player);
        }
        if (Brightened.Contains(player))
        {
            opt.SetVision(false);
            opt.SetFloat(FloatOptionNames.CrewLightMod, opt.GetFloat(FloatOptionNames.CrewLightMod) + Vision.GetFloat());
            opt.SetFloat(FloatOptionNames.ImpostorLightMod, opt.GetFloat(FloatOptionNames.ImpostorLightMod) + Vision.GetFloat());
            Brightened.Remove(player);
        }
    }

    public static bool OnMurderAsTarget(PlayerControl killer, PlayerControl target)
    {
        //1: Speed
        //2: Vision
        //3: Cooldown
        if (IRandom.Instance.Next(1, 100) > BountyChance.GetFloat())
        {
            switch (IRandom.Instance.Next(1, 3))
            {
                case 1:
                    Dimmed.Add(killer);
                    break;
                case 2:
                    Main.AllPlayerSpeed[killer.PlayerId] -= Speed.GetFloat();
                    break;
                case 3:
                    Main.AllPlayerKillCooldown[killer.PlayerId] += Cooldown.GetFloat();
                    break;
            }
        }
        else
        {
            switch (IRandom.Instance.Next(1, 3))
            {
                case 1:
                    Brightened.Add(killer);
                    break;
                case 2:
                    Main.AllPlayerSpeed[killer.PlayerId] += Speed.GetFloat();
                    break;
                case 3:
                    Main.AllPlayerKillCooldown[killer.PlayerId] -= Cooldown.GetFloat();
                    break;
            }
        }
        return true;
    }
}
