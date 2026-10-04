---
title: Independent fighters
description: Summon, observe, guide and dismiss fighters owned by your mod.
---

Actors are additional native fighters, with independent health, equipment,
animation and optional native or Lua-directed AI. Register a warrior, describe an actor using
that warrior, then spawn it from a main fighter's combat callback. An actor can
be an ally or an adversary. Use ordinary Lua to guide it; no operation language
or native model access is needed.

Declare `content.register` for definitions and `combat.actors` for live operations
in `mod.toml`. Actors currently require an active **offline, non-raid** round
and a living main fighter. Local versus, PvP, raids and encounters containing
unmanaged extra fighters reject spawning. Definitions are not permission to
spawn during menu or round lifecycle callbacks.

AI uses the warrior's tactic and equipment. A tactic registered with
`on_decide` runs with private memory for each actor's native controller. Its
`event.self.actor` and `event.opponent.actor` provide copied identity/owner/team
observations; see [scripted companions](../../guides/scripted-actors/). A general
Lua combat behavior host is not attached to each actor yet: run your logic in the spawning fighter's
behavior, reacquire actor references on each callback, and store plain data keyed
by snapshot IDs. Actors are separate from [weapon-child projectiles](../projectiles/).
Typed projectile spawning/queries currently require a main fighter; an actor
does not expose those methods. Native equipment moves may still create their
ordinary weapon children.

## sf2.actors.register

Describe a fighter that your mod may summon. Definitions are immutable.

**Signature:** `sf2.actors.register(definition)`

**Returns:** An opaque actor definition handle. Invalid fields or references
raise a registration error and roll back the mod's registration transaction.

**When:** During script registration, before combat callbacks.

**Requires:** `content.register`.

```lua
local sf2 = require("sf2")
local companion = sf2.warriors.register {
    id = "companion",
    template = sf2.warriors.get_template("core:warrior-templates/default"),
    level = 1, tactic = "Standard", skeleton = "Skeleton",
    items = { sf2.items.get("core:items/weapon/WEAPON_KNIVES") },
}
local ally = sf2.actors.register {
    id = "ally", character = companion,
    team = "owner", ai = true, lifetime_frames = 900, max_health = 1,
}
```

| Field | Contract |
| --- | --- |
| `id` | Required local definition ID; becomes `<mod-id>:actors/<id>`. |
| `character` | Required registered warrior handle, visible through declared dependencies. |
| `team` | `"owner"` (default) or `"opponent"`, relative to the spawning main fighter. |
| `ai` | Boolean, default `true`. Uses the warrior's native or Lua-directed tactic; `false` allows manual Lua guidance. |
| `lifetime_frames` | Integer 1–36000, default 1800; simulation frames starting after native birth initialization. |
| `max_health` | Finite number 0.01–100, default 1. Native health units: 1 is a full normal fighter's health pool. |

An actor starts at its own maximum health. Equipment/tactic/rig must form a
usable native warrior; field validation does not guarantee every recovered rig
or outfit can run. Definition content participates in the mod compatibility
fingerprint. Live actors and IDs are round state and are not saved.

## fighter:spawn_actor

Queue an instance owned by this mod and this spawning fighter.

**Signature:** `fighter:spawn_actor(definition, x, y, z?)`

**Returns:** A retained observation receipt with `status = "queued"`, `"applied"`
or `"failed"`. Applied receipts have `actor_id`; failed receipts have `error`.
Editing a receipt does not change the request.

**When:** Active simulation combat callbacks. Native creation is deferred until
model/collision iteration finishes; entry and placement finish on a later step.

**Requires:** `combat.actors`; a definition registered by the calling mod.

```lua
-- Fragment: ally is the definition above; call once from on_tick.
local receipt = fighter:spawn_actor(ally, 180, 0)
-- Keep receipt as plain observation data and inspect it on a later callback.
```

Offsets are relative to the spawning fighter's center at creation, in native
arena units. Positive X is right; positive Y is down. Optional Z defaults to 0.
Each axis must be finite in −1000..1000. Capacity includes queued births:
**4 actors per mod across both main fighters, 8 per fight**. Rejections settle
as failed receipts; malformed Lua arguments throw. A failed birth never becomes
a live query result. Owner/session/body/round changes cancel pending births.

## fighter:actors

Reacquire this mod's live actors spawned by the callback's main fighter.

**Signature:** `fighter:actors()`

**Returns:** An ordered array of actor references, or `nil, error` if unavailable.
Pending births, retiring actors and other mods' actors are omitted.

**When:** Active simulation combat callbacks. References expire when that
callback returns, even if the native actor remains alive.

**Requires:** `combat.actors`.

```lua
local actors, error = fighter:actors()
if actors then
    for _, actor in ipairs(actors) do
        local view = actor:snapshot()
        if view then sf2.log.info(view.id .. " health=" .. view.health) end
    end
end
```

At most **32 combined actor/event queries per callback** are allowed. There is
no `fighter.opponent:actors` method. A behavior dispatched for the opponent may
use its own `fighter:actors()` according to its declared rule target.

## fighter:actor_events

Read copied lifecycle observations for this mod and spawning side.

**Signature:** `fighter:actor_events()`

**Returns:** An array of copied `{ sequence, actor_id, kind, frame }` tables, or
`nil, error`. The latest 64 events per mod and spawning side are retained.

**When:** Combat callbacks, including round/fight lifecycle callbacks while the
main fighter and script session are still current.

**Requires:** `combat.actors`.

```lua
-- last_sequence is plain Lua state, initialized to 0.
for _, event in ipairs(fighter:actor_events() or {}) do
    if event.sequence > last_sequence then
        sf2.log.info(event.kind .. ": " .. event.actor_id)
        last_sequence = event.sequence
    end
end
```

