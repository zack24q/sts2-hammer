using HammerMod.Characters;
using HammerMod.Gameplay;
using HammerMod.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Models.Capabilities;
using STS2RitsuLib.Scaffolding.Content;

namespace HammerMod.Cards;

[RegisterCard(typeof(HammerModCardPool))]
public sealed class ContinuousSideSwing : HammerCard, ICombatPreviewDescriptionCard
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

    public ContinuousSideSwing()
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
public sealed class WirebugSpin : HammerCard, ICombatPreviewDescriptionCard
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

    public WirebugSpin()
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
        DynamicVars["Cards"].UpgradeValueBy(1);
    }
}

[RegisterCard(typeof(HammerModCardPool))]
public sealed class AffinitySliding : HammerCard
{
    protected override bool ShouldGlowGoldInternal => HasChargeAtLeast(2);

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IntVar("RequiredCharge", 2),
        new IntVar("NormalStrength", 2),
        new IntVar("ChargedStrength", 4)
    ];

    public AffinitySliding()
        : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var strength = ChargeLevel >= DynamicVars["RequiredCharge"].IntValue
            ? DynamicVars["ChargedStrength"].IntValue
            : DynamicVars["NormalStrength"].IntValue;
        await PowerCmd.Apply<AffinitySlidingStrengthPower>(
            choiceContext,
            Owner.Creature,
            strength,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["NormalStrength"].UpgradeValueBy(2);
        DynamicVars["ChargedStrength"].UpgradeValueBy(2);
    }
}

[RegisterCard(typeof(HammerModCardPool))]
public sealed class SweepThePath : HammerCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(6, MegaCrit.Sts2.Core.ValueProps.ValueProp.Move),
        new CardsVar("Cards", 1)
    ];

    public SweepThePath()
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
public sealed class InvincibleWindFireWheel : HammerCard, ICombatPreviewDescriptionCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(3, MegaCrit.Sts2.Core.ValueProps.ValueProp.Move),
        ModCardVars.Computed(
            "Hits",
            static context => PreviewAttackHitCount(
                context,
                context.GetCardIntOrDefault("BaseHits", 4)),
            baseValue: 4),
        ModCardVars.Computed(
            "ResolvedStun",
            static context => PreviewAttackHitCount(
                    context,
                    context.GetCardIntOrDefault("BaseHits", 4))
                * context.GetCardIntOrDefault("StunPerHit", 2),
            baseValue: 8),
        new IntVar("BaseHits", 4),
        new IntVar("StunPerHit", 2)
    ];

    public InvincibleWindFireWheel()
        : base(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        var baseHits = DynamicVars["BaseHits"].IntValue;
        var resolvedHits = ResolveAttackHitCount(baseHits);
        await Attack(
            choiceContext,
            cardPlay.Target,
            DynamicVars.Damage.BaseValue,
            baseHits);

        if (cardPlay.Target.IsAlive)
        {
            await HammerStun.Apply(
                choiceContext,
                this,
                cardPlay.Target,
                resolvedHits * DynamicVars["StunPerHit"].IntValue,
                cardPlay);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["BaseHits"].UpgradeValueBy(2);
    }
}

[RegisterCard(typeof(HammerModCardPool))]
public sealed class TrueSpinningImpact : HammerCard, ICombatPreviewDescriptionCard
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
            static context => PreviewEnergyXValue(context)
                * context.GetCardIntOrDefault("StunPerEnergy", 2),
            baseValue: 0),
        new IntVar("BonusHits", 0),
        new IntVar("StunPerEnergy", 2)
    ];

    public TrueSpinningImpact()
        : base(0, CardType.Attack, CardRarity.Rare, TargetType.AllEnemies)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var energyX = ResolveEnergyXValue();
        var originalHitCount = energyX + DynamicVars["BonusHits"].IntValue;
        if (originalHitCount <= 0)
            return;

        var targets = CombatState!.HittableEnemies.ToArray();
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
                energyX * DynamicVars["StunPerEnergy"].IntValue,
                cardPlay);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["BonusHits"].UpgradeValueBy(1);
    }

    private static int PreviewHits(ComputedDynamicVarContext context)
    {
        var originalHitCount = PreviewEnergyXValue(context)
            + context.GetCardIntOrDefault("BonusHits", 0);
        return PreviewAttackHitCount(context, originalHitCount);
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
        : base(2, CardType.Power, CardRarity.Rare, TargetType.Self)
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
public sealed class HarderWithEverySmash : HammerCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<StrengthPower>(3)
    ];

    public HarderWithEverySmash()
        : base(3, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<HarderWithEverySmashPower>(
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
        : base(2, CardType.Power, CardRarity.Rare, TargetType.Self)
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
public sealed class OneMoreBonk : HammerCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IntVar("ExtraHits", 3)
    ];

    public OneMoreBonk()
        : base(3, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<OneMoreBonkPower>(
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
        : base(2, CardType.Power, CardRarity.Rare, TargetType.Self)
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
