using static TOHO.Options;

namespace TOHO.Roles.Modifiers.Common;

public class Chronos : IModifier
{
    public CustomRoles Role => CustomRoles.Chronos;
    private const int Id = 45700;
    public ModifierTypes Type => ModifierTypes.Mixed;

    public static OptionItem ExtendedMeetingTime;
    public static OptionItem ShowModifierPresence;

    public void SetupCustomOption()
    {
        SetupAdtRoleOptions(Id, CustomRoles.Chronos, canSetNum: true, teamSpawnOptions: true);
        ExtendedMeetingTime = IntegerOptionItem.Create(Id + 10, "ChronosExtendedMeetingTime", new(15, 40, 1), 20, TabGroup.Modifiers, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Chronos])
            .SetValueFormat(OptionFormat.Seconds);
        ShowModifierPresence = BooleanOptionItem.Create(Id + 11, "ChronosShowModifierPresence", true, TabGroup.Modifiers, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Chronos]);
    }
    public void Init()
    { }
    public void Add(byte playerId, bool gameIsLoading = true)
    { }
    public void Remove(byte playerId)
    { }

    // The reporter is treated as "you called the meeting" - rushed trial.
    public static bool IsReporterChronos(PlayerControl reporter) => reporter != null && reporter.Is(CustomRoles.Chronos);

    // Every other alive Chronos holder drags the deliberation out by their configured amount.
    public static int TotalExtendedMeetingTime(PlayerControl reporter)
    {
        int count = 0;
        foreach (var pc in Main.AllAlivePlayerControls)
        {
            if (pc.Is(CustomRoles.Chronos) && (reporter == null || pc.PlayerId != reporter.PlayerId))
                count++;
        }
        return count * ExtendedMeetingTime.GetInt();
    }

    // Passive ping: a subtle clock hints Chronos is in play without revealing the exact effect.
    public static string GetSuffix(PlayerControl target)
    {
        if (!ShowModifierPresence.GetBool()) return string.Empty;
        if (target == null || !target.Is(CustomRoles.Chronos)) return string.Empty;

        return " ⊕";
    }
}