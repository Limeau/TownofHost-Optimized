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
using UnityEngine;

namespace TOHO;

public static class Utils
{
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

    public static BooleanOptionItem GetBooleanOptionItemFromId(int id)
    {
        foreach (var option in Main.AllBooleanOptionItems.Where(x => x.Id == id)) return option;
        return null;
    }
    public static NumberOptionItem GetNumberOptionItemFromId(int id)
    {
        foreach (var option in Main.AllNumberOptionItems.Where(x => x.Id == id)) return option;
        return null;
    }

    public static List<RoleOptionItem> GetRoleOptionItemFromCategory(this RoleCategories category)
    {
        switch (category)
        {
            case RoleCategories.CrewmateVanilla:
                return GameSettingMenuPatch.CrewmateSettings;
            case RoleCategories.CrewmateVanillaGhost:                
                return GameSettingMenuPatch.CrewmateSettings;
            case RoleCategories.ImpostorVanilla:
                return GameSettingMenuPatch.ImpostorSettings;
        }
        return null;
    }

    public static bool IsKillButton(this CustomRoles role)
    {
        if (role.Basis == RoleTypes.Impostor) return true;
        if (role.Basis == RoleTypes.Shapeshifter) return true;
        if (role.Basis == RoleTypes.Viper) return true;
        if (role.Basis == RoleTypes.Phantom) return true;
        return false;
    }

    public static CustomRoles GetCustomRole(this PlayerControl player)
    {
        var role = Main.CustomRoles[player];
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
}

public enum Custom_Team
{
    Crewmate,
    Impostor,
    Neutral,
    Coven,
    Unknown
}