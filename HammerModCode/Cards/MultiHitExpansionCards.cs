using HammerMod.Characters;
using HammerMod.Gameplay;
using HammerMod.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;

namespace HammerMod.Cards;

[RegisterCard(typeof(HammerModCardPool))]
public sealed class DoubleSideSwing : HammerCard, IContextualDescriptionCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(3, MegaCrit.Sts2.Core.ValueProps.ValueProp.Move),
        ModCardVars.Computed(
            "Hits",
            static context => PreviewAttackHitCount(
                context,
                context.GetCardIntOrDefault("BaseHits", 2)),
            baseValue: 2),
        new IntVar("BaseHits", 2)
    ];

    public DoubleSideSwing()
        : base(0, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        await Attack(
            choiceContext,
            cardPlay.Target,
            DynamicVars.Damage.BaseValue,
            DynamicVars["BaseHits"].IntValue);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["BaseHits"].UpgradeValueBy(1);
    }
}

[RegisterCard(typeof(HammerModCardPool))]
public sealed class IronbugFollowUp : HammerCard, IContextualDescriptionCard
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(3, MegaCrit.Sts2.Core.ValueProps.ValueProp.Move),
        ModCardVars.Computed(
            "Hits",
            static context => PreviewAttackHitCount(
                context,
                context.GetCardIntOrDefault("BaseHits", 2)),
            baseValue: 2),
        new IntVar("BaseHits", 2),
        new CardsVar("Cards", 1)
    ];

    public IronbugFollowUp()
        : base(0, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        await Attack(
            choiceContext,
            cardPlay.Target,
            DynamicVars.Damage.BaseValue,
            DynamicVars["BaseHits"].IntValue);
        await CardPileCmd.Draw(
            choiceContext,
            DynamicVars["Cards"].IntValue,
            Owner);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2);
    }
}

[RegisterCard(typeof(HammerModCardPool))]
public sealed class SlidingCombo : HammerCard, IContextualDescriptionCard
{
    protected override bool ShouldGlowGoldInternal => HasChargeAtLeast(2);

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(2, MegaCrit.Sts2.Core.ValueProps.ValueProp.Move),
        ModCardVars.Computed(
            "Hits",
            static context => PreviewAttackHitCount(
                context,
                context.GetCardIntOrDefault("BaseHits", 3)),
            baseValue: 3),
        new IntVar("BaseHits", 3),
        new IntVar("RequiredCharge", 2),
        new PowerVar<StrengthPower>(1)
    ];

    public SlidingCombo()
        : base(0, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        var gainStrength = ChargeLevel >= DynamicVars["RequiredCharge"].IntValue;
        await Attack(
            choiceContext,
            cardPlay.Target,
            DynamicVars.Damage.BaseValue,
            DynamicVars["BaseHits"].IntValue);

        if (gainStrength)
        {
            await PowerCmd.Apply<SlidingComboStrengthPower>(
                choiceContext,
                Owner.Creature,
                DynamicVars.Strength.BaseValue,
                Owner.Creature,
                this);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Strength.UpgradeValueBy(1);
    }
}

[RegisterCard(typeof(HammerModCardPool))]
public sealed class SweepingPreparation : HammerCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(9, MegaCrit.Sts2.Core.ValueProps.ValueProp.Move),
        new CardsVar("Cards", 1)
    ];

    public SweepingPreparation()
        : base(1, CardType.Attack, CardRarity.Common, TargetType.AllEnemies)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await AttackAll(choiceContext, DynamicVars.Damage.BaseValue);
        await CardPileCmd.Draw(
            choiceContext,
            DynamicVars["Cards"].IntValue,
            Owner);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3);
    }
}

[RegisterCard(typeof(HammerModCardPool))]
public sealed class PoundingSmash : HammerCard, IContextualDescriptionCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(6, MegaCrit.Sts2.Core.ValueProps.ValueProp.Move),
        ModCardVars.Computed(
            "Hits",
            static context => PreviewAttackHitCount(
                context,
                context.GetCardIntOrDefault("BaseHits", 3)),
            baseValue: 3),
        new IntVar("BaseHits", 3),
        new IntVar("Stun", 4)
    ];

    public PoundingSmash()
        : base(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        await Attack(
            choiceContext,
            cardPlay.Target,
            DynamicVars.Damage.BaseValue,
            DynamicVars["BaseHits"].IntValue);

        if (cardPlay.Target.IsAlive)
        {
            await HammerStun.Apply(
                choiceContext,
                this,
                cardPlay.Target,
                DynamicVars["Stun"].IntValue,
                cardPlay);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2);
        DynamicVars["Stun"].UpgradeValueBy(2);
    }
}

