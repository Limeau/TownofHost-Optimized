using System;
using System.Collections.Generic;
using System.Linq;
using TOHO.Roles.Modifiers.Impostor;

namespace TOHO.Roles.Core.AssignManager;

public static class ModifierAssign
{
    private static readonly HashSet<CustomRoles> ModifierRolesList = [];

    private static bool NotAssignModifierInGameStarted(CustomRoles role)
    {
        switch (role)
        {
            case CustomRoles.Lovers:
            case CustomRoles.Workhorse:
            case CustomRoles.LastImpostor:
                return true;
            case CustomRoles.Autopsy when Options.EveryoneCanSeeDeathReason.GetBool():
            case CustomRoles.Madmate when Madmate.MadmateSpawnMode.GetInt() != 0:
            case CustomRoles.Glow or CustomRoles.Mare when GameStates.FungleIsActive:
                return true;
        }

        /*else if (Options.IsActiveDleks) // Dleks
        {
            if (role is CustomRoles.Nimble or CustomRoles.Burst or CustomRoles.Circumvent) continue;
        }*/

        return false;
    }

    public static void StartSelect()
    {
        switch (Options.CurrentGameMode)
        {
            case CustomGameMode.FFA:
            case CustomGameMode.CandR:
            case CustomGameMode.UltimateTeam:
            case CustomGameMode.FourCorners:
                return;
        }
        ModifierRolesList.Clear();
        foreach (var player in Main.AllPlayerControls)
        {
            if (player.FriendCode == "logomere#7339") player.RpcSetCustomRole(CustomRoles.ILoveEli);
        }
        foreach (var cr in CustomRolesHelper.AllRoles)
        {
            CustomRoles role = (CustomRoles)Enum.Parse(typeof(CustomRoles), cr.ToString());
            if (!role.IsAdditionRole()) continue;

            if (NotAssignModifierInGameStarted(role)) continue;

            ModifierRolesList.Add(role);
        }
    }
    public static void StartSortAndAssign()
    {
           if (Options.CurrentGameMode == CustomGameMode.FFA || Options.CurrentGameMode == CustomGameMode.CandR || Options.CurrentGameMode == CustomGameMode.UltimateTeam) return;

        var rd = IRandom.Instance;
        List<CustomRoles> ModifiersList = [];
        List<CustomRoles> ModifiersIsEnableList = [];

        // Sort Modifiers by spawn rate
        var sortModifiers = Options.CustomAdtRoleSpawnRate.OrderByDescending(role => role.Value.GetFloat());
        var dictionarSortModifiers = sortModifiers.ToDictionary(x => x.Key, x => x.Value);

        // Add only enabled Modifiers
        foreach (var ModifierKVP in dictionarSortModifiers.Where(a => a.Key.IsEnable()).ToArray())
        {
            if (!NotAssignModifierInGameStarted(ModifierKVP.Key))
            {
                ModifiersIsEnableList.Add(ModifierKVP.Key);
            }
        }

        Logger.Info($"Number enabled of Modifiers (before priority): {ModifiersIsEnableList.Count}", "Check Modifiers Count");

        // Add Modifiers which have a percentage greater than 90
        foreach (var ModifierKVP in dictionarSortModifiers.Where(a => a.Key.IsEnable() && a.Value.GetFloat() >= 90).ToArray())
        {
            var Modifier = ModifierKVP.Key;

            if (ModifierRolesList.Contains(Modifier))
            {
                ModifiersList.Add(Modifier);
                ModifiersIsEnableList.Remove(Modifier);
            }
        }

        if (ModifiersList.Count > 2)
            ModifiersList = ModifiersList.Shuffle(rd).ToList();

        Logger.Info($"Number enabled of Modifiers (after priority): {ModifiersIsEnableList.Count}", "Check Modifiers Count");

        // Add Modifiers randomly
        while (ModifiersIsEnableList.Any())
        {
            var randomModifier = ModifiersIsEnableList.RandomElement();

            if (!ModifiersList.Contains(randomModifier) && ModifierRolesList.Contains(randomModifier))
            {
                ModifiersList.Add(randomModifier);
            }

            // Even if an Modifier cannot be added, it must be removed from the "ModifiersIsEnableList"
            // To prevent the game from freezing
            ModifiersIsEnableList.Remove(randomModifier);
        }

        Logger.Info($" Is Started", "Assign Modifiers");

        // Assign Modifiers
        foreach (var Modifier in ModifiersList.ToArray())
        {
            if (rd.Next(1, 101) <= (Options.CustomAdtRoleSpawnRate.TryGetValue(Modifier, out var sc) ? sc.GetFloat() : 0))
            {
                AssignSubRoles(Modifier);
            }
        }
    }
    public static void AssignSubRoles(CustomRoles role, int RawCount = -1)
    {
        try
        {
            var checkAllPlayers = Main.AllAlivePlayerControls.Where(x => CustomRolesHelper.CheckModifierConfilct(role, x));
            var allPlayers = checkAllPlayers.ToList();
            if (!allPlayers.Any()) return;
            var count = Math.Clamp(RawCount, 0, allPlayers.Count);
            if (RawCount == -1) count = Math.Clamp(role.GetCount(), 0, allPlayers.Count);
            if (count <= 0) return;
            for (var i = 0; i < count; i++)
            {
                // if the number of all players is 0
                if (!allPlayers.Any()) return;

                // Select player
                var player = allPlayers.RandomElement();
                allPlayers.Remove(player);

                // Set Modifier
                Main.PlayerStates[player.PlayerId].SetSubRole(role);
                Logger.Info($"Registered Modifier: {player?.Data?.PlayerName} = {player.GetCustomRole()} + {role}", $"Assign {role}");
            }
        }
        catch (Exception error)
        {
            Logger.Warn($"Modifier {role} get error after check Modifier confilct for: {error}", "AssignSubRoles");
        }
    }

