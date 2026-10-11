using AmongUs.GameOptions;
using HarmonyLib;
using Il2CppSystem;

namespace TOHO;

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcSetRole))]
public static class RpcSetRolePatch
{
    [HarmonyPrefix] public static void Prefix(PlayerControl __instance, ref RoleTypes __0)
    {
        if (!RoleAssign.RoleSet) RoleAssign.AssignRoles();
        
        Main.Logger.LogInfo(
            $"[RoleSync] RpcSetRole called: " +
            $"player={__instance?.Data?.PlayerName}, " +
            $"requestedRole={__0}"
        );
        
        Main.Logger.LogInfo(
            $"[RoleSync] Call stack:\n{Environment.StackTrace}");

        if (__instance == null)
            return;

        if (Main.CustomRoles == null)
        {
            Main.Logger.LogWarning("[RoleSync] CustomRoles is null");
            return;
        }

        Main.Logger.LogInfo(
            $"[RoleSync] CustomRoles count: {Main.CustomRoles.Count}"
        );

        if (!Main.CustomRoles.TryGetValue(__instance, out var customRole))
        {
            Main.Logger.LogWarning(
                $"[RoleSync] Player not found in CustomRoles: " +
                $"{__instance.Data?.PlayerName}"
            );
            return;
        }

        __0 = customRole.Basis;

        Main.Logger.LogInfo(
            $"[RoleSync] Replaced role with {__0}"
        );
    }
}