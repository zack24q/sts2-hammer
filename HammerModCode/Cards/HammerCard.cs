using HammerMod.Gameplay;
using HammerMod.Powers;
using HammerMod.Relics;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.HoverTips;
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

public interface ICombatPreviewDescriptionCard
{
}

public interface ITargetPreviewDescriptionCard
{
}

public abstract class HammerCard : ModCardTemplate, ICardDescriptionContributor
{
    private readonly ChargeReleaseSnapshot _releaseChargeSnapshot = new();

    protected static IEnumerable<DynamicVar> ChargeTierVars(
        string prefix,
        IReadOnlyList<int> baseValues)
    {
        for (var charge = 0; charge <= HammerResources.MaxCharge; charge++)
            yield return new IntVar($"{prefix}{charge}", baseValues[charge]);
    }

    protected void UpgradeChargeTierVars(
        string prefix,
        IReadOnlyList<int> baseValues,
        IReadOnlyList<int> upgradedValues)
    {
        for (var charge = 0; charge <= HammerResources.MaxCharge; charge++)
        {
            DynamicVars[$"{prefix}{charge}"].UpgradeValueBy(
                upgradedValues[charge] - baseValues[charge]);
        }
    }

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/placeholders/{GetType().Name}.png");

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        this is IChargeReleaseCard ? [HammerKeywords.ChargeRelease] : [];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
        HammerCardHoverTips.Create(this);

    protected HammerCard(int baseCost, CardType type, CardRarity rarity, TargetType target)
        : base(baseCost, type, rarity, target)
    {
    }

    protected int ChargeLevel => HammerResources.GetCharge(Owner);

    protected int BeginChargeRelease(CardPlay cardPlay)
    {
        return _releaseChargeSnapshot.Begin(
            ChargeLevel,
            cardPlay.IsFirstInSeries);
    }

    protected bool HasChargeAtLeast(int amount)
    {
        return CombatState is not null
            && HammerResources.GetCharge(Owner) >= amount;
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
        var player = context.Player;
        if (player is null || !UsesLiveCombatValues(context))
            return 0;

        return HammerResources.GetCharge(player);
    }

    public virtual IEnumerable<CardDescriptionFragment> GetDescriptionFragments(
        CardDescriptionContext context)
    {
        return [];
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
        try
        {
            if (!_releaseChargeSnapshot.ShouldRelease(cardPlay.IsLastInSeries))
                return;

            if (ShouldClearCharge(
                    Owner.Creature.GetPower<OverchargePower>() is not null))
            {
                await SecondaryResourceCmd.Reset(
                    Owner,
                    HammerResources.Charge.Id,
                    source: this);
            }

            if (releasedCharge >= 3
                && Owner.Creature.GetPower<EndlessMomentumPower>() is { } momentum)
            {
                await momentum.TriggerRelease(choiceContext, this);
            }

            if (releasedCharge >= 3
                && Owner.Creature.GetPower<HarderWithEverySmashPower>() is { } courage)
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
            _releaseChargeSnapshot.Finish(cardPlay.IsLastInSeries);
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
            .WithHitCount(hitCount)
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
            .WithHitCount(hitCount)
            .FromCard(this)
            .TargetingAllOpponents(CombatState!)
            .Execute(choiceContext);
    }

    protected int ResolveAttackHitCount(int originalHitCount)
    {
        return ResolveAttackHitCount(
            originalHitCount,
            Owner.Creature.GetPowerAmount<OneMoreBonkPower>());
    }

    protected static int PreviewAttackHitCount(
        ComputedDynamicVarContext context,
        int originalHitCount)
    {
        if (!UsesLiveCombatValues(context) || context.Player is null)
            return originalHitCount;

        return ResolveAttackHitCount(
            originalHitCount,
            context.Player!.Creature.GetPowerAmount<OneMoreBonkPower>());
    }

    protected static int PreviewEnergyXValue(ComputedDynamicVarContext context)
    {
        if (!UsesLiveCombatValues(context) || !context.HasCombatState)
            return 0;

        var card = context.Card!;
        var energyToSpend = card.EnergyCost.GetAmountToSpend();
        return Hook.ModifyXValue(
            context.CombatState!,
            card,
            energyToSpend);
    }

    private static bool UsesLiveCombatValues(ComputedDynamicVarContext context)
    {
        return ShouldUseLiveCombatValues(
            context.IsCardInCombat,
            CombatManager.Instance.IsInProgress);
    }

    internal static bool ShouldUseLiveCombatValues(
        bool isCardInCombat,
        bool combatInProgress)
    {
        return isCardInCombat && combatInProgress;
    }

    internal static bool ShouldClearCharge(bool overchargeActive)
    {
        return !overchargeActive;
    }

    internal static int ResolveAttackHitCount(int originalHitCount, int extraHitCount)
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

    protected static async Task LoseHpDirectly(Creature target, decimal amount)
    {
        if (amount <= 0 || !target.IsAlive)
            return;

        await CreatureCmd.SetCurrentHp(target, Math.Max(0, target.CurrentHp - amount));
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
