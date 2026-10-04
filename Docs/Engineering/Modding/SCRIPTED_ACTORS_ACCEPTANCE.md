# Lua-directed independent fighters

2026-10-04. This closes a concrete public-content integration gap: creators can
summon a reusable warrior with a Lua tactic and prove its independent decisions
produce native damage. It advances E1/E2/E5; G01–G14/E1–E8 remain open.

## Source and API

- `ModActorIdentity` in shared `Runtime/Modding/ModActors.cs` is detached, immutable
  provenance: instance ID, definition ID, owner mod and absolute player/opponent
  team. `ModFighterSnapshot.Actor` is optional and defaults to null.
- `FightActors.CaptureEclipseActorIdentity` identifies registered, initialized
  roots only. Removing/pending roots and ordinary main fighters omit metadata.
  The fight's combat snapshot adapter and `ModRuntimeP1D.AiSnapshot` carry it
  alongside health, position and current animation. No native state is written.
- `MoonSharpScriptRuntime.FighterSnapshotTable` allocates a fresh actor table for
  each Lua observation. Changes to its fields cannot change ownership/team/native
  state or manufacture a command reference. Actor snapshots inherit the same data.
- The existing `sf2.tactics.register { on_decide = ... }` uses weak native-controller
  identity and per-tactic memory. Each actor's native controller receives its own
  state. Current native root/target animation bindings and eligibility/throttle
  remain authoritative. Nil/wait/action and error fallback contracts are unchanged.
- `Mods/example.scripted-actors` registers one Lua tactic, a warrior, two actor
  definitions and a summon/dismiss HUD. It approaches a hostile actor, chooses
  eligible punch/kick candidates and voluntarily waits between attacks. Missing
  actor targets request wait. The sample uses no play_move or direct health command.
  The spawning behavior targets the pair explicitly so a closer main fighter
  does not silently replace its partner. The original duel may still interfere.
  VS Code's `templates/scripted-actors` mirrors all authored files.

This adds observations and an end-to-end creation example; it does not add a
separate actor combat behavior host, generic instruction DSL or native Model access.
Source/assets stay project-owned; existing Unity identity is preserved.

## Native acceptance

`Tools/Tests/Combat/TestScriptedActorsUnity.ps1` prepares a marked full-game
Unity 6000.6.0f1 fixture, isolated mod root and unique fresh post-tutorial product/
profile. Main control/AI and spacing are controlled. The actual public HUD
summons actors; fixture commands do not select or play their attacks.

The accepted run passes **30 checks**, log
`Temp/FighterPlaybackUnity-d997a2c104cd4564b9767ae6eb4b68d8/validation-c820f4092ee04a5995824a22521f196d.log`.
Both roots use independent AI/health, owner/team metadata and actor IDs. Lua
selects actual native ShortUpwardElbowStrike/HighKneeUp candidates and
DoubleStepForward/BackHandflip movements. Native attack-start listeners reject
an attack not previously chosen by the corresponding actor's Lua policy.

First observed contact leaves both actor pools at **9.965178**. Later contact,
required to follow a subsequent attack start whose target health is captured at
that start, leaves pools at **9.86071 / 9.930355**, with **5 / 5** observed starts.
Each damaged root's native StrikeResult identifies its hostile peer as source.
The main player/opponent pools stay unchanged. Pause freezes simulation/age;
dismissal restores canonical targets; replacement IDs a3/a4 receive new Lua
memory, and surrender removes actors and the owned HUD. Fresh result timestamps
and Unity exit 0 are required. No explicit playback is permitted by this fixture.

An intermediate explicit-target run failed: after crossing the chosen partner,
an actor could keep advancing in its previous facing direction. The final Lua
policy chooses Forward or Back from observed position and facing, prefers the
shortest eligible non-jumping movement, and starts attack selection within 95
units. The passing run above includes explicit pair targets and this correction.
Movement candidates still come from native eligibility and may be double steps
or backward flips; the example does not guarantee a single-step animation.

The original Actor Companions regression is also rerun after the snapshot change.
It passes **42 checks**, log
`Temp/FighterPlaybackUnity-d997a2c104cd4564b9767ae6eb4b68d8/validation-7e136703561c45e3b2fce574f9b53f26.log`.
Its separate contract/command/lifecycle checks do not replace autonomous evidence.

## Managed, creator tools and public docs

`TestModAi.ps1` passes **14** controlled native eligibility checks, **56** production
adapter/nominal timing/control mapping checks, **33** real parser/67-node clip
metadata checks and **95** actual Lua decisions/copy/memory/action lifetime/
fallback/instruction-budget checks. It uses ignored portable Windows-reference
builds where necessary; original AI/charge fixtures may live under ArchivedMods.
The copied actor table is edited by Lua and sampled again to prove isolation.
Ordinary main snapshots are checked to omit it.
The actual public sparring policy is also executed with controlled shortlists:
approach on both sides and facings, partner crossing, shortest non-looping attack,
cooldown boundary, fresh-controller state, missing/main/friendly targets and
empty/looping-only actions. These are policy regressions, not native contact tests.

All four managed assemblies compile with Unity 6.6 references. The authored
editor schema, generated API JSON/Lua definitions and editor guide document
`FighterSnapshot.actor`/`ActorIdentity`. All **55** project tests pass, and actual LuaLS and VS Code
completion check the fields. The complete starter has no LuaLS diagnostics.
The VS Code native integration passes **19** checks, including actor provenance. The wiki
includes a public scripted companion guide, updated actor/fighter/tactic reference,
examples and sidebar. Reference coverage remains 244 functions/aliases/callbacks.
Build/types/search pass (zero type errors/warnings/hints), with **6786** checked
links/assets across **63** pages. The existing duplicate-404 warning remains.
Default test runtime/CLI paths were unavailable or npm-mangled; using the installed
LuaLS binary and launching the VS Code runner directly with its actual executable
completed the real integrations. Those failures are not counted as acceptance.

## Remaining scope

Core Skeleton/knife content, two actor roots and controlled Campaign spacing/input
bound this evidence. It does not accept every rig, outfit, weapon, arena, tactic,
physical input, exported build, multiplayer/rollback, raid or performance case.
General actor hit/damage/lifecycle behavior hosts, actor-authored projectiles,
custom victory and persistent actor worlds remain open. Other owner's observations
are copied data, not permissions. Existing unrelated native startup/title warnings
remain; the logs are not claimed error-free.
