# Umbra, the Last Eclipse

A three-phase boss battle. After enabling the mod, open the map: the battle sits
on its own map page. One round, six health bars (two per phase), the Titan fight
music, at the Gate of Shadows.

It is a short chapter. Entering the fight plays a narrator card and a conversation
between the Sensei, Umbra and Shadow, with their portraits (after the first time,
just Umbra's challenge; turn on **Full prologue every time** under
Options > Mod settings to see the whole scene again). Winning sets up an epilogue that plays the next time the
map opens.

Every phase plays: no hit can take him past the next phase's threshold until
its transformation has happened, and he cannot be hurt for 2.5 seconds while he
changes. The change also sends out a shockwave that shoves the player back.

- **Phase I** (full health): a samurai lord with a katana. The fight opens in slow
  motion with a gong, a dark push-in toward Umbra and his title banner.
- **Phase II** (two thirds): time nearly stops, he laughs, takes up a greatsword
  and calls two shades to fight for him. They stay until you beat them. He is shielded through the change.
- **Phase III** (one third): the sun goes out. He becomes the storm hermit and
  every two and a half seconds a pale column marks the player's position; one
  second later lightning strikes it for 6% health. Keep moving.

The story uses `sf2.story.before_fight` to hold the fight entry while
`sf2.story.play_sequence` shows the story dialogs, then resumes the entry. It
uses dialogs rather than act screens because the game plays its own full-screen
entry card as the fight loads. A `battle_result` subscription marks the epilogue, which a `scene_enter`
subscription plays on the map. Two saved mod-state flags remember the prologue
and the pending epilogue.

How it is built: one `sf2.rules.behavior` rule on the opponent reads
`fighter:snapshot()` health each tick and caps incoming damage in
`on_damage_resolving` with `scale_incoming_damage`. Phase changes use
`fighter:change_form`, `add_damage_shield`, `spawn_actor`, a shockwave of small
`fighter.opponent:move_by` steps and a HUD banner from `sf2.ui.open`. The
darkness, flashes and slow motion are `trigger = "script"` screen grades fired
with `sf2.fx.play`. Lightning uses `fighter:mark_rect` for the warning and
`overlaps_rect` for the hit.

Verification: headless checks drive the rule with controlled fighters (intro,
the damage cap at both thresholds, invulnerable transformations, shockwave,
both phase changes, minion spawns, lightning schedule and damage, marker and
banner cleanup). Native form changes, AI, the map entry and the look of the
arena need a playtest. Tune `level`, `STRIKE_EVERY` and `STRIKE_DAMAGE` in
`scripts/main.lua`.
