#!/usr/bin/env python3
"""Prepare a subdued 16:9 background for the character-select scene."""

from __future__ import annotations

import argparse
from pathlib import Path

from PIL import Image, ImageEnhance, ImageFilter, ImageOps


OUTPUT_SIZE = (1920, 1080)
SATURATION = 0.32
CONTRAST = 0.82
BRIGHTNESS = 0.58
COOL_WASH = (10, 17, 30)
COOL_WASH_STRENGTH = 0.18
BLUR_RADIUS = 1.8


def fit_cover(source: Image.Image) -> Image.Image:
    source_ratio = source.width / source.height
    output_ratio = OUTPUT_SIZE[0] / OUTPUT_SIZE[1]

    if source_ratio > output_ratio:
        crop_width = round(source.height * output_ratio)
        left = (source.width - crop_width) // 2
        source = source.crop((left, 0, left + crop_width, source.height))
    elif source_ratio < output_ratio:
        crop_height = round(source.width / output_ratio)
        top = (source.height - crop_height) // 2
        source = source.crop((0, top, source.width, top + crop_height))

    return source.resize(OUTPUT_SIZE, Image.Resampling.LANCZOS)


def create_selection_background(source: Image.Image) -> Image.Image:
    background = fit_cover(ImageOps.exif_transpose(source).convert("RGB"))
    background = background.filter(ImageFilter.GaussianBlur(BLUR_RADIUS))
    background = ImageEnhance.Color(background).enhance(SATURATION)
    background = ImageEnhance.Contrast(background).enhance(CONTRAST)
    background = ImageEnhance.Brightness(background).enhance(BRIGHTNESS)
    cool_wash = Image.new("RGB", OUTPUT_SIZE, COOL_WASH)
    return Image.blend(background, cool_wash, COOL_WASH_STRENGTH)


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("source", type=Path)
    parser.add_argument("output", type=Path)
    args = parser.parse_args()

    with Image.open(args.source) as source:
        background = create_selection_background(source)

    args.output.parent.mkdir(parents=True, exist_ok=True)
    background.save(args.output, optimize=True)


if __name__ == "__main__":
    main()
