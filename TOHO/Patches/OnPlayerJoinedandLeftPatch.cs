using System;
using System.Linq;
using System.Text.RegularExpressions;
using HarmonyLib;
using InnerNet;
using TOHO;

[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnPlayerJoined))]
public static class OnPlayerJoinedPatch
{
    public static bool IsDisconnected(this ClientData client)
    {
        var __instance = AmongUsClient.Instance;
        for (int i = 0; i < __instance.allClients.Count; i++)
        { 
            ClientData clientData = __instance.allClients[i]; 
            if (clientData.Id == client.Id)
            {
                return true;
            }
        } 
        return false; 
    }    
    public static bool HasInvalidFriendCode(string friendcode)
    {
        if (string.IsNullOrEmpty(friendcode))
        {
            return true;
        }

        if (friendcode.Length < 7)
        {
            return true;
        }

        if (friendcode.Count(c => c == '#') != 1)
        {
            return true;
        }

        string pattern = @"[\W\d]";
        if (Regex.IsMatch(friendcode[..friendcode.IndexOf("#")], pattern))
        {
            return true;
        }

        return false;
    }

    private static bool IsSuspiciousPlayer(ClientData client)
    {
        if (client.PlayerName.Contains("SEEKER"))
            return true;

        if (client.PlatformData.Platform == Platforms.Unknown)
            return true;

        return false;
    }
    public static void Postfix([HarmonyArgument(0)] ClientData client)
    {
        Main.Logger.LogInfo($"{client.PlayerName}(ClientID:{client.Id}/FriendCode:{client.FriendCode}/Puid:{client.ProductUserId}/Platform:{client.PlatformData.Platform}) Joining room");
        
        Main.PlayerNames[client.Character] = client.PlayerName;
        
        _ = new LateTask(() =>
        {
            try
            {
                // Check for suspicious players
                if (IsSuspiciousPlayer(client))
                {
                    AmongUsClient.Instance.KickPlayer(client.Id, false);
                    Main.Logger.LogInfo($"Kicked suspicious player: {client.PlayerName}");
                    return;
                }
            }
            catch { }
        }, 4.5f, "green bean kick late task", false);
        
        if (DestroyableSingleton<FriendsListManager>.Instance.IsPlayerBlockedUsername(client.FriendCode) && AmongUsClient.Instance.AmHost)
        {
            AmongUsClient.Instance.KickPlayer(client.Id, true);
            Main.Logger.LogInfo($"Blocked player {client?.PlayerName}({client.FriendCode}) has been banned.");
        }

        if (AmongUsClient.Instance.AmHost && HasInvalidFriendCode(client.FriendCode) && Options.KickPlayerFriendCodeInvalid.Value)
        {
            if (AmongUsClient.Instance.NetworkMode == NetworkModes.LocalGame) return;
            var region = ServerManager.Instance.CurrentRegion;
            var Ip = region.Servers.FirstOrDefault()?.Ip ?? string.Empty;
            if (Ip.Contains("aumods.us", StringComparison.Ordinal) || Ip.Contains("duikbo.at", StringComparison.Ordinal)) return;
            AmongUsClient.Instance.KickPlayer(client.Id, false);
            Main.Logger.LogInfo($"Kicked a player {client?.PlayerName} because they have an invalid friend code");
        }
    }
}


[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnPlayerLeft))]
public static class OnPlayerLeftPatch
{
    public static void Prefix([HarmonyArgument(0)] ClientData client)
    {
        client.UpdatePlayerName(Main.PlayerNames[client.Character]);
        Main.PlayerNames.Remove(client.Character);
    }
}
