using HammerMod.Characters;
using HammerMod.Gameplay;
using HammerMod.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Models.Capabilities;

namespace HammerMod.Cards;

[RegisterCard(typeof(HammerModCardPool))]
public sealed class ChargedOverheadSmash : HammerCard
{
    private const int BaseDamage = 9;
    private const int BaseChargedDamage = 15;
    private const int UpgradedDamage = 12;
    private const int UpgradedChargedDamage = 20;

    protected override bool ShouldGlowGoldInternal => HasChargeAtLeast(2);

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.ComputedDamage(
            "Damage",
            static context => ResolveDamage(PreviewCharge(context), context.IsUpgraded),
            baseValue: BaseDamage),
        new IntVar("NormalDamage", BaseDamage),
        new IntVar("ChargedDamage", BaseChargedDamage)
    ];

    public ChargedOverheadSmash()
        : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        await Attack(choiceContext, cardPlay.Target, ResolveDamage(ChargeLevel, IsUpgraded));
    }

    protected override void OnUpgrade()
    {
        DynamicVars["NormalDamage"].UpgradeValueBy(3);
        DynamicVars["ChargedDamage"].UpgradeValueBy(5);
    }

    internal static int ResolveDamage(int charge, bool upgraded)
    {
        if (charge >= 2)
            return upgraded ? UpgradedChargedDamage : BaseChargedDamage;

        return upgraded ? UpgradedDamage : BaseDamage;
    }
}

[RegisterCard(typeof(HammerModCardPool))]
public sealed class ChargedSideSmash : HammerCard
{
    private const int BaseDamage = 6;
    private const int BaseChargedDamage = 10;
    private const int UpgradedDamage = 8;
    private const int UpgradedChargedDamage = 13;

    protected override bool ShouldGlowGoldInternal => HasChargeAtLeast(2);

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.ComputedDamage(
            "Damage",
            static context => ResolveDamage(PreviewCharge(context), context.IsUpgraded),
            baseValue: BaseDamage),
        new IntVar("NormalDamage", BaseDamage),
        new IntVar("ChargedDamage", BaseChargedDamage)
    ];

    public ChargedSideSmash()
        : base(1, CardType.Attack, CardRarity.Common, TargetType.AllEnemies)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await AttackAll(choiceContext, ResolveDamage(ChargeLevel, IsUpgraded));
    }

    protected override void OnUpgrade()
    {
        DynamicVars["NormalDamage"].UpgradeValueBy(2);
        DynamicVars["ChargedDamage"].UpgradeValueBy(3);
    }

    internal static int ResolveDamage(int charge, bool upgraded)
    {
        if (charge >= 2)
            return upgraded ? UpgradedChargedDamage : BaseChargedDamage;

        return upgraded ? UpgradedDamage : BaseDamage;
    }
}

[RegisterCard(typeof(HammerModCardPool))]
public sealed class MightyChargeBonk : HammerCard, IChargeReleaseCard, ICombatPreviewDescriptionCard
{
    private static readonly int[] BaseDamage = [16, 22, 30, 40];
    private static readonly int[] UpgradedDamage = [20, 28, 38, 50];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.ComputedDamage(
            "Damage",
            static context => Resolve(PreviewCharge(context, true), context.IsUpgraded),
            baseValue: BaseDamage[0]),
        .. ChargeTierVars("DamageAt", BaseDamage)
    ];

    public MightyChargeBonk()
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

    protected override void OnUpgrade()
    {
        UpgradeChargeTierVars("DamageAt", BaseDamage, UpgradedDamage);
    }

    private static int Resolve(int charge, bool upgraded)
    {
        return (upgraded ? UpgradedDamage : BaseDamage)[Math.Clamp(charge, 0, 3)];
    }
}

