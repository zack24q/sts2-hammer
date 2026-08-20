#!/usr/bin/env python3
"""Build individual transparent power icons from the 5x5 source sheet."""

from __future__ import annotations

import argparse
import colorsys
from pathlib import Path

from PIL import Image, ImageChops, ImageDraw, ImageOps


GRID_SIZE = 5
ICON_SIZE = (256, 256)
VISIBLE_ICON_SIZE = (248, 248)
GREEN_EXCESS_THRESHOLD = 28
FOREGROUND_BAND_MIN_PIXELS = 10
NEON_GREEN_MINIMUM = 235
NEON_RED_BLUE_MAXIMUM = 40
CHARGE_SWITCH_STRENGTH_INDEX = 17

POWER_ICONS = (
    ("HammerStun.png", "Stun Value"),
    ("Aftershock.png", "Aftershock"),
    ("Focus.png", "Focus"),
    ("EndlessMomentum.png", "Endless Momentum"),
    ("DashJuice.png", "Dash Juice"),
    ("FelyneKoTechnique.png", "Felyne KO Technique"),
    ("PileDriver.png", "Pile Driver"),
    ("ConcussionResonance.png", "Concussion Resonance"),
    ("ImpactBurst.png", "Impact Burst"),
    ("CounterForm.png", "Counter Form"),
    ("Challenger.png", "Challenger"),
    ("WirebugContinuation.png", "Wirebug Continuation"),
    ("WeaknessExploit.png", "Weakness Exploit"),
    ("HarderWithEverySmash.png", "Harder With Every Smash"),
    ("Partbreaker.png", "Partbreaker"),
    ("OneMoreBonk.png", "One More Bonk"),
    ("BloodRite.png", "Blood Rite"),
    ("ChargeSwitchStrength.png", "Charge Switch: Strength"),
    ("LuckyVoucher.png", "Lucky Voucher"),
    ("Overcharge.png", "Overcharge"),
    ("FaceOff.png", "Face Off"),
    ("Wirefall.png", "Wirefall"),
    ("Farcaster.png", "Farcaster"),
    ("FreeMeal.png", "Free Meal"),
    ("ChargeSwitchCourage.png", "Charge Switch: Courage"),
)


def resize_rgba(image: Image.Image, size: tuple[int, int]) -> Image.Image:
    """Resize with premultiplied alpha so green pixels cannot bleed."""
    return (
        image.convert("RGBa")
        .resize(size, Image.Resampling.LANCZOS)
        .convert("RGBA")
    )


