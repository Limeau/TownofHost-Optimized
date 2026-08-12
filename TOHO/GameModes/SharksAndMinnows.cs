using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;
using static TOHO.Options;
using static TOHO.Translator;

namespace TOHO;
internal static class SharksAndMinnows
{
    public static OptionItem ShowChatInGame;
    public static OptionItem AmountOfRounds;
    public static OptionItem RoundTime;
    public static OptionItem NumSharks;
    public static int RoundsLeft;
    public static float RemainingTime;

    public static HashSet<byte> AlivePlayers = [];
    public static Dictionary<PlayerControl, string> Reasons = [];

    public static void SetupCustomOption()
    {
        ShowChatInGame = BooleanOptionItem.Create(70_226_02, "ShowChatInGame", false, TabGroup.ModSettings, false)
            .SetGameMode(CustomGameMode.SharksAndMinnows);
        AmountOfRounds = IntegerOptionItem
            .Create(70_226_03, "AmountOfRoundsSAM", new(2, 10, 1), 5, TabGroup.ModSettings, false)
            .SetGameMode(CustomGameMode.SharksAndMinnows); 
        RoundTime = IntegerOptionItem.Create(70_226_04, "RoundTimeSAM", new(5, 30, 1), 15, TabGroup.ModSettings, false)
            .SetGameMode (CustomGameMode.SharksAndMinnows) 
            .SetValueFormat(OptionFormat.Seconds);
        NumSharks = IntegerOptionItem.Create(70_226_05, "NumSharks", new(1, 5, 1), 2, TabGroup.ModSettings, false)
            .SetGameMode (CustomGameMode.SharksAndMinnows);
    }

    public static void Init()
    {
        if (CurrentGameMode != CustomGameMode.SharksAndMinnows) return;

        AlivePlayers = [];
        Reasons = [];
    }

    public static bool WAIT;
    
    public static void SetData()
    {
        if (Options.CurrentGameMode != CustomGameMode.SharksAndMinnows) return;
        
        WAIT = true;
        RoundsLeft = AmountOfRounds.GetInt();
        RemainingTime = RoundTime.GetFloat();
        foreach (var player in Main.AllAlivePlayerControls)
        {
            FixedUpdateInGameModeSAMPatch.PlayerCrossState[player] = CrossStates.NotCrossed;
            AlivePlayers.Add(player.PlayerId);
            player.RpcTeleport(GetMainRoom(Utils.GetActiveMapName()));

            var tmpspeed = Main.AllPlayerSpeed[player.PlayerId];
            
            if (player.Is(CustomRoles.Shark))
            {
                Main.AllPlayerSpeed[player.PlayerId] = 0;
                player.MarkDirtySettings();
            }
            
            player.Notify("Get ready!", time: 18f);

            _ = new LateTask(() =>
            {
                Main.AllPlayerSpeed[player.PlayerId] = tmpspeed;
                WAIT = false;
                player.MarkDirtySettings();
            }, 18f, "Start SAM");
        }
    }

    public static Dictionary<byte, CustomRoles> SetRoles()
    {
        Dictionary<byte, CustomRoles> finalRoles = [];
        List<PlayerControl> AllPlayers = Main.AllPlayerControls.ToList();

        if (Main.EnableGM.Value)
        {
            finalRoles[PlayerControl.LocalPlayer.PlayerId] = CustomRoles.GM;
            Main.PlayerStates[PlayerControl.LocalPlayer.PlayerId].MainRole = CustomRoles.GM;//might cause bugs
            AllPlayers.Remove(PlayerControl.LocalPlayer);
        }
        foreach (byte spectator in ChatCommands.Spectators)
        {
            finalRoles.AddRange(ChatCommands.Spectators.ToDictionary(x => x, _ => CustomRoles.GM));
            Main.PlayerStates[spectator].MainRole = CustomRoles.GM;
            AllPlayers.RemoveAll(x => ChatCommands.Spectators.Contains(x.PlayerId));
        }

        AllPlayers.Shuffle();
        int optImpNum = NumSharks.GetInt();
        foreach (PlayerControl pc in AllPlayers)
        {
            if (pc == null) continue; 
            if (optImpNum > 0)
            {
                finalRoles[pc.PlayerId] = CustomRoles.Shark;
                Main.PlayerStates[pc.PlayerId].MainRole = CustomRoles.Shark;
                pc.RpcSetCustomRole(CustomRoles.Shark);
                pc.RpcChangeRoleBasis(CustomRoles.Shark);
                optImpNum--;
            }
            else
            {
                finalRoles[pc.PlayerId] = CustomRoles.Minnow;
                Main.PlayerStates[pc.PlayerId].MainRole = CustomRoles.Minnow;
                pc.RpcSetCustomRole(CustomRoles.Minnow);
                pc.RpcChangeRoleBasis(CustomRoles.Minnow);
            }
            Logger.Msg($"set role for {pc.PlayerId}: {finalRoles[pc.PlayerId]}", "SetRoles");
        }        
        return finalRoles;
    }

