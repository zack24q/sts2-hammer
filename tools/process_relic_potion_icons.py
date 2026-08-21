#!/usr/bin/env python3
"""Build relic and potion icons from the user-supplied 4x4 sprite sheet."""

from __future__ import annotations

import argparse
import colorsys
from pathlib import Path

from PIL import Image, ImageChops, ImageDraw, ImageOps


GRID_SIZE = 4
ICON_SIZE = (256, 256)
VISIBLE_ICON_SIZE = (252, 252)
GREEN_EXCESS_THRESHOLD = 28
CHROMA_SEED_EXCESS_THRESHOLD = 110
SKIPPED_SOURCE_INDICES = frozenset({9, 15})
SOURCE_CELL_INSETS = {
    7: (0, 16, 0, 0),
    8: (0, 0, 0, 16),
}

ITEM_ICONS = (
    (0, "relics/HammerTechniqueCharm.png", "Hammer Technique Charm"),
    (1, "relics/MasterHammerTechniqueCharm.png", "Master Hammer Technique Charm"),
    (2, "relics/FrostcraftCharm.png", "Frostcraft Charm"),
    (3, "relics/SlidingBoostJewel.png", "Heroics Charm"),
    (4, "relics/CounterstrikeCharm.png", "Counterstrike Charm"),
    (5, "relics/DownedPursuitCharm.png", "Downed Pursuit Charm"),
    (6, "relics/WirebugCage.png", "Wirebug Cage"),
    (7, "potions/FlashBomb.png", "Flash Bomb"),
    (8, "potions/DashJuiceG.png", "Dash Juice"),
    (10, "potions/AdamantSeed.png", "Adamant Seed"),
    (11, "potions/MightSeed.png", "Might Seed"),
    (12, "relics/EvasionMantle.png", "Evasion Mantle"),
    (13, "relics/RocksteadyMantle.png", "Rocksteady Mantle"),
    (14, "potions/Pitfall.png", "Pitfall Trap"),
)


def flattened_pixels(image: Image.Image):
    """Use Pillow's non-deprecated flat-pixel API when it is available."""
    getter = getattr(image, "get_flattened_data", None)
    return getter() if getter is not None else image.getdata()


def resize_rgba(image: Image.Image, size: tuple[int, int]) -> Image.Image:
    """Resize with premultiplied alpha so green pixels cannot bleed."""
    return (
        image.convert("RGBa")
        .resize(size, Image.Resampling.LANCZOS)
        .convert("RGBA")
    )


def split_cell(source: Image.Image, index: int) -> Image.Image:
    row, column = divmod(index, GRID_SIZE)
    left = round(column * source.width / GRID_SIZE)
    top = round(row * source.height / GRID_SIZE)
    right = round((column + 1) * source.width / GRID_SIZE)
    bottom = round((row + 1) * source.height / GRID_SIZE)
    inset_left, inset_top, inset_right, inset_bottom = SOURCE_CELL_INSETS.get(
        index,
        (0, 0, 0, 0),
    )
    left += inset_left
    top += inset_top
    right -= inset_right
    bottom -= inset_bottom
    return source.crop((left, top, right, bottom))


def clear_residual_chroma_green(icon: Image.Image) -> Image.Image:
    """Clear chroma pixels reintroduced by high-quality resize ringing."""
    cleaned: list[tuple[int, int, int, int]] = []
    for red, green, blue, alpha in flattened_pixels(icon):
        if green - max(red, blue) > CHROMA_SEED_EXCESS_THRESHOLD:
            cleaned.append((0, 0, 0, 0))
        else:
            cleaned.append((red, green, blue, alpha))

    result = Image.new("RGBA", icon.size)
    result.putdata(cleaned)
    return result


