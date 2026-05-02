using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using static TOHO.Options;
using static TOHO.Translator;

namespace TOHO;
internal static class FourCorners
{
    public static OptionItem ShowChatInGame;
    public static OptionItem TimeBetweenRounds;
    public static int RoundTime;

    public static HashSet<byte> AlivePlayers = [];
    public static List<SystemTypes> ActiveRooms = [];
    public static Dictionary<PlayerControl, string> Reasons = [];

    public static void SetupCustomOption()
    {
        ShowChatInGame = BooleanOptionItem.Create(68_226_02, "ShowChatInGame", false, TabGroup.ModSettings, false)
            .SetGameMode(CustomGameMode.FourCorners);
        TimeBetweenRounds = IntegerOptionItem.Create(68_226_03, "TimeBetweenRoundsFC", new(5, 45, 1), 25, TabGroup.ModSettings, false)
            .SetGameMode (CustomGameMode.FourCorners) 
            .SetValueFormat(OptionFormat.Seconds);
    }

    public static void Init()
    {
        if (CurrentGameMode != CustomGameMode.FourCorners) return;

        AlivePlayers = [];
        ActiveRooms.Clear();
        Reasons = [];
    }

    public static void SetData()
    {
        RoundTime = TimeBetweenRounds.GetInt() + 8;
        foreach (var player in Main.AllAlivePlayerControls)
        {
            AlivePlayers.Add(player.PlayerId);
        }
        var validRooms = SystemTypeHelpers.AllTypes
            .Where(x => x != SystemTypes.HeliSabotage && ShipStatus.Instance.AllRooms.Select(room => room.RoomId).ToList().Contains(x))
            .ToList();
        ActiveRooms.Add(validRooms[IRandom.Instance.Next(0, validRooms.Count)]);
        ActiveRooms.Add(validRooms[IRandom.Instance.Next(0, validRooms.Count)]);
        ActiveRooms.Add(validRooms[IRandom.Instance.Next(0, validRooms.Count)]);
        ActiveRooms.Add(validRooms[IRandom.Instance.Next(0, validRooms.Count)]);
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

        foreach (PlayerControl pc in AllPlayers)
        {
            finalRoles[pc.PlayerId] = CustomRoles.FourCorners; 
            Main.PlayerStates[pc.PlayerId].MainRole = CustomRoles.FourCorners; 
            pc.RpcSetCustomRole(CustomRoles.FourCorners); 
            pc.RpcChangeRoleBasis(CustomRoles.FourCorners); 
            pc.SetName(" ");
            Logger.Msg($"set role for {pc.PlayerId}: {finalRoles[pc.PlayerId]}", "SetRoles");
        }
        return finalRoles;
    }

    public static string GetNotifyText(byte playerId)
    {
        var player = Utils.GetPlayerById(playerId);
        if (player.IsAlive()) return string.Format(GetString("FourCornersTimeRemain"), RoundTime.ToString(), GetString(ActiveRooms[0].ToString()), GetString(ActiveRooms[1].ToString()), GetString(ActiveRooms[2].ToString()), GetString(ActiveRooms[3].ToString()));
        else
        {
            if (Reasons[player] == "invalid") return string.Format(GetString("InvalidRoomFC"));
            if (Reasons[player] == "chosen") return string.Format(GetString("ChosenRoomFC"));
        }

        return string.Empty;
    }
    
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
    class FixedUpdateInGameModeUltimatePatch
    {
        private static long LastFixedUpdate;
        public static void Postfix()
        {
            if (!GameStates.IsInTask || Options.CurrentGameMode != CustomGameMode.FourCorners) return;

            var now = Utils.GetTimeStamp();

            if (LastFixedUpdate == now) return;
            LastFixedUpdate = now;

            foreach (var player in Main.AllPlayerControls) player.Notify(GetNotifyText(player.PlayerId));
            
            RoundTime--;
            if (RoundTime <= 0)
            {
                var roomToDestroy = ActiveRooms.RandomElement();
                foreach (var player in Main.AllAlivePlayerControls)
                {
                    if (player.GetPlainShipRoom() == null || !ActiveRooms.Contains(player.GetPlainShipRoom().RoomId))
                    {
                        if (player != null) Reasons[player] = "invalid";
                        player.KillWithoutBody(player);
                    }
                    else if (player.GetPlainShipRoom().RoomId == roomToDestroy)
                    {
                        if (player != null) Reasons[player] = "chosen";
                        player.KillWithoutBody(player);
                    }
                    player.KillFlash();
                }

                ActiveRooms.Clear();
                var validRooms = SystemTypeHelpers.AllTypes
                    .Where(x => x != SystemTypes.HeliSabotage && ShipStatus.Instance.AllRooms.Select(room => room.RoomId).ToList().Contains(x))
                    .ToList();
                
                var r1 = validRooms.RandomElement();
                validRooms.Remove(r1);
                var r2 = validRooms.RandomElement();
                validRooms.Remove(r2);
                var r3 = validRooms.RandomElement();
                validRooms.Remove(r3);
                var r4 = validRooms.RandomElement();
                validRooms.Remove(r4);
                
                ActiveRooms.Add(r1);
                ActiveRooms.Add(r2);
                ActiveRooms.Add(r3);
                ActiveRooms.Add(r4);
                
                RoundTime = TimeBetweenRounds.GetInt();
            }
        }
    }
}
