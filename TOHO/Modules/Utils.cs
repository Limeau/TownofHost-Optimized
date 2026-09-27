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
}