using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using TOHO.Modules;
using UnityEngine;
using static NetworkedPlayerInfo;
using static TOHO.Options;
using static TOHO.Translator;

namespace TOHO;
internal static class KOTH
{
    public static OptionItem GameTime;
    public static OptionItem TimeBeforeSwitch;
    public static OptionItem ShowChatInGame;
    public static OptionItem PointsToWin;
    public static OptionItem KillCooldown;

    public static Dictionary<PlayerControl, int> Points = [];
    public static int RoundTime;
    public static int SwitchTime;
    public static PlayerControl HighestScorer;
    public static SystemTypes CurrentRoom;
    
    public static void SetupCustomOption()
    {
        GameTime = IntegerOptionItem.Create(69_225_001, "GameTime", new(30, 600, 10), 300, TabGroup.ModSettings, false)
            .SetGameMode(CustomGameMode.KOTH)
            .SetValueFormat(OptionFormat.Seconds)
            .SetHeader(true);
        TimeBeforeSwitch = IntegerOptionItem.Create(69_225_002, "TimeBeforeSwitch", new(10, 60, 1), 30, TabGroup.ModSettings, false)
            .SetGameMode(CustomGameMode.KOTH)
            .SetValueFormat(OptionFormat.Seconds);
        ShowChatInGame = BooleanOptionItem.Create(69_225_03, "ShowChatInGame", false, TabGroup.ModSettings, false)
            .SetGameMode(CustomGameMode.KOTH);
        PointsToWin = IntegerOptionItem.Create(69_225_04, "PointsToWin", new(50, 200, 5), 100, TabGroup.ModSettings, false)
            .SetGameMode(CustomGameMode.KOTH);
        KillCooldown = FloatOptionItem.Create(69_225_05, RoleBase.GeneralOption.KillCooldown, new(1f, 60f, 1f), 10f, TabGroup.ModSettings, false).SetGameMode(CustomGameMode.KOTH).SetValueFormat(OptionFormat.Seconds);
    }

    public static void Init()
    { }

    public static Dictionary<byte, CustomRoles> SetRoles()
    {
        Dictionary<byte, CustomRoles> finalRoles = [];
        var random = IRandom.Instance;
        List<PlayerControl> AllPlayers = Main.AllPlayerControls.Shuffle(random).ToList();

        if (Main.EnableGM.Value)
        {
            finalRoles[PlayerControl.LocalPlayer.PlayerId] = CustomRoles.GM;
            Main.PlayerStates[PlayerControl.LocalPlayer.PlayerId].MainRole = CustomRoles.GM;//might cause bugs
            AllPlayers.Remove(PlayerControl.LocalPlayer);
        }
        foreach (byte spectator in ChatCommands.Spectators)
        {
            finalRoles.AddRange(ChatCommands.Spectators.ToDictionary(x => x, _ => CustomRoles.GM));
            foreach (var specId in ChatCommands.Spectators)
            {
                Main.PlayerStates[specId].MainRole = CustomRoles.GM;
            }
            AllPlayers.RemoveAll(x => ChatCommands.Spectators.Contains(x.PlayerId));
        }

        foreach (PlayerControl pc in AllPlayers)
        {
            finalRoles[pc.PlayerId] = CustomRoles.KingOfTheHill;
            Main.PlayerStates[pc.PlayerId].MainRole = CustomRoles.KingOfTheHill;
            pc.RpcSetCustomRole(CustomRoles.KingOfTheHill);
            pc.RpcChangeRoleBasis(CustomRoles.KingOfTheHill);
            Logger.Msg($"set role for {pc.PlayerId}: {finalRoles[pc.PlayerId]}", "SetRoles");
        }
        return finalRoles;
    }

    public static void SetData()
    {
        if (CurrentGameMode != CustomGameMode.KOTH) return;

        RoundTime = GameTime.GetInt() + 8;
        SwitchTime = 8;
        var now = Utils.GetTimeStamp() + 8;

        foreach (var player in Main.AllAlivePlayerControls)
        {
            Main.AllPlayerKillCooldown[player.PlayerId] = KillCooldown.GetFloat();
            Points[player] = 0;
        }
    }

    public static bool OnMurder(PlayerControl killer, PlayerControl target)
    {
        target.RpcTeleport(target.GetClosestVent().transform.position);
        killer.RpcGuardAndKill();
        Points[killer] += 1;
        return false;
    }

    public static string GetProgressText(byte playerId)
    {
        var player = Utils.GetPlayerById(playerId);

        if (!Points.ContainsKey(player)) Points[player] = 0;
        
        string progressText = $"\n{GetString(CurrentRoom.ToString())} - <color={player.GetRoleColorCode()}>({Points[player]})</color>\n";
        return progressText;
    }

    public static string GetHudText()
    {
        return string.Format(GetString("GameModeTimeRemain"), RoundTime.ToString());
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
    class FixedUpdateInGameModeKothPatch
    {
        private static long LastFixedUpdate;
        public static void Postfix()
        {
            if (!GameStates.IsInTask || Options.CurrentGameMode != CustomGameMode.KOTH) return;

            var now = Utils.GetTimeStamp();

            if (LastFixedUpdate == now) return;
            LastFixedUpdate = now;

            RoundTime--;
            SwitchTime--;

            if (SwitchTime <= 0)
            {
                var shipRoom = ShipStatus.Instance.AllRooms;
                var currentMapRooms = shipRoom.Select(room => room.RoomId).ToList();
        
                var validRooms = SystemTypeHelpers.AllTypes
                    .Where(x => x != SystemTypes.HeliSabotage && currentMapRooms.Contains(x))
                    .ToList();
            
                if (validRooms.Count == 0)
                {
                    return;
                }

                CurrentRoom = validRooms[IRandom.Instance.Next(0, validRooms.Count)];

                SwitchTime = TimeBeforeSwitch.GetInt();
            }

            foreach (var player in Main.AllAlivePlayerControls)
            {
                if (HighestScorer == null)
                {
                    HighestScorer = player;
                }
                
                if (Points[HighestScorer] < Points[player])
                {
                    HighestScorer = player;
                }
                
                if (player.GetPlainShipRoom().RoomId == CurrentRoom)
                {
                    foreach (var player2 in Main.AllAlivePlayerControls.Where(x => x != player))
                    {
                        if (player2.GetPlainShipRoom().RoomId == CurrentRoom) return;
                    }

                    Points[player] += 1;
                }
            }
        }
    }
}