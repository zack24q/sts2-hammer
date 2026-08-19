# Character Art Sources

`sources/HammerMod_character_idle_source.png` is the canonical pink
bone-armored hunter artwork. `tools/process_character_art.py` takes a
portrait-oriented crop from that transparent source and generates:

- `HammerMod_character_select.png`: the character-select, merchant, and rest-site
  portrait.
- `HammerMod_character_select_locked.png`: a dark locked-character variant.

`sources/HammerMod_character_select_source.jpg` is the retired white-haired
hunter draft. It is retained only as an archived source and is not used by the
processing tools, runtime scenes, or exported PCK.

The old SVG and dwarf PNG files were development placeholders and are no longer
referenced by the mod.

`tools/process_character_idle.py` also crops the transparent margins of the
canonical source and fits the complete hunter and hammer into
`HammerMod_character_idle.png` on the 900x900 combat canvas. The selection,
merchant, and rest-site portrait uses a tighter vertical crop of the same
character so the bone armor remains readable in those layouts.

`sources/HammerMod_character_defeated_source.png` is the user-supplied defeated
combat portrait. The same processing tool fits it into
`HammerMod_character_defeated.png` with bottom alignment so the fallen hunter
rests near the combat floor. RitsuLib's native `die` visual cue switches to this
texture, while `revive` switches back to `HammerMod_character_idle.png`.
