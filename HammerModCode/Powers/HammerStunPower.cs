using HammerMod.Gameplay;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace HammerMod.Powers;

[RegisterPower]
public sealed class HammerStunPower : ModPowerTemplate
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IntVar("Threshold", 10)
    ];

    public override PowerType Type => PowerType.None;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override bool ShouldScaleInMultiplayer => false;

    public override PowerAssetProfile AssetProfile => HammerPowerAssets.Profile;

    public override Task BeforeApplied(
        Creature target,
        decimal amount,
        Creature? applier,
        CardModel? cardSource)
    {
        SetThreshold(HammerStun.GetThreshold(target));
        return Task.CompletedTask;
    }

    internal void SetThreshold(int threshold)
    {
        var thresholdVar = DynamicVars["Threshold"];
        thresholdVar.UpgradeValueBy(Math.Max(0, threshold) - thresholdVar.BaseValue);
        thresholdVar.FinalizeUpgrade();
    }
}
