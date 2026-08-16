using HammerMod.Characters;
using HammerMod.Gameplay;
using HammerMod.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Models.Capabilities;

namespace HammerMod.Cards;

[RegisterCard(typeof(HammerModCardPool))]
public sealed class ChargedOverheadSmash : HammerCard, IChargeContextDescriptionCard
{
    protected override bool ShouldGlowGoldInternal => HasChargeAtLeast(2);

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.ComputedDamage(
            "Damage",
            static context =>
            {
                var charged = PreviewCharge(context) >= 2;
                return context.IsUpgraded
                    ? charged ? 18 : 12
                    : charged ? 15 : 9;
            },
            baseValue: 9),
        new IntVar("NormalDamage", 9),
        new IntVar("ChargedDamage", 15)
    ];

    public ChargedOverheadSmash()
        : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        var damage = IsUpgraded
            ? ChargeLevel >= 2 ? 18 : 12
            : ChargeLevel >= 2 ? 15 : 9;
        await Attack(choiceContext, cardPlay.Target, damage);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["NormalDamage"].UpgradeValueBy(3);
        DynamicVars["ChargedDamage"].UpgradeValueBy(3);
    }
}

[RegisterCard(typeof(HammerModCardPool))]
public sealed class ChargedSideSmash : HammerCard, IChargeContextDescriptionCard
{
    protected override bool ShouldGlowGoldInternal => HasChargeAtLeast(2);

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.ComputedDamage(
            "Damage",
            static context =>
            {
                var charged = PreviewCharge(context) >= 2;
                return context.IsUpgraded
                    ? charged ? 15 : 9
                    : charged ? 11 : 6;
            },
            baseValue: 6),
        new IntVar("NormalDamage", 6),
        new IntVar("ChargedDamage", 11)
    ];

    public ChargedSideSmash()
        : base(1, CardType.Attack, CardRarity.Common, TargetType.AllEnemies)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var damage = IsUpgraded
            ? ChargeLevel >= 2 ? 15 : 9
            : ChargeLevel >= 2 ? 11 : 6;
        await AttackAll(choiceContext, damage);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["NormalDamage"].UpgradeValueBy(3);
        DynamicVars["ChargedDamage"].UpgradeValueBy(4);
    }
}

[RegisterCard(typeof(HammerModCardPool))]
public sealed class MightyChargeSlam : HammerCard, IChargeReleaseCard, IChargeContextDescriptionCard
{
    private static readonly int[] BaseDamage = [14, 21, 31, 44];
    private static readonly int[] UpgradedDamage = [18, 27, 39, 55];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.ComputedDamage(
            "Damage",
            static context => Resolve(PreviewCharge(context, true), context.IsUpgraded),
            baseValue: BaseDamage[0]),
        .. ChargeTierVars("DamageAt", BaseDamage, UpgradedDamage)
    ];

    public MightyChargeSlam()
        : base(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        var charge = BeginChargeRelease(cardPlay);
        await Attack(choiceContext, cardPlay.Target, Resolve(charge, IsUpgraded));
        await ReleaseCharge(choiceContext, charge, cardPlay);
    }

    private static int Resolve(int charge, bool upgraded)
    {
        return (upgraded ? UpgradedDamage : BaseDamage)[Math.Clamp(charge, 0, 3)];
    }
}

[RegisterCard(typeof(HammerModCardPool))]
public sealed class SilkbindSpinningBludgeon : HammerCard, IChargeReleaseCard, IChargeContextDescriptionCard
{
    protected override bool ShouldGlowGoldInternal => HasChargeAtLeast(1);

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(8, MegaCrit.Sts2.Core.ValueProps.ValueProp.Move),
        ModCardVars.Computed(
            "Hits",
            static context =>
            {
                var originalHitCount = PreviewCharge(context, true) + 1;
                return PreviewAttackHitCount(context, originalHitCount);
            },
            baseValue: 1),
        new IntVar("HitsAt0", 1),
        new IntVar("HitsAt1", 2),
        new IntVar("HitsAt2", 3),
        new IntVar("HitsAt3", 4)
    ];

    public SilkbindSpinningBludgeon()
        : base(2, CardType.Attack, CardRarity.Uncommon, TargetType.AllEnemies)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var charge = BeginChargeRelease(cardPlay);
        await AttackAll(choiceContext, DynamicVars.Damage.BaseValue, charge + 1);
        await ReleaseCharge(choiceContext, charge, cardPlay);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2);
    }
}

