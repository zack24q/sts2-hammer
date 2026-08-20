#!/usr/bin/env python3
"""Build the transparent HammerMod charge icon from a green-screen source."""

from __future__ import annotations

import argparse
from pathlib import Path

from PIL import Image, ImageChops, ImageOps


CANVAS_SIZE = (256, 256)
CONTENT_SIZE = (240, 240)
GREEN_EXCESS_THRESHOLD = 28


def resize_rgba(image: Image.Image, size: tuple[int, int]) -> Image.Image:
    """Resize with premultiplied alpha so removed green pixels cannot bleed."""
    return (
        image.convert("RGBa")
        .resize(size, Image.Resampling.LANCZOS)
        .convert("RGBA")
    )


def extract_hammer(source: Image.Image) -> Image.Image:
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
        raise ValueError("The source contains no non-green hammer pixels.")

    clean_green = ImageChops.darker(green, strongest_non_green)
    hammer = Image.merge("RGBA", (red, clean_green, blue, alpha)).crop(bounds)
    scale = min(
        CONTENT_SIZE[0] / hammer.width,
        CONTENT_SIZE[1] / hammer.height,
    )
    hammer = resize_rgba(
        hammer,
        (round(hammer.width * scale), round(hammer.height * scale)),
    )

    canvas = Image.new("RGBA", CANVAS_SIZE, (0, 0, 0, 0))
    position = (
        (CANVAS_SIZE[0] - hammer.width) // 2,
        (CANVAS_SIZE[1] - hammer.height) // 2,
    )
    canvas.alpha_composite(hammer, position)
    return canvas


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("source", type=Path)
    parser.add_argument("output", type=Path)
    args = parser.parse_args()

    with Image.open(args.source) as source:
        hammer = extract_hammer(source)

    args.output.parent.mkdir(parents=True, exist_ok=True)
    hammer.save(args.output, optimize=True)


if __name__ == "__main__":
    main()
