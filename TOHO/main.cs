using System;
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
    
    public static int CurrentLanguageId;
    
    public static bool IsInitialRelease = DateTime.Now.Month == 7 && DateTime.Now.Day == 27;
    
    public override void Load()
    {
        Instance = this;
        Logger = BepInEx.Logging.Logger.CreateLogSource("TownofHostOptimized");
        Logger.LogInfo($"{Application.version} Among Us Version");
        harmony.PatchAll();
        
        #if RELEASE
        ConsoleManager.DetachConsole();
        #endif

        CurrentLanguageId = 0;
        Logger.LogInfo("========= TOHO loaded! =========");
    }
}