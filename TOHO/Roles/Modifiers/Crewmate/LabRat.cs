using System.Collections.Generic;
using System.Linq;
using static TOHO.Options;

namespace TOHO.Roles.Modifiers.Crewmate;

public class LabRat : IModifier
{
    public CustomRoles Role => CustomRoles.LabRat;
    private const int Id = 41700;
    public ModifierTypes Type => ModifierTypes.Helpful;

    public static bool IsEnable = false;
    
    public static OptionItem CanBeEngineer;
    public static OptionItem CanBeScientist;
    public static OptionItem CanBeTracker;
    public static OptionItem CanBeNoisemaker;
    public static OptionItem CanBeDetective;

    public void SetupCustomOption()
    {
        Options.SetupAdtRoleOptions(Id, CustomRoles.LabRat, canSetNum: true);
        CanBeEngineer = BooleanOptionItem.Create(Id + 10, "CanBeEngineer417", true, TabGroup.Modifiers, false).SetParent(CustomRoleSpawnChances[CustomRoles.LabRat]);
        CanBeScientist = BooleanOptionItem.Create(Id + 11, "CanBeScientist417", true, TabGroup.Modifiers, false).SetParent(CustomRoleSpawnChances[CustomRoles.LabRat]);
        CanBeTracker = BooleanOptionItem.Create(Id + 12, "CanBeTracker417", true, TabGroup.Modifiers, false).SetParent(CustomRoleSpawnChances[CustomRoles.LabRat]);
        CanBeNoisemaker = BooleanOptionItem.Create(Id + 13, "CanBeNoisemaker417", true, TabGroup.Modifiers, false).SetParent(CustomRoleSpawnChances[CustomRoles.LabRat]);
        CanBeDetective = BooleanOptionItem.Create(Id + 14, "CanBeDetective417", true, TabGroup.Modifiers, false).SetParent(CustomRoleSpawnChances[CustomRoles.LabRat]);
    }
    public void Init()
    {        
        IsEnable = false;
    }

    public void Add(byte playerId, bool gameIsLoading = true)
    {
        IsEnable = true;
    }
    public void Remove(byte playerId)
    { }

    public static void AfterMeetingTasks()
    {
        List<CustomRoles> roles = [];
        
        if (CanBeEngineer.GetBool()) roles.Add(CustomRoles.EngineerTOHO);
        if (CanBeScientist.GetBool()) roles.Add(CustomRoles.ScientistTOHO);
        if (CanBeTracker.GetBool()) roles.Add(CustomRoles.TrackerTOHO);
        if (CanBeNoisemaker.GetBool()) roles.Add(CustomRoles.NoisemakerTOHO);
        if (CanBeDetective.GetBool()) roles.Add(CustomRoles.DetectiveTOHO);
        
        foreach (var player in Main.AllAlivePlayerControls.Where(x => x.Is(CustomRoles.LabRat)))
        {
            if (!roles.Any()) player.RpcChangeRoleBasis(CustomRoles.CrewmateTOHO);
            else player.RpcChangeRoleBasis(roles.RandomElement());
            roles.Clear();
        }
    }
}
