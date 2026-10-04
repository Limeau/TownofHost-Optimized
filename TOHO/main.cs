using System;
using System.Collections.Generic;
using System.IO;
using AmongUs.InnerNet.GameDataMessages;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using Hazel;
using InnerNet;

namespace TOHO;

[BepInPlugin("com.Limeau.TownofHostOptimized", "TownofHostOptimized", "4.0.0")]
[BepInProcess("Among Us.exe")]
public class Main : BasePlugin
{
    public static Main Instance;
    public static ManualLogSource Logger;
    public Harmony harmony = new("com.Limeau.TownofHostOptimized");

    public static string ModColor = "#b47ede";
    public static string ModName = "Town of Host Optimized";
    public static string DisplayVersion = "v4.0.0";
    public static string DisplaySuffix = " Beta 1";
    
    public static List<BooleanOptionItem> AllBooleanOptionItems;
    public static List<NumberOptionItem> AllNumberOptionItems;
    
    public static int CurrentLanguageId;
    
    public static bool IsInitialRelease = DateTime.Now.Month == 7 && DateTime.Now.Day == 27;
    
    public static string StarData => Environment.GetEnvironmentVariable("STAR_DATA_PATH");  
    
    public static readonly string BasePath =
        OperatingSystem.IsAndroid()
            ? (!string.IsNullOrEmpty(StarData) ? StarData : Application.persistentDataPath)
            : Paths.GameRootPath;

    public static string TohoData => Path.Combine(BasePath, "TOHO-DATA");
    
    public override void Load()
    {
        Instance = this;
        Logger = BepInEx.Logging.Logger.CreateLogSource("TownofHostOptimized");
        Logger.LogInfo($"{Application.version} Among Us Version");
        if (!Directory.Exists(TohoData)) Directory.CreateDirectory(TohoData);
        OptionSaver.Load();
        harmony.PatchAll();
        
        #if RELEASE
        ConsoleManager.DetachConsole();
        #endif

        
        CurrentLanguageId = 0;
        Logger.LogInfo("========= TOHO loaded! =========");
        
    }
}