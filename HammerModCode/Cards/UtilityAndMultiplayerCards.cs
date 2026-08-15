using HammerMod.Characters;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;

namespace HammerMod.Cards;

[RegisterCard(typeof(HammerModCardPool))]
public sealed class Reposition : HammerCard
{
    private static readonly LocString SelectionPrompt =
        new("cards", "HAMMER_MOD_CARD_REPOSITION.selectionPrompt");

    public override bool GainsBlock => true;

    protected override bool ShouldGlowGoldInternal =>
        CombatState is not null && Owner.PlayerCombatState?.DiscardPile.IsEmpty == false;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(6, ValueProp.Move)
    ];

    public Reposition()
        : base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        await GainBlock(DynamicVars.Block.BaseValue, cardPlay);

        var discardPile = Owner.PlayerCombatState!.DiscardPile;
        if (discardPile.IsEmpty)
            return;

        var prefs = new CardSelectorPrefs(SelectionPrompt, 1, 1)
        {
            Cancelable = false
        };
        var selected = (await CardSelectCmd.FromCombatPile(
            choiceContext,
            discardPile,
            Owner,
            prefs)).FirstOrDefault();

        if (selected is not null)
        {
            await CardPileCmd.Add(
                selected,
                PileType.Draw,
                CardPilePosition.Top,
                null,
                skipVisuals: false);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(3);
    }
}

[RegisterCard(typeof(HammerModCardPool))]
public sealed class LaunchTeammate : HammerCard
{
    public override CardMultiplayerConstraint MultiplayerConstraint =>
        CardMultiplayerConstraint.MultiplayerOnly;

    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(16, ValueProp.Move),
        new EnergyVar("Energy", 1)
    ];

    public LaunchTeammate()
        : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.AnyAlly)
    {
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        var teammate = cardPlay.Target.Player;
        ArgumentNullException.ThrowIfNull(teammate);

        await CreatureCmd.GainBlock(
            teammate.Creature,
            DynamicVars.Block.BaseValue,
            ValueProp.Move,
            cardPlay);
        await PowerCmd.Apply<EnergyNextTurnPower>(
            choiceContext,
            teammate.Creature,
            DynamicVars["Energy"].BaseValue,
            Owner.Creature,
            this);

        var getBackUp = CombatState!.CreateCard<GetBackUp>(teammate);
        await CardPileCmd.AddGeneratedCardToCombat(
            getBackUp,
            PileType.Hand,
            Owner,
            CardPilePosition.Bottom);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(6);
    }
}

[RegisterCard(typeof(HammerModCardPool))]
public sealed class DemonPowder : HammerCard
{
    public override CardMultiplayerConstraint MultiplayerConstraint =>
        CardMultiplayerConstraint.MultiplayerOnly;

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<StrengthPower>(1)
    ];

    public DemonPowder()
        : base(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        foreach (var player in CombatState!.Players.Where(
                     static player => player.Creature.IsAlive))
        {
            await PowerCmd.Apply<StrengthPower>(
                choiceContext,
                player.Creature,
                DynamicVars.Strength.BaseValue,
                Owner.Creature,
                this);
        }
    }

    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Exhaust);
    }
}

[RegisterCard(typeof(HammerModCardPool))]
public sealed class HardshellPowder : HammerCard
{
    public override CardMultiplayerConstraint MultiplayerConstraint =>
        CardMultiplayerConstraint.MultiplayerOnly;

    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(5, ValueProp.Move)
    ];

    public HardshellPowder()
        : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        foreach (var player in CombatState!.Players.Where(
                     static player => player.Creature.IsAlive))
        {
            await CreatureCmd.GainBlock(
                player.Creature,
                DynamicVars.Block.BaseValue,
                ValueProp.Move,
                cardPlay);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(3);
    }
}

[RegisterCard(typeof(HammerModCardPool))]
public sealed class GetBackUp : HammerCard
{
    public override CardMultiplayerConstraint MultiplayerConstraint =>
        CardMultiplayerConstraint.MultiplayerOnly;

    public override bool CanBeGeneratedInCombat => false;
    public override bool CanBeGeneratedByModifiers => false;

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        [CardKeyword.Exhaust];

    public override int MaxUpgradeLevel => 0;

    public GetBackUp()
        : base(1, CardType.Status, CardRarity.Status, TargetType.Self)
    {
    }

    protected override Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        return Task.CompletedTask;
    }
}
