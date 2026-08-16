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
using STS2RitsuLib.Scaffolding.Content;

namespace HammerMod.Cards;

[RegisterCard(typeof(HammerModCardPool))]
public sealed class SideSmash : HammerCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(6, MegaCrit.Sts2.Core.ValueProps.ValueProp.Move),
        new IntVar("Stun", 3)
    ];

    public SideSmash()
        : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        await Attack(choiceContext, cardPlay.Target, DynamicVars.Damage.BaseValue);
        await HammerStun.Apply(
            choiceContext,
            this,
            cardPlay.Target,
            DynamicVars["Stun"].IntValue,
            cardPlay);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3);
        DynamicVars["Stun"].UpgradeValueBy(1);
    }
}

[RegisterCard(typeof(HammerModCardPool))]
public sealed class ChargedUpswing : HammerCard, IChargeReleaseCard, IChargeContextDescriptionCard
{
    private static readonly int[] BaseStun = [3, 5, 8, 12];
    private static readonly int[] UpgradedStun = [4, 6, 9, 15];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(14, MegaCrit.Sts2.Core.ValueProps.ValueProp.Move),
        ModCardVars.Computed(
            "Stun",
            static context => ResolveStun(PreviewCharge(context, true), context.IsUpgraded),
            baseValue: BaseStun[0]),
        .. ChargeTierVars("StunAt", BaseStun, UpgradedStun)
    ];

    public ChargedUpswing()
        : base(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        var charge = BeginChargeRelease(cardPlay);
        await Attack(choiceContext, cardPlay.Target, DynamicVars.Damage.BaseValue);
        await HammerStun.Apply(
            choiceContext,
            this,
            cardPlay.Target,
            ResolveStun(charge, IsUpgraded),
            cardPlay);
        await ReleaseCharge(choiceContext, charge, cardPlay);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(5);
    }

    private static int ResolveStun(int charge, bool upgraded)
    {
        return (upgraded ? UpgradedStun : BaseStun)[Math.Clamp(charge, 0, 3)];
    }
}

[RegisterCard(typeof(HammerModCardPool))]
public sealed class RisingDragonHammer : HammerCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(9, MegaCrit.Sts2.Core.ValueProps.ValueProp.Move),
        new IntVar("Stun", 7)
    ];

    public RisingDragonHammer()
        : base(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        await Attack(choiceContext, cardPlay.Target, DynamicVars.Damage.BaseValue);
        await HammerStun.Apply(
            choiceContext,
            this,
            cardPlay.Target,
            DynamicVars["Stun"].IntValue,
            cardPlay);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3);
        DynamicVars["Stun"].UpgradeValueBy(2);
    }
}

[RegisterCard(typeof(HammerModCardPool))]
public sealed class GroundShock : HammerCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(15, MegaCrit.Sts2.Core.ValueProps.ValueProp.Move),
        new IntVar("Stun", 3)
    ];

    public GroundShock()
        : base(2, CardType.Attack, CardRarity.Common, TargetType.AllEnemies)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await AttackAll(choiceContext, DynamicVars.Damage.BaseValue);
        foreach (var enemy in CombatState!.HittableEnemies.ToArray())
        {
            await HammerStun.Apply(
                choiceContext,
                this,
                enemy,
                DynamicVars["Stun"].IntValue,
                cardPlay);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(4);
        DynamicVars["Stun"].UpgradeValueBy(1);
    }
}

[RegisterCard(typeof(HammerModCardPool))]
public sealed class EarthsplitterShock : HammerCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(24, MegaCrit.Sts2.Core.ValueProps.ValueProp.Move),
        new IntVar("Stun", 7)
    ];

    public EarthsplitterShock()
        : base(3, CardType.Attack, CardRarity.Rare, TargetType.AllEnemies)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var targets = CombatState!.HittableEnemies.ToArray();
        await AttackAll(choiceContext, DynamicVars.Damage.BaseValue);
        foreach (var enemy in targets.Where(static enemy => enemy.IsAlive))
        {
            await HammerStun.Apply(
                choiceContext,
                this,
                enemy,
                DynamicVars["Stun"].IntValue,
                cardPlay);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(6);
        DynamicVars["Stun"].UpgradeValueBy(3);
    }
}

