using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;

namespace TOHO.Roles.Modifiers.Common;

public class Distracted : IModifier
{
    public CustomRoles Role => CustomRoles.Distracted;
    private const int Id = 39100;
    public ModifierTypes Type => ModifierTypes.Harmful;
    public static bool IsEnable = false;

    private static OptionItem SpeedBoost;
    private static OptionItem Radius;

    private static bool Active;
    private static readonly HashSet<byte> CountNearplr = [];
    private static readonly Dictionary<byte, float> TempSpeed = [];

    public void SetupCustomOption()
    {
        Options.SetupAdtRoleOptions(Id, CustomRoles.Distracted, canSetNum: true, tab: TabGroup.Modifiers, teamSpawnOptions: true);
        SpeedBoost = FloatOptionItem.Create(Id + 10, "SpeedReduction391", new(0f, 2f, 0.25f), 1f, TabGroup.Modifiers, false).SetParent(Options.CustomRoleSpawnChances[CustomRoles.Distracted])
             .SetValueFormat(OptionFormat.Multiplier);
        Radius = FloatOptionItem.Create(Id + 11, "Radius391", new(1f, 3f, 0.5f), 1.5f, TabGroup.Modifiers, false).SetParent(Options.CustomRoleSpawnChances[CustomRoles.Distracted])
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
        foreach (var (Distracted, speed) in TempSpeed)
        {
            Main.AllPlayerSpeed[Distracted] = speed;
            Distracted.GetPlayer()?.MarkDirtySettings();
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
        if (!victim.Is(CustomRoles.Distracted)) return;
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
