using MegaCrit.Sts2.Core.Entities.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace HammerMod.Powers;

[RegisterPower]
public sealed class HammerStunPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.None;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override bool ShouldScaleInMultiplayer => false;

    public override PowerAssetProfile AssetProfile => HammerPowerAssets.Profile;
}
