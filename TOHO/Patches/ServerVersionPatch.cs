using HarmonyLib;

namespace TOHO.Patches;

[HarmonyPatch(typeof(Constants), nameof(Constants.GetBroadcastVersion))]
class ServerUpdatePatch
{
    static void Postfix(ref int __result)
    {
        if (AmongUsClient.Instance.NetworkMode == NetworkModes.LocalGame)
        {
            Main.Logger.LogInfo($"IsLocalGame: {__result}");
        }
        if (AmongUsClient.Instance.NetworkMode == NetworkModes.OnlineGame)
        {
            __result += 25;
            Main.Logger.LogInfo($"IsOnlineGame: {__result}");
        }
    }
}
[HarmonyPatch(typeof(Constants), nameof(Constants.IsVersionModded))]
public static class IsVersionModdedPatch
{
    public static bool Prefix(ref bool __result)
    {
        __result = true;
        return false;
    }
}