using HarmonyLib;
using UnityEngine;

[HarmonyPatch(typeof(StringOption))]
public static class StringOptionPatch
{
    [HarmonyPatch(nameof(StringOption.Increase))]
    [HarmonyPrefix]
    public static bool IncreasePrefix(StringOption __instance)
    {
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
        if (!StringOptionItem.OptionMap.TryGetValue(__instance, out var option)) return true;

        option.SetValue(option.Value - 1);

        __instance.Value = option.Value;
        __instance.ValueText.text = option.AllValues[option.Value];

        return false;
    }
}