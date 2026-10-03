using AmongUs.Data;
using Discord;
using InnerNet;
using System;
using HarmonyLib;

namespace TOHO.Patches
{
#if ANDROID
#else
    // Originally from Town of Us Rewritten, by Det
    [HarmonyPatch(typeof(ActivityManager), nameof(ActivityManager.UpdateActivity))]
    public class DiscordRPC
    {
        private static string lobbycode = "";
        private static string region = "";
        public static void Prefix([HarmonyArgument(0)] Activity activity)
        {
            if (activity == null) return;

            var details = $"TOHO {Main.DisplayVersion + Main.DisplaySuffix}";
            activity.Details = details;

            activity.Assets = new ActivityAssets
            {
                LargeImage = "https://i.imgur.com/7c1JEZC.png"
            };
            if (activity.State != "In Menus") 
            { 
                if (!DataManager.Settings.Gameplay.StreamerMode) 
                { 
                    if (AmongUsClient.Instance.GameState == InnerNetClient.GameStates.Joined) 
                    { 
                        lobbycode = GameCode.IntToGameName(AmongUsClient.Instance.GameId);
                        region = Utils.GetRegionName();
                        details = $"TOHO {Main.DisplayVersion + Main.DisplaySuffix} ({lobbycode} - {region})";
                    }

                    
                    if (lobbycode != "" && region != "")
                    {
                        if (AmongUsClient.Instance.GameState == InnerNetClient.GameStates.Started)
                            details = $"TOHO {Main.DisplayVersion + Main.DisplaySuffix} ({lobbycode} - {region})";
                    }

                    activity.Details = details;
                }
                else
                {
                    if (AmongUsClient.Instance.GameState == InnerNetClient.GameStates.Joined) details = $"TOHO {Main.DisplayVersion + Main.DisplaySuffix}";
                    if (AmongUsClient.Instance.GameState == InnerNetClient.GameStates.Started) details = $"TOHO {Main.DisplayVersion + Main.DisplaySuffix}";
                    activity.Details = details;
                        
                }
            }
        }
    }
#endif
}