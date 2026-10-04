# Creator platform implementation record

2026-10-04: exported Blender/Gymnast packages can select their own playable
warrior through `--playable` and the existing owned encounter field. Generated
move conditions/intervals now match the current Lua contract and face the enemy
at playback; standalone Lua tactics initialize required empty native lists.
See [packaged character acceptance](PACKAGED_CHARACTER_ACCEPTANCE.md) for source,
complete authoring/reader/native evidence and remaining arbitrary-rig/platform
limits. Experimental 3D stays deferred and the full creator objective stays open.

2026-10-04: actor behavior hosts now create and guide their own typed native
projectiles. Queries stay mod/caster-scoped; retirement fails pending receipts,
releases reservations and removes live children. Ranged Companion Duel supplies
actual public Lua/native contact and a mirrored starter. See
[actor projectile acceptance](ACTOR_PROJECTILES_ACCEPTANCE.md) for source/API,
49 native checks, 585 controlled production checks, creator tooling and limits.
The full Minecraft-style creator objective remains open.

2026-10-04: independent actor definitions now attach owned stateful behaviors,
with spawn/end, tick, contact/damage and animation callbacks. Native bonuses and
mitigation match actual health loss; each replacement has fresh state. See
[actor behavior acceptance](ACTOR_BEHAVIORS_ACCEPTANCE.md) for source/API, 57 native
checks, 81 definition/state/scope checks, creator tooling and limits. The full
Minecraft-style creator objective and G01–G14/E1–E8 scope stay open.

2026-10-04: shipped the owned independent fighter API and companion HUD example.
See [actor API acceptance](ACTOR_API_ACCEPTANCE.md) for source, inferred native
names, actual Lua/native evidence (42 checks), definition/fingerprint checks
(50), wiki/editor verification and limits. Broad creator work remains open;
the user-prioritized procedural 3D refinement then shipped in e76f014d.

2026-10-04: autonomous Lua-directed actors now have copied provenance and a
playable sparring example. See [scripted actor acceptance](SCRIPTED_ACTORS_ACCEPTANCE.md)
for source routing, 30 actual native checks, repeated contact after subsequent
starts, 95 Lua AI checks, docs/editor coverage and remaining creator scope.

The objective is Minecraft-style creative freedom for Shadow Fight 2: creators
should be able to introduce gameplay systems, share frameworks, compose packs,
and ship substantial new experiences through a documented, usable API. One
feature, a function count or a downstream content port does not meet that objective.

The existing G01–G14 and E1–E8 requirements in
[the roadmap status](ROADMAP_STATUS.md),
[engine extensibility](MOD_ENGINE_EXTENSIBILITY.md), and
[the implementation plan](DE_API_IMPLEMENTATION_PLAN.md) remain in scope. This
record adds evidence to those requirements; it does not replace their exit criteria.
Public documentation describes implemented contracts, with acceptance limits.

## 2026-10-03: typed framework services

Before this change, separate Lua mods could reference declared content but could
not publish reusable procedural functions to dependent mods. The new
`sf2.extensions` module allows a framework to publish a versioned request/response
contract and handler. Dependent mods call it during loading or supported runtime
callbacks. The framework retains its capabilities and state, while data is copied
between script contexts. This advances E7/E8 composition and ownership work.

### Source and routing

| Source | Responsibility |
| --- | --- |
| `Assets/Scripts/Eclipse/Runtime/Modding/ModExtensions.cs` | Declarative service definitions, transactional catalog registration and session-owned routing. Checks direct dependencies, exact version, active owner, re-entry and shared budgets. Independent of recovered assemblies. |
| `Assets/Scripts/Eclipse/Runtime/Modding/ModContent.cs` | Includes services in registration capacity, validation, commit and rollback. |
| `Assets/Scripts/Eclipse/Runtime/Modding/ModScripting.cs` | Supplies the session registry to each facade. Existing central capability guard also blocks reward-configuration calls. |
| `Assets/Scripts/Eclipse/Modding/ModScriptSession.cs` | Creates one registry, activates exports only after successful content commit, removes failed owners and disposes services with their session. |
| `Assets/Scripts/Eclipse/Modding/MoonSharpScriptRuntimeExtensions.cs` | Four Lua bindings, opaque script-owned handles, typed primitive records and bounded provider handler execution. `try_call` supplies recoverable errors in the hard sandbox. |
| `Assets/Scripts/Eclipse/Modding/MoonSharpScriptRuntime.cs` | Opens execution scopes for shared budgets, clears handles/exports on disposal and restricts service calls during state migration. |
| `Assets/Scripts/Eclipse/Runtime/Modding/ModSaveData.cs` | Appends sorted service contracts to fingerprints only when services exist. Existing content without services keeps its previous fingerprint. |

Schemas reuse the existing parameter contract: at most 64 fields, finite numbers,
exact safe integers, booleans and bounded strings. Native handles, nested tables
and closures do not cross contexts. Calls permit depth 8 and 32 routed attempts
per outermost Lua execution; each provider handler has a 200,000-instruction
budget. Providers can intentionally expose their own operations, so handlers
must validate domain constraints and authorize callers where necessary. Applied
changes are not rolled back across mods if a later call fails.

### Creator workflow

`Mods/example.focus-framework` owns a saved Focus resource and publishes status
and hit services. `Mods/example.focus-addon` declares its dependency, obtains the
two handles and uses the services in combat callbacks. It applies outgoing damage
with its own capability and updates a HUD. Every third positive, unblocked hit
receives half its pending damage as a bonus, capped at one normalized unit.
The framework can serve other add-ons without duplicating its state logic.

The public [service reference](../../Modding/src/content/docs/api/extensions.md)
documents every function and the handler, schema limits, dependency/version
rules, authority, failure behavior and saves. The manifest guide, examples,
sidebar, compatibility guide and VS Code guide are updated. The editor's
authored schema, generated definitions and copied framework/add-on templates
cover the same contract. Editor-only definitions remain outside executable mods.

### Verification

- `TestModExtensions.ps1`: 126 checks using production MoonSharp and `ModScriptSession`, with
  controlled asset-host/combat/UI inputs. Covers schemas, capabilities, versions,
  direct dependencies, detached data, failed-provider rollback, re-entry, runaway
  handlers, shared call/depth budgets, state migration, fingerprints and the actual
  two-mod example across save/reload. Its showcase prerequisite also passes.
- All four managed assemblies compile using ignored temporary project copies
  with references remapped to installed Unity 6.6 (`6000.6.0f1`). The tracked
  project files retain their original reference paths; direct standard commands
  hit local SDK/analyzer path issues. The two new source Includes are tracked.
- Editor generation/check/45 unit tests, LuaLS 3.19.1 protocol integration and real
  VS Code integration pass. Local LuaLS 3.18.2 fails initialization with a duplicate
  worker channel; it is not claimed as passing for this change.
- Wiki build, binding coverage, types, search index and local link checks pass.
  Build output is ignored; authored files and generated editor definitions are tracked.

At the first feature commit, Unity editor validation and a full-game playtest
had not been performed. The controlled combat test proves routed callbacks and HUD data,
not real contact ordering, rendering, menu navigation or native save acceptance.

### Remaining work toward the vision

Services do not supply arbitrary engine operations. Custom fight outcomes,
broader actor creation/control, general custom scene workflows, additional
profile/inventory operations, cross-mod events, modpack conflict tooling and
multiplayer extension policy retain their existing roadmap requirements.
Framework composition needs further pack/lifecycle and native acceptance.
Creator workflows should be demonstrated by independent mods, including their
failure, removal/reinstallation and save cases. The broader objective remains
active; this feature establishes reusable procedural composition only.

## 2026-10-03: native framework acceptance follow-up

Added `Tools/Tests/Modding/TestModExtensionsUnity.ps1`,
`ModExtensionsUnity.cs` and `ModExtensionsUnityStubs.cs`. The runner creates an
ignored fixture using the repository's matching Unity 6.6 editor/package versions,
the production runtime APIs, MoonSharp context, script session, HUD view/fade and
the actual shipped Focus mods. It imports canonical vanilla stage definitions
and copies the recovered font with its existing GUID. It runs with graphics and
exits only after recording acceptance evidence.

33 Play Mode checks pass: separate provider/add-on activation, native font loading,
HUD text and visible changing pixels, blocked-hit behavior, third-hit damage,
fight-end cleanup, actual fade destruction, XML write/read and removal/reinstall
state preservation. The `Focus: 2/3` screenshot was visually inspected. Screenshots,
profiles and logs remain in `Temp/ModExtensionsUnity-*`; they are not committed.

This strengthens verification of the existing feature; no public function or
capability was added. Public reference/example verification statements and the
test index now reflect that evidence. Contact notifications, asset-host construction
and unused artwork/scroll paths are controlled. Full-game contact ordering,
native menu/profile lifecycle, physical input and durable crash/restart acceptance
remain open, as do the wider creator-platform requirements above.

## 2026-10-03: declared offline round objectives

Adds reusable objective/result authority toward E2. A behavior rule declares
`controls_outcome = true`, requiring `combat.round_outcome`; its active combat
callback can call `fighter:end_round("win" | "loss")`. Results are relative to
the player and are provisional until the existing native round boundary.
No DE-specific policy, generic Lua operation DSL or direct reward grant is added.

### Source and execution

- `Runtime/Modding/ModRoundOutcomes.cs` supplies a recovered-type-independent
  typed host interface, exclusive controller validation and per-round request
  state. `ModContent.cs` validates attached controllers before catalog mutation;
  disjoint mode/round scopes can coexist. `LegacyContentAdapter.cs` validates
  generated encounter lists before returning projected XML. Selection checks
  in `ModBattleRuleInstances` defend the combat boundary too.
- `ModScripting.cs` associates result authority with a declared rule on its
  instance wrapper; `ModRuntime.cs` supplies that rule only for fight-rule dispatch.
  Ordinary rules, equipment and perks do not gain authority by sharing a behavior.
  `MoonSharpScriptRuntime.cs` enforces capabilities, exact outcomes and callback
  lifetime/timing, returning acceptance or a refusal reason.
- `Fight.cs` queues requests while an offline round is active, consumes them
  after native KO/timeout/end-rule arbitration, and uses the existing `EndRound`
  score/end-stance path. Surrender cancels pending requests. `GetWinner` retains
  the consumed result through end presentation; next-round setup resets it.
  Same pending requests are idempotent, contradictory requests are refused.
  No health mutation or recursive result settlement occurs in the Lua callback.
  Requests are transient; accepted side effects are not rolled back by a later
  callback error. Online raid, PvP/versus, training and title sparring refuse them.
- `ModSaveData.cs` fingerprints declared authority without changing hashes for
  ordinary rules. The new runtime source and `.meta` are tracked; recovered
  asset identities and original project reference paths are preserved.

### Creator workflow and verification

`Mods/example.hit-objective` uses typed round state, damage/tick observations,
a HUD and one controller: three positive unblocked hits within ten active
simulation seconds win; expiry loses without killing either fighter. It patches
Act I tournament battle 3 in normal and Eclipse modes. The editor ships the same
manifest/script starter. Public round-outcome/rule references, manifest,
compatibility, examples, editor guide, generated definitions, capability checks
and sidebar cover the implemented contract.

