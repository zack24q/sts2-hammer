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
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Interop.AutoRegistration;

namespace HammerMod.Powers;

[RegisterPower]
public sealed class SlidingComboStrengthPower : TemporaryStrengthPower
{
    public override AbstractModel OriginModel => ModelDb.Card<SlidingCombo>();
    protected override bool IsPositive => true;
}

[RegisterPower]
public sealed class ChallengerStrengthPower : TemporaryStrengthPower
{
    public override AbstractModel OriginModel => ModelDb.Card<Challenger>();
    protected override bool IsPositive => true;
}

[RegisterPower]
public sealed class ChallengerPower : HammerAbilityPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterPlayerTurnStart(
        PlayerChoiceContext choiceContext,
        Player player)
    {
        if (player.Creature != Owner
            || !CombatState.HittableEnemies.Any(
                static enemy => enemy.Monster?.IntendsToAttack == true))
        {
            return;
        }

        Flash();
        await PowerCmd.Apply<ChallengerStrengthPower>(
            choiceContext,
            Owner,
            Amount,
            Owner,
            null);
    }
}

[RegisterPower]
public sealed class WirebugContinuationPower : HammerAbilityPower
{
    private sealed class Data
    {
        public HashSet<CardPlay> QualifyingPlays { get; } = [];
    }

    private const int RequiredEnergy = 2;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override object InitInternalData()
    {
        return new Data();
    }

    public override Task BeforeCardPlayed(CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner.Creature == Owner && cardPlay.IsLastInSeries)
            GetInternalData<Data>().QualifyingPlays.Add(cardPlay);

        return Task.CompletedTask;
    }

    public override async Task AfterCardPlayed(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        if (!GetInternalData<Data>().QualifyingPlays.Remove(cardPlay)
            || cardPlay.Resources.EnergySpent < RequiredEnergy
            || Owner.Player is null)
        {
            return;
        }

        Flash();
        await SecondaryResourceCmd.Gain(
            Owner.Player,
            HammerResources.Charge.Id,
            Amount,
            source: this);
    }
}

[RegisterPower]
public sealed class WeaknessExploitPower : HammerAbilityPower
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
            || !props.IsPoweredAttack()
            || target.GetPowerAmount<VulnerablePower>() <= 0)
        {
            return 1m;
        }

        return 1m + Amount / 100m;
    }
}

[RegisterPower]
public sealed class ChargeSwitchCouragePower : HammerAbilityPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public async Task TriggerRelease(
        PlayerChoiceContext choiceContext,
        CardModel source)
    {
        Flash();
        await PowerCmd.Apply<StrengthPower>(
            choiceContext,
            Owner,
            Amount,
            Owner,
            source);
    }
}

[RegisterPower]
public sealed class PartbreakerPower : HammerAbilityPower
{
    private sealed class Data
    {
        public List<CardPlay> ActivePlays { get; } = [];
        public Dictionary<CardPlay, HashSet<Creature>> DamagedTargets { get; } = [];
    }

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override object InitInternalData()
    {
        return new Data();
    }

    public override Task BeforeCardPlayed(CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner.Creature != Owner
            || cardPlay.Card.Type != CardType.Attack)
        {
            return Task.CompletedTask;
        }

        var data = GetInternalData<Data>();
        data.ActivePlays.Add(cardPlay);
        data.DamagedTargets[cardPlay] = [];
        return Task.CompletedTask;
    }

    public override Task AfterDamageGiven(
        PlayerChoiceContext choiceContext,
        Creature? dealer,
        DamageResult result,
        ValueProp props,
        Creature target,
        CardModel? cardSource)
    {
        if (dealer != Owner
            || !target.IsMonster
            || result.UnblockedDamage <= 0
            || cardSource?.Type != CardType.Attack
            || !props.IsPoweredAttack())
        {
            return Task.CompletedTask;
        }

        var data = GetInternalData<Data>();
        for (var index = data.ActivePlays.Count - 1; index >= 0; index--)
        {
            var activePlay = data.ActivePlays[index];
            if (!ReferenceEquals(activePlay.Card, cardSource))
                continue;

            data.DamagedTargets[activePlay].Add(target);
            break;
        }

        return Task.CompletedTask;
    }

    public override async Task AfterCardPlayed(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        var data = GetInternalData<Data>();
        data.ActivePlays.Remove(cardPlay);
        if (!data.DamagedTargets.Remove(cardPlay, out var targets)
            || targets.Count == 0)
        {
            return;
        }

        Flash();
        foreach (var target in targets.Where(static target => target.IsAlive))
        {
            await PowerCmd.Apply<VulnerablePower>(
                choiceContext,
                target,
                Amount,
                Owner,
                null);
        }
    }
}

