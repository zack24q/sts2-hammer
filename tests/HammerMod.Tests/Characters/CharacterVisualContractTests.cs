using Godot;
using HammerMod.Characters;
using HammerMod.Gameplay;

namespace HammerMod.Tests.Characters;

public sealed class CharacterVisualContractTests
{
    private const string CombatIdleTexturePath =
        $"{Entry.ResPath}/images/characters/HammerMod_character_combat_idle.png";
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
    [InlineData("HammerMod_character_combat_idle.png")]
    [InlineData("HammerMod_character_idle.png")]
    [InlineData("HammerMod_character_defeated.png")]
    [InlineData("HammerMod_character_select.png")]
    [InlineData("HammerMod_character_select_locked.png")]
    [InlineData("HammerMod_character_select_background.png")]
    [InlineData("HammerMod_character_icon.png")]
    [InlineData("HammerMod_character_icon_outline.png")]
    [InlineData("HammerMod_map_marker.png")]
    [InlineData("HammerMod_energy_orb_layer_1.png")]
    [InlineData("HammerMod_energy_orb_layer_2.png")]
    [InlineData("charge_counter.png")]
    [InlineData("energy_big.png")]
    [InlineData("energy_text.png")]
    public void CharacterVisualAssetsArePackaged(string fileName)
    {
        Assert.True(File.Exists(Path.Combine(
            FindRepositoryRoot(),
            "HammerMod",
            "images",
            "characters",
            fileName)));
    }

    [Fact]
    public void RunMapUsesDedicatedArrowMarker()
    {
        Assert.Equal(
            $"{Entry.ResPath}/images/characters/HammerMod_map_marker.png",
            new HammerModCharacter().AssetProfile.Ui?.MapMarkerPath);
    }

    [Fact]
    public void TopPanelUsesDedicatedCharacterPortraitAndOutline()
    {
        var ui = new HammerModCharacter().AssetProfile.Ui;

        Assert.NotNull(ui);
        Assert.Equal(
            $"{Entry.ResPath}/images/characters/HammerMod_character_icon.png",
            ui.IconTexturePath);
        Assert.Equal(
            $"{Entry.ResPath}/images/characters/HammerMod_character_icon_outline.png",
            ui.IconOutlineTexturePath);
    }

    [Fact]
    public void EnergyCounterUsesCrystalAndNativeRotationLayer()
    {
        var scene = File.ReadAllText(Path.Combine(
            FindRepositoryRoot(),
            "HammerMod",
            "scenes",
            "characters",
            "HammerMod_energy_counter.tscn"));

        Assert.Contains("HammerMod_energy_orb_layer_1.png", scene);
        Assert.Contains("HammerMod_energy_orb_layer_2.png", scene);
        Assert.Contains(
            "[node name=\"Layer2\" type=\"TextureRect\" " +
            "parent=\"Layers/RotationLayers\"]",
            scene);
        Assert.Contains("pivot_offset = Vector2(64, 64)", scene);
        Assert.DoesNotContain("HammerMod_energy_orb_layer_4.png", scene);
        Assert.DoesNotContain("HammerMod_energy_orb_layer_5.png", scene);
    }

    [Fact]
    public void ChargeCounterUsesHammerIconAtRegentStyleEnergyOffset()
    {
        Assert.Equal("charge_counter.png", HammerResources.ChargeCounterIconFileName);
        Assert.Equal(new Vector2(-36f, 40f), HammerResources.ChargeCounterPosition);
        Assert.Equal(new Vector2(0.8f, 0.8f), HammerResources.ChargeCounterScale);
    }

    [Fact]
    public void CombatSceneUsesCombatIdlePortrait()
    {
        var scene = File.ReadAllText(Path.Combine(
            FindRepositoryRoot(),
            "HammerMod",
            "scenes",
            "characters",
            "HammerMod_character.tscn"));

        Assert.Contains("HammerMod_character_combat_idle.png", scene);
        Assert.DoesNotContain("HammerMod_character_idle.png", scene);
        Assert.DoesNotContain("HammerMod_character_defeated.png", scene);
    }

    [Theory]
    [InlineData("HammerMod_character_select_bg.tscn")]
    [InlineData("HammerMod_merchant.tscn")]
    [InlineData("HammerMod_rest_site.tscn")]
    public void NonCombatIdleScenesUseStandingPortrait(string sceneName)
    {
        var scene = File.ReadAllText(Path.Combine(
            FindRepositoryRoot(),
            "HammerMod",
            "scenes",
            "characters",
            sceneName));

        Assert.Contains("HammerMod_character_idle.png", scene);
        Assert.DoesNotContain("HammerMod_character_combat_idle.png", scene);
        Assert.DoesNotContain("HammerMod_character_select.png", scene);
        Assert.DoesNotContain("HammerMod_character_defeated.png", scene);
    }

    [Fact]
    public void CharacterSelectSceneUsesCompleteStandingArtWithoutStandaloneIcon()
    {
        var scene = File.ReadAllText(Path.Combine(
            FindRepositoryRoot(),
            "HammerMod",
            "scenes",
            "characters",
            "HammerMod_character_select_bg.tscn"));

        Assert.DoesNotContain("hammer_power.svg", scene);
        Assert.DoesNotContain("[node name=\"Icon\"", scene);
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
