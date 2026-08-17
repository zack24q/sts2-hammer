using System.Reflection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Models.Encounters.Mocks;
using MegaCrit.Sts2.Core.Models.Monsters.Mocks;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.TestSupport;

namespace HammerMod.Tests.Compatibility;

public sealed class GameTestSupportContractTests
{
    [Fact]
    [Trait("Category", "Compatibility")]
    public void LockedGameBuildExposesExpectedCombatTestBuildingBlocks()
    {
        const BindingFlags staticMethods =
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;

        Assert.NotNull(typeof(TestMode).GetMethod("TurnOnInternal", staticMethods));
        Assert.Contains(
            typeof(RunState).GetMethods(staticMethods),
            static method => method.Name == "CreateForTest");
        Assert.Contains(
            typeof(CombatState).GetConstructors(),
            static constructor =>
            {
                var parameters = constructor.GetParameters();
                return parameters.Length == 5
                    && parameters[0].ParameterType.Name == "EncounterModel"
                    && parameters[1].ParameterType == typeof(IRunState)
                    && parameters[4].ParameterType.Name == "MultiplayerScalingModel";
            });
        Assert.NotNull(typeof(CombatManager).GetMethod(
            nameof(CombatManager.SetUpCombat),
            [typeof(CombatState)]));
        Assert.NotNull(typeof(MockMonsterEncounter).GetConstructor(Type.EmptyTypes));
        Assert.NotNull(typeof(MockAttackMonster).GetConstructor(Type.EmptyTypes));
    }
}
