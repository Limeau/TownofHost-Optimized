using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;
using static TOHO.Options;
using static TOHO.Translator;

namespace TOHO;
internal static class BeanTrials
{
    public static OptionItem ShowChatInGame;
    public static OptionItem TimeBeforeStart;
    public static OptionItem TimeBeforeDraw;
    public static OptionItem PointsToWin;
    public static int RoundTime;

    public static Dictionary<byte, int> Scores = [];
    public static Dictionary<byte, byte> Choices = [];
    public static PlayerControl Player1;
    public static PlayerControl Player2;
    public static bool KillStage = false;

    public static void SetupCustomOption()
    {
        ShowChatInGame = BooleanOptionItem.Create(69_226_02, "ShowChatInGame", false, TabGroup.ModSettings, false)
            .SetGameMode(CustomGameMode.BeanTrials);
        TimeBeforeStart = IntegerOptionItem.Create(69_226_03, "TimeBeforeStartBR", new(3, 20, 1), 10, TabGroup.ModSettings, false)
            .SetGameMode (CustomGameMode.BeanTrials) 
            .SetValueFormat(OptionFormat.Seconds);
        TimeBeforeDraw = IntegerOptionItem.Create(69_226_04, "TimeBeforeDrawBR", new(20, 90, 1), 40, TabGroup.ModSettings, false)
            .SetGameMode (CustomGameMode.BeanTrials) 
            .SetValueFormat(OptionFormat.Seconds);
        PointsToWin = IntegerOptionItem.Create(69_226_05, "PointsToWinBR", new(2, 20, 1), 5, TabGroup.ModSettings, false)
            .SetGameMode(CustomGameMode.BeanTrials);
    }

    public static void Init()
    {
        if (CurrentGameMode != CustomGameMode.BeanTrials) return;
    }

    public static void SetData()
    {
        RoundTime = TimeBeforeDraw.GetInt() + 8;
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
            finalRoles[pc.PlayerId] = CustomRoles.Spectator; 
            Main.PlayerStates[pc.PlayerId].MainRole = CustomRoles.Spectator; 
            pc.RpcSetCustomRole(CustomRoles.Spectator); 
            pc.RpcChangeRoleBasis(CustomRoles.Spectator); 
            Logger.Msg($"set role for {pc.PlayerId}: {finalRoles[pc.PlayerId]}", "SetRoles");
        }
        return finalRoles;
    }

    public static string GetNotifyText(byte playerId)
    {
        return string.Format(GetString("BeanTrialsTimeRemain"), Player1.GetRealName(), Player2.GetRealName(), Scores[playerId].ToString(), RoundTime.ToString());
    }

    public static void OnCheckMurder(PlayerControl killer, PlayerControl target)
    {
        foreach (var player in Main.AllPlayerControls)
        {
            if (Choices[player.PlayerId] == target.PlayerId) Scores[player.PlayerId]++;
            RoundTime = 0;
        }
        killer.RpcMurderPlayer(target);
    }
    
    public static bool OnCheckProtect(PlayerControl angel, PlayerControl target)
    {
        Choices[angel.PlayerId]  = target.PlayerId;
        return true;
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
                RoundTime = KillStage ? TimeBeforeDraw.GetInt() : TimeBeforeStart.GetInt();
                if (KillStage)
                {
                    Choices.Clear();
                    foreach (var player in Main.AllPlayerControls)
                    {
                        player.RpcChangeRoleBasis(CustomRoles.Spectator);
                        player.RpcSetCustomRole(CustomRoles.Spectator);
                    }
                    List<PlayerControl> TributePlayers = [Main.AllPlayerControls.RandomElement(), Main.AllPlayerControls.RandomElement()];
                    while (TributePlayers[0] == TributePlayers[1])
                    {
                        TributePlayers[1] = Main.AllPlayerControls.RandomElement();
                    }
                    foreach (var player in TributePlayers)
                    {
                        player.RpcChangeRoleBasis(CustomRoles.Tribute);
                        player.RpcSetCustomRole(CustomRoles.Tribute);
                        Main.AllPlayerSpeed[player.PlayerId] = 0;
                    }

                    Choices[TributePlayers[0].PlayerId] = TributePlayers[1].PlayerId;
                    Choices[TributePlayers[1].PlayerId] = TributePlayers[0].PlayerId;
                    Player1 = TributePlayers[0];
                    Player2 = TributePlayers[1];
                    switch (Utils.GetActiveMapName())
                    {
                        case MapNames.Skeld:
                        {
                            Player1.RpcTeleport(new Vector2(16.5f, -4.8f)); //Navigation
                            Player2.RpcTeleport(new Vector2(-20.5f, -5.5f)); //Reactor(Skeld)
                            break;
                        }
                        case MapNames.MiraHQ:
                        {
                            Player1.RpcTeleport(new Vector2(24.0f, -2.0f)); //Balcony
                            Player2.RpcTeleport(new Vector2(17.8f, 23.0f)); //Greenhouse
                            break;
                        }
                        case MapNames.Polus:
                        {
                            Player1.RpcTeleport(new Vector2(2.3f, -24.0f)); //Boiler Room
                            Player2.RpcTeleport(new Vector2(36.5f, -7.5f)); //Laboratory
                            break;
                        }
                        case MapNames.Dleks:
                        {
                            Player1.RpcTeleport(new Vector2(-16.5f, -4.8f)); //Navigation(Dleks)
                            Player2.RpcTeleport(new Vector2(20.5f, -5.5f)); //Reactor(Dleks)
                            break;
                        }
                        case MapNames.Airship:
                        {
                            Player1.RpcTeleport(new Vector2(21.2f, -0.8f)); //Showers
                            Player2.RpcTeleport(new Vector2(-23.5f, -1.6f)); //Cockpit
                            break;
                        }
                        case MapNames.Fungle:
                        {
                            Player1.RpcTeleport(new Vector2(-15.6f, -1.8f)); //Splash Zone
                            Player2.RpcTeleport(new Vector2(22.3f, -7.0f)); //Reactor(Fungle)
                            break;
                        }
                    }
                }
                else
                {
                    foreach (var player in Main.AllAlivePlayerControls) Main.AllPlayerSpeed[player.PlayerId] = Main.LastAllPlayerSpeed[player.PlayerId];
                }
                KillStage = !KillStage;
            }
        }
    }
}