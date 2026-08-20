#!/usr/bin/env python3
"""Build the HammerMod energy icons from a green-screen gemstone source."""

from __future__ import annotations

import argparse
from pathlib import Path

from PIL import Image, ImageChops, ImageOps


CANVAS_SIZE = (256, 256)
ORB_CONTENT_SIZE = (232, 232)
TEXT_ICON_SIZE = (24, 24)
GREEN_EXCESS_THRESHOLD = 28


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


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("source", type=Path)
    parser.add_argument("output_dir", type=Path)
    args = parser.parse_args()

    with Image.open(args.source) as source:
        crystal = extract_crystal(source)

    text_icon = resize_rgba(crystal, TEXT_ICON_SIZE)

    args.output_dir.mkdir(parents=True, exist_ok=True)
    crystal.save(
        args.output_dir / "HammerMod_energy_orb_layer_1.png",
        optimize=True,
    )
    crystal.save(args.output_dir / "energy_big.png", optimize=True)
    text_icon.save(args.output_dir / "energy_text.png", optimize=True)


if __name__ == "__main__":
    main()
