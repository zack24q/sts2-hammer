# Character Art Sources

`sources/HammerMod_character_idle_source.png` is the user-supplied green-screen
artwork for the pink bone-armored hunter's non-combat standing pose.
`tools/process_character_idle.py` removes the green screen, preserves the
complete hunter and hammer, and fits them into the transparent 900x900
`HammerMod_character_idle.png` canvas. The character-select, merchant, and
rest-site scenes use this asset for their idle portrait.

`sources/HammerMod_character_combat_idle_source.png` is the user-supplied
green-screen combat-ready pose. The same processing tool generates the
transparent 900x900 `HammerMod_character_combat_idle.png`. The combat scene uses
this dedicated asset while the hunter is standing, and RitsuLib's native
`revive` visual cue switches back to it after the defeated pose.

`tools/process_character_art.py` generates the two legacy selection variants
from the non-combat standing source:

- `HammerMod_character_select.png`: the legacy unlocked selection portrait.
- `HammerMod_character_select_locked.png`: a dark locked-character variant.

`sources/HammerMod_character_select_source.jpg` is the retired white-haired
hunter draft. It is retained only as an archived source and is not used by the
processing tools, runtime scenes, or exported PCK.

`sources/HammerMod_character_select_background_source.png` is the user-supplied
dragon artwork used behind the standing portrait on the character-select
screen. `tools/process_character_select_background.py` creates the packaged
1920x1080 background, reducing saturation, contrast, brightness, and fine detail
so the separate character portrait remains the visual focus.

`sources/HammerMod_character_icon_source.png` is the user-supplied head-and-
shoulders portrait used by the top character panel. The deterministic
`tools/process_character_icon.py` script removes the green screen and creates
the transparent 88x88 `HammerMod_character_icon.png` plus the expanded white
silhouette required by `HammerMod_character_icon_outline.png`.

`sources/HammerMod_energy_icon_source.png` is the user-supplied pink crystal
used for the character's energy theme. `tools/process_energy_icon.py` removes
the green screen and generates the static 256x256 combat-counter crystal, a
separate centered hammer spinner, the 256x256 large energy icon, and the 24x24
inline-text energy icon. The spinner occupies the native `RotationLayers`
container so it uses the same runtime rotation behavior as the Ironclad.

`sources/HammerMod_charge_counter_source.png` is the user-supplied pink
bone-hammer artwork used for the charge-level display. The deterministic
`tools/process_charge_counter_icon.py` script removes the green screen and fits
the complete hammer into the transparent 256x256 `charge_counter.png`. In
combat, the counter copies the Regent star counter's 128x128 base size, 0.8
scale, and `(-36, 40)` offset relative to the energy HUD so it overlaps the
energy icon's lower-left corner.

The legacy selection PNGs, retired character-select SVGs, and dwarf character
PNGs are development artifacts and are no longer referenced by runtime idle
scenes.

`sources/HammerMod_character_defeated_source.png` is the user-supplied
green-screen defeated combat pose. `tools/process_character_idle.py` removes
the green screen and fits it into `HammerMod_character_defeated.png` with bottom
alignment so the fallen hunter rests near the combat floor. RitsuLib's native
`die` visual cue switches to this texture; it is intentionally not used by any
idle scene.
