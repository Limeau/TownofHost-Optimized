using AmongUs.Data;
using AmongUs.GameOptions;
using Hazel;
using InnerNet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Rewired;
using UnityEngine;

namespace TOHO;

public static class Utils
{
    public static int GetImpostorCount()
    {
        var n = 0;

        foreach (var Player in PlayerControl.AllPlayerControls)
        {
            if (Player.Data.IsDead) continue;
            if (Player.IsPlayerImpostor()) n += 1;
        }

        return n;
    }    
    public static int GetNKcount()
    {
        var n = 0;

        foreach (var Player in PlayerControl.AllPlayerControls)
        {
            if (Player.Data.IsDead) continue;
            if (Player.GetCustomRole().Category == RoleCategories.NeutralKilling) n += 1;
        }

        return n;
    }    
    public static int GetCovenCount()
    {
        var n = 0;

        foreach (var Player in PlayerControl.AllPlayerControls)
        {
            if (Player.Data.IsDead) continue;
            if (Player.IsPlayerCoven()) n += 1;
        }

        return n;
    }    
    public static void RpcSetNameLocal(this PlayerControl player, string name)
    {
        var sender = new CustomRpcSender("RpcSetNameLocal", SendOption.Reliable, false);
        sender.RpcSetName(player, name, player);
        sender.SendMessage();
    }
    public static void RpcSetNameSpecific(this PlayerControl seer, PlayerControl target, string name)
    {
        var sender = new CustomRpcSender("RpcSetNameLocal", SendOption.Reliable, false);
        sender.RpcSetName(target, name, seer);
        sender.SendMessage();
    }
    public static void RpcSetNameToAll(this PlayerControl player, string name)
    {
        var sender = new CustomRpcSender("RpcSetName", SendOption.Reliable, false);
        sender.RpcSetName(player, name);
        sender.SendMessage();
    }
    
    public static void SetNameWithRole(this PlayerControl seer, PlayerControl player = null, bool toAll = false, bool overrideToLivingRole = false)
    {
        CustomRoles role = null;

        if (player == null) player = seer;
        
        role = player.Data.IsDead ? player.GetGhostRole() : player.GetCustomRole();

        if (overrideToLivingRole) role = player.GetCustomRole();
        else role = player.Data.IsDead ? player.GetGhostRole() : player.GetCustomRole();

        var k = "";
        foreach (var modifier in Main.Modifiers[player.PlayerId])
        {
            k += $" <color={modifier.RoleColor}>";
            k += "(";
            k += modifier.RoleName;
            k += ")";
            k += "</color>";
        }
        
        if (player.OwnerId == AmongUsClient.Instance.HostId)
        {
            if (role == null)
            {
                player.cosmetics.nameText.text = Main.PlayerNames[player.PlayerId];
                return;
            }   
            var h = "";

            h += $"<color={role.RoleColor}>";
            h += Main.PlayerNames[player.PlayerId];
            h += "\n";
            h += "<size=70%>";
            h += role.RoleName;
            h += "</color>";
            h += k;
            
            player.cosmetics.nameText.text = h;
            return;
        }

        if (role == null)
        {
            if (toAll) player.RpcSetNameToAll(Main.PlayerNames[player.PlayerId]);
            else if (player == seer) player.RpcSetNameLocal(Main.PlayerNames[player.PlayerId]);
            else seer.RpcSetNameSpecific(player, Main.PlayerNames[player.PlayerId]);
            return;
        }
        
        var s = "";
            
        s += $"<color={role.RoleColor}>";
        s += Main.PlayerNames[player.PlayerId];
        s += "\n";
        s += "<size=70%>";
        s += role.RoleName;
        s += "</color>";
        s += k;
        
        if (toAll) player.RpcSetNameToAll(s);
        else if (player == seer) player.RpcSetNameLocal(s);
        else seer.RpcSetNameSpecific(player, s);
    }
    
    public static Dictionary<string, Sprite> CachedSprites = [];
    public static Sprite LoadSprite(string path, float pixelsPerUnit = 1f)
    {
        try
        {
            if (CachedSprites.TryGetValue(path + pixelsPerUnit, out var sprite)) return sprite;
            Texture2D texture = LoadTextureFromResources(path);
            sprite = Sprite.Create(texture, new(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), pixelsPerUnit);
            sprite.hideFlags |= HideFlags.HideAndDontSave | HideFlags.DontSaveInEditor;
            return CachedSprites[path + pixelsPerUnit] = sprite;
        }
        catch (Exception e)
        {
            Main.Logger.LogInfo($"Failed to read Texture： {path}, error: {e.Message}");
        }
        return null;
    }
    public static Texture2D LoadTextureFromResources(string path)
    {
        try
        {
            var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(path);
            if (stream == null) throw new MissingManifestResourceException($"Resource not found: {path}");

            var texture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
            using var ms = new MemoryStream();
            stream.CopyTo(ms);
            texture.LoadImage(ms.ToArray(), false);
            return texture;
        }
        catch (Exception e)
        {
            Main.Logger.LogInfo($"Failed to read Texture： {path}, error: {e.Message}");
        }
        return null;
    }

