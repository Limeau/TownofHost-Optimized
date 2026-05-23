using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using TOHO.Roles.Core;
using TOHO.Roles.Neutral;
using UnityEngine;
using static TOHO.Options;
using static TOHO.Translator;

namespace TOHO.Roles.Impostor;

internal class Bane : RoleBase
{
    //===========================SETUP================================\\
    private const int Id = 32700;
    private static readonly HashSet<byte> playerIdList = [];
    public static bool HasEnabled => playerIdList.Any();
    public override CustomRoles Role => CustomRoles.Bane;
    public override CustomRoles ThisRoleBase => CustomRoles.Shapeshifter;
    public override Custom_RoleType ThisRoleType => Custom_RoleType.ImpostorHindering;
    public override bool TOHORole => true;
    //==================================================================\\
    public override void SetAbilityButtonText(HudManager hud, byte playerId)
    {
        hud.AbilityButton.OverrideText(GetString("BaneButtonText"));
    }

    private static OptionItem ShapeshiftCooldown;
    private static OptionItem MaxTrapCount;
    private static OptionItem TrapMaxPlayerCountOpt;
    private static OptionItem TrapDurationOpt;
    private static OptionItem TrapRadius;

    private static HashSet<BaneTrap> Traps = [];
    private static readonly HashSet<byte> ReducedVisionPlayers = [];

    private static float DefaultSpeed = new();
    public static float TrapMaxPlayerCount = new();
    public static float TrapDuration = new();

    public override void SetupCustomOption()
    {
        SetupRoleOptions(Id, TabGroup.ImpostorRoles, CustomRoles.Bane);
        ShapeshiftCooldown = FloatOptionItem.Create(Id + 10, "BaneTrapCooldown", new(1f, 180f, 1f), 20f, TabGroup.ImpostorRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.Bane])
            .SetValueFormat(OptionFormat.Seconds);
        MaxTrapCount = FloatOptionItem.Create(Id + 11, "BaneMaxTrapCount", new(1f, 5f, 1f), 1f, TabGroup.ImpostorRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.Bane])
            .SetValueFormat(OptionFormat.Times);
        TrapMaxPlayerCountOpt = FloatOptionItem.Create(Id + 12, "BaneTrapMaxPlayerCount", new(1f, 15f, 1f), 3f, TabGroup.ImpostorRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.Bane])
            .SetValueFormat(OptionFormat.Times);
        TrapDurationOpt = FloatOptionItem.Create(Id + 13, "BaneTrapDuration", new(5f, 180f, 1f), 30f, TabGroup.ImpostorRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.Bane])
            .SetValueFormat(OptionFormat.Seconds);
        TrapRadius = FloatOptionItem.Create(Id + 14, "BaneTrapRadius", new(0.5f, 3f, 0.5f), 1f, TabGroup.ImpostorRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.Bane])
            .SetValueFormat(OptionFormat.Multiplier);
    }
    public override void Init()
    {
        playerIdList.Clear();
        Traps.Clear();
        ReducedVisionPlayers.Clear();
        DefaultSpeed = new();
        TrapMaxPlayerCount = new();
        TrapDuration = new();
    }
    public override void Add(byte playerId)
    {
        playerIdList.Add(playerId);
        DefaultSpeed = Main.AllPlayerSpeed[playerId];

        TrapMaxPlayerCount = TrapMaxPlayerCountOpt.GetFloat();
        TrapDuration = TrapDurationOpt.GetFloat();
    }

    public override void ApplyGameOptions(IGameOptions opt, byte playerId)
    {
        AURoleOptions.ShapeshifterCooldown = ShapeshiftCooldown.GetFloat();
    }

    public override void AfterMeetingTasks()
    {
        Traps.Clear();
    }

    public override void UnShapeShiftButton(PlayerControl shapeshifter)
    {
        // Remove inactive traps so there is room for new traps
        Traps = Traps.Where(a => a.IsActive).ToHashSet();

        Vector2 position = shapeshifter.transform.position;
        var playerTraps = Traps.Where(a => a.BanePlayerId == shapeshifter.PlayerId).ToArray();
        if (playerTraps.Length < MaxTrapCount.GetInt())
        {
            var newtrap = new BaneTrap
            {
                BanePlayerId = shapeshifter.PlayerId,
                Location = position,
                PlayersTrapped = [],
                IsActive = true
            };
            Traps.Add(newtrap);
            new LateTask(() =>
            {
                newtrap.IsActive = false;
            }, TrapDuration, "Bane Trap Removal");
        }

        shapeshifter.Notify(GetString("RejectShapeshift.AbilityWasUsed"), time: 2f);
    }

    public override void OnFixedUpdate(PlayerControl pc, bool lowLoad, long nowTime, int timerLowLoad)
    {
        foreach (var player in Main.AllAlivePlayerControls)
        {
            
            if (Pelican.IsEaten(player.PlayerId) || !player.IsAlive() || player.Is(CustomRoles.Bane)) return;

            Vector2 position = player.transform.position;

            foreach (var trap in Traps.Where(a => a.IsActive).ToArray())
            {
                if (trap.PlayersTrapped.Contains(player.PlayerId))
                {
                    continue;
                }

                var dis = Vector2.Distance(trap.Location, position);
                if (dis > TrapRadius.GetFloat()) continue;

                player.RpcMurderPlayer(player);
                player.SetDeathReason(PlayerState.DeathReason.Toxined);

                player.Notify(Utils.ColorString(Utils.GetRoleColor(CustomRoles.Bane), GetString("BaneTrap")));
            }
        }
    }
}

public class BaneTrap
{
    public int BanePlayerId;
    public Vector2 Location;
    public List<int> PlayersTrapped;
    public bool IsActive = false;
}
