using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib;
using STS2RitsuLib.Combat.CardTargeting;
using STS2RitsuLib.Keywords;

namespace HammerMod.Gameplay;

internal static class HammerKeywords
{
    internal static CardKeyword Charge { get; private set; }
    internal static CardKeyword ChargeRelease { get; private set; }
    internal static CardKeyword Stun { get; private set; }

    internal static void Register()
    {
        var registry = RitsuLibFramework.GetKeywordRegistry(Entry.ModId);

        Charge = registry
            .RegisterCardKeywordOwnedByLocNamespace(
                "CHARGE",
                iconPath: string.Empty,
                ModKeywordCardDescriptionPlacement.None,
                includeInCardHoverTip: false)
            .CardKeywordValue;

        ChargeRelease = registry
            .RegisterCardKeywordOwnedByLocNamespace(
                "CHARGE_RELEASE",
                iconPath: string.Empty,
                ModKeywordCardDescriptionPlacement.BeforeCardDescription,
                includeInCardHoverTip: true)
            .CardKeywordValue;

        Stun = registry
            .RegisterCardKeywordOwnedByLocNamespace(
                "STUN",
                iconPath: string.Empty,
                ModKeywordCardDescriptionPlacement.None,
                includeInCardHoverTip: false)
            .CardKeywordValue;
    }
}

internal static class HammerTargetTypes
{
    internal static TargetType FaceOff { get; private set; }

    internal static void Register()
    {
        FaceOff = CustomTargetType.RegisterSingleTargetType(
            Entry.ModId,
            "FACE_OFF",
            static (Creature target, Player user) => IsFaceOffTarget(user, target));
    }

    internal static bool IsFaceOffTarget(Player user, Creature target)
    {
        return target.IsMonster
            && target.IsAlive
            && ModelDb.Singleton<HammerRules>().HasPlayedAttackThisTurn(user.Creature);
    }
}
