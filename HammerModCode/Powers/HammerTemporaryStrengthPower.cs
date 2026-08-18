using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Scaffolding.Content.Patches;

namespace HammerMod.Powers;

public abstract class HammerTemporaryStrengthPower :
    TemporaryStrengthPower,
    IModPowerAssetOverrides
{
    public PowerAssetProfile AssetProfile => HammerPowerAssets.Profile;
    public string? CustomIconPath => AssetProfile.IconPath;
    public string? CustomBigIconPath => AssetProfile.BigIconPath;
}
