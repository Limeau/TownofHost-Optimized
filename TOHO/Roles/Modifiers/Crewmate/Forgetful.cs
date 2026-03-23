
namespace TOHO.Roles.Modifiers.Crewmate;

public class Forgetful : IModifier
{
    public CustomRoles Role => CustomRoles.Forgetful;
    private const int Id = 38700;
    public static bool IsEnable = false;
    public ModifierTypes Type => ModifierTypes.Harmful;

    public void SetupCustomOption()
    {
        Options.SetupAdtRoleOptions(Id, CustomRoles.Forgetful, canSetNum: true);
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
