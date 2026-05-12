using BepInEx.Unity.IL2CPP.Utils.Collections;
using HarmonyLib;
using System;
using Il2CppSystem.IO;
using TMPro;
using TOHO.Modules;
using UnityEngine;
using UnityEngine.UI;

namespace TOHO.Patches;

[HarmonyPatch(typeof(LobbyBehaviour), nameof(LobbyBehaviour.Start))]
public class LobbyStartPatch
{
    private static GameObject LobbyPaintObject;
    private static GameObject DropshipDecorationsObject;
    private static Sprite LobbyPaintSprite;
    private static Sprite DropshipDecorationsSprite;

    private static bool FirstDecorationsLoad = true;
    public static void Prefix()
    {
        LobbyPaintSprite = Utils.LoadSprite("TOHO.Resources.Images.LobbyPaint.png", 290f);
        DropshipDecorationsSprite = Utils.LoadSprite("TOHO.Resources.Images.TOHO_decor.png", 60f);
    }
    public static void Postfix(LobbyBehaviour __instance)
    {
        if (!Directory.Exists(@$"/BepInEx/resources/music/")) Directory.CreateDirectory(@$"/BepInEx/resources/music/");

        if (Main.DisableLobbyMusic.Value && Directory.GetFiles(@$"/BepInEx/resources/music/", "*.wav").Count != 0)
        {
            SoundManager.Instance.StopNamedSound("MapTheme");
            CustomSoundsManager.MusicPlay();
        }
        
        float waitTime = 0f;
        if (FirstDecorationsLoad)
            waitTime = 0.25f;
        else
            waitTime = 0.05f;

        _ = new LateTask(() =>
        {
            __instance.StartCoroutine(CoLoadDecorations().WrapToIl2Cpp());
        }, waitTime, "Co Load Dropship Decorations", shoudLog: false);

        var Engine1 = GameObject.Find("RightEngine");
        if (Engine1 != null)
        {
            Engine1.GetComponent<SpriteRenderer>().color = Color.green;
        }
        else Logger.Info("RightEngine is null", "LobbyPatch");
        var Engine2 = GameObject.Find("LeftEngine");
        if (Engine2 != null)
        {
            Engine2.GetComponent<SpriteRenderer>().color = Color.red;
        }
        else Logger.Info("LeftEngine is null", "LobbyPatch");

        static System.Collections.IEnumerator CoLoadDecorations()
        {
            var LeftBox = GameObject.Find("Leftbox");
            if (LeftBox != null)
            {
                LobbyPaintObject = UnityEngine.Object.Instantiate(LeftBox, LeftBox.transform.parent.transform);
                LobbyPaintObject.name = "Lobby Paint";
                LobbyPaintObject.transform.localPosition = new Vector3(0.042f, -2.59f, -10.5f);
                SpriteRenderer renderer = LobbyPaintObject.GetComponent<SpriteRenderer>();
                renderer.sprite = LobbyPaintSprite;
            }

            yield return null;

            if (Main.EnableCustomDecorations.Value)
            {
                var Dropship = GameObject.Find("SmallBox");
                if (Dropship != null)
                {
                    DropshipDecorationsObject = UnityEngine.Object.Instantiate(Dropship, UnityEngine.Object.FindAnyObjectByType<LobbyBehaviour>().transform);
                    DropshipDecorationsObject.name = "Lobby_Decorations";
                    DropshipDecorationsObject.transform.DestroyChildren();
                    UnityEngine.Object.Destroy(DropshipDecorationsObject.GetComponent<PolygonCollider2D>());
                    DropshipDecorationsObject.GetComponent<SpriteRenderer>().sprite = DropshipDecorationsSprite;
                    DropshipDecorationsObject.transform.SetSiblingIndex(1);
                    DropshipDecorationsObject.transform.localPosition = new(0.05f, 0.8334f);
                }
            }

            yield return null;

            FirstDecorationsLoad = false;
        }
    }
}

// https://github.com/SuperNewRoles/SuperNewRoles/blob/master/SuperNewRoles/Patches/LobbyBehaviourPatch.cs
[HarmonyPatch(typeof(LobbyBehaviour))]
public class LobbyBehaviourPatch
{
    [HarmonyPatch(nameof(LobbyBehaviour.Update)), HarmonyPostfix]
    public static void Update_Postfix(LobbyBehaviour __instance)
    {
        if (Main.DisableLobbyMusic.Value || Directory.GetFiles(@$"{Environment.CurrentDirectory.Replace(@"\", "/")}/BepInEx/resources/music/", "*.wav").Count != 0) SoundManager.Instance.StopNamedSound("MapTheme");
    } 
}
[HarmonyPatch(typeof(HostInfoPanel), nameof(HostInfoPanel.SetUp))]
public static class HostInfoPanelUpdatePatch
{
    private static TextMeshPro HostText;
    public static bool Prefix()
    {
        return GameStates.IsLobby;
    }
    public static void Postfix(HostInfoPanel __instance)
    {
        try
        {
            if (AmongUsClient.Instance.AmHost)
            {
                if (HostText == null)
                    HostText = __instance.content.transform.FindChild("Name").GetComponent<TextMeshPro>();

                string htmlStringRgb = ColorUtility.ToHtmlStringRGB(Palette.PlayerColors[__instance.player.ColorId]);
                string hostName = Main.HostRealName;
                string youLabel = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.HostYouLabel);

                // Set text in host info panel
                HostText.text = $"<color=#{htmlStringRgb}>{hostName}</color>  <size=90%><b><font=\"Barlow-BoldItalic SDF\" material=\"Barlow-BoldItalic SDF Outline\">({youLabel})";
            }
        }
        catch
        { }
    }
}
