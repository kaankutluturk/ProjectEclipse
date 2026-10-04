# Independent combat actors: native feasibility

This records the recovered engine boundaries for a future typed actor API.
Projectiles are `WeaponModel` children; they are not substitutes for independently
living fighters. This investigation supplies a repeatable isolated native probe,
model-identity binding repair and native AI readiness/notification repairs,
without shipping a Lua actor namespace.

## Existing native seams

| System | Current source behavior | Consequence for an actor API |
| --- | --- | --- |
| Construction | `Fight.AddModel(ModelParameters)` constructs a root `Model`, initializes it and registers camera, events, perks and animation selection. | A third root has a plausible native construction seam. Creation must be deferred outside model iteration and made transactional. |
| Health | A copied `ModelParameters` owns its own current/max life. `Fight.UpdateLife(Model, amount)` acts on that model. | Independent health does not require pretending to be a child weapon. Definition/instance identity, bounded values and death lifetime still need a contract. |
| Enemy registration | `AddModel` adds every existing model as a mutual enemy through `Model.CJNGMIMHFCC`. | There are no teams or explicit hostility rules at this seam. Friendly summons cannot use this unchanged. |
| Target selection | `SetNearestEnemy` takes `_Enemies[0]`; collision, facing and AI use the cached `EGGEACCDAEK()` target. | Registration order wins over distance. Retargeting must update collision, animation, AI and event state together. Merely editing a Lua target ID is insufficient. |
| Distance helper | `FindNearestEnemy` uses signed horizontal distance, includes every enemy entry and has no C# callers in the tracked source. | It is not an existing working nearest-target implementation. Do not expose or activate it as one without new tests. |
| Simulation | `RenderFight` iterates live model lists for rendering, collision and AI. Native controllers now seed an already-running own move in any fight mode. Root animation starts update the starter's controller and registered root enemies that currently target it. | Active stage/action state and an eligible entry animation still need explicit initialization. Core third-root autonomous contacts now have native acceptance; general actor ownership/behavior remains unfinished. |
| Round results | `RenderRound` checks the main player/opponent parameters; `GetWinner`, control and campaign settlement retain duel semantics. | Actor knockout must have a separate lifecycle. Teams, alternate victory policies and encounter completion need explicit integration. |
| Lua routing | `EclipseFighterOperations` and damage/lifecycle dispatch distinguish main player/opponent identities. | Existing fighter callbacks do not automatically become actor behavior hosts. Actor events need their own instance identity and safe scoped handles. |
| Removal | `RemoveModel` removes camera, enemy references, perks, animation selection and native resources. A non-null parent assumes a `WeaponModel`. | Independent actors must remain roots; ownership cannot be implemented with the native weapon-parent pointer. Owned children and pending commands must be retired too. |
| Animation bindings | `DistancePoint` now keeps weak root-identity bindings alongside legacy side-only fallback. Native helper bindings retain their existing identity lists. | Same-side node/pivot overwrite is repaired. Differently equipped/rigged actors, paired attacks and complete animation-state independence still need acceptance. |

## Acceptance boundary

`Tools/Tests/Combat/TestExtraFighterUnity.ps1` runs the full game through matching
Unity 6.6 in a marked isolated project with a unique fresh post-tutorial profile,
an empty user-mod root and immutable TAR-cache sharing. It uses reflection to
enter native construction/entry/removal seams; the test is not a supported mod
creation path. Original player input and original opponent AI are disabled;
spacing is controlled. The extra root clones the native opponent rig/loadout and
tactic, with its own parameter object and active native AI.

The probe checks root identity, independent life, rendering, bounded native AI
observation, player contact attribution, insertion-order targeting, third-root
knockout without ending the main duel, explicit retirement and continued original
targets. If AI produces no contact in 120 simulation frames, it separately starts
a native knife attack; this fallback is recorded in the result, not credited as
autonomous AI acceptance.
The runner rejects an already-open fixture and fails early on compilation errors.
Fresh result timestamps and a successful Unity process exit are required.

The accepted run passes 27 checks: AI remained in `StanceIdle` during the bounded
observation. An explicitly requested `KnivesSlash` reduced player life from
.9220222 to .8700371, with the native strike naming the extra root as its attacker.
That was the initial feasibility run. The binding follow-up passes 174 native
checks, including 84 actual shared move-point observations across three roots and
56 across the remaining pair after removal. `DistancePoint` now selects nodes and
pivots by the live root/target's ModelObject identity; a weak table avoids retaining
retired bodies through node-owner cycles. Unbound live roots do not borrow another
body's side slot. Context-free archival callers retain the legacy side fallback;
existing helper lists and child-reset semantics are preserved.

The extracted regression first failed on the original same-side overwrite, then
passed 23 checks covering root/target/pivot identity, optional node absence,
form rollback, legacy side fallback, helper isolation and retired-root collection.
Return Dart's 45-check native regression still passes after this repair. The
follow-up AI observation still stayed in idle under the core tabular tactic;
the binding defect is proven fixed, but it did not establish autonomous attacks.
At that binding-only stage, autonomous selection remained unverified.

The AI follow-up fixes two additional native issues. `ModelAi.Render` previously
initialized a missed own-move observation only for versus/title fights, leaving
the Campaign extra root's controller without a ready own move. That readiness
now applies to every active fight. `Model.PAMICDLAMHC` now supplies its actual move
to its own controller exactly once and notifies distinct non-weapon registered
enemies whose cached target is this source. The renamed inferred helper
`ObserveEnemyAnimationStarted` observes the actual source and wakes its native
decision delay. It no longer infers a source controller from an observer's target.
The native root enemy registry is still mutual; asymmetric team hostility is not
implemented by this change.

`-RequireAutonomous` forbids fixture-requested attack playback, allows at most
600 simulation frames for the first contact, then requires a second distinct
native attack start and subsequent contact. The accepted run passes 180 checks:
`KnivesLowSlash` changes player life 1 -> .9220222, then a later attack changes it
to .4270475. `StepForward` and `ThrowForward` are observed. The fixture checks own
and target animation observations, 84/56 shared node bindings, separate health,
main-duel result policy and retirement. This accepts autonomous attacks in that
controlled core scenario, not arbitrary AI/rigs, universal throws or an actor API.
The earlier short idle observations are historical evidence, not current behavior.
The final log is recorded in the creator platform work log. Static observations
above are distinct from native acceptance.
No teams, native incoming-strike proof on the extra root, original custom rig,
paired throw, form exchange, all equipment/modes, physical input, exported build,
rollback or long-run performance acceptance is implied.

## Next implementation boundary

Start with typed character references plus explicit team/hostility data, bounded
round-owned instances and deferred creation receipts. Keep Lua behavior ordinary
procedural code. A genuine actor instance needs copied observations, scoped
motion/playback/health commands, an owned behavior host and death/removal events.
Use a separate ownership record from the native parent link. Exclude competitive
and rollback modes until their feasibility is demonstrated.

Before making that public, prove atomic target changes and same-side animation
bindings with two differently equipped/rigged actors; verify incoming/outgoing
damage identity, projectile children, death, round reset, form replacement,
owner disable and fight teardown. Preserve ordinary duel behavior. Explicitly
separate summons that die without ending a duel from encounters whose teams
determine the winner. None of these contracts is implemented by this probe.
