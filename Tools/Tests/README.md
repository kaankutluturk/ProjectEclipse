# Regression tests and validators

Generated character packages: `Animation/TestGymnastPipeline.ps1` (under Tools)
verifies Blender/Gymnast pose export, watched/playable roles, exact payloads and
real Lua registration, then the recovered Unity reader. `CharacterForms/
TestPackagedCharacterUnity.ps1 -Package <generated integration mod>` runs that
unchanged package in a marked full-game project/fresh profile, checking native
Punch playback in both facings, wrist motion, planar weighted skin helpers,
opponent Lua AI and cleanup. Use `-OpponentOnly` for the default package role.
This fixture expects the supplied integration scene/skin, not an arbitrary mod.
`Modding/TestTacticProjection.ps1` separately projects actual standalone and
templated definitions into the native tactic compiler/parser. See
[packaged character acceptance](../../Docs/Engineering/Modding/PACKAGED_CHARACTER_ACCEPTANCE.md).

Editable mod UI: `Modding/TestModUiRuntime.ps1` covers typed text validation,
state/ownership, notifications and input eligibility; `TestModUiLua.ps1` exercises
actual Lua bindings, malformed values, handles and callback budgets.
`Modding/TestTextInputUnity.ps1` installs Text Input Lab in a marked isolated
Unity 6.6 project/profile, exercising native fields, Lua callbacks/getter,
HUD capture, multiline, Tab/Back, ancestor/dialog and owner cleanup. Synthetic
native key Events do not establish physical keyboard/IME or mobile acceptance.

Camera control: `Modding/TestCameraRuntime.ps1` executes actual Lua capability,
settings, ownership, callback/registration/cleanup and shipped Camera Lab HUD
tests with a controlled provider. `TestCameraNativePolicies.ps1` compiles the
production native helper/runtime with controlled eligibility, actor/session and
rounded layer services; repeated draws check pan restoration without drift.
`TestCameraUnity.ps1` runs the installed example in an isolated Unity 6.6 fight,
checking native pan/zoom, pause, retained defaults/release, host conflict, HUD
and unclaimed surrender cleanup, then captures a focus view. Camera form/actor
lifetime and all-arena/platform/performance acceptance have separate limits.

Actor form changes: `CharacterForms/TestActorFormBindings.ps1` executes production
actor preparation/participant methods with controlled native services. It checks
both teams, instance/state/lifetime retention, shield/status/behavior-key transfer,
role/collision preflight and preparation disposal. Form coordinator, presentation
and transition fixtures cover late rollback, untouched main panels and removal
before application. `Combat/TestActorDefinitions.ps1` also verifies actual Lua
form capabilities, typed arguments, receipts, unsupported backend and expiry.
The authored Unity run changes the live actor core and back, then requires fresh
bidirectional native attacks/contact with the same actor identities.

Arena artwork: `Modding/TestArenaRuntime.ps1` checks production Lua sprite/marker
creation, updates, typed handles, scopes, shared limits, malformed input and the
complete Pulse Arena schedule with controlled services. `Modding/TestArenaUnity.ps1`
runs the installed sample in an isolated Unity project/profile, checking cropped
atlas UV animation, in-place motion, sensor/direct health loss, pause, shared
native budgets and cleanup. It captures warning and active art; this is not
solid/swept physics acceptance.

Original authored fighter: `CharacterForms/TestAuthoredFighterUnity.ps1` installs
the actual Authored Fighter Lab mod in a marked isolated Unity fixture. It checks
owned body/skin composition, preserved binary point count, the original clip's
native reader/playback, root-relative helper deformation, both facing directions,
real contact source/damage, receipts, pause and actor/HUD cleanup. It uses compatible
core equipment/idle and controlled spacing/profile, not arbitrary rig acceptance.
Player-form coverage changes the canonical player through the public HUD,
dispatches Punch through the native controller, and checks original-strike
contact/callbacks and character-specific selection after a comparison swap.
Pass `-PlayerEntry` to choose its authored player through the native setup UI,
or `-ComparisonEntry` for the comparison warrior. Both check Back/reopening,
saved player choice, isolated generated-instance/blueprint parameters and
player/control flags before any form request. With the pair alive, an owner form
swap must preserve actor handles/IDs, bodies, private behavior instances, birth
frames and mutual targets, followed by further native contact in both directions.
`CharacterForms/TestFormRenderBindings.ps1` additionally checks production actor
owner/explicit-target/queued-birth binding, commit and late presentation rollback
with controlled services. The fight launches directly;
map/mode menu clicks are outside this acceptance runner.
`Tools/Animation/TestAuthoredFighterExample.py` reproduces the shipped geometry,
binary and sidecar and checks source motion/skin structure.

