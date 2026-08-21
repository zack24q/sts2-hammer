using HammerMod.Cards;
using HammerMod.Powers;

namespace HammerMod.Tests.Powers;

public sealed class ConcussionResonancePowerTests
{
    [Theory]
    [InlineData(false, 100)]
    [InlineData(true, 101)]
    public void CardEncodesItsUpgradeState(bool upgraded, int expectedAmount)
    {
        Assert.Equal(expectedAmount, ConcussionResonance.ResolvePowerAmount(upgraded));
    }

    [Theory]
    [InlineData(-1, 0, 0)]
    [InlineData(0, 0, 0)]
    [InlineData(100, 2, 1)]
    [InlineData(101, 1, 1)]
    [InlineData(200, 4, 2)]
    [InlineData(201, 3, 2)]
    [InlineData(202, 2, 2)]
    public void CalculateTurnEffectSupportsMixedUpgradeStacks(
        int packedAmount,
        int expectedChargeLoss,
        int expectedEnergy)
    {
        Assert.Equal(
            (expectedChargeLoss, expectedEnergy),
            ConcussionResonancePower.CalculateTurnEffect(packedAmount));
    }
}
