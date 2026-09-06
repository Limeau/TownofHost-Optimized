using System.Collections.Generic;
using System.Linq;
using static TOHO.Options;
using static TOHO.Utils;
using static TOHO.Translator;

namespace TOHO.Modules;

internal class PowerUpManager
{
    public static long LastFixedUpdate;
    public static Dictionary<PlayerControl, PowerUp> UnclaimedPowerUps = [];
    public static Dictionary<PlayerControl, PowerUp> ClaimedPowerUps = [];
    public static void Init()
    {
        UnclaimedPowerUps.Clear();
        ClaimedPowerUps.Clear();
        if (!EnablePowerUps.GetBool()) return;
        foreach (var player in Main.AllAlivePlayerControls)
        {
            player.RpcSetPet("pet_Crewmate");
            UnclaimedPowerUps[player] = PowerUp.Create(PowerUpType.Null);
            ClaimedPowerUps[player] = PowerUp.Create(PowerUpType.Null);
        }

        LastFixedUpdate = GetTimeStamp();
    }
    public static void OnFixedUpdate()
    {
        if (!EnablePowerUps.GetBool()) return;
        var now = GetTimeStamp();
        if (CurrentGameMode != CustomGameMode.Standard) return;
        if (!GameStates.IsInGame || GameStates.IsMeeting) return;
        if (LastFixedUpdate == now) return;
        LastFixedUpdate = now;
    }
}

public class PowerUp
{
    public PowerUpType PowerUpType;
    public static PowerUp Create(PowerUpType powerUpType)
    {
        var powerUp = new PowerUp();
        powerUp.PowerUpType = powerUpType;
        return powerUp;
    }

    public static void OnPet(PlayerControl pc)
    {
        if (!PowerUpManager.ClaimedPowerUps.ContainsKey(pc)) return;
        switch (PowerUpManager.ClaimedPowerUps[pc].PowerUpType)
        {
            case PowerUpType.Null:
                pc.Notify("You do not have a Power-Up to use.");
                break;
        }
    }
}

public enum PowerUpType
{
    Null,
}