    public static PlayerControl GetPlayerById(this byte PlayerId)
    {
        foreach (var pc in PlayerControl.AllPlayerControls)
        {
            if (pc.PlayerId == PlayerId) return pc;
        }

        return null;
    }

    public static string GetString(string key)
    {
        switch (Main.CurrentLanguageId)
        {
            default:
                var val = English.Strings[key];
                if (val != null) return val;
                break;
        }
        return string.Empty;
    }
    
    public static string GetRegionName(IRegionInfo region = null)
    {
        try
        {
            region ??= ServerManager.Instance.CurrentRegion;
        }

        catch (Exception e)
        {
            Main.Logger.LogInfo($"{e}");
        }

        string name = "";
        try
        {
            name = region.Name;
        }
        
        catch (Exception e)
        {
            Main.Logger.LogInfo($"{e}");
        }

        if (AmongUsClient.Instance.NetworkMode != NetworkModes.OnlineGame)
        {
            name = "Local Games";
            return name;
        }

        if (region.PingServer.EndsWith(".among.us", StringComparison.Ordinal))
        {
            // Official Server
            if (name == "North America") name = "NA";
            else if (name == "Europe") name = "EU";
            else if (name == "Asia") name = "AS";

            return name;
        } 
        var Ip = region.Servers.FirstOrDefault()?.Ip ?? string.Empty;

        if (Ip.Contains("aumods.us", StringComparison.Ordinal) || Ip.Contains("duikbo.at", StringComparison.Ordinal))
        {
            // Official Modded Server
            if (Ip.Contains("au-eu")) name = "MEU";
            else if (Ip.Contains("au-as")) name = "MAS";
            else if (Ip.Contains("www.")) name = "MNA";

            return name;
        }

        if (Ip.Contains("moddedamong.us", StringComparison.Ordinal))
        {
            // MAUL Server
            if (Ip.Contains("au")) name = "MAUL NA";
            else if (Ip.Contains("eu")) name = "MAUL EU";

            return name;
        }

        if (Ip.Contains("gurge44.eu", StringComparison.Ordinal))
        {
            // EHR Server
            if (Ip.Contains("play-hu")) name = "GG HU";
            else if (Ip.Contains("play-us")) name = "GG US";

            return name;
        }

        if (name.Contains("nikocat233", StringComparison.OrdinalIgnoreCase))
        {
            name = name.Replace("nikocat233", "Niko233", StringComparison.OrdinalIgnoreCase);
        }

        return name;
    }
    public static void DestroyTranslator(this GameObject obj)
    {
        var translator = obj.GetComponent<TextTranslatorTMP>();
        if (translator != null)
        {
            UnityEngine.Object.Destroy(translator);
        }
    }
    public static void DestroyTranslator(this MonoBehaviour obj) => obj.gameObject.DestroyTranslator();

    public static BooleanOptionItem GetBooleanOptionItemFromId(string id)
    {
        foreach (var option in Main.AllBooleanOptionItems.Where(x => x.Id == id)) return option;
        return null;
    }

    public static void SyncSettings(this PlayerControl player)
    {
        
    }
    
    public static NumberOptionItem GetNumberOptionItemFromId(string id)
    {
        foreach (var option in Main.AllNumberOptionItems.Where(x => x.Id == id)) return option;
        return null;
    }

