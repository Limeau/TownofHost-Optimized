using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using Hazel;
using TOHO.Modules;
using static TOHO.Options;
using static TOHO.Translator;

namespace TOHO.Roles.Neutral
{
    internal class Artist : RoleBase
    {
        //===========================SETUP================================\\
        public override CustomRoles Role => CustomRoles.Artist;
        private const int Id = 32900;
        public static bool HasEnabled => PlayerIds.Any();
        public override CustomRoles ThisRoleBase => CustomRoles.Impostor;
        public override Custom_RoleType ThisRoleType => Custom_RoleType.NeutralKilling;
        public override bool TOHORole => true;
        //==================================================================\\

        private static readonly NetworkedPlayerInfo.PlayerOutfit PaintedOutfit = new NetworkedPlayerInfo.PlayerOutfit().Set("", 15, "", "", "visor_Crack", "", "");
        private static readonly Dictionary<byte, NetworkedPlayerInfo.PlayerOutfit> OriginalPlayerSkins = new Dictionary<byte, NetworkedPlayerInfo.PlayerOutfit>();
        private static readonly Dictionary<byte, List<byte>> PlayerSkinsPainted = new Dictionary<byte, List<byte>>();
        private static readonly Dictionary<byte, List<byte>> PaintingTarget = new Dictionary<byte, List<byte>>();
        private static readonly HashSet<byte> PlayerIds = new HashSet<byte>();

        private static OptionItem KillCooldown;
        private static OptionItem PaintCooldown;
        private static OptionItem CanVent;
        private static OptionItem HasImpostorVision;
        private static OptionItem AbilityUses;

        public override void SetupCustomOption()
        {
            SetupRoleOptions(Id, TabGroup.NeutralRoles, CustomRoles.Artist);
            KillCooldown = FloatOptionItem.Create(Id + 10, GeneralOption.KillCooldown, new(0f, 180f, 2.5f), 30f, TabGroup.NeutralRoles, false)
                .SetParent(CustomRoleSpawnChances[CustomRoles.Artist])                
                .SetValueFormat(OptionFormat.Seconds);
            PaintCooldown = FloatOptionItem.Create(Id + 11, "ArtistPaintCooldown", new(0f, 180f, 2.5f), 15f, TabGroup.NeutralRoles, false)
                .SetParent(CustomRoleSpawnChances[CustomRoles.Artist])
                .SetValueFormat(OptionFormat.Seconds);
            CanVent = BooleanOptionItem.Create(Id + 12, GeneralOption.CanVent, true, TabGroup.NeutralRoles, false)
                .SetParent(CustomRoleSpawnChances[CustomRoles.Artist]);
            HasImpostorVision = BooleanOptionItem.Create(Id + 13, GeneralOption.ImpostorVision, true, TabGroup.NeutralRoles, false)
                .SetParent(CustomRoleSpawnChances[CustomRoles.Artist]);
            AbilityUses = IntegerOptionItem.Create(Id + 14, "ArtistAbilityUses", new(0, 15, 1), 5, TabGroup.NeutralRoles, false)
                .SetParent(CustomRoleSpawnChances[CustomRoles.Artist]);
        }

        public override void Init()
        {
            PlayerSkinsPainted.Clear();
            OriginalPlayerSkins.Clear();
            PlayerIds.Clear();
        }

        public override void Add(byte playerId)
        {
            playerId.SetAbilityUseLimit(AbilityUses.GetInt());
            PlayerSkinsPainted[playerId] = new List<byte>();
            PlayerIds.Add(playerId);
            PaintingTarget[playerId] = new List<byte>();

            var pc = Utils.GetPlayerById(playerId);
            pc.AddDoubleTrigger();

            if (!Main.ResetCamPlayerList.Contains(playerId))
                Main.ResetCamPlayerList.Add(playerId);
        }
        public override void SetKillCooldown(byte id) => Main.AllPlayerKillCooldown[id] = KillCooldown.GetFloat();
        public override bool CanUseKillButton(PlayerControl pc) => true;
        public override bool CanUseImpostorVentButton(PlayerControl pc) => CanVent.GetBool();

        public override void ApplyGameOptions(IGameOptions opt, byte id)
        {
            opt.SetVision(HasImpostorVision.GetBool());
        }

        public override bool OnCheckMurderAsKiller(PlayerControl killer, PlayerControl target)
        {
            if (killer == null) return true;
            if (target == null) return true;
            if (killer.GetAbilityUseLimit() <= 0) return true;

            if (PlayerSkinsPainted[killer.PlayerId].Contains(target.PlayerId))
            {
                return true;
            }
            if (killer.CheckDoubleTrigger(target, () => { })) return true;
            
            killer.RpcRemoveAbilityUse();
            target.SetNewOutfit(PaintedOutfit);
            PlayerSkinsPainted[killer.PlayerId].Add(target.PlayerId);
            killer.RpcGuardAndKill();
            killer.SetKillCooldown(PaintCooldown.GetFloat());
            
            Main.OvverideOutfit[target.PlayerId] = (PaintedOutfit, "");
            RPC.SyncAllPlayerNames();
            Main.Instance.StartCoroutine(Utils.NotifyEveryoneAsync(speed: 5));
            
            return false;
        }
    }
}
