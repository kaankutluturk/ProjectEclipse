---
title: Create a projectile ability
description: Connect an owned cast, native child actor, flight, contact attack and expiry.
---

The complete [Arc Dart example](https://github.com/dawc17/ProjectEclipse/tree/main/Mods/example.arc-dart)
adds a rechargeable HUD button to Act I Tournament stage 3. It throws a native
child projectile even with no ranged weapon equipped. Copy its complete folder,
enable it in **Mods**, choose **Apply & Restart**, and enter that fight.
Both normal and Eclipse entries are patched; verification limits are recorded below.

It needs `content.register`, `content.patch`, `combat.animation`, `ui.create`,
and a `core` dependency. Three compatible native animation binaries are shipped.
Renaming arbitrary files does not convert animations. Review
[Activate an authored move](../move-abilities/) before adapting the HUD behavior.

## Connect cast, launch and flight

The **cast** runs on the main fighter. The **launch** positions the child during
the throw. The **flight** moves it and supplies a contact attack. Register flight
first, launch second and cast last, so every action receives an existing typed
move handle.

The example's flight uses the four-node `SkeletonMissile` rig and Monk shuriken
model. These fields belong inside its complete `sf2.moves.register` definition:

```lua
velocity = { x = 18 },
intervals = { { type = "Attack", attack = {
    edges = { "RANGED-Edge1", "RANGED-Edge2" },
    damage = 0.075,
    damage_terms = { RangedDamage = 0, UnarmedDamage = -10 },
    hit = "High", impulse = { x = 450 }, id = 441,
    options = { ignores_block = true, no_critical = true,
        ignores_invulnerable = { "Evade", "Dash" } },
} } },
timeline = {
    [1] = { sound = "snd_shuriken_fly" },
    strike = { delete_actor = "Me" },
    animation_end = { delete_actor = "Me" },
},
```

The native collision and damage pipeline evaluates the attack. Native attack
attributes, defense, reactions and incoming modifiers matter; this example does
not directly change health. Its declared options intentionally bypass block and
disable critical hits. Damage terms select the strongest adjusted native
attribute contribution; they are not summed. See
[attack fields](../../api/moves-and-tactics/#shared-move-fields).

Flight has no selection events and does not loop. Animation-end deletion bounds
a miss's lifetime; strike deletion ends a contact. `Me` means the actor running
the move, so put these actions on the child. Deletion uses the fight's normal
removal queue. The core missile tags also enable normal native cleanup such as
arena exit and an interrupted launch. Inspect inherited behavior when choosing
templates.

The launch's native `ranged_light_weapon` clip contains the missile's throwing-hand
coordinates. It aligns to its parent's animation origin, then transitions:

```lua
-- Inside the complete launch definition, after registering flight:
timeline = { animation_end = { play_animation = flight, player = "Me" } },
```

Flight retains its own `Ranged-Node2_1` point on entry. Arbitrary cross-rig node
alignment needs separate validation: structural validation of a point is not
proof of native node-index compatibility.

The cast creates the named child at a sample index:

```lua
local dart_item = sf2.items.get("core:items/ranged/RANGED_C2_Z2_MONK_SHURIKEN")
-- Inside the complete cast definition, after registering launch:
timeline = { [5] = { projectile = {
    name = sf2.mod.id .. ".dart", core_skeleton = "SkeletonMissile",
    item = dart_item, start_move = launch,
} } },
```

`item` copies resolved equipment into the child's Weapon slot, without equipping
the main fighter or granting inventory. Alternatively, `copy_parent_type` uses
the fighter's current Weapon, Ranged or Magic item. Supply exactly one source.
Match subtype, skeleton, node order and attacking edges. The example's actor-name
condition isolates its child moves from unrelated actor names.

## Activate and observe

The UI callback stores a boolean. `on_tick` reads a fresh snapshot and calls
`fighter:play_move(cast)`. An `applied` receipt confirms **cast startup**. The
spawn action is later and can be skipped if the cast is interrupted. Cooldown
is ordinary Lua policy measured in active simulation frames, so pause holds it.

`on_animation_start` with the exact owned flight name increments `Flights`; its
`target` is `other`. This relationship provides no live child handle or ownership
identity. `on_damage_dealt` increments `Hits` for all damage attributed to this
main fighter, including ordinary attacks. It is not a projectile-specific contact
callback. Dedicated retained actor handles, steering and child-instance queries
are not supplied by these APIs.

Round-local behavior state resets counters and cooldown. Round/fight end closes
the HUD and clears intent. Closing the HUD does not delete native children:
their move graphs and fight own their lifetime. Never save receipts, closures,
fighter references or UI handles.

## Change one thing and test it

Try lowering `velocity.x`, changing the damage multiplier or changing cooldown
from 180 frames. Keep miss expiry. Test hit, miss, pause, interruption, both
facing directions, walls, round changes, surrender and competing playback mods.

Check actual rig edges before changing attack parts. Faster velocity may change
native contact behavior: arbitrary swept physics, bouncing, homing, summons and
extra main fighters are not implied. These snippets explain the graph; use the
complete example to run it.

## What has been verified

The complete example passed a Unity 6.6 Campaign Tournament 3 run with visible
flight, native contact damage, caster attribution, pause/resume, deletion on hit,
expiry on miss and cooldown. A captured frame was visually inspected. Input, AI
and spacing were controlled. Reverse facing, interrupted casts, surrender with a
live child, all arenas and exported players still need game acceptance.

Managed fixtures separately exercise the shipped Lua, registration, playback
receipts, round reset and native damage attribution. Native child attacks report
their root main fighter as the Lua attacker; native damage still uses the child's
actual equipment and animation. Animation actions raised while another queued
action runs are deferred to the next selection pass.
