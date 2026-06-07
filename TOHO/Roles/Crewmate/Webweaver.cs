using System.Collections.Generic;
using AmongUs.GameOptions;
using TOHO.Modules;
using static TOHO.Options;
using static TOHO.Translator;

namespace TOHO.Roles.Crewmate;

internal class Webweaver : RoleBase
{
    //===========================SETUP================================\\
    public override CustomRoles Role => CustomRoles.Webweaver;
    private const int Id = 42700;
    public override CustomRoles ThisRoleBase => CustomRoles.Engineer;
    public override Custom_RoleType ThisRoleType => Custom_RoleType.CrewmateHindering;
    public override bool BlockMoveInVent(PlayerControl pc) => true;

    public override bool TOHORole => true;

    public override string IdeaRole => "bearyyyy_cocoa";
    //==================================================================\\

    private static OptionItem WebweaverTrapTime;
    private static OptionItem VentCooldown;

    public readonly HashSet<int> BombedVents = [];

    public override void SetupCustomOption()
    {
        SetupRoleOptions(Id, TabGroup.CrewmateRoles, CustomRoles.Webweaver);
        WebweaverTrapTime = FloatOptionItem.Create(Id + 10, "WebweaverTrapTime", new(10f, 40f, 1f), 20f, TabGroup.CrewmateRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.Webweaver])
            .SetValueFormat(OptionFormat.Seconds);
        VentCooldown = FloatOptionItem.Create(Id + 11, GeneralOption.EngineerBase_VentCooldown, new(10f, 40f, 1f), 20f, TabGroup.CrewmateRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.Webweaver])
            .SetValueFormat(OptionFormat.Seconds);
    }
    public override void Init()
    {
        BombedVents.Clear();
    }
    public override void ApplyGameOptions(IGameOptions opt, byte playerId)
    {
        AURoleOptions.EngineerInVentMaxTime = 1;
        AURoleOptions.EngineerCooldown = VentCooldown.GetFloat();
    }

    public override bool OnCoEnterVentOthers(PlayerPhysics physics, int ventId)
    {
        var prev = physics.myPlayer.GetCustomRole();
        if (BombedVents.Contains(ventId))
        {
            physics.myPlayer.RpcChangeRoleBasis(CustomRoles.CrewmateTOHO);
            _ = new LateTask(() => { physics.myPlayer.RpcChangeRoleBasis(prev); }, WebweaverTrapTime.GetFloat(),
                "Webweaver Remove Trap");
        }            
        return true;
    }

    public override void OnEnterVent(PlayerControl pc, Vent vent)
    {
        var prev = pc.GetCustomRole();
        if (BombedVents.Contains(vent.Id))
        {
            pc.RpcChangeRoleBasis(CustomRoles.CrewmateTOHO);
            _ = new LateTask(() => { pc.RpcChangeRoleBasis(prev); }, WebweaverTrapTime.GetFloat(),
                "Webweaver Remove Trap");
            return;
        }            
        _ = new LateTask(() =>
        {
            BombedVents.Add(vent.Id);
        }, 2f, "Webweaver Add Trap");
    }

    public override void AfterMeetingTasks()
    {
        BombedVents.Clear();
    }
}