Authored character geometry: `CharacterForms/TestAuthoredGeometry.ps1` checks
the production body/skin validator and extracted loader with controlled native
parsing, including path/field diagnostics, composition limits, helper bindings,
iteration names and archival core compatibility. `TestAuthoredGeometryUnity.ps1`
boots a full-game isolated Unity 6.6 fixture and exercises real hidden model
preparation/disposal and invalid-binding rejection against cached authored
body/skin documents. It does not prove a public authored mod, new animation,
contact or visual quality. Python export/packager tests remain in `Tools/Animation/`.

Ranged independent fighters: `Combat/TestRangedActorsUnity.ps1` runs the actual
Ranged Companion Duel Lua/HUD and native child geometry/contact in an isolated
Unity fixture. It checks actor-root provenance/receipts, repeated bidirectional
damage, unchanged main life, pause, hold/resume, live-child teardown and a
controlled queued-birth retirement race with sibling children preserved.
`Combat/TestProjectiles.ps1` complements it with production Lua/partial-source
ownership, eligibility, capacity, lifetime, expiry and cancellation checks using
controlled native inputs. These scopes do not imply arbitrary rig/platform support.

Independent fighters: `Combat/TestActorDefinitions.ps1` checks production Lua
registration, bounds, strict fields, typed behavior attachments/parameters,
rollback/fingerprints, private instance state and callback-scoped self references.
`Combat/TestActorsUnity.ps1` runs the actual companion mod/HUD in an isolated
Unity project, checking scoped commands, independent root state, teams,
death/removal, pause/TTL and surrender cleanup on a controlled core encounter.

Scripted actors: `Combat/TestScriptedActorsUnity.ps1` runs the complete public
sparring mod/HUD in an isolated Unity fixture. It forbids fixture or Lua manual
playback and requires Lua-selected native attacks, source-attributed contact in
both directions, repeated damage after subsequent starts, independent memory/
identity, reactive behavior callbacks, outgoing/incoming modifiers matched to
native health loss, unchanged main health and exactly-once replacement/teardown
callbacks. Core rig/equipment
and controlled spacing/input bound the evidence. `Modding/TestModAi.ps1` covers
copied actor provenance, memory isolation, action lifetime, fallback/budgets and
real native clip metadata; optional compile/assembly/Unity-reference paths allow
portable managed builds. Archived original AI/charge fixtures stay supported.

Procedural fighter rendering: `Presentation/TestSculptedSkinRuntime.ps1` checks
production geometry, cap bounds, normals and animation topology with Unity
managed math, including an independent reference pose that preserves native
limb lengths while separating guarding hands and touching legs. The native
probe also rebuilds the skin during a paused high kick and checks pose isolation.
`Presentation/TestExperimental3DUnity.ps1` runs the full game in a
marked isolated Unity 6.6 fixture, checks the real experimental Settings toggle,
connected skins, native high-kick deformation, restore and teardown, and captures
normal/bright screenshots. These checks do not cover every outfit or performance.

Run these commands from the repository root. Runners and their C# fixtures stay
together here; generated sources, binaries, logs and isolated projects go under
ignored `Temp/`. Tool-specific suites in `Tools/Animation/`, `Tools/ModdingEditor/`,
`Tools/NetplayTests/` and `Tools/ModZipInstallerTests/` keep their existing locations.

