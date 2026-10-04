# Installed authored fighter acceptance

2026-10-04. Authored Fighter Lab is a reproducible creator example combining
owned body/skin geometry and an original animation with the existing public API.
It advances character authoring acceptance; it does not close the broader
Minecraft-style mod-engine vision or arbitrary-rig/importer acceptance.

## Source and public API

`Mods/example.authored-fighter/` owns `assets/models/body.xml`, its connected
`sash.xml`, and `assets/animations/strike.bytes` with a matching rig sidecar.
The body preserves the canonical ordered 67-point rig and its constraints,
with authored capsule radii and torso faces. Six weighted sash helpers form
four shared-vertex triangles appended after equipment. The source generator
`Tools/Animation/BuildAuthoredFighterExample.py` creates an original 61-sample
two-bone arm strike with torso lean through `CharacterPipeline`'s native baker.
It never copies a recovered clip or overwrites an existing output directory.

`scripts/main.lua` registers a warrior with typed `body_model`/`skin_models`,
a move with native attack intervals, and two actor definitions with private
behavior state. The HUD summons the pair through `fighter:spawn_actor`, and
callback-scoped actor references set targets, move, play the move, and dismiss.
Queued playback receipts and actor-local animation/damage/end callbacks provide
observable outcomes. Repeated mode uses ordinary Lua cooldowns and bounded
three-unit approach steps. Spacing resets use successive requests bounded to
100 units per axis. Native hit reactions can move and turn the pair even with
zero explicit attack impulse; reset spacing before controlled single strikes.

The manifest declares `content.register`, `content.patch`, `combat.actors` and
`ui.create`, with a dependency on `core`. Actor motion and playback need
`combat.actors`; main-fighter motion/playback permissions are separate.
Core kung-fu equipment, idle, locomotion and hit reactions remain explicit
dependencies. No production recovered source, serialized asset or API schema
changes are needed for this example. No generic Lua operation language is added.

`Tools/ModdingEditor/templates/authored-fighter/` mirrors every playable file.
The editor guide and public character-authoring/example pages document the
contract and installation, reproduction and verification limits together.

## Evidence

- `TestAuthoredFighterUnity.ps1`: **45 full-game checks** using the actual
  installed mod and its HUD in isolated Campaign Tournament 3, Unity 6.6,
  fresh profile and controlled main-fighter input/AI/spacing. The fixture
  sends HUD commands; it does not substitute fixture movement or playback
  for the mod's public actor calls.
- Native composition includes the mod-owned body, all six sash helpers and
  all four sash triangles. Equipment produces 323 composed nodes while the
  native reader retains 67 animation points across all 61 samples. Total
  composed node count is not the animation binding count.
- Root-relative sash coordinates change during the original strike. Both
  left and mirrored right playback cause native health loss with the peer
  root recorded as attacker. The right strike uses native facing -1 and
  paired-node mirroring. Receipts and per-actor starts/damage callbacks agree.
- Repeated mode produces additional native damage in both directions through
  public Lua approach/playback. Main-fighter health remains unchanged. Pause
  freezes both the fight clock and actor pose; dismissal delivers two end
  callbacks, removes both roots, and round teardown closes the HUD. No Lua
  callback failures remain.
- Final fresh native log:
  `Temp/FighterPlaybackUnity-d997a2c104cd4564b9767ae6eb4b68d8/validation-2f9e3a1850d5446eb049ad483a728b2d.log`.
  The captured `authored-fighter-native.png` was visually inspected in the
  original renderer. It shows the independent pair alongside the player and
  the actual lab HUD. It is contact/deformation evidence, not an art-quality
  or experimental-3D acceptance claim.
- `TestAuthoredFighterExample.py`: **3 tests** covering byte-reproducible
  exports/sidecars, refusal to overwrite, ordered native decoding, constant
  arm segment lengths, loop recovery, shared sash edges and differential
  deformation. Character pipeline/package regressions are checked separately.
