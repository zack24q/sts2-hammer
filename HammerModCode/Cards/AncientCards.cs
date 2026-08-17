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
public sealed class ImpactCrater : HammerCard, IChargeReleaseCard, ICombatPreviewDescriptionCard
{
    private static readonly int[] BaseDamage = [14, 18, 24, 32];
    private static readonly int[] UpgradedDamage = [18, 22, 30, 40];
    private static readonly int[] BaseStun = [4, 7, 11, 16];
    private static readonly int[] UpgradedStun = [5, 9, 14, 20];

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
        .. ChargeTierVars("DamageAt", BaseDamage),
        .. ChargeTierVars("StunAt", BaseStun)
    ];

    public ImpactCrater()
        : base(1, CardType.Attack, CardRarity.Ancient, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);

        var charge = BeginChargeRelease(cardPlay);
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

        await ReleaseCharge(choiceContext, charge, cardPlay);
    }

    protected override void OnUpgrade()
    {
        UpgradeChargeTierVars("DamageAt", BaseDamage, UpgradedDamage);
        UpgradeChargeTierVars("StunAt", BaseStun, UpgradedStun);
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
