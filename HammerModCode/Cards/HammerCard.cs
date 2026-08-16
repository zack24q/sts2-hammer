using HammerMod.Gameplay;
using HammerMod.Powers;
using HammerMod.Relics;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Models.Capabilities;
using STS2RitsuLib.Scaffolding.Content;

namespace HammerMod.Cards;

public interface IChargeReleaseCard
{
}

public interface IContextualDescriptionCard
{
}

public interface IChargeContextDescriptionCard : IContextualDescriptionCard
{
}

public abstract class HammerCard : ModCardTemplate, ICardDescriptionContributor
{
    private bool _snapshotChargeOnNextRelease;
    private int? _releaseChargeSnapshot;

    protected static IEnumerable<DynamicVar> ChargeTierVars(
        string prefix,
        IReadOnlyList<int> baseValues,
        IReadOnlyList<int> upgradedValues)
    {
        for (var charge = 0; charge <= HammerResources.MaxCharge; charge++)
        {
            var tier = charge;
            yield return ModCardVars.Computed(
                $"{prefix}{tier}",
                context => context.IsUpgraded ? upgradedValues[tier] : baseValues[tier],
                baseValue: baseValues[tier]);
        }
    }

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/placeholders/{GetType().Name}.png");

    protected HammerCard(int baseCost, CardType type, CardRarity rarity, TargetType target)
        : base(baseCost, type, rarity, target)
    {
    }

    protected int ChargeLevel =>
        this is IChargeReleaseCard
        && Owner.Creature.GetPower<OverchargePower>() is not null
            ? HammerResources.MaxCharge
            : HammerResources.GetCharge(Owner);

    internal void SnapshotChargeOnNextRelease()
    {
        _snapshotChargeOnNextRelease = true;
    }

    protected int BeginChargeRelease(CardPlay cardPlay)
    {
        if (_snapshotChargeOnNextRelease && cardPlay.IsFirstInSeries)
        {
            _releaseChargeSnapshot = ChargeLevel;
            _snapshotChargeOnNextRelease = false;
        }

        return _releaseChargeSnapshot ?? ChargeLevel;
    }

    protected bool HasChargeAtLeast(int amount)
    {
        return CombatState is not null
            && (this is IChargeReleaseCard
                && Owner.Creature.GetPower<OverchargePower>() is not null
                || HammerResources.GetCharge(Owner) >= amount);
    }

    protected bool AnyHittableEnemy(Func<Creature, bool> predicate)
    {
        return CombatState?.HittableEnemies.Any(predicate) ?? false;
    }

    protected bool AnyHittableEnemyIntendsToAttack()
    {
        return AnyHittableEnemy(IntendsToAttack);
    }

    protected static int PreviewCharge(
        ComputedDynamicVarContext context,
        bool isChargeReleaseCard = false)
    {
        if (!context.HasPlayer || context.Card?.Pile?.Type != PileType.Hand)
            return 0;

        return isChargeReleaseCard
            && context.Player.Creature.GetPower<OverchargePower>() is not null
                ? 3
                : HammerResources.GetCharge(context.Player);
    }

    public virtual IEnumerable<CardDescriptionFragment> GetDescriptionFragments(
        CardDescriptionContext context)
    {
        if (this is not IContextualDescriptionCard)
            return [];

        var isInHand = context.PileType == PileType.Hand
            && context.Card.Pile?.Type == PileType.Hand
            && !context.IsUpgradePreview;
        var suffix = isInHand
            ? ".handDescription"
            : ".tierDescription";

        return
        [
            new CardDescriptionFragment(
                new LocString("cards", Id.Entry + suffix),
                CardDescriptionFragmentPlacement.BeforeBase,
                0)
        ];
    }

    protected async Task GainCharge(int amount)
    {
        if (amount <= 0)
            return;

        await SecondaryResourceCmd.Gain(
            Owner,
            HammerResources.Charge.Id,
            amount,
            source: this);
    }

