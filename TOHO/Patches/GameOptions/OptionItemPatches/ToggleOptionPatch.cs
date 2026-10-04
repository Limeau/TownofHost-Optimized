using HarmonyLib;

[HarmonyPatch(typeof(ToggleOption), nameof(ToggleOption.UpdateValue))]
public static class ToggleOptionPatch
{
    public static void Postfix(ToggleOption __instance)
    {
        foreach (var option in GameSettingMenuPatch.BoolModSettings)
        {
            if (option.obj.GetComponent<ToggleOption>() == __instance) option.SetValue(__instance.GetBool());
        }
    }
}