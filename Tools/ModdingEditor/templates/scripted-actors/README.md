# Scripted Actor Sparring

Enable this mod and play Act I Tournament stage 3 (normal or Eclipse). Use the
HUD to summon two independent fighters or dismiss them. Each uses the same Lua
tactic with its own controller memory. They approach, choose a currently eligible
punch/kick, and voluntarily wait between attacks. Facing-relative forward/back
movement keeps each actor approaching after crossing its partner. No Lua play_move is used.

The original Skeleton, knife equipment and Standard tactic provide the native
rig, available actions, collision and reactions. Lua chooses from immutable
observations; it cannot force unavailable actions or bypass native eligibility.
The spawning behavior explicitly targets the pair at each other, respecting
native attack-safe target transitions even if a main fighter moves closer.
The tactic waits when its target is a main fighter. The original duel remains
active, so ordinary fighters can still interrupt the pair; this is a sparring
demonstration rather than an isolated arena or an extra player opponent. It may take a few
seconds for movement and repeated contacts to settle.

`event.self.actor` and `event.opponent.actor` contain copied ID, definition,
owner and absolute player/opponent team. Main fighters have no actor field.
Use callback `memory` for per-controller cooldowns. Reacquire scoped actor
references each combat tick; retain plain IDs and spawn receipts instead.

Actors have independent health pools of 10 and expire after 1800 active simulation
frames. Dismissal, main-round teardown and surrender remove the pair and HUD.
Each also owns a `reactive_sparring` behavior with private ID/tick/hit state.
The ally adds 0.01 and rival 0.02 to outgoing native damage; both halve incoming
damage. The manifest declares the corresponding outgoing/incoming hit capabilities.
Actor spawn/end, hit-phase, damage and animation callbacks illustrate the reactive
host separately from the tactic's decision memory. Replacement state starts fresh.
This controlled core-content example is accepted in Unity with autonomous
bidirectional contact, repeated contact after later attack starts, unchanged main
health, native damage modifiers, pause and replacement-memory/teardown checks.
Actor-authored projectiles, arbitrary rigs/outfits, custom outcomes, exports,
raids and multiplayer remain open.