    protected async Task ReleaseCharge(
        PlayerChoiceContext choiceContext,
        int releasedCharge,
        CardPlay cardPlay)
    {
        var usesSnapshot = _releaseChargeSnapshot.HasValue;
        try
        {
            if (usesSnapshot && !cardPlay.IsFirstInSeries)
                return;

            await SecondaryResourceCmd.Reset(
                Owner,
                HammerResources.Charge.Id,
                source: this);

            if (releasedCharge == HammerResources.MaxCharge
                && Owner.Creature.GetPower<EndlessMomentumPower>() is { } momentum)
            {
                await momentum.TriggerRelease(choiceContext, this);
            }

            if (releasedCharge == HammerResources.MaxCharge
                && Owner.Creature.GetPower<ChargeSwitchCouragePower>() is { } courage)
            {
                await courage.TriggerRelease(choiceContext, this);
            }

            if (releasedCharge == HammerResources.MaxCharge
                && Owner.GetRelic<WirebugCage>() is { } wirebugCage)
            {
                await wirebugCage.TriggerFullRelease(choiceContext);
            }
        }
        finally
        {
            if (usesSnapshot && cardPlay.IsLastInSeries)
                _releaseChargeSnapshot = null;
        }
    }

    protected async Task GainBlock(decimal amount, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(Owner.Creature, amount, ValueProp.Move, cardPlay);
    }

    protected async Task Attack(
        PlayerChoiceContext choiceContext,
        Creature target,
        decimal damage,
        int hitCount = 1)
    {
        await DamageCmd.Attack(damage)
            .WithHitCount(ResolveAttackHitCount(hitCount))
            .FromCard(this)
            .Targeting(target)
            .Execute(choiceContext);
    }

    protected async Task AttackAll(
        PlayerChoiceContext choiceContext,
        decimal damage,
        int hitCount = 1)
    {
        await DamageCmd.Attack(damage)
            .WithHitCount(ResolveAttackHitCount(hitCount))
            .FromCard(this)
            .TargetingAllOpponents(CombatState!)
            .Execute(choiceContext);
    }

    protected int ResolveAttackHitCount(int originalHitCount)
    {
        return ResolveAttackHitCount(
            originalHitCount,
            Owner.Creature.GetPowerAmount<ComboBoostPower>());
    }

    protected static int PreviewAttackHitCount(
        ComputedDynamicVarContext context,
        int originalHitCount)
    {
        if (!IsLiveHandPreview(context))
            return originalHitCount;

        return ResolveAttackHitCount(
            originalHitCount,
            context.Player!.Creature.GetPowerAmount<ComboBoostPower>());
    }

    protected static int PreviewEnergyXValue(ComputedDynamicVarContext context)
    {
        if (!IsLiveHandPreview(context) || !context.HasCombatState)
            return 0;

        var card = context.Card!;
        var energyToSpend = card.EnergyCost.GetAmountToSpend();
        return Hook.ModifyXValue(
            context.CombatState!,
            card,
            energyToSpend);
    }

    private static bool IsLiveHandPreview(ComputedDynamicVarContext context)
    {
        return context.HasPlayer
            && context.HasCard
            && !context.IsUpgradePreview
            && context.Card!.Pile?.Type == PileType.Hand;
    }

    private static int ResolveAttackHitCount(int originalHitCount, int extraHitCount)
    {
        if (originalHitCount < 2)
            return originalHitCount;

        return originalHitCount + extraHitCount;
    }

    protected async Task DealEffectDamage(
        PlayerChoiceContext choiceContext,
        Creature target,
        decimal amount)
    {
        if (amount <= 0 || !target.IsAlive)
            return;

        await CreatureCmd.Damage(
            choiceContext,
            target,
            amount,
            ValueProp.Unblockable | ValueProp.Unpowered,
            Owner.Creature,
            this);
    }

    protected static bool IntendsToAttack(Creature? target)
    {
        return target?.Monster?.IntendsToAttack == true;
    }

    protected static int GetAttackCount(Creature? target)
    {
        var monster = target?.Monster;
        if (monster?.IntendsToAttack != true)
            return 0;

        var count = monster.NextMove.Intents
            .OfType<AttackIntent>()
            .Sum(static intent => Math.Max(1, intent.Repeats));

        return Math.Max(1, count);
    }

    protected static bool IsSleeping(Creature? target)
    {
        return target?.Monster?.NextMove.Intents.Any(static intent => intent is SleepIntent) == true;
    }
}
