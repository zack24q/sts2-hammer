# Character Art Sources

`sources/HammerMod_character_select_source.jpg` is the user-supplied source
portrait.
`tools/process_character_art.py` removes only the white background connected to
the image boundary, crops the transparent margins, mirrors the hunter to face
right, and generates:

- `HammerMod_character_select.png`: the in-game portrait.
- `HammerMod_character_select_locked.png`: a dark locked-character variant.

The old SVG and dwarf PNG files were development placeholders and are no longer
referenced by the mod.
