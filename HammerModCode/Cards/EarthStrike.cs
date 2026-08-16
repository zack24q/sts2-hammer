using HammerMod.Characters;
using HammerMod.Gameplay;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;

namespace HammerMod.Cards;

[RegisterCard(typeof(HammerModCardPool))]
[RegisterCharacterStarterCard(typeof(HammerModCharacter), 1, Order = 40)]
[RegisterArchaicToothTranscendence(typeof(ImpactCrater))]
public sealed class EarthStrike : HammerCard, IChargeReleaseCard, IChargeContextDescriptionCard
{
    private static readonly int[] BaseDamage = [11, 16, 22, 30];
    private static readonly int[] UpgradedDamage = [15, 20, 27, 36];
    private static readonly int[] BaseStun = [0, 2, 4, 5];
    private static readonly int[] UpgradedStun = [1, 3, 5, 6];

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

    public EarthStrike() : base(2, CardType.Attack, CardRarity.Basic, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);

        var charge = BeginChargeRelease(cardPlay);
        var damage = ResolveDamage(charge, IsUpgraded);
        var stun = ResolveStun(charge, IsUpgraded);

        await DamageCmd.Attack(damage)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);

        if (cardPlay.Target.IsAlive)
            await HammerStun.Apply(choiceContext, this, cardPlay.Target, stun, cardPlay);

        await ReleaseCharge(choiceContext, charge, cardPlay);
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
