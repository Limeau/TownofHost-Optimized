using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using AmongUs.GameOptions;
using TOHO.Modules;
using TOHO.Roles.Impostor;
using static TOHO.Options;
using Color = UnityEngine.Color;

namespace TOHO.Roles.Neutral;

internal class Beholder : RoleBase
{
    //===========================SETUP================================\\
    public override CustomRoles Role => CustomRoles.Beholder;
    private const int Id = 43500;
    public override CustomRoles ThisRoleBase => CustomRoles.Shapeshifter;
    public override Custom_RoleType ThisRoleType => Custom_RoleType.NeutralKilling;
    public override bool TOHORole => true;
    //==================================================================\\
    
    private static OptionItem KillCooldown;
    private static OptionItem ShapeshiftCooldown;
    private static OptionItem EnableBlindRay;
    private static OptionItem EnableFreezeRay;
    private static OptionItem EnableDeathRay;
    private static OptionItem EnableConsumeRay;
    
    private static bool HasUsed = false;
    private static List<PlayerControl> Blinded = [];
    private static List<int> RayOptions = [];
    private static Dictionary<PlayerControl, float> Frozen = [];

    public override void SetupCustomOption()
    {
        SetupSingleRoleOptions(Id, TabGroup.NeutralRoles, CustomRoles.Beholder);
        KillCooldown = FloatOptionItem.Create(Id + 10, GeneralOption.KillCooldown, new(1f, 60f, 1f), 20f, TabGroup.NeutralRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Beholder])
            .SetValueFormat(OptionFormat.Seconds);
        ShapeshiftCooldown = FloatOptionItem.Create(Id + 11, GeneralOption.ShapeshifterBase_ShapeshiftCooldown, new(1f, 60f, 1f), 20f, TabGroup.NeutralRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Beholder])
            .SetValueFormat(OptionFormat.Seconds);
        EnableBlindRay = BooleanOptionItem.Create(Id + 12, "EnableBlindRay", true, TabGroup.NeutralRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Beholder])
            .SetColor(Color.yellow);
        EnableFreezeRay = BooleanOptionItem.Create(Id + 13, "EnableFreezeRay", true, TabGroup.NeutralRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Beholder])
            .SetColor(Color.cyan);
        EnableDeathRay = BooleanOptionItem.Create(Id + 14, "EnableDeathRay", true, TabGroup.NeutralRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Beholder])
            .SetColor(Color.red);
        EnableConsumeRay = BooleanOptionItem.Create(Id + 15, "EnableConsumeRay", true, TabGroup.NeutralRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Beholder])
            .SetColor(Color.gray);
    }

    public override bool CanUseKillButton(PlayerControl pc)
    {
        return true;
    }

    public override void SetKillCooldown(byte id) => Main.AllPlayerKillCooldown[id] = KillCooldown.GetFloat();

    public override void Add(byte playerId)
    {
        RayOptions.Clear();
        if (EnableBlindRay.GetBool())
        {
            RayOptions.Add(0);
        }
        if (EnableFreezeRay.GetBool())
        {
            RayOptions.Add(1);
        }
        if (EnableDeathRay.GetBool())
        {
            RayOptions.Add(2);
        }
        if (EnableConsumeRay.GetBool())
        {
            RayOptions.Add(3);
        }
    }

    public override void ApplyGameOptions(IGameOptions opt, byte playerId)
    {
        AURoleOptions.ShapeshifterCooldown = ShapeshiftCooldown.GetFloat();
    }

    public override bool OnCheckMurderAsKiller(PlayerControl killer, PlayerControl target)
    {
        if (HasUsed)
        {
            killer.RpcGuardAndKill();
            return false;
        }
        return true;
    }

    public override bool OnCheckShapeshift(PlayerControl shapeshifter, PlayerControl target, ref bool resetCooldown,
        ref bool shouldAnimate)
    {
        if (HasUsed) return false;
        if (!RayOptions.Any()) return true;
        HasUsed = true;
        switch (RayOptions.RandomElement())
        {
            case 0:
                Main.PlayerStates[target.PlayerId].IsBlackOut = true;
                target.MarkDirtySettings();
                Blinded.Add(target);
                break;
            case 1:
                Frozen.Add(target, Main.AllPlayerSpeed[target.PlayerId]);
                Main.AllPlayerSpeed[target.PlayerId] = 0f;
                target.MarkDirtySettings();
                break;
            case 2:
                shapeshifter.KillWithoutBody(target);
                break;
            case 3:
                target.SetNewOutfit(Devourer.ConsumedOutfit, setName: false, setNamePlate: false);
                break;
        }
        shapeshifter.RpcResetAbilityCooldown();
        return false;
    }

    public override void AfterMeetingTasks()
    {
        foreach (var target in Blinded)
        {
            Main.PlayerStates[target.PlayerId].IsBlackOut = false;
            target.MarkDirtySettings();
        }
        foreach (var tar in Frozen)
        {
            Main.AllPlayerSpeed[tar.Key.PlayerId] = tar.Value;
            tar.Key.MarkDirtySettings();
        }
        HasUsed = false;
    }
}
