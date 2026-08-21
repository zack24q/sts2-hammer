using HammerMod.Characters;
using HammerMod.Powers;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;

namespace HammerMod.Cards;

[RegisterCard(typeof(HammerModCardPool))]
public sealed class FindASlope : HammerCard
{
    private static readonly LocString SelectionPrompt =
        new("cards", "HAMMER_MOD_CARD_FIND_A_SLOPE.selectionPrompt");

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CardsVar("Cards", 2)
    ];

    public FindASlope()
        : base(0, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        var drawPile = Owner.PlayerCombatState!.DrawPile;
        var selectionCount = Math.Min(DynamicVars["Cards"].IntValue, drawPile.Cards.Count);
        if (selectionCount <= 0)
            return;

        var prefs = new CardSelectorPrefs(
            SelectionPrompt,
            selectionCount,
            selectionCount)
        {
            Cancelable = false
        };
        var selected = await CardSelectCmd.FromCombatPile(
            choiceContext,
            drawPile,
            Owner,
            prefs);

        foreach (var card in selected.Reverse())
        {
            await CardPileCmd.Add(
                card,
                PileType.Draw,
                CardPilePosition.Top,
                this,
                skipVisuals: false);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Cards"].UpgradeValueBy(1);
    }
}

[RegisterCard(typeof(HammerModCardPool))]
public sealed class Wirefall : HammerCard
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    public Wirefall()
        : base(0, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        await PowerCmd.Apply<WirefallPower>(
            choiceContext,
            Owner.Creature,
            1,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Exhaust);
    }
}

[RegisterCard(typeof(HammerModCardPool))]
public sealed class Farcaster : HammerCard
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    public Farcaster()
        : base(3, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        foreach (var power in Owner.Creature.Powers.ToArray())
            await PowerCmd.Remove(power);

        await PowerCmd.Apply<FarcasterPower>(
            choiceContext,
            Owner.Creature,
            1,
            Owner.Creature,
            this);
        PlayerCmd.EndTurn(Owner, canBackOut: false);
    }

    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Exhaust);
    }
}
