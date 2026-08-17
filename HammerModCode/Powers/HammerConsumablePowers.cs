using HammerMod.Gameplay;
using HammerMod.Potions;
using HammerMod.Relics;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;

namespace HammerMod.Powers;

[RegisterPower]
public sealed class CounterstrikeRecoveryPower : HammerAbilityPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override async Task AfterPlayerTurnStart(
        PlayerChoiceContext choiceContext,
        Player player)
    {
        if (player.Creature != Owner)
            return;

        var relic = player.GetRelic<CounterstrikeCharm>();
        await PowerCmd.Remove(this);
        if (relic is null)
            return;

        relic.Flash();
        await PowerCmd.Apply<CounterstrikeStrengthPower>(
            choiceContext,
            Owner,
            relic.DynamicVars.Strength.BaseValue,
            Owner,
            null);
        await CreatureCmd.GainBlock(
            Owner,
            relic.DynamicVars.Block.BaseValue,
            ValueProp.Unpowered,
            null,
            fast: true);
    }
}

[RegisterPower]
public sealed class CounterstrikeStrengthPower : TemporaryStrengthPower
{
    public override AbstractModel OriginModel => ModelDb.Relic<CounterstrikeCharm>();
    protected override bool IsPositive => true;
}

[RegisterPower]
public sealed class AdamantSeedPower : HammerAbilityPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override async Task AfterPlayerTurnStart(
        PlayerChoiceContext choiceContext,
        Player player)
    {
        if (player.Creature == Owner)
            await PowerCmd.Remove(this);
    }
}

[RegisterPower]
public sealed class MightSeedPower : TemporaryStrengthPower
{
    public override AbstractModel OriginModel => ModelDb.Potion<MightSeed>();
    protected override bool IsPositive => true;
}

[RegisterPower]
public sealed class SlidingBoostPower : TemporaryStrengthPower
{
    public override AbstractModel OriginModel => ModelDb.Relic<SlidingBoostJewel>();
    protected override bool IsPositive => true;
}
