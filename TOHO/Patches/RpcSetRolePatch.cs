using AmongUs.GameOptions;
using HarmonyLib;

namespace TOHO;
[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcSetRole))]
public static class RpcSetRolePatch
{
    public static bool Prefix(PlayerControl __instance, ref RoleTypes __0)
    {
        return false;
    }
}