using System;
using System.Text;
using HarmonyLib;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TOHO;

[HarmonyPatch(typeof(PingTracker), nameof(PingTracker.Update))]
class PingTrackerUpdatePatch
{
    public static PingTracker Instance;
    private static string sb = string.Empty;

    private static bool Prefix(PingTracker __instance)
    {
        try
        {
            Instance = __instance;
            
            __instance.text.alignment = TextAlignmentOptions.Center;
            __instance.text.outlineColor = Color.black;
            __instance.text.outlineWidth = 0.40f; 

            var ping = AmongUsClient.Instance.Ping;
            string pingcolor = "#ff4500";
            if (ping < 30) pingcolor = "#44dfcc";
            else if (ping < 100) pingcolor = "#7bc690";
            else if (ping < 200) pingcolor = "#f3920e";
            else if (ping < 400) pingcolor = "#ff146e";
            sb = $"\r\n<size=75%><color={Main.ModColor}>TOHO</color> {Main.DisplayVersion}<color=#808080>{Main.DisplaySuffix}</color>\n<color={pingcolor}>Ping: {ping} ms</color>\r\n<color=#a54aff>Server: <color=#f34c50>{Utils.GetRegionName()}</color></size>";
            
            __instance.transform.position = GetPingPosition();
            __instance.text.text = sb;

            return false;
        }
        catch (Exception e)
        {
            sb = "";
            Main.Logger.LogInfo($"{e}");
            return false;
        }
    }
    private static Vector3 GetPingPosition()
    {
        var offset_x = 0f;
        var offset_y = 0.5f;
        Vector3 position = new Vector3(offset_x, offset_y, 1000f);
        return position;
    }
}
[HarmonyPatch(typeof(VersionShower), nameof(VersionShower.Start))]
class VersionShowerStartPatch
{
    static TextMeshPro SpecialEventText;
    private static void Postfix(VersionShower __instance)
    {
        var credentialsText = $"<size=100%><b><color={Main.ModColor}>{Main.ModName}</color> <color=#00ac03>{Main.DisplayVersion}</color></b>{Main.DisplaySuffix}</size>";
        var buildtype = "";

        Main.Logger.LogInfo($"v{Main.DisplayVersion}, {buildtype}:{ThisAssembly.Git.Branch}:({ThisAssembly.Git.Commit}), link [{ThisAssembly.Git.RepositoryUrl}], dirty: [{ThisAssembly.Git.IsDirty}]");

        var credentials = Object.Instantiate(__instance.text);
        credentials.text = credentialsText;
        credentials.alignment = TextAlignmentOptions.Center;
        credentials.transform.position = new Vector3(0f, 2.67f, -2f);
        credentials.fontSize = credentials.fontSizeMax = credentials.fontSizeMin = 2f;

        if (SpecialEventText == null && MainMenuManagerStartPatch.TOHOLogo != null)
        {
            SpecialEventText = Object.Instantiate(__instance.text, MainMenuManagerStartPatch.TOHOLogo.transform);
            SpecialEventText.name = "SpecialEventText";
            SpecialEventText.text = "";
            SpecialEventText.color = Color.white;
            SpecialEventText.fontSizeMin = 3f;
            SpecialEventText.alignment = TextAlignmentOptions.Center;
            SpecialEventText.transform.localPosition = new Vector3(0f, 0.8f, 0f);
        }
        if (SpecialEventText != null)
        {
            SpecialEventText.enabled = MainMenuManagerStartPatch.TOHOLogo != null;
        }
        if (Main.IsInitialRelease)
        {
            SpecialEventText.text = $"Happy Birthday to {Main.ModName}!";
            if (ColorUtility.TryParseHtmlString(Main.ModColor, out var col))
            {
                SpecialEventText.color = col;
            }
        }
    }
}