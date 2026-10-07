# Combo Hype

Buy the **Showman's Lotus** in the armour shop (level 2, one coin) and wear it.
Every combo turns the fight up:

| Hits | Tier | What changes |
| --- | --- | --- |
| 3 | NICE | Colour and contrast lift. |
| 5 | GREAT | More colour, a vignette, the camera squeezes in slightly. A white pop. |
| 8 | SAVAGE | An orange glow on highlights, the rest of the fight sinks under you. A slow-motion stinger. |
| 12 | UNSTOPPABLE | Gold glow and tint, the tightest camera, the fight muffled. A gong. |

A big counter on the right shows the hits and the tier. When the combo breaks,
the colour drains for a moment and the final count stays up briefly.

How it is built: an innate Lua perk on the armour listens to the game's own combo
counter (`on_combo_changed`). Each tier is a `trigger = "script"` screen grade held
for 1.6 seconds and refreshed by every hit, so it lasts exactly as long as the
combo; stingers fire once on reaching a tier. The camera squeeze uses the grade
`zoom`, which always keeps both fighters in view.

Tuning: the tier thresholds and grades are the `TIERS` table at the top of
`scripts/main.lua`.

Verification: headless checks drive the perk with native-shaped combo events
(no hype below 3 hits, tiers in order, one stinger per tier, counter text, the
drain, the counter hiding and a new combo restarting). The look and the game's
combo timing need a playtest.
