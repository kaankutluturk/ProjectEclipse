# Shadow Clones

Buy the **Faceless Mask** in the helmet shop (level 2, one coin) and wear it.
In a fight the RaidCharge button appears. Press it: the fighter performs the
Grasp of Darkness cast, the picture sinks into ink while time stalls, and three
shadows rise beside you: a long katana, a kusarigama and twin swords. They fight
with the game's own AI for ten seconds, then fade. The button refills over
20 seconds.

While clones are out the camera pulls back to frame every fighter. Switches under
**Options > Mod settings**: Frame the clones, Summon flash.

How it is built:

- `sf2.items.set_innate_perks` puts a Lua perk on the helm, so the ability goes
  wherever the mask goes.
- The cast is an owned move bound to RaidCharge. `assets/animations/darkness_hug_player.bytes`
  is an unchanged copy of the base game's `Assets/Resources/gamedata/animations/binary/darkness_hug_player.bytes`.
  The move requires the perk's `ready` flag, which the perk clears on cast and
  restores when the cooldown ends.
- `on_animation_start` sees the cast, fires a `trigger = "script"` screen grade
  with `sf2.fx.play`, and the next ticks spawn three `sf2.actors` clones.
- `fighter:acquire_camera` frames the clones while they live.

Clone strength comes from the core warrior templates at level 30. Change
`CLONE_FRAMES`, `COOLDOWN_FRAMES` or the templates at the top of
`scripts/main.lua`.

Verification: headless checks load the mod and drive its perk with a controlled
fighter (flag, cooldown ring, summon flash, three spawns behind the caster,
camera take and release, cooldown recovery). The RaidCharge input outside raids,
the cast's alignment, actor AI and the framing need a playtest. Actors do not
spawn in local versus, PvP or raids.
