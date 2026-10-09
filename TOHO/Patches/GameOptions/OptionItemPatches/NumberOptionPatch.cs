using HarmonyLib;
using TOHO;

[HarmonyPatch(typeof(NumberOption), nameof(NumberOption.UpdateValue))]
public static class NumberOptionPatch
{
    public static void Postfix(NumberOption __instance)
    {
        foreach (var option in GameSettingMenuPatch.NumberModSettings)
        {
            if (option.obj.GetComponent<NumberOption>() == __instance)
            {
                __instance.ValueText.text = $"{__instance.GetFloat()}";
                option.SetValue(__instance.GetFloat());
            }
        }
        foreach (var role in Main.AllRoles)
        {
            foreach (var option in role.Value.SubNumberOptions)
            {
                if (option.obj.GetComponent<NumberOption>() == __instance)
                {
                    __instance.ValueText.text = $"{__instance.GetFloat()}";
                    option.SetValue(__instance.GetFloat());
                }
            }
        }
    }
}