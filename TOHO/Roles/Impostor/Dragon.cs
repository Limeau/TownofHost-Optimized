using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using Rewired;
using UnityEngine;
using static TOHO.Options;
using static TOHO.Translator;

namespace TOHO.Roles.Impostor;

internal class Dragon : RoleBase
{
    //===========================SETUP================================\\
    private const int Id = 45600;
    public override CustomRoles ThisRoleBase => CustomRoles.Impostor;
    public override Custom_RoleType ThisRoleType => Custom_RoleType.ImpostorKilling;
    public override CustomRoles Role => CustomRoles.Dragon;
    public override bool TOHORole => true;
    public override bool NewRole => true;
    //==================================================================\\

    public static OptionItem KillCooldown;
    private static OptionItem DragonTimeBeforeKill;
    private static OptionItem DragonRadius;
    private static OptionItem CanUseDoubleClick;

    public static List<PlayerControl> EnflamedPlayers = [];
    
    public override void SetupCustomOption()
    {
        SetupRoleOptions(Id, TabGroup.ImpostorRoles, CustomRoles.Dragon);
        KillCooldown = FloatOptionItem.Create(Id + 2, GeneralOption.KillCooldown, new(1f, 60f, 1f), 20f, TabGroup.ImpostorRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.Dragon])
            .SetValueFormat(OptionFormat.Seconds);
        DragonTimeBeforeKill = FloatOptionItem.Create(Id + 3, "DragonTimeBeforeKill", new(1f, 15f, 1f), 5f, TabGroup.ImpostorRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.Dragon])
            .SetValueFormat(OptionFormat.Seconds);
        DragonRadius = FloatOptionItem.Create(Id + 4, "DragonRadius", new(0.5f, 2.5f, 0.5f), 1.5f, TabGroup.ImpostorRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.Dragon])
                .SetValueFormat(OptionFormat.Multiplier);
        CanUseDoubleClick = BooleanOptionItem.Create(Id + 5, "CanUseDoubleClick", true, TabGroup.ImpostorRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.Dragon]);
    }

    public override void Add(byte playerId)
    {
        // Double Trigger
        var pc = Utils.GetPlayerById(playerId);
        if (CanUseDoubleClick.GetBool()) pc.AddDoubleTrigger();
    }

    public static void BurnPlayer(PlayerControl player)
    {            
        EnflamedPlayers.Add(player);
        player.Notify("YOU ARE ON FIRE!!!", time: DragonTimeBeforeKill.GetFloat());
        _ = new LateTask(() =>
        {
            if (!GameStates.IsMeeting)
            {
                player.SetDeathReason(PlayerState.DeathReason.Torched);
                player.RpcMurderPlayer(player);
                EnflamedPlayers.Remove(player);
            }
        }, DragonTimeBeforeKill.GetFloat(), "Dragon Burn");
    }
    
    public override bool OnCheckMurderAsKiller(PlayerControl killer, PlayerControl target)
    {
        if (CanUseDoubleClick.GetBool() && killer.CheckDoubleTrigger(target, () => {} ))
        {
            return true;
        }
        killer.RpcGuardAndKill();
        BurnPlayer(target);
        return false;
    }

    public override void OnFixedUpdate(PlayerControl player, bool lowLoad, long nowTime, int timerLowLoad)
    {
        if (!EnflamedPlayers.Any()) return;
        foreach (var burning in EnflamedPlayers)
        {
            foreach (var onfire in Main.AllAlivePlayerControls.Where(x =>
                         !EnflamedPlayers.Contains(x) && !x.Is(CustomRoles.Dragon)))
            {
                if (Utils.GetDistance(burning.transform.position, onfire.transform.position) <= DragonRadius.GetFloat())
                    BurnPlayer(onfire);
            }
        }
    }

    public override void SetAbilityButtonText(HudManager hud, byte playerId)
    {
        hud.KillButton.OverrideText($"{GetString("DragonKillText")}");
    }
    public override Sprite GetKillButtonSprite(PlayerControl player, bool shapeshifting) => CustomButton.Get("DragonAbility", 230f);
}