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

## Declared encounter player follow-up

Authored Fighter Lab 1.2 registers its own fight and repeatable mode. The optional
`sf2.fights.register.player_character` takes a warrior handle, defaults to the
saved player's ordinary combat setup, and belongs to owned registration rather
than a core fight patch. `ModContent` validates category/dependency references
and committed existence; all fight copy operations retain the declaration.
`ModSaveData` includes a tagged character reference only when supplied, keeping
omitted/nil fingerprints compatible with the prior stream.

The MoonSharp binding forwards the typed reference through the facade and
transaction. `ModRuntime.BuildFightPlayerParameters` resolves the installed
fight's registered runtime identity and uses the existing warrior/form builder.
`GameUtils` initializes those parameters as the canonical controlled player,
both for native fight construction and difficulty evaluation. Core encounters
and definitions without this field keep their prior setup; local versus takes
its existing separate factory path. The character's own tactic does not give
the player AI ownership. Profile equipment is not rewritten.

The actual public Lua contract runner passes registration, wrong-handle/scalar
rejection, transactional rollback, defaults, deterministic/changed fingerprints
and retained character identity in copied fights. All four portable managed
assemblies compile. Editor generation/check/build and all 58 project tests pass;
actual LuaLS verifies field completion and wrong Battle-handle rejection. All
24 VS Code checks pass, including this field in the mirrored authored starter.
The public guide/reference/example index, starter and tool guide change together.

The `-PlayerEntry` native route verifies the declared player identity, body and
control flags before any form request, then runs the original actor, form and
native Punch checks. The probe waits until the lab click no longer captures
gameplay input and holds Punch across native frames: editor callbacks can share
a rendered frame with a HUD click or precede a game tick. It does not bypass UI
capture, force move playback or substitute scripted damage for contact.
The runner launches the registered fight directly. Map/mode menu navigation,
save/reload during combat, arbitrary rigs and exported platforms remain open.

Final native acceptance: **66 owned-entry checks**, fresh log
`Temp/FighterPlaybackUnity-d997a2c104cd4564b9767ae6eb4b68d8/validation-d46b7b64b0134e50a6c19bcd945abffa.log`.
The default Tournament route also passes **63 checks** in
`validation-254cda1ebc1944ed8b603e46842d4736.log`. Both require process exit 0.
The wiki builds 63 pages and validates 6,826 local links/assets and fragments.

## Prepared player choice follow-up

Authored Fighter Lab 1.3 supplies a public `on_prepare` menu for its owned
encounter. Back cancels the pending request; choosing the authored or core
comparison warrior resolves `{ player_character = warrior }` through
`sf2.modes.resolve`, closes the view and defers native entry until the callback
returns. Omission/nil inherits the registered fight's player character.

`ModEncounterPlan` stores the optional typed warrior identity. The Lua reader
rejects scalar, wrong-category and foreign handles; `LegacyContentAdapter`
checks installed ownership/existence, and native construction validates the
warrior parameters before the plan is saved. `ModRuntime` associates the choice
with each generated `FightList` using a weak table, rather than changing the
shared registered blueprint or choosing by fight ID alone. The existing player
factory and difficulty paths consume that instance selection.

`ModModeProgress` writes version 3 only when a player choice exists. Versions 1/2
remain readable and unchanged plans retain their previous format. The managed
public-Lua fixture verifies typed choice, malformed input rejection, omission/nil,
save/reload with rules and description, preserved rejection of the new field in
an old format, and plan consumption on loss. The mode workflow passes 33 checks
using its tracked editor starter; its runner no longer depends on a missing
`Mods/example.generated-expedition` directory. All four managed assemblies and
the wiki build pass. Editor schema/definitions, actual LuaLS and VS Code tests
cover the encounter-plan field and stay synchronized with the example starter.

Both entry choices pass 78 full-game native checks. The fixture exercises actual choice UI
cancellation and reopening, both player selections, retained saved selection,
unchanged shared blueprint, independent generated-instance choices and rejection
of an unavailable player without save mutation. It must wait for the requested
fight ID: the dojo training fight can briefly coexist with the loading screen.
Form-input tests allow the first stance and the deliberate opponent action to
settle before delivering a real controller Punch, without forcing move playback
or substituting scripted damage for contact.
The controlled opponent receives a native control role for its deliberate Punch
and return to stance; disabling both AI and control leaves no ordinary controller
to choose the next stance. Menu captures wait for the campaign loading overlay
and entrance fade, and visual review corrects a clipped hint in the shared sample
and editor starter.

Authored entry passes in
`Temp/FighterPlaybackUnity-d997a2c104cd4564b9767ae6eb4b68d8/validation-80f8a50680aa4b86a35e920ce5d4f720.log`;
comparison entry passes in `validation-8156850303354d559c03e5c389706d45.log`.
Both finish with process exit 0. The final comparison run also verifies the
corrected hint. The chooser and player captures are visually reviewed and saved
to ignored `Temp/AuthoredCharacterChoiceReview-20261004/`. The existing Tournament
regression passes 63 checks with process exit 0 in
`validation-c6945061226a47cca4633e529e5adb26.log`. The wiki builds 63 pages and
validates 6,828 local links/assets and fragments. The editor project suite passes
58 checks; actual LuaLS and all 25 VS Code integration checks pass.

This selects a temporary encounter character, not a persistent profile loadout.
Native entry is launched through the registered fight fixture; full map
navigation, native process restart/resume and exported platforms remain open.

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
