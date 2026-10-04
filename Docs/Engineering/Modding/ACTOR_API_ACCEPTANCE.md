# Owned independent fighter API

2026-10-04. Public actors now ship; the earlier feasibility record is historical.
This advances the creator platform without closing G01–G14 or E1–E8.

The [reactive actor follow-up](ACTOR_BEHAVIORS_ACCEPTANCE.md) adds direct behavior
attachments and per-instance native callbacks. Earlier host limitations in this
record refer to the initial actor API snapshot.

`ModActors.cs` supplies immutable transactional definitions, copied observations,
events and shared capability interfaces. `FightActors.cs` owns native root
instances by calling mod, spawning main body, round and script session. It uses
prepared construction, deferred entry/placement before collisions, independent
health/tactic/equipment, team enemy-list transactions, attack-safe targets,
pending commands, simulation lifetime and native child/root retirement.
Canonical camera, profile and victory identity remains on the two main fighters.
Actor contacts do not feed their legacy duel hit/strike rules, profile counters
or knockout slow mode. `MoonSharpScriptRuntimeActors.cs` exposes registration,
receipts, scoped actor queries and snapshot/motion/target/health/playback/removal.
Store plain Lua data by actor ID and reacquire references each callback.

The scripted follow-up adds copied actor provenance to fighter/AI snapshots.
The public Scripted Actor Sparring example uses the existing Lua tactic callback
with per-controller memory; actual native acceptance requires Lua-selected
attacks and repeated bidirectional contact without manual playback. See
[SCRIPTED_ACTORS_ACCEPTANCE.md](SCRIPTED_ACTORS_ACCEPTANCE.md) for the separate
30-check run and its limits. This advances programmable actor AI, rather than
claiming a general per-actor combat callback host.

No separate actor Lua behavior host ships yet. Typed projectile commands retain
their main-root guard; actors can use native equipment children. Copied attack
sources identify actor root ID/owner separately from child kind. Definitions
participate in compatibility fingerprints; live actors are not saved.

Three narrow inferred native names are `Model.GetCombatTarget`,
`Model.GetWeaponModels` and `ModelParameters.SetCurrentLife`, each marked with
`// best guess for name`. Owning-type callers and fixtures change with them;
the distinct `Model.GFNCMLFKBGP` wrapper remains. These are not confirmed
deobfuscation mappings. Existing Unity GUIDs/assets remain intact.

Verification:

- Actual Actor Companions mod/Lua/HUD in Unity 6000.6.0f1: **42 checks**, log
  `validation-90868e3abba946c1beda26033cef53e8.log` in the marked isolated
  FighterPlaybackUnity fixture. Registration, independent render/health/AI
  observations/teams, spawn/motion/target/health/owned move playback, death/
  dismissal, copied state, expired/forged/nonfinite/query-limit failures,
  pause/TTL and surrender cleanup pass with a fresh product/profile.
  Expected sandbox faults run in separate callbacks; `pcall` is unavailable.
- **50** production Lua actor definition/default/bound/strict field/handle/
  duplicate/transaction rollback/content fingerprint checks.
- **557** projectile, **43** target binding, **15** animation observer,
  **258** form initialization, prepared form request and showcase Lua regressions.
- Compiled health: **1282** Underworld and **208** PvP assertions. Underworld
  audit retains its existing `fungus_raid: missing /layer_0_2` artwork issue.
- Four assemblies compile using ignored Windows-remapped projects and
  `dotnet msbuild`; default Visual Studio msbuild could not resolve the SDK.
- Wiki builds with dedicated reference coverage for **244** functions/aliases/
  callbacks and checked links. Generated editor contracts, **54** project tests,
  actual LuaLS and VS Code actor completion pass; those are not playtests.

The companion example and mirrored editor starter include summon, movement,
damage, explicit punch, defeat/dismiss and short TTL controls. Public docs state
fields/defaults/limits, scope, receipts and verification limits.

Not accepted here: arbitrary rigs/outfits/tactics, autonomous contact in every
pairing, actor Lua AI/behavior hosts, owner form/disable/session faults and all
rollback permutations in full-game Unity, exports, raids, multiplayer, custom
victory semantics or persistent actors. Earlier cloned-core autonomous contact
probes are separate evidence. No percentage or broad completion claim.
