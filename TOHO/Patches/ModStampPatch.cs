using HarmonyLib;
using TOHO;
using UnityEngine;

[HarmonyPatch(typeof(ModManager), nameof(ModManager.LateUpdate))]
class ModManagerLateUpdatePatch
{
    public static void Prefix(ModManager __instance)
    {
        __instance.ShowModStamp();
        RPC.Tick();
        LateTask.Update(Time.deltaTime);
    }
}