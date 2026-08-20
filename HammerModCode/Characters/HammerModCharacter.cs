using Godot;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Nodes.Combat;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Characters;
using STS2RitsuLib.Scaffolding.Godot;
using STS2RitsuLib.Scaffolding.Visuals.Definition;

namespace HammerMod.Characters;

[RegisterCharacter]
public sealed class HammerModCharacter : ModCharacterTemplate<HammerModCardPool, HammerModRelicPool, HammerModPotionPool>
{
    public static readonly Color ThemeColor = new("FFAAC8");
    public static readonly Color ThemeOutlineColor = new("7E3150");

    private const string SceneRoot = $"{Entry.ResPath}/scenes/characters";
    private const string CharacterScenePath = $"{SceneRoot}/HammerMod_character.tscn";
    private const string EnergyCounterScenePath = $"{SceneRoot}/HammerMod_energy_counter.tscn";
    private const string MerchantScenePath = $"{SceneRoot}/HammerMod_merchant.tscn";
    private const string RestSiteScenePath = $"{SceneRoot}/HammerMod_rest_site.tscn";
    private const string CharacterSelectBgScenePath = $"{SceneRoot}/HammerMod_character_select_bg.tscn";
    private const string HammerAvatarPath = $"{Entry.ResPath}/images/powers/hammer_power.svg";
    private const string CharacterIconTexturePath =
        $"{Entry.ResPath}/images/characters/HammerMod_character_icon.png";
    private const string CharacterIconOutlineTexturePath =
        $"{Entry.ResPath}/images/characters/HammerMod_character_icon_outline.png";
    private const string MapMarkerTexturePath =
        $"{Entry.ResPath}/images/characters/HammerMod_map_marker.png";
    private const string CombatIdleTexturePath =
        $"{Entry.ResPath}/images/characters/HammerMod_character_combat_idle.png";
    private const string CombatDefeatedTexturePath =
        $"{Entry.ResPath}/images/characters/HammerMod_character_defeated.png";

    private static readonly VisualCueSet CombatVisualCues = VisualCueSetBuilder.Create()
        .Single("die", CombatDefeatedTexturePath)
        .Single("revive", CombatIdleTexturePath)
        .Build();

    public override Color NameColor => ThemeColor;
    public override Color EnergyLabelOutlineColor => ThemeOutlineColor;
    public override Color MapDrawingColor => ThemeColor;

    public override CharacterGender Gender => CharacterGender.Neutral;

    public override int StartingHp => 80;
    public override int StartingGold => 99;

    public override CharacterAssetProfile AssetProfile => new(
        Scenes: new CharacterSceneAssetSet(
            VisualsPath: CharacterScenePath,
            EnergyCounterPath: EnergyCounterScenePath,
            MerchantAnimPath: MerchantScenePath,
            RestSiteAnimPath: RestSiteScenePath),
        Ui: new CharacterUiAssetSet(
            IconTexturePath: CharacterIconTexturePath,
            IconOutlineTexturePath: CharacterIconOutlineTexturePath,
            CharacterSelectBgPath: CharacterSelectBgScenePath,
            CharacterSelectIconPath: HammerAvatarPath,
            CharacterSelectLockedIconPath: HammerAvatarPath,
            MapMarkerPath: MapMarkerTexturePath));

    public override string? PlaceholderCharacterId => "ironclad";
    public override string? CustomCastSfx => "event:/sfx/characters/ironclad/ironclad_cast";
    // The demo skips story/epoch scaffolding so the gameplay loop can be tested first.
    public override bool RequiresEpochAndTimeline => false;
    public override float AttackAnimDelay => 0f;
    public override float CastAnimDelay => 0f;
    public override VisualCueSet? VisualCues => CombatVisualCues;

    protected override NCreatureVisuals? TryCreateCreatureVisuals()
    {
        return RitsuGodotNodeFactories.CreateFromScenePath<NCreatureVisuals>(
            CharacterScenePath);
    }

    public override List<string> GetArchitectAttackVfx()
    {
        return
        [
            "vfx/vfx_attack_blunt",
            "vfx/vfx_heavy_blunt",
            "vfx/vfx_attack_slash",
            "vfx/vfx_bloody_impact",
            "vfx/vfx_rock_shatter"
        ];
    }
}
