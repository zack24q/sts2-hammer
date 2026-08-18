using HammerMod.Powers;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Models;

namespace HammerMod.Gameplay;

[RegisterSingleton]
public sealed class LuckyVoucherRewards : HookedSingletonModel
{
    private readonly HashSet<ulong> _pendingPlayers = [];

    public LuckyVoucherRewards() : base(HookType.Run)
    {
    }

    public override Task AfterCombatEnd(CombatRoom room)
    {
        foreach (var player in room.CombatState.Players)
        {
            if (player.Creature.GetPower<LuckyVoucherPower>() is not null)
                _pendingPlayers.Add(player.NetId);
            else
                _pendingPlayers.Remove(player.NetId);
        }

        return Task.CompletedTask;
    }

    public override bool TryModifyRewardsLate(
        Player player,
        List<Reward> rewards,
        AbstractRoom? room)
    {
        if (room is not CombatRoom)
            return false;

        var shouldEnable = ShouldEnableReroll(
            isCombatRoom: true,
            _pendingPlayers.Remove(player.NetId));
        if (!shouldEnable)
            return false;

        var modified = false;
        foreach (var cardReward in rewards.OfType<CardReward>())
        {
            cardReward.CanReroll = true;
            modified = true;
        }

        return modified;
    }

    internal static bool ShouldEnableReroll(bool isCombatRoom, bool isPending)
    {
        return isCombatRoom && isPending;
    }
}
