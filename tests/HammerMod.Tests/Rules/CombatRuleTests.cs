using HammerMod.Cards;
using HammerMod.Gameplay;
using HammerMod.Powers;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace HammerMod.Tests.Rules;

public sealed class CombatRuleTests
{
    [Fact]
    public void LiveCardValuesRequireAnActiveCombatCard()
    {
        Assert.True(HammerCard.ShouldUseLiveCombatValues(
            isCardInCombat: true,
            combatInProgress: true));

        Assert.False(HammerCard.ShouldUseLiveCombatValues(
            isCardInCombat: true,
            combatInProgress: false));
        Assert.False(HammerCard.ShouldUseLiveCombatValues(
            isCardInCombat: false,
            combatInProgress: true));
    }

    [Theory]
    [InlineData(-1, 10)]
    [InlineData(0, 10)]
    [InlineData(1, 20)]
    [InlineData(2, 40)]
    [InlineData(3, 80)]
    [InlineData(20, 10_485_760)]
    [InlineData(21, 10_485_760)]
    public void StunThresholdDoublesAfterEachStun(int priorStuns, int expected)
    {
        Assert.Equal(expected, HammerStun.CalculateThreshold(priorStuns));
    }

    [Theory]
    [InlineData(10)]
    [InlineData(20)]
    [InlineData(80)]
    public void StunPowerDescriptionTracksTheCurrentThreshold(int threshold)
    {
        var power = new HammerStunPower();

        power.SetThreshold(threshold);

        Assert.Equal(threshold, power.DynamicVars["Threshold"].BaseValue);
        Assert.False(power.DynamicVars["Threshold"].WasJustUpgraded);
    }

    [Theory]
    [InlineData(0, 3, 0)]
    [InlineData(1, 3, 1)]
    [InlineData(2, 0, 2)]
    [InlineData(2, 3, 5)]
    [InlineData(3, 5, 8)]
    public void ComboBoostOnlyExtendsMultiHitAttacks(
        int originalHits,
        int extraHits,
        int expected)
    {
        Assert.Equal(
            expected,
            HammerCard.ResolveAttackHitCount(originalHits, extraHits));
    }

    [Theory]
    [InlineData(false, 2, 2, 3, 2)]
    [InlineData(true, 1, 1, 3, 1)]
    [InlineData(true, 2, 2, 3, 5)]
    [InlineData(true, 2, 4, 3, 7)]
    [InlineData(true, 2, 2, -1, 2)]
    public void ComboBoostUsesTheGlobalAttackHitHook(
        bool isOwnersAttackCard,
        int originalHits,
        int currentHits,
        int extraHits,
        int expected)
    {
        Assert.Equal(
            expected,
            ComboBoostPower.CalculateHitCount(
                isOwnersAttackCard,
                originalHits,
                currentHits,
                extraHits));
    }

    [Theory]
    [InlineData(0, 1, 0)]
    [InlineData(1, 1, 1)]
    [InlineData(4, 1, 4)]
    [InlineData(7, 2, 14)]
    [InlineData(-1, 2, 0)]
    public void ImpactBurstUsesFinalHitCount(
        int hitCount,
        int stunPerHit,
        int expected)
    {
        Assert.Equal(expected, ImpactBurstPower.CalculateStun(hitCount, stunPerHit));
    }

    [Theory]
    [InlineData(0, 0, 7, 3, 0, 0)]
    [InlineData(2, 0, 7, 3, 14, 2)]
    [InlineData(2, 2, 7, 3, 17, 1)]
    [InlineData(3, 3, 9, 3, 36, 0)]
    [InlineData(3, 3, 9, 4, 39, 0)]
    public void SpinningChargeResolvesBlockAndCharge(
        int x,
        int currentCharge,
        int blockPerEnergy,
        int excessBlock,
        int expectedBlock,
        int expectedCharge)
    {
        Assert.Equal(
            (expectedBlock, expectedCharge),
            SpinningCharge.ResolveSpinningCharge(
                x,
                currentCharge,
                blockPerEnergy,
                excessBlock));
    }

