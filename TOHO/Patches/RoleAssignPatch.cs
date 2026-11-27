using HarmonyLib;

namespace TOHO;
[HarmonyPatch(typeof(RoleManager), nameof(RoleManager.SelectRoles))]
public static class SelectRolePatch
{
    [HarmonyPrefix] public static void Prefix()
    {
        if (!RoleAssign.RoleSet) RoleAssign.AssignRoles();
    }
}