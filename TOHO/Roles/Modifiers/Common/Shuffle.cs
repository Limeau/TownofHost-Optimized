using System.Collections.Generic;
using System.Linq;
using TOHO.Roles.Core;

namespace TOHO.Roles.Modifiers.Common;

public class Shuffle : IModifier
{
    public CustomRoles Role => CustomRoles.Shuffle;
    private const int Id = 47300;
    public ModifierTypes Type => ModifierTypes.Misc;
    public static bool IsEnable = false;
    private static List<CustomRoles> Modifiers = [];

    public static CustomRoles CurrentModifier;
    
    public void SetupCustomOption()
    {
        Options.SetupAdtRoleOptions(Id, CustomRoles.Shuffle, canSetNum: true, teamSpawnOptions: true);
    }

    public void Init()
    {
        IsEnable = false;
        Modifiers.Clear();
        Modifiers.AddRange(Options.GroupedModifiers[ModifierTypes.Helpful]);
        Modifiers.AddRange(Options.GroupedModifiers[ModifierTypes.Harmful]);
        Modifiers.AddRange(Options.GroupedModifiers[ModifierTypes.Mixed]);
        Modifiers.AddRange(Options.GroupedModifiers[ModifierTypes.Misc]);
    }

    public void Add(byte playerId, bool gameIsLoading = true)
    {       
        IsEnable = true;
        var player = Utils.GetPlayerById(playerId);
        var role = Modifiers.Where(x => IsAcceptableModifier(x)).RandomElement();
        player.RpcSetCustomRole(role, checkModifiers: false, checkAAconflict: false);
        CurrentModifier = role;
    }

    public void Remove(byte playerId)
    {
        IsEnable = false;

    }

    public static bool IsAcceptableModifier(CustomRoles modifier)
    {
        if (modifier.GetCustomRoleTeam() != Custom_Team.Modifier) return false;
        if (modifier.IsBetrayalModifier()) return false;
        if (modifier.IsImpOnlyModifier()) return false;
        if (modifier.IsModifierAssignedMidGame()) return false;
        if (CrewOnlyModifiers.Contains(modifier)) return false;
        if (modifier == CustomRoles.Shuffle) return false;
        return true;
    }

    public static List<CustomRoles> CrewOnlyModifiers =
    [
        CustomRoles.Bloodthirst,
        CustomRoles.Forgetful,
        CustomRoles.Ghoul,
        CustomRoles.Hurried,
        CustomRoles.LabRat,
        CustomRoles.Lazy,
        CustomRoles.Nimble,
        CustomRoles.Peacemaker,
        CustomRoles.Rage,
        CustomRoles.Rascal,
        CustomRoles.Torch,
        CustomRoles.Workhorse,
    ];

    public static void AfterMeetingTasks()
    {
        foreach (var player in Main.AllAlivePlayerControls.Where(x => x.Is(CustomRoles.Shuffle)))
        {
            if (player.GetCustomSubRoles().Contains(CurrentModifier)) Main.PlayerStates[player.PlayerId].RemoveSubRole(CurrentModifier);
            var role = Modifiers.Where(x => IsAcceptableModifier(x))
                .RandomElement();
            CurrentModifier = role;
            player.RpcSetCustomRole(role, checkModifiers: false, checkAAconflict: false);
        }
    }
}