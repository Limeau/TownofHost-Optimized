using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TOHO.Modules;
using TOHO.Roles.Core;
using TOHO.Roles.Crewmate;
using TOHO.Roles.Impostor;
using static TOHO.Options;
using static TOHO.Translator;

namespace TOHO.Roles.Modifiers.Common;

public class Oblivion : IModifier
{
    public CustomRoles Role => CustomRoles.Oblivion;
    private const int Id = 25700;
    private readonly static List<byte> playerIdList = [];
    public static bool IsEnable = false;
    public ModifierTypes Type => ModifierTypes.Mixed;


    private static OptionItem CanPassOn;
    private static OptionItem ChangeNeutralRole;

    [Obfuscation(Exclude = true)]
    private enum ChangeRolesSelectList
    {
        Role_NoChange,
        Role_Amnesiac,
        Role_Imitator
    }

    public static readonly CustomRoles[] NRoleChangeRoles =
    [
        CustomRoles.Amnesiac,
        CustomRoles.Imitator,
    ]; //Just -1 to use this LOL

    public void SetupCustomOption()
    {
        SetupAdtRoleOptions(Id, CustomRoles.Oblivion, canSetNum: true, tab: TabGroup.Modifiers, teamSpawnOptions: true);
        CanPassOn = BooleanOptionItem.Create(Id + 14, "OblivionCanPassOn", true, TabGroup.Modifiers, false).SetParent(CustomRoleSpawnChances[CustomRoles.Oblivion]);
        ChangeNeutralRole = StringOptionItem.Create(Id + 15, "NeutralChangeRolesForOblivion", EnumHelper.GetAllNames<ChangeRolesSelectList>(), 1, TabGroup.Modifiers, false).SetParent(CustomRoleSpawnChances[CustomRoles.Oblivion]);
    }
    public void Init()
    {
        IsEnable = false;
        playerIdList.Clear();
    }
    public void Add(byte playerId, bool gameIsLoading = true)
    {
        if (!playerIdList.Contains(playerId))
            playerIdList.Add(playerId);

        IsEnable = true;
    }
    public static void PassOnKiller(byte playerId)
    {
        if (!playerIdList.Contains(playerId))
            playerIdList.Add(playerId);
        IsEnable = true;
    }
    public void Remove(byte playerId)
    {
        playerIdList.Remove(playerId);

        if (!playerIdList.Any())
            IsEnable = false;
    }