- `TestRoundOutcomes.ps1`: 702 checks using production MoonSharp/runtime APIs
  and extracted current native arbitration/score/winner/surrender methods.
  Covers invalid inputs, capability/instance isolation, conflict rollback,
  disjoint scopes, expired callbacks, native-result precedence, lifecycle guards,
  offline raids, generated encounter projection, queued pause/resume, ordinary
  native results, zero-health/death-completion precedence, detached fights,
  fingerprint distinction and both paths of the shipped example.
  Models, clock, end presentation and settlement are controlled.
- `TestRoundOutcomesUnity.ps1`: 634 isolated Unity 6.6 Play Mode checks using
  production script sessions, shipped objective, actual font/view/fade and
  rendered changing text. The completed `3/3` screenshot was visually inspected.
  Round arbitration/score/winner methods are extracted from current `Fight.cs`;
  model/contact/clock/end-presentation/settlement inputs remain controlled.
  Unity's search-index startup logged an unrelated `ArgumentOutOfRangeException`;
  the objective completed and wrote explicit acceptance evidence. The managed
  fixture also prints MoonSharp's default Unity loader reflection warning under
  .NET; production mod asset loading and all acceptance checks still pass.
- All four managed assemblies compile with ignored temporary project copies
  remapped to installed Unity 6.6 (`6000.6.0f1`). Standard tracked project paths
  have the previously recorded local SDK/analyzer mismatch and are preserved.
- Editor generation/check/build, 46 unit tests, LuaLS 3.19.1 protocol tests and
  real VS Code integration pass; wiki coverage/types/build/search/link checks pass.
- Underworld runtime: 1282 assertions pass against freshly built game assemblies.
  `AuditUnderworld.py` still reports the previously recorded missing
  `fungus_raid` `/layer_0_2`; no location assets were changed.

This establishes a bounded round-objective contract. E2 stays open for broader
combat control and full-game acceptance. Actual contact ordering, complete native
end-animation selection, full result/reward/save lifecycle, removal/reinstallation
in a real profile and crash/restart acceptance are not proven by these fixtures.
The wider creator-platform goal remains active, including broader actor/scene
creation, pack conflicts/composition and independent creator workflows.

## 2026-10-03: additive fight rules across mods

Before this change, two independent `append_rules` patches to one fight conflicted
even when their rules were compatible. This prevented the shipped Focus add-on
and round-objective examples from running together. Additive contributions now
compose; replacements and conflicting result authority retain explicit rejection.
This advances E7/E8 without closing their broader requirements.

### Source and API contract

- `Runtime/Modding/ModContent.cs` gives fight rules an append/replace field policy
  without granting removal, records `append_rules` as `Append`, and accepts only
  compatible append/append overlap. The final immutable fight definition retains
  contributor order and every patch record. Exclusive replacements report all
  existing contributors. Duplicate handles, one-field-per-transaction and the
  aggregate 100-handle limit still reject before any catalog mutation. Existing
  controller validation checks the combined list before commit. Failures discard
  the incoming mod's entire transaction and do not poison later independent mods.
- `LegacyContentAdapter.cs` projects each `(target, field)` once. This is essential:
  each patch record references the final combined list, so applying all records
  separately would multiply static native rules. Original battle clones and the
  existing restore path remain in use. No recovered XML/assets/GUIDs were edited.
- Existing `DependencyResolver` ordering and `ModBattleRuleInstances` dispatch
  supply the order: dependencies before dependents, ordinal ID selection among
  ready mods, and authored order within each appended list. Discovery/folder
  order does not control the combined definition. Order-dependent effects remain
  sequential rather than being described as commutative.
- Existing fingerprints include all patch owners and ordered fight rule handles.
  Recording append operations changes fingerprints for older configurations that
  used them; public docs state this compatibility implication. Owned state IDs and
  schemas are unchanged. Removal still means an enabled-set change plus restart,
  not hot unloading during a fight.

The actual Focus/objective scripts and their editor starters now use separate
upper-left HUD placements. The new public Combine mods guide explains setup,
authority, order, conflicts, the whole-transaction failure boundary, removal and
verification limits. The fight patch/round references, examples, compatibility
and editor guides are updated. Authored schema/generated Lua definitions include
field hover guidance for additive contributions versus exclusive replacement.

### Verification

- `TestModPackRules.ps1`: 86 checks with production Lua, script sessions, catalog,
  dependency resolution, state/save data and current extracted native adapter
  apply/remove and round methods. Covers input discovery order, dependency-before-ID
  order, all replacement/append combinations, contributor diagnostics, controller
  conflicts/disjoint scopes, failed dependencies, later independent recovery,
  aggregate bounds, duplicate handles, static projection/restoration/reinstall and
  the three shipped mods across objective/whole-pack removal and reinstallation.
  Battle source storage and combat models/clock/settlement remain controlled.
- `TestRoundOutcomesUnity.ps1 -WithFocusPack`: 651 isolated Unity 6.6 Play Mode
  checks. The shipped framework/add-on/objective run together with a static-rule
  fixture, actual HUD placement/fit, recovered font, pixels, fades, Focus's bonus
  and custom round win/loss. Current adapter methods project normal/Eclipse source
  clones once and restore them exactly around controlled battle source storage.
  The combined `Focus: 0/3` and completed `3/3` screenshot was visually inspected.
- Standalone objective native fixture: 636 checks. Standalone framework native
  fixture: 33 checks, including saved-state removal/reinstall. Existing managed
  outcome (702), extension (126) and fight-patch (179) checks pass. The fight-patch
  runner now reads its existing examples from `ArchivedMods`, their authoritative
  location, instead of failing before execution on stale `Mods` paths.
- All four managed assemblies compile with the previously documented ignored
  Unity 6.6 reference remapping. Original tracked project paths remain unchanged.
- Editor generate/check/build, 46 unit tests, LuaLS 3.19.1 (including both field
  hovers) and real VS Code field-hover integration pass. Wiki coverage/types,
  build, search and links pass. Generated build/runtime artifacts remain ignored.

Unity's search-index startup again logged an unrelated indexing exception; the
explicit acceptance checks completed successfully. The managed fixture's default
MoonSharp Unity-loader reflection warning under .NET is also unchanged. Neither
is presented as a game failure or hidden by a claim of a clean editor log.

Full native map/menu lifecycle, actual contacts/AI/end animations, result/reward
and real-profile disk/crash/restart acceptance remain unverified. Arbitrary pack
compatibility, automatic HUD conflict resolution, broader engine operations and
source-free independent creator acceptance remain open. The full vision stays active.

## 2026-10-03: callback attribution and bounded runtime diagnostics

Modders previously needed to interpret general native logs to find a failing or
expensive Lua callback. The existing F3 performance overlay/F4 text report now
include the active mod session and callback attribution. This advances E8's
source-free diagnostics requirement; it does not close creator acceptance or
make runtime control domains complete.

### Source and creator contract

- New `Runtime/Modding/ModCallbackDiagnostics.cs` is independent of Unity and
  recovered assemblies. A session owns its collector; `ModApiFacade`,
  `ModScriptSession`, `ModHost` and `ModRuntime` share it across active mods.
  Captured rows contain only detached IDs, strings and numbers. The new `.meta`
  and narrow generated-project compile inclusion are tracked; all existing GUIDs
  and original project reference paths are preserved.
- `MoonSharpScriptRuntime.RunBounded` brackets execution/worker setup and cleanup,
  attributes the callback to its actual owner, records escaped exceptions, and
  marks instruction exhaustion at the actual forced-yield limit. Nested service
  calls retain parent IDs and subtract measured child time from caller self time.
  A handled provider failure remains visible while the caller can succeed.
  Existing worker reuse, capability checks, limits, callback order and exception
  propagation remain in place. Diagnostics never access event/fighter arguments.
- `UI/PerformanceOverlay.cs` starts/stops timing with the existing Off/Compact/
  Detailed setting. Detailed shows totals and the two highest accumulated self
  times, including during loading. Its larger text area was visually inspected.
  F4 report format 5 includes active mod versions in resolved load order,
  registration/state diagnostics, statistics, completion-order traces and errors.
  The existing report save/clipboard path is retained. No Lua function/capability
  or public content/save format is added; editor-generated contracts are unchanged.
- Successful calls are not measured/retained while the overlay is off. Failures
  are still retained in a separate 64-entry ring, so high tick volume cannot
  immediately flush them. Limits are 1024 owner/source statistics, 128 timed
  completions, 512-character source labels, 2048-character errors and 64 measured
  nesting levels. Overflow counts are explicit. A new script session starts clean.

The new public **Debug callbacks in game** guide documents setup, inclusive/self
time, completion-order nesting, handled errors, forced yields, bounds, lifetime,
instrumentation overhead and verification limits, with a supported Lua callback
fragment. Logging/troubleshooting/sidebar and editor guide link the workflow.
Timings are wall-clock observations; they do not grant aggregate frame budgets,
correlate callbacks to individual simulation frames or alter results/RNG/saves.
Only bounded execution is captured: parsing, host asset errors and pre/post-handler
validation can still belong to ordinary game logs or other diagnostics.

### Verification and limits

- `TestModCallbackDiagnostics.ps1`: 33 checks with production MoonSharp/session,
  actual nested provider/consumer calls, instruction exhaustion and recovery,
  handled/untimed errors, recording toggles, clipped messages, bounded rings and
  detached snapshots. A 10,000-call disabled-success check allocates no collector
  records. Message text cannot forge the instruction-limit classification.
- `TestModCallbackDiagnosticsUnity.ps1`: 21 Unity 6.6 Play Mode checks. Actual Lua
  runs through the production overlay's native Update and IMGUI paths; scripted
  F3/F4 keys control recording and write a real report/clipboard path. The captured
  overlay PNG was visually inspected, and the saved report's provider ownership,
  parent sequence, failure text and instruction-limit entries were inspected.
  Game/session lookup, keys and report directory are controlled. The fixture uses
  a hidden isolated editor with a rendering Game View: batch mode cannot present
  this IMGUI screenshot. Earlier batch attempts were corrected after explicit
  loading/capture failures; their results are not acceptance evidence.
- Existing extension (126), pack composition (86), round outcome (702) and
  culture-independent performance CSV (98) regressions pass. The shared showcase
  runner includes the new runtime source. Native fixture artifacts remain ignored.
- All four managed assemblies compile using `dotnet msbuild` and the previously
  recorded ignored Unity 6.6 reference remapping. Direct installed Visual Studio
  MSBuild still cannot resolve this machine's SDK; original tracked references
  were not rewritten to make that limitation disappear.
- Editor generate/check and 46 unit tests pass; no binding/hover surface changed,
  so LuaLS/VS Code integration was not rerun. Wiki dedicated-function coverage
  (214), types, build, search and 5462 links/assets across 55 pages pass.

The fresh Unity fixture again logged the unrelated Search indexing startup
exception; its explicit diagnostics acceptance completed. The .NET combat fixtures
still print MoonSharp's default Unity-loader reflection warning and then pass.
Neither log is described as completely error-free.

Physical keyboard/controller/touch export, full-game callback-to-frame attribution,
production overhead profiling, inspector/reload workflow, aggregate frame limits
and independent creator acceptance remain open. This feature is creator tooling,
not proof that arbitrary actors, scenes, combat systems or packs are now possible.
The full Minecraft-style modding objective remains active.

