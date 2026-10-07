# The Gauntlet

Five fighters, one fight, no break. After enabling the mod the battle sits on its
own map page, in the arena.

The opponent has five health bars, and each bar is a different fighter: the Sai
Dancer, the Chain Widow, the Hammer, Lynx and finally the Butcher. Empty a bar and
the screen flashes white, time nearly stops, and the next fighter leaps in at
fighting distance with a **WAVE n / 5** banner. You get 12% health back between
waves. No hit can skip a wave, and the newcomer cannot be hurt while entering.

How it is built: one `sf2.rules.behavior` rule on the opponent. It caps incoming
damage at each wave's threshold in `on_damage_resolving`, then on the next tick
shields the opponent, swaps the fighter with `fighter:change_form`, slides the
newcomer back with `move_by`, heals the player and shows the banner. The flash
and slow motion are script-triggered screen grades.

Tuning: `WAVES`, `SWAP_FRAMES`, `BREATHER` and the wave levels are at the top of
`scripts/main.lua`.

Verification: headless checks drive the rule with controlled fighters (opening,
every wave entering in order even from one huge hit, no damage while entering,
heals, the newcomer's slide, the last wave being beatable and cleanup). Native
form changes between rigs and the AI need a playtest.
