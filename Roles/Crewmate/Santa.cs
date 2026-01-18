using AmongUs.GameOptions;
using System;
using UnityEngine;
using static TOHO.Options;

namespace TOHO.Roles.Crewmate;

internal class Santa : RoleBase
{
    //===========================SETUP================================\\
    public override CustomRoles Role => CustomRoles.Santa;
    private const int Id = 38100;
    public override bool IsDesyncRole => true;
    public override CustomRoles ThisRoleBase => CustomRoles.Impostor;
    public override Custom_RoleType ThisRoleType => Custom_RoleType.CrewmateSupport;
    //==================================================================\\

    private static OptionItem SantaKillCooldown;

    public static List<PlayerControl> NiceList = [];
    public static List<PlayerControl> NaughtyList = [];
    
    public override void SetupCustomOption()
    {
        SetupRoleOptions(Id, TabGroup.CrewmateRoles, CustomRoles.Santa);
        SantaKillCooldown = FloatOptionItem.Create(Id + 10, GeneralOption.AbilityCooldown, new(1f, 60f, 1f), 20f, TabGroup.CrewmateRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.Santa])
            .SetValueFormat(OptionFormat.Seconds);
        }
    public override void SetKillCooldown(byte id) => Main.AllPlayerKillCooldown[id] = SantaKillCooldown.GetFloat();
    public override bool CanUseImpostorVentButton(PlayerControl pc) => false;
    public override bool CanUseKillButton(PlayerControl pc) => true;

    public override void ApplyGameOptions(IGameOptions opt, byte playerId) => opt.SetVision(false);
    public override bool OnCheckMurderAsKiller(PlayerControl killer, PlayerControl target)
    {
        if (target.IsPlayerCrewmateTeam()) NiceList.Add(target);
        if (target.IsPlayerImpostorTeam()) NaughtyList.Add(target);
        killer.RpcGuardAndKill();
        killer.ResetKillCooldown();
        return false;
    }

    public override bool CheckMurderOnOthersTarget(PlayerControl killer, PlayerControl target)
    {
        if (NaughtyList.Contains(killer) || NiceList.Contains(target))
        {
            if (NaughtyList.Contains(killer)) NaughtyList.Remove(killer);
            if (NiceList.Contains(target)) NiceList.Remove(target);
            killer.RpcGuardAndKill();
            killer.ResetKillCooldown();
            return true;
        }
        return false;
    }
    
    public override void SetAbilityButtonText(HudManager hud, byte playerId)
    {
        hud.KillButton.OverrideText(Translator.GetString("SantaButtonText"));
    }

    public override Sprite GetKillButtonSprite(PlayerControl player, bool shapeshifting) => CustomButton.Get("Santa");
}
