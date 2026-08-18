using HammerMod.Cards;
using HammerMod.Gameplay;
using HammerMod.Powers;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace HammerMod.Tests.Rules;

public sealed class HammerUtilityRuleTests
{
    [Theory]
    [InlineData(3, 4, 5, 5, 3, 2, 0)]
    [InlineData(0, 2, 2, 5, 0, 2, 2)]
    [InlineData(8, 2, 1, 5, 5, 0, 0)]
    [InlineData(-1, 2, 4, 5, 0, 2, 3)]
    [InlineData(2, 2, 2, 0, 0, 0, 0)]
    public void CoalescenceReducesAtMostTheSharedCap(
        int weak,
        int vulnerable,
        int frail,
        int cap,
        int expectedWeak,
        int expectedVulnerable,
        int expectedFrail)
    {
        Assert.Equal(
            (expectedWeak, expectedVulnerable, expectedFrail),
            Coalescence.CalculateReductions(weak, vulnerable, frail, cap));
    }

    [Fact]
    public void FreeMealSkipsExactlyOneOfficialPotionRemoval()
    {
        Assert.False(FreeMealPotionPatch.ShouldRunOriginal(hasFreeMeal: true));
        Assert.True(FreeMealPotionPatch.ShouldRunOriginal(hasFreeMeal: false));

        var power = new FreeMealPower();
        Assert.Equal(PowerType.Buff, power.Type);
        Assert.Equal(PowerStackType.Single, power.StackType);
    }

    [Theory]
    [InlineData(true, true, true)]
    [InlineData(true, false, false)]
    [InlineData(false, true, false)]
    [InlineData(false, false, false)]
    public void LuckyVoucherOnlyEnablesCombatRewards(
        bool isCombatRoom,
        bool isPending,
        bool expected)
    {
        Assert.Equal(
            expected,
            LuckyVoucherRewards.ShouldEnableReroll(isCombatRoom, isPending));
    }

    [Fact]
    public void LuckyVoucherPowerDoesNotStackExtraRerolls()
    {
        var power = new LuckyVoucherPower();
        Assert.Equal(PowerType.Buff, power.Type);
        Assert.Equal(PowerStackType.Single, power.StackType);
    }
}
