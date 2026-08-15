using MegaCrit.Sts2.Core.Entities.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace HammerMod.Powers;

[RegisterPower]
public sealed class HammerStunResistancePower : ModPowerTemplate
{
    public override PowerAssetProfile AssetProfile => HammerPowerAssets.Profile;
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override bool ShouldPlayVfx => false;
    public override bool ShouldScaleInMultiplayer => false;

    protected override bool IsVisibleInternal => false;
}
