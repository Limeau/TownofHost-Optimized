using AmongUs.Data;
using Discord;
using System;

namespace TOHE.Patches
{
    // Originally from Town of Us Rewritten, by Det
    [HarmonyPatch(typeof(ActivityManager), nameof(ActivityManager.UpdateActivity))]
    public class DiscordRPC
    {
        private static string lobbycode = "";
        private static string region = "";
        public static void Prefix([HarmonyArgument(0)] Activity activity)
        {
            if (activity == null) return;

            var details = $"TOHO v{Main.PluginDisplayVersion}";
            activity.Details = details;

            try
            {
                if (activity.State != "In Menus")
                {
                    if (!DataManager.Settings.Gameplay.StreamerMode)
                    {
                        int maxSize = GameOptionsManager.Instance.CurrentGameOptions.MaxPlayers;
                        if (GameStates.IsLobby)
                        {
                            lobbycode = GameStartManager.Instance.GameRoomNameCode.text;
                            region = ServerManager.Instance.CurrentRegion.Name;
                            if (region == "North America") region = "NA";
                            else if (region == "Europe") region = "EU";
                            else if (region == "Asia") region = "AS";
                            else if (region.Contains("MNA")) region = "MNA";
                            else if (region.Contains("MEU")) region = "MEU";
                            else if (region.Contains("MAS")) region = "MAS";
                        }

                        if (lobbycode != "" && region != "")
                        {
                            if (GameStates.IsNormalGame)
                                details = $"TOHO - {lobbycode} ({region})";

                            else if (GameStates.IsHideNSeek)
                                details = $"TOHO Hide & Seek - {lobbycode} ({region})";
                        }

                        activity.Details = details;
                    }
                    else
                    {
                        if (GameStates.IsNormalGame)
                            details = $"TOHO v{Main.PluginDisplayVersion}";

                        else if (GameStates.IsHideNSeek)
                            details = $"TOHO v{Main.PluginDisplayVersion} - Hide & Seek";

                        else details = $"TOHO v{Main.PluginDisplayVersion}";

                        activity.Details = details;
                    }
                }
            }

            catch (ArgumentException ex)
            {
                Logger.Error("Error in updating discord rpc", "DiscordPatch");
                Logger.Exception(ex, "DiscordPatch");
                details = $"TOHO v{Main.PluginDisplayVersion}";
                activity.Details = details;
            }
        }
    }
}