## 2026-10-03: queued fighter movement and Repulse

Lua abilities can now request relative movement of the callback's main fighter
or its opponent. Previously Lua could observe positions and change health, but
could not implement a retreat, dash or repulse through a supported displacement
operation. `fighter:move_by(x, y, z?)` requires `combat.motion`; the opponent
method additionally requires `combat.target`. This advances E2/E8 with a real
playable ability, while the broader creative-freedom objective remains active.

### Source and native boundary

| Source | Responsibility |
| --- | --- |
| `Assets/Scripts/Eclipse/Runtime/Modding/ModScripting.cs` | Independent typed motion interface, shared finite/axis/request limits and behavior-instance delegation. |
| `Assets/Scripts/Eclipse/Modding/MoonSharpScriptRuntime.cs` | Self/opponent bindings; strict numeric arguments, optional Z, capability checks, callback lifetime and begin/end rejection. Returns acceptance plus a reason, without running native translation inside Lua. |
| `Assets/Scripts/Eclipse/Modding/FightFighterMotion.cs` | Owned partial Fight implementation: eligibility, additive per-body queue, round/session identity, coalescing, pause retention, stale/dead/body cancellation and native failure isolation. |
| `Assets/Scripts/Assembly-CSharp/Fight.cs` | Narrow typed adapter and application before `RenderRound`, after model/collision/animation processing. Reset, next round, transition closure/unload and native surrender/failure clear pending motion. |
| `Mods/example.repulse/` and `Tools/ModdingEditor/templates/repulse/` | A Lua HUD button, intent consumed by fresh tick handles, independent retreat/opponent requests, round-scoped 180-frame cooldown and lifecycle cleanup. Appends its rule to Act I Tournament stage 3 in normal/Eclipse mode. |

Accepted requests add in callback order, shared across mods. Each axis of an
individual request and its running sum is bounded to ±1000 native units; at most
32 nonzero requests can be pending per body/step. A rejected request preserves
earlier ones. Zero requests use no slot; a net-zero sum skips native translation.
One translation per current participant uses `Model.ShiftModelPosition(delta,
true)`, which moves current/previous rig points and running keyframes/buffers.
Its native constraint/friction solver also adjusts rig positions and velocity.
Positive model Y is downward, supported by the recovered integration and floor
constraint code. This is displacement, not a velocity impulse or a safe-destination
query, and does not sweep collision along the path.

Paused application retains pending movement until simulation resumes; new
requests while paused reject. Eligibility is checked again before application.
Replaced/dead/detached bodies and ended rounds/sessions discard their requests.
Local versus, PvP, title sparring and unsupported raids reject; the owned offline
raid path is eligible. Self and opponent calls are independent, not an atomic
pair. Acceptance is not a completion receipt or an invulnerability/attack grant.

The wiki adds dedicated self/opponent sections and a beginner movement-ability
guide, with manifests, examples, sidebar and editor guide updated together.
The editor's authored schema, generated data/definitions, callback diagnostics
and template include the new contract. No executable Lua definitions ship.

### Verification and limits

- `TestFighterMotion.ps1`: 477 checks with production MoonSharp bindings and the
  complete production queue, controlled native models/clock/session/translation.
  Includes strict types, bounds, capabilities, expired handles, unsupported
  callbacks/modes, additive independent contexts, zero/cancelled sums, per-step
  limits, pause/resume, stale/dead bodies, cancellation and translation failures.
  The actual shipped Lua source also exercises click intent, rejected requests,
  cooldown, HUD teardown and next-round reinitialization.
- `TestFighterMotionUnity.ps1`: 8689 full-game native checks in an isolated Unity
  6.6 project with actual assets, boot, Campaign and the core encounter. Separate
  Lua rules produce observed self/opponent deltas of 56.9948/-30.1646 for queued
  sums +57/-30. Native simulation continues between observations. A bounded
  rig/physics/animation snapshot restores an identical baseline, then compares
  every XYZ rig point against the existing native translation path, including
  constraint corrections. It checks 410 real current/previous coordinates and
  2613 real animation-buffer/keyframe coordinates. This is not whole-game rollback.
- Native pause preserves movement, resume applies it once, and the shipped native
  HUD button queues approximately -39.9944/+100.1351 displacement. Its 180-frame
  cooldown recharges with running animation retained. Surrender through the
  recovered result path closes the HUD and clears queued motion. Callback failure
  history is empty. The final rendered `repulse-native.png` was visually inspected;
  the panel was moved below the portrait and given readable status styling.
- The matching hidden, graphics-enabled fixture uses controlled idle/immortal
  participants and a fresh isolated native save seeded with `Tutorial="END"`
  after the title releases its preview. An earlier fresh-profile attempt was
  blocked by onboarding; invoking its dialog handler without tutorial movement
  could not establish a valid tutorial fight. The corrected fixture explicitly
  excludes first-time onboarding rather than bypassing native HUD input gating.
  Earlier overly strict pure-translation assertions were corrected to compare
  native constraint behavior. Only passing final runs are acceptance evidence.
- Existing round-outcome (702), pack-composition (86), and model-transition
  regressions pass; transition closure now also asserts motion cancellation.
  All four managed assemblies compile using the previously recorded ignored
  `dotnet msbuild` Unity 6.6 reference remapping. Tracked machine references remain
  intact; the new owned source has its explicit main-project include and meta GUID.
- Editor generation/check, 47 unit tests, LuaLS completion/hover/full-example
  diagnostics and VS Code integration pass. Wiki reference coverage (216), types,
  site/search build and 5615 local links/assets across 56 pages pass. The initially
  missing default LuaLS test binary was resolved
  by passing the installed 3.18.2 executable explicitly.

Native artifacts are ignored under `Temp/FighterMotionUnity-*`; the reusable
fixture runner checks its marker/path and freshness of the result. It never
imports the test harness into the main game project. The editor still logs its
unrelated Search indexing startup exception, and the managed fixture prints the
known default MoonSharp Unity-loader reflection warning before passing. Neither
log is claimed error-free.

Vertical/Z movement, every arena/animation/contact combination, normal-versus-
Eclipse gameplay coverage, physical keyboard/controller/touch activation and
exported-player acceptance remain open. This does not provide arbitrary actors,
custom physics, attack animation authoring or swept collision. Native acceptance
of this ability is useful progress toward Minecraft-style modding, not completion
of the vision.

## 2026-10-03: explicit authored move playback and Active Strike

Creators can now start a registered attack/move from a live Lua ability instead
of requiring recovered input/AI/event selection to activate it. Self and opponent
`play_move` accept typed move handles owned by the current script. They require
`combat.animation`, with `combat.target` for the opponent. Requests return
transient queued/applied/failed receipts so Lua can distinguish acceptance from
native startup and start cooldown only after application. This advances E2/E5/E8.

### Source and contract

| Source | Change |
| --- | --- |
| `Runtime/Modding/ModScripting.cs` | Independent playback interface with completion/update delegate and behavior-instance delegation. |
| `Modding/MoonSharpScriptRuntime.cs` | Dedicated self/opponent binding, exact one-argument owned move handle, capability/lifetime/begin/end checks and receipt mutation. Updates do not execute Lua callbacks. |
| `Modding/FightFighterPlayback.cs` | Owned partial Fight queue and receipt lifecycle. Current native animation-cache membership is checked at request and application. Shared active-round/main-body eligibility is reused from motion. |
| `Assembly-CSharp/Fight.cs` | Typed native adapter, playback after motion before round arbitration/interpolation, and reset/next-round/transition-close/surrender cancellation. |
| `Assembly-CSharp/Model.cs`, `ModelAi.cs`, `Nekki/SF2/Core/Fights/ModelContainer.cs` | Narrow inferred rename of the model animation-cache getter to `GetAvailableAnimations`, marked `// best guess for name`; existing callers and controlled fixtures updated. The unrelated SF2Paths token is unchanged. |
| `Mods/example.active-strike` and `Tools/ModdingEditor/templates/active-strike` | An owned punch definition, compatible binary, Lua HUD intent/readiness, polling of playback receipt, round-scoped 120-frame cooldown and cleanup. Additive encounter patches in normal/Eclipse mode. |

Owned source paths above are relative to `Assets/Scripts/Eclipse/`; recovered paths
are relative to `Assets/Scripts/`. The new partial has its own meta GUID and main
project include. Existing GUIDs and serialized identity are retained. Current
callers/fixture declarations and serialized Unity text references were searched;
this guess does not enter confirmed deobfuscation mappings.

Native playback uses the existing named `Model.PlayAnimation` path and its
readiness/physics checks. Explicit playback deliberately does not run selection
events/conditions, input bindings, priorities, cancel windows or player control
restrictions. Authors may interrupt running animation and define ability costs,
readiness and cancellation policy in ordinary Lua. Rig/equipment/locks determine
the fighter's available native animation cache; cache membership alone does not
prove appropriateness in every state. Static attack/animation/effect data stays
typed/declarative. No operation DSL or arbitrary engine-name call is introduced.

One request per current main body/step is shared across mods. The first accepted
request wins, including against a later identical request; competitors get a
failed receipt without overwriting it. Bodies have independent slots. A later
handler error does not roll back an accepted request. The batch detaches before
native work and rejects playback requests during application to prevent recursive
start/end chains. Native failure on one participant does not suppress the other.
`applied` means startup, not completion/contact/damage. Receipts are transient
tables, cannot control the host by editing fields and are never saved.

Pause retains queued work; death, replacement/detachment, unavailable move,
round/session change and teardown fail it without replay. A completion-update
exception is isolated. Same-step displacement applies before playback; operations
are independent rather than an atomic ability transaction.

### Verification and limits

- `TestFighterPlayback.ps1`: 283 checks execute production MoonSharp and the complete
  playback/motion queues against controlled model/asset/session/clock services.
  It covers strict/forged/expired handles, dot/colon calls, capabilities and
  callback timing, both participants, independent mod conflicts, later handler
  failure, stale/dead/replaced state, pause/resume, cache loss, native rejection/
  exception, receipt failure, recursion and source cancellation/boundary hooks.
  The actual shipped example covers HUD intent, queued/applied/failed receipts,
  cooldown, unavailable equipment, teardown and next-round reset.
- `TestFighterPlaybackUnity.ps1`: 17 full-game native Unity 6.6 checks. Real boot,
  Campaign and Act I Tournament stage 3 use the shipped authored move/HUD button.
  Startup is deferred from the click, the owned native animation plays, its
  animation-start callback fires once, and the real attack reduces enemy health
  from 1 to 0.9966142774. Cooldown is ready after 122 frames from click (startup
  receipt is observed before its 120-frame cooldown begins). The applied HUD
  state is observed with its button disabled, and premature recharge is rejected.
  No callback failures
  are retained. Another queued request survives native pause, then surrender
  fails its receipt, clears the queue and closes the HUD.
- Controls/AI/spacing and a fresh isolated post-tutorial profile are controlled;
  first-time onboarding is excluded. Native acceptance uses the real rig and
  binary, attack intervals/contact/damage path and rendering. The final right-side
  HUD PNG was visually inspected, leaving space for the Repulse panel. This is
  one normal-mode encounter; no complete damage/balance or all-arena claim follows.
