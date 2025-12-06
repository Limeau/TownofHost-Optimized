using AmongUs.GameOptions;
using Rewired.ComponentControls;
using TOHO.Modules;
using static TOHO.Options;
using static TOHO.Translator;

namespace TOHO.Roles.Impostor;

internal class Magnet : RoleBase
{
    //===========================SETUP================================\\
    public override CustomRoles Role => CustomRoles.Magnet;
    private const int Id = 37800;
    public override CustomRoles ThisRoleBase => CustomRoles.Impostor;
    public override Custom_RoleType ThisRoleType => Custom_RoleType.ImpostorSupport;
    //==================================================================\\
    private static PlayerControl Mark;
    private static OptionItem KillCooldown;
    public override void SetupCustomOption()
    {
        SetupSingleRoleOptions(Id, TabGroup.ImpostorRoles, CustomRoles.Magnet);
        KillCooldown = FloatOptionItem.Create(Id + 10, GeneralOption.KillCooldown, new(1f, 60f, 1f), 20f, TabGroup.ImpostorRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Magnet])
            .SetValueFormat(OptionFormat.Seconds);
    }
    public override void Add(byte playerId)
    {
        // Double Trigger
        var pc = Utils.GetPlayerById(playerId);
        pc.AddDoubleTrigger();
    }
    public override void SetKillCooldown(byte id) => Main.AllPlayerKillCooldown[id] = KillCooldown.GetFloat();
    public override bool OnCheckMurderAsKiller(PlayerControl killer, PlayerControl target)
    {
        if (killer.CheckDoubleTrigger(target, () => { }))
        {
            return true;
        }

        if (Mark == null)
        {
            Mark = target;
            killer.RpcGuardAndKill();
            killer.ResetKillCooldown();
        }
        else
        {
            target.RpcTeleport(Mark.GetCustomPosition());
            Mark = null;
        }
        
        return false;
    }
}
