using System.Buffers.Binary;
using HammerMod.Potions;
using HammerMod.Relics;

namespace HammerMod.Tests.Items;

public sealed class ItemAssetContractTests
{
    private static readonly Type[] RelicTypes =
    [
        typeof(HammerTechniqueCharm),
        typeof(MasterHammerTechniqueCharm),
        typeof(FrostcraftCharm),
        typeof(SlidingBoostJewel),
        typeof(CounterstrikeCharm),
        typeof(DownedPursuitCharm),
        typeof(WirebugCage),
        typeof(EvasionMantle),
        typeof(RocksteadyMantle),
    ];

    private static readonly Type[] PotionTypes =
    [
        typeof(FlashBomb),
        typeof(DashJuiceG),
        typeof(AdamantSeed),
        typeof(MightSeed),
        typeof(Pitfall),
    ];

    [Fact]
    public void EveryHammerRelicUsesItsPackagedClassNamedPng()
    {
        Assert.Equal(9, RelicTypes.Length);
        foreach (var relicType in RelicTypes)
        {
            var fileName = $"{relicType.Name}.png";
            var expectedPath = $"{Entry.ResPath}/images/relics/{fileName}";
            var filePath = Path.Combine(
                FindRepositoryRoot(),
                "HammerMod",
                "images",
                "relics",
                fileName);

            var relic = Assert.IsAssignableFrom<HammerRelic>(
                Activator.CreateInstance(relicType));
            Assert.Equal(expectedPath, relic.AssetProfile.IconPath);
            Assert.Equal(expectedPath, relic.AssetProfile.IconOutlinePath);
            Assert.Equal(expectedPath, relic.AssetProfile.BigIconPath);
            AssertRgbaPng(filePath);
        }
    }

    [Fact]
    public void EveryHammerPotionUsesItsPackagedClassNamedPng()
    {
        Assert.Equal(5, PotionTypes.Length);
        foreach (var potionType in PotionTypes)
        {
            var fileName = $"{potionType.Name}.png";
            var expectedPath = $"{Entry.ResPath}/images/potions/{fileName}";
            var filePath = Path.Combine(
                FindRepositoryRoot(),
                "HammerMod",
                "images",
                "potions",
                fileName);

            var potion = Assert.IsAssignableFrom<HammerPotion>(
                Activator.CreateInstance(potionType));
            Assert.Equal(expectedPath, potion.AssetProfile.ImagePath);
            Assert.Equal(expectedPath, potion.AssetProfile.OutlinePath);
            AssertRgbaPng(filePath);
        }
    }

    [Fact]
    public void UnusedSpriteCellsHaveNoRegisteredItemModels()
    {
        var modelNames = typeof(HammerRelic).Assembly
            .GetTypes()
            .Select(static type => type.Name)
            .ToHashSet(StringComparer.Ordinal);

        Assert.DoesNotContain("AncientMaxPotion", modelNames);
        Assert.DoesNotContain("ParalysisTrap", modelNames);

        var potionImageDirectory = Path.Combine(
            FindRepositoryRoot(),
            "HammerMod",
            "images",
            "potions");
        Assert.False(File.Exists(Path.Combine(
            potionImageDirectory,
            "AncientMaxPotion.png")));
        Assert.False(File.Exists(Path.Combine(
            potionImageDirectory,
            "ParalysisTrap.png")));
    }

    private static void AssertRgbaPng(string path)
    {
        Assert.True(File.Exists(path), $"Missing packaged item icon {path}.");
        var bytes = File.ReadAllBytes(path);
        ReadOnlySpan<byte> pngSignature =
        [
            0x89, 0x50, 0x4e, 0x47, 0x0d, 0x0a, 0x1a, 0x0a
        ];
        Assert.True(bytes.Length >= 29);
        Assert.True(bytes.AsSpan(0, pngSignature.Length).SequenceEqual(pngSignature));
        Assert.Equal(256u, BinaryPrimitives.ReadUInt32BigEndian(bytes.AsSpan(16, 4)));
        Assert.Equal(256u, BinaryPrimitives.ReadUInt32BigEndian(bytes.AsSpan(20, 4)));
        Assert.Equal(8, bytes[24]);
        Assert.Equal(6, bytes[25]);
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
