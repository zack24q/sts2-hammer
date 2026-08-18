using HammerMod.Powers;
using MegaCrit.Sts2.Core.Entities.Cards;

namespace HammerMod.Tests.Powers;

public sealed class HammerEmergencyPowerTests
{
    [Theory]
    [InlineData(true, true, true, true, 1, true)]
    [InlineData(true, true, true, true, 0, false)]
    [InlineData(true, true, true, false, 8, false)]
    [InlineData(true, true, false, true, 8, false)]
    [InlineData(true, false, true, true, 8, false)]
    [InlineData(false, true, true, true, 8, false)]
    public void WirefallOnlyArmsAfterEnemyAttackPiercesBlock(
        bool targetIsOwner,
        bool isEnemyTurn,
        bool dealerIsEnemy,
        bool isPoweredAttack,
        int unblockedDamage,
        bool expected)
    {
        Assert.Equal(
            expected,
            WirefallPower.ShouldArmProtection(
                targetIsOwner,
                isEnemyTurn,
                dealerIsEnemy,
                isPoweredAttack,
                unblockedDamage));
    }

    [Theory]
    [InlineData(true, true, true, true, true, true)]
    [InlineData(true, true, true, true, false, false)]
    [InlineData(true, false, true, true, true, false)]
    [InlineData(true, true, false, true, true, false)]
    [InlineData(true, true, true, false, true, false)]
    public void WirefallOnlyPreventsLaterEnemyAttacks(
        bool targetIsOwner,
        bool isEnemyTurn,
        bool dealerIsEnemy,
        bool isPoweredAttack,
        bool protectionArmed,
        bool expected)
    {
        Assert.Equal(
            expected,
            WirefallPower.ShouldPreventAttack(
                targetIsOwner,
                isEnemyTurn,
                dealerIsEnemy,
                isPoweredAttack,
                protectionArmed));
    }

    [Fact]
    public void FarcasterProtectionIsScopedToItsOwnerDuringEnemyTurn()
    {
        Assert.True(FarcasterPower.ShouldPreventDamage(true, true));
        Assert.False(FarcasterPower.ShouldPreventDamage(true, false));
        Assert.False(FarcasterPower.ShouldPreventDamage(false, true));

        Assert.True(FarcasterPower.ShouldBlockPower(true, true, false));
        Assert.False(FarcasterPower.ShouldBlockPower(true, true, true));
        Assert.False(FarcasterPower.ShouldBlockPower(true, false, false));
    }

    [Theory]
    [InlineData(true, true, PileType.None, true, true)]
    [InlineData(true, false, PileType.None, true, false)]
    [InlineData(false, true, PileType.None, true, false)]
    [InlineData(true, true, PileType.Draw, true, false)]
    [InlineData(true, true, PileType.None, false, false)]
    public void FarcasterOnlyRemovesNewCombatCardsDuringEnemyTurn(
        bool ownerMatches,
        bool isEnemyTurn,
        PileType oldPileType,
        bool isInCombatPile,
        bool expected)
    {
        Assert.Equal(
            expected,
            FarcasterPower.ShouldRemoveInsertedCard(
                ownerMatches,
                isEnemyTurn,
                oldPileType,
                isInCombatPile));
    }
}
