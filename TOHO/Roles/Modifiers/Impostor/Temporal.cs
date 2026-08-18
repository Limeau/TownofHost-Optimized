using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using Hazel;
using InnerNet;
using static TOHO.Options;

namespace TOHO.Roles.Modifiers.Impostor;

public class Temporal : IModifier
{
    public CustomRoles Role => CustomRoles.Temporal;
    private const int Id = 46300;
    public ModifierTypes Type => ModifierTypes.Impostor;

    private static OptionItem TemporalIncreaseTime;

    public static IGameOptions BasedGameOptions => GameStates.IsNormalGame ? Main.RealOptionsData.Restore(new NormalGameOptionsV11(new UnityLogger().Cast<ILogger>()).Cast<IGameOptions>()) : Main.RealOptionsData.Restore(new HideNSeekGameOptionsV11(new UnityLogger().Cast<ILogger>()).Cast<IGameOptions>());
    
    public void SetupCustomOption()
    {
        SetupAdtRoleOptions(Id, CustomRoles.Temporal, canSetNum: true, tab: TabGroup.Modifiers);
        TemporalIncreaseTime = IntegerOptionItem.Create(Id + 3, "TemporalIncreaseTime", new(1, 10, 1), 5, TabGroup.Modifiers, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Temporal])
            .SetValueFormat(OptionFormat.Seconds);
    }

    public void Init()
    { }
    public void Add(byte playerId, bool gameIsLoading = true)
    {
        Increased = BasedGameOptions.GetInt(Int32OptionNames.EmergencyCooldown);
    }
    public void Remove(byte playerId)
    { }

    public static int Increased = 0;
    
    public static void SetGameOptions(IGameOptions opt)
    {
        opt.SetInt(Int32OptionNames.EmergencyCooldown, Increased);
    }
    
    public static void OnMurderPlayer(PlayerControl killer)
    {
        Increased += TemporalIncreaseTime.GetInt();
    }
}
