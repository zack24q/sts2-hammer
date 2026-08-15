using HammerMod.Characters;
using HammerMod.Powers;
using HammerMod.Relics;
using MegaCrit.Sts2.Core.Commands;
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
public sealed class HammerRules : HookedSingletonModel, ISecondaryResourceHookListener
{
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
            || player.Character is not HammerModCharacter
            || dealer is null
            || !dealer.IsMonster
            || !props.HasFlag(ValueProp.Move)
            || result.UnblockedDamage <= 0
            || HammerResources.GetCharge(player) <= 0
            || target.GetPower<DashJuicePower>() is not null
            || target.GetPower<AdamantSeedPower>() is not null
            || player.GetRelic<RocksteadyMantle>() is not null)
        {
            return;
        }

        var chargeBeforeHit = HammerResources.GetCharge(player);
        await SecondaryResourceCmd.Reset(
            player,
            HammerResources.Charge.Id,
            source: this);

        if (HammerResources.GetCharge(player) < chargeBeforeHit
            && player.GetRelic<CounterstrikeCharm>() is { } counterstrike)
        {
            await counterstrike.ScheduleRecovery(choiceContext);
        }
    }
}
