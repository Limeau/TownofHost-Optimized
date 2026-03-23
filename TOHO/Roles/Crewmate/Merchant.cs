using System.Collections.Generic;
using System.Linq;
using TOHO.Roles.Modifiers;
using static TOHO.Options;
using static TOHO.Translator;

namespace TOHO.Roles.Crewmate;

internal class Merchant : RoleBase
{
    //===========================SETUP================================\\
    public override CustomRoles Role => CustomRoles.Merchant;
    private const int Id = 8800;
    public override CustomRoles ThisRoleBase => CustomRoles.Crewmate;
    public override Custom_RoleType ThisRoleType => Custom_RoleType.CrewmateSupport;
    //==================================================================\\

    private static List<CustomRoles> Modifiers = [];
    private static readonly Dictionary<byte, int> ModifiersSold = [];
    private static readonly Dictionary<byte, HashSet<byte>> bribedKiller = [];

    private static OptionItem OptionMaxSell;
    private static OptionItem OptionMoneyPerSell;
    private static OptionItem OptionMoneyRequiredToBribe;
    private static OptionItem OptionNotifyBribery;
    private static OptionItem OptionCanTargetCrew;
    private static OptionItem OptionCanTargetImpostor;
    private static OptionItem OptionCanTargetNeutral;
    private static OptionItem OptionCanTargetCoven;
    private static OptionItem OptionCanSellHelpful;
    private static OptionItem OptionCanSellHarmful;
    private static OptionItem OptionCanSellNeutral;
    private static OptionItem OptionSellOnlyHarmfulToEvil;
    private static OptionItem OptionSellOnlyHelpfulToCrew;
    private static OptionItem OptionSellOnlyEnabledModifiers;

    private static int GetCurrentAmountOfMoney(byte playerId)
    {
        return (ModifiersSold[playerId] * OptionMoneyPerSell.GetInt()) - (bribedKiller[playerId].Count * OptionMoneyRequiredToBribe.GetInt());
    }

