using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using static TOHO.Translator;
using Object = UnityEngine.Object;

namespace TOHO;

[HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start)), HarmonyPriority(Priority.First)]
public class MainMenuManagerStartPatch
{
    public static GameObject amongUsLogo;
    public static GameObject Ambience;
    public static GameObject PlayerParticles;
    public static GameObject starfield;
    public static GameObject bgmusic;
    public static string BGpath = "./TOHO-DATA/background.mp4";
    public static SpriteRenderer TOHOLogo { get; private set; }

    private static void Postfix(MainMenuManager __instance)
    {
        amongUsLogo = GameObject.Find("LOGO-AU");
        if (amongUsLogo != null)
        {
            amongUsLogo.GetComponent<SpriteRenderer>().sprite = Utils.LoadSprite("TOHO.Resources.Images.tohologo.png");
        }
        
        var rightpanel = __instance.gameModeButtons.transform.parent;
        var logoObject = new GameObject("titleLogo_TOHO");
        var logoTransform = logoObject.transform;

        TOHOLogo = logoObject.AddComponent<SpriteRenderer>();
        logoTransform.parent = rightpanel;
        logoTransform.localPosition = new(-0.16f, 0f, 1f);
        logoTransform.localScale *= 1.2f;

        if ((Ambience = GameObject.Find("Ambience")) != null)
        {
            Ambience.SetActive(true);
        }

        PlayerParticles = GameObject.Find("PlayerParticles");
        starfield = GameObject.Find("starfield");
        /*
        if (PlayerParticles != null)
        {
            PlayerParticles.SetActive(false);
        }
        if (starfield != null)
        {
            starfield.SetActive(false);
        }
        */
        SetButtonColor(__instance.playButton);
        SetButtonColor(__instance.inventoryButton);
        SetButtonColor(__instance.shopButton); 
        SetButtonColor(__instance.newsButton);
        SetButtonColor(__instance.myAccountButton);
        SetButtonColor(__instance.settingsButton);
        SetButtonColor(__instance.creditsButton);
        SetButtonColor(__instance.quitButton);
        SetButtonColor(__instance.PlayOnlineButton);
        SetButtonColor(__instance.playLocalButton);
    }

    private static void SetButtonColor(PassiveButton playButton)
    {
        playButton.inactiveSprites.GetComponent<SpriteRenderer>().color = new Color32(180, 126, 222, byte.MaxValue);
        playButton.activeSprites.GetComponent<SpriteRenderer>().color = new Color32(180, 126, 222, byte.MaxValue);
    }
    
}
[HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.LateUpdate))]
class MainMenuManagerLateUpdatePatch
{
    private static int lateUpdate = 590;
    //private static GameObject LoadingHint;

    private static void Postfix(MainMenuManager __instance)
    {
        if (__instance == null) return;

        if (lateUpdate <= 600)
        {
            lateUpdate++;
            return;
        }
        lateUpdate = 0;
        var PlayOnlineButton = __instance.PlayOnlineButton;
        if (PlayOnlineButton != null)
        {
            if (RunLoginPatch.isAllowedOnline && !Main.hasAccess)
            {
                var PlayLocalButton = __instance.playLocalButton;
                if (PlayLocalButton != null) PlayLocalButton.gameObject.SetActive(false);

                PlayOnlineButton.gameObject.SetActive(false);
                DisconnectPopup.Instance.ShowCustom(GetString("NoAccess"));
            }
        }
    }
}
[HarmonyPatch(typeof(MainMenuManager))]
public static class MainMenuManagerPatch
{
    private static PassiveButton template;
    private static PassiveButton gitHubButton;
    private static PassiveButton donationButton;
    private static PassiveButton discordButton;
    private static PassiveButton websiteButton;
    //private static PassiveButton patreonButton;

