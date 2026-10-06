using HarmonyLib;

[HarmonyPatch(typeof(Scroller), nameof(Scroller.UpdateScrollBars))]
public static class ScrollBarPatch
{
    public static float stupidBitch;
    public static void Prefix(Scroller __instance)
    {
        if (__instance == GameSettingMenuPatch.ModSettingsTab.scrollBar)
        {
            stupidBitch = GameSettingMenuPatch.ModSettingsTab.scrollBar.ContentYBounds.min;
        }
    }
    public static void Postfix(Scroller __instance)
    {
        if (__instance == GameSettingMenuPatch.ModSettingsTab.scrollBar)
        {
           GameSettingMenuPatch.ModSettingsTab.scrollBar.ContentYBounds.min = stupidBitch;
        }
    }
}