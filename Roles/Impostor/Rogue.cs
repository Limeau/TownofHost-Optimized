using static TOHO.Utils;

namespace TOHO.Roles.Impostor;

internal class Rogue : RoleBase
{
    //===========================SETUP================================\\
    public override CustomRoles Role => CustomRoles.Rogue;
    private const int Id = 37500;
    public override CustomRoles ThisRoleBase => CustomRoles.Impostor;
    public override Custom_RoleType ThisRoleType => Custom_RoleType.ImpostorKilling;
    //==================================================================\\

    private static OptionItem RogueCD;
    private static OptionItem RogueSpeedBoost;
    private static OptionItem BoostDuration;
    
    private static readonly Dictionary<byte, float> TempSpeed = [];

    public override void SetupCustomOption()
    {
        Options.SetupRoleOptions(Id, TabGroup.ImpostorRoles, CustomRoles.Rogue);
        RogueCD = FloatOptionItem.Create(Id + 2, GeneralOption.KillCooldown, new(0f, 180f, 2.5f), 20f, TabGroup.ImpostorRoles, false)
            .SetParent(Options.CustomRoleSpawnChances[CustomRoles.Rogue])
            .SetValueFormat(OptionFormat.Seconds);
        RogueSpeedBoost = FloatOptionItem.Create(Id + 10, "SpeedBoost375", new(1f, 5f, 0.25f), 3f, TabGroup.ImpostorRoles, false).SetParent(Options.CustomRoleSpawnChances[CustomRoles.Rogue])
            .SetValueFormat(OptionFormat.Multiplier);
        BoostDuration = FloatOptionItem.Create(Id + 11, "Duration375", new(1f, 5f, 1f), 3f, TabGroup.ImpostorRoles, false).SetParent(Options.CustomRoleSpawnChances[CustomRoles.Rogue])
            .SetValueFormat(OptionFormat.Seconds);
    }

    public override void SetKillCooldown(byte id) => Main.AllPlayerKillCooldown[id] = RogueCD.GetFloat();

    public override void Init()
    {
        TempSpeed.Clear();
    }
    
    public override void Add(byte playerId)
    {
        TempSpeed[playerId] = Main.AllPlayerSpeed[playerId];
    }
    
    public override bool CanUseKillButton(PlayerControl pc) => true;

    public override bool OnCheckMurderAsKiller(PlayerControl killer, PlayerControl target)
    {
        
        Main.AllPlayerSpeed[killer.PlayerId] = RogueSpeedBoost.GetFloat();
        killer.MarkDirtySettings();
        new LateTask(() =>
        {
            float tmpFloat = TempSpeed[killer.PlayerId];
            Main.AllPlayerSpeed[killer.PlayerId] = Main.AllPlayerSpeed[killer.PlayerId] - RogueSpeedBoost.GetFloat() + tmpFloat;
            killer.MarkDirtySettings();
        }, BoostDuration.GetFloat(), "Rogue speed boost");
        return true;
    }
}