| Directory | Coverage |
| --- | --- |
| [CharacterForms](CharacterForms/) | Model replacement, form transitions, animation bindings and transferred state |
| [Combat](Combat/) | Moves, projectiles, perks, rules and local versus |
| [Progression](Progression/) | Profiles, purchases, rewards, quests, story, tutorials and raids |
| [Modding](Modding/) | API contracts, content registration, Lua, UI and mod lifecycle |
| [Presentation](Presentation/) | Loading, title previews, locations, artwork, layouts and performance |
| [Runtime](Runtime/) | Assembly cleanup, archive audits, offline runtime and shared runtime checks |
| [DE128](DE128/) | Downstream DE128 content acceptance; may require local downstream inputs |
| [Shared](Shared/) | Common managed loader and stubs used across suites |

## Python entry points

Audio instance acceptance uses `Tools/Tests/Modding/TestAudioRuntime.ps1` for
production Lua and owned lifetime/budget contracts with a controlled backend.
`Tools/Tests/Modding/TestAudioUnity.ps1` boots the full game in an isolated
Unity 6.6 project/profile, runs Audio Lab through native buttons and checks actual
WAV/source progression, clocks, volume/mute, global bounds and teardown. Pass the
matching editor with `-Unity`; `-ExistingFixture` requires this runner's marker
inside repository `Temp`. Fresh Windows profiles use a junction to an immutable
TAR cache inside the fixture, avoiding repeated system-drive extraction. The
runner does not delete profiles or caches. Device audibility is not tested.

```sh
python Tools/Tests/Runtime/TestAuditDECorpus.py
python Tools/Tests/Runtime/TestDEXmlAudit.py
python Tools/Tests/CharacterForms/TestCharacterForms.py
python Tools/Tests/CharacterForms/TestCharacterForms.py --case TestFormModifierTransfer
python Tools/Tests/Presentation/TestVersusTransition.py
```

The versus-transition suite checks native loading artwork ownership and the
unchanged VS completion timer for local, online, replay, training and campaign
paths with controlled Unity dependencies. It does not render the transition or
run a game playtest.

`pwsh -NoProfile -File Tools/Tests/Presentation/TestVersusPreviewNative.ps1`
uses the matching Unity editor in an isolated project with graphics enabled. It
checks the production fighter preview's animation ticks, changing pixels and
transparency across scene unloading, UI visibility and world/texture cleanup.
Fighter meshes are controlled animated quads; this is native rendering validation,
not a multiplayer game playtest.

The corpus suite uses temporary controlled inputs. Character-form checks reuse
the extracted-C# assertions from the PowerShell runners and require the matching
installed Unity editor's bundled compiler plus a .NET 10 runtime. They do not
launch Unity by default. Use `--help` for compiler paths and explicit native runs.

The four fixtures repaired during the October 1 cleanup cover transition barriers
and interpolation call order, speculative body disposal, the current node enum,
and own-animation observation in versus/title AI readiness. These use controlled
multiplayer/rendering dependencies; passing them is not rollback or Unity acceptance.

The other `Test*Native.py` scripts prepare or run isolated editor fixtures; read
their help and docstrings for required inputs and execution behavior.

## PowerShell entry points

`Combat/TestExtraFighterUnity.ps1` is a full-game feasibility probe for a third
native root fighter. It clones the core opponent's parameters/rig/tactic and
enters recovered construction and animation seams through reflection. It checks
independent health, rendering, bounded AI observation/contact attribution, insertion-order
targeting, extra-root knockout, explicit removal and original-duel continuation.
It runs with controlled input/spacing, an empty user-mod root and a unique fresh
post-tutorial profile. This is not a Lua actor API or acceptance for teams,
different rigs, reliable autonomous attacks, paired attacks, multiplayer, exported
players or physical input. A fallback explicitly starts a native knife attack if
AI produces no contact within 120 simulation frames; results record that choice.
The probe also checks actual shared native move-point bindings across the live
roots and repeats those observations after removing the extra root.
Pass `-RequireAutonomous` to forbid fixture-requested attack playback, allow up to
600 simulation frames for the first native AI contact and require a second
  distinct attack start after first contact and later damage sampled after that
  start. This mode checks current own/target AI observations
