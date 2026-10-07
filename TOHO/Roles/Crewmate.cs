/*using AmongUs.GameOptions;

namespace TOHO;

internal class Crewmate
{
    public static CustomRoles Role => CustomRoles.Crewmate;

    public static int RoleID => 10;
    public static string RoleName => "Crewmate";
    public static string ShortDescription => "Find and eject the Impostors";
    public static string LongDescription => "The Crewmate wins at the end of the game if all of the Impostors, Neutral Killers, and Coven are no longer living.";
    public static string RoleColor => "#00ffff";
    public static RoleTypes Basis => RoleTypes.Crewmate;
    public static RoleCategories Category => RoleCategories.CrewmateVanilla;

    public static void SetupCustomOption()
    {
        var parentOption = new RoleOptionItem(CustomRoles.Crewmate, RoleID, RoleName, Category);
        GameSettingMenuPatch.CrewmateSettings.Add(parentOption);
    }
}*/