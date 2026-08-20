#!/usr/bin/env python3
"""Remove a green screen and fit character art into the idle canvas."""

from __future__ import annotations

import argparse
from pathlib import Path

from PIL import Image, ImageChops, ImageOps


CANVAS_SIZE = (900, 900)
CONTENT_SIZE = (850, 848)
ALPHA_THRESHOLD = 8
CANVAS_MARGIN = 25
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
    """Resize premultiplied RGBA so removed green pixels cannot bleed."""
    resized = (
        image.convert("RGBa")
        .resize(size, Image.Resampling.LANCZOS)
        .convert("RGBA")
    )
    red, green, blue, alpha = resized.split()
    clean_green = ImageChops.darker(green, ImageChops.lighter(red, blue))
    return Image.merge("RGBA", (red, clean_green, blue, alpha))


def fit_portrait(source: Image.Image, vertical_align: str = "center") -> Image.Image:
    portrait = remove_green_screen(source)
    alpha = portrait.getchannel("A")
    opaque_alpha = alpha.point(
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

    canvas = Image.new("RGBA", CANVAS_SIZE, (0, 0, 0, 0))
    y_position = (
        (CANVAS_SIZE[1] - portrait.height) // 2
        if vertical_align == "center"
        else CANVAS_SIZE[1] - portrait.height - CANVAS_MARGIN
    )
    position = ((CANVAS_SIZE[0] - portrait.width) // 2, y_position)
    canvas.alpha_composite(portrait, position)
    return canvas


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("source", type=Path)
    parser.add_argument("output", type=Path)
    parser.add_argument(
        "--vertical-align",
        choices=("center", "bottom"),
        default="center",
    )
    args = parser.parse_args()

    args.output.parent.mkdir(parents=True, exist_ok=True)
    with Image.open(args.source) as source:
        fit_portrait(source, args.vertical_align).save(args.output, optimize=True)


if __name__ == "__main__":
    main()
