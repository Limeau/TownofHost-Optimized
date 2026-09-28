using System;
using System.Linq;
using HarmonyLib;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TOHO;

[HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start)), HarmonyPriority(Priority.First)]
public class MainMenuManagerStartPatch
{
    public static SpriteRenderer TOHOLogo;
    private static void Postfix(MainMenuManager __instance)
    {
        var amongUsLogo = GameObject.Find("LOGO-AU");
        if (amongUsLogo)
        {
            amongUsLogo.GetComponent<SpriteRenderer>().sprite = Utils.LoadSprite("TOHO.Resources.Images.TohoLogo.png");
        }
        
        var rightpanel = __instance.gameModeButtons.transform.parent;
        var logoObject = new GameObject("titleLogo_TOHO");
        var logoTransform = logoObject.transform;

        TOHOLogo = logoObject.AddComponent<SpriteRenderer>();
        logoTransform.parent = rightpanel;
        logoTransform.localPosition = new(-0.16f, 0f, 1f);
        logoTransform.localScale *= 1.2f;

        var ambience = (GameObject.Find("Ambience"));
        if (ambience != null)
        {
            ambience.SetActive(true);
        }

        SetButtonColor(__instance.playButton);
        SetButtonColor(__instance.inventoryButton);
        SetButtonColor(__instance.shopButton); 
        SetButtonColor(__instance.newsButton);
        SetButtonColor(__instance.myAccountButton);
        SetButtonColor(__instance.settingsButton);
        SetButtonColor(__instance.creditsButton);
        SetButtonColor(__instance.quitButton);
    }

    public static void SetButtonColor(PassiveButton playButton)
    {
        playButton.inactiveSprites.GetComponent<SpriteRenderer>().color = new Color32(180, 126, 222, byte.MaxValue);
        playButton.activeSprites.GetComponent<SpriteRenderer>().color = new Color32(180, 126, 222, byte.MaxValue);
    }
    
}
[HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.LateUpdate))]
class MainMenuManagerLateUpdatePatch
{
    private static void Postfix(MainMenuManager __instance)
    {
        if (!__instance || !__instance.finishStartup) return;
        var playOnlineButton = __instance.PlayOnlineButton;
        if (playOnlineButton != null)
        {
            var playLocalButton = __instance.playLocalButton;
            if (playLocalButton != null) playLocalButton.gameObject.SetActive(false);

            playOnlineButton.gameObject.SetActive(false);
        }
        MainMenuManagerStartPatch.SetButtonColor(__instance.PlayOnlineButton);
        MainMenuManagerStartPatch.SetButtonColor(__instance.playLocalButton);
    }
}
[HarmonyPatch(typeof(MainMenuManager))]
public static class MainMenuManagerPatch
{
    [HarmonyPatch(nameof(MainMenuManager.Start)), HarmonyPostfix, HarmonyPriority(Priority.Normal)]
    public static void Start_Postfix(MainMenuManager __instance)
    {
        Application.targetFrameRate = 165;
        GameObject rightPanel = __instance.mainMenuUI.FindChild<Transform>("RightPanel").gameObject;
        __instance.screenTint.enabled = false;
        GameObject maskedBlackScreen = rightPanel.FindChild<Transform>("MaskedBlackScreen").gameObject;
        maskedBlackScreen.GetComponent<SpriteRenderer>().enabled = false;
        var background = GameObject.Find("BackgroundTexture");
        if (background != null)
        {
            var render = background.GetComponent<SpriteRenderer>();
            render.flipY = true;
            render.color = new Color(126f, 0f, 194f, 1f);
        }
        var tint = GameObject.Find("MainUI").transform.GetChild(0).gameObject;
        if (tint != null)
        {
            tint.GetComponent<SpriteRenderer>().enabled = false;
        }
        
        GameObject leftPanel = __instance.mainMenuUI.FindChild<Transform>("LeftPanel").gameObject;
        leftPanel.GetComponentsInChildren<SpriteRenderer>(true).Where(r => r.name == "Shine").ToList().ForEach(r => r.enabled = false);
        leftPanel.gameObject.FindChild<SpriteRenderer>("Divider").enabled = false;
        
        var playerParticles = GameObject.Find("PlayerParticles");
        var starfield = GameObject.Find("starfield");
        playerParticles.SetActive(false);
        starfield.SetActive(false);
        
        GameObject splashArt = new("SplashArt");
        splashArt.SetActive(true);
        splashArt.transform.position = new Vector3(2f, 0f, 600f);
        SpriteRenderer menuBackground = splashArt.AddComponent<SpriteRenderer>();

        switch (DateTime.Now.Month)
        {
            case 1:
                menuBackground.sprite = Utils.LoadSprite("TOHO.Resources.Background.January.jpg", 200f);
                break;
            case 2:
                menuBackground.sprite = Utils.LoadSprite("TOHO.Resources.Background.February.jpg", 200f);
                break;
            case 3:
                menuBackground.sprite = Utils.LoadSprite("TOHO.Resources.Background.March.jpg", 200f);
                break;
            case 4:
                menuBackground.sprite = Utils.LoadSprite("TOHO.Resources.Background.April.jpg", 200f);
                break;
            case 5:
                menuBackground.sprite = Utils.LoadSprite("TOHO.Resources.Background.May.jpg", 200f);
                break;
            case 6:
                menuBackground.sprite = Utils.LoadSprite("TOHO.Resources.Background.June.jpg", 200f);
                break;
            case 7:
                menuBackground.sprite = Utils.LoadSprite("TOHO.Resources.Background.July.jpg", 200f);
                break;
            case 8:
                menuBackground.sprite = Utils.LoadSprite("TOHO.Resources.Background.August.jpg", 200f);
                break;
            case 9:
                menuBackground.sprite = Utils.LoadSprite("TOHO.Resources.Background.September.jpg", 200f);
                break;
            case 10:
                menuBackground.sprite = Utils.LoadSprite("TOHO.Resources.Background.October.jpg", 200f);
                break;
            case 11:
                menuBackground.sprite = Utils.LoadSprite("TOHO.Resources.Background.November.jpg", 200f);
                break;
            case 12:
                menuBackground.sprite = Utils.LoadSprite("TOHO.Resources.Background.December.jpg", 200f);
                break;
        }

        var howToPlayButton = __instance.howToPlayButton;
        var freeplayButton = howToPlayButton.transform.parent.Find("FreePlayButton");

        if (freeplayButton != null) freeplayButton.gameObject.SetActive(false);

        howToPlayButton.transform.SetLocalX(0);

    }
    public static T FindChild<T>(this GameObject obj, string name) where T : Object
    {
        string name2 = name;
        return obj.GetComponentsInChildren<T>().First(c => c.name == name2);
    }
    
    [HarmonyPatch(nameof(MainMenuManager.OpenGameModeMenu))]
    [HarmonyPatch(nameof(MainMenuManager.OpenAccountMenu))]
    [HarmonyPatch(nameof(MainMenuManager.OpenCredits))]
    [HarmonyPostfix]
    public static void OpenMenu_Postfix()
    {
        if (MainMenuManagerStartPatch.TOHOLogo != null) MainMenuManagerStartPatch.TOHOLogo.gameObject.SetActive(false);
    }
    [HarmonyPatch(nameof(MainMenuManager.ResetScreen)), HarmonyPostfix]
    public static void ResetScreen_Postfix()
    {
        if (MainMenuManagerStartPatch.TOHOLogo != null) MainMenuManagerStartPatch.TOHOLogo.gameObject.SetActive(true);
    }
}