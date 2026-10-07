using AmongUs.GameOptions;

namespace TOHO;

public class CustomRoles
{
    public int RoleID;
    public string RoleName;
    public string ShortDescription;
    public string LongDescription;
    public string RoleColor;
    public RoleTypes Basis;
    public RoleCategories Category;
    
    public RoleOptionItem Option;

    public CustomRoles(int roleID, string roleName, string shortDescription, string longDescription, string roleColor, RoleTypes basis, RoleCategories category)
    {
        RoleID = roleID;
        RoleName = roleName;
        ShortDescription = shortDescription;
        LongDescription = longDescription;
        RoleColor = roleColor;
        Basis = basis;
        Category = category;
        
        Main.AllRoles.Add(roleName, this);
    }

    public void SetupCustomOption()
    {
        Option = new RoleOptionItem(this, RoleID, $"<color={RoleColor}>{RoleName}</color>", RoleCategories.CrewmateVanilla);
        GameSettingMenuPatch.CrewmateSettings.Add(Option);
    }

    public void SetupAdvancedOptions()
    {
        
    }
}