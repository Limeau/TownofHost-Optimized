using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using UnityEngine;
using TOHO.Modules;
using static TOHO.Options;

namespace TOHO.Roles.Impostor;

internal class Lifestealer : RoleBase
{
    //===========================SETUP================================\\
    public override CustomRoles Role => CustomRoles.Lifestealer;
    private const int Id = 45900;
    public override CustomRoles ThisRoleBase => CustomRoles.Shapeshifter;
    public override Custom_RoleType ThisRoleType => Custom_RoleType.ImpostorConcealing;
    public override bool TOHORole => true;
    public override bool NewRole => true;
    public override string IdeaRole => "balloons0528";
    //==================================================================\\

    private static OptionItem KillCooldown;

    private static bool StealMode;
    
    public override void SetupCustomOption()
    {
        SetupRoleOptions(Id, TabGroup.ImpostorRoles, CustomRoles.Lifestealer);
        KillCooldown = FloatOptionItem.Create(Id + 10, GeneralOption.KillCooldown, new(0f, 180f, 2.5f), 20f, TabGroup.ImpostorRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Lifestealer])
            .SetValueFormat(OptionFormat.Seconds);
    }

    public override void Add(byte playerId)
    {
        StealMode = false;
    }

    public override void SetKillCooldown(byte id) => Main.AllPlayerKillCooldown[id] = KillCooldown.GetFloat();

    public override void ApplyGameOptions(IGameOptions opt, byte playerId)
    {
        AURoleOptions.ShapeshifterCooldown = 1f;
    }

    public override bool OnCheckReportDeadBody(PlayerControl reporter, NetworkedPlayerInfo deadBody, PlayerControl killer)
    {
        if (!reporter.Is(CustomRoles.Lifestealer) || !StealMode) return true;
        
        var target = deadBody.Object;
        string kname = reporter.GetRealName(isMeeting: true);
        string tname = target.GetRealName(isMeeting: true);
        var reporterSkin = new NetworkedPlayerInfo.PlayerOutfit()
            .Set(kname, reporter.CurrentOutfit.ColorId, reporter.CurrentOutfit.HatId, reporter.CurrentOutfit.SkinId, reporter.CurrentOutfit.VisorId, reporter.CurrentOutfit.PetId, reporter.CurrentOutfit.NamePlateId);
        var reporterLvl = reporter.Data.PlayerLevel;
        var targetSkin = new NetworkedPlayerInfo.PlayerOutfit()
            .Set(tname, target.CurrentOutfit.ColorId, target.CurrentOutfit.HatId, target.CurrentOutfit.SkinId, target.CurrentOutfit.VisorId, target.CurrentOutfit.PetId, target.CurrentOutfit.NamePlateId);
        var targetLvl = target.Data.PlayerLevel;
        target.SetNewOutfit(reporterSkin, newLevel: reporterLvl);
        Main.OvverideOutfit[target.PlayerId] = (reporterSkin, Main.PlayerStates[reporter.PlayerId].NormalOutfit.PlayerName);
        Logger.Info("Changed target skin", "Lifestealer");
        reporter.SetNewOutfit(targetSkin, newLevel: targetLvl);
        Main.OvverideOutfit[reporter.PlayerId] = (targetSkin, Main.PlayerStates[target.PlayerId].NormalOutfit.PlayerName);
        Logger.Info("Changed reporter skin", "Lifestealer");
        RPC.SyncAllPlayerNames();
        Main.Instance.StartCoroutine(Utils.NotifyEveryoneAsync(speed: 5));
        return false;
    }

    private static void SwitchMode(PlayerControl player)
    {
        if (StealMode) player.Notify("Mode: Steal");
        else player.Notify("Mode: Report");
    }

    public override void UnShapeShiftButton(PlayerControl shapeshifter)
    {
        StealMode = !StealMode;
        SwitchMode(shapeshifter);
    }

    public override void AfterMeetingTasks()
    {
        StealMode = false;
        SwitchMode(_Player);
    }
}