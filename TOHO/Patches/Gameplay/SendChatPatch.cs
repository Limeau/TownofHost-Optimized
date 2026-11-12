using System.Collections.Generic;
using AmongUs.GameOptions;
using HarmonyLib;
using Hazel;
using InnerNet;
using TOHO;

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.HandleRpc))]
internal class RPCHandlerPatch
{
    public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] byte callId, [HarmonyArgument(1)] MessageReader reader)
    {
        var rpcType = (RpcCalls)callId;
        MessageReader subReader = MessageReader.Get(reader);
        if (rpcType == RpcCalls.SendChat)
        {
            var text = subReader.ReadString();
            Main.Logger.LogInfo($"{Main.PlayerNames[__instance.PlayerId]}:{text}");
            var canceled = ChatCommands.OnReceiveChat(__instance, text);
            return !canceled;
        }
        
        return true;
    }
}

[HarmonyPatch(typeof(ChatController), nameof(ChatController.SendChat))]
internal class SendChatPatch
{
    public static bool Prefix(ChatController __instance)
    {
        var canceled = false;
        ChatCommands.OnReceiveChat(PlayerControl.LocalPlayer, __instance.freeChatField.textArea.text);
        return !canceled;
    }
}

public static class ChatCommands
{
    public static bool OnReceiveChat(PlayerControl sender, string message)
    {
        string[] args = message.Trim().Split(' ');
        
        bool send = true;

        if (Main.CustomRoles.ContainsKey(sender.PlayerId) && AmongUsClient.Instance.GameState == InnerNetClient.GameStates.Started)
        {
            var r = Main.CustomRoles[sender.PlayerId];
            r.OnSendChat(sender, message);
        }

        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (Main.CustomRoles.ContainsKey(player.PlayerId) && AmongUsClient.Instance.GameState == InnerNetClient.GameStates.Started)
            {
                var r = Main.CustomRoles[player.PlayerId];
                r.OnSendChatAsSpect(sender, message);
            }
        }
        
        switch (args[0])
        {
            case "/m":
                if (Main.CustomRoles.ContainsKey(sender.PlayerId)) sender.SendChatMessage(Main.CustomRoles[sender.PlayerId].LongDescription, Main.CustomRoles[sender.PlayerId].RoleName);
                else sender.SendChatMessage("You do not have a custom role assigned.");
                break;
            case "/r":
                string en = "";
                string db = "";
                foreach (var role in Main.AllRoles)
                {
                    if (args.Length <= 1)
                    {
                        if (role.Value.Enabled) en += $"<color={role.Value.RoleColor}>{role.Value.RoleName}</color>\n";
                        else db += $"{role.Value.RoleName}\n";
                    }
                    
                    else if (args[1].ToLower() == role.Key.ToLower())
                    {
                        sender.SendChatMessage(role.Value.LongDescription, role.Key);
                        break;
                    }
                }
                
                if (en.Length > 0)
                {
                    sender.SendChatMessage(en, "<color=#00ff00>Enabled Roles</color>");
                    sender.SendChatMessage(db, "<color=#ff0000>Disabled Roles</color>");
                }
                
                break;
        }
        
        return send;
    }
}