- `high_punch.bytes` is an unchanged base-game asset copied into the example and
  template, SHA256 `EE6A6CDBFDEEC580EFCA6A705A42EF1DDEE08B2ED3D0E2ACA6161DFCB582F8E7`.
  This verifies reuse of a compatible binary, not an original animation exporter.
- Motion (477), round outcomes (702), AI eligibility (14), showcase editor and
  model-transition regressions pass. Transition closure also checks playback
  cancellation. All four managed assemblies compile with the recorded ignored
  Unity 6.6 `dotnet msbuild` remapping; tracked references stay intact.
- Editor schema/generated data/types/template, manifest/timing diagnostics and
  guide update together. Generation/check, 48 unit tests, actual LuaLS completion/
  hover/typed receipts/full-example diagnostics and real VS Code integration pass.
  Wiki reference coverage (218), Astro types/build/search and 5772 local links/
  assets across 57 pages pass.
  On Windows the npm-forwarded VS Code path was escaped incorrectly; the same
  built test was rerun successfully through the direct Node launcher.

The fresh native editor logs its unrelated Search indexing startup exception;
the controlled .NET fixture prints the known MoonSharp default Unity-loader
reflection warning before passing. Neither log is described as error-free.
Native PNGs/logs/profiles and test artifacts remain isolated/ignored. Physical
keyboard/controller/touch activation, Eclipse-mode playback, all equipment/rig/
arena/cancel combinations, exported builds, original animation authoring and
arbitrary actors/physics remain open. The full Minecraft-style objective remains
active; this feature adds explicit ability control rather than closing the vision.


## 2026-10-03: owned audio instances and Audio Lab

Creators can now start sound independently of a move or screen effect, retain a
playback handle, query liveness, change its volume and stop only that instance.
This advances E6/E8 for ability cues, hazard telegraphs and custom UI. The existing
character/animation tools were inspected before choosing this missing domain;
no second exporter was created and no original-clip combat acceptance is claimed.

### Source and API route

| Source | Change |
| --- | --- |
| `Assets/Scripts/Eclipse/Runtime/Modding/ModAudioRuntime.cs` | Engine-independent options, backend/voice interfaces, per-script 16-voice scope and owned instance lifecycle; optional UI-owner close subscription. |
| `Assets/Scripts/Eclipse/Modding/MoonSharpScriptRuntimeAudio.cs` | `sf2.audio.play`, `is_playing`, `set_volume`, `stop`; exact argument/option types and counts, `audio.play` capability, actual asset/instance/UI-handle identity and weak instance lookup. |
| `Assets/Scripts/Eclipse/Modding/MoonSharpScriptRuntime.cs` | Audio backend injection, module binding, registration/cleanup playback exclusion and script-context shutdown. Existing constructor calls remain compatible through an optional argument. |
| `Assets/Scripts/Eclipse/Modding/ModAudioPlayer.cs` | Independent non-spatial Unity AudioSources, shared 64-voice limit, saved sound-volume/mute multiplication, game/real pause semantics, natural completion and active-scene teardown. No Lua executes inside this backend. |
| `Assets/Scripts/Eclipse/Runtime/Modding/ModContent.cs` | Exposes the shared catalog-freeze observation through registration so a loaded provider cannot play from a still-loading dependent. |
| `Assets/Scripts/Eclipse/Modding/ModRuntime.cs` | Supplies the native backend to actual game script contexts. |
| `Mods/example.audio-lab` and `Tools/ModdingEditor/templates/audio-lab` | Original beacon WAV and four native HUD buttons, game/real loop selection, quieter playback, explicit stop and round/fight cleanup through UI ownership. |

The three new owned C# files have new meta GUIDs. Existing meta files and recovered
code/assets are untouched. Main/runtime project includes were added with their
original references/BOM retained; portable Unity 6.6 compile projects remain ignored.

`sf2.assets.audio` supplies a typed asset handle resolved by this context. From a
runtime callback, `sf2.audio.play(asset, options?)` validates volume (finite 0..1),
loop, clock and optional open UI owner. It returns an opaque owned instance/nil
or nil/error on runtime refusal. Structural/capability/timing misuse raises a Lua
error. Other audio functions accept only an instance owned by the same context;
writing fields on an asset/instance table cannot create, redirect or control a
native voice. Instances are transient and cannot be saved or passed through the
primitive-only framework service contracts.

The scope retains at most 16 active voices; the native backend shares at most 64
across scopes. Paused/muted voices count. New requests fail without evicting older
voices. Completed/stopped instances free slots; stopped handles stay safe to query,
update (false) or stop again (false). `is_playing` means active, including paused or
muted, rather than proving an audible signal.

Both clocks use normal real-time clip rate. Game means native combat/listener
pause following, not sample-accurate simulation, slow-motion pitch or time-scale
control. Real ignores combat/listener pause while still following saved sound
volume/mute and normal listener effects. Native startup has a first-frame grace
period and retains voices during listener/clip loading pause instead of mistaking
`isPlaying=false` for completion. There is no spatial/pitch/pan/seek/stream or
music-channel API in this change.

All voices close on active-scene change or script/mod shutdown. An optional UI
owner additionally closes its voices, including on UI callback failure. Without
an owner, round/fight end does not itself stop a sound: author lifecycle cleanup
in Lua. UI cleanup and an unfrozen registration catalog cannot start new audio, including
a loaded framework provider invoked while a dependent is still loading. The
example owns its loop through its
round HUD and closes that HUD in both round/fight end callbacks.

### Verification and limits

- `TestAudioRuntime.ps1`: 153 checks execute the production Lua binding and scope
  against controlled native voices. Covers defaults/options/updates/stop, fake/raw
  handles, strict counts/types/nonfinite numbers, unknown fields, capability and
  registration/cleanup exclusion (including post-entrypoint/pre-global-freeze),
  unavailable/failed backend, per-scope bounds,
  completed-slot reuse, UI/script shutdown and independent-scope ownership.
  The actual shipped manifest and Lua source register against canonical stages;
  real HUD click handlers exercise both clocks, replacement, volume, stop and
  round/fight cleanup. The payload in generic metadata tests is controlled; this
  is not an audio decoder or physical device test.
- `TestAudioUnity.ps1`: 104 full-game Unity 6.6 checks, actual boot/Campaign/core
  Tournament 3, fresh isolated post-tutorial profile and actual Audio Lab/native
  buttons. The original 22,050-sample WAV decodes to a nonzero signal, native
  timeSamples advances, volume follows 0.8 saved level then a 0.25 multiplier,
  and saved mute yields zero source volume without losing the instance.
  Game-clock samples stay fixed through combat and listener pause then resume;
  real-clock playback advances through both. Natural completion and explicit
  stop release sources. Four controlled native scopes fill the shared 64-voice
  pool; another request rejects without eviction. This is backend composition,
  not four independently installed audio mods. UI/scope/scene cleanup leaves no
  retained source. Callback failure history is empty. The rendered Audio Lab HUD
  was captured and visually inspected alongside the existing Active Strike HUD.
- Controls/AI and post-tutorial profile are controlled; the native case is normal
  mode. Physical controls, listening on the user's audio device, all platforms,
  Eclipse-mode audio and exported builds remain unverified. No device audibility
  or sample-perfect scheduling claim follows from source/clip checks.
- `beacon.wav` is original mono PCM16 22,050 Hz, one second, a 660 Hz tone with a
  sine-squared first-0.2-second envelope at 18% amplitude, then silence. SHA256
  `F0B913926FCE2F93F6BECB1555986D6E945A89562E8E9B6A9BB7B84EAC6CE9EF`.
  Example/template files are compared by the editor unit test.
- Existing playback (283), trial-rule (50) and Phase1 runtime registration tests
  pass. Both explicit-source fixture lists now include audio; the trial fixture
  also needed its previously omitted extension/diagnostic/outcome dependencies.
  Its pre-existing duplicate-source warnings remain. The playback .NET fixture
  still prints its known default Unity-loader reflection warning before passing.
  All four managed assemblies compile through the recorded ignored Unity 6.6
  remapping; no tracked reference rewriting is included.
- Editor generate/check and 49 unit tests pass. Actual LuaLS 3.18.2 offers audio
  options, distinct instance types, capabilities/wiki hover and a clean complete
  Audio Lab source; the full integration suite passes. Real VS Code integration
  passes, including the new four-function audio completion check. Wiki coverage
  (222 public sections), Astro types/build/search and 5928 links/assets across 58
  pages pass. Public API, asset guide, capability table, examples, editor guide,
  schema/generated data/types and mirrored starter update together.

The first native attempt exposed a test-harness local-name compile collision;
after repair, the shipped wildcard dependency range was rejected by actual
manifest parsing. It now uses the supported core comparator range, with managed
shipped-source coverage. A later native boot hit system-drive exhaustion while
extracting TAR data. Automatic approval review rejected removing generated cache
files (blocked by policy); no removal occurred. The runner now creates fresh
acceptance profiles with a junction to a hash-addressed immutable cache inside
its marked project-drive fixture. Existing profiles/caches remain intact. Fresh
native passes above use that route; logs/results have unique/freshness checks.
The editor's unrelated Search startup exception remains; logs are not claimed
error-free. Native artifacts and fixture data are ignored.

Owned audio is implemented and accepted in this scenario. Timed arena hazards,
arbitrary actors/physics, spatial presentation, music-channel ownership, physical
input/device listening, exported platforms and independent newcomer acceptance
remain part of the full active Minecraft-style objective.


## 2026-10-04: arena geometry, owned warnings and Pulse Arena

Continued the active full Minecraft-style objective through E6/E8. This is a new
world seam: mods can author a timed environmental hazard with native pose sensors
and owned arena art using ordinary Lua. The broad G01–G14/E1–E8 scope remains
active; no overall completion percentage or platform-complete claim is made.

### Source and public contract

- New `Runtime/Modding/ModArenaRuntime.cs` holds an engine-independent rectangle,
  exact capsule/rectangle XY distance test, `IModFighterRegions`, native marker
  interface and per-script scope/instance. Rectangles require finite minimum X/Y
  in -10000..10000 and positive width/height <=4000. Boundary contact counts;
  rounded corners are not approximated by an inflated AABB.
- New `Modding/ModArenaGeometry.cs` consumes the native collision-edge list with
  current endpoint positions, endpoint margins and radius, capped at 512 edges.
  This is the list filled by `ModelLoader` for `Type=Edge, Collisible>0` and read
  by `ModelCollision.CrossModel`, including any collidable equipment edges. It
  ignores attack/defense/invulnerability flags and Z. No swept/solid collision,
  new physical actor or attack reaction is implied.
- `ModelObject.GetCollisionEdges`, `ModelEdge.GetCollisionRadius`,
  `GetStartMargin`, and `GetEndMargin` are narrow getter-name guesses supported
  by loader/collision/segment code, each marked `// best guess for name`. Only
  verified owning-type callers changed; no confirmed recovery map was amended.
  `Model.GetRenderObject` adds a typed rendering seam without changing legacy
  getter callers. The DE128 native tier-boss validator uses the renamed collision
  getter as well. Existing asset/meta identities are preserved; four new source
  files have new metas. Project source lists include the new files.
