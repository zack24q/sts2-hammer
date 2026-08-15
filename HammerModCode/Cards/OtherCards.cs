using HammerMod.Characters;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;

namespace HammerMod.Cards;

[RegisterCard(typeof(HammerModCardPool))]
public sealed class WakeUpHit : HammerCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.ComputedDamage(
            "Damage",
            static context => ResolveDamage(context.Target, context.IsUpgraded),
            baseValue: 9),
        new IntVar("NormalDamage", 9),
        new IntVar("NonAttackDamage", 18),
        new IntVar("SleepingDamage", 27)
    ];

    public WakeUpHit()
        : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        var damage = ResolveDamage(cardPlay.Target, IsUpgraded);
        await Attack(choiceContext, cardPlay.Target, damage);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["NormalDamage"].UpgradeValueBy(3);
        DynamicVars["NonAttackDamage"].UpgradeValueBy(6);
        DynamicVars["SleepingDamage"].UpgradeValueBy(9);
    }

    private static int ResolveDamage(MegaCrit.Sts2.Core.Entities.Creatures.Creature? target, bool upgraded)
    {
        if (IsSleeping(target))
            return upgraded ? 36 : 27;

        if (target is not null && !IntendsToAttack(target))
            return upgraded ? 24 : 18;

        return upgraded ? 12 : 9;
    }
}

[RegisterCard(typeof(HammerModCardPool))]
public sealed class ShellBreaker : HammerCard
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override bool ShouldGlowGoldInternal =>
        AnyHittableEnemy(static enemy => enemy.Block > 0);

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(11, MegaCrit.Sts2.Core.ValueProps.ValueProp.Move)
    ];

    public ShellBreaker()
        : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        await CreatureCmd.LoseBlock(cardPlay.Target, cardPlay.Target.Block);
        await Attack(choiceContext, cardPlay.Target, DynamicVars.Damage.BaseValue);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(4);
    }
}

[RegisterCard(typeof(HammerModCardPool))]
public sealed class ToolSpecialist : HammerCard
{
    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(6, MegaCrit.Sts2.Core.ValueProps.ValueProp.Move),
        new CardsVar("Cards", 1),
        new CardsVar("NextTurnCards", 1)
    ];

    public ToolSpecialist()
        : base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await GainBlock(DynamicVars.Block.BaseValue, cardPlay);
        await CardPileCmd.Draw(
            choiceContext,
            DynamicVars["Cards"].IntValue,
            Owner);
        await PowerCmd.Apply<DrawCardsNextTurnPower>(
            choiceContext,
            Owner.Creature,
            DynamicVars["NextTurnCards"].BaseValue,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(3);
    }
}

[RegisterCard(typeof(HammerModCardPool))]
public sealed class WarmUpExercise : HammerCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IntVar("Stats", 2)
    ];

    public WarmUpExercise()
        : base(2, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var amount = DynamicVars["Stats"].BaseValue;
        await PowerCmd.Apply<StrengthPower>(
            choiceContext,
            Owner.Creature,
            amount,
            Owner.Creature,
            this);
        await PowerCmd.Apply<DexterityPower>(
            choiceContext,
            Owner.Creature,
            amount,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Stats"].UpgradeValueBy(1);
    }
}
