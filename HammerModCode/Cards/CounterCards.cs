using HammerMod.Characters;
using HammerMod.Gameplay;
using HammerMod.Powers;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;

namespace HammerMod.Cards;

[RegisterCard(typeof(HammerModCardPool))]
public sealed class FaceOff : HammerCard
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Ethereal];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<StrengthPower>(1)
    ];

    protected override bool ShouldGlowGoldInternal => IsPlayable;

    protected override bool IsPlayable =>
        AnyHittableEnemy(target => HammerTargetTypes.IsFaceOffTarget(Owner, target));

    public FaceOff()
        : base(1, CardType.Skill, CardRarity.Rare, HammerTargetTypes.FaceOff)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        if (!HammerTargetTypes.IsFaceOffTarget(Owner, cardPlay.Target))
            return;

        await PowerCmd.Apply<FaceOffPower>(
            choiceContext,
            Owner.Creature,
            1,
            cardPlay.Target,
            this);
        await PowerCmd.Apply<StrengthPower>(
            choiceContext,
            Owner.Creature,
            DynamicVars.Strength.BaseValue,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Ethereal);
    }
}

[RegisterCard(typeof(HammerModCardPool))]
public sealed class WaterStrike : HammerCard
{
    public override bool GainsBlock => true;

    protected override bool ShouldGlowGoldInternal => AnyHittableEnemyIntendsToAttack();

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(9, MegaCrit.Sts2.Core.ValueProps.ValueProp.Move),
        new BlockVar(6, MegaCrit.Sts2.Core.ValueProps.ValueProp.Move)
    ];

    public WaterStrike()
        : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        var gainBlock = IntendsToAttack(cardPlay.Target);
        await Attack(choiceContext, cardPlay.Target, DynamicVars.Damage.BaseValue);
        if (gainBlock)
            await GainBlock(DynamicVars.Block.BaseValue, cardPlay);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3);
        DynamicVars.Block.UpgradeValueBy(3);
    }
}

[RegisterCard(typeof(HammerModCardPool))]
public sealed class LeveragedSwing : HammerCard
{
    protected override bool ShouldGlowGoldInternal => AnyHittableEnemyIntendsToAttack();

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(9, MegaCrit.Sts2.Core.ValueProps.ValueProp.Move),
        new EnergyVar("Energy", 1)
    ];

    public LeveragedSwing()
        : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        var refundEnergy = IntendsToAttack(cardPlay.Target);
        await Attack(choiceContext, cardPlay.Target, DynamicVars.Damage.BaseValue);
        if (refundEnergy)
            await PlayerCmd.GainEnergy(DynamicVars["Energy"].IntValue, Owner);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3);
    }
}

[RegisterCard(typeof(HammerModCardPool))]
public sealed class PredictiveFootwork : HammerCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IntVar("Power", 1)
    ];

    public PredictiveFootwork()
        : base(0, CardType.Skill, CardRarity.Common, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        if (IntendsToAttack(cardPlay.Target))
        {
            await PowerCmd.Apply<WeakPower>(
                choiceContext,
                cardPlay.Target,
                DynamicVars["Power"].BaseValue,
                Owner.Creature,
                this);
        }
        else
        {
            await PowerCmd.Apply<VulnerablePower>(
                choiceContext,
                cardPlay.Target,
                DynamicVars["Power"].BaseValue,
                Owner.Creature,
                this);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Power"].UpgradeValueBy(1);
    }
}

[RegisterCard(typeof(HammerModCardPool))]
public sealed class LegSweepHammer : HammerCard
{
    protected override bool ShouldGlowGoldInternal => AnyHittableEnemyIntendsToAttack();

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(6, MegaCrit.Sts2.Core.ValueProps.ValueProp.Move),
        new PowerVar<WeakPower>(1)
    ];

    public LegSweepHammer()
        : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AllEnemies)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var attackingEnemies = CombatState!.HittableEnemies
            .Where(IntendsToAttack)
            .ToArray();

        await AttackAll(choiceContext, DynamicVars.Damage.BaseValue);
        foreach (var enemy in attackingEnemies.Where(static enemy => enemy.IsAlive))
        {
            await PowerCmd.Apply<WeakPower>(
                choiceContext,
                enemy,
                DynamicVars.Weak.BaseValue,
                Owner.Creature,
                this);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2);
        DynamicVars.Weak.UpgradeValueBy(1);
    }
}

