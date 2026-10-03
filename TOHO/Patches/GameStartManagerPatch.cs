using HarmonyLib;

[HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Update))]
public static class GameStartManagerPatch
{
    public static void Prefix(GameStartManager __instance)
    {
        __instance.MinPlayers = 1;
    }
}