[RegisterCard(typeof(HammerModCardPool))]
public sealed class MightyChargeSpin : HammerCard, IChargeReleaseCard, ICombatPreviewDescriptionCard
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

    public MightyChargeSpin()
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
public sealed class MightyChargeRoll : HammerCard, IChargeReleaseCard, ICombatPreviewDescriptionCard
{
    private static readonly int[] BaseBlock = [8, 14, 20, 28];
    private static readonly int[] UpgradedBlock = [12, 18, 26, 32];

    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.ComputedBlock(
            "Block",
            static context => Resolve(PreviewCharge(context, true), context.IsUpgraded),
            baseValue: BaseBlock[0]),
        .. ChargeTierVars("BlockAt", BaseBlock)
    ];

    public MightyChargeRoll()
        : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var charge = BeginChargeRelease(cardPlay);
        await GainBlock(Resolve(charge, IsUpgraded), cardPlay);
        await ReleaseCharge(choiceContext, charge, cardPlay);
    }

    protected override void OnUpgrade()
    {
        UpgradeChargeTierVars("BlockAt", BaseBlock, UpgradedBlock);
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
public sealed class ChargeStep : HammerCard
{
    protected override bool ShouldGlowGoldInternal => HasChargeAtLeast(3);

    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IntVar("Charge", 1),
        new BlockVar("Block", 5, MegaCrit.Sts2.Core.ValueProps.ValueProp.Move),
        new BlockVar("FullBlock", 15, MegaCrit.Sts2.Core.ValueProps.ValueProp.Move)
    ];

    public ChargeStep()
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
        DynamicVars["FullBlock"].UpgradeValueBy(3);
    }
}

[RegisterCard(typeof(HammerModCardPool))]
public sealed class SteadierWithEverySpin : HammerCard, ICombatPreviewDescriptionCard
{
    protected override bool ShouldGlowGoldInternal => HasChargeAtLeast(3);

    public override bool GainsBlock => true;
    protected override bool HasEnergyCostX => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IntVar("BlockPerEnergy", 7),
        new IntVar("ExcessBlock", 3),
        ModCardVars.ComputedBlock(
            "ResolvedBlockPerEnergy",
            static context => context.GetCardIntOrDefault("BlockPerEnergy", 7),
            baseValue: 7),
        ModCardVars.ComputedBlock(
            "ResolvedExcessBlock",
            static context => context.GetCardIntOrDefault("ExcessBlock", 3),
            baseValue: 3),
        ModCardVars.Computed(
            "ResolvedBlock",
            static context => ResolveSteadierWithEverySpinPreviewBlock(context),
            baseValue: 0),
        ModCardVars.Computed(
            "ResolvedCharge",
            static context => ResolveSteadierWithEverySpin(context).Charge,
            baseValue: 0)
    ];

    public SteadierWithEverySpin()
        : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var (energy, chargeGain, excess) = ResolveSteadierWithEverySpinCounts(
            ResolveEnergyXValue(),
            ChargeLevel,
            HammerResources.MaxCharge);

        for (var index = 0; index < energy; index++)
            await GainBlock(DynamicVars["BlockPerEnergy"].IntValue, cardPlay);
        for (var index = 0; index < excess; index++)
            await GainBlock(DynamicVars["ExcessBlock"].IntValue, cardPlay);
        await GainCharge(chargeGain);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["BlockPerEnergy"].UpgradeValueBy(2);
        DynamicVars["ExcessBlock"].UpgradeValueBy(1);
    }

    private static (int Block, int Charge) ResolveSteadierWithEverySpin(
        ComputedDynamicVarContext context)
    {
        return ResolveSteadierWithEverySpin(
            PreviewEnergyXValue(context),
            PreviewCharge(context),
            context.GetCardIntOrDefault("BlockPerEnergy", 7),
            context.GetCardIntOrDefault("ExcessBlock", 3));
    }

    private static decimal ResolveSteadierWithEverySpinPreviewBlock(
        ComputedDynamicVarContext context)
    {
        var (energy, _, excess) = ResolveSteadierWithEverySpinCounts(
            PreviewEnergyXValue(context),
            PreviewCharge(context),
            HammerResources.MaxCharge);
        var blockPerEnergy = context.EvaluateCardVarOrDefault(
            "ResolvedBlockPerEnergy",
            context.GetCardIntOrDefault("BlockPerEnergy", 7));
        var excessBlock = context.EvaluateCardVarOrDefault(
            "ResolvedExcessBlock",
            context.GetCardIntOrDefault("ExcessBlock", 3));
        return energy * blockPerEnergy + excess * excessBlock;
    }

    internal static (int Block, int Charge) ResolveSteadierWithEverySpin(
        int x,
        int currentCharge,
        int blockPerEnergy,
        int excessBlock)
    {
        var (energy, chargeGain, excess) = ResolveSteadierWithEverySpinCounts(
            x,
            currentCharge,
            HammerResources.MaxCharge);
        return (
            energy * Math.Max(0, blockPerEnergy) + excess * Math.Max(0, excessBlock),
            chargeGain);
    }

    internal static (int Energy, int Charge, int Excess) ResolveSteadierWithEverySpinCounts(
        int x,
        int currentCharge,
        int maxCharge)
    {
        var energy = Math.Max(0, x);
        var room = Math.Max(0, maxCharge - currentCharge);
        var charge = Math.Min(energy, room);
        return (energy, charge, Math.Max(0, energy - charge));
    }
}

