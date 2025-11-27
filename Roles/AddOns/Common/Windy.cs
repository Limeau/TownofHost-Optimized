using AmongUs.GameOptions;

namespace TOHO.Roles.AddOns.Common;

public class Windy : IAddon
{
    public CustomRoles Role => CustomRoles.Windy;
    private const int Id = 37400;
    public AddonTypes Type => AddonTypes.Helpful;
    public static bool IsEnable = false;

    private static OptionItem SpeedBoost;
    private static OptionItem Radius;

    private static bool Active;
    private static readonly HashSet<byte> CountNearplr = [];
    private static readonly Dictionary<byte, float> TempSpeed = [];

    public void SetupCustomOption()
    {
        Options.SetupAdtRoleOptions(Id, CustomRoles.Windy, canSetNum: true, tab: TabGroup.Addons, teamSpawnOptions: true);
        SpeedBoost = FloatOptionItem.Create(Id + 10, "SpeedBoost374", new(1f, 5f, 0.25f), 3f, TabGroup.Addons, false).SetParent(Options.CustomRoleSpawnChances[CustomRoles.Windy])
             .SetValueFormat(OptionFormat.Multiplier);
        Radius = FloatOptionItem.Create(Id + 11, "Radius374", new(1f, 3f, 0.5f), 1.5f, TabGroup.Addons, false).SetParent(Options.CustomRoleSpawnChances[CustomRoles.Windy])
             .SetValueFormat(OptionFormat.Multiplier);
    }

    public void Init()
    {
        IsEnable = false;
        CountNearplr.Clear();
        TempSpeed.Clear();
        Active = true;
    }

    public void Add(byte playerId, bool gameIsLoading = true)
    {
        var speed = Main.AllPlayerSpeed[playerId];
        TempSpeed[playerId] = speed;
        IsEnable = true;
    }
    public void Remove(byte playerId)
    {
        if (Main.AllPlayerSpeed[playerId] == SpeedBoost.GetFloat())
        {
            Main.AllPlayerSpeed[playerId] = Main.AllPlayerSpeed[playerId] - SpeedBoost.GetFloat() + TempSpeed[playerId];
            playerId.GetPlayer()?.MarkDirtySettings();
        }
        TempSpeed.Remove(playerId);

        if (!TempSpeed.Any())
            IsEnable = false;
    }

    public static void AfterMeetingTasks()
    {
        foreach (var (Windy, speed) in TempSpeed)
        {
            Main.AllPlayerSpeed[Windy] = speed;
            Windy.GetPlayer()?.MarkDirtySettings();
        }
        Active = false;
        CountNearplr.Clear();
        _ = new LateTask(() =>
        {
            Active = true;
        }, 6f);
    }

    public void OnFixedUpdate(PlayerControl victim)
    {
        if (!victim.Is(CustomRoles.Windy)) return;
        if (!victim.IsAlive() && victim != null)
        {
            var currentSpeed = Main.AllPlayerSpeed[victim.PlayerId];
            var normalSpeed = Main.RealOptionsData.GetFloat(FloatOptionNames.PlayerSpeedMod);
            if (currentSpeed != normalSpeed)
            {
                Main.AllPlayerSpeed[victim.PlayerId] = normalSpeed;
                victim.MarkDirtySettings();
            }
            return;
        }

        foreach (var PVC in Main.AllPlayerControls)
        {
            if (!PVC.IsAlive())
            {
                CountNearplr.Remove(PVC.PlayerId);
            }
            if (CountNearplr.Contains(PVC.PlayerId) && Utils.GetDistance(PVC.transform.position, victim.transform.position) > Radius.GetFloat())
            {
                CountNearplr.Remove(PVC.PlayerId);
            }
        }

        if (Active)
        {
            foreach (var plr in Main.AllAlivePlayerControls)
            {
                if (Utils.GetDistance(plr.transform.position, victim.transform.position) < 2f && plr != victim)
                {
                    if (!CountNearplr.Contains(plr.PlayerId)) CountNearplr.Add(plr.PlayerId);
                }
            }

            if (CountNearplr.Count >= 1)
            {
                if (Main.AllPlayerSpeed[victim.PlayerId] != SpeedBoost.GetFloat())
                {
                    Main.AllPlayerSpeed[victim.PlayerId] = SpeedBoost.GetFloat();
                    victim.MarkDirtySettings();
                }
                return;
            }
            else if (Main.AllPlayerSpeed[victim.PlayerId] == SpeedBoost.GetFloat())
            {
                float tmpFloat = TempSpeed[victim.PlayerId];
                Main.AllPlayerSpeed[victim.PlayerId] = Main.AllPlayerSpeed[victim.PlayerId] - SpeedBoost.GetFloat() + tmpFloat;
                victim.MarkDirtySettings();
            }
        }
    }
}
