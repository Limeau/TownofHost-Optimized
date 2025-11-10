using System.Text;
using AmongUs.GameOptions;
using UnityEngine;
using static TOHO.Options;

namespace TOHO.Roles.Neutral;

internal class Atoner : RoleBase
{
    //===========================SETUP================================\\
    public override CustomRoles Role => CustomRoles.Atoner;
    private const int Id = 36700;
    public override bool IsDesyncRole => true;
    public override CustomRoles ThisRoleBase => CustomRoles.Impostor;
    public override Custom_RoleType ThisRoleType => Custom_RoleType.NeutralKilling;
    //==================================================================\\

    private static OptionItem StartingChance;
    private static OptionItem IncreasedChance;
    private static OptionItem KillCooldown;

    private static int CurrentChance;
    
    public override void SetupCustomOption()
    {
        SetupSingleRoleOptions(Id, TabGroup.NeutralRoles, CustomRoles.Atoner, 1, zeroOne: false);
        KillCooldown = FloatOptionItem.Create(Id + 10, GeneralOption.KillCooldown, new(1f, 60f, 1f), 10f, TabGroup.NeutralRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.Atoner])
            .SetValueFormat(OptionFormat.Seconds);
        StartingChance = IntegerOptionItem.Create(Id + 11, "StartingChance367", new(0, 100, 5), 20, TabGroup.NeutralRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.Atoner])
            .SetValueFormat(OptionFormat.Percent);
        IncreasedChance = IntegerOptionItem.Create(Id + 12, "IncreasedChance367", new(0, 100, 5), 20, TabGroup.NeutralRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.Atoner])
            .SetValueFormat(OptionFormat.Percent);
    }
    public override void SetKillCooldown(byte id) => Main.AllPlayerKillCooldown[id] = KillCooldown.GetFloat();
    public override bool CanUseKillButton(PlayerControl pc) => true;
    public override bool CanUseImpostorVentButton(PlayerControl pc) => true;

    public override void Add(byte playerId)
    {
        CurrentChance = StartingChance.GetInt();
    }

    public override bool OnCheckMurderAsKiller(PlayerControl killer, PlayerControl target)
    {
        var rand = IRandom.Instance;
        if (rand.Next(1, 100) <= CurrentChance)
        {
            new LateTask(() => { killer.RpcMurderPlayer(killer); }, 1f, "Atoner Kill");
        }
        else CurrentChance += IncreasedChance.GetInt();
        return true;
    }

    public override string GetProgressText(byte playerId, bool comms)
    {
        var ProgressText = new StringBuilder();
        if (CurrentChance <= 20) ProgressText.Append(Utils.ColorString(Color.cyan, $"{CurrentChance}%") + $"");
        if (CurrentChance < 50 && CurrentChance > 20) ProgressText.Append(Utils.ColorString(Color.yellow, $"{CurrentChance}%") + $"");
        if (CurrentChance >= 50) ProgressText.Append(Utils.ColorString(Color.red, $"{CurrentChance}%") + $"");
        return ProgressText.ToString();
    }
}
