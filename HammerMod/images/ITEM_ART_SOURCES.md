# Relic And Potion Icon Source

`sources/relic_potion_icons_source.png` is the user-supplied 4x4 green-screen
sprite sheet for Hammer character relics and potions. Run
`tools/process_relic_potion_icons.py` to remove chroma-green regions and build
the 14 transparent 256x256 runtime PNGs in `images/relics` and
`images/potions`. The script crops each icon to its visible alpha bounds before
fitting it to a 252x252 content area, matching the effective scale of official
relic and potion icons without clipping the artwork.

The source order is left to right, then top to bottom:

| Row | Icons |
| --- | --- |
| 1 | Hammer Technique Charm, Master Hammer Technique Charm, Frostcraft Charm, Heroics Charm |
| 2 | Counterstrike Charm, Downed Pursuit Charm, Wirebug Cage, Flash Bomb |
| 3 | Dash Juice, Ancient Max Potion (unused), Adamant Seed, Might Seed |
| 4 | Evasion Mantle, Rocksteady Mantle, Pitfall Trap, Paralysis Trap (unused) |

The Ancient Max Potion and Paralysis Trap cells are intentionally skipped
because the mod has no registered model for either item. No runtime files are
generated for those two cells. `SlidingBoostJewel.png` is the legacy runtime
class filename for the localized Heroics Charm.
