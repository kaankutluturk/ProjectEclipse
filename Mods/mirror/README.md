# The Mirror

A fight on its own map page, on the lantern-lit water. The Mirror fights with
your weapon type and at your level, and it learns from you:

- **Echo.** Every attack you throw, it throws straight back a moment later.
  The screen blinks cold and the banner says what it copied.
- **Read.** Use the same attack three times and it has learned it. From then on,
  when you start that attack it steps back out of it, with a short stall and
  **READ YOU**. The banner counts how many of your moves it has learned.
- What it learns lasts the whole three-round fight. Vary your attacks.

How it is built:

- `sf2.modes.register` with `on_prepare` reads your equipped weapon
  (`sf2.profile.equipment`) and level when you enter, and picks a pre-registered
  Mirror carrying a core weapon of the same type, so it has your move set.
- A behavior rule on the Mirror sees every move you start (`on_animation_start`
  with `target = "opponent"`) and records your attacks.
- A Lua tactic (`on_decide`) turns that record into choices: it returns the
  same attack from its legal actions to echo you, or a backward move to read
  you, and otherwise leaves the game's own AI in charge.
- The rule shows the HUD and fires script-triggered screen grades.

Tuning: `ECHO_DELAY`, `ECHO_WINDOW` and `READ_AFTER` at the top of
`scripts/main.lua`.

Verification: headless checks drive the rule and the tactic with controlled
snapshots and actions (wake, ignoring movement, echo after the delay and only
once, learning on the third use, the read and the HUD). Weapon selection on
entry, move-name matching between your fighter and the Mirror, and the AI in a
real fight need a playtest. Mod weapons are matched by their weapon type.
