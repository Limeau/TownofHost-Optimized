using AmongUs.GameOptions;
using System;
using System.Collections.Generic; 
using UnityEngine;
using static TOHE.Options;
using static TOHE.Utils;
using static TOHE.Translator;
using TOHE.Roles.Core;

namespace TOHE.Roles.Neutral
{
    internal class Repellant : RoleBase
    {
        //===========================SETUP================================\\
        private const int Id = 30500;
        private static readonly HashSet<byte> PlayerIds = new HashSet<byte>(); // Initialize HashSet
        public static bool HasEnable = PlayerIds.Any();
        
        public override CustomRoles ThisRoleBase => CustomRoles.Engineer;
        public static bool HasEnabled => CustomRoleManager.HasEnabled(CustomRoles.Repellant);
        public override Custom_RoleType ThisRoleType => Custom_RoleType.NeutralBenign;

        private static OptionItem RepellantSkillCooldown;
        private static OptionItem RepellantSkillDuration;
        private static OptionItem RepellantMaxUses;

        private static readonly Dictionary<byte, int> RepellantNum = new Dictionary<byte, int>(); // Initialize Dictionary
        private static readonly Dictionary<byte, long> RepellantInProtect = new Dictionary<byte, long>(); // Initialize Dictionary

        public override void SetupCustomOption()
        {
            SetupRoleOptions(Id, TabGroup.NeutralRoles, CustomRoles.Repellant);
            RepellantSkillCooldown = FloatOptionItem.Create(Id + 10, "RepellantSkillCooldown", new(1f, 180f, 1f), 20f, TabGroup.NeutralRoles, false)
                .SetParent(CustomRoleSpawnChances[CustomRoles.Repellant])
                .SetValueFormat(OptionFormat.Seconds);
            RepellantSkillDuration = FloatOptionItem.Create(Id + 11, "RepellantSkillDuration", new(1f, 180f, 1f), 20f, TabGroup.NeutralRoles, false)
                .SetParent(CustomRoleSpawnChances[CustomRoles.Repellant])
                .SetValueFormat(OptionFormat.Seconds);
            RepellantMaxUses = IntegerOptionItem.Create(Id + 12, "RepellantMaxUses", new(0, 20, 1), 1, TabGroup.NeutralRoles, false)
                .SetParent(CustomRoleSpawnChances[CustomRoles.Repellant])
                .SetValueFormat(OptionFormat.Times);
        }

        public override void Init()
        {
            RepellantNum.Clear();
            RepellantInProtect.Clear();
        }

        public override void Add(byte playerId)
        {
            RepellantNum.TryAdd(playerId, 0);
            AbilityLimit = RepellantMaxUses.GetInt();
        }

        public override void ApplyGameOptions(IGameOptions opt, byte playerId)
        {
            AURoleOptions.EngineerCooldown = RepellantSkillCooldown.GetFloat();
            AURoleOptions.EngineerInVentMaxTime = 1;
        }

        public override void OnFixedUpdateLowLoad(PlayerControl player)
        {
            if (RepellantInProtect.TryGetValue(player.PlayerId, out var vtime) && vtime + RepellantSkillDuration.GetInt() < GetTimeStamp())
            {
                RepellantInProtect.Remove(player.PlayerId);
                if (!DisableShieldAnimations.GetBool())
                    player.RpcGuardAndKill();
                else
                    player.RpcResetAbilityCooldown();
                player.Notify(GetString("RepellantSkillStop"));
            }
        }

        public override bool OnCheckMurderAsTarget(PlayerControl killer, PlayerControl target)
        {
        
            if (RepellantInProtect.ContainsKey(target.PlayerId) && killer.PlayerId != target.PlayerId)
            {
                
                if (RepellantInProtect[target.PlayerId] + RepellantSkillDuration.GetInt() >= GetTimeStamp(DateTime.UtcNow))
                {
                    
                    killer.Notify(GetString("RepellantBlockedMurder"));

                    
                    target.Notify(GetString("RepellantProtected"));

                    
                    return true;
                }
            }
            
            
            return false;
        }

        public override void OnEnterVent(PlayerControl pc, Vent AirConditioning)
        {
            if (AbilityLimit >= 1)
            {
              AbilityLimit -= 1;
                RepellantInProtect.Remove(pc.PlayerId);
                RepellantInProtect.Add(pc.PlayerId, GetTimeStamp());

                if (!pc.IsModClient())
                {
                    pc.RpcGuardAndKill(pc);
               }
                pc.Notify(GetString("RepellantOnGuard"), RepellantSkillDuration.GetFloat());
            }   
        }
    }
}
