using System.Collections.Generic;
using System.Linq;
using static TOHO.Options;
using static TOHO.Utils;
using static TOHO.Translator;

namespace TOHO.Modules;
internal class AnomalyManager
{
    private static readonly List<string> which1 = [];
    public static long LastColorChange;
    public static Dictionary<byte, CustomRoles> FormerRoles = [];
    
    public static void Init()
    {
        LastColorChange = Utils.GetTimeStamp();
    }
    public static void AnomalyChance()
    {
        which1.Clear();
        var rand = IRandom.Instance;
        if (rand.Next(100) <= AnomalyMeetingPCT.GetInt())
        {
            if (Options.ClownFest.GetBool())
            {
                which1.Add("ClownFest");
            }
            if (Options.Retrial.GetBool())
            {
                which1.Add("Retrial");
            }
            if (Options.NewYear.GetBool())
            {
                which1.Add("NewYear");
            }
            if (Options.Holiday.GetBool())
            {
                which1.Add("Holiday");
            }
            if (Options.Shuffle.GetBool())
            {
                which1.Add("Shuffle");
            }
            if (Options.CrazyColors.GetBool())
            {
                which1.Add("CrazyColors");
            }
            if (Options.TruthPotion.GetBool())
            {
                which1.Add("TruthPotion");
            }
            var which2 = which1.RandomElement();
            switch (which2)
            {
                case "ClownFest":
                    AfterAnomaly();
                    ClownFest();
                    break;
                case "Retrial":
                    AfterAnomaly();
                    Retrial();
                    break;
                case "NewYear":
                    AfterAnomaly();
                    NewYear();
                    break;
                case "Holiday":
                    AfterAnomaly();
                    Holiday();
                    break;
                case "Shuffle":
                    AfterAnomaly();
                    Shuffle();
                    break;
                case "CrazyColors":
                    AfterAnomaly();
                    CrazyColors = true;
                    break;
                case "TruthPotion":
                    AfterAnomaly();
                    TruthPotion();
                    break;
            }
        }
        else AfterAnomaly();
    }

    public static void AfterAnomaly()
    {
        CrazyColors = false;
        foreach (var former in FormerRoles)
        {
            var role = former.Value;
            var player = Utils.GetPlayer(former.Key);
            if (!player.IsAlive()) continue;
            player.RpcChangeRoleBasis(role);
            player.RpcSetCustomRole(role);
            FormerRoles.Remove(former.Key);
        }
    }

    public static void TruthPotion()
    {
        var randpc = Main.AllAlivePlayerControls.RandomElement();

        foreach (var pc in Main.AllAlivePlayerControls)
        {
            var rolestr = GetString($"{randpc.GetCustomRole()}");
            pc.Notify($"The Truth Potion anomaly has revealed {randpc.name} as the {rolestr}!");
        }
    }
    
    public static void ClownFest()
    {
        foreach (var clown in Main.AllAlivePlayerControls)
        {
            var role = clown.GetCustomRole();
            if (role.IsImpostor()) return;
            if (role.IsCoven()) return;
            if (role.IsNeutral()) return;
            if (role.IsCrewmate())
            {
                FormerRoles[clown.PlayerId] = role;
                clown.RpcChangeRoleBasis(CustomRoles.Jester);
                clown.RpcSetCustomRole(CustomRoles.Jester);
            }
            clown.Notify(ColorString(GetRoleColor(CustomRoles.Jester), GetString("ClownFestAnomaly")));
        }
    }
    public static void Retrial()
    {
        foreach (var player in Main.AllPlayerControls)
        {
            if (player.IsHost())
            {
                player.NoCheckStartMeeting(null, force: true);
            }
        }
    }

    public static void NewYear()
    {
        List<PlayerControl> assigning = [];
        List<CustomRoles> ModifierStorage = [];
        foreach (var player in Main.AllAlivePlayerControls)
        {
            assigning.Add(player);
            var Modifiers = Main.PlayerStates[player.PlayerId].SubRoles.ToList();
            foreach (var role in Modifiers)
            {
                if (role.IsImpOnlyModifier() ||
                    role is CustomRoles.Lovers ||
                    role.IsBetrayalModifier()) return;
                Modifiers.Remove(role);
                ModifierStorage.Add(role);

                var RandomModifier = ModifierStorage.RandomElement();
                if (assigning.Contains(player))
                {
                    assigning.Remove(player);
                    ModifierStorage.Remove(RandomModifier);
                    Modifiers.Add(RandomModifier);
                }
            }
            player.Notify(ColorString(GetRoleColor(CustomRoles.Jester), GetString("NewYearAnomaly")));
        }

    }

    public static void Holiday()
    {
        foreach (var player in Main.AllAlivePlayerControls)
        {
            var role = player.GetCustomRole();
            if (role.IsImpostor())
            {
                FormerRoles[player.PlayerId] = role;
                player.RpcChangeRoleBasis(CustomRoles.ImpostorTOHO);
                player.RpcSetCustomRole(CustomRoles.ImpostorTOHO);
            }
            if (role.IsCoven()) return;
            if (role.IsNeutral()) return;
            if (role.IsCrewmate())
            {
                FormerRoles[player.PlayerId] = role;
                player.RpcChangeRoleBasis(CustomRoles.CrewmateTOHO);
                player.RpcSetCustomRole(CustomRoles.CrewmateTOHO);
            }
            player.Notify(ColorString(GetRoleColor(CustomRoles.Jester), GetString("HolidayAnomaly")));
        }
    }

    public static void Shuffle()
    {
        List<PlayerControl> assigning = [];
        List<CustomRoles> ModifierStorage = [];
        foreach (var player in Main.AllAlivePlayerControls)
        {
            assigning.Add(player);
            var role = Main.PlayerStates[player.PlayerId].MainRole;
            if (role.IsImpostor() ||
                role.IsCoven() ||
                role.IsNK() ||
                role.IsNA()) return;
            ModifierStorage.Add(role);

            var RandomModifier = ModifierStorage.RandomElement();
            if (assigning.Contains(player))
            {
                player.RpcChangeRoleBasis(RandomModifier);
                player.RpcSetCustomRole(RandomModifier);
                assigning.Remove(player);
                ModifierStorage.Remove(RandomModifier);
            }
            player.Notify(ColorString(GetRoleColor(CustomRoles.Jester), GetString("ShuffleAnomaly")));
        }
    }

    public static bool CrazyColors = false;
    
    public static void OnFixedUpdate()
    { 
        if (!CrazyColors) return;
        foreach (var player in Main.AllAlivePlayerControls)
        {
            player.Notify(ColorString(GetRoleColor(CustomRoles.Rainbow), GetString("CrazyColorsAnomaly")));
            if (LastColorChange + Options.ColorChangeCoolDown.GetInt() <= Utils.GetTimeStamp())
            {
                LastColorChange = Utils.GetTimeStamp();
                var sender = CustomRpcSender.Create("Anomaly CrazyColors Sender");
                int color = IRandom.Instance.Next(0, 18);
                player.SetColor(color);
                sender.AutoStartRpc(player.NetId, (byte)RpcCalls.SetColor)
                    .Write(player.Data.NetId)
                    .Write((byte)color)
                    .EndRpc();
                sender.SendMessage();
            }
        }
    }
}