- Editor generation/check/build and **58 project tests** pass. Actual LuaLS
  checks the complete example without diagnostics. **22 real VS Code checks**
  pass, including the nested starter's own manifest scope, removal/restoration
  of its actor capability, and zero diagnostics on the complete starter.
  An initial VS Code run used a stale built extension; rebuilding the current
  source resolved it without granting extra runtime capabilities.
- All four managed assemblies compile through the existing ignored portable
  Unity 6.6 reference projects. Production geometry preflight passes 34 checks;
  character pipeline and packaging regressions pass 6 and 7 tests respectively.
- The wiki builds 63 pages, covers all 246 public functions/aliases/callbacks,
  and validates 6,822 local links/assets, fragments and GitHub Pages paths.

## Player-form and native input follow-up

Authored Fighter Lab 1.1 adds player/comparison form buttons and
`combat.transform`. The player rule requests `fighter:change_form` only from
its active tick callback, retains the live result temporarily, and displays
queued/applied/failed status. The move now declares `events = "controlled"`,
the owned character condition, native Punch and `controllable` readiness.
Actor playback remains explicit and the existing actor checks still pass.
The comparison is a newly registered core-body kung-fu warrior, not a restore
of the original profile loadout. No production runtime changes or new Lua API
members were required; the public form/input contracts supply this behavior.

The expanded native fixture passes **63 checks**. After actor contact/pause/
dismissal it arranges 65% health and requests the player form through the real
HUD. It verifies retained percentage, player/control flags, non-reset clock,
canonical model-list membership, opponent target rebinding and the same rule
HUD. The authored skin is present on the player. Input is sent through
`GameController.SendGamepadControlEvent`, which reaches the existing controller
and fight input routing; the fixture never calls `PlayAnimation` for this
strike. Native selection, contact health loss, attacker-root attribution and
main-player animation/damage callbacks are observed.

An unrelated fighter does not select the owned clip. The comparison swap
removes the authored body/skin and preserves player state. Its native Punch
selects a real core attack while the owned clip remains absent throughout the
observation window. This positive core-action check prevents an inert input
path from passing the negative character-binding test. Round teardown closes
the retained HUD, and Lua callback failures remain absent.

Final fresh log:
`Temp/FighterPlaybackUnity-d997a2c104cd4564b9767ae6eb4b68d8/validation-b5c7f7339b8b42d3b0a704d3e7e91246.log`.
The separate `authored-player-native.png` was visually inspected; the applied
player and all eight lab buttons fit the native HUD. Managed assemblies and
the three reproducibility tests pass. Editor generation/check/build, all
58 project tests, actual LuaLS and **23 VS Code checks** pass; the editor now
tests the player form capability independently of actor permission. Public
guides/reference/examples, mirrored starter and tool guide change together.

This verifies native controller dispatch, not physical keyboard/gamepad/touch
devices. The form test runs after dismissing the pair; simultaneous owned
actors during a player swap, other loadouts/effects, saving/reloading during
a fight and exported players are not covered by this follow-up.

## Limits

This is the standard compatible rig with custom silhouette geometry and skin,
not a newly designed skeleton, arbitrary skinned mesh, FBX importer or complete
Blender/Gymnast export acceptance. Core equipment may obscure body details.
Native damage depends on recovered level/equipment attributes and hit reactions;
this fixture does not establish balance or an unrestricted encounter system.
Main fighters remain controllable in the installed mod; players need room for
the pair. Actors have a finite lifetime and this example targets offline
non-raid fights. Both mode registrations exist; the native fixture accepts
Campaign only. Other outfits/forms/arenas, exported players, all platforms and
sustained performance remain open. The procedural 3D renderer approximates
standard anatomy and may alter custom silhouette details; it has separate
acceptance evidence in `Docs/Engineering/EXPERIMENTAL_FIGHTER_3D.md`.