def fit_visible_icon(icon: Image.Image) -> Image.Image:
    """Crop transparent cell padding and match the official item-icon scale."""
    bounds = icon.getchannel("A").getbbox()
    if bounds is None:
        raise ValueError("A source cell contains no visible icon pixels.")

    cropped = icon.crop(bounds)
    scale = min(
        VISIBLE_ICON_SIZE[0] / cropped.width,
        VISIBLE_ICON_SIZE[1] / cropped.height,
    )
    fitted = resize_rgba(
        cropped,
        (round(cropped.width * scale), round(cropped.height * scale)),
    )
    canvas = Image.new("RGBA", ICON_SIZE, (0, 0, 0, 0))
    canvas.alpha_composite(
        fitted,
        ((ICON_SIZE[0] - fitted.width) // 2, (ICON_SIZE[1] - fitted.height) // 2),
    )
    return clear_residual_chroma_green(canvas)


def remove_green_screen(cell: Image.Image) -> Image.Image:
    """Remove green regions anchored by the source's neon chroma color."""
    source_rgb = cell.convert("RGB")
    red, green, blue = source_rgb.split()
    strongest_non_green = ImageChops.lighter(red, blue)
    green_excess = ImageChops.subtract(green, strongest_non_green)
    candidates = green_excess.point(
        lambda value: 255 if value > GREEN_EXCESS_THRESHOLD else 0
    )

    chroma_seeds = green_excess.point(
        lambda value: (
            255 if value > CHROMA_SEED_EXCESS_THRESHOLD else 0
        )
    )

    connected_marker = 128
    while True:
        unfilled_candidates = candidates.point(
            lambda value: 255 if value == 255 else 0
        )
        seeded_candidates = ImageChops.multiply(
            unfilled_candidates,
            chroma_seeds,
        )
        bounds = seeded_candidates.getbbox()
        if bounds is None:
            break

        seeded_region = seeded_candidates.crop(bounds)
        offset = seeded_region.tobytes().find(b"\xff")
        if offset < 0:
            raise ValueError("Could not locate a chroma-green seed pixel.")
        region_width = bounds[2] - bounds[0]
        seed = (
            bounds[0] + offset % region_width,
            bounds[1] + offset // region_width,
        )
        ImageDraw.floodfill(
            candidates,
            seed,
            connected_marker,
            thresh=0,
        )

    alpha = candidates.point(
        lambda value: 0 if value == connected_marker else 255
    )
    if alpha.getbbox() is None:
        raise ValueError("A source cell contains no non-green icon pixels.")

    icon = source_rgb.convert("RGBA")
    icon.putalpha(alpha)
    return fit_visible_icon(clear_residual_chroma_green(icon))


def count_green_pixels(icon: Image.Image) -> int:
    count = 0
    for red, green, blue, alpha in flattened_pixels(icon):
        if alpha < 64:
            continue
        hue, saturation, value = colorsys.rgb_to_hsv(
            red / 255,
            green / 255,
            blue / 255,
        )
        if 70 <= hue * 360 <= 170 and saturation >= 0.2 and value >= 0.12:
            count += 1
    return count


def validate_icons(icons: dict[int, Image.Image]) -> None:
    expected_indices = set(range(GRID_SIZE * GRID_SIZE)) - SKIPPED_SOURCE_INDICES
    actual_indices = {source_index for source_index, _, _ in ITEM_ICONS}
    if actual_indices != expected_indices or set(icons) != expected_indices:
        raise ValueError("The generated icon set does not match the 14 used cells.")

    for source_index, relative_path, _ in ITEM_ICONS:
        icon = icons[source_index]
        if icon.size != ICON_SIZE or icon.mode != "RGBA":
            raise ValueError(
                f"{relative_path} is not a "
                f"{ICON_SIZE[0]}x{ICON_SIZE[1]} RGBA image."
            )
        alpha = icon.getchannel("A")
        alpha_bounds = alpha.getbbox()
        if alpha.getextrema()[0] != 0 or alpha_bounds is None:
            raise ValueError(
                f"{relative_path} does not contain transparent background "
                "and icon pixels."
            )
        visible_size = (
            alpha_bounds[2] - alpha_bounds[0],
            alpha_bounds[3] - alpha_bounds[1],
        )
        if max(visible_size) != max(VISIBLE_ICON_SIZE):
            raise ValueError(
                f"{relative_path} has visible size {visible_size}, expected "
                f"one dimension to be {max(VISIBLE_ICON_SIZE)} pixels."
            )
        if (
            alpha_bounds[0] == 0
            or alpha_bounds[1] == 0
            or alpha_bounds[2] == icon.width
            or alpha_bounds[3] == icon.height
        ):
            raise ValueError(
                f"{relative_path} has non-transparent pixels touching an edge."
            )
        opaque_chroma_pixels = sum(
            1
            for red, green, blue, alpha in flattened_pixels(icon)
            if (
                alpha >= 64
                and green - max(red, blue) > CHROMA_SEED_EXCESS_THRESHOLD
            )
        )
        if opaque_chroma_pixels:
            raise ValueError(
                f"{relative_path} retains {opaque_chroma_pixels} opaque "
                "chroma-green background pixels."
            )

    if count_green_pixels(icons[10]) < 300:
        raise ValueError("Green-screen removal damaged the Adamant Seed greens.")


def create_preview(icons: dict[int, Image.Image]) -> Image.Image:
    preview_size = (ICON_SIZE[0] * GRID_SIZE, ICON_SIZE[1] * GRID_SIZE)
    preview = Image.new("RGBA", preview_size, (0, 0, 0, 255))
    draw = ImageDraw.Draw(preview)
    tile_size = 16
    for top in range(0, preview.height, tile_size):
        for left in range(0, preview.width, tile_size):
            shade = 64 if (left // tile_size + top // tile_size) % 2 == 0 else 88
            draw.rectangle(
                (left, top, left + tile_size - 1, top + tile_size - 1),
                fill=(shade, shade, shade, 255),
            )

    for source_index, icon in icons.items():
        row, column = divmod(source_index, GRID_SIZE)
        preview.alpha_composite(
            icon,
            (column * ICON_SIZE[0], row * ICON_SIZE[1]),
        )
    return preview


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("source", type=Path)
    parser.add_argument("output_root", type=Path)
    parser.add_argument("--preview", type=Path)
    args = parser.parse_args()

    with Image.open(args.source) as source_image:
        source = ImageOps.exif_transpose(source_image).convert("RGB")
        if source.width != source.height:
            raise ValueError("The source sprite sheet must be square.")

        icons = {
            source_index: remove_green_screen(split_cell(source, source_index))
            for source_index, _, _ in ITEM_ICONS
        }

    validate_icons(icons)
    for source_index, relative_path, _ in ITEM_ICONS:
        output_path = args.output_root / relative_path
        output_path.parent.mkdir(parents=True, exist_ok=True)
        icons[source_index].save(output_path, optimize=True)

    if args.preview is not None:
        args.preview.parent.mkdir(parents=True, exist_ok=True)
        create_preview(icons).convert("RGB").save(args.preview, optimize=True)


if __name__ == "__main__":
    main()