    [HarmonyPatch(nameof(MainMenuManager.Start)), HarmonyPostfix, HarmonyPriority(Priority.Normal)]
    public static void Start_Postfix(MainMenuManager __instance)
    {
        if (template == null) template = __instance.quitButton;

        // FPS
        Application.targetFrameRate = Main.UnlockFPS.Value ? 165 : 60;
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
        
        GameObject splashArt = GameObject.CreatePrimitive(PrimitiveType.Quad);
        splashArt.name = "SplashArt";
        splashArt.transform.position = new Vector3(2f, 0f, 600f);
        RenderTexture rt = new(512, 512, 0);
        float videoWidth = rt.width;
        float videoHeight = rt.height;
        float aspect = videoWidth / videoHeight;
        float desiredHeight = 3f * 1.818f;
        float desiredWidth = 3f * aspect * 3.232f;
        splashArt.transform.localScale = new Vector3(desiredWidth, desiredHeight, 1f);
        VideoPlayer vp = splashArt.AddComponent<VideoPlayer>();
        vp.url = System.IO.Path.GetFullPath("./TOHO-DATA/background.mp4");
        vp.targetTexture = rt;
        vp.isLooping = true;
        vp.Play();
        Renderer renderer = splashArt.GetComponent<Renderer>();
        Material mat = new(Shader.Find("Unlit/Texture"));
        mat.mainTexture = rt;
        renderer.material = mat;

        if (template == null) return;

        var PlayerParticles = GameObject.Find("PlayerParticles");
        var starfield = GameObject.Find("starfield");
        if (PlayerParticles != null)
        {
            PlayerParticles.SetActive(false);
        }
        if (starfield != null)
        {
            starfield.SetActive(false);
        }
        

        // donation Button
        if (donationButton == null)
        {
            donationButton = CreateButton(
                "donationButton",
                new(-1.8f, -1.1f, 1f),
                new(0, 255, 255, byte.MaxValue),
                new(75, 255, 255, byte.MaxValue),
                (UnityEngine.Events.UnityAction)(() => Application.OpenURL(Main.DonationInviteUrl)),
                GetString("SupportUs")); //"Donation"
        }
        donationButton.gameObject.SetActive(Main.ShowDonationButton);

        // GitHub Button
        if (gitHubButton == null)
        {
            gitHubButton = CreateButton(
                "GitHubButton",
                new(-1.7f, -2f, 1f),
                new(153, 153, 153, byte.MaxValue),
                new(209, 209, 209, byte.MaxValue),
                (UnityEngine.Events.UnityAction)(() => Application.OpenURL(Main.GitHubInviteUrl)),
                GetString("GitHub")); //"GitHub"
        }
        gitHubButton.gameObject.SetActive(Main.ShowGitHubButton);

        // Discord Button
        if (discordButton == null)
        {
            discordButton = CreateButton(
                "DiscordButton",
                new(-0.5f, -2f, 1f),
                new(88, 101, 242, byte.MaxValue),
                new(148, 161, byte.MaxValue, byte.MaxValue),
                (UnityEngine.Events.UnityAction)(() => Application.OpenURL(Main.DiscordInviteUrl)),
                GetString("Discord")); //"Discord"
        }
        discordButton.gameObject.SetActive(Main.ShowDiscordButton);

        // Website Button
        if (websiteButton == null)
        {
            websiteButton = CreateButton(
                "WebsiteButton",
                new(-1.8f, -2.3f, 1f),
                new(251, 81, 44, byte.MaxValue),
                new(211, 77, 48, byte.MaxValue),
                (UnityEngine.Events.UnityAction)(() => Application.OpenURL(Main.WebsiteInviteUrl)),
                GetString("Website")); //"Website"
        }
        websiteButton.gameObject.SetActive(Main.ShowWebsiteButton);

        var howToPlayButton = __instance.howToPlayButton;
        var freeplayButton = howToPlayButton.transform.parent.Find("FreePlayButton");

        if (freeplayButton != null) freeplayButton.gameObject.SetActive(false);

        howToPlayButton.transform.SetLocalX(0);

    }

    public static PassiveButton CreateButton(string name, Vector3 localPosition, Color32 normalColor, Color32 hoverColor, UnityEngine.Events.UnityAction action, string label, Vector2? scale = null)
    {
        var button = Object.Instantiate(template, MainMenuManagerStartPatch.TOHOLogo.transform);
        button.name = name;
        Object.Destroy(button.GetComponent<AspectPosition>());
        button.transform.localPosition = localPosition;

        button.OnClick = new();
        button.OnClick.AddListener(action);

        var buttonText = button.transform.Find("FontPlacer/Text_TMP").GetComponent<TMP_Text>();
        buttonText.DestroyTranslator();
        buttonText.fontSize = buttonText.fontSizeMax = buttonText.fontSizeMin = 3.5f;
        buttonText.enableWordWrapping = false;
        buttonText.text = label;
        var normalSprite = button.inactiveSprites.GetComponent<SpriteRenderer>();
        var hoverSprite = button.activeSprites.GetComponent<SpriteRenderer>();
        normalSprite.color = normalColor;
        hoverSprite.color = hoverColor;

        var container = buttonText.transform.parent;
        Object.Destroy(container.GetComponent<AspectPosition>());
        Object.Destroy(buttonText.GetComponent<AspectPosition>());
        container.SetLocalX(0f);
        buttonText.transform.SetLocalX(0f);
        buttonText.horizontalAlignment = HorizontalAlignmentOptions.Center;

        var buttonCollider = button.GetComponent<BoxCollider2D>();
        if (scale.HasValue)
        {
            normalSprite.size = hoverSprite.size = buttonCollider.size = scale.Value;
        }

        buttonCollider.offset = new(0f, 0f);

        return button;
    }
    public static void Modify(this PassiveButton passiveButton, UnityEngine.Events.UnityAction action)
    {
        if (passiveButton == null) return;
        passiveButton.OnClick = new Button.ButtonClickedEvent();
        passiveButton.OnClick.AddListener(action);
    }
    public static T FindChild<T>(this MonoBehaviour obj, string name) where T : Object
    {
        string name2 = name;
        return obj.GetComponentsInChildren<T>().First((T c) => c.name == name2);
    }
    public static T FindChild<T>(this GameObject obj, string name) where T : Object
    {
        string name2 = name;
        return obj.GetComponentsInChildren<T>().First((T c) => c.name == name2);
    }
    public static void ForEach<TSource>(this IEnumerable<TSource> source, Action<TSource> action)
    {
        //if (source == null) throw new ArgumentNullException("source");
        if (source == null) throw new ArgumentNullException(nameof(source));

        IEnumerator<TSource> enumerator = source.GetEnumerator();
        while (enumerator.MoveNext())
        {
            action(enumerator.Current);
        }

        enumerator.Dispose();
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
