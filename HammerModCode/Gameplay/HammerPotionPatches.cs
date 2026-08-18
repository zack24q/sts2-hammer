using HammerLib = HarmonyLib;
using HammerMod.Powers;
using MegaCrit.Sts2.Core.Models;

namespace HammerMod.Gameplay;

[HammerLib.HarmonyPatch(typeof(PotionModel), nameof(PotionModel.RemoveBeforeUse))]
internal static class FreeMealPotionPatch
{
    [HammerLib.HarmonyPrefix]
    internal static bool PreservePotion(PotionModel __instance)
    {
        var hasFreeMeal = __instance.Owner.Creature.GetPower<FreeMealPower>() is not null;
        return ShouldRunOriginal(hasFreeMeal);
    }

    internal static bool ShouldRunOriginal(bool hasFreeMeal)
    {
        return !hasFreeMeal;
    }
}