    [Theory]
    [InlineData(3, 0, 3, 3, 0)]
    [InlineData(3, 2, 3, 1, 2)]
    [InlineData(3, 3, 3, 0, 3)]
    [InlineData(-1, 0, 0, 0, 0)]
    public void SpinningChargeTracksEachBlockGain(
        int x,
        int currentCharge,
        int expectedEnergyGains,
        int expectedCharge,
        int expectedExcessGains)
    {
        Assert.Equal(
            (expectedEnergyGains, expectedCharge, expectedExcessGains),
            SpinningCharge.ResolveSpinningChargeCounts(x, currentCharge, 3));
    }

    [Theory]
    [InlineData(-1, 0, 0)]
    [InlineData(100, 1, 1)]
    [InlineData(101, 2, 1)]
    [InlineData(201, 3, 2)]
    [InlineData(202, 4, 2)]
    public void EndlessMomentumTriggersOnEveryQualifyingRelease(
        int packedAmount,
        int expectedEnergy,
        int expectedCards)
    {
        Assert.Equal(
            (expectedEnergy, expectedCards),
            EndlessMomentumPower.CalculateRewards(packedAmount));
    }

    [Theory]
    [InlineData(false, true)]
    [InlineData(true, false)]
    public void OverchargeControlsWhetherReleaseClearsCharge(
        bool overchargeActive,
        bool expected)
    {
        Assert.Equal(expected, HammerCard.ShouldClearCharge(overchargeActive));
    }

    [Fact]
    public void DelayedPowersUseTheirPromisedStackingScopes()
    {
        Assert.Equal(
            PowerStackType.Counter,
            new OverchargeBacklashPower().StackType);
        Assert.Equal(
            PowerInstanceType.InstancedPerApplier,
            new AftershockPower().InstanceType);
    }

    [Theory]
    [InlineData(9, -1, 1, 9)]
    [InlineData(9, 0, 1, 9)]
    [InlineData(9, 7, 1, 16)]
    [InlineData(9, 7, 2, 23)]
    [InlineData(-3, 7, 2, 14)]
    public void HeadHunterAddsCurrentStunToBaseDamage(
        decimal baseDamage,
        int currentStun,
        int stunMultiplier,
        decimal expected)
    {
        Assert.Equal(
            expected,
            HeadHunterSmash.CalculateDamage(baseDamage, currentStun, stunMultiplier));
    }

    [Theory]
    [InlineData(100, -1, 0)]
    [InlineData(100, 0, 0)]
    [InlineData(100, 1, 1)]
    [InlineData(100, 3, 3)]
    [InlineData(101, 0, 1)]
    [InlineData(101, 3, 4)]
    [InlineData(200, 2, 4)]
    [InlineData(201, 2, 5)]
    public void KoTechniqueUsesActualEnergyAndUpgradeBonus(
        int packedAmount,
        int energySpent,
        int expected)
    {
        Assert.Equal(
            expected,
            FelyneKoTechniquePower.CalculateStun(packedAmount, energySpent));
    }

    [Theory]
    [InlineData(-1, 1, 0)]
    [InlineData(3, -1, 0)]
    [InlineData(0, 1, 0)]
    [InlineData(1, 1, 1)]
    [InlineData(2, 1, 2)]
    [InlineData(3, 1, 3)]
    [InlineData(3, 2, 6)]
    public void MarathonStrengthTracksChargeAndStacks(
        int charge,
        int stacks,
        int expected)
    {
        Assert.Equal(
            expected,
            MarathonHammererPower.CalculateStrength(charge, stacks));
    }

    [Theory]
    [InlineData(new[] { 6, 6 }, 1, 0)]
    [InlineData(new[] { 12 }, 1, 1)]
    [InlineData(new[] { 19, 9 }, 1, 1)]
    [InlineData(new[] { 20, 10 }, 1, 3)]
    [InlineData(new[] { 29 }, 2, 4)]
    [InlineData(new[] { 30 }, 0, 0)]
    public void BloodRiteUsesTotalHpLostByEachTarget(
        int[] hpLostByTarget,
        int stacks,
        int expected)
    {
        Assert.Equal(
            expected,
            BloodRitePower.CalculateHealing(hpLostByTarget, stacks));
    }
}