- `Fight.EclipseFighterOperations` supplies current-main-fighter/offline-round
  geometry and round-bound marker creation. Native PVP/online raids, local versus,
  title sparring and stale/ended rounds reject. Mod-owned offline raids use the
  same contract. `ModInstanceFighter` forwards both operations through rule/perk
  wrappers. Creation captures fight/round identity, not the lifetime of the old
  player model; current render coordinates follow a replacement model.
- New `Modding/ModArenaMarkerRenderer.cs` owns each native mesh/material/object,
  projects through the current player render transform and uses the arena parent.
  Filled rectangles inherit native mirroring, use Sprites/Default and normal
  deferred Unity destruction. Script disposal hides immediately; native round/
  fight/arena invalidation removes art by the next presentation frame. Pause
  retains art. A full session may have 64 active markers; excess creation rejects
  without eviction. Handles are transient, never saved.
- New `MoonSharpScriptRuntimeArena.cs` binds `fighter:overlaps_rect`,
  `fighter.opponent:overlaps_rect`, `fighter:mark_rect` and
  `sf2.world.{set_marker_color,is_marker_active,remove_marker}`. Fighter callable
  references expire at callback exit. Self observation has no additional cap;
  opponent observation requires `combat.target`. Marker creation/operations require
  `presentation.visuals`. Strict shape/color/argument/handle identity checks reject
  numeric strings, nonfinite values, unknown fields and forged/foreign handles.
  Creation cannot occur during registration, begin/end callbacks or UI cleanup.
  Queries share 32 calls per callback across self/opponent; a script owns 16 markers.
  These bounds do not close the aggregate native/Lua frame-budget requirement.
- Native testing revealed `round.processing` can remain true during the surrender
  animation after round/fight-end callbacks. Both native script dispatch methods
  now suppress Tick after `_eclipseEndedRound` or `_eclipseFightEndDispatched`.
  This preserves cleanup; no recovered native simulation/animation timing changed.
- `Mods/example.pulse-arena` and the mirrored editor starter derive a fixed column
  from the initial fighter midpoint, then run two-second warning, active and safe
  phases. Every 30 active ticks, geometric contact loses 0.025 normalized health
  through the existing direct `change_health` route. That route bypasses native
  attack/block/armor/critical calculations, shields and hit reactions. Lua owns
  phase arithmetic and policy; no hard-coded Eclipse hazard behavior was added.
  Round state, owned marker/HUD cleanup and closed-HUD checks are explicit.
- Wiki arena reference has dedicated sections for all six bindings; callback,
  fighter/location pointers, capability table, examples, sidebar and editor guide
  update together. Editor authored schema, generated contracts/types, callback
  timing diagnostics, unit/LuaLS/VS Code tests and starter mirror the contract.
  Explicit managed Lua fixture inventories now include the arena module/runtime.

### Verification and limits

- `TestArenaRuntime.ps1`: 545 checks against production rectangle/Lua/scopes plus
  the shipped hazard. It covers tangent/rounded-corner/degenerate capsules, strict
  invalid inputs, expired references, scope teardown, unavailable/rejecting hosts,
  query/marker bounds, global-registration and UI-cleanup guards, MoonSharp foreign
  resource isolation, exact phase/contact/cooldown sequence and new-round reset.
  Native geometry/render/clock providers are controlled in this managed runner.
- `TestArenaUnity.ps1`: 104 full-game checks in Unity 6.6, Campaign/core normal
  Tournament 3 with the actual shipped script. It checks native rig contacts and
  pose displacement, warning/recolor projection, native health loss, pause/resume,
  recovery/recurrence, 64 shared markers through four controlled native scopes,
  scope/surrender cleanup and direct suppression of both ended-round Tick paths.
  This is not four installed hazard mods or physical-input acceptance. Input/AI/
  spacing are controlled and the fresh profile begins after tutorial. The rendered
  PNG was inspected with Pulse Arena, Audio Lab and Active Strike HUDs visible.
- The runner uses unique profiles and freshness-checked logs/results, sharing only
  immutable TAR cache data through a junction into its marked project-drive fixture.
  Native artifacts stay ignored under Temp. The root scene/profile is untouched.
- Audio runtime (153), battle rules (268), trial rules (50) and prerequisite Phase1
  registration pass. The battle runner now falls back to its archived example
  (the active Mods path was removed earlier); this changes fixture lookup only.
  Existing trial-runner duplicate-source warnings remain. All four managed projects
  compile through ignored Unity 6.6 reference remapping, without tracked reference
  rewrites. The original managed/native checks prove different parts of the contract.
- Editor generate/check and 50 unit tests pass (228 bindings, 74 constants, 281
  types). LuaLS 3.19.1 protocol integration passes, including arena table fields,
  opponent sensors and complete hazard diagnostics. Real VS Code integration passes
  14 checks, including world module and fighter sensor/marker completion. The absent
  default LuaLS path was resolved with the installed isolated extension binary;
  passing Code.exe directly to Node avoids npm's Windows space-path escaping.
- Wiki Astro types/build/search and 6093 internal links/assets across 59 pages pass.
  The existing duplicate 404 route warning remains; there are no Astro type issues.

Early native attempts found an incorrectly assumed X=0 stage origin and then
post-surrender ticks accessing an already-closed HUD. Initial health alone was
insufficient contact evidence. The example now centers from live snapshots; the
native test checks its actual Lua contact counter together with health loss, and
the engine suppresses ended-round ticks. Managed fixture setup errors (foreign
MoonSharp resource injection and missing logical UI mount) were corrected without
relaxing production ownership/cleanup checks. Unity's unrelated Search startup
exception remains in logs; logs are not claimed error-free.

This provides a rendered timed hazard with real geometric contact for one native
arena. General actors, solid/swept physics, arbitrary camera/lighting control,
all-arena/form behavior, physical input, exported platforms and independent
newcomer acceptance remain required parts of the active objective.

## 2026-10-04: native projectile ability and caster attribution

Arc Dart (`Mods/example.arc-dart`, mirrored editor starter) completes a native
projectile ability using existing typed move registration, explicit queued
playback, scheduled projectile creation, resolved core equipment, native attack
edges, lifecycle callbacks and owned HUD. Lua owns activation intent and a
180-simulation-frame cooldown; static graphs own cast/launch/flight, contact
attacks and hit/miss deletion. No new public binding or operation DSL was added.
The three bundled animation binaries are unchanged base exports. The guide,
callbacks reference, example index, sidebar and editor guide explain the contract.

Source changes fix two failures found in the actual game. SelectAnimation now
snapshots and clears its pending action batch before executing it; reentrant
animation events survive for the following selection pass without invalidating
the current enumerator. Fight resolves Lua attacker identity through the native
child's root main fighter, including event/strike fallback and both sides' damage
notifications. Native contact actor, equipment, damage arithmetic and target stay
unchanged. Model.GetRootModel is a narrow descriptive guess with the required
comment; existing callers and three character-form fixtures use that name.

Verification: TestArcDartUnity passes 28 full-game Unity 6.6 checks in Campaign
normal Tournament 3: native WeaponModel/SkeletonMissile and resolved shuriken,
visible child, launch/flight lifecycle, 213.9264 units observed travel, contact
health loss from 1 to 0.9836771 and caster Lua callback, pause/resume, hit deletion,
miss expiry without extra damage, cooldown and surrender after expiry/HUD cleanup.
The early-flight PNG was inspected. Input, AI and spacing are controlled and a
fresh profile begins after tutorial; the root project/profile is untouched.
The runner shares immutable TAR cache data through a junction in a marked Temp
fixture and checks unique profiles, process completion and fresh result/log data.

Managed playback passes 492 checks, including actual shipped Lua and graph,
receipts, cooldown, failed-start recovery and round reset. The production-method
perk/damage fixture passes 58 checks for nested roots, both sides, fallback,
mutation and unrelated-root/NPC exclusions. Scheduled event batching passes five
checks; all three affected character-form fixtures pass. Assembly-CSharp compiles
against the ignored Unity 6.6 remapped project. Managed providers are controlled;
these checks are distinct from the full-game run.

Early attempts exposed a fixture mod-root mismatch, an unsupported direct
cross-rig hand alignment (native index error), the queue crash and missing caster
attribution. The example uses compatible native launch-origin alignment followed
by its missile's own flight point. No general rig-alignment fix is claimed.
Harness assumptions about the NoRanged placeholder and querying a deleted model
were corrected without changing production behavior. Unity's unrelated Search
startup exception remains; logs are not claimed error-free. The existing
MoonSharp loader reflection warning remains in the passing managed fixture.

This proves one creator-owned native projectile graph. Live child handles,
steering/homing/bouncing, original rig import, reverse-facing/interrupted-cast
and live-child surrender acceptance, all arenas, physical input and exported
players remain open. G01–G14 and E1–E8 retain their broader scope.

Final tooling checks: all four managed projects compile through ignored Unity
6.6 reference remapping. Editor generate/check/build and 51 unit tests pass
(228 bindings, 74 constants, 281 structures); actual LuaLS 3.19.1 integration
includes the complete Arc Dart script, and actual VS Code integration passes its
14 checks. Wiki types/build/search and 6249 links/assets across 60 pages pass;
the existing duplicate 404 route warning remains. Generated editor contract
changes are description updates, not newly supported functions.

## 2026-10-04: requested experimental perspective rendering

After committing/pushing Arc Dart, the user requested a 3D rendering experiment
before broader API work continued. Options > Display now has an off-by-default,
immediate persistent 3D fighters switch. Recovered interpolated XYZ, rounded
solid limbs, thickened native polygons and a lit perspective camera supply a
first approximation. Presentation depth separation is attenuated to 35% so
panels authored for flat overlap remain closer to the solid body. The active
fight viewer exclusively owns the additional pass; arena/HUD/menu projection
and native simulation remain unchanged. No public Lua function was added and
this does not close the arbitrary camera/actor/character pipeline requirements.

See Docs/Engineering/EXPERIMENTAL_FIGHTER_3D.md for the exact source, geometry,
camera reflection/ownership, limits and failed attempts. Full-game Unity 6.6
Campaign acceptance passes 26 checks, including actual Settings click/save,
projection orientation, native depth, 176 volume meshes/normals, menu isolation,
paused pose invariance, off restoration, animation resume, default reset and
camera teardown. Final before/after/Settings PNGs were inspected. Four managed
projects compile; 19 interpolation checks and wiki types/build/search with 6249
links across 60 pages pass. Input/AI/spacing are controlled. Geometry remains
rough, with seams and flat cloth/head panels; all equipment/forms/arenas, mod
visual integration, physical input, exported players and performance are open.

## 2026-10-04: programmable owned native projectiles

Typed projectile actions now project action-declaring ownership and a bounded
simulation lifetime (1..600 frames, default 180) into ActionCreateModel. Native
Model.SpawnWeaponModel checks capacity/eligibility before construction and records
ownership before the create event. The new FightProjectiles partial owns bounded
references, queued additive motion/deletion, TTL and round/form/session cleanup.
Native deletion also removes newborn-list membership, so a removed child cannot
be added back from the pending birth list. Two narrowly inferred native names
(SpawnWeaponModel and RequestModelRemoval) carry the required best-guess comment;
existing callers/event registrations are updated. Session disposal is explicit.

