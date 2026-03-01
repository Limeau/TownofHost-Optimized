using TOHO.Modules;
using UnityEngine;

namespace TOHO.Roles.Impostor;

internal class Crow : RoleBase
{
    //===========================SETUP================================\\
    public override CustomRoles Role => CustomRoles.Crow;
    private const int Id = 38400;
    public override CustomRoles ThisRoleBase => CustomRoles.Impostor;
    public override Custom_RoleType ThisRoleType => Custom_RoleType.ImpostorSupport;
    //==================================================================\\
    private static OptionItem KillCooldown;
    private static OptionItem AbilityUses;
    private static List<PlayerControl> playerList = [];
    public override void SetupCustomOption()
    {
        Options.SetupRoleOptions(Id, TabGroup.ImpostorRoles, CustomRoles.Crow);
        KillCooldown = FloatOptionItem.Create(Id + 10, GeneralOption.KillCooldown, new(5f, 180f, 2.5f), 30f, TabGroup.ImpostorRoles, false)
            .SetParent(Options.CustomRoleSpawnChances[CustomRoles.Crow])
            .SetValueFormat(OptionFormat.Seconds);
        AbilityUses = IntegerOptionItem.Create(Id + 11, "CrowUses", new(1, 5, 1), 3, TabGroup.ImpostorRoles, false)
            .SetParent(Options.CustomRoleSpawnChances[CustomRoles.Crow])
            .SetValueFormat(OptionFormat.Times);
    }

    public override void Add(byte playerId)
    {
        var player = Utils.GetPlayerById(playerId);
        player.SetAbilityUseLimit(AbilityUses.GetInt());
    }

    public override bool OnCheckMurderAsKiller(PlayerControl killer, PlayerControl target)
    {
        if (killer.GetAbilityUseLimit() <= 0) return true;
        killer.RpcRemoveAbilityUse();
        playerList.Add(killer);
        killer.RpcChangeRoleBasis(target.GetCustomRole());
        killer.RpcSetCustomRole(target.GetCustomRole());
        killer.RpcSetCustomRole(CustomRoles.Madmate);
        killer.Notify(string.Format(Translator.GetString("CrowNotify"), target.GetRealName(), Translator.GetString($"{target.GetCustomRole().ToString()}")));
        return true;
    }

    public static void UnAfterMeetingTasks()
    {
        foreach (var player in playerList) if (player.IsAlive())
        {
            player.RpcChangeRoleBasis(CustomRoles.Crow);
            player.RpcSetCustomRole(CustomRoles.Crow);
            Main.PlayerStates[player.PlayerId].RemoveSubRole(CustomRoles.Madmate);
            player.ResetKillCooldown();
            player.SetKillCooldown();
            playerList.Remove(player);
        }
    }

    public override void SetKillCooldown(byte id) => Main.AllPlayerKillCooldown[id] = KillCooldown.GetFloat();
}