    public static bool IsInsideMainRoom(this PlayerControl player)
    {
        if (Utils.GetActiveMapName() is MapNames.Skeld && player.GetPlainShipRoom().RoomId == SystemTypes.Cafeteria) return true;
        if (Utils.GetActiveMapName() is MapNames.MiraHQ && player.GetPlainShipRoom().RoomId == SystemTypes.Cafeteria) return true;
        if (Utils.GetActiveMapName() is MapNames.Polus && player.GetPlainShipRoom().RoomId == SystemTypes.Office) return true;
        if (Utils.GetActiveMapName() is MapNames.Airship && player.GetPlainShipRoom().RoomId == SystemTypes.Engine) return true;
        if (Utils.GetActiveMapName() is MapNames.Fungle && player.GetPlainShipRoom().RoomId == SystemTypes.Highlands) return true;
        if (Utils.GetActiveMapName() is MapNames.Dleks && player.GetPlainShipRoom().RoomId == SystemTypes.Cafeteria) return true;
        return false;
    }
    
    public static Vector2 GetMainRoom(MapNames map)
    {
        if (map is MapNames.Skeld) return new Vector2(-1.0f, 3.0f);
        if (map is MapNames.MiraHQ) return new Vector2(25.5f, 2.0f);
        if (map is MapNames.Polus) return new Vector2(26.0f, -17.0f);
        if (map is MapNames.Airship) return new Vector2(-0.7f, -1.0f);
        if (map is MapNames.Fungle) return new Vector2(-15.6f, -1.8f);
        if (map is MapNames.Dleks) return new Vector2(1.0f, 3.0f);
        return new Vector2(0f, 0f); // no LI support
    }
    
    public static bool OnMurder(PlayerControl killer, PlayerControl target)
    {
        if (target.IsInsideMainRoom()) return true;
        else return false;
    }
    
    public static string GetNotifyText(byte playerId)
    {
        return GetHudText();
    }
    public static string GetHudText()
    {
        return $"\nRounds left: {RoundsLeft}\nTime left this round: {RemainingTime}";
    }
    
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
    class FixedUpdateInGameModeSAMPatch
    {
        private static long LastFixedUpdate;
        public static Dictionary<PlayerControl, CrossStates> PlayerCrossState = [];
        public static void Postfix()
        {
            if (!GameStates.IsInTask || Options.CurrentGameMode != CustomGameMode.SharksAndMinnows) return;

            var now = Utils.GetTimeStamp();

            if (LastFixedUpdate == now) return;
            LastFixedUpdate = now;
            
            if (!AmongUsClient.Instance.AmHost || WAIT) return;

            RemainingTime--;
            
            bool allCrossed = true;
            foreach (var player in Main.AllAlivePlayerControls)
            {
                if (!player.IsAlive()) continue;
                
                if (player.Is(CustomRoles.Shark) && !player.IsInsideMainRoom()) player.RpcTeleport(GetMainRoom(Utils.GetActiveMapName()));
                
                if (player.Is(CustomRoles.Minnow))
                {
                    if (player.IsInsideMainRoom())
                    {
                        if (PlayerCrossState[player] == CrossStates.NotCrossed)
                            PlayerCrossState[player] = CrossStates.IsCrossing;
                    }
                    else
                    {
                        if (PlayerCrossState[player] == CrossStates.IsCrossing)
                            PlayerCrossState[player] = CrossStates.HasCrossed;
                    }
                    
                    if (PlayerCrossState[player] != CrossStates.HasCrossed)
                    {
                        allCrossed = false;
                        if (RemainingTime <= 0)
                        {
                            player.RpcMurderPlayer(player);
                        }
                    }
                }
            }

            if (allCrossed || RemainingTime <= 0)
            {
                RoundsLeft--;
                RemainingTime = RoundTime.GetInt();
                foreach (var player in Main.AllAlivePlayerControls)
                {
                    PlayerCrossState[player] = CrossStates.NotCrossed;
                }
            }
        }
    }

    enum CrossStates
    {
        NotCrossed,
        IsCrossing,
        HasCrossed,
    }
}