using HammerMod.Characters;
using HammerMod.Gameplay;
using HammerMod.Powers;
using MegaCrit.Sts2.Core.CardSelection;
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
public sealed class Upswing : HammerCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(6, MegaCrit.Sts2.Core.ValueProps.ValueProp.Move),
        new IntVar("Stun", 3)
    ];

    public Upswing()
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
public sealed class MightyChargeUppercut : HammerCard, IChargeReleaseCard, ICombatPreviewDescriptionCard
{
    private static readonly int[] BaseStun = [4, 7, 11, 16];
    private static readonly int[] UpgradedStun = [5, 9, 14, 20];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(14, MegaCrit.Sts2.Core.ValueProps.ValueProp.Move),
        ModCardVars.Computed(
            "Stun",
            static context => ResolveStun(PreviewCharge(context, true), context.IsUpgraded),
            baseValue: BaseStun[0]),
        .. ChargeTierVars("StunAt", BaseStun)
    ];

    public MightyChargeUppercut()
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
        DynamicVars.Damage.UpgradeValueBy(4);
        UpgradeChargeTierVars("StunAt", BaseStun, UpgradedStun);
    }

    private static int ResolveStun(int charge, bool upgraded)
    {
        return (upgraded ? UpgradedStun : BaseStun)[Math.Clamp(charge, 0, 3)];
    }
}

[RegisterCard(typeof(HammerModCardPool))]
public sealed class MightyUpswing : HammerCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(14, MegaCrit.Sts2.Core.ValueProps.ValueProp.Move),
        new IntVar("Stun", 8)
    ];

    public MightyUpswing()
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
        DynamicVars.Damage.UpgradeValueBy(4);
        DynamicVars["Stun"].UpgradeValueBy(2);
    }
}

[RegisterCard(typeof(HammerModCardPool))]
public sealed class GroundShock : HammerCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(12, MegaCrit.Sts2.Core.ValueProps.ValueProp.Move),
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
        DynamicVars.Damage.UpgradeValueBy(6);
        DynamicVars["Stun"].UpgradeValueBy(1);
    }
}

[RegisterCard(typeof(HammerModCardPool))]
public sealed class Cataclysm : HammerCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(24, MegaCrit.Sts2.Core.ValueProps.ValueProp.Move),
        new IntVar("Stun", 10)
    ];

    public Cataclysm()
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
        DynamicVars.Damage.UpgradeValueBy(8);
        DynamicVars["Stun"].UpgradeValueBy(3);
    }
}

[RegisterCard(typeof(HammerModCardPool))]
public sealed class FocusBlowEarthquake : HammerCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(3, MegaCrit.Sts2.Core.ValueProps.ValueProp.Move),
        new IntVar("Stun", 1),
        new PowerVar<VulnerablePower>(1)
    ];

    public FocusBlowEarthquake()
        : base(0, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        await PowerCmd.Apply<VulnerablePower>(
            choiceContext,
            cardPlay.Target,
            DynamicVars.Vulnerable.BaseValue,
            Owner.Creature,
            this);

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
        DynamicVars.Damage.UpgradeValueBy(2);
        DynamicVars["Stun"].UpgradeValueBy(1);
        DynamicVars.Vulnerable.UpgradeValueBy(1);
    }
}

