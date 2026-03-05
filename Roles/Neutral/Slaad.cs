using AmongUs.GameOptions;
using TOHO.Modules;
using static TOHO.Options;

namespace TOHO.Roles.Neutral;

internal class Slaad : RoleBase
{
    //===========================SETUP================================\\
    public override CustomRoles Role => CustomRoles.Slaad;
    private const int Id = 38600;
    public override bool IsDesyncRole => true;
    public override CustomRoles ThisRoleBase => CustomRoles.Impostor;
    public override Custom_RoleType ThisRoleType => Custom_RoleType.NeutralKilling;
    //==================================================================\\

    private static List<PlayerControl> Stage1Players = [];
    private static List<PlayerControl> BlindedPlayers = [];
    private static List<PlayerControl> Stage2Players = [];
    private static PlayerControl TheSlaad = null;
    private static int DeathsCounter = 0;
    private static readonly NetworkedPlayerInfo.PlayerOutfit GrayOutfit = new NetworkedPlayerInfo.PlayerOutfit().Set("", 15, "", "", "visor_Crack", "", "");
    
    private static OptionItem KillCooldown;
    private static OptionItem HasImpostorVision;
    private static OptionItem CanUsesSabotage;
    private static OptionItem Stage1DelayTime;
    private static OptionItem Speed;
    private static OptionItem Vision;
    private static OptionItem KillCooldownReduction;
    private static OptionItem MinimumKillCooldown;

    public override void SetupCustomOption()
    {
        SetupSingleRoleOptions(Id, TabGroup.NeutralRoles, CustomRoles.Slaad, 1, zeroOne: false);
        KillCooldown = FloatOptionItem.Create(Id + 10, GeneralOption.KillCooldown, new(0f, 180f, 2.5f), 20f, TabGroup.NeutralRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Slaad])
            .SetValueFormat(OptionFormat.Seconds);
        HasImpostorVision = BooleanOptionItem.Create(Id + 11, GeneralOption.ImpostorVision, true, TabGroup.NeutralRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Slaad]);
        CanUsesSabotage = BooleanOptionItem.Create(Id + 12, GeneralOption.CanUseSabotage, true, TabGroup.NeutralRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Slaad]);
        Stage1DelayTime = FloatOptionItem.Create(Id + 13, "SlaadStage1DelayTime", new(1f, 10f, 0.5f), 5f, TabGroup.NeutralRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Slaad])
            .SetValueFormat(OptionFormat.Seconds);
        Speed = FloatOptionItem.Create(Id + 14, "SlaadSpeed", new(0.5f, 2f, 0.1f), 1f, TabGroup.NeutralRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Slaad])
            .SetValueFormat(OptionFormat.Multiplier);
        Vision = FloatOptionItem.Create(Id + 15, "SlaadVision", new(0.5f, 2f, 0.1f), 1f, TabGroup.NeutralRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Slaad])
            .SetValueFormat(OptionFormat.Multiplier);
        KillCooldownReduction = FloatOptionItem.Create(Id + 16, "SlaadKillCooldownReduction", new(0f, 10f, 0.5f), 4f, TabGroup.NeutralRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Slaad])
            .SetValueFormat(OptionFormat.Seconds);
        MinimumKillCooldown = FloatOptionItem.Create(Id + 17, "SlaadMinimumKillCooldown", new(0f, 30f, 1f), 8f, TabGroup.NeutralRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Slaad])
            .SetValueFormat(OptionFormat.Seconds);
    }
    public override void ApplyGameOptions(IGameOptions opt, byte playerId)
    {
        opt.SetVision(HasImpostorVision.GetBool());
    }
    public override void Add(byte playerId)
    {
        var slaad = Utils.GetPlayerById(playerId);
        slaad.AddDoubleTrigger();
        TheSlaad = slaad;
    }
    public override void SetKillCooldown(byte id) => Main.AllPlayerKillCooldown[id] = KillCooldown.GetFloat();
    public override bool CanUseKillButton(PlayerControl pc) => true;
    public override bool CanUseImpostorVentButton(PlayerControl pc) => true;
    public override bool CanUseSabotage(PlayerControl pc) => CanUsesSabotage.GetBool();

    public static void ApplyGameOptionsForOthers(IGameOptions opt, PlayerControl player)
    {
        if (BlindedPlayers.Any())
        {
            opt.SetVision(false);
            opt.SetFloat(FloatOptionNames.CrewLightMod, Vision.GetFloat());
            opt.SetFloat(FloatOptionNames.ImpostorLightMod, Vision.GetFloat());
        }
    }
    
    public override bool OnCheckMurderAsKiller(PlayerControl killer, PlayerControl target)
    {
        if (killer.CheckDoubleTrigger(target, () => { }))
        {
            if (Stage1Players.Contains(target)) Stage1Players.Remove(target);
            if (Stage2Players.Contains(target)) Stage2Players.Remove(target);
            return true;
        }

        if (Stage1Players.Contains(target)) return false;
        if (Stage2Players.Contains(target)) return false;
        
        Stage1Players.Add(target);
        killer.RpcGuardAndKill();
        killer.ResetKillCooldown();
        new LateTask(() =>
        {
            BlindedPlayers.Add(target);
            Main.AllPlayerSpeed[target.PlayerId] = Speed.GetFloat();
            Utils.MarkEveryoneDirtySettings();
        }, Stage1DelayTime.GetFloat(), "Slaad Delay");
        return false;
    }

    public override void AfterMeetingTasks()
    {
        foreach (var player in Stage2Players)
        {
            if (player.IsAlive())
            {
                Main.PlayerStates[player.PlayerId].deathReason = PlayerState.DeathReason.Drained;
                player.KillWithoutBody(player);
            }

            DeathsCounter++;
            Stage2Players.Remove(player);
        }
        foreach (var player in Stage1Players)
        {
            player.SetNewOutfit(GrayOutfit);
            Stage2Players.Add(player);
            Stage1Players.Remove(player);  
        }
        if (KillCooldown.GetFloat() - (KillCooldownReduction.GetFloat() * DeathsCounter) <= MinimumKillCooldown.GetFloat()) TheSlaad.SetKillCooldown(MinimumKillCooldown.GetFloat());
        else TheSlaad.SetKillCooldown(KillCooldown.GetFloat() - (KillCooldownReduction.GetFloat() * DeathsCounter));
        TheSlaad.ResetKillCooldown();
    }
}
