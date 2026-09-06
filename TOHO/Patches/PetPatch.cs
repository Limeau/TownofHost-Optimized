// Credit: Endless Host Roles by Gurge44

using System.Collections.Generic;
using HarmonyLib;
using Hazel;
using TOHO;
using TOHO.Modules;
using TOHO.Roles.Neutral;


[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.TryPet))]
internal static class LocalPetPatch
{
    private static readonly Dictionary<byte, long> LastProcess = [];

    public static bool Prefix(PlayerControl __instance)
    {
        if (!(AmongUsClient.Instance.AmHost && AmongUsClient.Instance.AmClient)) return true;
        if (GameStates.IsLobby || !__instance.IsAlive()) return true;
        
        if (__instance.petting) return true;
        __instance.petting = true;
        
        LastProcess.TryAdd(__instance.PlayerId, Utils.TimeStamp - 2);
        if (LastProcess[__instance.PlayerId] + 1 >= Utils.TimeStamp) return true;

        if (GameStates.IsLobby || !Options.EnablePowerUps.GetBool() || Options.CurrentGameMode != CustomGameMode.Standard || !AmongUsClient.Instance.AmHost) return true;
        
        ExternalRpcPetPatch.Prefix(__instance.MyPhysics, (byte)RpcCalls.Pet);

        return false;
    }

    public static void Postfix(PlayerControl __instance)
    {
        __instance.petting = false;
    }
}

[HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.HandleRpc))]
internal static class ExternalRpcPetPatch
{
    private static readonly Dictionary<byte, long> LastProcess = [];

    public static void Prefix(PlayerPhysics __instance, [HarmonyArgument(0)] byte callID)
    {
        if (GameStates.IsLobby || !Options.EnablePowerUps.GetBool() || Options.CurrentGameMode != CustomGameMode.Standard || !AmongUsClient.Instance.AmHost || (RpcCalls)callID != RpcCalls.Pet) return;

        PlayerControl pc = __instance.myPlayer;
        PlayerPhysics physics = __instance;

        if (!pc.IsAlive()) return;
        
        LastProcess.TryAdd(pc.PlayerId, Utils.TimeStamp - 2);
        if (LastProcess[pc.PlayerId] + 1 >= Utils.TimeStamp) return;
        LastProcess[pc.PlayerId] = Utils.TimeStamp;

        if (!pc.inVent && !pc.inMovingPlat && !pc.walkingToVent && !pc.onLadder && !physics.Animations.IsPlayingEnterVentAnimation() && !physics.Animations.IsPlayingClimbAnimation() && !physics.Animations.IsPlayingAnyLadderAnimation() && !Pelican.IsEaten(pc.PlayerId) && GameStates.IsInTask)
        {
            CancelPet();
            _ = new LateTask(CancelPet, 0.4f);

            void CancelPet()
            {
                physics.CancelPet();
                MessageWriter w = AmongUsClient.Instance.StartRpcImmediately(physics.NetId, (byte)RpcCalls.CancelPet, SendOption.None);
                AmongUsClient.Instance.FinishRpcImmediately(w);
            }
        }

        TOHO.Logger.Info($"Player {pc.GetNameWithRole()} petted their pet", "PetActionTrigger");

        _ = new LateTask(() => OnPetUse(pc), 0.2f, $"OnPetUse: {pc.GetNameWithRole()}", false);
    }

    private static void OnPetUse(PlayerControl pc)
    {
        if (!pc || pc.inVent || pc.inMovingPlat || pc.onLadder || pc.walkingToVent || pc.MyPhysics.Animations.IsPlayingEnterVentAnimation() || pc.MyPhysics.Animations.IsPlayingClimbAnimation() || pc.MyPhysics.Animations.IsPlayingAnyLadderAnimation() || Pelican.IsEaten(pc.PlayerId) || !AmongUsClient.Instance.AmHost || GameStates.IsLobby || AntiBlackout.SkipTasks) return;

        PowerUp.OnPet(pc);
    }
}