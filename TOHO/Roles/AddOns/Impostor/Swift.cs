using TOHO.Roles.Modifiers.Common;
using static TOHO.Options;

namespace TOHO.Roles.Modifiers.Impostor;

public class Swift : IModifier
{
    public CustomRoles Role => CustomRoles.Swift;
    private const int Id = 23300;
    public ModifierTypes Type => ModifierTypes.Impostor;

    public void SetupCustomOption()
    {
        SetupAdtRoleOptions(Id, CustomRoles.Swift, canSetNum: true, tab: TabGroup.Modifiers);
    }
    public void Init()
    { }
    public void Add(byte playerId, bool gameIsLoading = true)
    { }
    public void Remove(byte playerId)
    { }
    public static bool OnCheckMurder(PlayerControl killer, PlayerControl target)
    {
        if (target.Is(CustomRoles.Stubborn))
        {
            killer.Notify(Translator.GetString("StubbornNotify"));
            return true;
        }
        if (!DisableShieldAnimations.GetBool())
            killer.RpcGuardAndKill(killer);

        if (target.Is(CustomRoles.Trapper))
            killer.TrapperKilled(target);

        killer.SetKillCooldown();

        RPC.PlaySoundRPC(killer.PlayerId, Sounds.KillSound);
        return false;
    }
}