[RegisterCard(typeof(HammerModCardPool))]
public sealed class HammerIai : HammerCard
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override bool ShouldGlowGoldInternal => IsPlayable;

    protected override bool IsPlayable =>
        CombatState is not null && HasRequiredCharge(ChargeLevel);

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new EnergyVar("EnergyPerCharge", 2)
    ];

    public HammerIai()
        : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    internal static bool HasRequiredCharge(int charge)
    {
        return charge >= 1;
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var charge = HammerResources.GetCharge(Owner);
        await SecondaryResourceCmd.Reset(
            Owner,
            HammerResources.Charge.Id,
            source: this);
        await PlayerCmd.GainEnergy(
            charge * DynamicVars["EnergyPerCharge"].IntValue,
            Owner);
    }

    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Exhaust);
    }
}

[RegisterCard(typeof(HammerModCardPool))]
public sealed class PressTheAdvantage : HammerCard
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override bool ShouldGlowGoldInternal => IsPlayable;

    protected override bool IsPlayable =>
        AnyHittableEnemy(static enemy => enemy.IsStunned);

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new EnergyVar("Energy", 2),
        new CardsVar("Cards", 2)
    ];

    public PressTheAdvantage()
        : base(0, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PlayerCmd.GainEnergy(DynamicVars["Energy"].IntValue, Owner);
        await CardPileCmd.Draw(
            choiceContext,
            DynamicVars["Cards"].IntValue,
            Owner);
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

    public Overcharge()
        : base(2, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<OverchargePower>(
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
public sealed class ChargeAsYouStrike : HammerCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(12, MegaCrit.Sts2.Core.ValueProps.ValueProp.Move),
        new IntVar("Charge", 2)
    ];

    public ChargeAsYouStrike()
        : base(1, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        await Attack(choiceContext, cardPlay.Target, DynamicVars.Damage.BaseValue);
        await GainCharge(IsUpgraded
            ? HammerResources.MaxCharge
            : DynamicVars["Charge"].IntValue);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(4);
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
        DynamicVars["FullCards"].UpgradeValueBy(1);
    }
}

[RegisterCard(typeof(HammerModCardPool))]
public sealed class EndlessMomentum : HammerCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new EnergyVar("Energy", 1),
        new CardsVar("Cards", 2)
    ];

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
public sealed class BraceWithTheHammer : HammerCard
{
    private const int BaseBlock = 8;
    private const int BaseChargedBlock = 13;
    private const int UpgradedBlock = 11;
    private const int UpgradedChargedBlock = 17;

