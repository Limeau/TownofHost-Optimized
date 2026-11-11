using System.Collections.Generic;
using AmongUs.GameOptions;
using HarmonyLib;
using Hazel;
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
        
        switch (args[0])
        {
            
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
        
        return true;
    }
}