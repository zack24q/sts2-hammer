# Character Art Sources

`sources/HammerMod_character_select_source.jpg` is the user-supplied source
portrait.
`tools/process_character_art.py` removes only the white background connected to
the image boundary, crops the transparent margins, mirrors the hunter to face
right, and generates:

- `HammerMod_character_select.png`: the character-select, merchant, and rest-site
  portrait.
- `HammerMod_character_select_locked.png`: a dark locked-character variant.

The old SVG and dwarf PNG files were development placeholders and are no longer
referenced by the mod.

`sources/HammerMod_character_idle_source.png` is the user-supplied combat idle
portrait. `tools/process_character_idle.py` crops its transparent margins and
fits the complete hunter and hammer into `HammerMod_character_idle.png` on the
900x900 combat canvas. This asset is used only by the normal combat visuals;
character select, merchant, and rest-site art remain separate.

`sources/HammerMod_character_defeated_source.png` is the user-supplied defeated
combat portrait. The same processing tool fits it into
`HammerMod_character_defeated.png` with bottom alignment so the fallen hunter
rests near the combat floor. RitsuLib's native `die` visual cue switches to this
texture, while `revive` switches back to `HammerMod_character_idle.png`.