[RegisterCard(typeof(HammerModCardPool))]
public sealed class HomeRunSwing : HammerCard, ITargetPreviewDescriptionCard
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
public sealed class BigBangCombo : HammerCard, ITargetPreviewDescriptionCard
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
public sealed class HeadOverHeels : HammerCard
{
    protected override bool ShouldGlowGoldInternal =>
        AnyHittableEnemy(static enemy => HammerStun.GetCurrent(enemy) > 0);

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IntVar("Multiplier", 1)
    ];

    public HeadOverHeels()
        : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.AllEnemies)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        foreach (var enemy in CombatState!.HittableEnemies.ToArray())
        {
            await LoseHpDirectly(
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
public sealed class ConcussionGuard : HammerCard, ICombatPreviewDescriptionCard
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
        : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
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
public sealed class KoTechnique : HammerCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IntVar("BonusStun", 0)
    ];

    public KoTechnique()
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
        : base(1, CardType.Power, CardRarity.Rare, TargetType.Self)
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
public sealed class SmashThatHead : HammerCard
{
    protected override bool ShouldGlowGoldInternal =>
        AnyHittableEnemy(static enemy => HammerStun.GetCurrent(enemy) > 0);

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(9, MegaCrit.Sts2.Core.ValueProps.ValueProp.Move),
        new IntVar("StunMultiplier", 1)
    ];

    public SmashThatHead()
        : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        var damage = CalculateDamage(
            DynamicVars.Damage.BaseValue,
            HammerStun.GetCurrent(cardPlay.Target),
            DynamicVars["StunMultiplier"].IntValue);
        await Attack(choiceContext, cardPlay.Target, damage);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["StunMultiplier"].UpgradeValueBy(1);
    }

    internal static decimal CalculateDamage(
        decimal baseDamage,
        int currentStun,
        int stunMultiplier)
    {
        return Math.Max(0, baseDamage)
            + Math.Max(0, currentStun) * Math.Max(0, stunMultiplier);
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
        new IntVar("ChargeLoss", 2),
        new EnergyVar("Energy", 1)
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
            ResolvePowerAmount(IsUpgraded),
            Owner.Creature,
            this);
    }

    internal static int ResolvePowerAmount(bool upgraded)
    {
        return upgraded ? 101 : 100;
    }

    protected override void OnUpgrade()
    {
        DynamicVars["ChargeLoss"].UpgradeValueBy(-1);
    }
}

[RegisterCard(typeof(HammerModCardPool))]
public sealed class ImpactBurst : HammerCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IntVar("StunPerHit", 1)
    ];

    public ImpactBurst()
        : base(1, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<ImpactBurstPower>(
            choiceContext,
            Owner.Creature,
            DynamicVars["StunPerHit"].IntValue,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["StunPerHit"].UpgradeValueBy(1);
    }
}

[RegisterCard(typeof(HammerModCardPool))]
public sealed class Whetstone : HammerCard
{
    protected override bool IsPlayable =>
        CombatState is not null
        && Owner.PlayerCombatState?.Hand.Cards.Any(
            card => !ReferenceEquals(card, this)) == true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CardsVar("Cards", 2),
        new IntVar("Charge", 1)
    ];

    public Whetstone()
        : base(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var hand = Owner.PlayerCombatState!.Hand;
        if (!hand.Cards.Any(card => !ReferenceEquals(card, this)))
            return;

        var prefs = new CardSelectorPrefs(
            CardSelectorPrefs.ExhaustSelectionPrompt,
            1,
            1)
        {
            Cancelable = false
        };
        var selected = (await CardSelectCmd.FromHand(
            choiceContext,
            Owner,
            prefs,
            card => !ReferenceEquals(card, this),
            this)).FirstOrDefault();
        if (selected is null)
            return;

        await CardCmd.Exhaust(
            choiceContext,
            selected,
            causedByEthereal: false,
            skipVisuals: false);
        await CardPileCmd.Draw(choiceContext, DynamicVars["Cards"].IntValue, Owner);
        await GainCharge(DynamicVars["Charge"].IntValue);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}

[RegisterCard(typeof(HammerModCardPool))]
public sealed class StaminaDrainingHammer : HammerCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(4, MegaCrit.Sts2.Core.ValueProps.ValueProp.Move),
        new IntVar("Stun", 7),
        new PowerVar<WeakPower>(1),
        new PowerVar<VulnerablePower>(1)
    ];

    public StaminaDrainingHammer()
        : base(1, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        await Attack(choiceContext, cardPlay.Target, DynamicVars.Damage.BaseValue);
        if (!cardPlay.Target.IsAlive)
            return;

        await HammerStun.Apply(
            choiceContext,
            this,
            cardPlay.Target,
            DynamicVars["Stun"].IntValue,
            cardPlay);
        await PowerCmd.Apply<WeakPower>(
            choiceContext,
            cardPlay.Target,
            DynamicVars.Weak.BaseValue,
            Owner.Creature,
            this);
        await PowerCmd.Apply<VulnerablePower>(
            choiceContext,
            cardPlay.Target,
            DynamicVars.Vulnerable.BaseValue,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2);
        DynamicVars["Stun"].UpgradeValueBy(3);
        DynamicVars.Weak.UpgradeValueBy(1);
        DynamicVars.Vulnerable.UpgradeValueBy(1);
    }
}
