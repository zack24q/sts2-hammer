using HammerMod.Potions;
using HammerMod.Relics;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;

namespace HammerMod.Powers;

[RegisterPower]
public sealed class CounterstrikeStrengthPower : HammerTemporaryStrengthPower
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
public sealed class MightSeedPower : HammerTemporaryStrengthPower
{
    public override AbstractModel OriginModel => ModelDb.Potion<MightSeed>();
    protected override bool IsPositive => true;
}
