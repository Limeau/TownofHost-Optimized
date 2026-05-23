using AmongUs.GameOptions;
using TOHO.Modules;
using TOHO.Roles.Core;
using static TOHO.Options;
namespace TOHO.Roles.Neutral;

internal class Shade : RoleBase
{
    //===========================SETUP================================\\
    private const int Id = 39000;
    public static bool HasEnabled => CustomRoleManager.HasEnabled(CustomRoles.Shade);
    public override CustomRoles Role => CustomRoles.Shade;
    public override CustomRoles ThisRoleBase => CustomRoles.Impostor;
    public override Custom_RoleType ThisRoleType => Custom_RoleType.NeutralKilling;
    public override bool TOHORole => true;
    //==================================================================\\

    public static OptionItem ShadeAbilityUses;
    public static OptionItem ShadeAbilityUsesGainedWithEachShadedKill;
    public static OptionItem ShadeProtectCooldown;
    public static OptionItem ShadePossessTime;
    
    public override void SetupCustomOption()
    {
        SetupRoleOptions(Id, TabGroup.NeutralRoles, CustomRoles.Shade);
        ShadeAbilityUses = FloatOptionItem.Create(Id + 10, "ShadeAbilityUses", (1f, 5f, 0.5f), 3f, TabGroup.NeutralRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Shade]);
        ShadeAbilityUsesGainedWithEachShadedKill = FloatOptionItem.Create(Id + 11, "ShadeAbilityUsesGainedWithEachShadedKill", (0f, 2f, 0.25f), 1f, TabGroup.NeutralRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Shade]);
        ShadeProtectCooldown = FloatOptionItem.Create(Id + 12, "ShadeProtectCooldown", (0f, 60f, 1f), 20f, TabGroup.NeutralRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Shade])
            .SetValueFormat(OptionFormat.Seconds);
        ShadePossessTime = FloatOptionItem.Create(Id + 13, "ShadePossessTime", (0f, 60f, 1f), 20f, TabGroup.NeutralRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Shade])
            .SetValueFormat(OptionFormat.Seconds);
    }

    public override void Add(byte playerId)
    {
        Utils.GetPlayerById(playerId).SetAbilityUseLimit(ShadeAbilityUses.GetFloat());
    }

    public override bool CanUseKillButton(PlayerControl pc)
    {
        return false;
    }

    public override void OnMurderPlayerAsTarget(PlayerControl killer, PlayerControl target, bool inMeeting,
        bool isSuicide)
    {
        target.RpcSetCustomRole(CustomRoles.ShadeX);
        target.RpcChangeRoleBasis(CustomRoles.ShadeX);
    }

    public override void AfterMeetingTasks()
    {
        if (!_Player.IsAlive())
        {
            _Player.RpcSetCustomRole(CustomRoles.ShadeX);
            _Player.RpcChangeRoleBasis(CustomRoles.ShadeX);
        }
    }
}

internal class ShadeX : RoleBase
{
    //===========================SETUP================================\\
    public override CustomRoles Role => CustomRoles.ShadeX;
    public override CustomRoles ThisRoleBase => CustomRoles.GuardianAngel;
    public override Custom_RoleType ThisRoleType => Custom_RoleType.NeutralKilling;
    //==================================================================\\

    public static PlayerControl Possessed = null;
    public static PlayerControl Possessor = null;
    private NetworkedPlayerInfo.PlayerOutfit Skin2 = null;
    public override bool OnCheckProtect(PlayerControl angel, PlayerControl target)
    {
        if (angel.GetAbilityUseLimit() < 1) return false;
        var Skin1 = new NetworkedPlayerInfo.PlayerOutfit()
            .Set(target.GetRealName(), target.CurrentOutfit.ColorId, target.CurrentOutfit.HatId, target.CurrentOutfit.SkinId, target.CurrentOutfit.VisorId, target.CurrentOutfit.PetId, target.CurrentOutfit.NamePlateId);
        Skin2 = new NetworkedPlayerInfo.PlayerOutfit()
            .Set(angel.GetRealName(), angel.CurrentOutfit.ColorId, angel.CurrentOutfit.HatId, angel.CurrentOutfit.SkinId, angel.CurrentOutfit.VisorId, angel.CurrentOutfit.PetId, angel.CurrentOutfit.NamePlateId);
        if (target.HasModifier(CustomRoles.Shaded))
        {
            angel.KillWithoutBody(target);
            angel.RpcSetCustomRole(target.GetCustomRole());
            angel.RpcChangeRoleBasis(target.GetCustomRole());
            angel.RpcRemoveAbilityUse();
            angel.SetNewOutfit(Skin1);
            Main.OvverideOutfit[angel.PlayerId] = (Skin1, Main.PlayerStates[target.PlayerId].NormalOutfit.PlayerName);
            Possessed = target;
            Possessor = angel;
            new LateTask(() =>
            {
                target.RpcRevive();
                angel.RpcSetCustomRole(CustomRoles.ShadeX);
                angel.RpcChangeRoleBasis(CustomRoles.ShadeX);
                angel.SetNewOutfit(Skin2);
                Main.OvverideOutfit[angel.PlayerId] = (Skin2, Main.PlayerStates[angel.PlayerId].NormalOutfit.PlayerName);
                Possessed = null;
                Possessor = null;
            }, Shade.ShadePossessTime.GetFloat(), "Shade Reset Possession");
        }
        else
        {
            target.RpcSetCustomRole(CustomRoles.Shaded);
            angel.RpcResetAbilityCooldown();
            angel.RpcRemoveAbilityUse();
        }
        return false;
    }

    public override void OnReportDeadBody(PlayerControl reporter, NetworkedPlayerInfo target)
    {
        if (Possessed != null && Possessor != null && Skin2 != null)
        {
            Possessed.RpcRevive();
            Possessor.RpcSetCustomRole(CustomRoles.ShadeX);
            Possessor.RpcChangeRoleBasis(CustomRoles.ShadeX);
            Possessor.SetNewOutfit(Skin2);
            Main.OvverideOutfit[Possessor.PlayerId] = (Skin2, Main.PlayerStates[Possessor.PlayerId].NormalOutfit.PlayerName);
            Possessed = null;
            Possessor = null;
        }
    }

    public override void ApplyGameOptions(IGameOptions opt, byte playerId)
    {
        AURoleOptions.GuardianAngelCooldown = Shade.ShadeProtectCooldown.GetFloat();
        AURoleOptions.ProtectionDurationSeconds = 0f;
    }

    public override void OnFixedUpdate(PlayerControl player, bool lowLoad, long nowTime, int timerLowLoad)
    {
        if (Possessed != null && Possessor != null)
        {
            if (!Possessed.IsHost()) Possessed.RpcTeleport(Possessor.transform.position, sendInfoInLogs: false);
            else
            {
                _ = new LateTask(() =>
                {
                    Possessed?.RpcTeleport(Possessor.transform.position, sendInfoInLogs: false);
                }, 0.25f, "Shade Teleport", shoudLog: false);
            }
        }
    }

    public override bool CheckMurderOnOthersTarget(PlayerControl killer, PlayerControl target)
    {
        if (killer.HasModifier(CustomRoles.Shaded)) _Player.RpcIncreaseAbilityUseLimitBy(Shade.ShadeAbilityUsesGainedWithEachShadedKill.GetFloat());
        return base.CheckMurderOnOthersTarget(killer, target);
    }
}