[RegisterCard(typeof(HammerModCardPool))]
public sealed class FocusBlowEarthquake : HammerCard
{
    protected override bool ShouldGlowGoldInternal =>
        AnyHittableEnemy(static enemy =>
            HammerStun.GetCurrent(enemy) > 0 || enemy.IsStunned);

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(9, MegaCrit.Sts2.Core.ValueProps.ValueProp.Move),
        new IntVar("Stun", 2),
        new PowerVar<VulnerablePower>(1)
    ];

    public FocusBlowEarthquake()
        : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        var hadStun = HammerStun.GetCurrent(cardPlay.Target) > 0 || cardPlay.Target.IsStunned;

        await Attack(choiceContext, cardPlay.Target, DynamicVars.Damage.BaseValue);
        await HammerStun.Apply(
            choiceContext,
            this,
            cardPlay.Target,
            DynamicVars["Stun"].IntValue,
            cardPlay);

        if (hadStun && cardPlay.Target.IsAlive)
        {
            await PowerCmd.Apply<VulnerablePower>(
                choiceContext,
                cardPlay.Target,
                DynamicVars.Vulnerable.BaseValue,
                Owner.Creature,
                this);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2);
        DynamicVars["Stun"].UpgradeValueBy(1);
        DynamicVars.Vulnerable.UpgradeValueBy(1);
    }
}

[RegisterCard(typeof(HammerModCardPool))]
public sealed class HomeRunSwing : HammerCard
{
    protected override bool ShouldGlowGoldInternal =>
        AnyHittableEnemy(static enemy => enemy.IsStunned);

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.ComputedDamage(
            "Damage",
            static context =>
            {
                var stunned = context.Target?.IsStunned == true;
                return context.IsUpgraded
                    ? stunned ? 36 : 12
                    : stunned ? 27 : 9;
            },
            baseValue: 9),
        new IntVar("NormalDamage", 9),
        new IntVar("StunnedDamage", 27)
    ];

    public HomeRunSwing()
        : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        var damage = IsUpgraded
            ? cardPlay.Target.IsStunned ? 36 : 12
            : cardPlay.Target.IsStunned ? 27 : 9;
        await Attack(choiceContext, cardPlay.Target, damage);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["NormalDamage"].UpgradeValueBy(3);
        DynamicVars["StunnedDamage"].UpgradeValueBy(9);
    }
}

[RegisterCard(typeof(HammerModCardPool))]
public sealed class BigBangCombo : HammerCard, IContextualDescriptionCard
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Retain];

    protected override bool ShouldGlowGoldInternal =>
        AnyHittableEnemy(static enemy => enemy.IsStunned);

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(6, MegaCrit.Sts2.Core.ValueProps.ValueProp.Move),
        ModCardVars.Computed(
            "Hits",
            static context =>
            {
                var originalHitCount = context.Target?.IsStunned == true
                    ? context.GetCardIntOrDefault("StunnedHits", 6)
                    : context.GetCardIntOrDefault("BaseHits", 3);
                return PreviewAttackHitCount(context, originalHitCount);
            },
            baseValue: 3),
        new IntVar("BaseHits", 3),
        new IntVar("StunnedHits", 6)
    ];

    public BigBangCombo()
        : base(2, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        var hits = cardPlay.Target.IsStunned
            ? DynamicVars["StunnedHits"].IntValue
            : DynamicVars["BaseHits"].IntValue;
        await Attack(choiceContext, cardPlay.Target, DynamicVars.Damage.BaseValue, hits);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2);
    }
}

[RegisterCard(typeof(HammerModCardPool))]
public sealed class FlashHammer : HammerCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IntVar("Stun", 5)
    ];

    public FlashHammer()
        : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.AllEnemies)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        foreach (var enemy in CombatState!.HittableEnemies.ToArray())
        {
            await HammerStun.Apply(
                choiceContext,
                this,
                enemy,
                DynamicVars["Stun"].IntValue,
                cardPlay);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Stun"].UpgradeValueBy(2);
    }
}

[RegisterCard(typeof(HammerModCardPool))]
public sealed class DizzyFall : HammerCard
{
    protected override bool ShouldGlowGoldInternal =>
        AnyHittableEnemy(static enemy => HammerStun.GetCurrent(enemy) > 0);

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IntVar("Multiplier", 1)
    ];

    public DizzyFall()
        : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.AllEnemies)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        foreach (var enemy in CombatState!.HittableEnemies.ToArray())
        {
            await DealEffectDamage(
                choiceContext,
                enemy,
                HammerStun.GetCurrent(enemy) * DynamicVars["Multiplier"].IntValue);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Multiplier"].UpgradeValueBy(1);
    }
}

