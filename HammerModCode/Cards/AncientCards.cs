using HammerMod.Characters;
using HammerMod.Gameplay;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;

namespace HammerMod.Cards;

[RegisterCard(typeof(HammerModCardPool))]
[RegisterDustyTomeCard(typeof(HammerModCharacter))]
public sealed class ImpactCrater : HammerCard, IChargeReleaseCard, IChargeContextDescriptionCard
{
    private static readonly int[] BaseDamage = [25, 33, 44, 57];
    private static readonly int[] UpgradedDamage = [37, 48, 63, 76];
    private static readonly int[] BaseStun = [4, 6, 8, 11];
    private static readonly int[] UpgradedStun = [6, 8, 11, 15];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.ComputedDamage(
            "Damage",
            static context => ResolveDamage(PreviewCharge(context, true), context.IsUpgraded),
            baseValue: BaseDamage[0]),
        ModCardVars.Computed(
            "Stun",
            static context => ResolveStun(PreviewCharge(context, true), context.IsUpgraded),
            baseValue: BaseStun[0]),
        .. ChargeTierVars("DamageAt", BaseDamage, UpgradedDamage),
        .. ChargeTierVars("StunAt", BaseStun, UpgradedStun)
    ];

    public ImpactCrater()
        : base(1, CardType.Attack, CardRarity.Ancient, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);

        var charge = ChargeLevel;
        await Attack(
            choiceContext,
            cardPlay.Target,
            ResolveDamage(charge, IsUpgraded));

        if (cardPlay.Target.IsAlive)
        {
            await HammerStun.Apply(
                choiceContext,
                this,
                cardPlay.Target,
                ResolveStun(charge, IsUpgraded),
                cardPlay);
        }

        await ReleaseCharge(choiceContext, charge);
    }

    private static int ResolveDamage(int charge, bool upgraded)
    {
        return (upgraded ? UpgradedDamage : BaseDamage)[Math.Clamp(charge, 0, 3)];
    }

    private static int ResolveStun(int charge, bool upgraded)
    {
        return (upgraded ? UpgradedStun : BaseStun)[Math.Clamp(charge, 0, 3)];
    }
}