The pure shared contract is ModProjectiles.cs. Lua gains fighter:projectiles(),
projectile:snapshot(), projectile:move_by() and projectile:remove(), guarded by
combat.projectiles and callback scope. Queries select the calling mod and root
fighter, never by actor name. Commands recheck owner, session, root, round,
membership and native deletion; 16 children/mod, 64/fight, 32 queries/callback,
32 nonzero commands/child/step and 100-unit per-axis aggregate movement bounds.
Translation uses the existing native rig/keyframe/buffer path after collision,
preparing the next discrete contact step; this does not supply swept collision.
Nondefault lifetime is fingerprinted; old default fingerprint encoding remains.
The new Return Dart example/starter performs a turn-and-return trajectory in
ordinary Lua with a zero-velocity looping native flight and a 120-frame safety
lifetime. It returns to recorded launch X, not to a moving hand. Native attack
edges/damage/strike deletion still perform contact. The public wiki, reference
inventory/sidebar and editor authored/generated contracts update in this change.

Verification: 166 actual MoonSharp/complete production native tracker checks with
controlled membership/translation cover ownership, stale/forged/retained methods,
argument/query/request/aggregate bounds, pause, removal, TTL, per-mod/global
capacity, round/session/disposal/root/death/owner retirement. 797 cumulative actual
Lua/projection/native action-parser checks cover metadata, strict lifetime fields,
archive compatibility and fingerprints. 492 existing playback checks pass. Four
managed assemblies compile through the ignored Unity 6.6 reference remapping;
root generated projects still contain Linux-local analyzer paths, so the parser
runner's build was redirected to the equivalent ignored compile project and
actual Unity managed references were preloaded. Existing standalone MoonSharp
loader reflection/native-call warnings remain.

Full-game Unity 6.6 Campaign Tournament 3 acceptance passes 32 checks: typed
cast/child rig/item/rendering, native hit (1 -> .9836771 health), caster Lua
attribution, paused lifetime/position/contact, guided missed flight travelling
254.75 sampled native units then reversing/returning/removing, cooldown and
surrender with a live child (owned references retired, rendering destroyed).
Inputs/AI/spacing are controlled; the hit target is held at a controlled X after
launch. Initial fixture root, reverse-direction measurement and surrender/current-
fight lifetime assumptions were corrected. The first hit scenario's moving
native enemy made travel unreliable; target spacing is now explicitly controlled.
An interrupted fixture left a scene-backup dialog; its backup was preserved in
that marked Temp fixture before rerunning. Root scene/profile were untouched.

Editor generate/check/build and 52 unit tests pass (232 bindings, 74 constants,
283 structures). Actual LuaLS 3.19.1 includes the complete Return Dart script;
actual VS Code passes 15 checks including inferred projectile methods. Wiki
build/types/search and 6412 local links/assets across 61 pages pass, with the
existing duplicate 404 warning. All fixture output remains ignored.

This closes the scoped live-control seam for typed native projectiles, not a
general actor platform. Native-world TTL under script failure, all arenas,
exported players, physical input, complicated trajectories, competing native
projectile motion and general rig imports retain broader acceptance work.
Arbitrary actor spawning/AI/contacts, original character rig authoring and the
remaining G01–G14/E1–E8 requirements remain open. The user's requested procedural
3D refinement follows this feature's commit/push before creator work continues.


## 2026-10-04: sculpted procedural fighter refinement

After committing/pushing owned projectile control and Return Dart as d1740b84,
the requested rendering refinement replaces uniform body capsules with standard
Skeleton anchor-driven tapered elliptical lofts. Arms continue through elbows;
legs continue through knees, ankles and feet. Torso/pelvis/head/hands have separate
profiles. Region-specific depth replaces global .35 attenuation. The native
ModelPresentation retains its Model; MeshNode/ModelLoader carry figure provenance
without changing node identities, triangle indices or serialized assets. The
narrowly inferred AddTriangle name has the required best-guess declaration comment.
New Eclipse-owned ProceduralFighterBody source has a fresh meta GUID; existing
metas, recovered assets, physics and collision remain unchanged.

FighterVolume constructs connected clothing panels with shared subdivision and
normal vertices, thin perimeter walls, curved interiors and broad folds, rather
than extruding every triangle. Recognized weapon-name groups retain sharp edges;
unknown native equipment identifiers still need classification. The shader uses
near-black matte diffuse shading and faint directional arena-coloured edge light,
removing steel grey/specular/blue rim. Perspective keeps the original camera
rotation. Native child/custom rigs missing standard anchors retain fallback
panel/elliptical-stroke geometry. Public visual docs and the engineering rendering
record update in this change; no Lua bindings/editor contracts change.

Final Unity 6.6 full-game acceptance passes 34 checks with both standard bodies
(over 3000 vertices each) and six volumes, no old body capsules underneath.
Coverage includes actual Settings toggle/default/persistence, original camera
rotation/reflected orientation, isolated viewer ownership, paused native frame/
health/pose, live off restore, resumed animation and camera/mask teardown.
Connected cloth and rigid quad fixtures verify shared topology and no internal
walls; coincident fallback geometry is finite. The final log is
Temp/FighterPlaybackUnity-d997a2c104cd4564b9767ae6eb4b68d8/validation-6b95ef7fa60943668a135f8b57f135b7.log.
Inputs/AI/spacing are controlled in Campaign Tournament 3 with empty mod root,
fresh isolated profile and shared immutable TAR cache. Root scene/profile are
untouched. Existing Search/preview warnings remain.

Screenshot review corrected nonexistent hand-node assumptions, disconnected
ankle/foot primitives and BODY_WOMAN prefixes leaving old opponent capsules
visible. Orthographic, default 3D, 4x bright inspection, resumed animation and
Settings images were inspected and copied to ignored Temp/Procedural3DReview-20261004.
The brighter image uses the same geometry with an acceptance-only exposure
multiplier; it deliberately reveals remaining outfit defects. Shoulder/hip
section intersections, simple head/hand shapes and noisy/separate complex armour
are not claimed solved. No complete skinned-character importer, textures, 3D
arena or effect integration is supplied. All outfits/arenas/forms, physical input,
exported builds and sustained performance remain broader acceptance work.

All four managed compile projects pass using ignored Unity 6.6 reference
remapping; 19 frame-interpolation assertions pass after matching Unity references
are preloaded and remapped assemblies copied to the runner's ignored directory.
Wiki build/types/search and 6412 local links/assets across 61 pages pass (existing
duplicate 404 warning). This is a renderer experiment refinement, not closure of
the broad Minecraft-style creator-platform goal; G01-G14/E1-E8 remain open.


## 2026-10-04: copied native attack-source observations

The API could route child projectile damage to its caster but could not distinguish
that contact from a punch or another child. Native hit/damage/block/critical events
now expose optional `event.attack`: kind (fighter/projectile/native_child), actual
model name, strike animation, finite native contact point, and tracked typed
projectile observation ID/declaring mod. IDs match live projectile snapshots and
are fight-local data, not handles. Reading needs no extra capability; acquiring
or controlling live children retains combat.projectiles/owner/lifetime guards.

The immutable pure ModAttackSource contract lives in ModProjectiles.cs. Optional
source fields on ModIncomingHit and ModDamageEvent preserve existing source callers.
FightProjectiles captures ownership from its tracker and copies the point/move.
Fight captures source before PostHit callbacks and retains it through outgoing,
incoming and resolved damage. MoonSharpScriptRuntime copies a fresh nested table
for each callback via the projectile partial; table mutation cannot alter a later
side/event or retain the native actor. Missing native/synthetic producer data
omits attack. Untracked native children supply no invented projectile ID/owner.

Native acceptance exposed a real early-phase attribution bug: Model.Strike fills
StrikeResult.AttackerModel before OnModelPostCrit but refreshes the reusable
EventModel target later. Lua attribution now prefers the current strike, with
EventModel only a fallback. Native damage/perk calculations and event order are
unchanged. The StrikeResult attacker/animation fields are narrowly named
AttackerModel/AttackAnimation with best-guess comments; actual owning type/callers
and reflection-name use were checked. Model, Fight, EventHit and PerkInfoItem
callers update; unrelated instances of the shared obfuscated attacker token are
preserved. No confirmed deobfuscation mappings or serialized assets change.

Return Dart and its mirrored editor starter filter applied-damage observations by
source kind, owner and actor name. The public callback/projectile/ability/editor
pages, editor guide, authored schema and generated definitions update together
(232 public bindings, 74 constants, 284 structures).

Verification: 184 production MoonSharp/full tracker checks with controlled native
models cover copied source data in all eight callback types, absent producers,
root/unowned-child kinds, IDs, invalid points, data surviving child retirement,
callback mutation isolation and existing projectile ownership/lifetime/limits.
No geometry/contact claim is made for that fixture. Sixty extracted current
hit-phase/status-icon/routing checks pass, including stale event targets on both
sides, native ordering/reentrancy/versus guards and outgoing arithmetic.

Unity 6.6 full-game Campaign Tournament 3 passes 40 Return Dart checks. An
acceptance-only second rule observes the same live child ID across HitPostCrit,
PostHit, DamageDealing/DamageResolving and DamageDealt/DamageReceived on both
sides; it mutates its copied actor/point and later callbacks retain original data.
Native contact reduces health 1 -> .9836771; missed flight turns/returns, pause,
cooldown, strike deletion and live-child surrender cleanup remain passing.
Input/AI/spacing and fresh post-tutorial profile are controlled; the marked Temp
fixture adds its probe to the same single fight patch, leaving shipped Lua and
root scene/profile untouched. Initial duplicate probe patch and stale-event
attribution failures were corrected. Final log:
Temp/FighterPlaybackUnity-d997a2c104cd4564b9767ae6eb4b68d8/validation-4a3530fd9d294d5aa9c4ffc1dfde7d76.log.
Native critical/block notifications beyond the unblocked/no-critical dart are
covered by controlled events, not this full-game contact scenario.

All four managed assemblies compile through ignored Unity 6.6 reference remapping.
Editor generate/check/build and 52 unit tests pass. Actual LuaLS verifies source
completion across all eight callbacks and the complete shipped example; actual
VS Code passes 16 checks including inferred attack fields. Wiki build/types/search and 6418 local links/assets across 61 pages pass.
The Underworld runtime has 1282 passing assertions with matching Unity managed
references preloaded; its asset audit retains the previously documented missing
fungus_raid /layer_0_2 resource. No location assets changed. Existing standalone MoonSharp
reflection loader warnings, unrelated Unity Search/preview warnings and the wiki
404 warning remain; logs are not claimed clean. General actors, arbitrary world
contacts, expiry/miss notifications, swept collision, all arenas/forms, physical
input/export and independent creator acceptance remain open. G01-G14/E1-E8 and
the broad Minecraft-style objective remain active.


## 2026-10-04 — reusable projectiles and direct procedural spawning

This turn adds a complete direct-spawn slice rather than treating native weapon
children as general fighters. `sf2.projectiles.register` supplies an owned,
transactional definition (rig, equipment, explicit start move, bounded lifetime).
`fighter:spawn_projectile(definition, x, y, z?)` queues a callback-scoped command
and supplies a retainable queued/applied/failed receipt. Applied receipts carry a
copied ID matching live queries/contact provenance. Ordinary Lua creates patterns;
no arithmetic/operation DSL, raw-model access or downstream DE policy is added.

