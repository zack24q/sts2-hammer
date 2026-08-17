using HammerMod.Characters;
using HammerMod.Gameplay;
using HammerMod.Powers;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace HammerMod.Relics;

public abstract class HammerRelic : ModRelicTemplate
{
    private string ImagePath => $"{Entry.ResPath}/images/relics/{GetType().Name}.png";

    public override RelicAssetProfile AssetProfile => new(
        IconPath: ImagePath,
        IconOutlinePath: ImagePath,
        BigIconPath: ImagePath);
}

[RegisterRelic(typeof(HammerModRelicPool))]
[RegisterCharacterStarterRelic(typeof(HammerModCharacter), Order = 0)]
[RegisterTouchOfOrobasRefinement(typeof(MasterHammerTechniqueCharm))]
public sealed class HammerTechniqueCharm : HammerRelic
{
    public override RelicRarity Rarity => RelicRarity.Starter;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IntVar("Charge", 1)
    ];

    public override async Task AfterPlayerTurnStartEarly(
        PlayerChoiceContext choiceContext,
        Player player)
    {
        if (player != Owner || HammerResources.GetCharge(player) > 0)
            return;

        Flash();
        await SecondaryResourceCmd.Gain(
            player,
            HammerResources.Charge.Id,
            DynamicVars["Charge"].IntValue,
            source: this);
    }
}

[RegisterRelic(typeof(HammerModRelicPool))]
public sealed class MasterHammerTechniqueCharm : HammerRelic
{
    public override RelicRarity Rarity => RelicRarity.Starter;

    public override RelicAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/relics/MasterHammerTechniqueCharm.svg",
        IconOutlinePath: $"{Entry.ResPath}/images/relics/MasterHammerTechniqueCharm.svg",
        BigIconPath: $"{Entry.ResPath}/images/relics/MasterHammerTechniqueCharm.svg");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IntVar("Charge", 2)
    ];

    public override async Task AfterPlayerTurnStartEarly(
        PlayerChoiceContext choiceContext,
        Player player)
    {
        if (player != Owner || HammerResources.GetCharge(player) > 0)
            return;

        Flash();
        await SecondaryResourceCmd.Gain(
            player,
            HammerResources.Charge.Id,
            DynamicVars["Charge"].IntValue,
            source: this);
    }
}

[RegisterRelic(typeof(HammerModRelicPool))]
public sealed class FrostcraftCharm : HammerRelic
{
    private bool _playedAttackThisTurn;
    private bool _isCharged;

    public override RelicRarity Rarity => RelicRarity.Uncommon;

    public override RelicAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/relics/KoCharm.png",
        IconOutlinePath: $"{Entry.ResPath}/images/relics/KoCharm.png",
        BigIconPath: $"{Entry.ResPath}/images/relics/KoCharm.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IntVar("BonusPercent", 100)
    ];

    public override Task BeforeCombatStart()
    {
        _playedAttackThisTurn = false;
        _isCharged = false;
        return Task.CompletedTask;
    }

    public override Task AfterPlayerTurnStart(
        PlayerChoiceContext choiceContext,
        Player player)
    {
        if (player == Owner)
            _playedAttackThisTurn = false;

        return Task.CompletedTask;
    }

    public override decimal ModifyDamageMultiplicative(
        Creature? target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        MegaCrit.Sts2.Core.Models.CardModel? cardSource)
    {
        return _isCharged
            && dealer == Owner.Creature
            && cardSource?.Type == CardType.Attack
            && props.IsPoweredAttack()
                ? 1m + DynamicVars["BonusPercent"].BaseValue / 100m
                : 1m;
    }

    public override Task AfterCardPlayed(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner == Owner
            && cardPlay.Card.Type == CardType.Attack
            && cardPlay.IsLastInSeries)
        {
            _playedAttackThisTurn = true;
            if (_isCharged)
            {
                _isCharged = false;
                Flash();
            }
        }

        return Task.CompletedTask;
    }

    public override Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (!participants.Contains(Owner.Creature))
            return Task.CompletedTask;

        _isCharged = !_playedAttackThisTurn;
        if (_isCharged)
            Flash();

        return Task.CompletedTask;
    }
}

[RegisterRelic(typeof(HammerModRelicPool))]
public sealed class SlidingBoostJewel : HammerRelic, ISecondaryResourceHookListener
{
    public override RelicRarity Rarity => RelicRarity.Common;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<StrengthPower>(1)
    ];

    public async Task AfterSecondaryResourceChanged(SecondaryResourceChangeContext context)
    {
        if (context.Player != Owner
            || context.Definition.Id != HammerResources.Charge.Id
            || context.Delta <= 0)
        {
            return;
        }

        Flash();
        await PowerCmd.Apply<SlidingBoostPower>(
            new ThrowingPlayerChoiceContext(),
            Owner.Creature,
            DynamicVars.Strength.BaseValue * context.Delta,
            Owner.Creature,
            null);
    }
}

