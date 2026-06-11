using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Rewired;
using static TOHO.Options;
using static TOHO.Translator;

namespace TOHO;
internal static class SimonSays
{
    public static OptionItem AFKTimer;
    public static OptionItem SimonWinTimer;
    
    public static bool IsSimonPicking = false;
    public static bool IsNosim = false;
    public static string CurrentTask = "";
    public static float AFKTime;
    public static float SimonTime;
    public static PlayerControl Simon;
    public static List<PlayerControl> Completed = [];
    
    public static void SetupCustomOption()
    {
        AFKTimer = IntegerOptionItem.Create(70_225_002, "SimonSaysAFKTimer", new(10, 60, 1), 30, TabGroup.ModSettings, false)
            .SetGameMode(CustomGameMode.SimonSays)
            .SetValueFormat(OptionFormat.Seconds);
        SimonWinTimer = IntegerOptionItem.Create(70_225_05, "SimonSaysSimonWinTimer", new(1, 60, 1), 15, TabGroup.ModSettings, false)
            .SetGameMode(CustomGameMode.SimonSays)
            .SetValueFormat(OptionFormat.Seconds);
    }

    /*
     * The game starts with instructions on how to play. Simon is randomly chosen among the players after 5 seconds.
     * Simon has AFK timer time to pick an action after being randomly chosen. If they do not, they die and the loop repeats.
     * When an action is chosen, if all players complete the action before SimonWinTimer, Simon dies.
     * If the SimonWinTimer expires, the last player who does not complete the action dies.
     * If the AFKTimer expires, all players who have not completed the action die.
     * The Actions include: Venting, Killing, Shapeshifting, Doing a task, Calling an emergency meeting
     * After each round all alive players and the Simon are reset to Engineer basis.
     * Venting can switch a player from Shapeshifter to Engineer base.
    */    
    
    public static void SetKillCooldown(PlayerControl player) => Main.AllPlayerKillCooldown[player.PlayerId] = 1f;
    
    public static void Init()
    {
    }

    public static void Loop()
    {
        IsNosim = false;
        AFKTime = AFKTimer.GetInt();
        SimonTime = SimonWinTimer.GetInt();
        CurrentTask = "";
        foreach (var player in Main.AllAlivePlayerControls.Where(x => !Completed.Contains(x)))
        {
            player.KillWithoutBody(player);
        }
        Completed.Clear();
        foreach (var player in Main.AllAlivePlayerControls)
        {
            player.RpcSetCustomRole(CustomRoles.Player);
            player.RpcChangeRoleBasis(CustomRoles.Player);
        }
        
        Simon.RpcSetCustomRole(CustomRoles.Player);
        Simon.RpcChangeRoleBasis(CustomRoles.Player);
        Simon = Main.AllAlivePlayerControls.RandomElement();
        Simon.RpcSetCustomRole(CustomRoles.Simon);
        Simon.RpcChangeRoleBasis(CustomRoles.Simon);
        Utils.SendMessage("Hello Simon! Here's how to pick an action for the players to do. Use /sim to command an action, or /nosim to try and trick them!\nThe actions available are:\n\n/sim kill\n/sim shift\n/sim vent\n/nosim kill\n/nosim shift\n/nosim vent", Simon.PlayerId);
        IsSimonPicking = true;
    }

    public static void ReceiveNosim(string arg)
    {
        if (arg.ToLower() == "vent")
        {
            CurrentTask = "NVent";
            IsSimonPicking = false;
            IsNosim = true;
        }
        else if (arg.ToLower() == "kill")
        {
            CurrentTask = "NKill";
            IsSimonPicking = false;           
            IsNosim = true;
        }
        else if (arg.ToLower() == "shift")
        {
            CurrentTask = "NShift";
            IsSimonPicking = false;
            IsNosim = true;
        }
        else
        {
            Utils.SendMessage("You can only use /nosim vent, /nosim kill, or /nosim shift.", Simon.PlayerId);
        }
    }
    
    public static void ReceiveSim(string arg)
    {
        if (arg.ToLower() == "vent")
        {
            CurrentTask = "Vent";
            IsSimonPicking = false;
        }
        else if (arg.ToLower() == "kill")
        {
            CurrentTask = "Kill";
            IsSimonPicking = false;
        }
        else if (arg.ToLower() == "shift")
        {
            CurrentTask = "Shift";
            IsSimonPicking = false;
        }
        else
        {
            Utils.SendMessage("You can only use /sim vent, /sim kill, or /sim shift.", Simon.PlayerId);
        }
    }
    
