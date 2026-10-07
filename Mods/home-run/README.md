# Home Run

Buy **The Slugger** in the weapon shop (level 1, one coin) and equip it. It is a
two-handed war hammer, so every normal hammer move works. In a fight the
RaidCharge button appears: press it to swing for the fences.

- The wind-up stalls time for a beat.
- The hit breaks guard and uses the game's physical-fall reaction with a huge
  launch impulse, so the opponent flies across the arena (often into the wall).
- On contact a white pop and near-frozen time hold on the victim, a
  **HOME RUN!** banner appears, and it counts the distance in metres as they fly.
- Fast-flying fighters leave a pale streak (switch: Launch streak).
- Recharges in one second (`COOLDOWN_FRAMES`); the button shows the ring.

With Final Blow and Chiaroscuro enabled, a lethal home run also gets the
knockout camera, and a wall landing gets debris and the wall-slam jolt.

How it is built: an innate Lua perk on the weapon shows the button and gates an
owned move bound to RaidCharge with a flag. The move is the base game's
two-handed heavy slash (`assets/animations/two_hand_heavy_slash.bytes` is an
unchanged copy of `Assets/Resources/gamedata/animations/binary/two_hand_heavy_slash.bytes`)
with `hit = "Physycal"` and a launch `impulse`. `on_post_hit` recognises the
swing by `event.attack.animation_name`, fires a script-triggered screen grade
and measures the flight from `fighter:snapshot()`.

Tuning: `LAUNCH` (the impulse; the native heavy slash uses x 800, y 850),
`SWING_DAMAGE` and `COOLDOWN_FRAMES` are at the top of `scripts/main.lua`.
The launch strength is a first guess and needs a playtest; if fighters clip or
barely move, change `LAUNCH` first.

Verification: headless checks drive the perk (arming, cooldown, wind-up,
contact recognition, banner, distance and cleanup). The native launch physics,
hit contact and AI response need a playtest.