    public static List<RoleOptionItem> GetRoleOptionItemFromCategory(this RoleCategories category)
    {
        switch (category)
        {
            case RoleCategories.CrewmateVanilla:
            case RoleCategories.CrewmateVanillaGhost:        
            case RoleCategories.CrewmateHindering:
            case RoleCategories.CrewmateGhosts:
            case RoleCategories.CrewmateInvestigative:         
            case RoleCategories.CrewmateKilling:
            case RoleCategories.CrewmatePower:           
            case RoleCategories.CrewmateSupport:             
                return GameSettingMenuPatch.CrewmateSettings;
            case RoleCategories.ImpostorGhosts:
            case RoleCategories.ImpostorConcealing:
            case RoleCategories.ImpostorHindering:
            case RoleCategories.ImpostorKilling:
            case RoleCategories.ImpostorSupport:
            case RoleCategories.ImpostorVanilla:
            case RoleCategories.ImpostorVanillaGhosts:
                return GameSettingMenuPatch.ImpostorSettings;
            case RoleCategories.NeutralBenign:
            case RoleCategories.NeutralChaos:
            case RoleCategories.NeutralEvil:
            case RoleCategories.NeutralGhosts:
            case RoleCategories.NeutralKilling:
                return GameSettingMenuPatch.NeutralSettings;
            case RoleCategories.CovenTrickery:
            case RoleCategories.CovenUtility:
            case RoleCategories.CovenKilling:
            case RoleCategories.CovenPower:
            case RoleCategories.CovenGhosts:
                return GameSettingMenuPatch.CovenSettings;
            case RoleCategories.Modifier:
                return GameSettingMenuPatch.ModifierSettings;
        }
        return null;
    }

    public static bool IsKillButton(this CustomRoles role)
    {
        if (role.Basis == RoleTypes.Impostor) return true;
        if (role.Basis == RoleTypes.Shapeshifter) return true;
        if (role.Basis == RoleTypes.Viper) return true;
        if (role.Basis == RoleTypes.Phantom) return true;
        if (role.Basis == RoleTypes.ImpostorGhost) return true;
        return false;
    }
    
    public static PlayerControl GetPlayerById(int PlayerId)
    {
        return PlayerControl.AllPlayerControls.ToArray().FirstOrDefault(pc => pc.PlayerId == PlayerId) ?? null;
    }

    public static CustomRoles GetCustomRole(this PlayerControl player)
    {
        if (!Main.CustomRoles.ContainsKey(player.PlayerId)) return null;
        var role = Main.CustomRoles[player.PlayerId];
        if (role != null) return role;
        return null;
    }
    public static CustomRoles GetGhostRole(this PlayerControl player)
    {
        if (!Main.GhostRoles.ContainsKey(player.PlayerId)) return null;
        var role = Main.GhostRoles[player.PlayerId];
        if (role != null) return role;
        return null;
    }

    public static bool IsPlayerCrewmate(this PlayerControl player)
    {
        if (player.GetCustomRole().Category == RoleCategories.CrewmateVanilla) return true;
        if (player.GetCustomRole().Category == RoleCategories.CrewmateVanillaGhost) return true;
        if (player.GetCustomRole().Category == RoleCategories.CrewmateHindering) return true;
        if (player.GetCustomRole().Category == RoleCategories.CrewmateInvestigative) return true;
        if (player.GetCustomRole().Category == RoleCategories.CrewmateKilling) return true;
        if (player.GetCustomRole().Category == RoleCategories.CrewmatePower) return true;
        if (player.GetCustomRole().Category == RoleCategories.CrewmateSupport) return true;
        if (player.GetCustomRole().Category == RoleCategories.CrewmateGhosts) return true;
        return false;
    }
    public static bool IsPlayerImpostor(this PlayerControl player)
    {
        if (player.GetCustomRole().Category == RoleCategories.ImpostorVanilla) return true;
        if (player.GetCustomRole().Category == RoleCategories.ImpostorConcealing) return true;
        if (player.GetCustomRole().Category == RoleCategories.ImpostorGhosts) return true;
        if (player.GetCustomRole().Category == RoleCategories.ImpostorVanillaGhosts) return true;
        if (player.GetCustomRole().Category == RoleCategories.ImpostorHindering) return true;
        if (player.GetCustomRole().Category == RoleCategories.ImpostorKilling) return true;
        if (player.GetCustomRole().Category == RoleCategories.ImpostorSupport) return true;
        return false;
    }
    public static bool IsPlayerNeutral(this PlayerControl player)
    {
        if (player.GetCustomRole().Category == RoleCategories.NeutralBenign) return true;
        if (player.GetCustomRole().Category == RoleCategories.NeutralEvil) return true;
        if (player.GetCustomRole().Category == RoleCategories.NeutralChaos) return true;
        if (player.GetCustomRole().Category == RoleCategories.NeutralGhosts) return true;
        if (player.GetCustomRole().Category == RoleCategories.NeutralKilling) return true;
        return false;
    }
    public static bool IsPlayerCoven(this PlayerControl player)
    {        
        if (player.GetCustomRole().Category == RoleCategories.CovenUtility) return true;
        if (player.GetCustomRole().Category == RoleCategories.CovenTrickery) return true;
        if (player.GetCustomRole().Category == RoleCategories.CovenPower) return true;
        if (player.GetCustomRole().Category == RoleCategories.CovenKilling) return true;
        if (player.GetCustomRole().Category == RoleCategories.CovenGhosts) return true;
        return false;
    }

