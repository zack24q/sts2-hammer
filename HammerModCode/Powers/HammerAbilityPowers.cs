using HammerMod.Cards;
using HammerMod.Gameplay;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Combat.AttackHits;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace HammerMod.Powers;

public abstract class HammerAbilityPower : ModPowerTemplate
{
    public override PowerAssetProfile AssetProfile =>
        HammerPowerAssets.ProfileFor(GetType());
}

[RegisterPower]
public sealed class OverchargePower : HammerAbilityPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override bool TryModifyEnergyCostInCombat(
        CardModel card,
        decimal originalCost,
        out decimal modifiedCost)
    {
        if (card.Owner.Creature == Owner
            && card is IChargeReleaseCard
            && !card.EnergyCost.CostsX)
        {
            modifiedCost = 0;
            return true;
        }

        modifiedCost = originalCost;
        return false;
    }

    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (!participants.Contains(Owner))
            return;

        await PowerCmd.Remove(this);
    }
}

[RegisterPower]
public sealed class ValorStylePower : HammerAbilityPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;
}

[RegisterPower]
public sealed class FocusPower : HammerAbilityPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override int DisplayAmount => Math.Max(1, Amount / 100);

    public override async Task AfterPlayerTurnStart(
        PlayerChoiceContext choiceContext,
        Player player)
    {
        if (player.Creature != Owner)
            return;

        var (chargeGains, cards) = CalculateTurnRewards(
            Amount,
            HammerResources.GetCharge(player));

        if (chargeGains > 0)
        {
            await SecondaryResourceCmd.Gain(
                player,
                HammerResources.Charge.Id,
                chargeGains,
                source: this);
        }

        if (cards > 0)
        {
            await CardPileCmd.Draw(
                choiceContext,
                cards,
                player);
        }
    }

    internal static (int Charge, int Cards) CalculateTurnRewards(
        int packedAmount,
        int currentCharge)
    {
        var safeAmount = Math.Max(0, packedAmount);
        var copies = safeAmount / 100;
        var upgradedCopies = Math.Min(copies, safeAmount % 100);
        var missingCharge = Math.Max(0, HammerResources.MaxCharge - currentCharge);
        var chargeGains = Math.Min(copies, missingCharge);
        var fullChargeTriggers = copies - chargeGains;
        var upgradedFullChargeTriggers = Math.Min(upgradedCopies, fullChargeTriggers);
        return (chargeGains, fullChargeTriggers + upgradedFullChargeTriggers);
    }
}

[RegisterPower]
public sealed class EndlessMomentumPower : HammerAbilityPower
{
    private int _lastTriggeredRound = -1;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override int DisplayAmount => Math.Max(1, Amount);

    public override LocString Description
    {
        get
        {
            var description = base.Description;
            var (energy, cards) = CalculateRewards(Amount);
            description.Add("Energy", energy);
            description.Add("Cards", cards);
            return description;
        }
    }

    public async Task TriggerRelease(
        PlayerChoiceContext choiceContext,
        CardModel source)
    {
        var (energy, cards) = CalculateRewards(Amount);
        if (energy <= 0
            || cards <= 0
            || !TryStartReleaseForRound(CombatState.RoundNumber))
            return;

        Flash();
        await PlayerCmd.GainEnergy(energy, Owner.Player!);
        await CardPileCmd.Draw(choiceContext, cards, Owner.Player!);
    }

    internal bool TryStartReleaseForRound(int round)
    {
        if (round < 0 || _lastTriggeredRound == round)
            return false;

        _lastTriggeredRound = round;
        return true;
    }

    internal static (int Energy, int Cards) CalculateRewards(int amount)
    {
        var stacks = Math.Max(0, amount);
        return (stacks, stacks * 2);
    }
}

