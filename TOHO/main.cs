using System;
using System.Collections.Generic;
using System.IO;
using AmongUs.GameOptions;
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

    public static Dictionary<string, CustomRoles> AllRoles = [];
    public static Dictionary<PlayerControl, CustomRoles> CustomRoles = [];
    public static Dictionary<PlayerControl, string> PlayerNames = [];
    
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

        SetupCustomRoles();
        
        foreach (var role in AllRoles)
        {
            role.Value.SetupCustomOption();
        }
        OptionSaver.Load();
        harmony.PatchAll();
        
        #if RELEASE
        ConsoleManager.DetachConsole();
        #endif
        
        CurrentLanguageId = 0;
        Logger.LogInfo("========= TOHO loaded! =========");
    }

    public static void SetupCustomRoles()
    {
        new CustomRoles(1100, "Crewmate", "Find and eject the Impostors", "The Crewmate wins at the end of the game if all of the Impostors, Neutral Killers, and Coven are no longer living.", "#00ffff", RoleTypes.Crewmate, RoleCategories.CrewmateVanilla).SetupScreenshot("TOHO.Resources.RoleScreenshots.Crewmate.png");
        new CustomRoles(1200, "Engineer", "Use the vents", "The Engineer has the ability to use the vents at all times except during Comms sabotages.", "#E6731E", RoleTypes.Engineer, RoleCategories.CrewmateVanilla);
        new CustomRoles(1300, "Scientist", "Access player vitals", "The Scientist can access portable vitals anywhere, allowing them to see if players are currently living or dead.", "#2A27F5", RoleTypes.Scientist, RoleCategories.CrewmateVanilla);
        new CustomRoles(1400, "Tracker", "Track players' location", "The Tracker can pick one player to track, and for a duration of time will know where they are on the map.", "#827153", RoleTypes.Tracker, RoleCategories.CrewmateVanilla);
        new CustomRoles(1500, "Noisemaker", "Alert players when you die", "When the Noisemaker is killed, all players will receive an alert that tells the players where the Noisemaker's body is.", "#24cf69", RoleTypes.Noisemaker, RoleCategories.CrewmateVanilla);
        new CustomRoles(1600, "Detective", "Take notes & discover Impostors", "The Detective can use the Interrogate button to try and find an Impostor, and take notes on players.", "#D3DFE3", RoleTypes.Detective, RoleCategories.CrewmateVanilla);
        new CustomRoles(1700, "Judge", "Take control of a meeting", "Once the Judge completes a certain amount of tasks, they can eject a player during a meeting single handedly.", "#7F8385", RoleTypes.Judge, RoleCategories.CrewmateVanilla);
        new CustomRoles(1800, "Impostor", "Stay hidden and kill all Crewmates", "The Impostor wins at the end of the game if the Impostor team outnumbers the Crewmates, and all other killers (Neutral Killers and Coven) are dead.", "#ff1919", RoleTypes.Impostor, RoleCategories.ImpostorVanilla).SetupAdvancedNumberOption(new NumberOptionItem(1802, "Kill Cooldown", OptionTabs.RoleSetting, 20f, 10f, 60f, 1f));
        new CustomRoles(1900, "Shapeshifter", "Disguise yourself as another player", "The Shapeshifter can use the Shift button to temporarily disguise themself as a select player.", "#ff1919", RoleTypes.Shapeshifter, RoleCategories.ImpostorVanilla).SetupAdvancedNumberOption(new NumberOptionItem(1902, "Kill Cooldown", OptionTabs.RoleSetting, 20f, 10f, 60f, 1f));
        new CustomRoles(2000, "Phantom", "Go invisible to conceal your kills", "The Phantom can use the Vanish button to temporarily go invisible. The Phantom cannot kill players while they are inviisible.", "#ff1919", RoleTypes.Phantom, RoleCategories.ImpostorVanilla).SetupAdvancedNumberOption(new NumberOptionItem(2002, "Kill Cooldown", OptionTabs.RoleSetting, 20f, 10f, 60f, 1f));
        new CustomRoles(2100, "Viper", "Slowly discard of all of your kills", "When the Viper kills, the body will slowly rot, to the point it is not visible to other players and cannot be reported.", "#ff1919", RoleTypes.Viper, RoleCategories.ImpostorVanilla).SetupAdvancedNumberOption(new NumberOptionItem(2102, "Kill Cooldown", OptionTabs.RoleSetting, 20f, 10f, 60f, 1f));
        new CustomRoles(2200, "Guardian Angel", "Protect Crewmates from Impostors' attacks", "The Guardian Angel can use the Protect button to temporarily protect players from kill button attacks.", "#A0FAFA", RoleTypes.GuardianAngel, RoleCategories.CrewmateVanillaGhost);
        new CustomRoles(2300, "Influencer", "Send the alive Crewmates hints about the game", "The Influencer can send the alive Crewmates hints about the game, in the form of pictures pre-decided by the game.", "#E8D125", RoleTypes.SpiritGuide, RoleCategories.CrewmateVanillaGhost);
    }
}