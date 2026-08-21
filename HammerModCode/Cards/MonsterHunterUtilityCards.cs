using HammerMod.Characters;
using HammerMod.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;

namespace HammerMod.Cards;

[RegisterCard(typeof(HammerModCardPool))]
public sealed class Coalescence : HammerCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IntVar("MaxReduction", 5),
        new PowerVar<StrengthPower>(1)
    ];

    public Coalescence()
        : base(3, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        var weak = Owner.Creature.GetPower<WeakPower>();
        var vulnerable = Owner.Creature.GetPower<VulnerablePower>();
        var frail = Owner.Creature.GetPower<FrailPower>();
        var reductions = CalculateReductions(
            Owner.Creature.GetPowerAmount<WeakPower>(),
            Owner.Creature.GetPowerAmount<VulnerablePower>(),
            Owner.Creature.GetPowerAmount<FrailPower>(),
            DynamicVars["MaxReduction"].IntValue);

        var weakReduced = await ReducePower(choiceContext, weak, reductions.Weak);
        var vulnerableReduced = await ReducePower(
            choiceContext,
            vulnerable,
            reductions.Vulnerable);
        var frailReduced = await ReducePower(choiceContext, frail, reductions.Frail);

        var totalReduced = weakReduced + vulnerableReduced + frailReduced;
        if (totalReduced > 0)
        {
            await PowerCmd.Apply<StrengthPower>(
                choiceContext,
                Owner.Creature,
                totalReduced * DynamicVars.Strength.BaseValue,
                Owner.Creature,
                this);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Strength.UpgradeValueBy(1);
    }

    internal static (int Weak, int Vulnerable, int Frail) CalculateReductions(
        int weak,
        int vulnerable,
        int frail,
        int maxReduction)
    {
        var cap = Math.Max(0, maxReduction);
        return (
            TakeReduction(weak, cap),
            TakeReduction(vulnerable, cap),
            TakeReduction(frail, cap));
    }

    private static int TakeReduction(int amount, int cap)
    {
        return Math.Min(Math.Max(0, amount), cap);
    }

    private async Task<int> ReducePower(
        PlayerChoiceContext choiceContext,
        PowerModel? power,
        int amount)
    {
        if (power is null || amount <= 0)
            return 0;

        var originalAmount = power.Amount;
        var newAmount = await PowerCmd.ModifyAmount(
            choiceContext,
            power,
            -amount,
            Owner.Creature,
            this,
            silent: false);
        return Math.Min(amount, (int)Math.Max(0m, originalAmount - newAmount));
    }
}

[RegisterCard(typeof(HammerModCardPool))]
public sealed class FreeMeal : HammerCard
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    public FreeMeal()
        : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        await PowerCmd.Apply<FreeMealPower>(
            choiceContext,
            Owner.Creature,
            1,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}

[RegisterCard(typeof(HammerModCardPool))]
public sealed class LuckyVoucher : HammerCard
{
    public LuckyVoucher()
        : base(2, CardType.Power, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        await PowerCmd.Apply<LuckyVoucherPower>(
            choiceContext,
            Owner.Creature,
            1,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
