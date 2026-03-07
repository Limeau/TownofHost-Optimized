
namespace TOHO.Roles.AddOns.Crewmate;

public class Feeblemind : IAddon
{
    public CustomRoles Role => CustomRoles.Feeblemind;
    private const int Id = 38700;
    public static bool IsEnable = false;
    public AddonTypes Type => AddonTypes.Harmful;

    public void SetupCustomOption()
    {
        Options.SetupAdtRoleOptions(Id, CustomRoles.Feeblemind, canSetNum: true);
    }

    public void Init()
    {
        IsEnable = false;
    }
    public void Add(byte playerId, bool gameIsLoading = true)
    {         
        IsEnable = true;
    }
    public void Remove(byte playerId)
    { }

    public static void AfterMeetingTasks()
    {
        if (PlayerControl.LocalPlayer.IsAlive())
        {
            PlayerControl.LocalPlayer.RpcResetTasks();
        }
}
    //Hard to check specific player, loop check all player
}
