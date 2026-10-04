---
title: Script companions with Lua AI
description: Give independent fighters their own decisions and cooldowns using native actions and copied actor observations.
---

The [Scripted Actor Sparring mod](https://github.com/dawc17/ProjectEclipse/tree/main/Mods/example.scripted-actors)
is a complete example. Enable it, play Act I Tournament stage 3, then use its HUD
to summon or dismiss a pair. The two fighters use the same Lua policy, each with
its own cooldown memory. They choose attacks themselves; the HUD never calls
`actor:play_move`. The spawning behavior explicitly targets the pair at each other, so proximity
to a main fighter does not change its chosen sparring partner. Native animations
still control when a target switch can happen. The original duel continues and
other fighters may interrupt the pair. The example also ships as the VS Code
`scripted-actors` starter.

The example approaches from either side using the target's position and its own
observed facing. `Forward` and `Back` describe controls relative to that facing;
after crossing a partner, blindly choosing `Forward` can move away from it. The
policy chooses the shortest eligible movement without an `Up` input, then a
short non-looping punch or kick within 95 game units. Depending on the native
shortlist, movement can include a double step or backward flip. Adapt these
distances and choices to your character's equipment and animation set.

You need `content.register` to define a tactic/warrior/actor, `combat.actors` to
spawn or query live actors, `content.patch` to add the behavior to an existing
fight, and `ui.create` for the example HUD. Actor spawning requires an active
offline, non-raid round and a living main fighter. Follow the
[independent fighter reference](../../api/actors/) for capacities and lifetimes.

The complete example also declares `combat.modify_outgoing_hit` and
`combat.modify_hit`. Its actor behavior adds 0.01 outgoing damage for the ally,
0.02 for the rival, then halves each actor's incoming damage. These values are
native health units. The original main fighters do not receive this attachment.

## Separate a character from its decisions

The native rig and equipment supply animation, eligible moves, collision and hit
reactions. A registered tactic supplies Lua decisions. Return a candidate from
this decision's `event.actions`, `"wait"` to request no action, or `nil` to allow
native fallback. You cannot force an unavailable move by inventing a table.

This small loading-time example selects a kick at most once per 90 simulation
frames. It deliberately waits when its target is a main fighter:

```lua
local sf2 = require("sf2")
local brain = sf2.tactics.register {
    id = "companion_ai", template = "Standard",
    on_decide = function(memory, event)
        local self = event.self and event.self.actor
        local target = event.opponent and event.opponent.actor
        if not self or not target or self.team == target.team then return "wait" end
        if event.frame < (memory.ready or 0) then return "wait" end
        for _, action in ipairs(event.actions) do
            if action.type == "attack" then
                for _, key in ipairs(action.inputs) do
                    if key.control == "Kick" and key.press == "tap" then
                        memory.ready = event.frame + 90
                        return action
                    end
                end
            end
        end
        return "wait"
    end,
}
local character = sf2.warriors.register {
    id = "companion", level = 1, skeleton = "Skeleton",
    template = sf2.warriors.get_template("core:warrior-templates/default"),
    tactic = sf2.tactics.name(brain),
    items = { sf2.items.get("core:items/weapon/WEAPON_KNIVES") },
}
local ally = sf2.actors.register { id = "ally", character = character }
local rival = sf2.actors.register { id = "rival", character = character, team = "opponent" }
```

In an active combat tick, summon through its scoped `fighter` reference. Retain
receipts, not the reference:

```lua
-- Fragment inside an on_tick callback; only run once when the player requests it.
local receipts = {
    fighter:spawn_actor(ally, 180, 0),
    fighter:spawn_actor(rival, 260, 0),
}
-- On later ticks, receipts[i].status becomes applied or failed.
```

Use the complete example for command queuing, retained receipts, HUD updates and
round/fight cleanup. Do not put this fragment unconditionally in every tick.

## Keep controller memory independent

`memory` belongs to this native fighter controller and this tactic. It survives
its decisions, but is not saved and is replaced with the controller. Script-scope
variables are shared across every fighter using this script. Store cooldowns,
combo phases and per-fighter counters in `memory`; use owned saved-state APIs only
when you intentionally need persistence across fights.

AI observations contain optional `actor = { id, definition, owner, team }` tables.
`id` is an instance ID within this fight. `definition` identifies the registered
actor type. `owner` identifies its mod. `team` is absolute player/opponent,
independent of the observer. Main roots omit `actor`. An actor can target a main
fighter, so always check for a missing target identity. These fields are copied;
editing them does not provide actor commands or change native data.

Native eligibility, interruption and a six-frame decision throttle still apply.
A callback failure disables that policy for that controller/tactic and uses
native fallback. Another actor retains its own memory and policy. Clip duration
is nominal metadata, not a guaranteed recovery time. See
[tactics and action observations](../../api/moves-and-tactics/#sf2tacticsregister).

## What has been verified

The complete example has actual Lua/HUD/native Unity acceptance with two
Skeleton/knife actors: separate IDs/memory/health, Lua-selected native attacks,
contact in both directions, and repeated damage after subsequent attack starts.
The main fighters' health stays unchanged. Pause, dismissal, replacement identity
and controller memory, and surrender cleanup are also checked. Additional
managed tests check the public policy's partner-crossing movement and cooldowns,
copied identity data, stale actions, memory isolation, native fallback and
instruction limits. Editor checks validate the starter and
complete identity fields; those checks are not game playtests.

## Add reactive abilities to each instance

AI chooses a move; an actor's optional `behavior` reacts to what happens. They
have separate memory/state. The example's `reactive_sparring` behavior uses a
typed `bonus` parameter and private `id`, `ticks` and `hits` state for each actor.
`on_actor_spawn` initializes it, `on_tick` advances it, outgoing and incoming
damage callbacks change the current native hit, and `on_actor_end` finishes it.
Replacement actors begin with defaults even when their definition is unchanged.

Attach the same behavior to multiple definitions with different `parameters`.
Their state remains separate. Register the behavior before the actor, in the
same mod transaction. `round`/`fight` state works; `saved` actor state is rejected
because these bodies belong to the current round.

Use `fighter.actor` for callback-scoped self commands with `combat.actors`.
Use `fighter:snapshot()` to observe this actor and its current target, and copied
`fighter.actor_id`/`actor_definition`/`actor_owner` for bookkeeping. End callbacks
retain those copied fields and an `actor_end_reason`, but retiring actor commands
are unavailable. [Actor behaviors](../../api/actors/#give-each-actor-its-own-reactive-logic)
list supported callbacks and operation limits.

The complete behavior has native acceptance for spawn/tick/animation events,
both hit-phase perspectives, resolved damage in each direction, outgoing bonuses
and incoming scaling matching actual health loss, fresh replacement state and
exactly-once end callbacks on dismissal/surrender. Managed checks additionally
exercise expired self references, copied observations and failed-state rollback.

For actor-authored projectiles, continue with
[companion ranged abilities](../projectile-abilities/#give-a-companion-a-ranged-ability).
This is a reusable autonomous and reactive fighter foundation. Custom rigs/outfits, every arena/weapon pairing, custom
victory conditions, exported players, raids and multiplayer need separate work.
