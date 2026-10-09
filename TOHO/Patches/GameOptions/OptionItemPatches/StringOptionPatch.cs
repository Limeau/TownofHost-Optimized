using HarmonyLib;
using TOHO;
using UnityEngine;

[HarmonyPatch(typeof(StringOption))]
public static class StringOptionPatch
{
    [HarmonyPatch(nameof(StringOption.Increase))]
    [HarmonyPrefix]
    public static bool IncreasePrefix(StringOption __instance)
    {
        foreach (var role in Main.AllRoles)
        {
            foreach (var roption in role.Value.SubStringOptions)
            {
                if (roption.obj.GetComponent<StringOption>() == __instance)
                {
                    __instance.ValueText.text = $"{roption.Value + 1}";
                    roption.SetValue(roption.Value + 1);
                }
            }
        }
        if (!StringOptionItem.OptionMap.TryGetValue(__instance, out var option)) return true;

        option.SetValue(option.Value + 1);

        __instance.Value = option.Value;
        __instance.ValueText.text = option.AllValues[option.Value];

        return false;
    }

    [HarmonyPatch(nameof(StringOption.Decrease))]
    [HarmonyPrefix]
    public static bool DecreasePrefix(StringOption __instance)
    {
        foreach (var role in Main.AllRoles)
        {
            foreach (var roption in role.Value.SubStringOptions)
            {
                if (roption.obj.GetComponent<StringOption>() == __instance)
                {
                    __instance.ValueText.text = $"{roption.Value - 1}";
                    roption.SetValue(roption.Value - 1);
                }
            }
        }
        if (!StringOptionItem.OptionMap.TryGetValue(__instance, out var option)) return true;

        option.SetValue(option.Value - 1);

        __instance.Value = option.Value;
        __instance.ValueText.text = option.AllValues[option.Value];

        return false;
    }
}