before knockout/removal. Its core cloned rig/tactic scenario is not broad AI,
team or mod-owned actor acceptance.
Reuse requires a marked project inside repository Temp; an open fixture is
rejected and compiler errors fail early. See the actor feasibility record under
`Docs/Engineering/Modding/` for the native boundaries and remaining proof work.

Pass `-RequireTargeting` to include the strict autonomous scenario, immediate
transactional target rollback, explicit hostile-root binding changes, incoming
native damage on the extra root with unchanged excluded player health, current
new-target node/AI observations and original-duel target restoration before removal.
This mode has core cloned-rig acceptance; it does not expose a Lua actor/team API.

`Combat/TestCombatTargetBindings.ps1` extracts production hostile-root and AI-target
transaction methods. Controlled services exercise exact synchronous rollback,
source/target validation, child propagation, selected-first ordering, old-target
observation/wait/throttle invalidation, empty/unarmed targets, mapping failure
before mutation and child-binding failure rollback. It is not a Unity playtest.

`Combat/TestAnimationObserverRouting.ps1` extracts the production animation-start
methods and checks own/source/observer identity with controlled native services:
ordinary pairs, extra roots, retargeting, duplicate/null/self/weapon exclusions and
native AI flags. It is separate from full-game autonomous contact acceptance.

`Combat/TestProjectiles.ps1` compiles the production Lua bindings and complete
projectile queue/tracker with controlled native models. It covers reusable
registered definitions, direct spawn receipts/placement, shared timeline/queued
capacity, constraints and cancellation, and the actual Scripted Burst HUD/source,
receipt-ID trajectory, partial bursts, cooldown and next-round reset.
`Combat/TestReturnDartUnity.ps1` also adds a private direct three-child probe to
its isolated shipped Return Dart scenario: applied IDs, native birth/rendering,
spacing, no extra caster start, actual contact and live-child surrender cleanup.
Use a marked existing fixture only inside repository Temp. Inputs, AI, spacing
and fresh post-tutorial profiles are controlled; this native probe and the managed
complete Scripted Burst scenario have different scopes. Arbitrary actors/rigs,
all loadouts/modes, physical input and exports are separate acceptance work.

`Combat/TestArcDartUnity.ps1` boots an isolated full-game Unity project/profile
with only Arc Dart enabled. The native HUD casts an owned launch/flight graph
through typed projectile actions: actual rig/item, rendering, travel, contact,
caster damage attribution, pause, strike removal, miss expiry and cooldown are
checked. `-ExistingFixture` accepts marked projectile/playback projects inside
repository Temp. Immutable TAR cache sharing avoids repeated system-drive data;
profiles/caches are not deleted. Controls/AI/spacing and post-tutorial state are
controlled; physical inputs, reverse facing, Eclipse mode and exports are separate.
`Combat/TestFighterPlayback.ps1` also exercises the shipped Lua graph and ability
lifecycle with controlled models. `Combat/TestScheduledMoveEvents.ps1` checks
the actual native action batch retains events raised by scheduled playback;
`Combat/TestDECombatPerksNative.ps1` covers child-owner hit/outgoing/resolved
routing in both directions, nested children and unrelated/retired roots.

`Combat/TestFighterPlayback.ps1` compiles the production Lua bindings and complete
playback/motion queues with controlled native dependencies. It checks typed owned
move handles, capability/timing/lifetime guards, competing mods, receipts,
availability, native rejection/exception isolation, recursion, pause and teardown.
The actual Active Strike source exercises HUD intent, applied/failed receipts,
cooldown and next-round reset. `Combat/TestFighterPlaybackUnity.ps1` boots a marked
isolated full game in Unity 6.6, plays the shipped authored punch through its
native HUD button and verifies animation-start notification, contact damage,
cooldown, pause retention and surrender cancellation. Controls/AI/spacing and a
post-tutorial profile are controlled. The rendered PNG is under ignored Temp.
Use `-ExistingFixture` only for a marked playback project inside repository Temp;
each run has separate logs, save identity and freshness-checked result.
Physical input, all rigs/equipment/arenas, Eclipse-mode and exports remain open.