[RegisterPower]
public sealed class DashJuicePower : HammerAbilityPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side != CombatSide.Player
            || !participants.Contains(Owner)
            || Owner.Player is null)
            return;

        var block = CalculateBlock(HammerResources.GetCharge(Owner.Player), Amount);
        if (block <= 0)
            return;

        Flash();
        await CreatureCmd.GainBlock(
            Owner,
            block,
            ValueProp.Unpowered,
            null,
            fast: true);
    }

    internal static int CalculateBlock(int charge, int blockPerCharge)
    {
        return Math.Max(0, charge) * Math.Max(0, blockPerCharge);
    }
}

[RegisterPower]
public sealed class FelyneKoTechniquePower : HammerAbilityPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override int DisplayAmount => Math.Max(1, Amount / 100);

    public override LocString Description
    {
        get
        {
            var description = base.Description;
            var (energyMultiplier, bonusStun) = CalculateBonuses(Amount);
            description.Add("EnergyMultiplier", energyMultiplier);
            description.Add("BonusStun", bonusStun);
            return description;
        }
    }

    public override async Task AfterCardPlayed(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner.Creature != Owner
            || cardPlay.Card.Type != CardType.Attack
            || !cardPlay.IsLastInSeries)
        {
            return;
        }

        var stun = CalculateStun(Amount, cardPlay.Resources.EnergySpent);
        if (stun <= 0)
            return;

        var targets = cardPlay.Target is { } target
            ? new[] { target }
            : CombatState.HittableEnemies.ToArray();

        foreach (var enemy in targets)
        {
            await HammerStun.Apply(
                choiceContext,
                cardPlay.Card,
                enemy,
                stun,
                cardPlay);
        }
    }

    internal static int CalculateStun(int packedAmount, int energySpent)
    {
        var (energyMultiplier, bonusStun) = CalculateBonuses(packedAmount);
        return Math.Max(0, energySpent) * energyMultiplier + bonusStun;
    }

    internal static (int EnergyMultiplier, int BonusStun) CalculateBonuses(
        int packedAmount)
    {
        var safeAmount = Math.Max(0, packedAmount);
        var copies = safeAmount / 100;
        var upgradedCopies = Math.Min(copies, safeAmount % 100);
        return (copies, upgradedCopies);
    }
}

[RegisterPower]
public sealed class PileDriverPower : HammerAbilityPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override decimal ModifyDamageMultiplicative(
        Creature? target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        if (dealer != Owner
            || target is null
            || cardSource?.Type != CardType.Attack
            || !props.IsPoweredAttack())
        {
            return 1m;
        }

        decimal bonusPerStack = target.IsStunned
            ? 1m
            : HammerStun.GetCurrent(target) > 0
                ? 0.5m
                : 0m;

        return 1m + Amount * bonusPerStack;
    }
}

[RegisterPower]
public sealed class ConcussionResonancePower : HammerAbilityPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override int DisplayAmount => Math.Max(1, Amount / 100);

    public override LocString Description
    {
        get
        {
            var description = base.Description;
            var (chargeLoss, energy) = CalculateTurnEffect(Amount);
            description.Add("ChargeLoss", chargeLoss);
            description.Add("Energy", energy);
            return description;
        }
    }

    public override async Task AfterPlayerTurnStart(
        PlayerChoiceContext choiceContext,
        Player player)
    {
        if (player.Creature != Owner)
            return;

        var (chargeLoss, energy) = CalculateTurnEffect(Amount);
        if (chargeLoss > 0 && HammerResources.GetCharge(player) > 0)
        {
            await SecondaryResourceCmd.Lose(
                player,
                HammerResources.Charge.Id,
                chargeLoss,
                source: this);
        }

        if (energy > 0)
        {
            Flash();
            await PlayerCmd.GainEnergy(energy, player);
        }
    }

    internal static (int ChargeLoss, int Energy) CalculateTurnEffect(int packedAmount)
    {
        var safeAmount = Math.Max(0, packedAmount);
        var copies = safeAmount / 100;
        var upgradedCopies = Math.Min(copies, safeAmount % 100);
        return (copies * 2 - upgradedCopies, copies);
    }
}

