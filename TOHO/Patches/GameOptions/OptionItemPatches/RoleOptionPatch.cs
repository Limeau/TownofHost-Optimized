using HarmonyLib;

[HarmonyPatch(typeof(ToggleOption), nameof(ToggleOption.UpdateValue))]
public static class RoleOptionPatch
{
    public static void Postfix(ToggleOption __instance)
    {
        foreach (var option in GameSettingMenuPatch.CrewmateSettings)
        {
            if (option.obj.GetComponent<ToggleOption>() == __instance) option.SetValue(__instance.GetBool());
        }
        foreach (var option in GameSettingMenuPatch.ImpostorSettings)
        {
            if (option.obj.GetComponent<ToggleOption>() == __instance) option.SetValue(__instance.GetBool());
        }
        foreach (var option in GameSettingMenuPatch.NeutralSettings)
        {
            if (option.obj.GetComponent<ToggleOption>() == __instance) option.SetValue(__instance.GetBool());
        }
        foreach (var option in GameSettingMenuPatch.CovenSettings)
        {
            if (option.obj.GetComponent<ToggleOption>() == __instance) option.SetValue(__instance.GetBool());
        }
        foreach (var option in GameSettingMenuPatch.ModifierSettings)
        {
            if (option.obj.GetComponent<ToggleOption>() == __instance) option.SetValue(__instance.GetBool());
        }
    }
}