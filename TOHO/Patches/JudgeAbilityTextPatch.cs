using HarmonyLib;
using System.Linq;
using TMPro;
using UnityEngine;

namespace TOHO.Patches;

[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start))]
public static class JudgeAbilityTextPatch
{
    private static readonly Vector3 PositionOffset = new(0.81f, -2.16f, 0f);
    private static readonly Vector3 ScaleMultiplier = new(1.35f, 1.35f, 0f);
    private static readonly Color TextColor = new(1f, 0.32f, 0f, 1f);

    private static readonly string[] NameHints = { "judge", "overrule" };
    private static readonly string[] ContentHints = { "overrule" };

    public static void Postfix(MeetingHud __instance)
    {
        if (__instance == null) return;
        if (!PlayerControl.LocalPlayer) return;
        if (!PlayerControl.LocalPlayer.Is(CustomRoles.JudgeTOHO)) return;
        if (!PlayerControl.LocalPlayer.IsAlive()) return;

        _ = new LateTask(() => TryAdjustJudgeText(__instance), 0.1f, "Judge Ability Text Adjust");
    }

    private static void TryAdjustJudgeText(MeetingHud meetingHud)
    {
        if (meetingHud == null) return;
        if (!GameStates.IsMeeting) return;

        var texts = meetingHud.GetComponentsInChildren<TMP_Text>(true);
        if (texts == null || texts.Length == 0) return;

        TMP_Text target = null;

        foreach (var t in texts)
        {
            if (t == null) continue;

            var goName = t.gameObject.name?.ToLowerInvariant() ?? "";
            var content = t.text?.ToLowerInvariant() ?? "";

            bool nameMatch = NameHints.Any(h => goName.Contains(h));
            bool contentMatch = ContentHints.Any(h => content.Contains(h));

            if (nameMatch || contentMatch)
            {
                target = t;
                break;
            }
        }

        if (target == null)
        {
            Logger.Warn("Could not auto-locate the Judge ability text. Candidates:", "JudgeAbilityTextPatch");
            foreach (var t in texts)
            {
                if (t == null || string.IsNullOrWhiteSpace(t.text)) continue;
                Logger.Warn($"  \"{t.gameObject.name}\": \"{t.text}\"", "JudgeAbilityTextPatch");
            }
            return;
        }

        var rt = target.rectTransform;
        rt.localPosition = PositionOffset;
        rt.localScale = ScaleMultiplier;
        target.color = TextColor;

        Logger.Info($"Adjusted Judge ability text on \"{target.gameObject.name}\"", "JudgeAbilityTextPatch");
    }
}