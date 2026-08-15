using HammerMod.Relics;
using HammerMod.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace HammerMod.Gameplay;

public static class HammerStun
{
    public static int GetCurrent(Creature target)
    {
        return target.GetPowerAmount<HammerStunPower>();
    }

    public static int GetThreshold(Creature target)
    {
        var priorStuns = Math.Clamp(
            target.GetPowerAmount<HammerStunResistancePower>(),
            0,
            20);

        return 10 * (1 << priorStuns);
    }

    public static async Task Apply(
        PlayerChoiceContext choiceContext,
        CardModel source,
        Creature target,
        int amount,
        CardPlay? cardPlay = null)
    {
        await Apply(
            choiceContext,
            source.Owner,
            source,
            target,
            amount,
            source,
            cardPlay);
    }

    public static async Task Apply(
        PlayerChoiceContext choiceContext,
        Player applier,
        AbstractModel source,
        Creature target,
        int amount,
        CardModel? cardSource = null,
        CardPlay? cardPlay = null)
    {
        if (amount <= 0
            || !target.IsAlive
            || target.IsStunned
            || !target.CanReceivePowers)
        {
            return;
        }

        var threshold = GetThreshold(target);

        var before = GetCurrent(target);
        await PowerCmd.Apply<HammerStunPower>(
            choiceContext,
            target,
            amount,
            applier.Creature,
            cardSource);
        var appliedAmount = Math.Max(0, GetCurrent(target) - before);

        if (appliedAmount <= 0)
            return;

        if (GetCurrent(target) < threshold || !target.IsAlive)
            return;

        await PowerCmd.Remove<HammerStunPower>(target);
        await PowerCmd.Apply<HammerStunResistancePower>(
            choiceContext,
            target,
            1,
            applier.Creature,
            cardSource,
            silent: true);
        await CreatureCmd.Stun(target);

        if (applier.GetRelic<DownedPursuitCharm>() is { } pursuitCharm)
            await pursuitCharm.TriggerStun(choiceContext, target);
    }
}
