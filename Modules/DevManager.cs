using System.Collections.Generic;
using System.Linq;

namespace TOHO;

public class DevUser(string code = "", string color = "null", string tag = "null", bool isUp = false, bool isDev = false, bool deBug = false, bool colorCmd = false, bool NameCmd = false, string upName = "Unknown")
{
    public string Code { get; set; } = code;
    public string Color { get; set; } = color;
    public string Tag { get; set; } = tag;
    public bool IsUp { get; set; } = isUp;
    public bool IsDev { get; set; } = isDev;
    public bool DeBug { get; set; } = deBug;
    public bool ColorCmd { get; set; } = colorCmd;
    public bool NameCmd { get; set; } = NameCmd;
    public string UpName { get; set; } = upName;

    public bool HasTag() => Tag != "null";
    public string GetTag() => Color == "null" ? $"<size=2>{Tag}</size>\r\n" : $"<color={Color}><size=2>{(Tag == "#Dev" ? Translator.GetString("Developer") : Tag)}</size></color> - ";
}

public static class DevManager
{
    public static DevUser DefaultDevUser = new();
    public static List<DevUser> DevUserList = [];

    public static void Init()
    {
        DevUserList =
        [
            /*CREDITED DEVS BELOW*/

            // Karped stays bcs he is cool
            new(code: "actorour#0029", color: "#ffc0cb", tag: "Original Developer", isUp: true, isDev: true, deBug: true, colorCmd: true, upName: "KARPED1EM"),

            // Gurge also stays bcs he is cool too
            new(code: "neatnet#5851", color: "#FFFF00", tag: "The 200IQ guy", isUp: true, isDev: false, deBug: false, colorCmd: false, upName: "The 200IQ guy"),
        
            /*CREDITED DEVS ABOVE*/

            /*TOHO DEVS BELOW*/

            // Lime
            new(code: "tighttune#4221", color: "#00ff00", tag: "Mod Developer", isUp: true, isDev: true, deBug: true, colorCmd: true, upName: "Lime"),

            // Ape old
            new(code: "simianpair#1270", color: "#0e2f44", tag: "Executive", isUp: true, isDev: true, deBug: true, colorCmd: true, upName: "Ape"),
            // Ape new
            new(code: "apemv#5959", color: "#0e2f44", tag: "Executive", isUp: true, isDev: true, deBug: true, colorCmd: true, upName: "MV"),

            // Dailyhare
            new(code: "noshsame#8116", color: "#011efe", tag: "Bait Killer", isUp: true, isDev: true, deBug: true, colorCmd: true, upName: "Dailyhare"),

            // PEPPERcula
            new(code: "motorlace#4741", color: "#DFB722", tag: "❖ Exclusive Tester ❖", isUp: true, isDev: true, deBug: true, colorCmd: true, upName: "PEPPERcula"),

             // Mirage
            new(code: "spyside#1041", color: "#a300a3", tag: "Zany’s Emergency Button", isUp: true, isDev: true, deBug: true, colorCmd: true, upName: "Mirage"),
            
            /*TOHO DEVS ABOVE*/
            /*TESTERS BELOW*/

            new(code: "electpout#2133", color: "#FF7DA7", tag: "<color=#FF7DA7>S</color><color=#ff87ad>i</color><color=#FF90b3>l</color><color=#FF9ab9>l</color><color=#FFa3bf>y</color> <color=#FFadc6>G</color><color=#FFb6cc>o</color><color=#FFc0d2>o</color><color=#FFc9d8>f</color><color=#FFd3de>y</color>", isUp: true, isDev: false, deBug: true, colorCmd: true, upName: "Clubmore"),
            
            // ArcaneX
            new(code: "tidybasket#6022", color: "#141314ff", tag: "<color=#000000>Exi</color><color=#cc5050>st</color><color=#e86a6a>i</color><color=#d60a0a>ng</color>", isUp: true, isDev: false, deBug: true, colorCmd: true, upName: "ArcaneX"),
            
            
            
            new(code: "tourdreamy#7988", color: "#964b00", tag: "2nd Clowniest Clown", isUp: true, isDev: false, deBug: true, colorCmd: true, upName: "Capy/Checkbox"),

            // BXO
            new(code: "justgust#5169", color: "#07296c", tag: "Tester", isUp: true, isDev: false, deBug: true, colorCmd: true, upName: "BXO"),

            // Apoc
            new(code: "crunchwide#1938", color: "#ffa500", tag: "Tester", isUp: true, isDev: false, deBug: true, colorCmd: true, upName: "Apoc"),

            // Diffy
            new(code: "funnytiger#8420", color: "#FFC5CB", tag: "Tester", isUp: true, isDev: false, deBug: true, colorCmd: true, upName: "Diffy"),

            // Plague
            new(code: "trunksun#2271", color: "#800080", tag: "Tester", isUp: true, isDev: false, deBug: true, colorCmd: true, upName: "Plague"),

            // Glazecraft
            new(code: "motorstack#2287", color: "#ec9d2f", tag: "Tester", isUp: true, isDev: false, deBug: true, colorCmd: true, upName: "Glazecraft"),

            // Zuzu
             new(code: "partyready#4849", color: "#a000c8", tag: "Cultist", isUp: true, isDev: false, deBug: true, colorCmd: true, upName: "Zuzu"),
            
            /*TESTERS ABOVE*/
            // Christmas advent calendar below
            
            new(code: "manesame#3484", color: "#9D00FF", tag: "Evol", isUp: true, isDev: false, deBug: true, colorCmd: true, upName: "Evol"),
            new(code: "manesame#3484", color: "#9D00FF", tag: "Evol", isUp: true, isDev: false, deBug: true, colorCmd: true, upName: "Evol"),
        ];
    }

    public static bool IsDevUser(this string code) => DevUserList.Any(x => x.Code == code);
    public static DevUser GetDevUser(this string code) => code.IsDevUser() ? DevUserList.Find(x => x.Code == code) : DefaultDevUser;
}
