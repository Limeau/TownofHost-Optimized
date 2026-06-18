using AmongUs.GameOptions;
using static TOHO.Options;

namespace TOHO.Roles.Modifiers.Common;

public class Snooze : IModifier
{
    public CustomRoles Role => CustomRoles.Snooze;
    private const int Id = 44100;
    public ModifierTypes Type => ModifierTypes.Harmful;

    private static OptionItem SnoozeChance;
    private static OptionItem SnoozeCooldown;

    private static bool Snoozing;
    
    public void SetupCustomOption()
    {
        SetupAdtRoleOptions(Id, CustomRoles.Snooze, canSetNum: false, tab: TabGroup.Modifiers, teamSpawnOptions: true);
        SnoozeChance = IntegerOptionItem.Create(Id + 10, "SnoozeChance", new(0, 100, 5), 50, TabGroup.Modifiers, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Snooze])
            .SetValueFormat(OptionFormat.Percent);
        SnoozeCooldown = FloatOptionItem.Create(Id + 11, "SnoozeCooldown", new(1f, 20f, 1f), 10f, TabGroup.Modifiers, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Snooze])
            .SetValueFormat(OptionFormat.Seconds);
    }
    public void Init()
    { }

    public void Add(byte playerId, bool gameIsLoading = true)
    {
        Snoozing = false;
    }
    public void Remove(byte playerId)
    { }

    public static bool ReportDeadBody(PlayerControl sleepy)
    {
        if (Snoozing) return false;
        
        if (IRandom.Instance.Next(100) < SnoozeChance.GetInt())
        {
            sleepy.Notify("Zzzz.....");
            Snoozing = true;
            _ = new LateTask(() =>
            {
                Snoozing = false;
            }, SnoozeCooldown.GetFloat(), "Snooze cooldown");
            return false;
        }
        
        return true;
    }
}