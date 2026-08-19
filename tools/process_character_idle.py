#!/usr/bin/env python3
"""Fit a transparent character portrait into the combat-art canvas."""

from __future__ import annotations

import argparse
from pathlib import Path

from PIL import Image


CANVAS_SIZE = (900, 900)
CONTENT_SIZE = (850, 848)
ALPHA_THRESHOLD = 8
CANVAS_MARGIN = 25


def fit_portrait(source: Image.Image, vertical_align: str = "center") -> Image.Image:
    portrait = source.convert("RGBA")
    alpha = portrait.getchannel("A")
    opaque_alpha = alpha.point(
        lambda value: 255 if value > ALPHA_THRESHOLD else 0
    )
    bounds = opaque_alpha.getbbox()
    if bounds is None:
        raise ValueError("Source portrait contains no visible pixels.")

    portrait = portrait.crop(bounds)
    portrait.thumbnail(CONTENT_SIZE, Image.Resampling.LANCZOS)

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