`Combat/TestFighterMotion.ps1` compiles the production Lua binding and complete
fighter-motion queue with controlled models/session/translation. It checks typed
arguments, capability/timing/lifetime guards, independent Lua contexts, shared
limits, deferred application, pause, cancellation, failure isolation and the
shipped Repulse button/cooldown/round state.
`Combat/TestFighterMotionUnity.ps1` copies the full game assets/source into a
marked isolated Unity 6.6 project and starts real Campaign/core combat. Independent
probe mods exercise the actual callback/simulation boundary; further checks inspect
real rig points and animation data, native HUD button/cooldown, continuing combat,
pause and surrender cleanup. Input/AI are controlled; each isolated profile is
seeded with completed tutorial state before its real campaign load. First-time
onboarding is not tested.
Use `-ExistingFixture <absolute marked path inside Temp>` to reuse imported assets
while refreshing current source and mods. Each run gets a fresh save identity,
separate log and timestamp-checked acceptance result. Physical controls, every
animation/arena and exported-player behavior still need broader playtesting.


`Modding/TestModCallbackDiagnostics.ps1` exercises the actual Lua worker/session,
nested framework provider attribution, instruction-budget failures/recovery,
recording toggles, detached snapshots and bounded callback/failure histories.
`Modding/TestModCallbackDiagnosticsUnity.ps1` uses an isolated hidden Unity 6.6
editor with a rendering Game View to exercise the production overlay's native
F3/F4 Update paths, IMGUI screenshot and saved report. Scripted keys, game/session
lookup and report directory are controlled; no physical input or full-game frame
attribution is claimed. Batch mode does not present IMGUI screens.

Use PowerShell 7 (`pwsh`) for managed-runtime checks. Individual scripts may also
require MSBuild, a .NET SDK, Visual Studio's compiler or a specified Unity editor.
For example, when MSBuild and the matching managed references are available:

```powershell
pwsh -NoProfile -File Tools/Tests/Progression/TestUnderworldRuntime.ps1
python Tools/Audits/AuditUnderworld.py
```

Select the runner for the subsystem you changed rather than invoking every
script. `Shared/LoadUnityManagedAssemblies.ps1` is the shared loader, not a test runner.
Some historical DE checks require downstream content or local research inputs.

`Combat/TestRoundOutcomes.ps1` executes production Lua bindings and extracts the
current `Fight` round arbitration, score, winner, end-stance and surrender methods
for a managed fixture. Models, clock, presentation and settlement are controlled.
It checks declared authority/conflicts, repeat calls, native-result precedence,
pause, stale fighter handles and round reset. It does not play native end animations,
grant game rewards or prove full-game save/settlement acceptance.
`Combat/TestRoundOutcomesUnity.ps1` runs the shipped objective with production
script sessions, native font/HUD/rendered pixels/fade teardown and extracted
current round methods in an isolated Unity 6.6 Play Mode fixture. Contacts,
models, clock, end presentation and settlement remain controlled. Both runners
share `Combat/ExportRoundOutcomeFixture.ps1`; it is an extraction helper, not a test.
Pass `-WithFocusPack` to include the actual Focus framework/add-on, separate HUD
placements, bonus behavior and a controlled static-rule contributor. This mode
also exercises extracted native adapter projection/restoration around controlled
battle source storage. It does not instantiate full native map/fight menus.

`Modding/TestModPackRules.ps1` tests additive append composition in production
Lua/script sessions: dependency/discovery order, replacements/controllers,
transaction rollback/recovery, duplicates and the aggregate rule limit. It uses
extracted current adapter apply/remove and round methods with controlled battle
storage/models, and the shipped Focus/objective save/removal/reinstallation pack.
`Modding/ExportModPackProjectionFixture.ps1` is its shared extraction helper.

