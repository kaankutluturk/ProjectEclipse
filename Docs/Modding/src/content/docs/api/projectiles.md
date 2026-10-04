---
title: Live projectiles
description: Observe and guide your mod's native child projectiles with ordinary Lua.
---

A **projectile reference** is a callback-scoped handle to a native child created
by typed `timeline.projectile` (or graph `create_projectile`), or a registered
projectile spawned with `fighter:spawn_projectile`. Its owner is the mod declaring
the action or definition, even when `start_move` belongs to a dependency.
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

## sf2.projectiles.register

Declare a reusable native child-weapon definition. Ordinary Lua decides when,
where and how often to spawn it; a main-fighter cast move is optional.

**Signature:** `sf2.projectiles.register(definition)`

**Returns:** An opaque projectile **definition handle**, owned by this mod.
This reusable recipe is distinct from a callback-scoped live projectile
reference. Invalid fields, duplicate IDs or missing/inaccessible references fail
transactional registration; a failed mod adds no definitions.

**When:** Mod startup, before registration commits. Required: local `id`, native
actor `name`, `core_skeleton`, a registered `start_move`, and exactly one of
`item` or `copy_parent_type`. Optional `lifetime_frames` defaults to 180.

**Requires:** `content.register`. Spawning separately needs `combat.projectiles`.

```lua
-- `flight` is a previously registered compatible child move.
local dart = sf2.projectiles.register {
    id = "dart", name = sf2.mod.id .. ".dart",
    core_skeleton = "SkeletonMissile",
    item = sf2.items.get("core:items/ranged/RANGED_C2_Z2_MONK_SHURIKEN"),
    start_move = flight, lifetime_frames = 120,
}
```

Definition IDs become `<mod-id>:projectiles/<local-id>`. `name` and
`core_skeleton` are case-sensitive symbols of 1–128 letters/digits, underscores,
hyphens or dots, without surrounding whitespace. Prefix names with your mod ID
and match any actor conditions on the flight move. Symbols alone do not validate
native asset existence or rig compatibility.

| Field | Contract |
| --- | --- |
| `id` | Required local ID, unique within this mod's projectile category. |
| `name` | Required native actor name. Names do not grant ownership. |
| `core_skeleton` | Required existing native rig, such as `SkeletonMissile`. |
| `start_move` | Required registered move handle. Its binary, rig, equipment locks and actions must work together. Native birth initializes this move. |
| `item` | Weapon, ranged or magic item handle; placed in the child's native **Weapon** slot. Mutually exclusive with `copy_parent_type`. |
| `copy_parent_type` | `"Weapon"`, `"Ranged"` or `"Magic"`; copies that parent slot into the child's Weapon slot. Missing/incompatible equipment can fail at runtime. Mutually exclusive with `item`. |
| `lifetime_frames` | Optional integer 1–600, default 180. Simulation-frame lifetime starts when birth initialization succeeds. |

`core_start_animation` is not accepted here; an explicit move handle is required.
Dependency references must be accessible through declared dependencies. There is
no definition lookup, opponent spawn method or cross-mod live control.
Definitions participate in the content compatibility fingerprint. Live children,
queued work and receipts are temporary fight state, not saved progress.

## fighter:spawn_projectile

Queue one native child from a definition registered by this mod.

**Signature:** `fighter:spawn_projectile(definition, x, y, z?)`

**Returns:** A retainable receipt with `status = "queued"`, `"applied"` or
`"failed"`. Success supplies string `projectile_id`, matching the child's
snapshot and copied attack source; failure supplies string `error` and no ID.
A queued receipt is not a live handle or proof of birth. Invalid handles/types,
nonfinite offsets, offsets outside -1000..1000 per axis or expired callback
references raise an error. Host/eligibility/capacity rejection immediately fails
the receipt.

**When:** Active simulation callbacks such as `on_tick`, on the callback's own
main fighter. Offsets are relative to its weighted native center of mass at **application**, not
request time. They use world axes: positive Y points down; X is not automatically
mirrored. Omitted/nil Z is zero. Creation runs after native collisions and before
animation selection. Native birth starts the move, then the entire rig and
running keyframes are positioned before the first collision pass on the next
step. Native constraints that prevent placement fail the receipt; subsequent
animation can move the weighted center even with zero declared velocity. Applied means initialization/positioning succeeded, not that the child hit
anything or remains alive later.

**Requires:** `combat.projectiles`. Register the definition with `content.register`.
No `combat.animation` is needed, and spawning does not request a caster move.

```lua
local pending
-- `dart` is the definition above. Fragment of a behavior:
on_tick = function(self, fighter)
    if not pending then
        pending = fighter:spawn_projectile(dart, 40, 0)
    elseif pending.status == "applied" then
        local children = fighter:projectiles()
        if children then for _, projectile in ipairs(children) do
            local view = projectile:snapshot()
            if view and view.id == pending.projectile_id then projectile:move_by(10, 0) end
        end end
    elseif pending.status == "failed" then sf2.log.warn(pending.error) end
end
```

Accepted queues reserve shared capacity: **16 children/requests per mod across
both fighters, 64 per fight**, including timeline spawns. Initializing/removing
children occupy capacity until retirement. Each request settles independently;
a loop can produce a partial burst. Pause freezes queued work and lifetime.
Round/fight exit, death, form replacement, owner/session loss or native birth
failure cancels pending initialization and settles receipts. Failure removes any
registered child through native cleanup. Queries exclude direct children still
initializing. Birth actions can remove the child immediately; that receipt fails.
First-frame actions run at the original birth pose before final placement, so
use later frames for position-sensitive effects.

Reacquire live references by `fighter:projectiles()` in a later callback. Use
[Scripted Burst](https://github.com/dawc17/ProjectEclipse/tree/main/Mods/example.scripted-burst)
for a complete three-projectile pattern and HUD. Native weapon children inherit
root combat context; they have no independent fighter health, AI, teams or
solid/swept collision. Controlled native direct-spawn acceptance checks receipts,
three offsets, rendering, contact damage and teardown. The complete example also has managed Lua/HUD, trajectory, cooldown, partial-
burst and round-reset checks with controlled native models, plus editor checks.
Those checks do not prove every loadout/mode or an uncontrolled playtest.

## fighter:projectiles

Query this mod's live typed projectiles for the callback's fighter.

**Signature:** `fighter:projectiles()`

**Returns:** A dense array in spawn order and `nil`, or `nil, error` when the
host is unavailable/ineligible. An empty array means no live owned children.
At most 32 queries per callback; exceeding that limit raises an error.

**When:** Active simulation callbacks, including `on_tick`. Removing and still-initializing direct children
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
