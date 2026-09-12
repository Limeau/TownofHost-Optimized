using AmongUs.GameOptions;
using TOHO.Roles.Core;
using static TOHO.Options;

namespace TOHO.Roles._Ghosts_.Neutral;

internal class Owl : RoleBase
{
    //===========================SETUP================================\\
    private const int Id = 47700;
    public static bool HasEnabled => CustomRoleManager.HasEnabled(CustomRoles.Owl);
    public override CustomRoles Role => CustomRoles.Owl;
    public override CustomRoles ThisRoleBase => CustomRoles.GuardianAngel;
    public override Custom_RoleType ThisRoleType => Custom_RoleType.NeutralGhosts;
    public override bool TOHORole => true;
    public override bool NewRole => true;
    //==================================================================\\

    public static int WinTeam;
    
    public override void SetupCustomOption()
    {
        SetupSingleRoleOptions(Id, TabGroup.NeutralRoles, CustomRoles.Owl);
    }
    public override void Init()
    {
        WinTeam = 0;
    }
    public override void ApplyGameOptions(IGameOptions opt, byte playerId)
    {
        AURoleOptions.GuardianAngelCooldown = 0f;
        AURoleOptions.ProtectionDurationSeconds = 0f;
    }
    public override bool OnCheckProtect(PlayerControl killer, PlayerControl target)
    {
        if (WinTeam == 0)
        {
            if (target.IsPlayerImpostorTeam()) WinTeam = 1;
            if (target.IsPlayerCrewmateTeam()) WinTeam = 2;
            if (target.IsPlayerNeutralTeam()) WinTeam = 3;
            if (target.IsPlayerCovenTeam()) WinTeam = 4;
            if (target.IsPlayerNeutralTeam() && target.IsNeutralApocalypse()) WinTeam = 5;
            killer.Notify("Team Selected");
        }
        return false;
    }

    public static bool CheckWinnerTeam(CustomWinner team)
    {
        switch (WinTeam)
        {
            case 0:
                break;
            case 1:
                if (team == CustomWinner.Crewmate) return true;
                break;
            case 2:
                if (team == CustomWinner.Impostor) return true;
                break;
            case 3:
                if (team != CustomWinner.Crewmate && team != CustomWinner.Impostor && team != CustomWinner.Coven && team != CustomWinner.Apocalypse) return true;
                break;
            case 4:
                if (team == CustomWinner.Coven) return true;
                break;
            case 5:
                if (team == CustomWinner.Apocalypse) return true;
                break;
        }
        return false;
    }
}