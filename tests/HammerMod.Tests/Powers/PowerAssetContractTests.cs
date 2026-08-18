using HammerMod.Powers;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Scaffolding.Content.Patches;

namespace HammerMod.Tests.Powers;

public sealed class PowerAssetContractTests
{
    private const string SharedIconPath =
        $"{Entry.ResPath}/images/powers/hammer_power.svg";

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
    public void EveryRegisteredPowerUsesThePackagedSharedIcon()
    {
        Assert.Equal(32, RegisteredPowerTypes.Length);
        Assert.Equal(ConcretePowerTypes, RegisteredPowerTypes);
        Assert.True(File.Exists(Path.Combine(
            FindRepositoryRoot(),
            "HammerMod",
            "images",
            "powers",
            "hammer_power.svg")));

        foreach (var powerType in RegisteredPowerTypes)
        {
            var power = Assert.IsAssignableFrom<PowerModel>(
                Activator.CreateInstance(powerType));
            var assetOverrides = Assert.IsAssignableFrom<IModPowerAssetOverrides>(power);
            Assert.Equal(SharedIconPath, assetOverrides.AssetProfile.IconPath);
            Assert.Equal(SharedIconPath, assetOverrides.AssetProfile.BigIconPath);
            Assert.Equal(SharedIconPath, assetOverrides.CustomIconPath);
            Assert.Equal(SharedIconPath, assetOverrides.CustomBigIconPath);
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