    public static void OnEnterVent(PlayerControl player)
    {
        if (CurrentTask == "Vent") Completed.Add(player);
        if (CurrentTask == "NVent")
        {
            player.KillWithoutBody(player);
            foreach (var player2 in Main.AllAlivePlayerControls)
            {
                Completed.Add(player2);
            }
            Completed.Add(Simon);
            Loop();
        }
    }
    public static bool OnMurderPlayer(PlayerControl killer)
    {
        killer.RpcGuardAndKill();
        if (CurrentTask == "Kill") Completed.Add(killer);
        if (CurrentTask == "NKill")
        {
            killer.KillWithoutBody(killer);
            foreach (var player2 in Main.AllAlivePlayerControls)
            {
                Completed.Add(player2);
            }
            Completed.Add(Simon);
            Loop();
        }
        return false;
    }
    public static bool OnCheckShapeshift(PlayerControl shapeshifter)
    {
        if (CurrentTask == "Shift") Completed.Add(shapeshifter);
        
        if (CurrentTask == "NShift")
        {
            shapeshifter.KillWithoutBody(shapeshifter);
            foreach (var player2 in Main.AllAlivePlayerControls)
            {
                Completed.Add(player2);
            }
            Completed.Add(Simon);
            Loop();
        }
        return false;
    }
    
    public static string GetProgressText(byte playerId)
    {
        var player = Utils.GetPlayerById(playerId);
        string progressText = "";
        if (CurrentTask == "Vent") progressText = "Simon says use the Vent";
        if (CurrentTask == "NVent") progressText = "Use the Vent";
        if (CurrentTask == "Kill") progressText = "Simon says Kill someone";
        if (CurrentTask == "NKill") progressText = "Kill someone";
        if (CurrentTask == "Shift") progressText = "Simon says use Shapeshift";
        if (CurrentTask == "NShift") progressText = "Use Shapeshift";
        if (CurrentTask == "") progressText = "Waiting for Simon to pick a task...";
        return progressText;
    }

    public static void SetData()
    {
        if (CurrentGameMode != CustomGameMode.SimonSays) return;
        AFKTime = AFKTimer.GetInt() + 13;
        SimonTime = SimonWinTimer.GetInt() + 13;
        new LateTask(() =>
        {
            foreach (var player in Main.AllAlivePlayerControls)
            {
                Completed.Add(player);
                player.Notify("Game starting soon...");
            }
        }, 8f, "Simon Says Initialize");new LateTask(() =>
        {
            Loop();
        }, 13f, "Simon Says Start");
    }
    
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
            finalRoles[pc.PlayerId] = CustomRoles.Player;
            Main.PlayerStates[pc.PlayerId].MainRole = CustomRoles.Player;
            pc.RpcSetCustomRole(CustomRoles.Player);
            pc.RpcChangeRoleBasis(CustomRoles.Player);
            Logger.Msg($"set role for {pc.PlayerId}: {finalRoles[pc.PlayerId]}", "SetRoles");
        }
        return finalRoles;
    }
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
    class UpdateInGameModeSimonPatch
    {
        private static long LastFixedUpdate;
        public static void Postfix()
        {
            var now = Utils.GetTimeStamp();
            
            if (!GameStates.IsInTask || !AmongUsClient.Instance.AmHost || Options.CurrentGameMode != CustomGameMode.SimonSays) return;
            
            if (LastFixedUpdate == now) return;
            LastFixedUpdate = now;
            
            if (!IsSimonPicking) SimonTime--;
            AFKTime--;

            if (AFKTime <= 0 && IsSimonPicking)
            {
                foreach (var player in Main.AllAlivePlayerControls)
                {
                    Completed.Add(player);
                    Loop();
                }
            }
            if (AFKTime <= 0 && !IsSimonPicking)
            {
                if (IsNosim)
                {
                    foreach (var player in Main.AllAlivePlayerControls)
                    {
                        Completed.Add(player);
                    }
                }
                else if (!Completed.Contains(Simon)) Completed.Add(Simon);
                Loop();
            }
            if (SimonTime <= 0 && !Completed.Contains(Simon) && !IsNosim)
            {
                Completed.Add(Simon);
            }

            if (SimonTime <= 0 && Completed.Count() >= Main.AllAlivePlayerControls.Count() - 1)
            {
                Loop();
            }
            if (SimonTime > 0 && Completed.Count() >= Main.AllAlivePlayerControls.Count())
            {
                Loop();
            }
        }
    }
}