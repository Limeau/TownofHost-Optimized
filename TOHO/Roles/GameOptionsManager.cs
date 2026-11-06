using System.Collections.Generic;
using AmongUs.GameOptions;
using Hazel;
using InnerNet;

public static class CustomGameOptionsManager
{
    public static Dictionary<PlayerControl, IGameOptions> PlayerOptions = [];
    
    public static void Set(this PlayerControl player)
    {
        PlayerOptions[player] = new NormalGameOptionsV12(new UnityLogger().Cast<ILogger>()).Cast<IGameOptions>();
    }

    public static void SetEngineerCooldown(this PlayerControl player, float cooldown)
    {
        if (player == null || player.Data == null || player.Data.Disconnected) return;

        if (!PlayerOptions.ContainsKey(player)) PlayerOptions[player] = new NormalGameOptionsV12(new UnityLogger().Cast<ILogger>()).Cast<IGameOptions>();
        
        var options = PlayerOptions[player];

        options.SetFloat(FloatOptionNames.EngineerCooldown, cooldown);
        
        PlayerOptions[player] = options;
        
        foreach (var component in GameManager.Instance.LogicComponents.GetEnumerator()._list)
        {
            LogicOptions logicOptions = component.TryCast<LogicOptions>();
            if (logicOptions != null) logicOptions.SetGameOptions(options);
        }
    }
}