[RegisterCard(typeof(HammerModCardPool))]
public sealed class TrueSpinningImpact : HammerCard, IContextualDescriptionCard
{
    protected override bool HasEnergyCostX => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(8, MegaCrit.Sts2.Core.ValueProps.ValueProp.Move),
        ModCardVars.Computed(
            "Hits",
            static context => PreviewHits(context),
            baseValue: 0),
        ModCardVars.Computed(
            "ResolvedStun",
            static context => PreviewHits(context)
                * context.GetCardIntOrDefault("StunPerHit", 1),
            baseValue: 0),
        new IntVar("StunPerHit", 1)
    ];

    public TrueSpinningImpact()
        : base(0, CardType.Attack, CardRarity.Rare, TargetType.AllEnemies)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var originalHitCount = ResolveEnergyXValue();
        if (originalHitCount <= 0)
            return;

        var targets = CombatState!.HittableEnemies.ToArray();
        var resolvedHitCount = ResolveAttackHitCount(originalHitCount);
        await AttackAll(
            choiceContext,
            DynamicVars.Damage.BaseValue,
            originalHitCount);

        foreach (var target in targets.Where(static target => target.IsAlive))
        {
            await HammerStun.Apply(
                choiceContext,
                this,
                target,
                resolvedHitCount * DynamicVars["StunPerHit"].IntValue,
                cardPlay);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2);
        DynamicVars["StunPerHit"].UpgradeValueBy(1);
    }

    private static int PreviewHits(ComputedDynamicVarContext context)
    {
        return PreviewAttackHitCount(context, PreviewEnergyXValue(context));
    }
}

[RegisterCard(typeof(HammerModCardPool))]
public sealed class Challenger : HammerCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<StrengthPower>(2)
    ];

    public Challenger()
        : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<ChallengerPower>(
            choiceContext,
            Owner.Creature,
            DynamicVars.Strength.BaseValue,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Strength.UpgradeValueBy(1);
    }
}

[RegisterCard(typeof(HammerModCardPool))]
public sealed class WirebugContinuation : HammerCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IntVar("Charge", 1),
        new EnergyVar("RequiredEnergy", 2)
    ];

    public WirebugContinuation()
        : base(2, CardType.Power, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<WirebugContinuationPower>(
            choiceContext,
            Owner.Creature,
            DynamicVars["Charge"].IntValue,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Charge"].UpgradeValueBy(1);
    }
}

[RegisterCard(typeof(HammerModCardPool))]
public sealed class WeaknessExploit : HammerCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IntVar("BonusPercent", 50)
    ];

    public WeaknessExploit()
        : base(2, CardType.Power, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<WeaknessExploitPower>(
            choiceContext,
            Owner.Creature,
            DynamicVars["BonusPercent"].IntValue,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["BonusPercent"].UpgradeValueBy(50);
    }
}

[RegisterCard(typeof(HammerModCardPool))]
public sealed class ChargeSwitchCourage : HammerCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<StrengthPower>(1)
    ];

    public ChargeSwitchCourage()
        : base(3, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<ChargeSwitchCouragePower>(
            choiceContext,
            Owner.Creature,
            DynamicVars.Strength.BaseValue,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}

[RegisterCard(typeof(HammerModCardPool))]
public sealed class Partbreaker : HammerCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<VulnerablePower>(1)
    ];

    public Partbreaker()
        : base(3, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<PartbreakerPower>(
            choiceContext,
            Owner.Creature,
            DynamicVars.Vulnerable.BaseValue,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}

[RegisterCard(typeof(HammerModCardPool))]
public sealed class ComboBoost : HammerCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IntVar("ExtraHits", 3)
    ];

    public ComboBoost()
        : base(3, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<ComboBoostPower>(
            choiceContext,
            Owner.Creature,
            DynamicVars["ExtraHits"].IntValue,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["ExtraHits"].UpgradeValueBy(2);
    }
}

[RegisterCard(typeof(HammerModCardPool))]
public sealed class BloodRite : HammerCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IntVar("DamageStep", 10),
        new IntVar("Healing", 1)
    ];

    public BloodRite()
        : base(3, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<BloodRitePower>(
            choiceContext,
            Owner.Creature,
            DynamicVars["Healing"].IntValue,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