    protected override bool ShouldGlowGoldInternal => HasChargeAtLeast(2);

    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.ComputedBlock(
            "Block",
            static context => ResolveBlock(PreviewCharge(context), context.IsUpgraded),
            baseValue: BaseBlock),
        new IntVar("NormalBlock", BaseBlock),
        new IntVar("ChargedBlock", BaseChargedBlock)
    ];

    public BraceWithTheHammer()
        : base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await GainBlock(ResolveBlock(ChargeLevel, IsUpgraded), cardPlay);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["NormalBlock"].UpgradeValueBy(3);
        DynamicVars["ChargedBlock"].UpgradeValueBy(4);
    }

    internal static int ResolveBlock(int charge, bool upgraded)
    {
        if (charge >= 2)
            return upgraded ? UpgradedChargedBlock : BaseChargedBlock;

        return upgraded ? UpgradedBlock : BaseBlock;
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
        await SecondaryResourceCmd.Reset(
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
public sealed class SwingAtEveryOpening : HammerCard
{
    protected override bool ShouldGlowGoldInternal =>
        AnyHittableEnemy(static enemy => !IntendsToAttack(enemy));

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(9, MegaCrit.Sts2.Core.ValueProps.ValueProp.Move),
        new IntVar("Charge", 2)
    ];

    public SwingAtEveryOpening()
        : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        var gainCharge = !IntendsToAttack(cardPlay.Target);
        await Attack(choiceContext, cardPlay.Target, DynamicVars.Damage.BaseValue);
        if (gainCharge)
            await GainCharge(DynamicVars["Charge"].IntValue);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3);
    }
}

[RegisterCard(typeof(HammerModCardPool))]
public sealed class MasterfulPositioning : HammerCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(3, MegaCrit.Sts2.Core.ValueProps.ValueProp.Unpowered)
    ];

    public MasterfulPositioning()
        : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<DashJuicePower>(
            choiceContext,
            Owner.Creature,
            DynamicVars.Block.BaseValue,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(1);
    }
}

[RegisterCard(typeof(HammerModCardPool))]
public sealed class HandCrankedTractor : HammerCard
{
    internal static StaticHoverTip ReplayHoverTip => StaticHoverTip.ReplayStatic;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IntVar("Replay", 1)
    ];

    public HandCrankedTractor()
        : base(2, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var candidates = Owner.PlayerCombatState!.DrawPile.Cards
            .Where(static card => card is IChargeReleaseCard)
            .ToArray();
        if (candidates.Length == 0)
            return Task.CompletedTask;

        var selected = Owner.RunState.Rng.CombatCardSelection.NextItem(candidates);
        if (selected is null)
            return Task.CompletedTask;

        selected.BaseReplayCount += DynamicVars["Replay"].IntValue;
        CardCmd.Preview(selected, 1.2f, CardPreviewStyle.HorizontalLayout);
        return Task.CompletedTask;
    }

    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Exhaust);
    }
}

[RegisterCard(typeof(HammerModCardPool))]
public sealed class ChargeSwitchStrength : HammerCard, ICombatPreviewDescriptionCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.ComputedPower<MegaCrit.Sts2.Core.Models.Powers.StrengthPower>(
            "StrengthPower",
            static context => ChargeSwitchStrengthPower.CalculateStrength(
                PreviewCharge(context),
                stacks: 1),
            baseValue: 1)
    ];

    public ChargeSwitchStrength()
        : base(1, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        var power = await PowerCmd.Apply<ChargeSwitchStrengthPower>(
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
        AddKeyword(CardKeyword.Innate);
    }
}

[RegisterCard(typeof(HammerModCardPool))]
public sealed class ChargeSwitchCourage : HammerCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IntVar("Charge", 1)
    ];

    public ChargeSwitchCourage()
        : base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        await PowerCmd.Apply<ChargeSwitchCouragePower>(
            choiceContext,
            Owner.Creature,
            DynamicVars["Charge"].IntValue,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
