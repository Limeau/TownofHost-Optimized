using System.Collections.Generic;
using System.Linq;
using static TOHO.Options;
using static TOHO.Translator;

namespace TOHO.Roles.Crewmate;

internal class Mage : RoleBase
{
    //===========================SETUP================================\\
    public override CustomRoles Role => CustomRoles.Mage;
    private const int Id = 38900;
    public override CustomRoles ThisRoleBase => CustomRoles.Impostor;
    public override Custom_RoleType ThisRoleType => Custom_RoleType.CrewmateKilling;
    public override bool TOHORole => true;
    //==================================================================\\
    public static OptionItem KillCooldown;

    public static List<PlayerControl> ActiveSpells = [];
    
    public override void SetupCustomOption()
    {
        SetupRoleOptions(Id, TabGroup.CrewmateRoles, CustomRoles.Mage);
        KillCooldown = FloatOptionItem.Create(Id + 10, GeneralOption.KillCooldown, new(1f, 50f, 1f), 20f, TabGroup.CrewmateRoles, true)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Mage])
            .SetValueFormat(OptionFormat.Seconds);
    }

    public override bool CanUseKillButton(PlayerControl pc)
    {
        return true;
    }

    public override void SetKillCooldown(byte id)
    {
        Main.AllPlayerKillCooldown[id] = KillCooldown.GetFloat();
    }

    public override bool OnCheckMurderAsKiller(PlayerControl killer, PlayerControl target)
    {
        if (target.IsPlayerCrewmateTeam() && !ActiveSpells.Contains(killer)) ActiveSpells.Add(killer);
        else if (!target.IsPlayerCrewmateTeam()) ActiveSpells.Add(target);
        killer.RpcGuardAndKill();
        killer.SetKillCooldown(KillCooldown.GetFloat());
        return false;
    }

    public override void AfterMeetingTasks()
    {
        foreach (var player in Main.AllAlivePlayerControls)
        {
            if (player.GetCustomRole() == CustomRoles.Mage)
            {
                foreach (var deadlol in ActiveSpells)
                {
                    deadlol.KillWithoutBody(deadlol);
                    deadlol.SetDeathReason(PlayerState.DeathReason.Curse);
                }
            }
        }
        ActiveSpells.Clear();
    }
    
    public override void OnMeetingHudStart(PlayerControl pc)
    {
        if (pc.IsAlive() && ActiveSpells.Any())
            MeetingHudStartPatch.AddMsg(string.Format(Translator.GetString("MageSpellNotice"), ActiveSpells.ToString()), 255, Utils.ColorString(Utils.GetRoleColor(CustomRoles.Mage), GetString("MageTitle")));
    }
}
