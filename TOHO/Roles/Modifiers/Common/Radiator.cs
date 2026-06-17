using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;

namespace TOHO.Roles.Modifiers.Common;

public class Radiator : IModifier
{
    public CustomRoles Role => CustomRoles.Radiator;
    private const int Id = 43800;
    public ModifierTypes Type => ModifierTypes.Mixed;
    public static bool IsEnable = false;

    private static OptionItem AmountNeeded;
    private static OptionItem Radius;
    
    private static long LastFixedUpdate;
    private static readonly Dictionary<PlayerControl, int> Points = [];

    public void SetupCustomOption()
    {
        Options.SetupAdtRoleOptions(Id, CustomRoles.Radiator, canSetNum: true, tab: TabGroup.Modifiers, teamSpawnOptions: true);
        AmountNeeded = FloatOptionItem.Create(Id + 10, "AmountNeeded438", new(1f, 60f, 1f), 30f, TabGroup.Modifiers, false).SetParent(Options.CustomRoleSpawnChances[CustomRoles.Radiator])
             .SetValueFormat(OptionFormat.Times);
        Radius = FloatOptionItem.Create(Id + 11, "Radius438", new(1f, 3f, 0.5f), 1.5f, TabGroup.Modifiers, false).SetParent(Options.CustomRoleSpawnChances[CustomRoles.Radiator])
             .SetValueFormat(OptionFormat.Multiplier);
    }

    public void Init()
    { }

    public void Add(byte playerId, bool gameIsLoading = true)
    {
        foreach (var plr in Main.AllPlayerControls)
        {
            Points[plr] = 0;
        }
    }
    public void Remove(byte playerId)
    { }

    public void OnFixedUpdate(PlayerControl radiator)
    {
        if (!radiator.Is(CustomRoles.Radiator)) return;
        if (!radiator.IsAlive() && radiator != null) return;
        if (GameStates.IsMeeting) return;
        var now = Utils.GetTimeStamp();
        if (LastFixedUpdate == now) return;
        LastFixedUpdate = now;    
        
        foreach (var plr in Main.AllAlivePlayerControls) 
        {
           if (Utils.GetDistance(plr.transform.position, radiator.transform.position) < 2f && plr != radiator)
           {
               Points[plr]++;
               if (Points[plr] >= AmountNeeded.GetFloat() * 2)
               {
                   plr.SetDeathReason(PlayerState.DeathReason.Enflamed);
                   plr.RpcMurderPlayer(plr);
                   Points[plr] = 0;
               }
           }
           else if (Points[plr] > 0)
           {
               Points[plr]--;
           }
        }
    }
}
