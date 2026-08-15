#!/usr/bin/env python3
"""Prepare the user-supplied Hammer Hunter portrait for the mod."""

from __future__ import annotations

import argparse
from pathlib import Path

import numpy as np
from PIL import Image, ImageDraw, ImageOps


WORK_SIZE = (750, 1000)
CROP_BOX = (50, 25, 650, 925)
FLOOD_MARKER = (1, 2, 3)


def extract_connected_white_background(source: Image.Image) -> Image.Image:
    rgb = ImageOps.exif_transpose(source).convert("RGB")
    rgb = rgb.resize(WORK_SIZE, Image.Resampling.LANCZOS)

    flood_map = rgb.copy()
    ImageDraw.floodfill(flood_map, (0, 0), FLOOD_MARKER, thresh=32)

    pixels = np.asarray(rgb)
    flooded = np.asarray(flood_map)
    alpha = np.where(np.all(flooded == FLOOD_MARKER, axis=2), 0, 255).astype(np.uint8)
    rgba = np.dstack((pixels, alpha))
    return Image.fromarray(rgba, "RGBA").crop(CROP_BOX)


def create_locked_variant(portrait: Image.Image) -> Image.Image:
    rgba = np.asarray(portrait).copy()
    luminance = np.asarray(ImageOps.grayscale(portrait.convert("RGB")), dtype=np.float32)
    detail = luminance / 255.0

    rgba[..., 0] = 18 + (detail * 34).astype(np.uint8)
    rgba[..., 1] = 14 + (detail * 27).astype(np.uint8)
    rgba[..., 2] = 24 + (detail * 38).astype(np.uint8)
    return Image.fromarray(rgba, "RGBA")


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("source", type=Path)
    parser.add_argument("output", type=Path)
    parser.add_argument("locked_output", type=Path)
    args = parser.parse_args()

    portrait = ImageOps.mirror(extract_connected_white_background(Image.open(args.source)))
    locked = create_locked_variant(portrait)

    args.output.parent.mkdir(parents=True, exist_ok=True)
    portrait.save(args.output, optimize=True)
    locked.save(args.locked_output, optimize=True)


if __name__ == "__main__":
    main()
