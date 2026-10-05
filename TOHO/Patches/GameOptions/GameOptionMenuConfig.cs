using System.Collections.Generic;

namespace TOHO;

public static class GameOptionMenuConfig
{
    public static void InitModSettings()
    {
        /***** Ejections Tab *****/

        GameSettingMenuPatch.BoolModSettings.Add(new BooleanOptionItem(1, "Show Impostors remaining on ejection", OptionTabs.ModSettings, true));
        GameSettingMenuPatch.BoolModSettings.Add(new BooleanOptionItem(2, "Show Neutral Killers remaining on ejection", OptionTabs.ModSettings, true));
        GameSettingMenuPatch.BoolModSettings.Add(new BooleanOptionItem(3, "Show Coven remaining on ejection", OptionTabs.ModSettings, true));
        //GameSettingMenuPatch.NumberModSettings.Add(new NumberOptionItem(10, "Example Number", OptionTabs.ModSettings, 3f, 1f, 5f, 1f));
        Dictionary<int, string> dict = [];
        dict.Add(0, "Show Team");
        dict.Add(1, "Show Role");
        dict.Add(2, "Show Nothing");
        GameSettingMenuPatch.StringModSettings.Add(new StringOptionItem(1, "Confirm Ejections mode", OptionTabs.ModSettings, dict));
    }
}