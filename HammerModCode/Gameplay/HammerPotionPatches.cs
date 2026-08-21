using System.Runtime.CompilerServices;
using HammerLib = HarmonyLib;
using HammerMod.Powers;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.TestSupport;

namespace HammerMod.Gameplay;

[HammerLib.HarmonyPatch(typeof(PotionModel), nameof(PotionModel.RemoveBeforeUse))]
internal static class FreeMealPotionPatch
{
    private sealed class PreservationMarker;

    private static readonly ConditionalWeakTable<PotionModel, PreservationMarker> PreservedPotions = new();

    [HammerLib.HarmonyPrefix]
    internal static bool PreservePotion(PotionModel __instance)
    {
        var hasFreeMeal = __instance.Owner.Creature.GetPower<FreeMealPower>() is not null;
        var shouldRunOriginal = ShouldRunOriginal(hasFreeMeal);
        if (!shouldRunOriginal)
            PreservedPotions.GetValue(__instance, static _ => new PreservationMarker());

        return shouldRunOriginal;
    }

    internal static bool ShouldRunOriginal(bool hasFreeMeal)
    {
        return !hasFreeMeal;
    }

    internal static bool TryTakePreservedPotion(PotionModel potion)
    {
        return PreservedPotions.Remove(potion);
    }
}

[HammerLib.HarmonyPatch(typeof(PotionModel), nameof(PotionModel.OnUseWrapper))]
internal static class FreeMealPotionUsePatch
{
    [HammerLib.HarmonyPostfix]
    internal static void RestorePreservedPotion(PotionModel __instance, ref Task __result)
    {
        if (!FreeMealPotionPatch.TryTakePreservedPotion(__instance))
            return;

        __result = CompleteUseThenRestore(__result, () => RestorePotionAvailability(__instance));
    }

    internal static async Task CompleteUseThenRestore(Task useTask, Action restore)
    {
        try
        {
            await useTask;
        }
        finally
        {
            restore();
        }
    }

    private static void RestorePotionAvailability(PotionModel potion)
    {
        if (TestMode.IsOff
            && NRun.Instance is { } run
            && LocalContext.IsMe(potion.Owner))
        {
            run.GlobalUi.TopBar.PotionContainer.OnPotionUseOrDiscardCanceled(potion);
        }

        potion.AfterUsageCanceled();
    }
}
