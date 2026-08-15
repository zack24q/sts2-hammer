using Godot;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Utils;

namespace HammerMod.Characters;

public sealed class HammerModCardPool : TypeListCardPoolModel
{
    private static readonly Material? PoolFrameTintMaterial =
        MaterialUtils.CreateReplaceHueShaderMaterial(
            HammerModCharacter.ThemeColor.R,
            HammerModCharacter.ThemeColor.G,
            HammerModCharacter.ThemeColor.B,
            1.3f);

    // Title 和 EnergyColorName 是池子的稳定标识，不是玩家看到的角色名。
    // 自定义角色卡、遗物、药水池保持同一个 EnergyColorName，方便实验室和文本统一读取能量图标。
    public override string Title => "HammerMod";
    public override string EnergyColorName => "HammerMod";

    // 这里指定卡牌文本和大图使用的能量图标路径。
    // res://HammerMod/... 里的 HammerMod 是 PCK 资源目录，不是 C# namespace。
    public override string? BigEnergyIconPath => $"{Entry.ResPath}/images/characters/energy_big.png";
    public override string? TextEnergyIconPath => $"{Entry.ResPath}/images/characters/energy_text.png";
    public override string CardFrameMaterialPath => "card_frame_pink";
    public override Material? PoolFrameMaterial => PoolFrameTintMaterial;

    public override Color DeckEntryCardColor => HammerModCharacter.ThemeColor;
    public override Color EnergyOutlineColor => HammerModCharacter.ThemeOutlineColor;

    // false 表示这是角色专属卡池，不是事件/状态那类无色卡池。
    public override bool IsColorless => false;
}
