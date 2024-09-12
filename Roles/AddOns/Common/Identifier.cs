using static UnityEngine.GraphicsBuffer;

namespace TOHE.Roles.AddOns.Common;

public static class Identifier
{
    private const int Id = 30100;

    public static OptionItem ImpCanBeIdentifier;
    public static OptionItem CrewCanBeIdentifier;
    public static OptionItem NeutralCanBeIdentifier;
    
    public static Dictionary<byte, string> IdentifierNotify = [];

    public static void SetupCustomOptions()
    {
        Options.SetupAdtRoleOptions(Id, CustomRoles.Identifier, canSetNum: true);
        ImpCanBeIdentifier = BooleanOptionItem.Create(Id + 10, "ImpCanBeIdentifier", true, TabGroup.Addons, false).SetParent(Options.CustomRoleSpawnChances[CustomRoles.Identifier]);
        CrewCanBeIdentifier = BooleanOptionItem.Create(Id + 11, "CrewCanBeIdentifier", true, TabGroup.Addons, false).SetParent(Options.CustomRoleSpawnChances[CustomRoles.Identifier]);
        NeutralCanBeIdentifier = BooleanOptionItem.Create(Id + 12, "NeutralCanBeIdentifier", true, TabGroup.Addons, false).SetParent(Options.CustomRoleSpawnChances[CustomRoles.Identifier]);
    }

    public static void Init()
    {
        IdentifierNotify = [];
    }
    public static void Clear()
    {
        IdentifierNotify.Clear();
    }

    public static void OnReportDeadBody(PlayerControl reporter, NetworkedPlayerInfo deadBody)
    {
        if (reporter.Is(CustomRoles.Identifier) && !deadBody.Object.IsAlive() && reporter.PlayerId != deadBody.PlayerId)
        {
            var realKiller = deadBody.Object.GetRealKiller();
            var killerOutfit = Camouflage.PlayerSkins[realKiller.PlayerId];

            if (killerOutfit.ColorId == 0 || killerOutfit.ColorId == 1 || killerOutfit.ColorId == 2 || killerOutfit.ColorId == 4 || killerOutfit.ColorId == 6 || killerOutfit.ColorId == 8 || killerOutfit.ColorId == 9 || killerOutfit.ColorId == 12 || killerOutfit.ColorId == 16)
            {
                string msg;
                msg = string.Format(Translator.GetString("IdentifierDark"));
                IdentifierNotify.Add(reporter.PlayerId, msg);
            }


            if (killerOutfit.ColorId == 3 || killerOutfit.ColorId == 5 || killerOutfit.ColorId == 7 || killerOutfit.ColorId == 10 || killerOutfit.ColorId == 11 || killerOutfit.ColorId == 13 || killerOutfit.ColorId == 14 || killerOutfit.ColorId == 15 || killerOutfit.ColorId == 17)
            {
                string msg;
                msg = string.Format(Translator.GetString("IdentifierLight"));
                IdentifierNotify.Add(reporter.PlayerId, msg);
            }
        }
    }
}