[RegisterCard(typeof(HammerModCardPool))]
public sealed class ChargedGuard : HammerCard, IChargeReleaseCard, IChargeContextDescriptionCard
{
    private static readonly int[] BaseBlock = [7, 14, 21, 28];
    private static readonly int[] UpgradedBlock = [8, 16, 25, 32];

    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.ComputedBlock(
            "Block",
            static context => Resolve(PreviewCharge(context, true), context.IsUpgraded),
            baseValue: BaseBlock[0]),
        .. ChargeTierVars("BlockAt", BaseBlock, UpgradedBlock)
    ];

    public ChargedGuard()
        : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var charge = BeginChargeRelease(cardPlay);
        await GainBlock(Resolve(charge, IsUpgraded), cardPlay);
        await ReleaseCharge(choiceContext, charge, cardPlay);
    }

    private static int Resolve(int charge, bool upgraded)
    {
        return (upgraded ? UpgradedBlock : BaseBlock)[Math.Clamp(charge, 0, 3)];
    }
}

[RegisterCard(typeof(HammerModCardPool))]
public sealed class ReadyToCharge : HammerCard
{
    protected override bool ShouldGlowGoldInternal => HasChargeAtLeast(3);

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IntVar("Charge", 2),
        new CardsVar("Cards", 1),
        new CardsVar("FullCards", 3)
    ];

    public ReadyToCharge()
        : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var full = ChargeLevel >= 3;
        if (!full)
            await GainCharge(DynamicVars["Charge"].IntValue);

        await CardPileCmd.Draw(
            choiceContext,
            full ? DynamicVars["FullCards"].IntValue : DynamicVars["Cards"].IntValue,
            Owner);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Cards"].UpgradeValueBy(1);
        DynamicVars["FullCards"].UpgradeValueBy(1);
    }
}

[RegisterCard(typeof(HammerModCardPool))]
public sealed class KeepingSway : HammerCard
{
    protected override bool ShouldGlowGoldInternal => HasChargeAtLeast(3);

    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IntVar("Charge", 1),
        new BlockVar("Block", 5, MegaCrit.Sts2.Core.ValueProps.ValueProp.Move),
        new BlockVar("FullBlock", 11, MegaCrit.Sts2.Core.ValueProps.ValueProp.Move)
    ];

    public KeepingSway()
        : base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var full = ChargeLevel >= 3;
        if (!full)
            await GainCharge(1);

        var block = full
            ? DynamicVars["FullBlock"].BaseValue
            : DynamicVars["Block"].BaseValue;
        await GainBlock(block, cardPlay);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Block"].UpgradeValueBy(3);
        DynamicVars["FullBlock"].UpgradeValueBy(4);
    }
}

[RegisterCard(typeof(HammerModCardPool))]
public sealed class SpinningCharge : HammerCard
{
    protected override bool ShouldGlowGoldInternal => HasChargeAtLeast(3);

    public override bool GainsBlock => true;
    protected override bool HasEnergyCostX => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IntVar("BlockPerEnergy", 7),
        new IntVar("ExcessBlock", 3)
    ];

    public SpinningCharge()
        : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var x = ResolveEnergyXValue();
        var room = Math.Max(0, 3 - ChargeLevel);
        var chargeGain = Math.Min(x, room);
        var excess = Math.Max(0, x - chargeGain);
        var block = x * DynamicVars["BlockPerEnergy"].IntValue
            + excess * DynamicVars["ExcessBlock"].IntValue;

        await GainBlock(block, cardPlay);
        await GainCharge(chargeGain);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["BlockPerEnergy"].UpgradeValueBy(2);
    }
}

[RegisterCard(typeof(HammerModCardPool))]
public sealed class SheatheAndBreathe : HammerCard
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override bool ShouldGlowGoldInternal => IsPlayable;

    protected override bool IsPlayable => HasChargeAtLeast(1);

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IntVar("Charge", 1),
        new EnergyVar("Energy", 1)
    ];

    public SheatheAndBreathe()
        : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await SecondaryResourceCmd.Lose(
            Owner,
            HammerResources.Charge.Id,
            DynamicVars["Charge"].IntValue,
            source: this);
        await PlayerCmd.GainEnergy(DynamicVars["Energy"].IntValue, Owner);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Energy"].UpgradeValueBy(1);
    }
}

[RegisterCard(typeof(HammerModCardPool))]
public sealed class VictoryCharge : HammerCard
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override bool ShouldGlowGoldInternal => IsPlayable;

    protected override bool IsPlayable =>
        AnyHittableEnemy(static enemy => enemy.IsStunned);

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new EnergyVar("Energy", 3)
    ];

    public VictoryCharge()
        : base(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PlayerCmd.GainEnergy(DynamicVars["Energy"].IntValue, Owner);
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Retain);
    }
}

