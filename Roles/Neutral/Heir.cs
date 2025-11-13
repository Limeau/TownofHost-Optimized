using TOHO.Roles.Core;
using static TOHO.Options;

namespace TOHO.Roles.Neutral;
internal class Heir : RoleBase
{
    //===========================SETUP================================\\
    public override CustomRoles Role => CustomRoles.Heir;
    private const int Id = 36600;
    public static bool HasEnabled => CustomRoleManager.HasEnabled(CustomRoles.Heir);
    public override CustomRoles ThisRoleBase => CustomRoles.Engineer;
    public override Custom_RoleType ThisRoleType => Custom_RoleType.NeutralBenign;
    //==================================================================\\
    private static OptionItem CanTargetImpostor;
    private static OptionItem CanTargetNeutral;
    private static OptionItem CanTargetCoven;
    private static OptionItem CanTargetCrewmate;
    
    public static HashSet<byte> TargetList = [];
    private byte TargetId;
    public override void SetupCustomOption()
    {
        SetupRoleOptions(Id, TabGroup.NeutralRoles, CustomRoles.Heir);
        CanTargetImpostor = BooleanOptionItem.Create(Id + 10, "LawyerCanTargetImpostor", true, TabGroup.NeutralRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Heir]);
        CanTargetNeutral = BooleanOptionItem.Create(Id + 11, "HeirCanTargetNeutral", false, TabGroup.NeutralRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Heir]);
        CanTargetCoven = BooleanOptionItem.Create(Id + 12, "LawyerCanTargetCoven", false, TabGroup.NeutralRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Heir]);
        CanTargetCrewmate = BooleanOptionItem.Create(Id + 13, "LawyerCanTargetCrewmate", false, TabGroup.NeutralRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Heir]);
    }
    public override void Init()
    {
        TargetId = byte.MaxValue;
        TargetList.Clear();
    }
    public override void Add(byte playerId)
    {
        var heir = _Player;
        if (AmongUsClient.Instance.AmHost && heir.IsAlive())
        {
            CustomRoleManager.CheckDeadBodyOthers.Add(OthersAfterPlayerDeathTask);

            List<PlayerControl> targetList = [];
            var rand = IRandom.Instance;
            foreach (var target in Main.AllPlayerControls)
            {
                if (playerId == target.PlayerId) continue;
                else if (TargetList.Contains(target.PlayerId)) continue;
                else if (!CanTargetImpostor.GetBool() && target.Is(Custom_Team.Impostor)) continue;
                else if (!CanTargetNeutral.GetBool() && target.IsPlayerNeutralTeam()) continue;
                else if (!CanTargetCoven.GetBool() && target.Is(Custom_Team.Coven)) continue;
                else if (!CanTargetCrewmate.GetBool() && target.Is(Custom_Team.Crewmate)) continue;
                else if (!CanTargetNeutral.GetBool() && target.Is(Custom_Team.Neutral)) continue;
                if (target.GetCustomRole() is CustomRoles.GM or CustomRoles.SuperStar or CustomRoles.NiceMini or CustomRoles.EvilMini) continue;
                if (heir.Is(CustomRoles.Lovers) && target.Is(CustomRoles.Lovers)) continue;

                targetList.Add(target);
            }

            if (targetList.Any())
            {
                var selectedTarget = targetList.RandomElement();
                TargetId = selectedTarget.PlayerId;
                TargetList.Add(selectedTarget.PlayerId);

                Logger.Info($"{heir?.GetNameWithRole()}:{selectedTarget.GetNameWithRole()}", "heir");
            }
            else
            {
                Logger.Info($"Wow, not target for heir to select! Changing heir role to other", "heir");

                // Unable to find a target? Try to turn to opportunist
                var changedRole = CustomRoles.Opportunist;

                heir.RpcChangeRoleBasis(changedRole);
                heir.RpcSetCustomRole(changedRole);
            }
        }
    }
    private void OthersAfterPlayerDeathTask(PlayerControl killer, PlayerControl target, bool inMeeting)
    {
        if (TargetId == target.PlayerId)
        {
            _Player.RpcSetCustomRole(target.GetCustomRole());
            if (_Player.IsAlive()) _Player.RpcChangeRoleBasis(target.GetCustomRole());
        }
    }
    
    public override string GetMarkOthers(PlayerControl seer, PlayerControl target, bool isForMeeting = false)
    {
        if (TargetId == byte.MaxValue) return string.Empty;

        if ((!seer.IsAlive() || seer.Is(CustomRoles.Heir)) && TargetId == target.PlayerId)
        {
            return Utils.ColorString(Utils.GetRoleColor(CustomRoles.Heir), "♦");
        }
        else if (seer.IsAlive() && TargetId == seer.PlayerId && _state.PlayerId == target.PlayerId)
        {
            return Utils.ColorString(Utils.GetRoleColor(CustomRoles.Heir), "♦");
        }

        return string.Empty;
    }
    
    public override bool KnowRoleTarget(PlayerControl player, PlayerControl target)
    {
        return player.Is(CustomRoles.Heir) && TargetId == target.PlayerId;
    }
}
