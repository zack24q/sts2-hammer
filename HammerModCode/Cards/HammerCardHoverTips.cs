using HammerMod.Gameplay;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HammerMod.Cards;

[Flags]
internal enum HammerCardMechanic
{
    None = 0,
    Charge = 1 << 0,
    Stun = 1 << 1,
    Stunned = 1 << 2,
    ChargeRelease = 1 << 3,
    Strength = 1 << 4,
    Dexterity = 1 << 5,
    Weak = 1 << 6,
    Vulnerable = 1 << 7,
    Regeneration = 1 << 8,
    Thorns = 1 << 9,
    Replay = 1 << 10,
    Block = 1 << 11,
    BackOnYourFeet = 1 << 12,
    Frail = 1 << 13
}

internal static class HammerCardHoverTips
{
    internal static StaticHoverTip StunHoverTip => StaticHoverTip.Stun;

    internal static IEnumerable<IHoverTip> Create(HammerCard card)
    {
        return Create(GetMechanics(card));
    }

    internal static IEnumerable<IHoverTip> Create(HammerCardMechanic mechanics)
    {
        if (mechanics.HasFlag(HammerCardMechanic.Charge))
            yield return HoverTipFactory.FromKeyword(HammerKeywords.Charge);
        if (mechanics.HasFlag(HammerCardMechanic.Stun)
            || mechanics.HasFlag(HammerCardMechanic.Stunned))
            yield return HoverTipFactory.Static(StunHoverTip);
        if (mechanics.HasFlag(HammerCardMechanic.ChargeRelease))
            yield return HoverTipFactory.FromKeyword(HammerKeywords.ChargeRelease);
        if (mechanics.HasFlag(HammerCardMechanic.Strength))
            yield return HoverTipFactory.FromPower<StrengthPower>();
        if (mechanics.HasFlag(HammerCardMechanic.Dexterity))
            yield return HoverTipFactory.FromPower<DexterityPower>();
        if (mechanics.HasFlag(HammerCardMechanic.Weak))
            yield return HoverTipFactory.FromPower<WeakPower>();
        if (mechanics.HasFlag(HammerCardMechanic.Vulnerable))
            yield return HoverTipFactory.FromPower<VulnerablePower>();
        if (mechanics.HasFlag(HammerCardMechanic.Frail))
            yield return HoverTipFactory.FromPower<FrailPower>();
        if (mechanics.HasFlag(HammerCardMechanic.Regeneration))
            yield return HoverTipFactory.FromPower<RegenPower>();
        if (mechanics.HasFlag(HammerCardMechanic.Thorns))
            yield return HoverTipFactory.FromPower<ThornsPower>();
        if (mechanics.HasFlag(HammerCardMechanic.Replay))
            yield return HoverTipFactory.Static(StaticHoverTip.ReplayStatic);
        if (mechanics.HasFlag(HammerCardMechanic.Block))
            yield return HoverTipFactory.Static(StaticHoverTip.Block);
        if (mechanics.HasFlag(HammerCardMechanic.BackOnYourFeet))
        {
            foreach (var tip in HoverTipFactory.FromCardWithCardHoverTips<BackOnYourFeet>())
                yield return tip;
        }
    }

    internal static HammerCardMechanic GetMechanics(HammerCard card)
    {
        return card switch
        {
            EarthStrike or ChargedUpswing or ImpactCrater =>
                HammerCardMechanic.Charge | HammerCardMechanic.Stun,
            BluntWeaponExpert => HammerCardMechanic.Charge | HammerCardMechanic.Stun,

            ChargedOverheadSmash or ChargedSideSmash or MightyChargeBonk
                or SilkbindSpinningBludgeon or MightyChargeRoll or ReadyToCharge
                or KeepingSway or SteadierWithEverySpin or HammerIai or Focus
                or BraceWithTheHammer or EmergencyEvade or SwingAtEveryOpening or ChargeStep
                or WirebugContinuation or ChargeAsYouStrike or ChargeSwitchCourage =>
                HammerCardMechanic.Charge,

            DashJuice => HammerCardMechanic.Charge | HammerCardMechanic.Block,
            AffinitySliding => HammerCardMechanic.Charge | HammerCardMechanic.Strength,
            ChargeSwitchStrength => HammerCardMechanic.Charge | HammerCardMechanic.Strength,

            Overcharge => HammerCardMechanic.Charge
                | HammerCardMechanic.ChargeRelease,
            EndlessMomentum => HammerCardMechanic.Charge
                | HammerCardMechanic.ChargeRelease,
            HarderWithEverySmash => HammerCardMechanic.Charge
                | HammerCardMechanic.ChargeRelease
                | HammerCardMechanic.Strength,
            HandCrankedTractor => HammerCardMechanic.Charge
                | HammerCardMechanic.ChargeRelease
                | HammerCardMechanic.Replay,

            Upswing or MightyUpswing or GroundShock or EarthsplitterShock
                or FlashHammer or HeadOverHeels or ConcussionGuard or FelyneKoTechnique
                or SmashThatHead or Aftershock or ImpactBurst or InvincibleWindFireWheel
                or TrueSpinningImpact => HammerCardMechanic.Stun,
            VictoryCharge or HomeRunSwing or BigBangCombo => HammerCardMechanic.Stunned,
            PileDriver => HammerCardMechanic.Stun | HammerCardMechanic.Stunned,
            FocusBlowEarthquake => HammerCardMechanic.Stun
                | HammerCardMechanic.Vulnerable,
            StaminaDrainingHammer => HammerCardMechanic.Stun
                | HammerCardMechanic.Weak
                | HammerCardMechanic.Vulnerable,
            ConcussionResonance => HammerCardMechanic.Stun
                | HammerCardMechanic.Weak
                | HammerCardMechanic.Vulnerable,

            FaceOff or UnloadingStance or BreakMomentum or Challenger
                or DemonPowder => HammerCardMechanic.Strength,
            WarmUpExercise => HammerCardMechanic.Strength | HammerCardMechanic.Dexterity,
            LaunchTeammate => HammerCardMechanic.Strength | HammerCardMechanic.BackOnYourFeet,
            HardshellPowder => HammerCardMechanic.Dexterity,
            PredictiveFootwork => HammerCardMechanic.Weak | HammerCardMechanic.Vulnerable,
            LegSweepHammer => HammerCardMechanic.Weak,
            WeaknessExploit or Partbreaker => HammerCardMechanic.Vulnerable,
            RecoveryMedicine => HammerCardMechanic.Regeneration,
            Coalescence => HammerCardMechanic.Weak
                | HammerCardMechanic.Vulnerable
                | HammerCardMechanic.Frail
                | HammerCardMechanic.Strength,
            WeaveAndBonk => HammerCardMechanic.Thorns,
            CounterForm or ShellBreaker => HammerCardMechanic.Block,
            _ => HammerCardMechanic.None
        };
    }
}
