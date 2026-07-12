using static TOHO.Options;

namespace TOHO.Roles.Modifiers.Common;

public class Rusher : IModifier
{
    public CustomRoles Role => CustomRoles.Rusher;
    private const int Id = 45400;
    public ModifierTypes Type => ModifierTypes.Helpful;
    public static OptionItem RusherSpeedIncrease;
    public void SetupCustomOption()
    {
        SetupAdtRoleOptions(Id, CustomRoles.Rusher, canSetNum: true, teamSpawnOptions: true);
        RusherSpeedIncrease = FloatOptionItem.Create(Id + 10, "RusherSpeedIncrease", (0.1f, 1f, 0.1f), 0.3f, TabGroup.Modifiers, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Rusher])
            .SetValueFormat(OptionFormat.Multiplier);
    }
    public void Init()
    { }
    public void Add(byte playerId, bool gameIsLoading = true)
    { }
    public void Remove(byte playerId)
    { }

    public static void OnTaskCompleteOrMurderPlayer(PlayerControl player)
    {
        Main.AllPlayerSpeed[player.PlayerId] += RusherSpeedIncrease.GetFloat() * Main.AllPlayerSpeed[player.PlayerId];
        player.MarkDirtySettings();
    }
}