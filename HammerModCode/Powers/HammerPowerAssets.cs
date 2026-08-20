using MegaCrit.Sts2.Core.Entities.Powers;
using STS2RitsuLib.Scaffolding.Content;

namespace HammerMod.Powers;

internal static class HammerPowerAssets
{
    private const string DefaultIconFileName = "hammer_power.svg";

    private static readonly IReadOnlyDictionary<Type, string> IconFileNames =
        new Dictionary<Type, string>
        {
            [typeof(HammerStunPower)] = "HammerStun.png",
            [typeof(AftershockPower)] = "Aftershock.png",
            [typeof(FocusPower)] = "Focus.png",
            [typeof(EndlessMomentumPower)] = "EndlessMomentum.png",
            [typeof(DashJuicePower)] = "DashJuice.png",
            [typeof(FelyneKoTechniquePower)] = "FelyneKoTechnique.png",
            [typeof(PileDriverPower)] = "PileDriver.png",
            [typeof(ConcussionResonancePower)] = "ConcussionResonance.png",
            [typeof(ImpactBurstPower)] = "ImpactBurst.png",
            [typeof(CounterFormPower)] = "CounterForm.png",
            [typeof(ChallengerPower)] = "Challenger.png",
            [typeof(ChallengerStrengthPower)] = "Challenger.png",
            [typeof(WirebugContinuationPower)] = "WirebugContinuation.png",
            [typeof(WeaknessExploitPower)] = "WeaknessExploit.png",
            [typeof(HarderWithEverySmashPower)] = "HarderWithEverySmash.png",
            [typeof(PartbreakerPower)] = "Partbreaker.png",
            [typeof(OneMoreBonkPower)] = "OneMoreBonk.png",
            [typeof(BloodRitePower)] = "BloodRite.png",
            [typeof(ChargeSwitchStrengthPower)] = "ChargeSwitchStrength.png",
            [typeof(LuckyVoucherPower)] = "LuckyVoucher.png",
            [typeof(OverchargePower)] = "Overcharge.png",
            [typeof(FaceOffPower)] = "FaceOff.png",
            [typeof(WirefallPower)] = "Wirefall.png",
            [typeof(FarcasterPower)] = "Farcaster.png",
            [typeof(FreeMealPower)] = "FreeMeal.png",
            [typeof(ChargeSwitchCouragePower)] = "ChargeSwitchCourage.png",
        };

    public static PowerAssetProfile ProfileFor(Type powerType)
    {
        ArgumentNullException.ThrowIfNull(powerType);
        var fileName = IconFileNames.GetValueOrDefault(
            powerType,
            DefaultIconFileName);
        var path = $"{Entry.ResPath}/images/powers/{fileName}";
        return new PowerAssetProfile(IconPath: path, BigIconPath: path);
    }
}
