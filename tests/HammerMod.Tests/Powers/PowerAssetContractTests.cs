using HammerMod.Powers;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Scaffolding.Content.Patches;

namespace HammerMod.Tests.Powers;

public sealed class PowerAssetContractTests
{
    private const string DefaultIconFileName = "hammer_power.svg";

    private static readonly IReadOnlyDictionary<Type, string>
        AssignedIconFileNames = new Dictionary<Type, string>
        {
            [typeof(HammerStunPower)] = "HammerStun.png",
            [typeof(AftershockPower)] = "Aftershock.png",
            [typeof(FocusPower)] = "Focus.png",
            [typeof(EndlessMomentumPower)] = "EndlessMomentum.png",
            [typeof(DashJuicePower)] = "DashJuice.png",
            [typeof(FelyneKoTechniquePower)] = "FelyneKoTechnique.png",
            [typeof(PileDriverPower)] = "PileDriver.png",
            [typeof(ConcussionResonancePower)] = "ConcussionResonance.png",
            [typeof(ImpactBurstPower)] = "ImpactBurst.png",
            [typeof(CounterFormPower)] = "CounterForm.png",
            [typeof(ChallengerPower)] = "Challenger.png",
            [typeof(ChallengerStrengthPower)] = "Challenger.png",
            [typeof(WirebugContinuationPower)] = "WirebugContinuation.png",
            [typeof(WeaknessExploitPower)] = "WeaknessExploit.png",
            [typeof(HarderWithEverySmashPower)] = "HarderWithEverySmash.png",
            [typeof(PartbreakerPower)] = "Partbreaker.png",
            [typeof(OneMoreBonkPower)] = "OneMoreBonk.png",
            [typeof(BloodRitePower)] = "BloodRite.png",
            [typeof(ChargeSwitchStrengthPower)] = "ChargeSwitchStrength.png",
            [typeof(LuckyVoucherPower)] = "LuckyVoucher.png",
            [typeof(OverchargePower)] = "Overcharge.png",
            [typeof(FaceOffPower)] = "FaceOff.png",
            [typeof(WirefallPower)] = "Wirefall.png",
            [typeof(FarcasterPower)] = "Farcaster.png",
            [typeof(FreeMealPower)] = "FreeMeal.png",
            [typeof(ChargeSwitchCouragePower)] = "ChargeSwitchCourage.png",
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
        Assert.Equal(26, AssignedIconFileNames.Count);
        Assert.Equal(25, AssignedIconFileNames.Values.Distinct().Count());
        Assert.Equal(
            AssignedIconFileNames[typeof(ChallengerPower)],
            AssignedIconFileNames[typeof(ChallengerStrengthPower)]);
        Assert.NotEqual(
            AssignedIconFileNames[typeof(ChargeSwitchStrengthPower)],
            AssignedIconFileNames[typeof(ChargeSwitchCouragePower)]);

        var repositoryRoot = FindRepositoryRoot();

        foreach (var powerType in RegisteredPowerTypes)
        {
            var fileName = AssignedIconFileNames.GetValueOrDefault(
                powerType,
                DefaultIconFileName);
            var expectedPath = $"{Entry.ResPath}/images/powers/{fileName}";
            Assert.True(
                File.Exists(Path.Combine(
                    repositoryRoot,
                    "HammerMod",
                    "images",
                    "powers",
                    fileName)),
                $"{powerType.Name} expects missing icon {fileName}.");

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
