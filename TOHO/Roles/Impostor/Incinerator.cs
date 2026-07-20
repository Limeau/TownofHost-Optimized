using TOHO.Modules;

namespace TOHO.Roles.Impostor;

internal class Incinerator : RoleBase
{
    //===========================SETUP================================\\
    public override CustomRoles Role => CustomRoles.Incinerator;
    private const int Id = 46000;
    public override CustomRoles ThisRoleBase => CustomRoles.Impostor;
    public override Custom_RoleType ThisRoleType => Custom_RoleType.ImpostorKilling;
    public override bool TOHORole => true;
    public override bool NewRole => true;
    public override string IdeaRole => "puffyxavy";

    //==================================================================\\

    private static OptionItem IncineratorCD;

    public override void SetupCustomOption()
    {
        Options.SetupRoleOptions(Id, TabGroup.ImpostorRoles, CustomRoles.Incinerator);
        IncineratorCD = FloatOptionItem.Create(Id + 2, GeneralOption.KillCooldown, new(0f, 180f, 2.5f), 20f, TabGroup.ImpostorRoles, false)
            .SetParent(Options.CustomRoleSpawnChances[CustomRoles.Incinerator])
            .SetValueFormat(OptionFormat.Seconds);
    }

    public override void SetKillCooldown(byte id) => Main.AllPlayerKillCooldown[id] = IncineratorCD.GetFloat();

    private static readonly NetworkedPlayerInfo.PlayerOutfit AshOutfit = new NetworkedPlayerInfo.PlayerOutfit().Set("ASHES", 6, "", "", "", "", "");

    public override bool OnCheckMurderAsKiller(PlayerControl killer, PlayerControl target)
    {
        target.SetNewOutfit(AshOutfit);
        Main.OvverideOutfit[target.PlayerId] = (AshOutfit, "ASHES");
        RPC.SyncAllPlayerNames();
        Main.Instance.StartCoroutine(Utils.NotifyEveryoneAsync(speed: 5));
        return true;
    }
}