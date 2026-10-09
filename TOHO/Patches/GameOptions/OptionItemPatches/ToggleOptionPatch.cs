using HarmonyLib;
using TOHO;

[HarmonyPatch(typeof(ToggleOption), nameof(ToggleOption.UpdateValue))]
public static class ToggleOptionPatch
{
    public static void Postfix(ToggleOption __instance)
    {
        foreach (var role in Main.AllRoles)
        {
            foreach (var roption in role.Value.SubBooleanOptions)
            {
                if (roption.obj.GetComponent<ToggleOption>() == __instance)
                {
                    roption.SetValue(__instance.GetBool());
                }
            }
        }
        
        foreach (var option in GameSettingMenuPatch.BoolModSettings)
        {
            if (option.obj.GetComponent<ToggleOption>() == __instance) option.SetValue(__instance.GetBool());
        }

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