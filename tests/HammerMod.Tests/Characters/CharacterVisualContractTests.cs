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
    [InlineData("charge_counter.png")]
    [InlineData("charge_counter_glow.png")]
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
    public void EnergyCounterUsesHeartGemstoneWithoutLegacySpinner()
    {
        var scene = File.ReadAllText(Path.Combine(
            FindRepositoryRoot(),
            "HammerMod",
            "scenes",
            "characters",
            "HammerMod_energy_counter.tscn"));

        Assert.Contains("HammerMod_energy_orb_layer_1.png", scene);
        Assert.Contains("[node name=\"RotationLayers\" type=\"Control\"", scene);
        Assert.Contains("pivot_offset = Vector2(64, 64)", scene);
        Assert.DoesNotContain("HammerMod_energy_orb_layer_2.png", scene);
        Assert.DoesNotContain("[node name=\"Layer2\"", scene);
        Assert.DoesNotContain("HammerMod_energy_orb_layer_4.png", scene);
        Assert.DoesNotContain("HammerMod_energy_orb_layer_5.png", scene);
    }

    [Theory]
    [InlineData("HammerMod_energy_orb_layer_2.png")]
    [InlineData("HammerMod_energy_orb_layer_3.png")]
    [InlineData("HammerMod_energy_orb_layer_4.png")]
    [InlineData("HammerMod_energy_orb_layer_5.png")]
    public void RetiredEnergyLayersAreNotPackaged(string fileName)
    {
        Assert.False(File.Exists(Path.Combine(
            FindRepositoryRoot(),
            "HammerMod",
            "images",
            "characters",
            fileName)));
    }

    [Fact]
    public void ChargeCounterUsesHammerAndPlacesAmountBelowIt()
    {
        Assert.Equal("charge_counter.png", HammerResources.ChargeCounterIconFileName);
        Assert.Equal("charge_counter_glow.png", HammerResources.ChargeCounterGlowFileName);
        Assert.Equal(new Vector2(-36f, 40f), HammerResources.ChargeCounterPosition);
        Assert.Equal(new Vector2(0.8f, 0.8f), HammerResources.ChargeCounterScale);
        Assert.Equal(new Vector2(0f, 78f), HammerResources.ChargeAmountLabelOffset);
    }

    [Fact]
    public void ChargeCounterGlowColorTracksChargeLevel()
    {
        Assert.Null(HammerResources.GetChargeGlowColor(0));
        Assert.Equal(
            HammerResources.ChargeLevelOneGlowColor,
            HammerResources.GetChargeGlowColor(1));
        Assert.Equal(
            HammerResources.ChargeLevelTwoGlowColor,
            HammerResources.GetChargeGlowColor(2));
        Assert.Equal(
            HammerResources.ChargeLevelThreeGlowColor,
            HammerResources.GetChargeGlowColor(3));
    }

    [Fact]
    public void ChargeCounterGlowGrowsWithChargeLevel()
    {
        Assert.Equal(1f, HammerResources.GetChargeGlowScale(1));
        Assert.Equal(1.03f, HammerResources.GetChargeGlowScale(2));
        Assert.Equal(1.06f, HammerResources.GetChargeGlowScale(3));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void ChargeCounterGlowStaysCenteredAtEveryLevel(int charge)
    {
        var counterSize = new Vector2(128f, 128f);
        var iconSize = new Vector2(128f, 128f);
        var scale = HammerResources.GetChargeGlowScale(charge);
        var position = HammerResources.GetChargeGlowPosition(
            counterSize,
            iconSize,
            charge);
        var transformedCenter = position + iconSize * scale * 0.5f;

        Assert.Equal(counterSize.X * 0.5f, transformedCenter.X, 4);
        Assert.Equal(counterSize.Y * 0.5f, transformedCenter.Y, 4);
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
