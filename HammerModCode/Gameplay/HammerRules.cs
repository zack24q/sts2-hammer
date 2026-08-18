using HammerMod.Characters;
using HammerMod.Powers;
using HammerMod.Relics;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Models;

namespace HammerMod.Gameplay;

[RegisterSingleton]
public sealed class HammerRules : HookedSingletonModel,
    ISecondaryResourceHookListener
{
    private ICombatState? _trackedCombat;
    private readonly HashSet<Creature> _playersWhoAttackedThisTurn = [];

    public HammerRules() : base(HookType.Combat)
    {
    }

    public async Task AfterSecondaryResourceChanged(SecondaryResourceChangeContext context)
    {
        if (context.Definition.Id != HammerResources.Charge.Id
            || context.Delta <= 0
            || context.Player.Character is not HammerModCharacter
            || !context.Player.Creature.IsAlive)
        {
            return;
        }

        NPowerUpVfx.CreateNormal(context.Player.Creature);
        await CreatureCmd.TriggerAnim(
            context.Player.Creature,
            "PowerUp",
            context.Player.Character.PowerUpAnimDelay);
    }

    public override Task AfterCardPlayed(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        var owner = cardPlay.Card.Owner.Creature;
        if (owner.CombatState is { } combat)
        {
            EnsureCombat(combat);
            if (cardPlay.Card.Type == CardType.Attack && cardPlay.IsLastInSeries)
                _playersWhoAttackedThisTurn.Add(owner);
        }

        return Task.CompletedTask;
    }

    public override Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side == CombatSide.Player)
        {
            foreach (var participant in participants)
                _playersWhoAttackedThisTurn.Remove(participant);
        }

        return Task.CompletedTask;
    }

    internal bool HasPlayedAttackThisTurn(Creature player)
    {
        if (CurrentCombatState is not { } combat)
            return false;

        EnsureCombat(combat);
        return _playersWhoAttackedThisTurn.Contains(player);
    }

    private void EnsureCombat(ICombatState combat)
    {
        if (ReferenceEquals(_trackedCombat, combat))
            return;

        _trackedCombat = combat;
        _playersWhoAttackedThisTurn.Clear();
    }

    public override async Task AfterDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        DamageResult result,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        var player = target.Player;

        if (!target.IsPlayer
            || player is null
            || player.Character is not HammerModCharacter)
        {
            return;
        }

        if (result.UnblockedDamage > 0
            && player.GetRelic<CounterstrikeCharm>() is { } counterstrike)
        {
            await counterstrike.TriggerCounterstrike(choiceContext);
        }

        var chargeLoss = CalculateChargeLoss(result.UnblockedDamage);
        if (dealer?.IsMonster != true
            || !props.HasFlag(ValueProp.Move)
            || chargeLoss <= 0
            || HammerResources.GetCharge(player) <= 0
            || target.GetPower<AdamantSeedPower>() is not null
            || player.GetRelic<RocksteadyMantle>() is not null)
        {
            return;
        }

        await SecondaryResourceCmd.Lose(
            player,
            HammerResources.Charge.Id,
            chargeLoss,
            source: this);
    }

    internal static int CalculateChargeLoss(int unblockedDamage)
    {
        if (unblockedDamage < 5)
            return 0;

        return Math.Min(HammerResources.MaxCharge, unblockedDamage / 5);
    }
}
