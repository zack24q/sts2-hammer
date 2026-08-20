using HammerMod.Cards;
using HammerMod.Characters;
using HammerMod.Gameplay;
using HammerMod.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace HammerMod.Potions;

public abstract class HammerPotion : ModPotionTemplate
{
    private string AssetImagePath =>
        $"{Entry.ResPath}/images/potions/{GetType().Name}.png";

    internal virtual HammerCardMechanic HoverTipMechanics => HammerCardMechanic.None;

    public override PotionUsage Usage => PotionUsage.CombatOnly;

    public override PotionAssetProfile AssetProfile => new(
        ImagePath: AssetImagePath,
        OutlinePath: AssetImagePath);

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
        HammerCardHoverTips.Create(HoverTipMechanics);
}

[RegisterPotion(typeof(HammerModPotionPool))]
public sealed class DashJuiceG : HammerPotion
{
    internal override HammerCardMechanic HoverTipMechanics => HammerCardMechanic.Charge;

    public override PotionRarity Rarity => PotionRarity.Common;
    public override TargetType TargetType => TargetType.Self;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IntVar("Charge", 3)
    ];

    protected override async Task OnUse(
        PlayerChoiceContext choiceContext,
        Creature? target)
    {
        await SecondaryResourceCmd.Gain(
            Owner,
            HammerResources.Charge.Id,
            DynamicVars["Charge"].IntValue,
            source: this);
    }
}

[RegisterPotion(typeof(HammerModPotionPool))]
public sealed class FlashBomb : HammerPotion
{
    internal override HammerCardMechanic HoverTipMechanics => HammerCardMechanic.Stun;

    public override PotionRarity Rarity => PotionRarity.Uncommon;
    public override TargetType TargetType => TargetType.AllEnemies;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IntVar("Stun", 5)
    ];

    protected override async Task OnUse(
        PlayerChoiceContext choiceContext,
        Creature? target)
    {
        var combatState = Owner.Creature.CombatState;
        if (combatState is null)
            return;

        foreach (var enemy in combatState.HittableEnemies.ToArray())
        {
            await HammerStun.Apply(
                choiceContext,
                Owner,
                this,
                enemy,
                DynamicVars["Stun"].IntValue);
        }
    }
}

[RegisterPotion(typeof(HammerModPotionPool))]
public sealed class AdamantSeed : HammerPotion
{
    internal override HammerCardMechanic HoverTipMechanics => HammerCardMechanic.Block;

    public override PotionRarity Rarity => PotionRarity.Uncommon;
    public override TargetType TargetType => TargetType.Self;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(24, ValueProp.Unpowered)
    ];

    protected override async Task OnUse(
        PlayerChoiceContext choiceContext,
        Creature? target)
    {
        ArgumentNullException.ThrowIfNull(target);
        await CreatureCmd.GainBlock(
            target,
            DynamicVars.Block.BaseValue,
            ValueProp.Unpowered,
            null);
    }
}

[RegisterPotion(typeof(HammerModPotionPool))]
public sealed class MightSeed : HammerPotion
{
    internal override HammerCardMechanic HoverTipMechanics => HammerCardMechanic.Strength;

    public override PotionRarity Rarity => PotionRarity.Rare;
    public override TargetType TargetType => TargetType.Self;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<StrengthPower>(10)
    ];

    protected override async Task OnUse(
        PlayerChoiceContext choiceContext,
        Creature? target)
    {
        ArgumentNullException.ThrowIfNull(target);
        var strengthAmount = DynamicVars.Strength.BaseValue;
        var strengthBefore = target.GetPowerAmount<StrengthPower>();
        await PowerCmd.Apply<MightSeedPower>(
            choiceContext,
            target,
            strengthAmount,
            Owner.Creature,
            null);

        // TemporaryStrengthPower normally applies Strength internally. Fill only
        // a missing remainder so this potion stays correct if that hook is skipped.
        var missingStrength = CalculateMissingStrength(
            strengthAmount,
            strengthBefore,
            target.GetPowerAmount<StrengthPower>());
        if (missingStrength > 0)
        {
            await PowerCmd.Apply<StrengthPower>(
                choiceContext,
                target,
                missingStrength,
                Owner.Creature,
                null);
        }
    }

    internal static decimal CalculateMissingStrength(
        decimal expectedStrength,
        int strengthBefore,
        int strengthAfter)
    {
        var strengthGained = strengthAfter - strengthBefore;
        return Math.Max(0m, expectedStrength - strengthGained);
    }
}

[RegisterPotion(typeof(HammerModPotionPool))]
public sealed class Pitfall : HammerPotion
{
    internal override HammerCardMechanic HoverTipMechanics => HammerCardMechanic.Vulnerable;

    public override PotionRarity Rarity => PotionRarity.Rare;
    public override TargetType TargetType => TargetType.AnyEnemy;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<VulnerablePower>(3)
    ];

    protected override async Task OnUse(
        PlayerChoiceContext choiceContext,
        Creature? target)
    {
        ArgumentNullException.ThrowIfNull(target);
        await PowerCmd.Apply<FaceOffPower>(
            choiceContext,
            Owner.Creature,
            1,
            target,
            null);
        await PowerCmd.Apply<VulnerablePower>(
            choiceContext,
            target,
            DynamicVars.Vulnerable.BaseValue,
            Owner.Creature,
            null);
    }
}
