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
observations; see [scripted companions](../../guides/scripted-actors/). Attach an
optional `behavior` for per-actor state, ticks, contact/damage and animation
callbacks, plus explicit spawn/end notifications. Actors are separate from
[weapon-child projectiles](../projectiles/).
An attached behavior can create and guide its own typed projectiles with
`combat.projectiles`; see [live projectiles](../projectiles/). Its query is scoped
to that actor and mod. Native equipment moves can also create weapon children.

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
| `behavior` | Optional behavior handle registered by this mod in the same transaction. Each spawned actor has its own instance state. |
| `parameters` | Optional typed configuration resolved against that behavior's schema, including defaults. Requires `behavior`; unknown fields or wrong types reject registration. |

An actor starts at its own maximum health. Equipment/tactic/rig must form a
usable native warrior; field validation does not guarantee every recovered rig
or outfit can run. Definition content participates in the mod compatibility
fingerprint. Live actors and IDs are round state and are not saved.

### Give each actor its own reactive logic

An attachment receives `on_actor_spawn`, `on_actor_end`, `on_tick`, the eight
hit/damage/block/critical callbacks, and `on_animation_start`/`on_animation_end`.
It does not receive main-fight/round, combo or style callbacks. Use spawn/end for
the actor's own lifetime; AI decisions remain in the warrior's separate tactic.
See the [callback reference](../combat-callbacks/#on_actor_spawn).

```lua
local ward = sf2.behaviors.register {
    id = "companion_ward",
    state = { lifetime = "round", fields = {
        ready = { type = "boolean", default = true },
    } },
    on_damage_resolving = function(self, fighter)
        if self.state.ready then
            fighter:scale_incoming_damage(0.5)
            self.state.ready = false
        end
    end,
}
-- Fragment: companion is the registered warrior above.
local guarded = sf2.actors.register { id = "guarded", character = companion, behavior = ward }
```

This example needs `combat.modify_hit`; registration itself needs
`content.register`. Spawn it using `combat.actors`. `round` and `fight` state are
supported and isolated per actor instance, even when actors share the same
behavior. State disappears with the actor; `saved` lifetime is rejected.

The callback's `fighter.source` is `"actor"`; `actor_id`, `actor_definition`,
`actor_owner` and absolute `side` describe its host. `fighter.actor` is a scoped
self reference supporting the actor methods below with `combat.actors`.
`fighter:snapshot().opponent` and `fighter.opponent` observe its current native
target, which may be another actor. Target-changing commands remain deferred to
safe native transitions. IDs and copied observations grant no command authority.

Existing fighter damage modifiers use their normal capabilities and native
timing. Self `change_health`, `move_by` and `play_move` use the actor's queued
commands and limits; they require the corresponding fighter capabilities
(`combat.change_life`, `combat.motion`, `combat.playback`). Alternatively use the
scoped `fighter.actor` methods with `combat.actors`. Opponent mutation additionally
requires `combat.target`. A lethal applied hit still notifies the actor host,
but cannot be reversed by queued healing. References expire after each callback.

Actor callbacks cannot spawn additional actors, query main-fighter
actor collections, change forms, set native combat flags, block player
controls, or claim round-outcome authority. Those host operations remain limited
to their existing supported contexts. Native equipment can still create children.
The complete [Scripted Actor Sparring example](../../guides/scripted-actors/)
demonstrates separate state, outgoing bonuses, incoming mitigation and cleanup.
[Ranged Companion Duel](../../guides/projectile-abilities/#give-a-companion-a-ranged-ability)
uses the same actor host to spawn and guide native children. Actor death,
expiration, dismissal or teardown cancels queued projectile births and removes
live children; callbacks cannot retain them past the caster's lifetime.

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
