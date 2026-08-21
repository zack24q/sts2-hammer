using MegaCrit.Sts2.Core.Entities.Powers;
using STS2RitsuLib.Scaffolding.Content;

namespace HammerMod.Powers;

internal static class HammerPowerAssets
{
    private static readonly IReadOnlyDictionary<Type, string> IconPaths =
        new Dictionary<Type, string>
        {
            [typeof(HammerStunPower)] = "images/powers/HammerStun.png",
            [typeof(HammerStunResistancePower)] = "images/powers/HammerStun.png",
            [typeof(AftershockPower)] = "images/powers/Aftershock.png",
            [typeof(FocusPower)] = "images/powers/Focus.png",
            [typeof(EndlessMomentumPower)] = "images/powers/EndlessMomentum.png",
            [typeof(DashJuicePower)] = "images/powers/DashJuice.png",
            [typeof(FelyneKoTechniquePower)] = "images/powers/FelyneKoTechnique.png",
            [typeof(PileDriverPower)] = "images/powers/PileDriver.png",
            [typeof(ConcussionResonancePower)] = "images/powers/ConcussionResonance.png",
            [typeof(ImpactBurstPower)] = "images/powers/ImpactBurst.png",
            [typeof(CounterFormPower)] = "images/powers/CounterForm.png",
            [typeof(WeaveAndBonkPower)] = "images/powers/CounterForm.png",
            [typeof(ChallengerPower)] = "images/powers/Challenger.png",
            [typeof(ChallengerStrengthPower)] = "images/powers/Challenger.png",
            [typeof(WirebugContinuationPower)] = "images/powers/WirebugContinuation.png",
            [typeof(WeaknessExploitPower)] = "images/powers/WeaknessExploit.png",
            [typeof(HarderWithEverySmashPower)] = "images/powers/HarderWithEverySmash.png",
            [typeof(PartbreakerPower)] = "images/powers/Partbreaker.png",
            [typeof(OneMoreBonkPower)] = "images/powers/OneMoreBonk.png",
            [typeof(BloodRitePower)] = "images/powers/BloodRite.png",
            [typeof(ChargeSwitchStrengthPower)] = "images/powers/ChargeSwitchStrength.png",
            [typeof(AffinitySlidingStrengthPower)] = "images/powers/ChargeSwitchStrength.png",
            [typeof(LuckyVoucherPower)] = "images/powers/LuckyVoucher.png",
            [typeof(OverchargePower)] = "images/powers/Overcharge.png",
            [typeof(ValorStylePower)] = "images/powers/ValorStyle.png",
            [typeof(FaceOffPower)] = "images/powers/FaceOff.png",
            [typeof(UnloadingStancePower)] = "images/powers/FaceOff.png",
            [typeof(WirefallPower)] = "images/powers/Wirefall.png",
            [typeof(FarcasterPower)] = "images/powers/Farcaster.png",
            [typeof(FreeMealPower)] = "images/powers/FreeMeal.png",
            [typeof(ChargeSwitchCouragePower)] = "images/powers/ChargeSwitchCourage.png",
            [typeof(CounterstrikeStrengthPower)] = "images/relics/CounterstrikeCharm.png",
            [typeof(AdamantSeedPower)] = "images/potions/AdamantSeed.png",
            [typeof(MightSeedPower)] = "images/potions/MightSeed.png",
        };

    public static PowerAssetProfile ProfileFor(Type powerType)
    {
        ArgumentNullException.ThrowIfNull(powerType);
        if (!IconPaths.TryGetValue(powerType, out var relativePath))
            throw new ArgumentException(
                $"No icon is assigned to registered power {powerType.FullName}.",
                nameof(powerType));

        var path = $"{Entry.ResPath}/{relativePath}";
        return new PowerAssetProfile(IconPath: path, BigIconPath: path);
    }
}
