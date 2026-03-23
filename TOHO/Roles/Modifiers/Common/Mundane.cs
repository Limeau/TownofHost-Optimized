using static TOHO.Options;

namespace TOHO.Roles.Modifiers.Common;

public class Mundane : IModifier
{
    public CustomRoles Role => CustomRoles.Mundane;
    private const int Id = 26700;
    public ModifierTypes Type => ModifierTypes.Harmful;

    public static OptionItem CanBeOnCrew;
    public static OptionItem CanBeOnNeutral;

    public void SetupCustomOption()
    {
        SetupAdtRoleOptions(Id, CustomRoles.Mundane, canSetNum: true, tab: TabGroup.Modifiers);
        CanBeOnCrew = BooleanOptionItem.Create(Id + 11, "CrewCanBeMundane", true, TabGroup.Modifiers, false).SetParent(Options.CustomRoleSpawnChances[CustomRoles.Mundane]);
        CanBeOnNeutral = BooleanOptionItem.Create(Id + 12, "NeutralCanBeMundane", true, TabGroup.Modifiers, false).SetParent(Options.CustomRoleSpawnChances[CustomRoles.Mundane]);
    }
    public void Init()
    { }
    public void Add(byte playerId, bool gameIsLoading = true)
    { }
    public void Remove(byte playerId)
    { }

    public static bool OnGuess(PlayerControl pc)
    {
        if (pc == null || !pc.Is(CustomRoles.Mundane)) return true;

        return pc.GetPlayerTaskState().IsTaskFinished;
    }
}
