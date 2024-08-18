using AmongUs.GameOptions;
using System;
using System.Linq;
using TOHE.Roles.AddOns.Common;
using TOHE.Roles.Core;
using TOHE.Roles.Double;
using UnityEngine;
using static TOHE.Options;
using static TOHE.Translator;
using static TOHE.Utils;

namespace TOHE.Roles._Ghosts_.Crewmate;

internal class Cursebearer : RoleBase
{
    //===========================SETUP================================\\
    private const int Id = 31200;
    public static bool HasEnabled => CustomRoleManager.HasEnabled(CustomRoles.Cursebearer);
    public override CustomRoles ThisRoleBase => CustomRoles.GuardianAngel;
    public override Custom_RoleType ThisRoleType => Custom_RoleType.CrewmateGhosts;
    //==================================================================\\

    public static OptionItem RevealCooldown;
    public int KeepCount = 0;
    public bool KnowTargetRole = false;
    public override void SetupCustomOption()
    {
        SetupSingleRoleOptions(Id, TabGroup.CrewmateRoles, CustomRoles.Cursebearer);
        RevealCooldown = FloatOptionItem.Create(Id + 10, GeneralOption.KillCooldown, new(0f, 120f, 2.5f), 25f, TabGroup.CrewmateRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.Cursebearer]);
    }
    public override void Init()
    {
        KeepCount = 0;
        KnowTargetRole = false;
    }
    public override void Add(byte playerId)
    {
        AbilityLimit = 1;

        foreach (var pc in Main.AllPlayerControls)
        {
            if (pc.IsAnySubRole(x => x.IsConverted()))
            {
                KeepCount++;
            }
        }

    }
    public override void ApplyGameOptions(IGameOptions opt, byte playerId)
    {
        AURoleOptions.GuardianAngelCooldown = RevealCooldown.GetFloat();
        AURoleOptions.ProtectionDurationSeconds = 0f;
    }
    public override bool OnCheckProtect(PlayerControl killer, PlayerControl target)
    {
        if (AbilityLimit <= 0) return false;
        else
        {
            target.RpcSetCustomRole(CustomRoles.Revealed);
            return true;
        }
    }


    public static bool KnowRole(PlayerControl seer, PlayerControl target)
    {
        if (target.Is(CustomRoles.Revealed)) return true;
        return false;
    }
}