[RegisterCard(typeof(HammerModCardPool))]
public sealed class DeepBreath : HammerCard
{
    protected override bool ShouldGlowGoldInternal => AnyHittableEnemyIntendsToAttack();

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new EnergyVar("Energy", 1),
        new EnergyVar("AttackEnergy", 2)
    ];

    public DeepBreath()
        : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        await PlayerCmd.GainEnergy(
            IntendsToAttack(cardPlay.Target)
                ? DynamicVars["AttackEnergy"].IntValue
                : DynamicVars["Energy"].IntValue,
            Owner);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["AttackEnergy"].UpgradeValueBy(1);
    }
}

[RegisterCard(typeof(HammerModCardPool))]
public sealed class UnloadingStance : HammerCard
{
    public override bool GainsBlock => true;

    protected override bool ShouldGlowGoldInternal => AnyHittableEnemyIntendsToAttack();

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(5, MegaCrit.Sts2.Core.ValueProps.ValueProp.Move),
        new IntVar("StrengthLoss", 3)
    ];

    public UnloadingStance()
        : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var attackingEnemies = CombatState!.HittableEnemies
            .Where(IntendsToAttack)
            .ToArray();
        await GainBlock(DynamicVars.Block.BaseValue, cardPlay);
        foreach (var enemy in attackingEnemies.Where(static enemy => enemy.IsAlive))
        {
            await PowerCmd.Apply<UnloadingStancePower>(
                choiceContext,
                enemy,
                DynamicVars["StrengthLoss"].BaseValue,
                Owner.Creature,
                this);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(2);
        DynamicVars["StrengthLoss"].UpgradeValueBy(2);
    }
}

[RegisterCard(typeof(HammerModCardPool))]
public sealed class WaterStance : HammerCard
{
    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(8, MegaCrit.Sts2.Core.ValueProps.ValueProp.Move),
        new IntVar("Thorns", 3)
    ];

    public WaterStance()
        : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await GainBlock(DynamicVars.Block.BaseValue, cardPlay);
        await PowerCmd.Apply<WaterStancePower>(
            choiceContext,
            Owner.Creature,
            DynamicVars["Thorns"].BaseValue,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(3);
        DynamicVars["Thorns"].UpgradeValueBy(1);
    }
}

[RegisterCard(typeof(HammerModCardPool))]
public sealed class ChargeStep : HammerCard
{
    public override bool GainsBlock => true;

    protected override bool ShouldGlowGoldInternal => AnyHittableEnemyIntendsToAttack();

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(8, MegaCrit.Sts2.Core.ValueProps.ValueProp.Move),
        new IntVar("Charge", 1)
    ];

    public ChargeStep()
        : base(1, CardType.Skill, CardRarity.Common, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        var gainCharge = IntendsToAttack(cardPlay.Target);
        await GainBlock(DynamicVars.Block.BaseValue, cardPlay);
        if (gainCharge)
            await GainCharge(1);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(3);
    }
}

[RegisterCard(typeof(HammerModCardPool))]
public sealed class OffsetUpswing : HammerCard
{
    protected override bool ShouldGlowGoldInternal => AnyHittableEnemyIntendsToAttack();

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(8, MegaCrit.Sts2.Core.ValueProps.ValueProp.Move)
    ];

    public OffsetUpswing()
        : base(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        var hits = GetAttackCount(cardPlay.Target);
        if (hits > 0)
        {
            await Attack(
                choiceContext,
                cardPlay.Target,
                DynamicVars.Damage.BaseValue,
                hits);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2);
    }
}

[RegisterCard(typeof(HammerModCardPool))]
public sealed class BreakMomentum : HammerCard
{
    protected override bool ShouldGlowGoldInternal =>
        AnyHittableEnemy(static enemy => enemy.GetPowerAmount<StrengthPower>() > 0);

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(6, MegaCrit.Sts2.Core.ValueProps.ValueProp.Move),
        new PowerVar<StrengthPower>(3)
    ];

    public BreakMomentum()
        : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        var reduceStrength = cardPlay.Target.GetPowerAmount<StrengthPower>() > 0;
        await Attack(choiceContext, cardPlay.Target, DynamicVars.Damage.BaseValue);
        if (reduceStrength && cardPlay.Target.IsAlive)
        {
            await PowerCmd.Apply<StrengthPower>(
                choiceContext,
                cardPlay.Target,
                -DynamicVars.Strength.BaseValue,
                Owner.Creature,
                this);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3);
        DynamicVars.Strength.UpgradeValueBy(2);
    }
}

[RegisterCard(typeof(HammerModCardPool))]
public sealed class CounterForm : HammerCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IntVar("Block", 3)
    ];

    public CounterForm()
        : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<CounterFormPower>(
            choiceContext,
            Owner.Creature,
            DynamicVars["Block"].BaseValue,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Block"].UpgradeValueBy(1);
    }
}
