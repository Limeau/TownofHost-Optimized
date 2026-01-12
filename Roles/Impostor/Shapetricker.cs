using AmongUs.GameOptions;
using TOHO.Modules;
using TOHO.Roles.Core;
using static TOHO.Options;
using static TOHO.Translator;
using static TOHO.Utils;

namespace TOHO.Roles.Impostor;

internal class Shapetricker : RoleBase
{
    //===========================SETUP================================\\
    public override CustomRoles Role => CustomRoles.Shapetricker;
    private const int Id = 38200;
    public static bool HasEnabled => CustomRoleManager.HasEnabled(CustomRoles.Shapetricker);
    public override CustomRoles ThisRoleBase => CustomRoles.Shapeshifter;
    public override Custom_RoleType ThisRoleType => Custom_RoleType.ImpostorConcealing;
    //==================================================================\\

    private static OptionItem ShapeshiftCooldown;
    private static OptionItem ShapeshiftDuration;
    private static OptionItem KillCooldown;

    public static NetworkedPlayerInfo.PlayerOutfit BaseSkin;
    
    public override void SetupCustomOption()
    {
        SetupRoleOptions(Id, TabGroup.ImpostorRoles, CustomRoles.Shapetricker);
        KillCooldown = FloatOptionItem.Create(Id + 10, GeneralOption.KillCooldown, new(1f, 180f, 1f), 20f, TabGroup.ImpostorRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.Shapetricker])
            .SetValueFormat(OptionFormat.Seconds);
        ShapeshiftCooldown = FloatOptionItem.Create(Id + 11, GeneralOption.ShapeshifterBase_ShapeshiftCooldown, new(1f, 180f, 1f), 20f, TabGroup.ImpostorRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.Shapetricker])
            .SetValueFormat(OptionFormat.Seconds);
        ShapeshiftDuration = FloatOptionItem.Create(Id + 12, GeneralOption.ShapeshifterBase_ShapeshiftDuration, new(1f, 999f, 1f), 10f, TabGroup.ImpostorRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.Shapetricker])
                .SetValueFormat(OptionFormat.Seconds);
    }
    
    public override void SetKillCooldown(byte id) => Main.AllPlayerKillCooldown[id] = KillCooldown.GetFloat();

    public override void Add(byte playerId)
    {
        var player = Utils.GetPlayerById(playerId);
        BaseSkin = new NetworkedPlayerInfo.PlayerOutfit().Set(player.GetRealName(), player.CurrentOutfit.ColorId, player.CurrentOutfit.HatId, player.CurrentOutfit.SkinId, player.CurrentOutfit.VisorId, player.CurrentOutfit.PetId, player.CurrentOutfit.NamePlateId);
    }

    public override void ApplyGameOptions(IGameOptions opt, byte playerId)
    {
        AURoleOptions.ShapeshifterCooldown = ShapeshiftCooldown.GetFloat();
        AURoleOptions.ShapeshifterDuration = ShapeshiftDuration.GetFloat();
    }

    public override bool OnCheckShapeshift(PlayerControl shapeshifter, PlayerControl target, ref bool resetCooldown,
        ref bool shouldAnimate)
    {
        var targetSkin = new NetworkedPlayerInfo.PlayerOutfit()
            .Set(target.GetRealName(), target.CurrentOutfit.ColorId, target.CurrentOutfit.HatId,
                target.CurrentOutfit.SkinId, target.CurrentOutfit.VisorId, target.CurrentOutfit.PetId,
                target.CurrentOutfit.NamePlateId);

        Main.CheckShapeshift.TryGetValue(shapeshifter.PlayerId, out var IsShapeshift);
        if (IsShapeshift)
        {
            shapeshifter.RpcShapeshift(target, true);
            shouldAnimate = false;
            _ = new LateTask(() => 
            {
                
                shapeshifter.SetNewOutfit(BaseSkin);
            }, 0.1f, "Shapetricker Shapeshift");
            return true;
        }
        else 
        {
            shapeshifter.SetNewOutfit(targetSkin);
            _ = new LateTask(() => 
            {
                shapeshifter.RpcShapeshift(target, true);
            }, 0.1f, "Shapetricker Shapeshift");
            
            return false;
        }
    }
}
