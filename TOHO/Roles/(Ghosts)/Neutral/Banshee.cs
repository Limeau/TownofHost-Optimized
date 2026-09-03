using AmongUs.GameOptions;
using TOHO.Roles.Core;
using static TOHO.Options;

namespace TOHO.Roles._Ghosts_.Neutral;

internal class Banshee : RoleBase
{
    //===========================SETUP================================\\
    private const int Id = 47600;
    public static bool HasEnabled => CustomRoleManager.HasEnabled(CustomRoles.Banshee);
    public override CustomRoles Role => CustomRoles.Banshee;
    public override CustomRoles ThisRoleBase => CustomRoles.GuardianAngel;
    public override Custom_RoleType ThisRoleType => Custom_RoleType.NeutralGhosts;
    public override bool TOHORole => true;
    public override bool NewRole => true;
    //==================================================================\\

    public static OptionItem MeetingsToWin;
    public int MeetingCount = 0;
    public override void SetupCustomOption()
    {
        SetupSingleRoleOptions(Id, TabGroup.NeutralRoles, CustomRoles.Banshee);
        MeetingsToWin = IntegerOptionItem.Create(Id + 10, "BansheeMeetingsToWin", new(1, 10, 1), 4, TabGroup.NeutralRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Banshee])
            .SetValueFormat(OptionFormat.Times);
    }
    public override void Init()
    {
        MeetingCount = 0;
    }

    public override void OnMeetingHudStart(PlayerControl pc)
    {
        MeetingCount += 1;
        if (MeetingCount >= MeetingsToWin.GetInt())
        {
            if (!CustomWinnerHolder.CheckForConvertedWinner(pc.PlayerId))
            {
                CustomWinnerHolder.ResetAndSetWinner(CustomWinner.Banshee);
                CustomWinnerHolder.WinnerIds.Add(pc.PlayerId);
            }
            return;
        }
        Utils.SendMessage(string.Format(Translator.GetString("BansheeGoingToWin"), MeetingsToWin.GetInt() - MeetingCount));
    }

    public override void ApplyGameOptions(IGameOptions opt, byte playerId)
    {
        AURoleOptions.GuardianAngelCooldown = 300f;
        AURoleOptions.ProtectionDurationSeconds = 0f;
    }
    public override bool OnCheckProtect(PlayerControl killer, PlayerControl target)
    {
        return false;
    }
}