[RegisterRelic(typeof(HammerModRelicPool))]
public sealed class CounterstrikeCharm : HammerRelic
{
    public override RelicRarity Rarity => RelicRarity.Rare;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<StrengthPower>(2),
        new BlockVar(6, ValueProp.Unpowered)
    ];

    public async Task ScheduleRecovery(PlayerChoiceContext choiceContext)
    {
        if (Owner.Creature.GetPower<CounterstrikeRecoveryPower>() is not null)
            return;

        Flash();
        await PowerCmd.Apply<CounterstrikeRecoveryPower>(
            choiceContext,
            Owner.Creature,
            1,
            Owner.Creature,
            null,
            silent: true);
    }
}

[RegisterRelic(typeof(HammerModRelicPool))]
public sealed class WirebugCage : HammerRelic
{
    private int _lastTriggeredRound = -1;

    public override RelicRarity Rarity => RelicRarity.Rare;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new EnergyVar("Energy", 1)
    ];

    public override Task BeforeCombatStart()
    {
        _lastTriggeredRound = -1;
        return Task.CompletedTask;
    }

    public async Task TriggerFullRelease(PlayerChoiceContext choiceContext)
    {
        var round = Owner.Creature.CombatState?.RoundNumber ?? -1;
        if (round < 0 || _lastTriggeredRound == round)
            return;

        _lastTriggeredRound = round;
        Flash();
        await PlayerCmd.GainEnergy(
            DynamicVars["Energy"].BaseValue,
            Owner);
    }
}

[RegisterRelic(typeof(HammerModRelicPool))]
public sealed class DownedPursuitCharm : HammerRelic
{
    private int _lastTriggeredRound = -1;

    public override RelicRarity Rarity => RelicRarity.Common;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CardsVar("Cards", 2)
    ];

    public override Task BeforeCombatStart()
    {
        _lastTriggeredRound = -1;
        return Task.CompletedTask;
    }

    public async Task TriggerStun(
        PlayerChoiceContext choiceContext,
        Creature target)
    {
        var round = Owner.Creature.CombatState?.RoundNumber ?? -1;
        if (round < 0 || _lastTriggeredRound == round)
            return;

        _lastTriggeredRound = round;
        Flash([target]);
        await CardPileCmd.Draw(
            choiceContext,
            DynamicVars["Cards"].IntValue,
            Owner);
    }
}

[RegisterRelic(typeof(HammerModRelicPool))]
public sealed class EvasionMantle : HammerRelic
{
    private int _lastTriggeredRound = -1;
    private readonly HashSet<CardPlay> _qualifyingPlays = [];

    public override RelicRarity Rarity => RelicRarity.Shop;

    public override RelicAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/relics/AdrenalineCharm.png",
        IconOutlinePath: $"{Entry.ResPath}/images/relics/AdrenalineCharm.png",
        BigIconPath: $"{Entry.ResPath}/images/relics/AdrenalineCharm.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<StrengthPower>(1)
    ];

    public override Task BeforeCombatStart()
    {
        _lastTriggeredRound = -1;
        _qualifyingPlays.Clear();
        return Task.CompletedTask;
    }

    public override Task BeforeCardPlayed(CardPlay cardPlay)
    {
        var round = Owner.Creature.CombatState?.RoundNumber ?? -1;
        if (round >= 0
            && _lastTriggeredRound != round
            && cardPlay.Card.Owner == Owner
            && cardPlay.Card.Type == CardType.Attack
            && cardPlay.Target?.Monster?.IntendsToAttack == true)
        {
            _qualifyingPlays.Add(cardPlay);
        }

        return Task.CompletedTask;
    }

    public override async Task AfterCardPlayed(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        var round = Owner.Creature.CombatState?.RoundNumber ?? -1;
        if (round < 0
            || _lastTriggeredRound == round
            || !_qualifyingPlays.Remove(cardPlay))
        {
            return;
        }

        _lastTriggeredRound = round;
        Flash();
        await PowerCmd.Apply<StrengthPower>(
            choiceContext,
            Owner.Creature,
            DynamicVars.Strength.BaseValue,
            Owner.Creature,
            null);
    }
}

[RegisterRelic(typeof(HammerModRelicPool))]
public sealed class RocksteadyMantle : HammerRelic
{
    public override RelicRarity Rarity => RelicRarity.Shop;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(3, ValueProp.Unpowered)
    ];

    public override async Task AfterPlayerTurnStart(
        PlayerChoiceContext choiceContext,
        Player player)
    {
        if (player != Owner)
            return;

        Flash();
        await CreatureCmd.GainBlock(
            Owner.Creature,
            DynamicVars.Block.BaseValue,
            ValueProp.Unpowered,
            null,
            fast: true);
    }
}