Source: ModProjectiles adds the shared immutable definition, catalog/registration
and pure spawning interface. ModContentP1D reuses projectile reference validation,
counts definitions in the transaction limit and commits/clears them atomically.
ModSaveData adds a sorted definition block only when nonempty, preserving older
content fingerprints for catalogs without this feature. Live children/receipts
are ephemeral. ModScripting and Fight's operation wrapper forward the command;
MoonSharpScriptRuntimeProjectiles owns genuine definition handles, registration,
argument/capability/timing checks and receipt updates. There is no opponent method
or lookup exposing another mod's definitions.

FightProjectiles queues with round/session/owner/root guards and shared 16/mod,
64/fight reservations, including both root fighters and timeline spawns.
Fight materializes after collisions and before native animation selection, then
initializes after native birth. LegacyContentAdapterP1D shares the existing item,
rig, owner/lifetime/start-move projection between timeline and direct creation.
ModRuntime forwards to the installed adapter. Model exposes whether explicit
birth playback actually started; legacy fallback remains available but does not
produce a falsely applied direct receipt. Creation-listener exceptions after
registration are retired even when the factory never returns a model.

Native acceptance found two important placement facts. WeaponModel first-keyframe
render precedes cached-center refresh; a zero native translation refreshes it.
The native constraint solver can change that center slightly after translation.
Birth corrects residual translation in at most four passes and accepts only
within .05 native units per axis; impossible/nonfinite placement fails and retires
the child. Coordinates mean weighted center of mass at application, not a floor,
pivot, hand or request-time point. Subsequent animation can move this observation.
Birth first-frame actions occur before final offset placement; this limitation is
publicly documented. Ownership attaches before create listeners, and initializing
children do not appear in Lua queries. TTL begins at successful initialization.

Scripted Burst is a complete manifest/flight binary/Lua/HUD example and mirrored
editor starter. Lua computes three offsets, correlates receipt IDs with reacquired
references, guides native contacts, counts only owned hits and accepts partial
bursts with an explicit cooldown. It does not request main-fighter playback.
Public projectile/ability/editor/example docs, editor schema/generated contracts
and relevant starter/diagnostic/completion tests update in this change. Coverage
is 234 public bindings, 74 constants and 287 typed structures.

Verification:

- 557 production MoonSharp/full queue/tracker checks with controlled native
  models pass: genuine handles, arguments, capabilities, callback expiry/timing,
  deferred creation, initialization visibility, receipts, bounded constraint
  correction/failure, shared reservations across roots and global slots,
  creation-listener exceptions, TTL/pause, cancellation, content fingerprint and
  transactional reference failure. The actual Scripted Burst source registers
  against imported canonical items/stages and runs its HUD, three-child receipts,
  Lua motion, source counter, cooldown, partial burst and round/fight reset.
  This harness controls native geometry, clock and contact events.
- Unity 6.6 Campaign/core Tournament 3 passes 45 native checks. The fixture adds
  a private direct three-child probe after two shipped Return Dart casts. It
  observes three applied IDs, real rig/equipment/rendering, 60-unit spacing,
  equal vertical placement, exactly two caster starts, native contact health
  .9836771 -> .9673543 and live-child surrender retirement. Earlier cast contact,
  eight copied-source notifications, pause, cooldown and out/return remain passing.
  Placement postconditions run at the actual birth boundary. Final log:
  Temp/FighterPlaybackUnity-d997a2c104cd4564b9767ae6eb4b68d8/validation-e1f29793eed9447e97d646db4065f949.log.
  Inputs/AI/spacing and fresh post-tutorial save identity are controlled. The
  complete Scripted Burst HUD was exercised in the managed harness, separately
  from this native probe; no arbitrary loadout/mode acceptance is implied.
- All four managed assemblies compile using ignored matching Unity 6.6 Windows
  reference remapping. Fighter playback regression passes 492 checks. Native XML/
  projectile-parser regression passes 211 assertions across 136 attack intervals;
  scheduled-event batching passes five checks. The optional combined
  TestMoveProjectiles chain could not run its automatic root-project rebuild:
  exported csprojs still reference unavailable Linux analyzer paths. This is not
  presented as a passing combined suite; remapped compilation and the above
  parser/native scenarios have separate evidence.
- Editor generate/check/build and 53 unit tests pass. Actual LuaLS completes the
  registration and receipt and checks the entire Scripted Burst source with no
  diagnostics; all previous integration checks pass. Real VS Code passes 17
  checks including direct receipt completion. Wiki types/build/search and 6435
  links/assets across 61 pages pass; the existing duplicate-404 warning remains.

Initial native probes exposed asynchronous all-side fixture assertions, retained
ended-animation names, cached birth centers and constraint residuals; checks and
production positioning were corrected. Interrupted isolated scene recovery was
preserved under ignored fixture directories. No root scene/save, serialized
assets, metas or confirmed deobfuscation maps were changed. Existing standalone
MoonSharp reflection-loader and unrelated Unity preview/Search warnings remain.

This advances programmable child spawning and E2/E8 acceptance. Independent
health/AI/team actors, original custom rigs, miss/expiry notifications, solid/swept
world physics, broader arena/form/loadout, physical input/export and independent
creator acceptance remain open. G01-G14/E1-E8 and the broad objective stay active.


## 2026-10-04 — independent native actor feasibility

This turn adds engineering evidence for general actors without presenting native
weapon children as independent fighters or inventing a public actor contract.
Tools/Tests/Combat/ExtraFighterUnity.cs and TestExtraFighterUnity.ps1 construct a
third real root through recovered native construction/entry seams in an isolated
full-game fixture. The probe clones the core opponent parameters, rig, equipment
and tactic, gives it independent health and active native stage/action state,
and observes actual rendering, targeting, contact and removal. Runtime source,
serialized assets, metas and public Lua bindings are unchanged.

Unity 6.6 passes 27 checks. The bounded 120-frame AI observation stayed in
StanceIdle and made no contact. The fixture then explicitly starts KnivesSlash;
player health changes .9220222 -> .8700371 and the native StrikeResult names the
extra root as attacker. Original opponent health is unchanged. The extra root's
zero health does not end the main duel or retire it automatically; requested
removal clears rendering and both original enemy lists, restores their mutual
targets and permits continued native animation. This proves a controlled attack
and life/removal seam, not reliable autonomous attack selection or full animation
binding restoration. Final successful log:
Temp/FighterPlaybackUnity-d997a2c104cd4564b9767ae6eb4b68d8/validation-25386ebda3bd4e948921160be9a3ed83.log.
A fresh result timestamp and successful Unity process exit were verified.

The fixture controls original input/AI and spacing, uses an empty user-mod root,
a unique fresh post-tutorial acceptance profile and shared immutable TAR cache.
Earlier probes exposed an overstrict animation-diversity assumption, a nested
fixture event-type compilation mistake and overlapping isolated editors. Those
runs are not accepted. Only known owned processes were stopped; interrupted
isolated scene backups were preserved. The authored runner now rejects an open
fixture and fails early on compiler errors. Root Unity, scenes and user saves
were untouched. Existing unrelated Unity Search/title-preview warnings remain.

ACTOR_FEASIBILITY.md records concrete integration gaps: AddModel makes all roots
mutual enemies; SetNearestEnemy chooses the first registration; the unused
FindNearestEnemy helper uses signed distance; DistancePoint caches root nodes in
two side slots; Lua fighter/damage routing and round settlement still identify
only the main pair; a native parent assumes WeaponModel. Side caching is a source
risk, not an experimentally isolated explanation of the idle observation. The
public projectile guide now explicitly distinguishes children from unavailable
independent actors. Test index, extensibility and roadmap status update together.

Wiki types/build/search and 6435 local links/assets across 61 pages pass;
reference coverage remains 234 public bindings. Existing duplicate-404 warning
remains. Matching Unity imported/compiled the full native probe and game; no new
runtime API/editor schema requires regeneration and no separate managed-only
compile or Lua suite is credited for this test/documentation change.

This is concrete feasibility progress for E1/E2/E5. Typed actor definitions,
round-owned instances/receipts, behavior hosts, team/hostility targeting, safe
per-instance bindings, autonomous attacks, incoming-hit identity, owned children,
forms, death/reset/disable/teardown, alternate victory policies, original rigs,
all equipment/modes, physical input, exports and rollback acceptance remain open.
G01-G14/E1-E8 and the broad Minecraft-style objective remain active.


## 2026-10-04 — model-identity animation point bindings

This turn fixes a demonstrated native prerequisite for independent fighters:
shared DistancePoint definitions had one root node/pivot slot per side. A third
same-side root overwrote the existing root, and opposite-side lookup could not
represent a same-side target. The extended extracted production regression first
failed on the old source's same-side node assertion, then passed after the repair.

DistancePoint.cs adds a ConditionalWeakTable<ModelObject, PointNode> for roots.
UpdateNode preserves each root's own resolved optional node and pivot; lookup uses
the live self/target ModelObject from ModelConditions rather than IsPlayer alone.
A live unbound root receives an empty point instead of another body's cached node.
Archival context-free callers preserve the original two-side fallback. Existing
helper identity lists, parent paths and child-reset semantics are unchanged. Weak
keys preserve form rollback bindings while allowing retired node-owner cycles to
collect. No assets, GUIDs, serialized formats, public API/schema or confirmed name
mappings change.

TestAnimationNodeRebind.ps1 passes 23 production-field/method checks with controlled
native geometry: legacy fallback, helpers, optional ranged points, multiple roots
on one side, distinct pivots, same-side/cross-side target identity, rebind/form
rollback, unknown-root isolation and GC collection of a retired root whose node
references its owner. This is real production extraction, not a copied algorithm.
Form animation entry/readiness passes 78 checks; the existing form commit and
render-registration orchestration suites also pass with their controlled services.
All four managed assemblies compile through matching Unity 6.6 Windows reference
remapping in ignored Temp projects.

The full-game ExtraFighterUnity probe now verifies actual shared native move-point
bindings for each live model, then rechecks after third-root retirement. Unity 6.6
passes 174 checks, including 84 real node observations across three roots and 56
across the remaining pair. Its core tactic is tabular; the bounded AI observation
still remains in StanceIdle, with native decision-delay field zero. Explicit
KnivesSlash contact changes player health .9604013 -> .9084162 with the third root
as native attacker. This proves the binding repair and confirms autonomous attack
readiness remains a separate gap. Final accepted log:
Temp/FighterPlaybackUnity-d997a2c104cd4564b9767ae6eb4b68d8/validation-30aad9efc187447c90e210af7c6a6693.log.

Return Dart's complete native regression still passes 45 checks: actual child
flight/contact/source IDs, pause, out-and-return, queued direct three-child burst,
spacing, no extra caster animation, contact health and live-child teardown. Log:
Temp/FighterPlaybackUnity-d997a2c104cd4564b9767ae6eb4b68d8/validation-8a420b19628f4535b6154707cea5d401.log.
Both runners have fresh result timestamps and successful Unity exits. Profiles,
original input/AI and spacing are controlled, and root scenes/saves are untouched.
Existing unrelated Unity startup/title warnings remain.

