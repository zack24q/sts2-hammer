#!/usr/bin/env python3
"""Prepare legacy selection portraits from green-screen character art."""

from __future__ import annotations

import argparse
from pathlib import Path

from PIL import Image, ImageChops, ImageOps


OUTPUT_SIZE = (600, 900)
CONTENT_SIZE = (580, 880)
ALPHA_THRESHOLD = 8
GREEN_EXCESS_THRESHOLD = 28


def remove_green_screen(source: Image.Image) -> Image.Image:
    oriented = ImageOps.exif_transpose(source)
    portrait = oriented.convert("RGBA")
    if "A" in oriented.getbands() and portrait.getchannel("A").getextrema()[0] < 255:
        return portrait

    red, green, blue = oriented.convert("RGB").split()
    strongest_non_green = ImageChops.lighter(red, blue)
    green_excess = ImageChops.subtract(green, strongest_non_green)
    green_screen = green_excess.point(
        lambda value: 255 if value > GREEN_EXCESS_THRESHOLD else 0
    )
    alpha = ImageOps.invert(green_screen)
    if alpha.getbbox() is None:
        raise ValueError("Source portrait contains no non-green pixels.")

    clean_green = ImageChops.darker(green, strongest_non_green)
    return Image.merge("RGBA", (red, clean_green, blue, alpha))


def resize_rgba(image: Image.Image, size: tuple[int, int]) -> Image.Image:
    resized = (
        image.convert("RGBa")
        .resize(size, Image.Resampling.LANCZOS)
        .convert("RGBA")
    )
    red, green, blue, alpha = resized.split()
    clean_green = ImageChops.darker(green, ImageChops.lighter(red, blue))
    return Image.merge("RGBA", (red, clean_green, blue, alpha))


def create_selection_portrait(source: Image.Image) -> Image.Image:
    portrait = remove_green_screen(source)
    opaque_alpha = portrait.getchannel("A").point(
        lambda value: 255 if value > ALPHA_THRESHOLD else 0
    )
    bounds = opaque_alpha.getbbox()
    if bounds is None:
        raise ValueError("Source portrait contains no visible pixels.")

    portrait = portrait.crop(bounds)
    scale = min(
        CONTENT_SIZE[0] / portrait.width,
        CONTENT_SIZE[1] / portrait.height,
        1.0,
    )
    portrait = resize_rgba(
        portrait,
        (round(portrait.width * scale), round(portrait.height * scale)),
    )

    canvas = Image.new("RGBA", OUTPUT_SIZE, (0, 0, 0, 0))
    position = (
        (OUTPUT_SIZE[0] - portrait.width) // 2,
        OUTPUT_SIZE[1] - portrait.height - 10,
    )
    canvas.alpha_composite(portrait, position)
    return canvas


def create_locked_variant(portrait: Image.Image) -> Image.Image:
    alpha = portrait.getchannel("A")
    luminance = ImageOps.grayscale(portrait.convert("RGB"))
    locked = ImageOps.colorize(
        luminance,
        black=(18, 14, 24),
        white=(52, 41, 62),
    ).convert("RGBA")
    locked.putalpha(alpha)
    return locked


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("source", type=Path)
    parser.add_argument("output", type=Path)
    parser.add_argument("locked_output", type=Path)
    args = parser.parse_args()

    with Image.open(args.source) as source:
        portrait = create_selection_portrait(source)
        locked = create_locked_variant(portrait)

    args.output.parent.mkdir(parents=True, exist_ok=True)
    portrait.save(args.output, optimize=True)
    locked.save(args.locked_output, optimize=True)


if __name__ == "__main__":
    main()
