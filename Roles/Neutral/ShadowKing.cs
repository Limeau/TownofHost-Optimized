using System;
using AmongUs.GameOptions;
using TOHO.Modules;
using TOHO.Roles.Core;
using static TOHO.Options;
namespace TOHO.Roles.Neutral;

internal class ShadowKing : RoleBase
{
    //===========================SETUP================================\\
    private const int Id = 37200;
    public static bool HasEnabled => CustomRoleManager.HasEnabled(CustomRoles.ShadowKing);
    public override CustomRoles Role => CustomRoles.ShadowKing;
    public override CustomRoles ThisRoleBase => CustomRoles.Shapeshifter;
    public override Custom_RoleType ThisRoleType => Custom_RoleType.NeutralKilling;
    //==================================================================\\

    public static OptionItem KillCooldown;
    public static OptionItem ShapeshiftCooldown;
    public static OptionItem AbilityUses;
    public static OptionItem AbilityUsesPerKill;

    public static bool SecondLife;
    
    public override void SetupCustomOption()
    {
        SetupRoleOptions(Id, TabGroup.NeutralRoles, CustomRoles.ShadowKing);
        KillCooldown = FloatOptionItem.Create(Id + 10, GeneralOption.KillCooldown, new(1f, 60f, 1f), 20f, TabGroup.NeutralRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.ShadowKing])
            .SetValueFormat(OptionFormat.Seconds);
        ShapeshiftCooldown = FloatOptionItem.Create(Id + 11, GeneralOption.ShapeshifterBase_ShapeshiftCooldown, new(1f, 60f, 1f), 20f, TabGroup.NeutralRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.ShadowKing])
            .SetValueFormat(OptionFormat.Seconds);
        AbilityUses = IntegerOptionItem.Create(Id + 12, "AbilityUses372", new(1, 5, 1), 3, TabGroup.NeutralRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.ShadowKing]);
        AbilityUsesPerKill = FloatOptionItem.Create(Id + 13, "AbilityUsesPerKill372", new(0.25f, 2f, 0.25f), 1f, TabGroup.NeutralRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.ShadowKing]);
    }
    public override void Add(byte playerId)
    {
        SecondLife = true;
        playerId.SetAbilityUseLimit(AbilityUses.GetInt());
    }

    public override bool OnCheckMurderAsTarget(PlayerControl killer, PlayerControl target)
    {
        if (SecondLife == false) return true;
        List<PlayerControl> CandidatesList = [];
        foreach (var candidate in Main.AllAlivePlayerControls)
        {
            if (candidate != target && candidate != killer && !candidate.IsHost()) CandidatesList.Add(candidate);
        }
        var hostage = CandidatesList.RandomElement();
        if (hostage == null) return true;
        
        string hname = killer.GetRealName(isMeeting: true);
        string tname = target.GetRealName(isMeeting: true);
        var hostageSkin = new NetworkedPlayerInfo.PlayerOutfit()
            .Set(hname, hostage.CurrentOutfit.ColorId, hostage.CurrentOutfit.HatId, hostage.CurrentOutfit.SkinId, hostage.CurrentOutfit.VisorId, hostage.CurrentOutfit.PetId, hostage.CurrentOutfit.NamePlateId);
        var hostageLvl = hostage.Data.PlayerLevel;
        var targetSkin = new NetworkedPlayerInfo.PlayerOutfit()
            .Set(tname, target.CurrentOutfit.ColorId, target.CurrentOutfit.HatId, target.CurrentOutfit.SkinId, target.CurrentOutfit.VisorId, target.CurrentOutfit.PetId, target.CurrentOutfit.NamePlateId);
        var targetLvl = target.Data.PlayerLevel;
        
        target.SetNewOutfit(hostageSkin, newLevel: hostageLvl);
        Main.OvverideOutfit[target.PlayerId] = (hostageSkin, Main.PlayerStates[hostage.PlayerId].NormalOutfit.PlayerName);
        Logger.Info("Changed target skin", "ShadowKing");
        hostage.SetNewOutfit(targetSkin, newLevel: targetLvl);
        Main.OvverideOutfit[killer.PlayerId] = (targetSkin, Main.PlayerStates[target.PlayerId].NormalOutfit.PlayerName);
        Logger.Info("Changed hostage skin", "ShadowKing");
        
        var positionTarget1 = hostage.GetCustomPosition();
        var positionTarget2 = target.GetCustomPosition();
        hostage.RpcTeleport(positionTarget2);
        target.RpcTeleport(positionTarget1);
        
        killer.RpcMurderPlayer(hostage);
        SecondLife = false;
        
        return false;
    }

    public override bool OnCheckShapeshift(PlayerControl shapeshifter, PlayerControl target, ref bool resetCooldown,
        ref bool shouldAnimate)
    {
        if (shapeshifter.GetAbilityUseLimit() <= 0) return false;
        shapeshifter.RpcRemoveAbilityUse();
        
        List<PlayerControl> AllDeadPlayerControls = [];
        foreach (var dead in Main.AllPlayerControls)
        {
            if (Main.AllAlivePlayerControls.Contains(dead)) continue;
            AllDeadPlayerControls.Add(dead);
        }

        var hostage = AllDeadPlayerControls.RandomElement();
        hostage.RpcRevive();
        hostage.RpcChangeRoleBasis(hostage.GetCustomRole());
        hostage.RpcMurderPlayer(target);
        hostage.RpcMurderPlayer(hostage);
        target.SetRealKiller(shapeshifter);
        hostage.SetRealKiller(shapeshifter);
        
        shapeshifter.RpcResetAbilityCooldown();
        
        return false;
    }

    public override bool OnCheckMurderAsKiller(PlayerControl killer, PlayerControl target)
    {
        killer.RpcIncreaseAbilityUseLimitBy(AbilityUsesPerKill.GetFloat());
        return true;
    }
    public override void SetKillCooldown(byte id)
    {
        Main.AllPlayerKillCooldown[id] = KillCooldown.GetFloat();
    }
    public override void ApplyGameOptions(IGameOptions opt, byte playerId)
    {
        AURoleOptions.ShapeshifterCooldown = ShapeshiftCooldown.GetFloat();
    }
    public override bool CanUseKillButton(PlayerControl pc) => true;
    public override bool CanUseImpostorVentButton(PlayerControl pc) => true;
}
