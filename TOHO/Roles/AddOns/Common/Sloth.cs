using AmongUs.GameOptions;
using static TOHO.Options;

namespace TOHO.Roles.Modifiers.Common;

public class Sloth : IModifier
{
    public CustomRoles Role => CustomRoles.Sloth;
    private const int Id = 29700;
    public ModifierTypes Type => ModifierTypes.Harmful;

    private static OptionItem OptionSpeed;

    public void SetupCustomOption()
    {
        SetupAdtRoleOptions(Id, CustomRoles.Sloth, canSetNum: true, tab: TabGroup.Modifiers, teamSpawnOptions: true);
        OptionSpeed = FloatOptionItem.Create(Id + 10, "SlothSpeed", new(0.25f, 1f, 0.25f), 0.5f, TabGroup.Modifiers, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Sloth])
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
