#!/usr/bin/env python3
"""Build the HammerMod charge icon and its tintable edge glow."""

from __future__ import annotations

import argparse
from pathlib import Path

from PIL import Image, ImageChops, ImageFilter, ImageOps


CANVAS_SIZE = (256, 256)
CONTENT_SIZE = (200, 208)
GREEN_EXCESS_THRESHOLD = 28
GLOW_EXPANSION_SIZE = 13
GLOW_BLUR_RADIUS = 5


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


def create_edge_glow(hammer: Image.Image) -> Image.Image:
    """Create a white silhouette glow that can be tinted by the combat UI."""
    alpha = hammer.getchannel("A")
    expanded = alpha.filter(ImageFilter.MaxFilter(GLOW_EXPANSION_SIZE))
    softened = expanded.filter(ImageFilter.GaussianBlur(GLOW_BLUR_RADIUS))
    glow_alpha = ImageChops.lighter(softened, alpha)
    glow = Image.new("RGBA", CANVAS_SIZE, (255, 255, 255, 0))
    glow.putalpha(glow_alpha)
    return glow


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("source", type=Path)
    parser.add_argument("output", type=Path)
    parser.add_argument("--glow-output", type=Path)
    args = parser.parse_args()

    with Image.open(args.source) as source:
        hammer = extract_hammer(source)
    glow = create_edge_glow(hammer)

    args.output.parent.mkdir(parents=True, exist_ok=True)
    hammer.save(args.output, optimize=True)
    glow_output = args.glow_output or args.output.with_name("charge_counter_glow.png")
    glow_output.parent.mkdir(parents=True, exist_ok=True)
    glow.save(glow_output, optimize=True)


if __name__ == "__main__":
    main()
