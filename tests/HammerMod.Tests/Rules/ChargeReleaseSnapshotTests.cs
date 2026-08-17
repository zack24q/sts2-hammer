using HammerMod.Gameplay;

namespace HammerMod.Tests.Rules;

public sealed class ChargeReleaseSnapshotTests
{
    [Fact]
    public void ReplaySeriesUsesOriginalChargeAndReleasesAfterLastPlay()
    {
        var snapshot = new ChargeReleaseSnapshot();

        Assert.Equal(3, snapshot.Begin(currentCharge: 3, isFirstInSeries: true));
        Assert.True(snapshot.IsActive);
        Assert.False(snapshot.ShouldRelease(isLastInSeries: false));
        snapshot.Finish(isLastInSeries: false);

        Assert.Equal(3, snapshot.Begin(currentCharge: 0, isFirstInSeries: false));
        Assert.True(snapshot.ShouldRelease(isLastInSeries: true));
        snapshot.Finish(isLastInSeries: true);

        Assert.False(snapshot.IsActive);
        Assert.Equal(1, snapshot.Begin(currentCharge: 1, isFirstInSeries: true));
        Assert.True(snapshot.ShouldRelease(isLastInSeries: true));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void EverySeriesAutomaticallySnapshotsCharge(int charge)
    {
        var snapshot = new ChargeReleaseSnapshot();

        Assert.Equal(charge, snapshot.Begin(charge, isFirstInSeries: true));
        Assert.True(snapshot.IsActive);
        Assert.False(snapshot.ShouldRelease(isLastInSeries: false));

        snapshot.Finish(isLastInSeries: true);
        Assert.False(snapshot.IsActive);
    }
}
