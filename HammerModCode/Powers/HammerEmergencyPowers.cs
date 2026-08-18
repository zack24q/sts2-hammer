using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;

namespace HammerMod.Powers;

[RegisterPower]
public sealed class WirefallPower : HammerAbilityPower
{
    private sealed class Data
    {
        public bool TookUnblockedAttackDamage { get; set; }
    }

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override object InitInternalData()
    {
        return new Data();
    }

    public override decimal ModifyDamageMultiplicative(
        Creature? target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        var shouldPrevent = ShouldPreventAttack(
            target == Owner,
            CombatState?.CurrentSide == CombatSide.Enemy,
            dealer?.IsEnemy == true,
            props.IsPoweredAttack(),
            GetInternalData<Data>().TookUnblockedAttackDamage);
        return shouldPrevent ? 0m : 1m;
    }

    public override Task AfterDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        DamageResult result,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        var data = GetInternalData<Data>();
        if (!data.TookUnblockedAttackDamage
            && ShouldArmProtection(
                target == Owner,
                CombatState?.CurrentSide == CombatSide.Enemy,
                dealer?.IsEnemy == true,
                props.IsPoweredAttack(),
                result.UnblockedDamage))
        {
            data.TookUnblockedAttackDamage = true;
            Flash();
        }

        return Task.CompletedTask;
    }

    public override async Task AfterPlayerTurnStart(
        PlayerChoiceContext choiceContext,
        Player player)
    {
        if (player.Creature != Owner)
            return;

        GetInternalData<Data>().TookUnblockedAttackDamage = false;
        await PowerCmd.Decrement(this);
    }

    internal static bool ShouldArmProtection(
        bool targetIsOwner,
        bool isEnemyTurn,
        bool dealerIsEnemy,
        bool isPoweredAttack,
        int unblockedDamage)
    {
        return targetIsOwner
            && isEnemyTurn
            && dealerIsEnemy
            && isPoweredAttack
            && unblockedDamage > 0;
    }

    internal static bool ShouldPreventAttack(
        bool targetIsOwner,
        bool isEnemyTurn,
        bool dealerIsEnemy,
        bool isPoweredAttack,
        bool protectionArmed)
    {
        return protectionArmed
            && targetIsOwner
            && isEnemyTurn
            && dealerIsEnemy
            && isPoweredAttack;
    }
}

[RegisterPower]
public sealed class FarcasterPower : HammerAbilityPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override decimal ModifyDamageCap(
        Creature? target,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        return ShouldPreventDamage(
            target == Owner,
            CombatState?.CurrentSide == CombatSide.Enemy)
            ? 0m
            : decimal.MaxValue;
    }

    public override decimal ModifyHpLostAfterOsty(
        Creature target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        return ShouldPreventDamage(
            target == Owner,
            CombatState?.CurrentSide == CombatSide.Enemy)
            ? 0m
            : amount;
    }

    public override Task AfterModifyingDamageAmount(CardModel? cardSource)
    {
        Flash();
        return Task.CompletedTask;
    }

    public override Task AfterModifyingHpLostAfterOsty()
    {
        Flash();
        return Task.CompletedTask;
    }

    public override bool TryModifyPowerAmountReceived(
        PowerModel canonicalPower,
        Creature target,
        decimal amount,
        Creature? applier,
        out decimal modifiedAmount)
    {
        if (ShouldBlockPower(
                target == Owner,
                CombatState?.CurrentSide == CombatSide.Enemy,
                canonicalPower is FarcasterPower))
        {
            modifiedAmount = 0m;
            return true;
        }

        modifiedAmount = amount;
        return false;
    }

    public override Task AfterModifyingPowerAmountReceived(PowerModel power)
    {
        Flash();
        return Task.CompletedTask;
    }

    public override async Task AfterCardChangedPiles(
        CardModel card,
        PileType oldPileType,
        AbstractModel? clonedBy)
    {
        if (!ShouldRemoveInsertedCard(
                card.Owner.Creature == Owner,
                CombatState?.CurrentSide == CombatSide.Enemy,
                oldPileType,
                card.Pile?.IsCombatPile == true))
        {
            return;
        }

        Flash();
        await CardPileCmd.RemoveFromCombat(card, skipVisuals: true);
    }

    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side == CombatSide.Enemy)
            await PowerCmd.Decrement(this);
    }

    internal static bool ShouldPreventDamage(bool targetIsOwner, bool isEnemyTurn)
    {
        return targetIsOwner && isEnemyTurn;
    }

    internal static bool ShouldBlockPower(
        bool targetIsOwner,
        bool isEnemyTurn,
        bool isFarcasterPower)
    {
        return targetIsOwner && isEnemyTurn && !isFarcasterPower;
    }

    internal static bool ShouldRemoveInsertedCard(
        bool ownerMatches,
        bool isEnemyTurn,
        PileType oldPileType,
        bool isInCombatPile)
    {
        return ownerMatches
            && isEnemyTurn
            && oldPileType == PileType.None
            && isInCombatPile;
    }
}
