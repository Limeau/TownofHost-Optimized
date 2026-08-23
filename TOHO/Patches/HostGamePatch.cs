using System;
using AmongUs.GameOptions;
using HarmonyLib;
using Hazel;
using InnerNet;

public static class HostGamePatch
{
    private const string ModGuidString = "995c2cea-a12e-418b-a500-6436eefaaf4d";
    
    public static void Init()
    {
        //CurrentModRegistration.ModRegistrationGuidString = ModGuidString;
    }
    /*
    [HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.HostGame))]
    public static bool Prefix(InnerNetClient __instance, IGameOptions settings, GameFilterOptions filterOpts)
    {
        
        if (!Guid.TryParse(ModGuidString, out Guid guid))
        {
            TOHO.Logger.Info("Failed to parse AMCI mod GUID, falling back to standard HostGame", "HostGamePatch");
            return true;
        }

        MessageWriter msg = MessageWriter.Get(SendOption.Reliable);
        msg.StartMessage(Tags.HostModdedGame);
        msg.WriteBytesAndSize(__instance.gameOptionsFactory.ToBytes(settings, AprilFoolsMode.IsAprilFoolsModeToggledOn));
        msg.Write(CrossplayMode.GetCrossplayFlags());
        filterOpts.Serialize(msg);
        msg.Write(guid.ToByteArray());
        msg.EndMessage();
        __instance.SendOrDisconnect(msg);
        msg.Recycle();

        return false;
    }
    */
}