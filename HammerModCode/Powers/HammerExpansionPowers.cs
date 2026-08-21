using HammerMod.Cards;
using HammerMod.Gameplay;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
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
public sealed class AffinitySlidingStrengthPower : HammerTemporaryStrengthPower
{
    public override AbstractModel OriginModel => ModelDb.Card<AffinitySliding>();
    protected override bool IsPositive => true;
}

[RegisterPower]
public sealed class ChallengerStrengthPower : HammerTemporaryStrengthPower
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
public sealed class HarderWithEverySmashPower : HammerAbilityPower
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
public sealed class OneMoreBonkPower : HammerAbilityPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override int ModifyAttackHitCount(AttackCommand attack, int hitCount)
    {
        return CalculateHitCount(
            attack.Attacker == Owner
                && attack.ModelSource is CardModel { Type: CardType.Attack },
            attack._hitCount,
            hitCount,
            Amount);
    }

    internal static int CalculateHitCount(
        bool isOwnersAttackCard,
        int originalHitCount,
        int currentHitCount,
        int extraHitCount)
    {
        if (!isOwnersAttackCard || originalHitCount < 2 || currentHitCount < 1)
            return currentHitCount;

        return currentHitCount + Math.Max(0, extraHitCount);
    }
}

[RegisterPower]
public sealed class ChargeSwitchStrengthPower : HammerAbilityPower, ISecondaryResourceHookListener
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
            if (adjustment > 0)
            {
                await PowerCmd.Apply<StrengthPower>(
                    choiceContext,
                    Owner,
                    adjustment,
                    Owner,
                    cardSource,
                    silent: true);
            }
            else if (Owner.GetPower<StrengthPower>() is { } strength)
            {
                strength.SetAmount(strength.Amount + adjustment, silent: true);
                if (strength.ShouldRemoveDueToAmount())
                    await PowerCmd.Remove(strength);
            }
            else
            {
                data.GrantedStrength = 0;
                if (desiredStrength > 0)
                {
                    await PowerCmd.Apply<StrengthPower>(
                        choiceContext,
                        Owner,
                        desiredStrength,
                        Owner,
                        cardSource,
                        silent: true);
                }
            }
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

        if (owner.GetPower<StrengthPower>() is not { } strength)
            return;

        strength.SetAmount(strength.Amount - grantedStrength, silent: true);
        if (strength.ShouldRemoveDueToAmount())
            await PowerCmd.Remove(strength);
    }

    internal static int CalculateStrength(int charge, int stacks)
    {
        if (charge < 0 || stacks <= 0)
            return 0;

        return (charge + 1) * stacks;
    }
}

[RegisterPower]
public sealed class ChargeSwitchCouragePower : HammerAbilityPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterCardPlayed(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        if (!ShouldTrigger(
                cardPlay.Card.Owner.Creature == Owner,
                cardPlay.Card.Type)
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

    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (!participants.Contains(Owner))
            return;

        await PowerCmd.Remove(this);
    }

    internal static bool ShouldTrigger(
        bool ownerMatches,
        CardType cardType)
    {
        return ownerMatches
            && cardType == CardType.Attack;
    }
}

[RegisterPower]
public sealed class BloodRitePower : HammerAbilityPower
{
    private const int DamageStep = 10;

    private sealed class Data
    {
        public List<CardPlay> ActivePlays { get; } = [];
        public Dictionary<CardPlay, Dictionary<Creature, int>> HpLostByTarget { get; } = [];
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
        data.HpLostByTarget[cardPlay] = [];
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
            || cardSource?.Type != CardType.Attack
            || !props.IsPoweredAttack())
        {
            return Task.CompletedTask;
        }

        var hpLost = Math.Max(0, result.UnblockedDamage - result.OverkillDamage);
        if (hpLost <= 0)
            return Task.CompletedTask;

        var data = GetInternalData<Data>();
        for (var index = data.ActivePlays.Count - 1; index >= 0; index--)
        {
            var activePlay = data.ActivePlays[index];
            if (!ReferenceEquals(activePlay.Card, cardSource))
                continue;

            var losses = data.HpLostByTarget[activePlay];
            losses[target] = losses.GetValueOrDefault(target) + hpLost;
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
        if (!data.HpLostByTarget.Remove(cardPlay, out var losses)
            || !Owner.IsAlive
            || Owner.CurrentHp >= Owner.MaxHp)
        {
            return;
        }

        var healing = CalculateHealing(losses.Values, Amount);
        if (healing <= 0)
            return;

        Flash();
        await CreatureCmd.Heal(Owner, healing);
    }

    internal static int CalculateHealing(IEnumerable<int> hpLostByTarget, int stacks)
    {
        if (stacks <= 0)
            return 0;

        return hpLostByTarget.Sum(
            static hpLost => Math.Max(0, hpLost) / DamageStep) * stacks;
    }
}