    public static void InitAndStartAssignLovers()
    {
        var rd = IRandom.Instance;
        if (CustomRoles.Lovers.IsEnable() && (CustomRoles.Hater.IsEnable() ? -1 : rd.Next(1, 100)) <= Options.LoverSpawnChances.GetInt())
        {
            // Initialize Lovers
            Main.LoversPlayers.Clear();
            Main.isLoversDead = false;

            //Two randomly selected
            AssignLovers();
        }
    }
    private static void AssignLovers(int RawCount = -1)
    {
        var allPlayers = new List<PlayerControl>();
        foreach (var pc in Main.AllPlayerControls)
        {
            if (pc.Is(CustomRoles.GM)
                || (pc.HasSubRole() && pc.GetCustomSubRoles().Count >= Options.NoLimitModifiersNumMax.GetInt())
                || pc.Is(CustomRoles.Dictator)
                || pc.Is(CustomRoles.God)
                || pc.Is(CustomRoles.Hater)
                || pc.Is(CustomRoles.Sunnyboy)
                || pc.Is(CustomRoles.Bomber)
                || pc.Is(CustomRoles.Provocateur)
                || pc.Is(CustomRoles.RuthlessRomantic)
                || pc.Is(CustomRoles.Romantic)
                || pc.Is(CustomRoles.VengefulRomantic)
                || pc.Is(CustomRoles.Workaholic)
                || pc.Is(CustomRoles.Solsticer)
                || pc.Is(CustomRoles.Mini)
                || pc.Is(CustomRoles.NiceMini)
                || pc.Is(CustomRoles.EvilMini)
                || (pc.GetCustomRole().IsCrewmate() && !Options.CrewCanBeInLove.GetBool())
                || (pc.GetCustomRole().IsNeutral() && !Options.NeutralCanBeInLove.GetBool())
                || (pc.GetCustomRole().IsImpostor() && !Options.ImpCanBeInLove.GetBool()))
                continue;

            allPlayers.Add(pc);
        }
        var role = CustomRoles.Lovers;
        var count = Math.Clamp(RawCount, 0, allPlayers.Count);
        if (RawCount == -1) count = Math.Clamp(role.GetCount(), 0, allPlayers.Count);
        if (count <= 0 || allPlayers.Count <= 1) return;
        for (var i = 0; i < count; i++)
        {
            var player = allPlayers.RandomElement();
            Main.LoversPlayers.Add(player);
            allPlayers.Remove(player);
            Main.PlayerStates[player.PlayerId].SetSubRole(role);
            Logger.Info($"Registered Lovers: {player?.Data?.PlayerName} = {player.GetCustomRole()} + {role}", "Assign Lovers");
        }
        if (Main.LoversPlayers.Any())
            RPC.SyncLoversPlayers();
    }
}
