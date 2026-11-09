using HarmonyLib;
using TOHO;

[HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Update))]
public static class GameStartManagerPatch
{
    public static void Prefix(GameStartManager __instance)
    {
        __instance.MinPlayers = 1;
    }
}

[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.CoStartGameHost))]
public static class CoStartGamePatch
{
    public static bool Prefix(AmongUsClient __instance, ref Il2CppSystem.Collections.IEnumerator __result)
    {
        RoleAssign.RoleSet = false;
        return true;
    }
}