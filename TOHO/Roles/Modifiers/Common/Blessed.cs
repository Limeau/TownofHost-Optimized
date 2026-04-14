using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using Rewired;
using static TOHO.Options;

namespace TOHO.Roles.Modifiers.Common;

public class Blessed : IModifier
{
    public CustomRoles Role => CustomRoles.Blessed;
    private const int Id = 39500;
    public ModifierTypes Type => ModifierTypes.Helpful;

    private static OptionItem BlessedShieldDuration;
    private static OptionItem MinPlayersShield;

    private static readonly HashSet<byte> playerList = [];
    public static bool IsEnable;

    public static bool IsShieldActive = false;
    
    public void SetupCustomOption()
    {
        SetupAdtRoleOptions(Id, CustomRoles.Blessed, canSetNum: true, teamSpawnOptions: true);
        BlessedShieldDuration = FloatOptionItem.Create(Id + 10, "BlessedShieldDuration", new(10f, 35f, 1f), 20f, TabGroup.Modifiers, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Blessed])
            .SetValueFormat(OptionFormat.Seconds);
        MinPlayersShield = IntegerOptionItem.Create(Id + 11, "MinPlayersShield", new(2, 6, 1), 4, TabGroup.Modifiers, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Blessed]);
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

    public static void AfterMeetingTasks()
    {
        IsShieldActive = true;
        new LateTask(() =>
        {
            IsShieldActive = false;
        }, BlessedShieldDuration.GetFloat(), "Blessed Shield Removal");
    }

    public static bool OnMurderAsTarget(PlayerControl killer, PlayerControl target)
    {
        if (IsShieldActive && Main.AllAlivePlayerControls.Length >= MinPlayersShield.GetInt())
        {
            killer.RpcGuardAndKill();
            killer.ResetKillCooldown();
            return false;
        }
        return true;
    }
}
