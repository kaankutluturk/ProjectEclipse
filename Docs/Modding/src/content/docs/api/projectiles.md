---
title: Live projectiles
description: Observe and guide your mod's native child projectiles with ordinary Lua.
---

A **projectile reference** is a callback-scoped handle to a native child created
by typed `timeline.projectile` (or graph `create_projectile`). Its owner is the
mod declaring that action, even when `start_move` belongs to a dependency.
Names do not grant ownership. Vanilla children and legacy `create_player` actions
are not exposed. Queries return only this mod's children rooted in the callback's
main fighter. There is no opponent query.

Declare `combat.projectiles` in `mod.toml`. Operations require a living main
fighter in an active offline round. Local versus, title sparring, PvP and network
raids are excluded; supported offline mod raids use the same guard. Fight/round
begin and end callbacks reject operations. Reacquire each callback; retain copied
snapshots and IDs for Lua state. IDs are strings unique within a fight, not save
identifiers. These references never expose native models or arbitrary actors.

Native contact callbacks supply a copied
[`event.attack`](../combat-callbacks/#identify-the-attack-that-made-contact).
For tracked typed children, its `projectile_id` matches `snapshot().id` and
`projectile_owner` names the declaring mod. Use those observations to distinguish
your own projectile's hit from ordinary attacks or another mod's child. They stay
readable after deletion but grant no live handle, and do not change query ownership.

## fighter:projectiles

Query this mod's live typed projectiles for the callback's fighter.

**Signature:** `fighter:projectiles()`

**Returns:** A dense array in spawn order and `nil`, or `nil, error` when the
host is unavailable/ineligible. An empty array means no live owned children.
At most 32 queries per callback; exceeding that limit raises an error.

**When:** Active simulation callbacks, including `on_tick`. Removing children
are excluded. Reads see native state before queued commands apply.

**Requires:** `combat.projectiles`.

```lua
on_tick = function(self, fighter)
    local children, error = fighter:projectiles()
    if not children then sf2.log.warn(error); return end
    for _, projectile in ipairs(children) do
        local view = projectile:snapshot()
        if view then sf2.log.info(view.name .. " at " .. view.position.x) end
    end
end
```

## projectile:snapshot

Copy a live child's position and lifetime observation.

**Signature:** `projectile:snapshot()`

**Returns:** `snapshot, nil`, or `nil, error` after expiry/removal. Fields: string
`id`, `name`, `animation_name` (native move or empty); `position = { x, y, z }`
in native fighter units; `age_frames` since creation and `lifetime_frames` limit.

**When:** Inside the acquiring callback. Copied observations remain usable
later; reference methods expire when the callback ends. Editing a snapshot does
not change the child.

**Requires:** `combat.projectiles`.

```lua
local view, error = projectile:snapshot()
if view then self.data.last_x = view.position.x
elseif error then sf2.log.warn(error) end
```

## projectile:move_by

Queue additive translation of the entire native rig, running keyframes and
interpolation buffers. Declared native velocity still applies; use zero velocity
when Lua supplies forward motion.

**Signature:** `projectile:move_by(x, y, z?)`

**Returns:** `true, nil` when queued, or `false, error` when rejected. Finite
numbers must be within -100..100 per axis; invalid arguments raise an error.
Omitted/nil `z` defaults to zero. Combined pending displacement is also limited
to 100 per axis per child per step, with at most 32 nonzero requests. Zero is a
successful no-op for a valid child.

**When:** Inside the acquiring simulation callback. Applies once after native
model/collision/animation processing, before round arbitration. Collision sees
that translated pose on the next step. Motion is discrete, without swept
collision; large steps can skip contact. Pause freezes commands and lifetime.
Ownership, session, root, round and membership are rechecked at application.
Accepted commands can be discarded after hit/expiry/retirement. Native constraints
can adjust the result. Later Lua errors do not roll back accepted commands.

**Requires:** `combat.projectiles`.

```lua
local view = projectile:snapshot()
if view then
    local dx = math.max(-10, math.min(10, target_x - view.position.x))
    local accepted, error = projectile:move_by(dx, 0)
    if not accepted then sf2.log.warn(error) end
end
```

## projectile:remove

Request native deletion and make this child unavailable immediately.

**Signature:** `projectile:remove()`

**Returns:** `true, nil` when accepted or `false, error` after expiry/removal or
host rejection. Repeated removal returns false. Pending motion is cancelled.
It does not synthesize a hit, damage or animation callback.

**When:** Inside the acquiring simulation callback. Native deletion uses the
normal deletion pass to preserve list iteration. Round end, surrender, form
transitions, session changes and owner/fighter retirement also retire children.
Native strike/delete actions remain effective.

**Requires:** `combat.projectiles`.

```lua
local view = projectile:snapshot()
if view and math.abs(view.position.x - launch_x) < 12 then
    local removed, error = projectile:remove()
    if not removed then sf2.log.warn(error) end
end
```

## Spawn and lifetime limits

Optional `projectile.lifetime_frames` is an integer in 1..600, default 180.
Age starts at creation, including launch animation time, and advances only with
fight simulation. At expiry the host retires the child before combat callbacks;
native deletion occurs at the normal deletion pass. Native graphs can delete
earlier. Nondefault lifetime affects the content compatibility fingerprint;
existing default fingerprints are preserved. Children do not survive rounds or
save reloads.

At most 16 live typed children per mod across both fighters and 64 per fight.
Extra scheduled spawns are rejected before construction with a
`[ModProjectiles]` game-log warning. Pending native deletion may retain a capacity
slot until retirement. These limits apply without `combat.projectiles` too; the
capability grants observation/control, not unbounded creation.

The complete [Return Dart example](https://github.com/dawc17/ProjectEclipse/tree/main/Mods/example.return-dart)
turns after 28 guided ticks and returns to its recorded launch X. Native attack
edges still handle contact. This is not a hand catch, rig rotation API, bounce
solver or arbitrary actor platform.