`Modding/TestModExtensions.ps1` runs the production Lua runtime and script-session
lifecycle against controlled asset/combat/UI sources. It covers typed framework
services, capability/dependency/version checks, isolated data, startup rollback,
execution limits, migration restrictions and the shipped Focus framework/add-on
with saved-state reload. It does not render a native fight or perform a game playtest.

`Modding/TestModExtensionsUnity.ps1` uses the matching Unity editor with graphics
enabled in isolated Play Mode. It executes the shipped Focus pair through the
production script session, renders its production HUD view with the recovered
font, checks changing pixels and teardown, and preserves serialized state through
mod removal/reinstallation. Contacts, the asset host and unused artwork/scroll
paths are controlled. Its PNGs/XML/logs stay in `Temp/`; full-game combat,
native menu/profile integration and disk-crash acceptance remain separate.

PvP balance checks are `Combat/TestPvpBalance.ps1` (strict JSON, immutable rules,
hashing, inheritance, nonlethal chip and recovery with controlled native
dependencies) and `Combat/TestPvpHealthRuntime.ps1` (compiled recovered health
setters, pool cloning/reset, production rollback policy and state hash).
The compiled health runner and Underworld runner accept `-AssemblyDirectory`
and `-UnityManagedDirectory` overrides for the matching compiled game/editor
references. `Presentation/TestPvpHealthBarNative.ps1` renders the production
recovery component and native skew mesh in an isolated Unity project, with
controlled health and atlas-base dependencies. None is a full online playtest.

Arena region acceptance uses `Modding/TestArenaRuntime.ps1` for the production
capsule/rectangle algorithm, Lua arguments, capabilities, ownership, budgets and
complete Pulse Arena schedule with controlled native sources.
`Modding/TestArenaUnity.ps1` runs the real game through Campaign/core Tournament 3
in an isolated Unity project and post-tutorial profile. It verifies warning art,
projection, pose/contact health loss, pause/resume, recovery, shared marker limits,
scope/surrender cleanup and suppression of ticks after round-end. Input, AI and
spacing are controlled; this does not prove solid/swept physics or all arenas.

## Native validators

`Presentation/TestExperimental3DUnity.ps1` launches the full game in an isolated
Unity 6.6 fixture with a unique post-tutorial profile and no enabled mods. It
clicks the actual Display/3D fighters toggle, captures before/after and Settings
PNGs, and checks native depth, volume normals, shader/perspective orientation,
viewer ownership, paused pose isolation, off restore, resumed animation,
preference reset and camera teardown. AI/input/spacing are controlled; this is
not all equipment/arenas, physical input, performance or exported-player proof.
Use `-ExistingFixture` only with its rendering marker or a fighter-playback marker
inside repository Temp. See the experimental rendering engineering note.

`Runtime/TestUnity6Workflows.ps1` uses the matching Unity 6.6 editor in an isolated
project to test input devices across Play Mode frames, modern UI modules and
native Build Profile/Multiplayer Play Mode scenario serialization. It does not
perform a full game build or multiplayer fight; see
[the workflow guide](../../Docs/Engineering/UNITY_6_WORKFLOWS.md).

`Validate*.cs` and `Verify*.cs` are fixtures invoked by their paired runners or
through an explicit Unity editor command; they are not imported into the main
project automatically. Follow the runner's instructions for fixture setup.
Managed checks and static audits do not prove native sprite import, rendering
or gameplay. Report native validation and playtests separately.

`Combat/TestProjectiles.ps1` compiles production Lua bindings and the complete
owned-projectile native tracker against controlled membership/translation. It
checks callback expiry, ownership, finite/aggregate/request/query limits, pause,
TTL, native deletion and round/session/owner retirement. `TestReturnDartUnity.ps1`
uses a marked full-game Unity fixture and fresh isolated profile to check the
shipped Lua trajectory, native contact, miss reversal, pause and live-child
surrender cleanup. The fixture controls inputs/AI/spacing; it is not an all-arena
or exported-player claim. Generated DLLs, fixture assets, logs and captures stay
in ignored `Temp/`.
