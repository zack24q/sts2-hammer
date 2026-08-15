using Godot;
using STS2RitsuLib.Scaffolding.Content;

namespace HammerMod.Characters;

public sealed class HammerModPotionPool : TypeListPotionPoolModel
{
    public override string EnergyColorName => "HammerMod";
    public override Color LabOutlineColor => HammerModCharacter.ThemeColor;

    // 药水池与角色共用锤手能量主题；每瓶药水在 Potions/ 下单独注册。
    public override string? BigEnergyIconPath => $"{Entry.ResPath}/images/characters/energy_big.png";
    public override string? TextEnergyIconPath => $"{Entry.ResPath}/images/characters/energy_text.png";
}
