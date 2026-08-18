#!/usr/bin/env python3
"""Generate deterministic numbered portraits for every HammerMod card."""

from pathlib import Path

from PIL import Image, ImageDraw, ImageFont


WIDTH = 1000
HEIGHT = 760
CARDS = [
    "OverheadSmash",
    "Roll",
    "Charge",
    "EarthStrike",
    "ChargedOverheadSmash",
    "ChargedSideSmash",
    "MightyChargeBonk",
    "SilkbindSpinningBludgeon",
    "MightyChargeRoll",
    "ReadyToCharge",
    "KeepingSway",
    "SteadierWithEverySpin",
    "Overcharge",
    "Focus",
    "EndlessMomentum",
    "BraceWithTheHammer",
    "EmergencyEvade",
    "SwingAtEveryOpening",
    "DashJuice",
    "Upswing",
    "ChargedUpswing",
    "GroundShock",
    "FocusBlowEarthquake",
    "HomeRunSwing",
    "BigBangCombo",
    "FlashHammer",
    "HeadOverHeels",
    "ConcussionGuard",
    "StunTechnique",
    "PileDriver",
    "SmashThatHead",
    "Aftershock",
    "ConcussionResonance",
    "ImpactBurst",
    "FaceOff",
    "WaterStrike",
    "PredictiveFootwork",
    "DeepBreath",
    "UnloadingStance",
    "WeaveAndBonk",
    "ChargeStep",
    "HammerForHammer",
    "BreakMomentum",
    "CounterForm",
    "WakeUpHit",
    "ShellBreaker",
    "ToolSpecialist",
    "WarmUpExercise",
    "ImpactCrater",
    "Reposition",
    "LaunchTeammate",
    "DemonPowder",
    "HardshellPowder",
    "BackOnYourFeet",
    "MightyUpswing",
    "LeveragedSwing",
    "HammerIai",
    "VictoryCharge",
    "SwitchGripSwing",
    "QuickCraft",
    "LegSweepHammer",
    "EarthsplitterShock",
    "ContinuousSideSwing",
    "WirebugSpin",
    "AffinitySliding",
    "SweepThePath",
    "InvincibleWindFireWheel",
    "TrueSpinningImpact",
    "Challenger",
    "WirebugContinuation",
    "WeaknessExploit",
    "HarderWithEverySmash",
    "Partbreaker",
    "OneMoreBonk",
    "BloodRite",
    "HandCrankedTractor",
    "ChargeSwitchStrength",
    "RecoveryMedicine",
    "BluntWeaponExpert",
    "ChargeAsYouStrike",
    "StaminaDrainingHammer",
    "FindASlope",
    "Wirefall",
    "Farcaster",
    "Coalescence",
    "FreeMeal",
    "LuckyVoucher",
    "ChargeSwitchCourage",
]


def find_font() -> str:
    candidates = [
        "/System/Library/Fonts/Supplemental/Arial Bold.ttf",
        "/usr/share/fonts/truetype/dejavu/DejaVuSans-Bold.ttf",
        "C:/Windows/Fonts/arialbd.ttf",
    ]
    for candidate in candidates:
        if Path(candidate).is_file():
            return candidate
    raise FileNotFoundError("No supported bold font was found.")


def accent_for(number: int) -> str:
    if number <= 4:
        return "#d9a441"
    if number <= 19 or number in {57, 58, 76}:
        return "#dc7544"
    if number <= 34 or number in {55, 62}:
        return "#d6ba4c"
    if number <= 44 or number in {56, 61}:
        return "#4db6ad"
    if number <= 48 or number in {50, 54, 59, 60}:
        return "#a7adb5"
    if number == 49:
        return "#b7a2d8"
    if 63 <= number <= 68 or number in {86, 87}:
        return "#d6ba4c"
    return "#5e9fd6"


def render(output_path: Path, number: int, font: ImageFont.FreeTypeFont) -> None:
    accent = accent_for(number)
    image = Image.new("RGB", (WIDTH, HEIGHT), "#15191e")
    draw = ImageDraw.Draw(image)

    for offset in range(-HEIGHT, WIDTH, 150):
        draw.polygon(
            [
                (offset, HEIGHT),
                (offset + 90, HEIGHT),
                (offset + HEIGHT + 90, 0),
                (offset + HEIGHT, 0),
            ],
            fill="#1d2329",
        )

    draw.rounded_rectangle(
        (30, 30, WIDTH - 30, HEIGHT - 30),
        radius=26,
        outline=accent,
        width=14,
    )
    draw.line((80, 94, WIDTH - 80, 94), fill=accent, width=6)
    draw.line((80, HEIGHT - 94, WIDTH - 80, HEIGHT - 94), fill=accent, width=6)

    label = f"{number:02d}"
    bounds = draw.textbbox((0, 0), label, font=font, stroke_width=3)
    text_width = bounds[2] - bounds[0]
    text_height = bounds[3] - bounds[1]
    x = (WIDTH - text_width) / 2
    y = (HEIGHT - text_height) / 2 - bounds[1]
    draw.text(
        (x + 12, y + 14),
        label,
        font=font,
        fill="#050607",
        stroke_width=3,
        stroke_fill="#050607",
    )
    draw.text(
        (x, y),
        label,
        font=font,
        fill="#f4f1e8",
        stroke_width=3,
        stroke_fill=accent,
    )

    image.save(output_path, format="PNG", optimize=True)


def main() -> None:
    if len(CARDS) != 88 or len(set(CARDS)) != len(CARDS):
        raise ValueError("Card list must contain exactly 88 unique entries.")

    project_root = Path(__file__).resolve().parents[1]
    output_dir = project_root / "HammerMod" / "images" / "cards" / "placeholders"
    output_dir.mkdir(parents=True, exist_ok=True)
    font = ImageFont.truetype(find_font(), 390)

    for number, card_name in enumerate(CARDS, start=1):
        render(output_dir / f"{card_name}.png", number, font)

    print(f"Generated {len(CARDS)} card portraits in {output_dir}")


if __name__ == "__main__":
    main()
