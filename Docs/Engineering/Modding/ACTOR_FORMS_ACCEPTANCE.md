# Independent actor character forms

2026-10-04. This extends live character composition toward the general creator
platform; arbitrary rigs and complete G01–G14/E1–E8 acceptance remain open.

## Source and public contract

`ModActors.cs` adds optional `IModActorForms`, preserving existing actor backends.
`MoonSharpScriptRuntimeActors.cs` exposes `actor:change_form(character)` with
both `combat.actors` and `combat.transform`, exact argument validation, this
context's registered warrior handle and callback lifetime. Results reuse the
queued/applied/failed form receipt. An unavailable optional backend produces a
failed receipt. Registration, main fight/round lifecycle and actor end callbacks
cannot grant a live form capability; escaped references expire. Existing `fighter:change_form` works on
an actor's own attached behavior using the same native path and its existing
`combat.transform` capability.

`FightActors.cs` prepares a hidden destination using the actor's absolute team,
AI enabled flag and definition max health, preserving its definition name. Form
characters must be registered by the owning mod. Initialization, removal,
expired owner/round/session, pause and duplicate pending forms reject the request.
`Fight.cs` accepts initialized live actors at its existing deferred form boundary,
and rechecks participant validity before applying. Current health is captured
at that boundary and copied as a percentage into the unchanged actor max pool.

The actor path skips the main-fighter rules inspector/participant/HUD slots while
sharing reversible camera/selector, enemy/weapon target, perk/action/effect,
combat state and event listener handover. A dedicated participant transaction
rekeys the same actor record and native body-list slot, preserving ID, private
behavior XML, team, owner, original birth frame and definition. Shield, status
icon and model-keyed behavior state follow the new body. The existing actor
reference transaction also moves explicit targets. A late rejection restores
all references/registrations without creating or ending the instance.

After commit, owned queued/live projectiles from the retired actor caster are
canceled, then its old body and children retire. Post-commit cleanup errors are
logged without rejecting the committed replacement or destroying it through
preparation disposal. The destination starts an eligible native entry animation;
old attacks and native AI controller memory do not resume. Private attached Lua
behavior state remains. No asset identity, save schema or definition fingerprint
format changes are introduced.

## Complete example and verification

Authored Fighter Lab 1.5 and its mirrored editor starter add **Left form: core /
authored**. The main rule requests the core body through `actor:change_form`;
the actor's own tick requests the authored return through `fighter:change_form`.
Repetition stops before either request. The same actor then resumes authored
native playback after spacing is restored.

- `CharacterForms/TestActorFormBindings.ps1`: **42 production-method checks**
  cover both teams, identity/private state/lifetime and shield/status/behavior
  keys, role/collision guards, own character, duplicate preparation and failure
  disposal. Native liveness, initialization, body creation and queue services
  are controlled.
- Existing production coordinator, presentation, boundary, commit and prepared
  request fixtures pass, including actor late rollback, unchanged main HUDs,
  removal before application and cancellation of the retired projectile caster
  only after commit. The commit test also throws from cleanup to verify that
  the applied form cannot become a failed receipt. **258** initialization checks
  retain main-side regression coverage.
- `Combat/TestActorDefinitions.ps1`: **114 actual Lua checks** include typed form
  arguments, both independent capability gates, immediate/deferred receipts,
  unsupported backend, expired reference and terminal callback rejection.
  `Combat/TestProjectiles.ps1`: **585** existing production Lua/projectile checks
  pass with controlled geometry/services, including actor caster retirement.
  Its existing UnityScriptLoader reflection warning under standalone .NET is
  not native game acceptance.
- `CharacterForms/TestAuthoredFighterUnity.ps1 -PlayerEntry` passes **125 native
  checks**, exit 0, in the marked isolated Unity 6000.6.0f1 project. Final log:
  `validation-2d8f7580c6bb491d86f7fd487e8b7d0c.log`. The installed mod tests public
  actor-reference and own-behavior form requests, core/authored character/skin
  replacement, exact actor record/ID/private XML/birth retention, team/control/
  max-health invariants, no healing or actor end, explicit/native target rebinding,
  removal of old root registrations and unchanged main parameters/health/HUD.
  After restoration it requires new starts and source-attributed native damage
  in both directions before dismissal. Existing player form/input, pause,
  prepared selection and surrender cleanup checks remain in the run. The final
  `-ComparisonEntry` run also passes **125 checks**, exit 0, in
  `validation-70ff7c5f3afd409daa000a30b2ddbff5.log`, using the same final sample
  and both public form routes. Its expanded HUD is visually reviewed; the native
  owner/pair capture is saved in ignored `Temp/ActorFormsReview-20261004/`.
- All four managed assemblies compile through ignored Windows-remapped projects.
  The default Visual Studio SDK resolver remains unavailable. Public reference,
  authoring guide, sample and editor contracts update together; generation/check,
  **59** project tests, actual LuaLS and all **25** VS Code integration checks pass.
  Actor/self-reference completion includes `change_form`; static diagnostics
  require both capabilities without duplicating `combat.actors` diagnostics.
  The wiki builds **63 pages**, covers **250 public functions/aliases/callbacks**,
  and validates **6,853 local links/assets**, **63 current Git source links** and
  three source-link regression cases. The existing duplicate-404 route warning
  remains; search generation and types pass.

Native evidence uses the example's standard compatible skeleton, manual Lua
playback, a player-team actor and core equipment. The opponent-team binding and
rollback use controlled managed services. Actor caster projectile retirement
at a form commit is production-source tested with controlled services, not a
new native ranged-form scene. Native AI memory transfer, arbitrary rigs/outfits,
all arenas, physical-device input, platform exports and aggregate performance
remain open. The main-fighter form contract keeps its existing limits.
