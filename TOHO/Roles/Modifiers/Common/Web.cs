using System.Collections.Generic;
using static TOHO.Options;

namespace TOHO.Roles.Modifiers.Common;

public class Web : IModifier
{
    public CustomRoles Role => CustomRoles.Web;
    private const int Id = 44500;
    public ModifierTypes Type => ModifierTypes.Mixed;
    public static bool IsEnable = false;

    public static Dictionary<byte, float> WebTrapIds; 

    public void SetupCustomOption()
    {
        SetupAdtRoleOptions(Id, CustomRoles.Web, canSetNum: true, teamSpawnOptions: true);
    }

    public void Init()
    {        
        IsEnable = false;
    }

    public void Add(byte playerId, bool gameIsLoading = true)
    {        
        IsEnable = true;
        WebTrapIds.Clear();
    }

    public void Remove(byte playerId)
    {        
        IsEnable = false;
    }

    public static void AfterMeetingTasks()
    {
        foreach (var kvp in WebTrapIds)
        {
            var id = kvp.Key;
            var speed = kvp.Value;
            
            var player = Utils.GetPlayerById(id);
            Main.AllPlayerSpeed[id] = speed;
            player.MarkDirtySettings();
        }
        WebTrapIds.Clear();
    }
}