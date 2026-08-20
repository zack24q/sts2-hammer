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

`sources/HammerMod_energy_icon_source.png` is the user-supplied pink heart
gemstone used for the character's energy theme. `tools/process_energy_icon.py`
removes the green screen and generates the static 256x256 combat-counter
gemstone, the 256x256 large energy icon, and the 24x24 inline-text energy icon.
The scene retains the native `RotationLayers` container required by the energy
counter contract, but the retired hammer spinner and unused legacy energy
layers are removed from the exported resources.

The current energy source was downloaded from
`https://mdn.alipayobjects.com/huamei_vswgvo/afts/img/A*zfmrT5gfIBcAAAAAgCAAAAgAesV6AQ/original`
and has SHA-256
`c79e174dd9bcd3b3fa8bf76f4152c535f5aca63bfe1bd156565a12b65c9d223e`.

`sources/HammerMod_charge_counter_source.png` is the user-supplied monochrome
hammer artwork used for the charge-level display. The deterministic
`tools/process_charge_counter_icon.py` script removes the green screen and fits
the complete hammer into the transparent 256x256 `charge_counter.png`; it also
generates `charge_counter_glow.png`, a white outer-edge glow that the combat UI
tints red, orange, or white for charge levels 1, 2, or 3. Level 0 hides the glow,
while levels 1, 2, and 3 scale it to 1.00x, 1.03x, and 1.06x around the hammer's
fixed center. The numeric charge amount is positioned below the hammer. The
counter keeps the Regent-style 128x128 base size, 0.8 scale, and `(-36, 40)`
offset relative to the energy HUD.

The current charge-counter source was downloaded from
`https://mdn.alipayobjects.com/huamei_vswgvo/afts/img/A*Xk1rT4cDxokAAAAAgCAAAAgAesV6AQ/original`
and has SHA-256
`5db62ac94f254414d5ea27a364fd3a46dc88d84aca757b54f609cba542f695b4`.

The legacy selection PNGs, retired character-select SVGs, and dwarf character
PNGs are development artifacts and are no longer referenced by runtime idle
scenes.

`sources/HammerMod_character_defeated_source.png` is the user-supplied
green-screen defeated combat pose. `tools/process_character_idle.py` removes
the green screen and fits it into `HammerMod_character_defeated.png` with bottom
alignment so the fallen hunter rests near the combat floor. RitsuLib's native
`die` visual cue switches to this texture; it is intentionally not used by any
idle scene.
