using HarmonyLib;
using TOHO;
using UnityEngine;

[HarmonyPatch(typeof(NumberOption), nameof(NumberOption.UpdateValue))]
public static class NumberOptionPatch
{
    public static void Postfix(NumberOption __instance)
    {
        if (__instance == null) return;

        foreach (var option in GameSettingMenuPatch.NumberModSettings)
        {
            if (option == null || option.obj == null) continue;

            var number = option.obj.GetComponent<NumberOption>();

            if (number != __instance) continue;

            __instance.ValueText.text = $"{__instance.GetFloat()}";
            option.SetValue(__instance.GetFloat());
            return;
        }

        foreach (var role in Main.AllRoles)
        {
            if (role.Value == null) continue;

            foreach (var option in role.Value.SubNumberOptions)
            {
                if (option == null || option.obj == null) continue;

                var number = option.obj.GetComponent<NumberOption>();

                if (number != __instance) continue;

                __instance.ValueText.text = $"{__instance.GetFloat()}";
                option.SetValue(__instance.GetFloat());
                return;
            }
        }
    }
}