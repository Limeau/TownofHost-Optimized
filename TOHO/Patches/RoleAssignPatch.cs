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

        if (__instance == null || Main.CustomRoles == null)
        {
            return;
        }

        if (__instance.Data.IsDead)
        {
            __0 = SetAsAGhost(__instance); 
        }

        else 
        {
            if (!Main.CustomRoles.TryGetValue(__instance, out var customRole))
            {
                return;
            }

            __0 = customRole.Basis;
        }
        
        __instance.SetNameWithRole();
    }

    public static RoleTypes SetAsAGhost(PlayerControl player)
    {
        if (Main.GhostRoles.TryGetValue(player, out var customRole))
        {
            return customRole.Basis;
        }
        return player.GetCustomRole().Basis;
    }
}