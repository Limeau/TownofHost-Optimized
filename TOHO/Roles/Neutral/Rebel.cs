using System.Linq;
using TOHO.Roles.Core;
using static TOHO.Translator;

namespace TOHO.Roles.Neutral;
internal class RebelLeader : RoleBase
{
    //===========================SETUP================================\\
    public override CustomRoles Role => CustomRoles.RebelLeader;
    private const int Id = 46700;
    public static bool HasEnabled => CustomRoleManager.HasEnabled(CustomRoles.RebelLeader);
    public override CustomRoles ThisRoleBase => CustomRoles.Impostor;
    public override Custom_RoleType ThisRoleType => Custom_RoleType.NeutralKilling;
    public override bool TOHORole => true;
    public override string IdeaRole => "scary_trickster";

    //==================================================================\\

    public override void SetupCustomOption()
    {
        Options.SetupSingleRoleOptions(Id, TabGroup.NeutralRoles, CustomRoles.RebelLeader);
    }

    private static bool RebelAlive;
    public static PlayerControl Rebel1;
    public static PlayerControl Rebel2;

    public override void Add(byte playerId)
    {
        RebelAlive = true;
        Rebel1 = Main.AllAlivePlayerControls.Where(x => x.PlayerId != playerId).RandomElement();
        Rebel2 = Main.AllAlivePlayerControls.Where(x => x.PlayerId != playerId && x != Rebel1).RandomElement();
    }

    public override bool OnCheckMurderAsTarget(PlayerControl killer, PlayerControl target)
    {
        if (RebelAlive)
        {
            killer.RpcGuardAndKill();
            return false;
        }
        return true;
    }

    public override bool CanUseKillButton(PlayerControl pc)
    {
        return !RebelAlive;
    }

    public override void OnFixedUpdate(PlayerControl player, bool lowLoad, long nowTime, int timerLowLoad)
    {
        if (Rebel1.IsAlive() && Rebel2.IsAlive()) return;
        if (!Rebel1.IsAlive() && !Rebel2.IsAlive()) return;
        if (!Rebel1.IsAlive() && Rebel2.IsAlive())
        {
            Rebel2.SetDeathReason(PlayerState.DeathReason.FollowingSuicide);
            Rebel2.KillWithoutBody(Rebel2);
            RebelAlive = false;
        }
        if (Rebel1.IsAlive() && !Rebel2.IsAlive())
        {
            Rebel1.SetDeathReason(PlayerState.DeathReason.FollowingSuicide);
            Rebel1.KillWithoutBody(Rebel1);
            RebelAlive = false;
        }
    }

    public override bool KnowRoleTarget(PlayerControl seer, PlayerControl target)
    {
        if (target == Rebel1 && seer == Rebel2) return true;
        if (target == Rebel2 && seer == Rebel1) return true;
        if (target == Rebel1 && seer.Is(CustomRoles.RebelLeader)) return true;
        if (target == Rebel2 && seer.Is(CustomRoles.RebelLeader)) return true;
        if (target.Is(CustomRoles.RebelLeader) && seer == Rebel1) return true;
        if (target.Is(CustomRoles.RebelLeader) && seer == Rebel2) return true;
        
        return false;
    }
}