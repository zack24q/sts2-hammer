#!/usr/bin/env python3
"""Prepare the pink bone-armored hunter portrait for character selection."""

from __future__ import annotations

import argparse
from pathlib import Path

from PIL import Image, ImageOps


OUTPUT_SIZE = (600, 900)
SOURCE_CROP_BOX = (690, 100, 1970, 2020)


def create_selection_portrait(source: Image.Image) -> Image.Image:
    portrait = ImageOps.exif_transpose(source).convert("RGBA")
    if portrait.width < SOURCE_CROP_BOX[2] or portrait.height < SOURCE_CROP_BOX[3]:
        raise ValueError(
            f"Source portrait must be at least {SOURCE_CROP_BOX[2]}x"
            f"{SOURCE_CROP_BOX[3]} pixels."
        )

    portrait = portrait.crop(SOURCE_CROP_BOX)
    return portrait.resize(OUTPUT_SIZE, Image.Resampling.LANCZOS)


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
