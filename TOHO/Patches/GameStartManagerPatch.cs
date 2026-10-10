using HarmonyLib;
using TOHO;

[HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Update))]
public static class GameStartManagerPatch
{
    public static void Prefix(GameStartManager __instance)
    {
        __instance.MinPlayers = 1;
    }
}

[HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Start))]
public static class GameStartManagerPatchPostfix
{
    public static void Postfix(GameStartManager __instance)
    {
        
        new LateTask(() =>
        {
            __instance.ClickEdit();
        }, 0.1f);
        new LateTask(() => 
        {
            GameSettingMenuPatch.ModSettingsButton.ReceiveClickDown();
        }, 0.3f);        
        new LateTask(() => 
        {
            GameSettingMenuPatch.VanillaSettingsButton.ReceiveClickDown();
        }, 0.5f);        
        new LateTask(() =>
        { 
            GameSettingMenuPatch.RoleSettingsButton.ReceiveClickDown();
        }, 0.7f);       
        new LateTask(() =>
        {
            GameSettingMenu.Instance.Close();
        }, 0.9f);
    }
}