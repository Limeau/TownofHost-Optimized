using System.Collections.Generic;
using System.Linq;
using static TOHO.Options;
using static TOHO.Translator;

namespace TOHO.Roles.Modifiers.Common;

public class Burst : IModifier
{
    public CustomRoles Role => CustomRoles.Burst;
    private const int Id = 19000;
    public static bool IsEnable = false;
    public ModifierTypes Type => ModifierTypes.Helpful;

    private static OptionItem BurstKillDelay;

    private static readonly HashSet<byte> BurstBodies = [];
    private static readonly HashSet<byte> playerList = [];

    public void SetupCustomOption()
    {
        SetupAdtRoleOptions(Id, CustomRoles.Burst, canSetNum: true, teamSpawnOptions: true);
        BurstKillDelay = FloatOptionItem.Create(Id + 13, "BurstKillDelay", new(1f, 180f, 1f), 5f, TabGroup.Modifiers, false).SetParent(CustomRoleSpawnChances[CustomRoles.Burst])
            .SetValueFormat(OptionFormat.Seconds);
    }

    public void Init()
    {
        IsEnable = false;
        BurstBodies.Clear();
        playerList.Clear();
    }
    public void Add(byte playerId, bool gameIsLoading = true)
    {
        playerList.Add(playerId);
        IsEnable = true;
    }
    public void Remove(byte playerId)
    {
        playerList.Remove(playerId);

        if (!playerList.Any())
            IsEnable = false;
    }

    public static void AfterMeetingTasks()
    {
        BurstBodies.Clear();
    }

    public static void AfterBurstDeadTasks(PlayerControl killer, PlayerControl target)
    {
        target.SetRealKiller(killer);
        BurstBodies.Add(target.PlayerId);
        if (killer.PlayerId != target.PlayerId && !killer.IsTransformedNeutralApocalypse())
        {
            killer.Notify(Utils.ColorString(Utils.GetRoleColor(CustomRoles.Burst), GetString("BurstNotify")));
            _ = new LateTask(() =>
            {
                if (!killer.inVent && killer.IsAlive() && !GameStates.IsMeeting && GameStates.IsInGame)
                {
                    killer.SetDeathReason(PlayerState.DeathReason.Bombed);
                    target.RpcMurderPlayer(killer);
                    killer.SetRealKiller(target);
                }
                else if (GameStates.IsInGame)
                {
                    RPC.PlaySoundRPC(killer.PlayerId, Sounds.TaskComplete);
                    killer.SetKillCooldown(time: Main.AllPlayerKillCooldown[killer.PlayerId] - BurstKillDelay.GetFloat(), forceAnime: true);
                    killer.Notify(Utils.ColorString(Utils.GetRoleColor(CustomRoles.Burst), GetString("BurstFailed")));
                }
                BurstBodies.Remove(target.PlayerId);
            }, BurstKillDelay.GetFloat(), "Burst Suicide");
        }
    }
}
