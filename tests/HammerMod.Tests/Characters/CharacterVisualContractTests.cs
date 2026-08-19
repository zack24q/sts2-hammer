using HammerMod.Characters;

namespace HammerMod.Tests.Characters;

public sealed class CharacterVisualContractTests
{
    private const string CombatIdleTexturePath =
        $"{Entry.ResPath}/images/characters/HammerMod_character_idle.png";
    private const string CombatDefeatedTexturePath =
        $"{Entry.ResPath}/images/characters/HammerMod_character_defeated.png";

    [Fact]
    public void CombatVisualCuesSwitchBetweenDefeatedAndIdlePortraits()
    {
        var cues = new HammerModCharacter().VisualCues;

        Assert.NotNull(cues);
        var textures = Assert.IsAssignableFrom<IReadOnlyDictionary<string, string>>(
            cues.TexturePathByCue);
        Assert.Equal(CombatDefeatedTexturePath, textures["die"]);
        Assert.Equal(CombatIdleTexturePath, textures["revive"]);
    }

    [Theory]
    [InlineData("HammerMod_character_idle.png")]
    [InlineData("HammerMod_character_defeated.png")]
    [InlineData("HammerMod_character_select.png")]
    [InlineData("HammerMod_character_select_locked.png")]
    public void CharacterPortraitAssetsArePackaged(string fileName)
    {
        Assert.True(File.Exists(Path.Combine(
            FindRepositoryRoot(),
            "HammerMod",
            "images",
            "characters",
            fileName)));
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
