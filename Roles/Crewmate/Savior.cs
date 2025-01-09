using AmongUs.GameOptions;
using Hazel;
using UnityEngine;
using TOHE.Modules;
using TOHE.Roles.Core;
using static TOHE.Utils;
using static TOHE.Translator;
using System;


namespace TOHE.Roles.Crewmate;

internal class Savior : RoleBase
{
    //===========================SETUP================================\\
    private const int Id = 31000;
    public static bool HasEnabled => CustomRoleManager.HasEnabled(CustomRoles.Savior);
    public override CustomRoles ThisRoleBase => CustomRoles.Impostor;
    public override Custom_RoleType ThisRoleType => Custom_RoleType.CrewmateSupport;
    //==================================================================\\

    private static OptionItem ResetCooldown;

    public static readonly List<byte> ProtectList = [];

    private static byte TempMarkProtected;

    public override void SetupCustomOption()
    {
        Options.SetupRoleOptions(Id, TabGroup.CrewmateRoles, CustomRoles.Savior);
        ResetCooldown = FloatOptionItem.Create(Id + 30, "SaviorResetCooldown", new(0f, 120f, 1f), 10f, TabGroup.CrewmateRoles, false)
            .SetParent(Options.CustomRoleSpawnChances[CustomRoles.Savior])
            .SetValueFormat(OptionFormat.Seconds);
    }
    public override void Init()
    {
        ProtectList.Clear();
        TempMarkProtected = byte.MaxValue;
    }
    public override void Add(byte playerId)
    {
        AbilityLimit = 1;

        if (!Main.ResetCamPlayerList.Contains(playerId))
            Main.ResetCamPlayerList.Add(playerId);
    }
    public override bool ForcedCheckMurderAsKiller(PlayerControl killer, PlayerControl target)
    {
        if (killer == null || target == null) return false;
        if (!CheckKillButton(killer.PlayerId)) return false;
        if (ProtectList.Contains(target.PlayerId)) return false;
        if (AbilityLimit <= 0) return false;

        AbilityLimit--;
        ProtectList.Add(target.PlayerId);
        TempMarkProtected = target.PlayerId;

        if (!Options.DisableShieldAnimations.GetBool()) killer.RpcGuardAndKill();

        return true;
    }

    public bool CheckKillButton(byte playerId)
           => !Main.PlayerStates[playerId].IsDead
           && AbilityLimit > 0;

    public override bool CanUseKillButton(PlayerControl pc) => CheckKillButton(pc.PlayerId);

    public override bool CheckMurderOnOthersTarget(PlayerControl killer, PlayerControl target)
    {
        var Saviors = Utils.GetPlayerListByRole(CustomRoles.Savior);
        if (killer == null || target == null || Saviors == null || !Saviors.Any()) return true;
        if (!ProtectList.Contains(target.PlayerId)) return false;
        killer.RpcGuardAndKill(target);
        killer.SetKillCooldown(ResetCooldown.GetFloat());
        Logger.Info($"{target.GetNameWithRole()} : Shield Shatter from the Savior", "Savior");
        return true;
    }
    public override void AfterMeetingTasks()
    {
        ProtectList.Clear();
        AbilityLimit = 1;
    }
}
