using System;
using System.Collections.Generic;
using AmongUs.InnerNet.GameDataMessages;
using Hazel;
using InnerNet;
using TOHO;
using UnityEngine;

public static class RPC
{
    public class PendingMsg
    {
        public uint TargetClientId;
        public Action<MessageWriter> SerializeBody;
    }

    private static readonly Queue<PendingMsg> Queue = new();
    private static float LastFlushTime = -999f;
    private const float TimeoutSeconds = 3f;
    private const int SoftMaxBytes = 1100;

    public static void EnqueueGameDataTo(uint targetClientId, Action<MessageWriter> serializeBody)
    {
        Queue.Enqueue(new PendingMsg
        {
            TargetClientId = targetClientId,
            SerializeBody = serializeBody
        });

        TryFlush(force: false);
    }

    public static void EnqueueRpc(uint targetClientId, uint netId, byte rpcCallId, Action<MessageWriter> writeRpcArgs)
    {
        EnqueueGameDataTo(targetClientId, writer =>
        {
            writer.StartMessage((byte)GameDataTypes.RpcFlag);
            writer.WritePacked(netId);
            writer.Write(rpcCallId);
            writeRpcArgs?.Invoke(writer);
            writer.EndMessage();
        });
    }

    public static void Tick()
    {
        if (Time.realtimeSinceStartup - LastFlushTime >= TimeoutSeconds)
        {
            if (Queue.Count == 0) return;
            TryFlush(force: true);
        }
    }

    private static void TryFlush(bool force)
    {
        if (Queue.Count == 0) return;

        if (!force && Queue.Count < 2) return;

        int gameId = AmongUsClient.Instance.GameId;
        var packet = MessageWriter.Get(SendOption.Reliable);

        packet.StartMessage(Tags.PackedGameDataTo);
        packet.WritePacked(gameId);

        int packedCount = 0;
        while (Queue.Count > 0)
        {
            var pending = Queue.Peek();
            
            int startPos = packet.Length;

            packet.StartMessage(Tags.GameDataTo);
            packet.Write(gameId);
            packet.WritePacked(pending.TargetClientId);

            pending.SerializeBody(packet);

            packet.EndMessage();

            if (packet.Length > SoftMaxBytes && packedCount > 0) break;

            Queue.Dequeue();
            packedCount++;
        }

        packet.EndMessage();

        if (packedCount > 0)
        {
            AmongUsClient.Instance.SendOrDisconnect(packet);
            packet.Recycle();
            LastFlushTime = Time.realtimeSinceStartup;
        }
        else packet.Recycle();
    }
}