[RegisterCard(typeof(HammerModCardPool))]
public sealed class ConcussionGuard : HammerCard, IContextualDescriptionCard
{
    protected override bool ShouldGlowGoldInternal =>
        AnyHittableEnemy(static enemy => HammerStun.GetCurrent(enemy) > 0);

    public override bool GainsBlock => true;
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.ComputedBlock(
            "Block",
            static context => context.HasCombatState
                ? context.CombatState!.HittableEnemies.Sum(HammerStun.GetCurrent)
                : 0,
            baseValue: 0)
    ];

    public ConcussionGuard()
        : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var block = CombatState!.HittableEnemies.Sum(HammerStun.GetCurrent);
        await GainBlock(block, cardPlay);
    }

    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Exhaust);
    }
}

[RegisterCard(typeof(HammerModCardPool))]
public sealed class FelyneKoTechnique : HammerCard
{
    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/placeholders/StunTechnique.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IntVar("BonusStun", 0)
    ];

    public FelyneKoTechnique()
        : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<FelyneKoTechniquePower>(
            choiceContext,
            Owner.Creature,
            100 + DynamicVars["BonusStun"].IntValue,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["BonusStun"].UpgradeValueBy(1);
    }
}

[RegisterCard(typeof(HammerModCardPool))]
public sealed class PileDriver : HammerCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IntVar("StunBonusPercent", 50),
        new IntVar("KnockedOutBonusPercent", 100)
    ];

    public PileDriver()
        : base(2, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<PileDriverPower>(
            choiceContext,
            Owner.Creature,
            1,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}

[RegisterCard(typeof(HammerModCardPool))]
public sealed class HeadHunterSmash : HammerCard
{
    protected override bool ShouldGlowGoldInternal =>
        AnyHittableEnemy(static enemy =>
            HammerStun.GetCurrent(enemy) * 2 >= HammerStun.GetThreshold(enemy));

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.ComputedDamage(
            "Damage",
            static context =>
            {
                var primed = context.Target is { } target
                    && HammerStun.GetCurrent(target) * 2 >= HammerStun.GetThreshold(target);
                return context.IsUpgraded
                    ? primed ? 28 : 14
                    : primed ? 20 : 10;
            },
            baseValue: 10),
        new IntVar("NormalDamage", 10),
        new IntVar("PrimedDamage", 20)
    ];

    public HeadHunterSmash()
        : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        var primed = HammerStun.GetCurrent(cardPlay.Target) * 2
            >= HammerStun.GetThreshold(cardPlay.Target);
        var damage = IsUpgraded
            ? primed ? 28 : 14
            : primed ? 20 : 10;
        await Attack(choiceContext, cardPlay.Target, damage);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["NormalDamage"].UpgradeValueBy(4);
        DynamicVars["PrimedDamage"].UpgradeValueBy(8);
    }
}

[RegisterCard(typeof(HammerModCardPool))]
public sealed class Aftershock : HammerCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(12, MegaCrit.Sts2.Core.ValueProps.ValueProp.Move),
        new IntVar("Stun", 4)
    ];

    public Aftershock()
        : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        await Attack(choiceContext, cardPlay.Target, DynamicVars.Damage.BaseValue);
        if (cardPlay.Target.IsAlive)
        {
            await PowerCmd.Apply<AftershockPower>(
                choiceContext,
                cardPlay.Target,
                DynamicVars["Stun"].BaseValue,
                Owner.Creature,
                this);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(4);
        DynamicVars["Stun"].UpgradeValueBy(2);
    }
}

[RegisterCard(typeof(HammerModCardPool))]
public sealed class ConcussionResonance : HammerCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IntVar("Stun", 3)
    ];

    public ConcussionResonance()
        : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<ConcussionResonancePower>(
            choiceContext,
            Owner.Creature,
            DynamicVars["Stun"].BaseValue,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Stun"].UpgradeValueBy(1);
    }
}

[RegisterCard(typeof(HammerModCardPool))]
public sealed class ImpactBurst : HammerCard
{
    public ImpactBurst()
        : base(2, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<ImpactBurstPower>(
            choiceContext,
            Owner.Creature,
            1,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
