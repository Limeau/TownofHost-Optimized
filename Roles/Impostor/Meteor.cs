using AmongUs.GameOptions;
using Hazel;
using System;
using System.Collections.Generic;
using UnityEngine;
using static TOHO.Options;
using static TOHO.Translator;
using static TOHO.Utils;
using TOHO.Modules;
using TOHO.Roles.Core;
using TOHO.Roles.Crewmate;

namespace TOHO.Roles.Impostor;

internal class Meteor : RoleBase
{
    //===========================SETUP================================\\
    public override CustomRoles Role => CustomRoles.Meteor;
    private const int Id = 37100;
    public override CustomRoles ThisRoleBase => CustomRoles.Impostor;
    public override Custom_RoleType ThisRoleType => Custom_RoleType.ImpostorKilling;
    //==================================================================\\

    private static OptionItem KillCooldown;
    private static OptionItem ExplosionDelay;
    private static OptionItem ExplosionRadius;
    private static OptionItem ImpostorsDieInExplosion;
    private static OptionItem TargetNotificationDelay;

    private static readonly Dictionary<byte, (byte targetId, long plantTime)> BombedPlayers = [];

    public override void SetupCustomOption()
    {
        SetupRoleOptions(Id, TabGroup.ImpostorRoles, CustomRoles.Meteor);
        KillCooldown = FloatOptionItem.Create(Id + 10, GeneralOption.KillCooldown, new(0f, 180f, 2.5f), 20f, TabGroup.ImpostorRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Meteor])
            .SetValueFormat(OptionFormat.Seconds);
        ExplosionDelay = FloatOptionItem.Create(Id + 11, "MeteorExplosionDelay", new(1f, 30f, 1f), 10f, TabGroup.ImpostorRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Meteor])
            .SetValueFormat(OptionFormat.Seconds);
        ExplosionRadius = FloatOptionItem.Create(Id + 12, "MeteorExplosionRadius", new(0.5f, 5f, 0.5f), 2f, TabGroup.ImpostorRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Meteor])
            .SetValueFormat(OptionFormat.Multiplier);
        ImpostorsDieInExplosion = BooleanOptionItem.Create(Id + 13, "MeteorImpostorsDieInExplosion", false, TabGroup.ImpostorRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Meteor]);
        TargetNotificationDelay = FloatOptionItem.Create(Id + 14, "MeteorTargetNotificationDelay", new(0f, 15f, 1f), 5f, TabGroup.ImpostorRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Meteor])
            .SetValueFormat(OptionFormat.Seconds);
    }

    public override void Init()
    {
        BombedPlayers.Clear();
    }

    public override void Add(byte playerId)
    {
        // Double Trigger
        var pc = GetPlayerById(playerId);
        pc.AddDoubleTrigger();
    }

   public override void SetKillCooldown(byte id) => KillCooldown.GetFloat();

    public override bool OnCheckMurderAsKiller(PlayerControl killer, PlayerControl target)
    {
        // Double click = normal kill
        if (killer.CheckDoubleTrigger(target, () => { }))
        {
            return true;
        }
       
        // Single click = plant bomb
        PlantBomb(killer, target);
        return false;
    }

    private static void PlantBomb(PlayerControl killer, PlayerControl target)
    {
        if (!AmongUsClient.Instance.AmHost) return;

        // Check if target is immune
        if (IsImmune(target))
        {
            killer.Notify(GetString("TargetIsImmune"));
            killer.SetKillCooldown();
            return;
        }

        BombedPlayers[killer.PlayerId] = (target.PlayerId, Utils.GetTimeStamp());
        SendRPC(killer.PlayerId, target.PlayerId, true);

        killer.Notify(string.Format(GetString("MeteorBombPlanted"), target.GetRealName(), ExplosionDelay.GetInt()), 3f);

        _ = new LateTask(() =>
        {
            if (BombedPlayers.ContainsKey(killer.PlayerId) && target.IsAlive())
            {
                target.Notify(GetString("MeteorBombPlantedOnYou"), 3f);
            }
        }, TargetNotificationDelay.GetFloat(), "Meteor Target Notification");

        killer.SetKillCooldown();

        killer.RpcGuardAndKill(target);

        Logger.Info($"{killer.GetNameWithRole()} planted bomb on {target.GetNameWithRole()}", "Meteor");
    }

    private static void TriggerExplosion(PlayerControl killer, PlayerControl bombTarget)
    {
        if (!AmongUsClient.Instance.AmHost) return;

        CustomSoundsManager.RPCPlayCustomSoundAll("Boom");

        _ = new Explosion(5f, 0.5f, bombTarget.GetCustomPosition());

        int additionalKills = 0;


        foreach (var target in Main.AllAlivePlayerControls)
        {
            if (target.PlayerId == bombTarget.PlayerId) continue;

            if (!target.IsAlive() || 
                IsImmune(target) || 
                Medic.IsProtected(target.PlayerId) || 
                target.inVent ||
                target.IsTransformedNeutralApocalypse())
                continue;

            if (target.Is(Custom_Team.Impostor) && !ImpostorsDieInExplosion.GetBool())
                continue;

            var distance = Vector2.Distance(bombTarget.transform.position, target.transform.position);
            if (distance > ExplosionRadius.GetFloat())
                continue;

            target.SetDeathReason(PlayerState.DeathReason.Bombed);
            target.RpcMurderPlayer(target);
            target.SetRealKiller(killer);
            additionalKills++;

            Logger.Info($"{target.GetNameWithRole()} killed in Meteor explosion", "Meteor");
        }

        bombTarget.SetDeathReason(PlayerState.DeathReason.Bombed);
        bombTarget.RpcMurderPlayer(bombTarget);
        bombTarget.SetRealKiller(killer);

        BombedPlayers.Remove(killer.PlayerId);
        SendRPC(killer.PlayerId, 0, false);

        if (additionalKills > 0)
        {
            killer.Notify(string.Format(GetString("MeteorExplosionKilled"), additionalKills), 3f);
        }
        else
        {
            killer.Notify(GetString("MeteorExplosionNoAdditionalKills"), 2f);
        }

        Logger.Info($"Meteor explosion triggered by {killer.GetNameWithRole()} - killed {additionalKills + 1} players", "Meteor");
    }

    private static void SendRPC(byte killerId, byte targetId, bool plant)
    {
        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, 
            (byte)CustomRPC.SyncRoleSkill, SendOption.Reliable, -1);
        writer.Write(killerId);
        writer.Write(plant);
        if (plant)
        {
            writer.Write(targetId);
        }
        AmongUsClient.Instance.FinishRpcImmediately(writer);
    }

    public static void ReceiveRPC(MessageReader reader)
    {
        byte killerId = reader.ReadByte();
        bool plant = reader.ReadBoolean();

        if (plant)
        {
            byte targetId = reader.ReadByte();
            BombedPlayers[killerId] = (targetId, Utils.GetTimeStamp());
        }
        else
        {
            BombedPlayers.Remove(killerId);
        }
    }

    public override void OnFixedUpdate(PlayerControl player, bool lowLoad, long nowTime, int timerLowLoad)
    {
        if (lowLoad) return;

        foreach (var (killerId, (targetId, plantTime)) in BombedPlayers.ToArray())
        {
            var killer = GetPlayerById(killerId);
            var target = GetPlayerById(targetId);

            if (killer == null || target == null || !target.IsAlive())
            {
                BombedPlayers.Remove(killerId);
                SendRPC(killerId, 0, false);
                continue;
            }

            if (plantTime + (long)ExplosionDelay.GetFloat() <= nowTime)
            {
                TriggerExplosion(killer, target);
            }
            else
            {
                // Notify target of remaining time in last 5 seconds
                var remainingTime = plantTime + (long)ExplosionDelay.GetFloat() - nowTime;
                if (remainingTime <= 5 && !target.IsModded())
                {
                    var timeSincePlant = nowTime - plantTime;
                    if (timeSincePlant >= TargetNotificationDelay.GetFloat())
                    {
                        target.Notify(string.Format(GetString("MeteorBombCountdown"), remainingTime), 1f, false);
                    }
                }
            }
        }
    }

    public override void OnReportDeadBody(PlayerControl reporter, NetworkedPlayerInfo target)
    {
        foreach (var killerId in BombedPlayers.Keys.ToArray())
        {
            BombedPlayers.Remove(killerId);
            SendRPC(killerId, 0, false);
        }
    }

    public override void OnPlayerExiled(PlayerControl player, NetworkedPlayerInfo exiled)
    {

        foreach (var (killerId, (targetId, _)) in BombedPlayers.ToArray())
        {
            if (exiled.PlayerId == killerId || exiled.PlayerId == targetId)
            {
                BombedPlayers.Remove(killerId);
                SendRPC(killerId, 0, false);
            }
        }
    }

    private static bool IsImmune(PlayerControl player)
    {
        return player.Is(CustomRoles.Solsticer) ||
               player.Is(CustomRoles.Necromancer) ||
               player.Is(CustomRoles.NiceMini) ||
               player.Is(CustomRoles.LazyGuy) ||
               player.IsTransformedNeutralApocalypse() ||
               player.Is(CustomRoles.PunchingBag) ||
               player.Is(CustomRoles.Jinx) ||
               player.Is(CustomRoles.GM);
    }

    public override string GetLowerText(PlayerControl seer, PlayerControl seen = null, bool isForMeeting = false , bool isForHud = false)
    {
        if (seer == null || !seer.IsAlive() || isForMeeting || !isForHud) return string.Empty;

        if (seer.Is(CustomRoles.Meteor))
        {
            if (BombedPlayers.TryGetValue(seer.PlayerId, out var bombData))
            {
                var target = GetPlayerById(bombData.targetId);
                if (target != null && target.IsAlive())
                {
                    var remainingTime = bombData.plantTime + (long)ExplosionDelay.GetFloat() - Utils.GetTimeStamp();
                    return string.Format(GetString("MeteorActiveBomb"), target.GetRealName(), Math.Max(0, remainingTime));
                }
            }
            return GetString("MeteorNoActiveBomb");
        }
        else
        {
            foreach (var (killerId, (targetId, plantTime)) in BombedPlayers)
            {
                if (targetId == seer.PlayerId)
                {
                    var timeSincePlant = Utils.GetTimeStamp() - plantTime;
                    if (timeSincePlant >= TargetNotificationDelay.GetFloat())
                    {
                        var remainingTime = plantTime + (long)ExplosionDelay.GetFloat() - Utils.GetTimeStamp();
                        return string.Format(GetString("MeteorBombOnYou"), Math.Max(0, remainingTime));
                    }
                    break;
                }
            }
        }

        return string.Empty;
    }

    public override void SetAbilityButtonText(HudManager hud, byte playerId)
    {
        hud.KillButton.OverrideText(GetString("MeteorKillButtonText"));
    }
}