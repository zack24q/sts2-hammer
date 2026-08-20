using HammerMod.Powers;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Scaffolding.Content.Patches;

namespace HammerMod.Tests.Powers;

public sealed class PowerAssetContractTests
{
    private static readonly IReadOnlyDictionary<Type, string>
        AssignedIconPaths = new Dictionary<Type, string>
        {
            [typeof(HammerStunPower)] = "images/powers/HammerStun.png",
            [typeof(HammerStunResistancePower)] = "images/powers/HammerStun.png",
            [typeof(AftershockPower)] = "images/powers/Aftershock.png",
            [typeof(FocusPower)] = "images/powers/Focus.png",
            [typeof(EndlessMomentumPower)] = "images/powers/EndlessMomentum.png",
            [typeof(DashJuicePower)] = "images/powers/DashJuice.png",
            [typeof(FelyneKoTechniquePower)] = "images/powers/FelyneKoTechnique.png",
            [typeof(PileDriverPower)] = "images/powers/PileDriver.png",
            [typeof(ConcussionResonancePower)] = "images/powers/ConcussionResonance.png",
            [typeof(ImpactBurstPower)] = "images/powers/ImpactBurst.png",
            [typeof(CounterFormPower)] = "images/powers/CounterForm.png",
            [typeof(WeaveAndBonkPower)] = "images/powers/CounterForm.png",
            [typeof(ChallengerPower)] = "images/powers/Challenger.png",
            [typeof(ChallengerStrengthPower)] = "images/powers/Challenger.png",
            [typeof(WirebugContinuationPower)] = "images/powers/WirebugContinuation.png",
            [typeof(WeaknessExploitPower)] = "images/powers/WeaknessExploit.png",
            [typeof(HarderWithEverySmashPower)] = "images/powers/HarderWithEverySmash.png",
            [typeof(PartbreakerPower)] = "images/powers/Partbreaker.png",
            [typeof(OneMoreBonkPower)] = "images/powers/OneMoreBonk.png",
            [typeof(BloodRitePower)] = "images/powers/BloodRite.png",
            [typeof(ChargeSwitchStrengthPower)] = "images/powers/ChargeSwitchStrength.png",
            [typeof(AffinitySlidingStrengthPower)] = "images/powers/ChargeSwitchStrength.png",
            [typeof(LuckyVoucherPower)] = "images/powers/LuckyVoucher.png",
            [typeof(OverchargePower)] = "images/powers/Overcharge.png",
            [typeof(FaceOffPower)] = "images/powers/FaceOff.png",
            [typeof(UnloadingStancePower)] = "images/powers/FaceOff.png",
            [typeof(WirefallPower)] = "images/powers/Wirefall.png",
            [typeof(FarcasterPower)] = "images/powers/Farcaster.png",
            [typeof(FreeMealPower)] = "images/powers/FreeMeal.png",
            [typeof(ChargeSwitchCouragePower)] = "images/powers/ChargeSwitchCourage.png",
            [typeof(CounterstrikeStrengthPower)] = "images/relics/CounterstrikeCharm.png",
            [typeof(AdamantSeedPower)] = "images/potions/AdamantSeed.png",
            [typeof(MightSeedPower)] = "images/potions/MightSeed.png",
        };

    private static readonly Type[] ConcretePowerTypes = typeof(HammerAbilityPower).Assembly
        .GetTypes()
        .Where(static type => !type.IsAbstract && typeof(PowerModel).IsAssignableFrom(type))
        .OrderBy(static type => type.Name, StringComparer.Ordinal)
        .ToArray();

    private static readonly Type[] RegisteredPowerTypes = typeof(HammerAbilityPower).Assembly
        .GetTypes()
        .Where(static type => !type.IsAbstract && type.GetCustomAttributesData().Any(
            static attribute => attribute.AttributeType.Name == "RegisterPowerAttribute"))
        .OrderBy(static type => type.Name, StringComparer.Ordinal)
        .ToArray();

    [Fact]
    public void EveryRegisteredPowerUsesItsPackagedAssignedIcon()
    {
        Assert.Equal(33, RegisteredPowerTypes.Length);
        Assert.Equal(ConcretePowerTypes, RegisteredPowerTypes);
        Assert.Equal(RegisteredPowerTypes, AssignedIconPaths.Keys.OrderBy(
            static type => type.Name,
            StringComparer.Ordinal));
        Assert.Equal(33, AssignedIconPaths.Count);
        Assert.Equal(28, AssignedIconPaths.Values.Distinct().Count());
        Assert.Equal(
            AssignedIconPaths[typeof(ChallengerPower)],
            AssignedIconPaths[typeof(ChallengerStrengthPower)]);
        Assert.NotEqual(
            AssignedIconPaths[typeof(ChargeSwitchStrengthPower)],
            AssignedIconPaths[typeof(ChargeSwitchCouragePower)]);

        var repositoryRoot = FindRepositoryRoot();
        Assert.False(File.Exists(Path.Combine(
            repositoryRoot,
            "HammerMod",
            "images",
            "powers",
            "hammer_power.svg")));

        foreach (var powerType in RegisteredPowerTypes)
        {
            var relativePath = AssignedIconPaths[powerType];
            var expectedPath = $"{Entry.ResPath}/{relativePath}";
            Assert.DoesNotContain("hammer_power.svg", expectedPath);
            Assert.True(
                File.Exists(Path.Combine(
                    repositoryRoot,
                    "HammerMod",
                    relativePath)),
                $"{powerType.Name} expects missing icon {relativePath}.");

            var power = Assert.IsAssignableFrom<PowerModel>(
                Activator.CreateInstance(powerType));
            var assetOverrides = Assert.IsAssignableFrom<IModPowerAssetOverrides>(power);
            Assert.Equal(expectedPath, assetOverrides.AssetProfile.IconPath);
            Assert.Equal(expectedPath, assetOverrides.AssetProfile.BigIconPath);
            Assert.Equal(expectedPath, assetOverrides.CustomIconPath);
            Assert.Equal(expectedPath, assetOverrides.CustomBigIconPath);
        }
    }

    private static string FindRepositoryRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory);
             directory is not null;
             directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "HammerMod.csproj")))
                return directory.FullName;
        }

        throw new DirectoryNotFoundException(
            $"Could not find HammerMod.csproj above {AppContext.BaseDirectory}.");
    }
}
