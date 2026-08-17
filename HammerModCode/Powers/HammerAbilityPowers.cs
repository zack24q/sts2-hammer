using HammerMod.Cards;
using HammerMod.Gameplay;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
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
    public override PowerAssetProfile AssetProfile => HammerPowerAssets.Profile;
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
public sealed class OverchargeBacklashPower : HammerAbilityPower
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;
    protected override bool IsVisibleInternal => false;

    public override async Task AfterPlayerTurnStart(
        PlayerChoiceContext choiceContext,
        Player player)
    {
        if (player.Creature != Owner)
            return;

        var amount = Amount;
        await PowerCmd.Remove(this);
        var weak = await PowerCmd.Apply<WeakPower>(
            choiceContext,
            Owner,
            amount,
            Owner,
            null);
        var vulnerable = await PowerCmd.Apply<VulnerablePower>(
            choiceContext,
            Owner,
            amount,
            Owner,
            null);
        if (weak is not null)
            weak.SkipNextDurationTick = false;
        if (vulnerable is not null)
            vulnerable.SkipNextDurationTick = false;
    }
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

        var copies = Amount / 100;
        var missingCharge = Math.Max(0, 3 - HammerResources.GetCharge(player));
        var chargeGains = Math.Min(copies, missingCharge);
        var drawTriggers = copies - chargeGains;

        if (chargeGains > 0)
        {
            await SecondaryResourceCmd.Gain(
                player,
                HammerResources.Charge.Id,
                chargeGains,
                source: this);
        }

        if (drawTriggers > 0)
        {
            await CardPileCmd.Draw(
                choiceContext,
                drawTriggers,
                player);
        }
    }
}

[RegisterPower]
public sealed class EndlessMomentumPower : HammerAbilityPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override int DisplayAmount => Math.Max(1, Amount / 100);

    public async Task TriggerRelease(
        PlayerChoiceContext choiceContext,
        CardModel source)
    {
        var (energy, cards) = CalculateRewards(Amount);
        if (energy <= 0 || cards <= 0)
            return;

        Flash();
        await PlayerCmd.GainEnergy(energy, Owner.Player!);
        await CardPileCmd.Draw(choiceContext, cards, Owner.Player!);
    }

    internal static (int Energy, int Cards) CalculateRewards(int packedAmount)
    {
        var safeAmount = Math.Max(0, packedAmount);
        var copies = safeAmount / 100;
        var upgradedCopies = safeAmount % 100;
        return (copies + upgradedCopies, copies);
    }
}

[RegisterPower]
public sealed class DashJuicePower : HammerAbilityPower, ISecondaryResourceHookListener
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public async Task AfterSecondaryResourceChanged(SecondaryResourceChangeContext context)
    {
        if (context.Player.Creature != Owner
            || context.Definition.Id != HammerResources.Charge.Id
            || context.Delta <= 0)
        {
            return;
        }

        Flash();
        await CreatureCmd.GainBlock(
            Owner,
            Amount * context.Delta,
            ValueProp.Unpowered,
            null,
            fast: true);
    }
}

[RegisterPower]
public sealed class FelyneKoTechniquePower : HammerAbilityPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override int DisplayAmount => Math.Max(1, Amount / 100);

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
        var copies = packedAmount / 100;
        var bonusStun = packedAmount % 100;
        return Math.Max(0, energySpent) * copies + bonusStun;
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

    public override async Task AfterPowerAmountChanged(
        PlayerChoiceContext choiceContext,
        PowerModel power,
        decimal amount,
        Creature? applier,
        CardModel? cardSource)
    {
        if (amount <= 0
            || applier != Owner
            || power.Owner == Owner
            || power is not (WeakPower or VulnerablePower))
        {
            return;
        }

        await HammerStun.Apply(
            choiceContext,
            Owner.Player!,
            this,
            power.Owner,
            Amount,
            cardSource);
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
public sealed class WaterStancePower : HammerAbilityPower
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
public sealed class UnloadingStancePower : TemporaryStrengthPower
{
    public override AbstractModel OriginModel => ModelDb.Card<UnloadingStance>();
    protected override bool IsPositive => false;
}
