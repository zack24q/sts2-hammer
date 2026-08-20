#!/usr/bin/env python3
"""Build the HammerMod energy icons from a green-screen gemstone source."""

from __future__ import annotations

import argparse
from pathlib import Path

from PIL import Image, ImageChops, ImageEnhance, ImageOps


CANVAS_SIZE = (256, 256)
ORB_CONTENT_SIZE = (232, 232)
TEXT_ICON_SIZE = (24, 24)
GREEN_EXCESS_THRESHOLD = 28
THEME_COLOR = (255, 170, 200)
COLOR_SATURATION = 0.55
THEME_BLEND = 0.18
CARD_ICON_Y_OFFSET = 8


def resize_rgba(image: Image.Image, size: tuple[int, int]) -> Image.Image:
    """Resize with premultiplied alpha so removed green pixels cannot bleed."""
    return (
        image.convert("RGBa")
        .resize(size, Image.Resampling.LANCZOS)
        .convert("RGBA")
    )


def extract_crystal(source: Image.Image) -> Image.Image:
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
        raise ValueError("The source contains no non-green crystal pixels.")

    # Cap residual green on the antialiased outline before downsampling.
    clean_green = ImageChops.darker(green, strongest_non_green)
    crystal = Image.merge("RGBA", (red, clean_green, blue, alpha)).crop(bounds)
    scale = min(
        ORB_CONTENT_SIZE[0] / crystal.width,
        ORB_CONTENT_SIZE[1] / crystal.height,
    )
    crystal = resize_rgba(
        crystal,
        (round(crystal.width * scale), round(crystal.height * scale)),
    )

    canvas = Image.new("RGBA", CANVAS_SIZE, (0, 0, 0, 0))
    position = (
        (CANVAS_SIZE[0] - crystal.width) // 2,
        (CANVAS_SIZE[1] - crystal.height) // 2,
    )
    canvas.alpha_composite(crystal, position)
    return canvas


def soften_to_theme(crystal: Image.Image) -> Image.Image:
    """Reduce saturation and pull the gem toward the character's pale pink."""
    alpha = crystal.getchannel("A")
    rgb = crystal.convert("RGB")
    softened = ImageEnhance.Color(rgb).enhance(COLOR_SATURATION)
    tint = Image.new("RGB", crystal.size, THEME_COLOR)
    themed = Image.blend(softened, tint, THEME_BLEND).convert("RGBA")
    themed.putalpha(alpha)
    return themed


def shifted_copy(image: Image.Image, y_offset: int) -> Image.Image:
    shifted = Image.new("RGBA", image.size, (0, 0, 0, 0))
    shifted.alpha_composite(image, (0, y_offset))
    return shifted


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("source", type=Path)
    parser.add_argument("output_dir", type=Path)
    args = parser.parse_args()

    with Image.open(args.source) as source:
        crystal = soften_to_theme(extract_crystal(source))

    text_icon = resize_rgba(crystal, TEXT_ICON_SIZE)
    card_icon = shifted_copy(crystal, CARD_ICON_Y_OFFSET)

    args.output_dir.mkdir(parents=True, exist_ok=True)
    crystal.save(
        args.output_dir / "HammerMod_energy_orb_layer_1.png",
        optimize=True,
    )
    card_icon.save(args.output_dir / "energy_big.png", optimize=True)
    text_icon.save(args.output_dir / "energy_text.png", optimize=True)


if __name__ == "__main__":
    main()
