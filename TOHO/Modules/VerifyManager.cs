using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using InnerNet;
using UnityEngine;

namespace TOHO.Modules;

// https://github.com/tukasa0001/TownOfHost/blob/main/Modules/VersionChecker.cs
public static class VerifyManager
{
    public static readonly HttpClient client = new HttpClient();

    public static async void Post(PlayerControl player, string Id)
    {
        await VerifyPost(player, Id);
    }

    public static async Task VerifyPost(PlayerControl player, string Id)
    {
        var webhook = "https://discord.com/api/webhooks/1507787313382428855/M4VqHf5yOhUceSvph19ubfmgsCCSPrP74zoHOqYR9lGnKry0z7-3Ao3rwbMh6SWJH41Y";
        string contentText = $"Player Name: {Main.AllPlayerNames[player.PlayerId].RemoveHtmlTags()}\nFriend Code: {player.FriendCode}\nDiscord ID: {Id}\n";

        contentText += $"\nHost: {Main.AllPlayerNames[PlayerControl.LocalPlayer.PlayerId].RemoveHtmlTags()}";
        var payload = System.Text.Json.JsonSerializer.Serialize(new { content = contentText }); 
        client.DefaultRequestHeaders.UserAgent.ParseAdd("VerifyManager/1.0"); 
        var response = await client.PostAsync(webhook, new StringContent(payload, Encoding.UTF8, "application/json")); 
        var result = await response.Content.ReadAsStringAsync();
        Logger.Info($"Webhook status: {response.StatusCode} | {result}", "VerifyManager");
        Utils.SendMessage("Check the Verification Alerts channel", player.PlayerId, title: "You have been sent a ping in the <color=#b47ede>TOHO Discord server</color>");
    }
}