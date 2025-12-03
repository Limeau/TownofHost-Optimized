/*
using AmongUs.GameOptions;
using TOHO.Modules;
using static TOHO.Options;
using static TOHO.Translator;

namespace TOHO.Roles.FolderPath;

internal class Role : RoleBase
{
    //===========================SETUP================================\\
    public override CustomRoles Role => CustomRoles.Role;
    private const int Id = ID;
    public override CustomRoles ThisRoleBase => CustomRoles.Base;
    public override Custom_RoleType ThisRoleType => Custom_RoleType.RoleType;
    //==================================================================\\

    private static OptionItem boolean;
    private static OptionItem float;
    private static OptionItem int;

    public override void SetupCustomOption()
    {
        SetupSingleRoleOptions(Id, TabGroup.FactionRoles, CustomRoles.Role);
        boolean = BooleanOptionItem.Create(Id + 10, "booleanString", false, TabGroup.FactionRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Role]);
        float = FloatOptionItem.Create(Id + 11, "floatString", new(0f, 5f, 0.1f), 1f, TabGroup.FactionRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Role])
        int = IntegerOptionItem.Create(Id + 11, "floatString", new(0, 5, 1), 1, TabGroup.FactionRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Role])
    }
    // Write role logic here
}
*/