[RegisterPower]
public sealed class ComboBoostPower : HammerAbilityPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
}

[RegisterPower]
public sealed class HandCrankedTractorPower : HammerAbilityPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override int ModifyCardPlayCount(
        CardModel card,
        Creature? target,
        int currentPlayCount)
    {
        if (Amount <= 0
            || card.Owner.Creature != Owner
            || card is not HammerCard hammerCard
            || card is not IChargeReleaseCard)
        {
            return currentPlayCount;
        }

        hammerCard.SnapshotChargeOnNextRelease();
        return currentPlayCount + 1;
    }

    public override async Task AfterModifyingCardPlayCount(CardModel card)
    {
        Flash();
        await PowerCmd.Decrement(this);
    }

    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (participants.Contains(Owner))
            await PowerCmd.Remove(this);
    }
}

[RegisterPower]
public sealed class MarathonHammererPower : HammerAbilityPower, ISecondaryResourceHookListener
{
    private sealed class Data
    {
        public int GrantedStrength { get; set; }
        public bool IsSyncing { get; set; }
    }

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override object InitInternalData()
    {
        return new Data();
    }

    public async Task AfterSecondaryResourceChanged(SecondaryResourceChangeContext context)
    {
        if (context.Player.Creature != Owner
            || context.Definition.Id != HammerResources.Charge.Id)
        {
            return;
        }

        await SyncStrength(new ThrowingPlayerChoiceContext(), null);
    }

    internal async Task SyncStrength(
        PlayerChoiceContext choiceContext,
        CardModel? cardSource)
    {
        var data = GetInternalData<Data>();
        if (data.IsSyncing || Owner.Player is null)
            return;

        var desiredStrength = CalculateStrength(
            HammerResources.GetCharge(Owner.Player),
            Amount);
        var adjustment = desiredStrength - data.GrantedStrength;
        if (adjustment == 0)
            return;

        data.IsSyncing = true;
        try
        {
            var strengthBefore = Owner.GetPowerAmount<StrengthPower>();
            Flash();
            await PowerCmd.Apply<StrengthPower>(
                choiceContext,
                Owner,
                adjustment,
                Owner,
                cardSource,
                silent: true);
            var strengthAfter = Owner.GetPowerAmount<StrengthPower>();
            data.GrantedStrength += strengthAfter - strengthBefore;
        }
        finally
        {
            data.IsSyncing = false;
        }
    }

    public override async Task AfterRemoved(Creature owner)
    {
        var data = GetInternalData<Data>();
        var grantedStrength = data.GrantedStrength;
        data.GrantedStrength = 0;
        if (grantedStrength == 0)
            return;

        await PowerCmd.Apply<StrengthPower>(
            new ThrowingPlayerChoiceContext(),
            owner,
            -grantedStrength,
            owner,
            null,
            silent: true);
    }

    internal static int CalculateStrength(int charge, int stacks)
    {
        return Math.Max(0, charge) * Math.Max(0, stacks);
    }
}

[RegisterPower]
public sealed class BloodRitePower : HammerAbilityPower
{
    private const int DamageStep = 10;

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
            || !Owner.IsAlive
            || Owner.CurrentHp >= Owner.MaxHp)
        {
            return;
        }

        var healing = CalculateHealing(
            result.UnblockedDamage,
            result.OverkillDamage,
            Amount);
        if (healing <= 0)
            return;

        Flash();
        await CreatureCmd.Heal(Owner, healing);
    }

    private static int CalculateHealing(
        int unblockedDamage,
        int overkillDamage,
        int stacks)
    {
        var actualDamage = Math.Max(0, unblockedDamage - overkillDamage);
        if (actualDamage <= DamageStep || stacks <= 0)
            return 0;

        return actualDamage / DamageStep * stacks;
    }
}
