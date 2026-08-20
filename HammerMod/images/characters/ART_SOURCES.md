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

`tools/process_character_art.py` generates the two character-select variants
from the non-combat standing source
`sources/HammerMod_character_idle_source.png`:

- `HammerMod_character_select.png`: the unlocked full-body selection art.
- `HammerMod_character_select_locked.png`: a dark locked-character variant.

The character asset profile assigns these files to the unlocked and locked
character-select slots respectively. The full character-select scene also uses
`HammerMod_character_idle.png` as its separate standing figure.

`sources/HammerMod_character_select_source.jpg` is the retired white-haired
hunter draft. It is retained only as an archived source and is not used by the
processing tools, runtime scenes, or exported PCK.

`sources/HammerMod_character_select_background_source.png` is the user-supplied
dragon artwork used behind the standing portrait on the character-select
screen. `tools/process_character_select_background.py` creates the packaged
1920x1080 background, reducing saturation, contrast, brightness, and fine detail
so the separate character portrait remains the visual focus.

`sources/HammerMod_character_icon_source.png` is the user-supplied green-screen
head-and-shoulders avatar. The deterministic `tools/process_character_icon.py`
script removes its green screen and creates the transparent 88x88
`HammerMod_character_icon.png` plus the expanded white silhouette required by
`HammerMod_character_icon_outline.png`. The character asset profile uses that
portrait for the top-panel texture and the compact runtime icon, preventing the
Ironclad placeholder from supplying either avatar.

`sources/HammerMod_energy_icon_source.png` is the user-supplied pink heart
gemstone used for the character's energy theme. `tools/process_energy_icon.py`
removes the green screen, reduces the source saturation, blends it toward the
character theme pink `#FFAAC8`, and generates the static 256x256 combat-counter
gemstone, the 256x256 large energy icon, and the 24x24 inline-text energy icon.
The scene retains the native `RotationLayers` container required by the energy
counter contract, but the retired hammer spinner and unused legacy energy
layers are removed from the exported resources. The combat texture is shifted
down by 9 UI pixels while the label keeps the official character-counter
geometry. The card-face large icon has its content shifted down by 8 pixels as
well, independently compensating for the heart silhouette's top-heavy visual
center in both contexts.

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
fixed center. The glow is a child of RitsuLib's native counter, so it shares the
counter's visibility and resource-update lifecycle. The library's rotating gain
burst is disabled; a subtle 1.10x scale-and-fade ghost expands from the icon
center instead. The numeric charge amount is positioned below the hammer. The
counter keeps the Regent-style 128x128 base size, 0.8 scale, and `(122, 16)`
offset to the right of the energy HUD. It retains the HUD's default canvas
layer so hover tips and pile overlays render above it.

The current charge-counter source was downloaded from
`https://mdn.alipayobjects.com/huamei_vswgvo/afts/img/A*Xk1rT4cDxokAAAAAgCAAAAgAesV6AQ/original`
and has SHA-256
`5db62ac94f254414d5ea27a364fd3a46dc88d84aca757b54f609cba542f695b4`.

The retired character-select SVGs and dwarf character PNGs are development
artifacts and are no longer referenced by runtime scenes.

`sources/HammerMod_character_defeated_source.png` is the user-supplied
green-screen defeated combat pose. `tools/process_character_idle.py` removes
the green screen and fits it into `HammerMod_character_defeated.png` with bottom
alignment so the fallen hunter rests near the combat floor. RitsuLib's native
`die` visual cue switches to this texture; it is intentionally not used by any
idle scene.
