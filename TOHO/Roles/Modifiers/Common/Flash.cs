using AmongUs.GameOptions;
using static TOHO.Options;

namespace TOHO.Roles.Modifiers.Common;

public class Flash : IModifier
{
    public CustomRoles Role => CustomRoles.Flash;
    private const int Id = 26100;
    public ModifierTypes Type => ModifierTypes.Helpful;

    private static OptionItem OptionSpeed;

    public void SetupCustomOption()
    {
        SetupAdtRoleOptions(Id, CustomRoles.Flash, canSetNum: true, tab: TabGroup.Modifiers, teamSpawnOptions: true);
        OptionSpeed = FloatOptionItem.Create(Id + 10, "FlashSpeed", new(0.25f, 5f, 0.25f), 2.5f, TabGroup.Modifiers, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Flash])
            .SetValueFormat(OptionFormat.Multiplier);
    }
    public void Init()
    { }
    public void Add(byte playerId, bool gameIsLoading = true)
    {
        Main.AllPlayerSpeed[playerId] = OptionSpeed.GetFloat();
    }
    public void Remove(byte playerId)
    {
        Main.AllPlayerSpeed[playerId] = Main.RealOptionsData.GetFloat(FloatOptionNames.PlayerSpeedMod);
        playerId.GetPlayer()?.MarkDirtySettings();
    }
    public static void SetSpeed(byte playerId)
    {
        Main.AllPlayerSpeed[playerId] = OptionSpeed.GetFloat();
    }
}
