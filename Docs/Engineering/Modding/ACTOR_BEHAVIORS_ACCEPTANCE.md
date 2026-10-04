# Reactive independent fighter behaviors

2026-10-04. Owned actor definitions can attach reusable Lua behavior instances.
This advances E1/E2/E5 beyond AI decisions: each actor can react to native events
and maintain typed state. G01–G14/E1–E8 remain open.

## Source and public contract

- `Runtime/Modding/ModActors.cs` adds optional own-transaction behavior identity
  and resolved typed parameters to immutable definitions. Registration rejects
  forged handles, unknown/wrong parameters and saved-state attachments.
- `ModSaveData.cs` appends sorted attachment/parameter data only when attachments
  exist. Definitions without behaviors retain their previous canonical actor block.
  Other behavior/schema hashing is reused.
- `ModScripting.cs` appends ActorSpawn/ActorEnd values after existing event numbers
  and adds an optional self-actor source to the instance wrapper. Runtime code
  remains independent of recovered Model types.
- `MoonSharpScriptRuntimeActors.cs` parses strict handles and parameters. The main
  runtime supplies `fighter.actor` using existing callback budget/expiry checks.
  `MoonSharpScriptRuntimeP2.cs` exposes the callbacks and defensively rejects
  saved actor state during invocation too.
- `FightActors.cs` owns a transient instance node per initialized actor, dispatching
  through the production script session with source, absolute side, IDs and epoch.
  Spawn follows settled placement/applied receipt; end runs once before retirement
  while the owner session remains active. Failed births omit callbacks. Defeated
  or removing bodies reject commands. Retirement clears shields/status icons even
  if callback execution fails.
- Narrow `Fight.cs` seams route actual attacker/victim roots to hit phases,
  outgoing/incoming modifiers and resolved damage/block/critical callbacks.
  Canonical player-before-opponent ordering is preserved. Actor animation
  relationships are captured before handlers and use the existing bounded queue;
  removed recipients and changed rounds are skipped. There is no actor observer
  list allocation when no owned actors exist.
- Fighter snapshots/opponent operations use the current native target instead of
  assuming the other main fighter. Actor self health/motion/playback uses bounded
  queued commands. Normal fighter capabilities still apply; actor-reference
  methods require combat.actors.

Attachments receive spawn/end, simulation ticks, eight contact/damage/block/
critical categories and animation start/end. They omit main fight/round, combo
and style events. State is private per spawn, fight/round only, and not persisted.
Actor callbacks do not acquire round-result authority or unsupported form/flag/
control/world/projectile operations.

## Public creation example

Scripted Actor Sparring 1.1.0 retains its actual Lua tactic/controller memory and
adds `reactive_sparring` to both actors. The ally adds 0.01 outgoing damage and
rival 0.02; both halve incoming damage. ID/tick/hit state is private per actor.
Spawn checks defaults/self identity; replacement state starts fresh. The VS Code
starter mirrors all authored files. No private fixture brain or explicit playback
selects attacks in this acceptance scenario.

## Verification

Isolated Unity 6000.6.0f1 acceptance passes **57 checks** with actual public Lua,
HUD commands and two core Skeleton/knife actors. It proves spawn/tick/self-animation,
both hit-phase perspectives, outgoing bonus visible at incoming resolving,
mitigation matching actual native health loss, later-start repeated contacts both
ways, unchanged main life, pause, dismissal, fresh AI/behavior instances and
exactly-once terminal callbacks. Final log:
`Temp/FighterPlaybackUnity-d997a2c104cd4564b9767ae6eb4b68d8/validation-cb21c6725e4a4b1d826ace1863624849.log`.
First contact pools were 9.972589 / 9.977589; later contacts reached
9.890354 / 9.955177 after five attack starts each. For the rival's hit, outgoing
0.03482202068 plus 0.02 reached resolving as 0.05482202023; the applied loss was
0.02741146088 after scaling by 0.5. The ally's corresponding loss was
0.02241134644. Controlled floating-point tolerance is 0.000005.

Actor Companions passes **42** contract/lifecycle regression checks, log
`Temp/FighterPlaybackUnity-d997a2c104cd4564b9767ae6eb4b68d8/validation-941bca1532334d2fab19bd75c4ad2667.log`.
Return Dart checks canonical main/projectile contact and ownership (**45** native
checks), final log
`Temp/FighterPlaybackUnity-d997a2c104cd4564b9767ae6eb4b68d8/validation-7cc5ab63cedc48699ed6dd02fbb9bfcd.log`.
Native cast contact reduced enemy health from 1 to 0.9836771; the direct queued
burst reduced it again to 0.9673543 without caster playback.

`TestActorDefinitions.ps1` passes **81** production Lua checks: registration,
defaults/bounds, typed parameters, rollback/fingerprints, separate/replacement state,
copied snapshots, expired retained self-reference reads/commands and failed-state
rollback. Body/clock inputs are controlled. `TestModAi.ps1` retains 14 eligibility,
56 adapter, 33 real parser and 95 actual Lua policy checks. All four managed
projects compile with remapped Unity 6.6 Windows references.

Editor schema/generated contracts/starter cover the same fields/callbacks. **56**
project tests include direct/aliased self-reference capabilities and terminal
timing. Actual LuaLS completes spawn/end context/self methods and accepts the
complete starter without diagnostics. Real VS Code passes **20** integration checks.
Actor/fighter/behavior/callback references, guide and examples change together;
reference coverage is **246**. The final wiki build passes Astro diagnostics
(zero errors/warnings/hints), builds 63 pages/search and checks **6805** local
links/assets with fragments and GitHub Pages paths. The existing duplicate 404
route build warning remains. Editor generation/check and all 56 project tests
were rerun against the final starter.

An initial native run rejected unsupported min/max fields mistakenly authored in
the example schemas; they were removed. Another reached correct damage and four
end callbacks but double-counted stored full/short keys in its final assertion;
a delivery counter corrected it. Neither failure counts as acceptance. LuaLS
required an explicit assert on the nullable snapshot; its completion test now
recognizes function labels with signatures, as the editor actually returns them.
Managed harness corrections provide the required tick clock and use failed
invocations for expiry checks because pcall is absent from the hard sandbox.

## Limits and remaining creator scope

Native evidence is controlled Campaign/core content, two roots, input/spacing and
a fresh post-tutorial profile. It does not force every block, critical, lethal
native hit, main/actor interaction, animation-end, simultaneous pack or owner-disable
case. Routing exists; further scenario acceptance remains. Actor-authored
projectiles, custom rigs/outfits, forms, alternate outcomes, persistent worlds,
exports, raids, multiplayer/rollback and sustained performance remain open.
Existing unrelated startup/title warnings are not claimed fixed.
