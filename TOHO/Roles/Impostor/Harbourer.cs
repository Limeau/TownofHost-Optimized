using System.Collections.Generic;
using System.Linq;
using TOHO.Roles.Core;

namespace TOHO.Roles.Impostor;

internal class Harbourer : RoleBase
{
    //===========================SETUP================================\\
    private const int Id = 41100;
    public override CustomRoles Role => CustomRoles.Harbourer;
    public override CustomRoles ThisRoleBase => CustomRoles.Shapeshifter;
    public override Custom_RoleType ThisRoleType => Custom_RoleType.ImpostorSupport;
    //==================================================================\\

    private static OptionItem HarbourerKillCooldown;
    private static OptionItem MaxRolesStored;
    public static OptionItem CanGuess;

    public static List<CustomRoles> StoredRoles = [CustomRoles.Harbourer];
    public static int Equipped = 0;
    
    public override void SetupCustomOption()
    {
        Options.SetupRoleOptions(Id, TabGroup.ImpostorRoles, CustomRoles.Harbourer);
        HarbourerKillCooldown = FloatOptionItem.Create(Id + 2, GeneralOption.KillCooldown, new(2.5f, 180f, 2.5f), 30f, TabGroup.ImpostorRoles, false)
            .SetParent(Options.CustomRoleSpawnChances[CustomRoles.Harbourer])
            .SetValueFormat(OptionFormat.Seconds);
        MaxRolesStored = IntegerOptionItem.Create(Id + 3, "MaxRolesStored411", new(1, 10, 1), 3, TabGroup.ImpostorRoles, false)
            .SetParent(Options.CustomRoleSpawnChances[CustomRoles.Harbourer]);
        CanGuess = BooleanOptionItem.Create(Id + 4, GeneralOption.CanGuess, false, TabGroup.ImpostorRoles, false)
            .SetParent(Options.CustomRoleSpawnChances[CustomRoles.Harbourer]);
    }

    public override void Add(byte playerId)
    {
        StoredRoles = [CustomRoles.Harbourer];
        Equipped = 0;
    }

    public override void UnShapeShiftButton(PlayerControl shapeshifter)
    {
        Equipped += 1;
        if (Equipped >= StoredRoles.Count) Equipped = 0;
        StoredRoles[Equipped].GetActualRoleName(out string realName);
        shapeshifter.Notify(Utils.ColorString(Utils.GetRoleColor(StoredRoles[Equipped]), $"<size=60%>You have equipped: {Translator.GetString(realName)}</size>"));
    }

    public override bool OnCheckMurderAsKiller(PlayerControl killer, PlayerControl target)
    {
        if (StoredRoles[Equipped] == CustomRoles.Harbourer) return true;
        StoredRoles[Equipped].GetStaticRoleClass().OnCheckMurderAsKiller(killer, target);
        StoredRoles.Remove(StoredRoles[Equipped]);
        Equipped = 0;
        return false;
    }

    public static void CheckTargetKill(PlayerControl killer, PlayerControl target)
    {
        if (StoredRoles.Count >= MaxRolesStored.GetInt() + 1)
        {
            return;
        }
        StoredRoles.Add(killer.GetCustomRole());
        target.GetCustomRole().GetActualRoleName(out string realName);
        target.Notify("<size=60%>You have stored the ability of " + Utils.ColorString(Utils.GetRoleColor(target.GetCustomRole()), realName) + "</size>");
    }

    public override void SetKillCooldown(byte id) => Main.AllPlayerKillCooldown[id] = HarbourerKillCooldown.GetFloat();
}