[RegisterPower]
public sealed class ImpactBurstPower : HammerAbilityPower, IAttackHitHookListener
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public async Task AfterAttackHit(AttackHitContext context)
    {
        if (context.Dealer != Owner
            || context.CardSource?.Type != CardType.Attack
            || context.TotalHitCount < 2
            || context.HitNumber != context.TotalHitCount
            || Owner.Player is null)
        {
            return;
        }

        Flash();
        var stun = CalculateStun((int)context.TotalHitCount, Amount);
        foreach (var target in context.Targets.Where(
                     static target => target.IsMonster && target.IsAlive))
        {
            await HammerStun.Apply(
                context.ChoiceContext,
                Owner.Player,
                this,
                target,
                stun,
                context.CardSource);
        }
    }

    internal static int CalculateStun(int hitCount, int stunPerHit)
    {
        return Math.Max(0, hitCount) * Math.Max(0, stunPerHit);
    }
}

[RegisterPower]
public sealed class FaceOffPower : HammerAbilityPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;
    public override PowerInstanceType InstanceType => PowerInstanceType.InstancedPerApplier;

    public override decimal ModifyDamageMultiplicative(
        Creature? target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        if (target != Owner || dealer != Applier)
            return 1m;

        return 0m;
    }

    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side == CombatSide.Enemy)
            await PowerCmd.Remove(this);
    }
}

[RegisterPower]
public sealed class WeaveAndBonkPower : HammerAbilityPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task BeforeDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        if (target != Owner
            || dealer is null
            || (!props.IsPoweredAttack() && cardSource is not Omnislice))
            return;

        Flash();
        await CreatureCmd.Damage(
            choiceContext,
            dealer,
            Amount,
            ValueProp.Unpowered | ValueProp.SkipHurtAnim,
            Owner,
            null);
    }

    public override async Task AfterPlayerTurnStart(
        PlayerChoiceContext choiceContext,
        Player player)
    {
        if (player.Creature == Owner)
            await PowerCmd.Remove(this);
    }
}

[RegisterPower]
public sealed class CounterFormPower : HammerAbilityPower
{
    private sealed class Data
    {
        public HashSet<CardPlay> QualifyingPlays { get; } = [];
    }

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override object InitInternalData()
    {
        return new Data();
    }

    public override Task BeforeCardPlayed(CardPlay cardPlay)
    {
        var data = GetInternalData<Data>();
        if (cardPlay.Card.Owner.Creature == Owner
            && cardPlay.Card.Type == CardType.Attack
            && cardPlay.Target?.Monster?.IntendsToAttack == true)
        {
            data.QualifyingPlays.Add(cardPlay);
        }

        return Task.CompletedTask;
    }

    public override async Task AfterCardPlayed(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        var data = GetInternalData<Data>();
        if (!data.QualifyingPlays.Remove(cardPlay))
            return;

        Flash();
        await CreatureCmd.GainBlock(
            Owner,
            Amount,
            ValueProp.Unpowered,
            null,
            fast: true);
    }
}

[RegisterPower]
public sealed class AftershockPower : HammerAbilityPower
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override PowerInstanceType InstanceType => PowerInstanceType.InstancedPerApplier;

    public override async Task AfterPlayerTurnStart(
        PlayerChoiceContext choiceContext,
        Player player)
    {
        if (Applier?.Player != player)
            return;

        var target = Owner;
        var stun = Amount;
        await PowerCmd.Remove(this);
        await HammerStun.Apply(
            choiceContext,
            player,
            this,
            target,
            stun);
    }
}

[RegisterPower]
public sealed class UnloadingStancePower : HammerTemporaryStrengthPower
{
    public override AbstractModel OriginModel => ModelDb.Card<StaminaDrainingRoar>();
    protected override bool IsPositive => false;
}
