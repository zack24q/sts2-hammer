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
    public override PowerStackType StackType => PowerStackType.Single;
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
        var upgradedCopies = Amount % 100;
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
            var upgradedDraws = Math.Min(upgradedCopies, drawTriggers);
            await CardPileCmd.Draw(
                choiceContext,
                drawTriggers + upgradedDraws,
                player);
        }
    }
}

[RegisterPower]
public sealed class EndlessMomentumPower : HammerAbilityPower
{
    private sealed class Data
    {
        public bool TriggeredThisTurn;
    }

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override object InitInternalData()
    {
        return new Data();
    }

    public override Task AfterPlayerTurnStart(
        PlayerChoiceContext choiceContext,
        Player player)
    {
        if (player.Creature == Owner)
            GetInternalData<Data>().TriggeredThisTurn = false;

        return Task.CompletedTask;
    }

    public async Task TriggerRelease(
        PlayerChoiceContext choiceContext,
        CardModel source)
    {
        var data = GetInternalData<Data>();
        if (data.TriggeredThisTurn)
            return;

        data.TriggeredThisTurn = true;
        Flash();
        await PlayerCmd.GainEnergy(Amount, Owner.Player!);
        await CardPileCmd.Draw(choiceContext, Amount, Owner.Player!);
    }
}

[RegisterPower]
public sealed class DashJuicePower : HammerAbilityPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;
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

        var copies = Amount / 100;
        var bonusStun = Amount % 100;
        var stun = cardPlay.Resources.EnergySpent * copies + bonusStun;
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
public sealed class ImpactBurstPower : HammerAbilityPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterDamageGiven(
        PlayerChoiceContext choiceContext,
        Creature? dealer,
        DamageResult result,
        ValueProp props,
        Creature target,
        CardModel? cardSource)
    {
        if (dealer != Owner
            || !target.IsMonster
            || !target.IsAlive
            || result.UnblockedDamage <= 0
            || Owner.Player is null)
        {
            return;
        }

        await HammerStun.Apply(
            choiceContext,
            Owner.Player,
            this,
            target,
            Amount,
            cardSource);
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
        if (target != Owner || dealer != Applier || !props.IsPoweredAttack())
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
        public bool TriggeredThisTurn;
        public HashSet<CardPlay> QualifyingPlays { get; } = [];
    }

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override object InitInternalData()
    {
        return new Data();
    }

    public override Task AfterPlayerTurnStart(
        PlayerChoiceContext choiceContext,
        Player player)
    {
        if (player.Creature == Owner)
        {
            GetInternalData<Data>().TriggeredThisTurn = false;
            GetInternalData<Data>().QualifyingPlays.Clear();
        }

        return Task.CompletedTask;
    }

    public override Task BeforeCardPlayed(CardPlay cardPlay)
    {
        var data = GetInternalData<Data>();
        if (!data.TriggeredThisTurn
            && cardPlay.Card.Owner.Creature == Owner
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
        if (data.TriggeredThisTurn
            || !data.QualifyingPlays.Remove(cardPlay))
        {
            return;
        }

        data.TriggeredThisTurn = true;
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
