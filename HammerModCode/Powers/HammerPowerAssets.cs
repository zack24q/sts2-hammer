using MegaCrit.Sts2.Core.Entities.Powers;
using STS2RitsuLib.Scaffolding.Content;

namespace HammerMod.Powers;

internal static class HammerPowerAssets
{
    public static PowerAssetProfile Profile => new(
        IconPath: $"{Entry.ResPath}/images/powers/hammer_power.svg",
        BigIconPath: $"{Entry.ResPath}/images/powers/hammer_power.svg");
}
