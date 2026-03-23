using System.Collections.Generic;
using static TOHO.Options;

namespace TOHO.Roles.Modifiers.Common;

public class DoubleShot : IModifier
{
    public CustomRoles Role => CustomRoles.DoubleShot;
    public static readonly HashSet<byte> IsActive = [];
    public ModifierTypes Type => ModifierTypes.Guesser;


    public static OptionItem ImpCanBeDoubleShot;
    public static OptionItem CrewCanBeDoubleShot;
    public static OptionItem NeutralCanBeDoubleShot;
    public static OptionItem CovenCanBeDoubleShot;

    public void SetupCustomOption()
    {
        SetupAdtRoleOptions(19200, CustomRoles.DoubleShot, canSetNum: true, tab: TabGroup.Modifiers);
        ImpCanBeDoubleShot = BooleanOptionItem.Create(19210, "ImpCanBeDoubleShot", true, TabGroup.Modifiers, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.DoubleShot]);
        CrewCanBeDoubleShot = BooleanOptionItem.Create(19211, "CrewCanBeDoubleShot", true, TabGroup.Modifiers, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.DoubleShot]);
        NeutralCanBeDoubleShot = BooleanOptionItem.Create(19212, "NeutralCanBeDoubleShot", true, TabGroup.Modifiers, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.DoubleShot]);
        CovenCanBeDoubleShot = BooleanOptionItem.Create(19213, "CovenCanBeDoubleShot", true, TabGroup.Modifiers, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.DoubleShot]);
    }
    public void Init()
    {
        IsActive.Clear();
    }
    public void Add(byte playerId, bool gameIsLoading = true)
    { }
    public void Remove(byte playerId)
    { }
}