Actor feasibility/roadmap/test documentation and the public form-change guide
update with this contract. Wiki types/build/search and 6435 links/assets across
61 pages pass; coverage remains 234 public bindings and the duplicate-404 warning
remains. This is E1/E2/E5 groundwork, not closure of general actor support. Typed
actor definitions/receipts, behavior/lifetime, team targeting, autonomous attacks,
broader rig/equipment/paired/contact/form/teardown, physical input/export and
rollback acceptance remain open. G01-G14/E1-E8 and the broad objective stay active.


## 2026-10-04 — independent native AI readiness and animation observers

The binding repair was committed/pushed separately as e13e297f before this
feature. This follow-up makes the third root act autonomously in a controlled
native fight, rather than describing an explicitly requested attack as AI proof.

ModelAi.Render's missed-own-move initialization previously applied only to
versus/title fights. An extra root joining Campaign could have a current idle
animation but no own-move observation, causing native decision eligibility to
reject it. The same initialization now applies in any active fight; existing
observations, nonplaying moves, missing fights and global/per-model AI guards
remain effective. No move is forced by this readiness hook.

Model.PAMICDLAMHC previously inferred the starting controller through its selected
enemy's target. That round trip works for a mutual duel, but an additional root
can target the player while the player still targets the original opponent. The
starter now observes its actual own animation once. Distinct non-weapon roots in
its native enemy registry whose cached target is that starter observe the actual
source and wake native decision delay. Null/self/duplicate/weapon entries and
roots watching other sources are excluded. ObserveEnemyAnimationStarted replaces
the narrowly used obfuscated helper; its inferred declaration has the required
best-guess comment. All source/tool/docs references were searched; no confirmed
recovery mapping, serialized asset or meta is changed. Native enemy registration
remains mutual; asymmetric team targeting/ownership is still future API work.

Verification:

- The production form-entry/readiness fixture passes 80 checks, including Campaign
  own-move initialization, retained observations, nonplaying state, no fight and
  native eligibility. New TestAnimationObserverRouting.ps1 extracts both production
  methods and passes 15 source/observer/own-identity, pair/third-root, retarget,
  duplicate/null/self/weapon and native AI-flag checks with controlled services.
  Existing AI eligibility passes 14 and snapshot adaptation 56 controlled checks.
- All four matching Unity 6.6 Windows-reference managed builds pass after the final
  runtime changes. No separate passing combined Lua/native-parser suite is claimed.
- ExtraFighterUnity -RequireAutonomous forbids fixture-requested attack playback.
  It allows at most 600 simulation frames for first native contact (the default
  research fallback remains 120), then records health at a second distinct attack
  start and requires later native contact. It checks the controller's own and
  actual target observations, source attribution, separate health, 84/56 real
  shared node bindings, third-root knockout without ending the original duel,
  explicit removal and original-pair continuation. The accepted full-game run
  passes 180 checks: KnivesLowSlash changes player life 1 -> .9220222; later contact
  changes it to .4270475. StepForward and ThrowForward are also observed. Explicit
  attack requested=False, sustained autonomous acceptance required=True. Final log:
  Temp/FighterPlaybackUnity-d997a2c104cd4564b9767ae6eb4b68d8/validation-044991f0b10346999ede9a553c06295c.log.
- The intermediate widened-readiness run passed 172 checks with native
  KnivesHeavySlash contact and no explicit request. The later routing run chose
  evade/advance moves and failed the original 120-frame contact window; it is not
  accepted. Increasing the strict bounded observation window accommodates ordinary
  native tactical timing rather than forcing an attack. Two separate attack starts
  and the health sampled at the second start prevent counting a multi-hit move as
  two autonomous decisions. This is one controlled core tactic/rig scenario, not
  deterministic universal attack timing or general AI/paired-throw acceptance.
- Return Dart's 45-check full-game regression passes after the final AI/routing
  source: flight/source IDs, pause/return, direct three-child queued burst, spacing,
  unchanged caster-start count, contact health and live-child teardown. Log:
  Temp/FighterPlaybackUnity-d997a2c104cd4564b9767ae6eb4b68d8/validation-feef2eb714934bc1b199acc38f3fae68.log.
  Both final native runs have fresh result timestamps and successful Unity exits.
  Fresh post-tutorial profiles, empty/owned mod roots, original controls/AI and
  spacing are controlled. Root scenes/saves remain untouched. Existing unrelated
  Unity startup/title warnings remain.
- Public AI guidance, actor feasibility/extensibility/roadmap records and test
  index update together. Wiki types/build/search and 6435 links/assets across
  61 pages pass; coverage remains 234 public bindings and the duplicate-404 warning
  remains. No public member/schema change requires editor contract regeneration.

This establishes native actor AI readiness/observation prerequisites for E1/E2/E5.
General actors still need typed definitions and owned instances, deferred receipts,
Lua behavior hosts, teams/hostility and atomic targeting, child ownership, death/
round/form/disable/teardown and alternate victory policies. Arbitrary rigs/outfits,
all tactics/modes/throws, physical input, exports and rollback remain unaccepted.
G01-G14/E1-E8 and the full Minecraft-style objective stay active.

## 2026-10-04: transactional native actor target integration

Previous turn classification: progress. The binding and AI prerequisites were
committed/pushed. This follow-up advances a reusable native actor integration
boundary; it does not expose unfinished ownership/team/behavior contracts to Lua.

Model.ReplaceCombatEnemies accepts distinct live hostile roots and an explicit
selected root, or an empty list/null target. It expands their weapon children,
places the selected root first for the legacy insertion-order fallback and updates
the root plus existing owned weapon children transactionally. Cached, animation
and event targets change together. ModelAi.ResetCombatTarget computes the new
weapon equivalent before mutation, invalidates old-target move observation,
response/frame/wait state and mod-handler decision throttle, while retaining the
own-move observation and tactic. Ordinary AI rendering initializes the new target
observation; the transaction invokes no tactic or random draw. Native decision
delay wakes. Item-mapping or child binding failures restore touched registries,
targets and controller state. The returned rollback is strictly synchronous,
before subsequent simulation/callback/target changes, not network rollback.
CheckCollision safely returns false when no target exists. A target change does
not restart a move or reset its already-used collision interval.

The new internal seam does not alter legacy AddModel's mutual-enemy construction,
automatically enforce teams, choose the nearest enemy or change duel settlement.
Round/session/mod ownership and transactional native creation still need separate
integration. Weapon propagation is production-source tested with controlled
children; full-game child retarget/form/disable/teardown acceptance remains open.
No new Lua binding/schema, asset format, save field or Unity GUID changes.

Verification:

- TestCombatTargetBindings.ps1 extracts all three production model methods and
  the AI invalidation method; 43 checks pass with controlled animation/AI services.
  These cover exact rollback, root/source/target validation, selected-first order,
  child expansion/propagation, stale observation/wait/throttle invalidation,
  unarmed/empty targets, mapping failure before mutation, child binding failure
  rollback and missing event context. It is not a Unity playtest.
- Existing controlled regressions pass: 15 animation observer routing, 23 weak
  model-identity point bindings, 80 form animation entry/readiness, 14 native AI
  eligibility and 56 AI snapshots. All four matching Unity 6.6 Windows-reference
  managed builds pass through the ignored portable compile projects.
- ExtraFighterUnity -RequireTargeting includes autonomous outgoing contact, an
  immediate target/animation/observed-opponent rollback, actual hostile-root
  changes and incoming native AI contact. It checks all actual target bindings,
  new-target point observations, separate actor health, unchanged excluded player
  health, source identity, restored main targets, knockout policy, removal and
  continued original duel. The accepted corrected run passes 303 checks. Native
  extra-root contacts change player life .9220222 -> .4270475, then .4270475 ->
  .3931057 after a subsequent attack start; ThrowThroughTheBack and AxeKick are
  observed. Retargeted original opponent contact changes actor life 1 -> .8020738
  while excluded player life remains unchanged. Explicit attack requested=False.
  Fresh result timestamp and successful Unity exit verified. Final log:
  Temp/FighterPlaybackUnity-d997a2c104cd4564b9767ae6eb4b68d8/validation-810bdabfa927423bb880431fad662aea.log.
- The earlier 180-check repeat assertion recorded a second start but did not
  require that start to follow the first observed contact. The new fixture records
  the start count at first contact, requires a later start and samples life at
  that start before requiring further damage. This closes an acceptance gap;
  historical logs do not establish this stronger ordering. Poll iterations do
  not inflate the new check count. The first targeting run failed because the
  fixture kept disabling original opponent AI each update; that fixture guard
  is corrected, and the failed run is not counted as acceptance.
- Return Dart's full-game native regression passes all 45 checks after the target
  changes: flight/source identity, pause/return, direct three-child receipts and
  placement, unchanged caster-start count, contact health and teardown. Fresh
  result timestamp and successful Unity exit verified. Final log:
  Temp/FighterPlaybackUnity-d997a2c104cd4564b9767ae6eb4b68d8/validation-91657ee405f6478da742493ad81e7165.log.
- Public projectile limits, actor feasibility/extensibility/roadmap records and
  test index update together. Wiki reference coverage remains 234 public bindings;
  types/build/search pass with 0 type errors/warnings/hints and 6435 links/assets
  across 61 pages. The existing duplicate-404 warning remains. No public member
  change requires editor contract regeneration. Existing unrelated native Unity
  startup/title warnings remain; no claim of error-free logs or exported builds.

Core cloned rigs/tactics, controlled spacing/input and a fresh isolated profile
bound this native evidence. Empty hostility has controlled coverage only. General
teams, asymmetric animation observer registries, arbitrary rigs/loadouts/throws,
mod-owned actor handles/behavior, death/round/form/disable/teardown, encounter
victory policies, physical input, exported players, multiplayer/rollback and
performance still need work. G01-G14/E1-E8 and the full objective remain active.


## 2026-10-04: editable owned mod UI

Added text_input nodes (placeholder, 1..8192 UTF-16 max_chars, multiline),
string on_change notifications and owned sf2.ui.get_text. Native themed fields
preserve focus across model updates; HUD editing captures fighter controls,
Tab traverses controls and Back leaves editing before view dismissal. Field
ownership, ancestors, foreground/native blocking, errors and context disposal
reuse existing UI lifetime gates. Installed Text Input Lab and its mirrored
manual starter show public Lua HUD and multiline form workflows without changing
native fighter names or saves. Public wiki/schema/generated/editor guides update
with source; no recovered identity or renderer experiment changes.

Verification: 203 managed UI checks/source guards; 996 actual Lua UI checks;
four matching Windows-reference assemblies; 61 editor project tests; actual LuaLS
completion/clean sample diagnostics and 26 isolated VS Code integration checks;
wiki 255 binding coverage, types/build/search, 7036 local links across 64 pages,
64 tracked current-branch source links and three link tests. Final isolated Unity
installed-mod run passes 35 checks and exits successfully with a fresh result.
Synthetic native keyboard Events prove component-to-Lua/editing/capture behavior;
physical keyboards, IME/software keyboard/glyph coverage and exported platform
acceptance remain open. Screenshot waits for the modal entrance fade to finish
before capture. See UI_TEXT_INPUT_ACCEPTANCE.md for exact source and evidence.
G01-G14/E1-E8 remain open. The human has deferred experimental procedural 3D.
