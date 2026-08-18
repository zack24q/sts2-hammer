using HammerMod.Cards;
using HammerMod.Powers;

namespace HammerMod.Tests.Powers;

public sealed class FocusPowerTests
{
    [Theory]
    [InlineData(false, 100)]
    [InlineData(true, 101)]
    public void FocusEncodesUpgradedDrawCount(bool upgraded, int expectedAmount)
    {
        Assert.Equal(expectedAmount, Focus.ResolvePowerAmount(upgraded));
    }

    [Theory]
    [InlineData(100, 0, 1, 0)]
    [InlineData(100, 3, 0, 1)]
    [InlineData(101, 0, 1, 0)]
    [InlineData(101, 2, 1, 0)]
    [InlineData(101, 3, 0, 2)]
    [InlineData(200, 2, 1, 1)]
    [InlineData(201, 2, 1, 2)]
    [InlineData(202, 2, 1, 2)]
    public void CalculateTurnRewardsHonorsUpgradedFullChargeDraw(
        int packedAmount,
        int currentCharge,
        int expectedCharge,
        int expectedCards)
    {
        Assert.Equal(
            (expectedCharge, expectedCards),
            FocusPower.CalculateTurnRewards(packedAmount, currentCharge));
    }
}
