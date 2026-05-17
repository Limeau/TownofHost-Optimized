using System.Collections.Generic;
using System.Linq;
using Rewired;
using TOHO;
using TOHO.Roles.Modifiers;
using static TOHO.Options;

namespace TOHO.Roles.Modifiers.Common;
internal class Productive : IModifier
{
    public CustomRoles Role => CustomRoles.Productive;
    private const int Id = 42000;
    public static bool IsEnable = false;

    public ModifierTypes Type => ModifierTypes.Mixed;
    public void SetupCustomOption()
    {
        SetupAdtRoleOptions(Id, CustomRoles.Productive, canSetNum: true, teamSpawnOptions: true);
    }

    private static PlayerControl ProductivePlayer;

    public void Init()
    { 
        IsEnable = false;
    }

    public void Add(byte playerId, bool gameIsLoading = true)
    {
        IsEnable = true;
        ProductivePlayer = Utils.GetPlayerById(playerId);
    }
    public void Remove(byte playerId)
    { }
    
    public static void OnOthersTaskComplete()
    {
        ProductivePlayer.KillFlash();
    }
}