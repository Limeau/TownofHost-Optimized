using HarmonyLib;

namespace TOHO;

// Ported from AmongUsQoLMod's HideRulesButtonFeature.cs. Hides the Roles
// List button - the "?" notepad icon next to Chat that opens
// HowToPlayScene's role-info pages.
//
// Entirely local: toggling this only ever calls GameObject.SetActive() on
// our own client's copy of the HUD. Nothing to RPC, nothing for the host to
// acknowledge - this needs no host-only guard and works identically whether
// you're hosting or not.
//
// The button is HudManager.MatchInfoButton (confirmed directly against
// Assembly-CSharp.dll in the source project this was ported from).
// HudManager is the persistent in-game HUD, not something rebuilt per
// meeting, so this patches HudManager.Update rather than MeetingHud.
[HarmonyPatch(typeof(HudManager), nameof(HudManager.Start))]
internal static class HideMatchInfoButton_HudManager_Start_Patch
{
    public static void Postfix(HudManager __instance)
    {
        if (__instance.MatchInfoButton == null)
            Logger.Warn("HudManager.MatchInfoButton is null - the Roles List button field may have been renamed in this game version.", "HideMatchInfoButton");
    }
}

// Re-applies every frame rather than trusting a one-time SetActive() to
// stick. HudManager.Update() runs constantly during a match, so this keeps
// the button hidden the whole game rather than just once at Start.
[HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
internal static class HideMatchInfoButton_HudManager_Update_Patch
{
    public static void Postfix(HudManager __instance)
    {
        var button = __instance.MatchInfoButton;
        if (button == null) return;

        var shouldBeVisible = !Main.HideRulesButton.Value;
        if (button.gameObject.activeSelf != shouldBeVisible)
            button.gameObject.SetActive(shouldBeVisible);
    }
}
