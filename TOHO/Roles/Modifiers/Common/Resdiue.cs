using System.Collections.Generic;
using System.Linq;
using TOHO;
using TOHO.Roles.Modifiers;
using static TOHO.Options;

namespace TOHO.Roles.Modifiers.Common;
internal class Residue : IModifier
{
    public CustomRoles Role => CustomRoles.Residue;
    private const int Id = 39800;
    public ModifierTypes Type => ModifierTypes.Harmful;
    public void SetupCustomOption()
    {
        SetupAdtRoleOptions(Id, CustomRoles.Residue, canSetNum: true, teamSpawnOptions: false);
    }
    public void Init()
    { }
    public void Add(byte playerId, bool gameIsLoading = true)
    { }
    public void Remove(byte playerId)
    { }

    public static Dictionary<PlayerControl, PlainShipRoom> ResidueAlive = [];
    
    public static void OnMeeting()
    {
        foreach (var pc in Main.AllAlivePlayerControls.Where(x => x.Is(CustomRoles.Residue) && !ResidueAlive.Keys.Contains(x)).ToArray())
        {
            MeetingHudStartPatch.AddMsg($"The player {pc.GetRealName()} was in the room {Translator.GetString(pc.GetPlainShipRoom().RoomId.ToString())} before the meeting was called.", title: Utils.ColorString(Utils.GetRoleColor(CustomRoles.Residue), "RESIDUE INFORMATION"));
        }
    }
}