def foreground_band_centers(source: Image.Image) -> tuple[list[int], list[int]]:
    """Locate the artwork rows and columns instead of assuming an exact grid."""
    row_counts = [0] * source.height
    column_counts = [0] * source.width
    pixels = source.load()
    for top in range(source.height):
        for left in range(source.width):
            red, green, blue = pixels[left, top]
            if green - max(red, blue) <= GREEN_EXCESS_THRESHOLD:
                row_counts[top] += 1
                column_counts[left] += 1

    def centers(counts: list[int], axis_name: str) -> list[int]:
        bands: list[tuple[int, int]] = []
        start = None
        for position, count in enumerate([*counts, 0]):
            if count > FOREGROUND_BAND_MIN_PIXELS and start is None:
                start = position
            elif count <= FOREGROUND_BAND_MIN_PIXELS and start is not None:
                bands.append((start, position - 1))
                start = None

        if len(bands) != GRID_SIZE:
            raise ValueError(
                f"Expected {GRID_SIZE} foreground {axis_name} bands, "
                f"found {len(bands)}: {bands}."
            )
        return [(band_start + band_end + 1) // 2 for band_start, band_end in bands]

    return centers(column_counts, "column"), centers(row_counts, "row")


def split_cell(
    source: Image.Image,
    index: int,
    column_centers: list[int],
    row_centers: list[int],
) -> Image.Image:
    row, column = divmod(index, GRID_SIZE)
    cell_size = round(min(source.size) / GRID_SIZE)
    half_cell = cell_size // 2
    left = column_centers[column] - half_cell
    top = row_centers[row] - half_cell
    right = left + cell_size
    bottom = top + cell_size
    if left < 0 or top < 0 or right > source.width or bottom > source.height:
        raise ValueError(
            f"Detected cell {index} falls outside the source image: "
            f"{(left, top, right, bottom)}."
        )
    return source.crop((left, top, right, bottom))


def flattened_pixels(image: Image.Image):
    """Use Pillow's non-deprecated flat-pixel API when it is available."""
    getter = getattr(image, "get_flattened_data", None)
    return getter() if getter is not None else image.getdata()


def clear_residual_neon_green(icon: Image.Image) -> Image.Image:
    """Clear chroma pixels reintroduced by high-quality resize ringing."""
    cleaned: list[tuple[int, int, int, int]] = []
    for red, green, blue, alpha in flattened_pixels(icon):
        if (
            green >= NEON_GREEN_MINIMUM
            and red <= NEON_RED_BLUE_MAXIMUM
            and blue <= NEON_RED_BLUE_MAXIMUM
        ):
            cleaned.append((0, 0, 0, 0))
        else:
            cleaned.append((red, green, blue, alpha))

    result = Image.new("RGBA", icon.size)
    result.putdata(cleaned)
    return result


def fit_visible_icon(icon: Image.Image) -> Image.Image:
    """Crop transparent cell padding and match the official power-icon scale."""
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
    return clear_residual_neon_green(canvas)


def remove_green_screen(cell: Image.Image) -> Image.Image:
    """Remove green regions anchored by the source's neon chroma color."""
    source_rgb = cell.convert("RGB")
    red, green, blue = source_rgb.split()
    strongest_non_green = ImageChops.lighter(red, blue)
    green_excess = ImageChops.subtract(green, strongest_non_green)
    candidates = green_excess.point(
        lambda value: 255 if value > GREEN_EXCESS_THRESHOLD else 0
    )

    neon_green = green.point(
        lambda value: 255 if value >= NEON_GREEN_MINIMUM else 0
    )
    low_red = red.point(
        lambda value: 255 if value <= NEON_RED_BLUE_MAXIMUM else 0
    )
    low_blue = blue.point(
        lambda value: 255 if value <= NEON_RED_BLUE_MAXIMUM else 0
    )
    neon_seeds = ImageChops.multiply(
        neon_green,
        ImageChops.multiply(low_red, low_blue),
    )

    connected_marker = 128
    while True:
        unfilled_candidates = candidates.point(
            lambda value: 255 if value == 255 else 0
        )
        seeded_candidates = ImageChops.multiply(
            unfilled_candidates,
            neon_seeds,
        )
        bounds = seeded_candidates.getbbox()
        if bounds is None:
            break

        seeded_region = seeded_candidates.crop(bounds)
        offset = seeded_region.tobytes().find(b"\xff")
        if offset < 0:
            raise ValueError("Could not locate a neon-green seed pixel.")
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
    return fit_visible_icon(clear_residual_neon_green(icon))


def recolor_cool_pixels_red(icon: Image.Image) -> Image.Image:
    """Shift blue and purple artwork to a red family without changing form."""
    recolored: list[tuple[int, int, int, int]] = []
    for red, green, blue, alpha in flattened_pixels(icon):
        hue, saturation, value = colorsys.rgb_to_hsv(
            red / 255,
            green / 255,
            blue / 255,
        )
        hue_degrees = hue * 360
        if (
            alpha > 0
            and 180 <= hue_degrees <= 315
            and saturation >= 0.08
            and value >= 0.12
        ):
            hue_progress = (hue_degrees - 180) / (315 - 180)
            red_hue_degrees = (350 + 18 * hue_progress) % 360
            saturation = min(1, max(0.28, saturation * 1.08))
            new_red, new_green, new_blue = colorsys.hsv_to_rgb(
                red_hue_degrees / 360,
                saturation,
                value,
            )
            recolored.append(
                (
                    round(new_red * 255),
                    round(new_green * 255),
                    round(new_blue * 255),
                    alpha,
                )
            )
        else:
            recolored.append((red, green, blue, alpha))

    result = Image.new("RGBA", icon.size)
    result.putdata(recolored)
    return result


def count_hue_family(icon: Image.Image, family: str) -> int:
    count = 0
    for red, green, blue, alpha in flattened_pixels(icon):
        if alpha < 64:
            continue
        hue, saturation, value = colorsys.rgb_to_hsv(
            red / 255,
            green / 255,
            blue / 255,
        )
        if saturation < 0.2 or value < 0.12:
            continue
        hue_degrees = hue * 360
        if family == "red" and (hue_degrees >= 340 or hue_degrees <= 20):
            count += 1
        elif family == "cool" and 180 <= hue_degrees <= 315:
            count += 1
        elif family == "green" and 70 <= hue_degrees <= 170:
            count += 1
    return count


def validate_icons(icons: list[Image.Image]) -> None:
    if len(icons) != len(POWER_ICONS):
        raise ValueError(f"Expected {len(POWER_ICONS)} icons, got {len(icons)}.")

    for (file_name, _), icon in zip(POWER_ICONS, icons, strict=True):
        if icon.size != ICON_SIZE or icon.mode != "RGBA":
            raise ValueError(
                f"{file_name} is not a "
                f"{ICON_SIZE[0]}x{ICON_SIZE[1]} RGBA image."
            )
        alpha = icon.getchannel("A")
        alpha_bounds = alpha.getbbox()
        if alpha.getextrema()[0] != 0 or alpha_bounds is None:
            raise ValueError(
                f"{file_name} does not contain transparent background "
                "and icon pixels."
            )
        if (
            alpha_bounds[0] == 0
            or alpha_bounds[1] == 0
            or alpha_bounds[2] == icon.width
            or alpha_bounds[3] == icon.height
        ):
            raise ValueError(
                f"{file_name} has non-transparent pixels touching an edge; "
                "the source crop may include adjacent artwork."
            )
        visible_size = (
            alpha_bounds[2] - alpha_bounds[0],
            alpha_bounds[3] - alpha_bounds[1],
        )
        if max(visible_size) != max(VISIBLE_ICON_SIZE):
            raise ValueError(
                f"{file_name} has visible size {visible_size}, expected one "
                f"dimension to be {max(VISIBLE_ICON_SIZE)} pixels."
            )
        opaque_neon_pixels = sum(
            1
            for red, green, blue, alpha in flattened_pixels(icon)
            if (
                alpha >= 64
                and green >= NEON_GREEN_MINIMUM
                and red <= NEON_RED_BLUE_MAXIMUM
                and blue <= NEON_RED_BLUE_MAXIMUM
            )
        )
        if opaque_neon_pixels:
            raise ValueError(
                f"{file_name} retains {opaque_neon_pixels} opaque "
                "neon-green background pixels."
            )

    strength = icons[CHARGE_SWITCH_STRENGTH_INDEX]
    courage = icons[-1]
    strength_red = count_hue_family(strength, "red")
    strength_cool = count_hue_family(strength, "cool")
    courage_cool = count_hue_family(courage, "cool")
    if strength_red < 500 or strength_cool * 10 >= strength_red:
        raise ValueError("Charge Switch: Strength was not converted to a red palette.")
    if courage_cool < 500:
        raise ValueError("Charge Switch: Courage no longer retains its cool palette.")

    wirebug_green = count_hue_family(icons[11], "green")
    farcaster_green = count_hue_family(icons[22], "green")
    if wirebug_green < 300 or farcaster_green < 300:
        raise ValueError(
            "The green-screen removal damaged the Wirebug or Farcaster greens."
        )


def create_preview(icons: list[Image.Image]) -> Image.Image:
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

    for index, icon in enumerate(icons):
        row, column = divmod(index, GRID_SIZE)
        preview.alpha_composite(
            icon,
            (column * ICON_SIZE[0], row * ICON_SIZE[1]),
        )
    return preview


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("source", type=Path)
    parser.add_argument("output_dir", type=Path)
    parser.add_argument("--preview", type=Path)
    args = parser.parse_args()

    with Image.open(args.source) as source_image:
        source = ImageOps.exif_transpose(source_image).convert("RGB")
        if source.width != source.height:
            raise ValueError("The source sprite sheet must be square.")

        column_centers, row_centers = foreground_band_centers(source)
        icons = []
        for index in range(len(POWER_ICONS)):
            cell = split_cell(source, index, column_centers, row_centers)
            base_icon = remove_green_screen(cell)
            icon = (
                recolor_cool_pixels_red(base_icon)
                if index == CHARGE_SWITCH_STRENGTH_INDEX
                else base_icon
            )
            icons.append(icon)

    validate_icons(icons)
    args.output_dir.mkdir(parents=True, exist_ok=True)
    for (file_name, _), icon in zip(POWER_ICONS, icons, strict=True):
        icon.save(args.output_dir / file_name, optimize=True)

    if args.preview is not None:
        args.preview.parent.mkdir(parents=True, exist_ok=True)
        create_preview(icons).convert("RGB").save(args.preview, optimize=True)


if __name__ == "__main__":
    main()
