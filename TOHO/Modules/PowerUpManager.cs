using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static TOHO.Options;
using static TOHO.Utils;
using static TOHO.Translator;

namespace TOHO.Modules;

internal class PowerUpManager
{
    public static long LastFixedUpdate;
    public static bool CanAndWillSpawn;
    public static Dictionary<PlayerControl, PowerUp> UnclaimedPowerUps = [];
    public static Dictionary<PlayerControl, PowerUp> ClaimedPowerUps = [];
    public static void Init()
    {
        UnclaimedPowerUps.Clear();
        ClaimedPowerUps.Clear();
        CanAndWillSpawn = false;
        if (!EnablePowerUps.GetBool()) return;
        foreach (var player in Main.AllPlayerControls)
        {
            player.RpcSetPet("pet_Crewmate");
            UnclaimedPowerUps[player] = PowerUp.Create(PowerUpType.Null, player);
            ClaimedPowerUps[player] = PowerUp.Create(PowerUpType.Null, player);
        }

        LastFixedUpdate = GetTimeStamp();
    }

    public static void AfterMeetingTasks()
    {
        CanAndWillSpawn = true;
    }
    
    public static void OnFixedUpdate()
    {
        if (!CanAndWillSpawn) return;
        if (!EnablePowerUps.GetBool()) return;
        if (CurrentGameMode != CustomGameMode.Standard) return;
        if (!GameStates.IsInGame || GameStates.IsMeeting) return;
        
        if (UnclaimedPowerUps.Any(x => x.Value.PowerUpType != PowerUpType.Null)) CheckPowerUpClaim();
        
        var now = GetTimeStamp();
        if (LastFixedUpdate == now) return;
        LastFixedUpdate = now;

        if (Main.AllAlivePlayerControls.Length <= 1) return;
        
        if (IRandom.Instance.Next(1, 100) <= PowerUpChance.GetInt()) SpawnPowerUp();
    }

    public static void SpawnPowerUp()
    {
        List<PlayerControl> activeList = Main.AllAlivePlayerControls.ToList();
        List<PlayerControl> inactiveList = [];
        foreach (var player in activeList)
        {
            if (UnclaimedPowerUps[player].PowerUpType != PowerUpType.Null || ClaimedPowerUps[player].PowerUpType != PowerUpType.Null) inactiveList.Add(player);
        }
        foreach (var item in inactiveList) activeList.Remove(item);
        
        List<PowerUpType> typeList = [];
        if (EnableSpeedBoost.GetBool()) typeList.Add(PowerUpType.SpeedBoost);
        if (EnableReduceCooldown.GetBool()) typeList.Add(PowerUpType.ReduceCooldown);
        if (EnableAbilityIncrease.GetBool()) typeList.Add(PowerUpType.AbilityIncrease);
        if (!typeList.Any()) return;
        var type = typeList.RandomElement();
        
        var pc = activeList.RandomElement();
        UnclaimedPowerUps[pc] = PowerUp.Create(type, pc);
        var powerUp = UnclaimedPowerUps[pc];
        TargetArrow.Add(pc.PlayerId, powerUp.Position.PlayerId);
        _ = new LateTask(() =>
        {
            if (UnclaimedPowerUps[pc] != powerUp) return;
            TargetArrow.RemoveAllTarget(pc.PlayerId);
            UnclaimedPowerUps[pc] = PowerUp.Create(PowerUpType.Null, pc);
            pc.Notify("You missed the power-up.");
        }, 10f);
    }
    public static void CheckPowerUpClaim()
    {
        foreach (var kvp in UnclaimedPowerUps.Where(x => x.Value.PowerUpType != PowerUpType.Null))
        {
            var pc = kvp.Key;
            var powerUp = kvp.Value;
            if (GetDistance(pc.GetTruePosition(), powerUp.Position.GetTruePosition()) <= 1f)
            {
                pc.Notify($"You have claimed: {GetString(powerUp.PowerUpType.ToString())}");
                ClaimedPowerUps[pc] = powerUp;
                UnclaimedPowerUps[pc] = PowerUp.Create(PowerUpType.Null, pc);
                TargetArrow.RemoveAllTarget(pc.PlayerId);
            }
            else
            {
                pc.Notify($"A power-up is waiting for you!");
            }
        }
    }
    public static string GetArrow(PlayerControl seer)
    {
        return Utils.ColorString(Utils.GetRoleColor(CustomRoles.Jackal), TargetArrow.GetArrows(seer));
    }
}

public class PowerUp
{
    public PowerUpType PowerUpType;
    public PlayerControl Position;
    public static PowerUp Create(PowerUpType powerUpType, PlayerControl pc)
    {
        var powerUp = new PowerUp();
        powerUp.PowerUpType = powerUpType;
        powerUp.Position = Main.AllAlivePlayerControls.Where(x => x != pc).RandomElement();
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
            case PowerUpType.SpeedBoost:
                pc.Notify("You have a temporary Speed Boost");
                var tmpSpeed = Main.AllPlayerSpeed[pc.PlayerId];
                Main.AllPlayerSpeed[pc.PlayerId] = tmpSpeed * 1.5f;
                pc.MarkDirtySettings();
                _ = new LateTask(() =>
                {
                    Main.AllPlayerSpeed[pc.PlayerId] = tmpSpeed;
                    pc.MarkDirtySettings();
                }, 10f);
                break;
            case PowerUpType.ReduceCooldown:
                pc.Notify("You have had your kill timer reduced by 5 seconds");
                pc.UpdateKillCooldown(-5f);
                break;
            case PowerUpType.AbilityIncrease:
                pc.Notify("You have gained 1 ability use");
                pc.RpcIncreaseAbilityUseLimitBy(1);
                break;
        }
        PowerUpManager.ClaimedPowerUps[pc] = Create(PowerUpType.Null, pc);
    }
}

public enum PowerUpType
{
    Null,
    SpeedBoost,
    ReduceCooldown,
    AbilityIncrease
}