[RegisterCard(typeof(HammerModCardPool))]
public sealed class Overcharge : HammerCard
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IntVar("Backlash", 1)
    ];

    public Overcharge()
        : base(2, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    public override IEnumerable<CardDescriptionFragment> GetDescriptionFragments(
        CardDescriptionContext context)
    {
        if (context.Card.IsUpgraded || context.IsUpgradePreview)
            return [];

        return
        [
            new CardDescriptionFragment(
                new LocString("cards", Id.Entry + ".backlashDescription"),
                CardDescriptionFragmentPlacement.AfterBase,
                0)
        ];
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<OverchargePower>(
            choiceContext,
            Owner.Creature,
            1,
            Owner.Creature,
            this);

        if (DynamicVars["Backlash"].IntValue > 0)
        {
            await PowerCmd.Apply<OverchargeBacklashPower>(
                choiceContext,
                Owner.Creature,
                DynamicVars["Backlash"].IntValue,
                Owner.Creature,
                this,
                silent: true);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Backlash"].UpgradeValueBy(-1);
    }
}

[RegisterCard(typeof(HammerModCardPool))]
public sealed class Focus : HammerCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CardsVar("FullCards", 1)
    ];

    public Focus()
        : base(1, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<FocusPower>(
            choiceContext,
            Owner.Creature,
            IsUpgraded ? 101 : 100,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["FullCards"].UpgradeValueBy(1);
        AddKeyword(CardKeyword.Innate);
    }
}

[RegisterCard(typeof(HammerModCardPool))]
public sealed class EndlessMomentum : HammerCard
{
    public EndlessMomentum()
        : base(1, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<EndlessMomentumPower>(
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
public sealed class ChargedStand : HammerCard, IChargeContextDescriptionCard
{
    protected override bool ShouldGlowGoldInternal => HasChargeAtLeast(2);

    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.ComputedBlock(
            "Block",
            static context =>
            {
                var charged = PreviewCharge(context) >= 2;
                return context.IsUpgraded
                    ? charged ? 19 : 11
                    : charged ? 16 : 8;
            },
            baseValue: 8),
        new IntVar("NormalBlock", 8),
        new IntVar("ChargedBlock", 16)
    ];

    public ChargedStand()
        : base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var block = IsUpgraded
            ? ChargeLevel >= 2 ? 19 : 11
            : ChargeLevel >= 2 ? 16 : 8;
        await GainBlock(block, cardPlay);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["NormalBlock"].UpgradeValueBy(3);
        DynamicVars["ChargedBlock"].UpgradeValueBy(3);
    }
}

[RegisterCard(typeof(HammerModCardPool))]
public sealed class EmergencyEvade : HammerCard
{
    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(13, MegaCrit.Sts2.Core.ValueProps.ValueProp.Move)
    ];

    public EmergencyEvade()
        : base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await STS2RitsuLib.Combat.SecondaryResources.SecondaryResourceCmd.Reset(
            Owner,
            HammerResources.Charge.Id,
            source: this);
        await GainBlock(DynamicVars.Block.BaseValue, cardPlay);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(4);
    }
}

[RegisterCard(typeof(HammerModCardPool))]
public sealed class StepSweep : HammerCard
{
    protected override bool ShouldGlowGoldInternal =>
        AnyHittableEnemy(static enemy => !IntendsToAttack(enemy));

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(10, MegaCrit.Sts2.Core.ValueProps.ValueProp.Move),
        new IntVar("Charge", 1)
    ];

    public StepSweep()
        : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        var gainCharge = !IntendsToAttack(cardPlay.Target);
        await Attack(choiceContext, cardPlay.Target, DynamicVars.Damage.BaseValue);
        if (gainCharge)
            await GainCharge(1);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3);
    }
}

[RegisterCard(typeof(HammerModCardPool))]
public sealed class DashJuice : HammerCard
{
    public DashJuice()
        : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<DashJuicePower>(
            choiceContext,
            Owner.Creature,
            1,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Innate);
    }
}

[RegisterCard(typeof(HammerModCardPool))]
public sealed class HandCrankedTractor : HammerCard
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    public HandCrankedTractor()
        : base(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<HandCrankedTractorPower>(
            choiceContext,
            Owner.Creature,
            1,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Exhaust);
    }
}

[RegisterCard(typeof(HammerModCardPool))]
public sealed class MarathonHammerer : HammerCard, IChargeContextDescriptionCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.ComputedPower<MegaCrit.Sts2.Core.Models.Powers.StrengthPower>(
            "StrengthPower",
            static context => PreviewCharge(context),
            baseValue: 0)
    ];

    public MarathonHammerer()
        : base(2, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        var power = await PowerCmd.Apply<MarathonHammererPower>(
            choiceContext,
            Owner.Creature,
            1,
            Owner.Creature,
            this);
        if (power is not null)
            await power.SyncStrength(choiceContext, this);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
