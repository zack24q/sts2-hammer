#!/usr/bin/env python3
"""Build the Hammer Hunter top-panel portrait and outline from green screen."""

from __future__ import annotations

import argparse
from pathlib import Path

from PIL import Image, ImageChops, ImageFilter, ImageOps


ICON_SIZE = (88, 88)
CONTENT_SIZE = (80, 80)
GREEN_EXCESS_THRESHOLD = 28
OUTLINE_EXPANSION = 4


def resize_rgba(image: Image.Image, size: tuple[int, int]) -> Image.Image:
    """Resize with premultiplied alpha to prevent green-screen edge bleed."""
    return (
        image.convert("RGBa")
        .resize(size, Image.Resampling.LANCZOS)
        .convert("RGBA")
    )


def extract_portrait(source: Image.Image) -> Image.Image:
    source_rgb = ImageOps.exif_transpose(source).convert("RGB")
    red, green, blue = source_rgb.split()
    strongest_non_green = ImageChops.lighter(red, blue)
    green_excess = ImageChops.subtract(green, strongest_non_green)
    green_screen = green_excess.point(
        lambda value: 255 if value > GREEN_EXCESS_THRESHOLD else 0
    )
    alpha = ImageOps.invert(green_screen)
    bounds = alpha.getbbox()
    if bounds is None:
        raise ValueError("The source contains no non-green portrait pixels.")

    clean_green = ImageChops.darker(green, strongest_non_green)
    portrait = Image.merge("RGBA", (red, clean_green, blue, alpha)).crop(bounds)
    scale = min(
        CONTENT_SIZE[0] / portrait.width,
        CONTENT_SIZE[1] / portrait.height,
    )
    portrait = resize_rgba(
        portrait,
        (round(portrait.width * scale), round(portrait.height * scale)),
    )

    icon = Image.new("RGBA", ICON_SIZE, (0, 0, 0, 0))
    position = (
        (ICON_SIZE[0] - portrait.width) // 2,
        (ICON_SIZE[1] - portrait.height) // 2,
    )
    icon.alpha_composite(portrait, position)
    return icon


def create_outline(icon: Image.Image) -> Image.Image:
    filter_size = OUTLINE_EXPANSION * 2 + 1
    alpha = icon.getchannel("A").filter(ImageFilter.MaxFilter(filter_size))
    outline = Image.new("RGBA", ICON_SIZE, (255, 255, 255, 0))
    outline.putalpha(alpha)
    return outline


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("source", type=Path)
    parser.add_argument("output_dir", type=Path)
    args = parser.parse_args()

    with Image.open(args.source) as source:
        icon = extract_portrait(source)
    outline = create_outline(icon)

    args.output_dir.mkdir(parents=True, exist_ok=True)
    icon.save(args.output_dir / "HammerMod_character_icon.png", optimize=True)
    outline.save(
        args.output_dir / "HammerMod_character_icon_outline.png",
        optimize=True,
    )


if __name__ == "__main__":
    main()
