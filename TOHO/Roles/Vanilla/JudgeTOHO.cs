using System.Collections.Generic;
using System.Linq;
using TOHO.Roles.Core;

namespace TOHO.Roles.Vanilla;

internal class JudgeTOHO : RoleBase
{
    //===========================SETUP================================\\
    public override CustomRoles Role => CustomRoles.JudgeTOHO;
    private const int Id = 47000;
    private static readonly HashSet<byte> playerIdList = [];
    public static bool HasEnabled => playerIdList.Any();

    public override CustomRoles ThisRoleBase => CustomRoles.Judge;
    public override Custom_RoleType ThisRoleType => Custom_RoleType.CrewmateVanilla;
    //==================================================================\\

    private static OptionItem TaskReq;

    public override void SetupCustomOption()
    {
        Options.SetupRoleOptions(Id, TabGroup.CrewmateRoles, CustomRoles.JudgeTOHO);
        TaskReq = IntegerOptionItem.Create(Id + 3, GeneralOption.JudgeBase_JudgeTaskRequirementPercentage, new(0, 100, 5), 25, TabGroup.CrewmateRoles, false)
            .SetParent(Options.CustomRoleSpawnChances[CustomRoles.JudgeTOHO])
            .SetValueFormat(OptionFormat.Percent);
    }

    public override void Init()
    {
        playerIdList.Clear();
    }
    public override void Add(byte playerId)
    {
        playerIdList.Add(playerId);
    }

    public static void ApplyGameOptionsForOthers(PlayerControl player)
    {
        AURoleOptions.JudgeTaskRequirementPercentage = TaskReq.GetInt();
    }
}