    public static void OnMurderPlayer(PlayerControl killer, PlayerControl target)
    {
        if (killer == null || target == null) return;
        if (killer.PlayerId == target.PlayerId) return;
        if (killer.Is(CustomRoles.KillingMachine) || killer.Is(CustomRoles.Rulebook) || killer.Is(CustomRoles.Massacre) || killer.IsTransformedNeutralApocalypse()) return;
        if ((killer.Is(CustomRoles.Ghoul) || killer.Is(CustomRoles.Burst)) && !killer.IsAlive()) return;
        if (!target.Is(CustomRoles.Oblivion)) return;
        if (!CanGetOblivioned(killer)) return;

        if (CanPassOn.GetBool() && !playerIdList.Contains(killer.PlayerId))
        {
            PassOnKiller(killer.PlayerId);
            killer.RpcSetCustomRole(CustomRoles.Oblivion);
            Logger.Info(killer.GetNameWithRole() + " gets Oblivion Modifier by " + target.GetNameWithRole(), "Oblivion");
        }

        if (!Eraser.ErasedRoleStorage.ContainsKey(killer.PlayerId))
        {
            Eraser.ErasedRoleStorage.Add(killer.PlayerId, killer.GetCustomRole());
            Logger.Info($"Added {killer.GetNameWithRole()} to ErasedRoleStorage", "Oblivion");
        }
        else
        {
            Logger.Info($"Canceled {killer.GetNameWithRole()} Oblivion bcz already erased.", "Oblivion");
            return;
        }

        var killerRole = killer.GetCustomRole();
        if (killer.HasGhostRole() || CopyCat.playerIdList.Contains(killer.PlayerId) || killer.Is(CustomRoles.Stubborn))
        {
            Logger.Info($"Oblivion {killer.GetNameWithRole().RemoveHtmlTags()} cannot eraser crew imp-based role", "Oblivion");
            return;
        }
        else if (killerRole.IsCoven() && !CovenManager.HasNecronomicon(killer))
        {
            killer.GetRoleClass().OnRemove(killer.PlayerId);
            killer.RpcChangeRoleBasis(CustomRoles.Amnesiac);
            killer.RpcSetCustomRole(CustomRoles.Amnesiac);
            Main.DesyncPlayerList.Remove(killer.PlayerId);
            killer.GetRoleClass().OnAdd(killer.PlayerId);
            killer.RpcSetCustomRole(CustomRoles.Enchanted, false);
            killer.AddInSwitchModifiers(killer, CustomRoles.Enchanted);
            Logger.Info($"Oblivion {killer.GetNameWithRole().RemoveHtmlTags()} with Coven without Necronomicon.", "Oblivion");
        }
        else if (CovenManager.HasNecronomicon(killer))
        {
            // Necronomicon holder immune to Oblivion
            Logger.Info($"Oblivion {killer.GetNameWithRole().RemoveHtmlTags()} with Coven with Necronomicon.", "Oblivion");
        }
        else if (killerRole.IsMadmate())
        {
            killer.GetRoleClass().OnRemove(killer.PlayerId);
            killer.RpcChangeRoleBasis(CustomRoles.Amnesiac);
            killer.RpcSetCustomRole(CustomRoles.Amnesiac);
            killer.GetRoleClass().OnAdd(killer.PlayerId);
            Main.DesyncPlayerList.Remove(killer.PlayerId);
            killer.RpcSetCustomRole(CustomRoles.Madmate);
            killer.AddInSwitchModifiers(killer, CustomRoles.Madmate);
            Logger.Info($"Oblivion {killer.GetNameWithRole().RemoveHtmlTags()} with Madmates assign.", "Oblivion");
        }
        else if (killer.Is(CustomRoles.Sidekick))
        {
            killer.GetRoleClass().OnRemove(killer.PlayerId);
            killer.RpcChangeRoleBasis(CustomRoles.Amnesiac);
            killer.RpcSetCustomRole(CustomRoles.Amnesiac);
            Main.DesyncPlayerList.Remove(killer.PlayerId);
            killer.GetRoleClass().OnAdd(killer.PlayerId);
            killer.RpcSetCustomRole(CustomRoles.Recruit);
            killer.AddInSwitchModifiers(killer, CustomRoles.Recruit);
            Logger.Info($"Oblivion {killer.GetNameWithRole().RemoveHtmlTags()} with Sidekicks assign.", "Oblivion");
        }
        else if (!killerRole.IsNeutral())
        {
            var readyrole = Eraser.GetErasedRole(killer.GetCustomRole().GetRoleTypes(), killer.GetCustomRole());
            
            killer.GetRoleClass()?.OnRemove(killer.PlayerId);
            killer.RpcChangeRoleBasis(readyrole);
            killer.RpcSetCustomRole(readyrole);
            Main.DesyncPlayerList.Remove(killer.PlayerId);
            killer.GetRoleClass()?.OnAdd(killer.PlayerId);
            Logger.Info($"Oblivion {killer.GetNameWithRole().RemoveHtmlTags()} with eraser assign.", "Oblivion");
        }
        else
        {
            int changeValue = ChangeNeutralRole.GetValue();

            if (changeValue != 0)
            {
                killer.GetRoleClass().OnRemove(killer.PlayerId);
                killer.RpcChangeRoleBasis(NRoleChangeRoles[changeValue - 1]);
                killer.RpcSetCustomRole(NRoleChangeRoles[changeValue - 1]);
                Main.DesyncPlayerList.Remove(killer.PlayerId);
                killer.GetRoleClass().OnAdd(killer.PlayerId);

                killer.SyncSettings();

                Logger.Info($"Oblivion {killer.GetNameWithRole().RemoveHtmlTags()} with Neutrals assign.", "Oblivion");
            }
        }
        killer.ResetKillCooldown();
        killer.SetKillCooldown();
        killer.Notify(GetString("LostRoleByOblivion"));
        killer.RPCPlayCustomSound("Oblivion");
        Logger.Info($"{killer.GetRealName()} was Oblivioned", "Oblivion");
    }

    private static bool CanGetOblivioned(PlayerControl player)
    {
        if (player.GetCustomRole().IsNeutral() && ChangeNeutralRole.GetValue() == 0) return false;
        if (player.Is(CustomRoles.Loyal) || player.Is(CustomRoles.Stubborn)) return false;

        return true;
    }
}
