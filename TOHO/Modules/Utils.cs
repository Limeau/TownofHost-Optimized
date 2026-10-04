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
}