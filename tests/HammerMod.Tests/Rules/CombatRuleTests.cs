using HammerMod.Cards;
using HammerMod.Gameplay;
using HammerMod.Powers;
using HammerMod.Relics;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace HammerMod.Tests.Rules;

public sealed class CombatRuleTests
{
    [Theory]
    [InlineData(-1, 0)]
    [InlineData(0, 0)]
    [InlineData(4, 0)]
    [InlineData(5, 1)]
    [InlineData(6, 1)]
    [InlineData(9, 1)]
    [InlineData(10, 2)]
    [InlineData(11, 2)]
    [InlineData(14, 2)]
    [InlineData(15, 3)]
    [InlineData(16, 3)]
    [InlineData(100, 3)]
    public void AttackDamageReducesChargeAtInclusiveFiveDamageThresholds(
        int unblockedDamage,
        int expectedLoss)
    {
        Assert.Equal(expectedLoss, HammerRules.CalculateChargeLoss(unblockedDamage));
    }

    [Theory]
    [InlineData(-1, 2, 0)]
    [InlineData(0, 2, 0)]
    [InlineData(1, 2, 0)]
    [InlineData(2, 2, 0)]
    [InlineData(3, 2, 1)]
    [InlineData(10, 2, 8)]
    [InlineData(10, -1, 10)]
    public void RocksteadyMantleReducesEachHpLossLikeTungstenRod(
        decimal amount,
        int reduction,
        decimal expected)
    {
        Assert.Equal(expected, RocksteadyMantle.ReduceHpLoss(amount, reduction));
    }

    [Theory]
    [InlineData(-1, 3, 0)]
    [InlineData(0, 3, 0)]
    [InlineData(1, 3, 3)]
    [InlineData(2, 3, 6)]
    [InlineData(3, 3, 9)]
    [InlineData(3, 4, 12)]
    [InlineData(3, -1, 0)]
    public void MasterfulPositioningUsesChargeAtTurnEnd(
        int charge,
        int blockPerCharge,
        int expectedBlock)
    {
        Assert.Equal(expectedBlock, DashJuicePower.CalculateBlock(charge, blockPerCharge));
    }

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
        Assert.Contains("Amount", power.Description.Variables.Keys);
        Assert.Contains("Threshold", power.Description.Variables.Keys);
    }

    [Theory]
    [InlineData(0, 3, 0)]
    [InlineData(1, 3, 1)]
    [InlineData(2, 0, 2)]
    [InlineData(2, 3, 5)]
    [InlineData(3, 5, 8)]
    public void OneMoreBonkOnlyExtendsMultiHitAttacks(
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
    public void OneMoreBonkUsesTheGlobalAttackHitHook(
        bool isOwnersAttackCard,
        int originalHits,
        int currentHits,
        int extraHits,
        int expected)
    {
        Assert.Equal(
            expected,
            OneMoreBonkPower.CalculateHitCount(
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
    public void SteadierWithEverySpinResolvesBlockAndCharge(
        int x,
        int currentCharge,
        int blockPerEnergy,
        int excessBlock,
        int expectedBlock,
        int expectedCharge)
    {
        Assert.Equal(
            (expectedBlock, expectedCharge),
            SteadierWithEverySpin.ResolveSteadierWithEverySpin(
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
    public void SteadierWithEverySpinTracksEachBlockGain(
        int x,
        int currentCharge,
        int expectedEnergyGains,
        int expectedCharge,
        int expectedExcessGains)
    {
        Assert.Equal(
            (expectedEnergyGains, expectedCharge, expectedExcessGains),
            SteadierWithEverySpin.ResolveSteadierWithEverySpinCounts(x, currentCharge, 3));
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
            SmashThatHead.CalculateDamage(baseDamage, currentStun, stunMultiplier));
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
            ChargeSwitchStrengthPower.CalculateStrength(charge, stacks));
    }

    [Theory]
    [InlineData(35, 100, 35, true)]
    [InlineData(34, 100, 35, true)]
    [InlineData(36, 100, 35, false)]
    [InlineData(7, 20, 35, true)]
    [InlineData(8, 20, 35, false)]
    [InlineData(1, 0, 35, false)]
    [InlineData(1, 100, -1, false)]
    public void FiregroundMightCharmUsesInclusiveHealthThreshold(
        decimal currentHp,
        decimal maxHp,
        int thresholdPercent,
        bool expected)
    {
        Assert.Equal(
            expected,
            SlidingBoostJewel.IsAtOrBelowThreshold(
                currentHp,
                maxHp,
                thresholdPercent));
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