Kinds are `spawned`, `removed`, `expired`, `died`, `owner_changed`, `round_ended`
and `spawn_failed`. Sequence numbers increase within a fight; queries do not
consume events. Polling can miss old events after the 64-event window rolls over.
Removal is automatic on death, expiry, round teardown, owner body replacement
or loss of the spawning mod/session. This API is an observation log, not an
actor callback subscription.

## actor:snapshot

Read a copy of an actor's current native state.

**Signature:** `actor:snapshot()`

**Returns:** A copied snapshot, or `nil, error` after retirement. It contains the
usual [fighter snapshot](../fighter/#fightersnapshot) fields plus `id`,
`definition`, `team`, `target_id`, `age_frames` and `lifetime_frames`.

**When:** The active callback that obtained the actor reference.

**Requires:** `combat.actors`.

```lua
local view = actor:snapshot()
if view then
    sf2.log.info(view.id .. " team=" .. view.team .. " x=" .. view.position.x)
end
```

`team` is absolute `"player"` or `"opponent"`. `target_id` is `"player"`,
`"opponent"`, an actor ID, or absent. IDs are observations, not handles, and
are unique only within a fight. Store a snapshot or ID across callbacks and
reacquire the live actor; retaining the actor reference itself is an error.
Health/max-health use native pools; position is a copied center, not a transform.

## actor:move_by

Queue a displacement without replacing native animation or collision state.

**Signature:** `actor:move_by(x, y, z?)`

**Returns:** `true, nil` if queued, or `false, error` if the live command is
rejected. Nonfinite/out-of-range individual coordinates throw.

**When:** The active simulation callback that obtained the actor reference;
displacement applies after that step's native model/collision work.

**Requires:** `combat.actors`.

```lua
local ok, error = actor:move_by(10, 0)
if not ok then sf2.log.warn(error) end
```

Each individual and combined displacement must be within −100..100 per axis
per step. At most 32 nonzero movement requests per actor per step; zero motion
does not consume capacity. This is displacement, not velocity, navigation or
obstacle avoidance. Native arena constraints and subsequent animation still apply.

## actor:set_target

Choose a hostile target for this actor or return to automatic selection.

**Signature:** `actor:set_target(target)`

**Returns:** `true, nil` if accepted, or `false, error` for friendly, defeated,
self or unavailable targets. Forged/expired actor tables throw.

**When:** The active callback that obtained both actor references.

**Requires:** `combat.actors`.

```lua
local actors = fighter:actors() or {}
if #actors == 2 then actors[1]:set_target(actors[2]) end
-- Or actor:set_target("opponent") / actor:set_target("nearest").
```

Targets are `"player"`, `"opponent"`, `"nearest"`, or a live actor reference
obtained by the calling mod in this callback. Actor references from other mods
cannot be supplied. Automatic selection chooses the closest living hostile
fighter by horizontal center distance and retains the current target on ties.
Manual targets remain selected while valid. Changes wait until a current native
attack can safely finish; they do not restart it or reset collision phases.
Team enemy lists include all hostile roots and native weapon children, and
exclude friends. The canonical main fighters retain camera/profile identity.

## actor:remove

Dismiss an actor and retire its native children.

**Signature:** `actor:remove()`

**Returns:** `true, nil` if queued, or `false, error` if already unavailable.

**When:** The active simulation callback that obtained the actor reference.

**Requires:** `combat.actors`.

```lua
for _, actor in ipairs(fighter:actors() or {}) do actor:remove() end
```

The actor becomes unavailable immediately; native deletion is deferred to a safe
removal boundary. Pending actor move playback settles as failed. Query lifecycle
events on a later callback to observe `removed`.

## actor:change_health

Queue damage (negative) or healing (positive) on the actor's own health pool.

**Signature:** `actor:change_health(amount)`

**Returns:** `true, nil` if queued, or `false, error` if invalid or unavailable.

**When:** The active simulation callback that obtained the actor reference.

**Requires:** `combat.actors`.

```lua
local view = actor:snapshot()
if view then actor:change_health(-view.max_health) end
```

Individual and combined changes must be finite within plus/minus this actor's
`max_health` per step; at most 32 nonzero requests per actor per step. Native
health clamping applies. This command does not invent a contact attacker or
independent actor Lua hit callback. A defeated actor retires with a `died` event.

## actor:play_move

Queue a registered move on this actor's existing native rig and equipment.

**Signature:** `actor:play_move(move)`

**Returns:** A retained receipt with `status = "queued"`, `"applied"` or
`"failed"`; failures have `error`. Applied means native playback accepted the
move, not that its whole animation or attack succeeded.

**When:** The active simulation callback that obtained the actor reference;
playback applies after native collision work.

**Requires:** `combat.actors`; a registered move handle available on this actor.

```lua
-- Fragment: strike is a handle from sf2.moves.register.
local receipt = actor:play_move(strike)
-- Inspect receipt.status on a later callback; reacquire the actor separately.
```

Only one move request may be pending per actor per step. Invalid rig/equipment
availability rejects the receipt. Teardown/death/removal cancels queued playback.
The native move, locks, conditions and first-frame actions remain authoritative;
this method does not bypass rig compatibility or add a separate actor behavior host.

The shipped `example.actor-companions` mod supplies a playable summon/move/dismiss
HUD and a short-lived summon. Native acceptance is scoped to a controlled core
Skeleton/Standard tactic encounter. Arbitrary rigs, custom outfit combinations,
actor-specific Lua AI hosts, exports, raids and multiplayer need separate work.
