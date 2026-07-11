using System;
using System.Collections.Generic;
using System.Linq;
using TOHO.Modules;
using static TOHO.Options;
using static TOHO.Translator;

namespace TOHO.Roles.Modifiers.Common;

public class Bait : IModifier
{
    public CustomRoles Role => CustomRoles.Bait;
    private const int Id = 18700;
    public ModifierTypes Type => ModifierTypes.Helpful;

    public static OptionItem BaitDelayMin;
    public static OptionItem BaitDelayMax;
    public static OptionItem BaitDelayNotify;
    public static OptionItem BaitNotification;
    public static OptionItem BaitCanBeReportedUnderAllConditions;

    public static readonly HashSet<byte> BaitAlive = [];

    public void SetupCustomOption()
    {
        SetupAdtRoleOptions(Id, CustomRoles.Bait, canSetNum: true, teamSpawnOptions: true);
        BaitDelayMin = FloatOptionItem.Create(Id + 13, "BaitDelayMin", new(0f, 5f, 1f), 0f, TabGroup.Modifiers, false).SetParent(CustomRoleSpawnChances[CustomRoles.Bait])
            .SetValueFormat(OptionFormat.Seconds);
        BaitDelayMax = FloatOptionItem.Create(Id + 14, "BaitDelayMax", new(0f, 10f, 1f), 0f, TabGroup.Modifiers, false).SetParent(CustomRoleSpawnChances[CustomRoles.Bait])
            .SetValueFormat(OptionFormat.Seconds);
        BaitDelayNotify = BooleanOptionItem.Create(Id + 15, "BaitDelayNotify", false, TabGroup.Modifiers, false).SetParent(CustomRoleSpawnChances[CustomRoles.Bait]);
        BaitNotification = BooleanOptionItem.Create(Id + 16, "BaitNotification", false, TabGroup.Modifiers, false).SetParent(CustomRoleSpawnChances[CustomRoles.Bait]);
        BaitCanBeReportedUnderAllConditions = BooleanOptionItem.Create(Id + 17, "BaitCanBeReportedUnderAllConditions", false, TabGroup.Modifiers, false).SetParent(CustomRoleSpawnChances[CustomRoles.Bait]);
    }

    public void Init()
    {
        BaitAlive.Clear();
    }
    public void Add(byte playerId, bool gameIsLoading = true)
    {
        BaitAlive.Add(playerId);
    }
    public void Remove(byte playerId)
    {
        BaitAlive.Remove(playerId);
    }
    public static void SendNotify()
    {
        if (MeetingStates.FirstMeeting && CustomRoles.Bait.RoleExist() && BaitNotification.GetBool())
        {
            foreach (var pc in Main.AllAlivePlayerControls.Where(x => x.Is(CustomRoles.Bait) && !BaitAlive.Contains(x.PlayerId)).ToArray())
            {
                BaitAlive.Add(pc.PlayerId);
            }
            HashSet<string> baitAliveList = [];
            foreach (var whId in BaitAlive.ToArray())
            {
                PlayerControl whpc = whId.GetPlayer();
                if (whpc == null) continue;
                baitAliveList.Add(whpc.GetRealName());
            }
            string separator = TranslationController.Instance.currentLanguage.languageID is SupportedLangs.English or SupportedLangs.Russian ? "], [" : "】, 【";
            MeetingHudStartPatch.AddMsg(string.Format(GetString("BaitAdviceAlive"), string.Join(separator, baitAliveList)), 255, Utils.ColorString(Utils.GetRoleColor(CustomRoles.Bait), GetString("BaitAliveTitle")));
        }
    }
    public static void BaitAfterDeathTasks(PlayerControl killer, PlayerControl target)
    {
        if (killer.PlayerId == target.PlayerId)
        {
            if (target.GetRealKiller() != null)
            {
                if (!target.GetRealKiller().IsAlive()) return;
                killer = target.GetRealKiller();
            }
        }

        if (killer.PlayerId == target.PlayerId) return;

        if (killer.Is(CustomRoles.KillingMachine)
            || killer.Is(CustomRoles.Swooper)
            || target.Is(CustomRoles.Massacre)
            || killer.Is(CustomRoles.Wraith)
            || killer.Is(CustomRoles.Cleaner)
            || killer.Is(CustomRoles.Councillor) && Main.PlayerStates[target.PlayerId].deathReason == PlayerState.DeathReason.Trialed
            || (DisableReportWhenCC.GetBool() && Utils.IsActive(SystemTypes.Comms) && Camouflage.IsActive && !BaitCanBeReportedUnderAllConditions.GetBool())
            || (killer.Is(CustomRoles.Oblivious) && Oblivious.ObliviousBaitImmune.GetBool()))
            return;

        {
            killer.RPCPlayCustomSound("Congrats");
            target.RPCPlayCustomSound("Congrats");
            float delay;
            if (BaitDelayMax.GetFloat() < BaitDelayMin.GetFloat()) delay = 0f;
            else delay = IRandom.Instance.Next((int)BaitDelayMin.GetFloat(), (int)BaitDelayMax.GetFloat() + 1);
            delay = Math.Max(delay, 0.15f);
            if (delay > 0.15f && BaitDelayNotify.GetBool()) killer.Notify(Utils.ColorString(Utils.GetRoleColor(CustomRoles.Bait), string.Format(GetString("KillBaitNotify"), (int)delay)), delay);
            Logger.Info($"{killer.GetNameWithRole()} 击杀诱饵 => {target.GetNameWithRole()}", "MurderPlayer");
            _ = new LateTask(() => { if (GameStates.IsInTask && GameStates.IsInGame) killer?.CmdReportDeadBody(target.Data); }, delay, "Bait Self Report");
        }
    }
}

