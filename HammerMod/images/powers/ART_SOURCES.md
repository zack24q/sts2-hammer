# Power Icon Source

`sources/status_icons_source.png` is the user-supplied 5x5 green-screen sprite
sheet for the Hammer character's main combat statuses. Run
`tools/process_power_icons.py` to split the sheet, remove chroma-green regions
anchored by the source's neon background color, and create the 25 transparent
256x256 runtime PNGs. This also clears enclosed background holes without
removing the natural greens used by leaves, the Wirebug, or the Farcaster. The
source artwork is not positioned on perfectly even 409.6-pixel rows, so the
script detects the five artwork row and column centers before making square
crops; mechanically dividing the canvas would mix parts of rows 4 and 5.

The source order is left to right, then top to bottom:

| Row | Icons |
| --- | --- |
| 1 | Stun Value, Aftershock, Focus, Endless Momentum, Dash Juice |
| 2 | Felyne KO Technique, Pile Driver, Concussion Resonance, Impact Burst, Counter Form |
| 3 | Challenger, Wirebug Continuation, Weakness Exploit, Harder With Every Smash, Partbreaker |
| 4 | One More Bonk, Blood Rite, Charge Switch: Strength, Lucky Voucher, Overcharge |
| 5 | Face Off, Wirefall, Farcaster, Free Meal, Charge Switch: Courage |

Charge Switch: Strength and Charge Switch: Courage share the same source
drawing. The processing script changes only the blue and purple pixels in
Charge Switch: Strength to a red family; Charge Switch: Courage and the other
23 source drawings keep their original palettes.
