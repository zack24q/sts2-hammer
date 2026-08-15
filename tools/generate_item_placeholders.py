#!/usr/bin/env python3
"""Generate deterministic numbered placeholders for HammerMod relics and potions."""

from pathlib import Path

from PIL import Image, ImageDraw, ImageFont


SIZE = 256
RELICS = [
    "HammerTechniqueCharm",
    "KoCharm",
    "SlidingBoostJewel",
    "CounterstrikeCharm",
    "WirebugCage",
    "DownedPursuitCharm",
    "AdrenalineCharm",  # Reused by EvasionMantle until final relic art replaces placeholder 07.
    "RocksteadyMantle",
]
POTIONS = [
    "DashJuiceG",
    "FlashBomb",
    "AdamantSeed",
    "MightSeed",
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


def render_numbered_badge(
    output_path: Path,
    number: int,
    font: ImageFont.FreeTypeFont,
    accent: str,
    shape: str,
) -> None:
    image = Image.new("RGBA", (SIZE, SIZE), (0, 0, 0, 0))
    draw = ImageDraw.Draw(image)

    if shape == "relic":
        draw.ellipse((18, 18, 238, 238), fill="#171b20", outline="#080a0c", width=12)
        draw.ellipse((31, 31, 225, 225), outline=accent, width=11)
        draw.arc((44, 44, 212, 212), 205, 335, fill="#f0d7a0", width=5)
    else:
        draw.rounded_rectangle((54, 20, 202, 64), radius=17, fill="#171b20", outline=accent, width=8)
        draw.rounded_rectangle((36, 48, 220, 238), radius=54, fill="#171b20", outline="#080a0c", width=12)
        draw.rounded_rectangle((49, 61, 207, 225), radius=43, outline=accent, width=11)
        draw.line((69, 90, 187, 90), fill="#f0d7a0", width=5)

    label = f"{number:02d}"
    bounds = draw.textbbox((0, 0), label, font=font, stroke_width=3)
    width = bounds[2] - bounds[0]
    height = bounds[3] - bounds[1]
    x = (SIZE - width) / 2
    y = (SIZE - height) / 2 - bounds[1] + (10 if shape == "potion" else 0)
    draw.text(
        (x + 5, y + 7),
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
        fill="#f7f2e8",
        stroke_width=3,
        stroke_fill=accent,
    )

    output_path.parent.mkdir(parents=True, exist_ok=True)
    image.save(output_path, format="PNG", optimize=True)


def main() -> None:
    project_root = Path(__file__).resolve().parents[1]
    font = ImageFont.truetype(find_font(), 104)

    relic_dir = project_root / "HammerMod" / "images" / "relics"
    for number, name in enumerate(RELICS, start=1):
        render_numbered_badge(relic_dir / f"{name}.png", number, font, "#d67a3d", "relic")

    potion_dir = project_root / "HammerMod" / "images" / "potions"
    potion_accents = ["#4eb6a5", "#e5c34b", "#79a5d8", "#d85b54"]
    for number, (name, accent) in enumerate(zip(POTIONS, potion_accents), start=1):
        render_numbered_badge(potion_dir / f"{name}.png", number, font, accent, "potion")

    print(f"Generated {len(RELICS)} relic and {len(POTIONS)} potion placeholders.")


if __name__ == "__main__":
    main()