    public override void SetupCustomOption()
    {
        SetupRoleOptions(Id, TabGroup.CrewmateRoles, CustomRoles.Merchant);
        OptionMaxSell = IntegerOptionItem.Create(Id + 2, "MerchantMaxSell", new(1, 99, 1), 5, TabGroup.CrewmateRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.Merchant]).SetValueFormat(OptionFormat.Times);
        OptionMoneyPerSell = IntegerOptionItem.Create(Id + 3, "MerchantMoneyPerSell", new(1, 99, 1), 1, TabGroup.CrewmateRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.Merchant]).SetValueFormat(OptionFormat.Times);
        OptionMoneyRequiredToBribe = IntegerOptionItem.Create(Id + 4, "MerchantMoneyRequiredToBribe", new(1, 99, 1), 5, TabGroup.CrewmateRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.Merchant]).SetValueFormat(OptionFormat.Times);
        OptionNotifyBribery = BooleanOptionItem.Create(Id + 5, "MerchantNotifyBribery", false, TabGroup.CrewmateRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.Merchant]);
        OptionCanTargetCrew = BooleanOptionItem.Create(Id + 6, "MerchantTargetCrew", true, TabGroup.CrewmateRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.Merchant]);
        OptionCanTargetImpostor = BooleanOptionItem.Create(Id + 7, "MerchantTargetImpostor", true, TabGroup.CrewmateRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.Merchant]);
        OptionCanTargetNeutral = BooleanOptionItem.Create(Id + 8, "MerchantTargetNeutral", true, TabGroup.CrewmateRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.Merchant]);
        OptionCanTargetCoven = BooleanOptionItem.Create(Id + 16, "MerchantTargetCoven", true, TabGroup.CrewmateRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.Merchant]);
        OptionCanSellHelpful = BooleanOptionItem.Create(Id + 9, "MerchantSellHelpful", true, TabGroup.CrewmateRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.Merchant]);
        OptionCanSellHarmful = BooleanOptionItem.Create(Id + 10, "MerchantSellHarmful", true, TabGroup.CrewmateRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.Merchant]);
        OptionCanSellNeutral = BooleanOptionItem.Create(Id + 11, "MerchantSellMixed", true, TabGroup.CrewmateRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.Merchant]);
        OptionSellOnlyHarmfulToEvil = BooleanOptionItem.Create(Id + 13, "MerchantSellHarmfulToEvil", false, TabGroup.CrewmateRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.Merchant]);
        OptionSellOnlyHelpfulToCrew = BooleanOptionItem.Create(Id + 14, "MerchantSellHelpfulToCrew", false, TabGroup.CrewmateRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.Merchant]);
        OptionSellOnlyEnabledModifiers = BooleanOptionItem.Create(Id + 15, "MerchantSellOnlyEnabledModifiers", false, TabGroup.CrewmateRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.Merchant]);

        Options.OverrideTasksData.Create(Id + 17, TabGroup.CrewmateRoles, CustomRoles.Merchant);
    }
    public override void Init()
    {
        Modifiers.Clear();
        ModifiersSold.Clear();
        bribedKiller.Clear();

        if (OptionCanSellHelpful.GetBool())
        {
            Modifiers.AddRange(GroupedModifiers[ModifierTypes.Helpful]);
        }

        if (OptionCanSellHarmful.GetBool())
        {
            Modifiers.AddRange(GroupedModifiers[ModifierTypes.Harmful]);
        }

        if (OptionCanSellNeutral.GetBool())
        {
            Modifiers.AddRange(GroupedModifiers[ModifierTypes.Mixed]);
        }
        if (OptionSellOnlyEnabledModifiers.GetBool())
        {
            Modifiers = Modifiers.Where(role => role.GetMode() != 0).ToList();
        }
    }

    public override void Add(byte playerId)
    {
        ModifiersSold[playerId] = 0;
        bribedKiller.TryAdd(playerId, []);
    }
    public override void Remove(byte playerId)
    {
        ModifiersSold.Remove(playerId);
        bribedKiller.Remove(playerId);
    }
    public override bool OnCheckMurderAsTarget(PlayerControl killer, PlayerControl target)
    {
        return !OnClientMurder(killer, target);
    }
    public override bool OnTaskComplete(PlayerControl player, int completedTaskCount, int totalTaskCount)
    {
        if (!player.IsAlive()) return true;

        if (ModifiersSold[player.PlayerId] >= OptionMaxSell.GetInt())
        {
            return true;
        }

        if (Modifiers.Count == 0)
        {
            player.Notify(Utils.ColorString(Utils.GetRoleColor(CustomRoles.Merchant), GetString("MerchantModifierSellFail")));
            Logger.Info("No Modifiers to sell.", "Merchant");
            return true;
        }

        var rd = IRandom.Instance;
        CustomRoles Modifier = Modifiers.RandomElement();

        List<PlayerControl> AllAlivePlayer =
            Main.AllAlivePlayerControls.Where(x =>
                x.PlayerId != player.PlayerId
                &&
                (!x.Is(CustomRoles.Stubborn))
                &&
                !Modifier.IsConverted()
                &&
                CustomRolesHelper.CheckModifierConfilct(Modifier, x, checkLimitModifiers: false)
                &&
                (!Cleanser.CantGetModifier() || (Cleanser.CantGetModifier() && !x.Is(CustomRoles.Cleansed)))
                &&
                (
                    (OptionCanTargetCrew.GetBool() && x.GetCustomRole().IsCrewmate())
                    ||
                    (OptionCanTargetImpostor.GetBool() && x.GetCustomRole().IsImpostor())
                    ||
                    (OptionCanTargetNeutral.GetBool() && x.GetCustomRole().IsNeutral())
                    ||
                    (OptionCanTargetCoven.GetBool() && x.GetCustomRole().IsCoven())
                )
            ).ToList();

        if (AllAlivePlayer.Count > 0)
        {
            bool helpfulModifier = GroupedModifiers[ModifierTypes.Helpful].Contains(Modifier);
            bool harmfulModifier = GroupedModifiers[ModifierTypes.Harmful].Contains(Modifier);

            if (helpfulModifier && OptionSellOnlyHarmfulToEvil.GetBool())
            {
                AllAlivePlayer = AllAlivePlayer.Where(a => a.GetCustomRole().IsCrewmate()).ToList();
            }

            if (harmfulModifier && OptionSellOnlyHelpfulToCrew.GetBool())
            {
                AllAlivePlayer = AllAlivePlayer.Where(a =>
                    a.GetCustomRole().IsImpostor()
                    ||
                    a.GetCustomRole().IsNeutral()
                    ||
                    a.GetCustomRole().IsCoven()

                ).ToList();
            }

            if (AllAlivePlayer.Count < 1)
            {
                SellFail(player);
                return true;
            }

            PlayerControl target = AllAlivePlayer.RandomElement();

            target.RpcSetCustomRole(Modifier);
            target.Notify(Utils.ColorString(Utils.GetRoleColor(CustomRoles.Merchant), GetString("MerchantModifierSell")));
            player.Notify(Utils.ColorString(Utils.GetRoleColor(CustomRoles.Merchant), GetString("MerchantModifierDelivered")));

            target.AddInSwitchModifiers(target, Modifier);

            ModifiersSold[player.PlayerId] += 1;
        }
        else
        {
            SellFail(player);
            return true;
        }

        static void SellFail(PlayerControl player)
        {
            player.Notify(Utils.ColorString(Utils.GetRoleColor(CustomRoles.Merchant), GetString("MerchantModifierSellFail")));
            Logger.Info("All Alive Player Count = 0", "Merchant");
        }

        return true;
    }
    public override bool OnRoleGuess(bool isUI, PlayerControl target, PlayerControl pc, CustomRoles role, ref bool guesserSuicide)
    {
        if (role != CustomRoles.Merchant) return false;
        if (IsBribedKiller(pc, target))
        {
            pc.ShowInfoMessage(isUI, GetString("BribedByMerchant2"));
            return true;
        }
        return false;
    }

    public static bool OnClientMurder(PlayerControl killer, PlayerControl target)
    {
        if (IsBribedKiller(killer, target))
        {
            NotifyBribery(killer, target);
            return true;
        }

        if (GetCurrentAmountOfMoney(target.PlayerId) >= OptionMoneyRequiredToBribe.GetInt())
        {
            NotifyBribery(killer, target);
            bribedKiller[target.PlayerId].Add(killer.PlayerId);
            return true;
        }

        return false;
    }

    public static bool IsBribedKiller(PlayerControl killer, PlayerControl target) => bribedKiller.TryGetValue(target.PlayerId, out var targets) && targets.Contains(killer.PlayerId);

    private static void NotifyBribery(PlayerControl killer, PlayerControl target)
    {
        killer.Notify(Utils.ColorString(Utils.GetRoleColor(CustomRoles.Merchant), GetString("BribedByMerchant")));

        if (OptionNotifyBribery.GetBool())
        {
            target.Notify(Utils.ColorString(Utils.GetRoleColor(CustomRoles.Merchant), GetString("MerchantKillAttemptBribed")));
        }
    }
}