    public static Custom_Team GetCustomRoleTeam(this PlayerControl player)
    {
        if (IsPlayerCrewmate(player)) return Custom_Team.Crewmate;
        if (IsPlayerImpostor(player)) return Custom_Team.Impostor;
        if (IsPlayerNeutral(player)) return Custom_Team.Neutral;
        if (IsPlayerCoven(player)) return Custom_Team.Coven;
        return Custom_Team.Unknown;
    }
    public static bool IsRoleCrewmate(this CustomRoles role)
    {
        if (role.Category == RoleCategories.CrewmateVanilla) return true;
        if (role.Category == RoleCategories.CrewmateVanillaGhost) return true;
        if (role.Category == RoleCategories.CrewmateHindering) return true;
        if (role.Category == RoleCategories.CrewmateInvestigative) return true;
        if (role.Category == RoleCategories.CrewmateKilling) return true;
        if (role.Category == RoleCategories.CrewmatePower) return true;
        if (role.Category == RoleCategories.CrewmateSupport) return true;
        if (role.Category == RoleCategories.CrewmateGhosts) return true;
        return false;
    }
    public static bool IsRoleImpostor(this CustomRoles role)
    {
        if (role.Category == RoleCategories.ImpostorVanilla) return true;
        if (role.Category == RoleCategories.ImpostorConcealing) return true;
        if (role.Category == RoleCategories.ImpostorGhosts) return true;
        if (role.Category == RoleCategories.ImpostorHindering) return true;
        if (role.Category == RoleCategories.ImpostorKilling) return true;
        if (role.Category == RoleCategories.ImpostorSupport) return true;
        if (role.Category == RoleCategories.ImpostorVanillaGhosts) return true;
        return false;
    }
    public static bool IsRoleNeutral(this CustomRoles role)
    {
        if (role.Category == RoleCategories.NeutralBenign) return true;
        if (role.Category == RoleCategories.NeutralEvil) return true;
        if (role.Category == RoleCategories.NeutralChaos) return true;
        if (role.Category == RoleCategories.NeutralGhosts) return true;
        if (role.Category == RoleCategories.NeutralKilling) return true;
        return false;
    }
    public static bool IsRoleCoven(this CustomRoles role)
    {        
        if (role.Category == RoleCategories.CovenUtility) return true;
        if (role.Category == RoleCategories.CovenTrickery) return true;
        if (role.Category == RoleCategories.CovenPower) return true;
        if (role.Category == RoleCategories.CovenKilling) return true;
        if (role.Category == RoleCategories.CovenGhosts) return true;
        return false;
    }

    public static Custom_Team GetCustomRoleTeamFromRole(this CustomRoles role)
    {
        if (IsRoleCrewmate(role)) return Custom_Team.Crewmate;
        if (IsRoleImpostor(role)) return Custom_Team.Impostor;
        if (IsRoleNeutral(role)) return Custom_Team.Neutral;
        if (IsRoleCoven(role)) return Custom_Team.Coven;
        return Custom_Team.Unknown;
    }
    
    public static void SendChatMessage(this PlayerControl sendTo, string text, string title = "")
    {
        if (!AmongUsClient.Instance.AmHost) return;
        if (title == "") title = "<color=#aaaaff>System Message</color>";
        if (title.Count(x => x == '\u2605') == 2 && !title.Contains('\n'))
        {
            if (title.Contains('<') && title.Contains('>') && title.Contains('#'))
                title = $"{title[..(title.IndexOf('>') + 1)]}\u27a1{title.Replace("\u2605", "")[..(title.LastIndexOf('<') - 2)]}\u2b05";
            else title = "\u27a1" + title.Replace("\u2605", "") + "\u2b05";
        }

        text = text.Replace("color=", string.Empty);

        if (text.Length > 1000)
        {
            var msg = text.Substring(0, 1000);
            var excess = text.Substring(1000);

            sendTo.RpcSendChatMessage(msg, title);
            sendTo.SendChatMessage(excess, title);
            return;
        };
        
        sendTo.RpcSendChatMessage(text, title);
    }
}

public enum Custom_Team
{
    Crewmate,
    Impostor,
    Neutral,
    Coven,
    Unknown
}