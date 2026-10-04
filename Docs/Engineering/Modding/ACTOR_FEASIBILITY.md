# Independent combat actors: native feasibility

This records the recovered engine boundaries for a future typed actor API.
Projectiles are `WeaponModel` children; they are not substitutes for independently
living fighters. This investigation adds a repeatable isolated native probe,
without shipping a Lua actor namespace or changing recovered fight behavior.

## Existing native seams

| System | Current source behavior | Consequence for an actor API |
| --- | --- | --- |
| Construction | `Fight.AddModel(ModelParameters)` constructs a root `Model`, initializes it and registers camera, events, perks and animation selection. | A third root has a plausible native construction seam. Creation must be deferred outside model iteration and made transactional. |
| Health | A copied `ModelParameters` owns its own current/max life. `Fight.UpdateLife(Model, amount)` acts on that model. | Independent health does not require pretending to be a child weapon. Definition/instance identity, bounded values and death lifetime still need a contract. |
| Enemy registration | `AddModel` adds every existing model as a mutual enemy through `Model.CJNGMIMHFCC`. | There are no teams or explicit hostility rules at this seam. Friendly summons cannot use this unchanged. |
| Target selection | `SetNearestEnemy` takes `_Enemies[0]`; collision, facing and AI use the cached `EGGEACCDAEK()` target. | Registration order wins over distance. Retargeting must update collision, animation, AI and event state together. Merely editing a Lua target ID is insufficient. |
| Distance helper | `FindNearestEnemy` uses signed horizontal distance, includes every enemy entry and has no C# callers in the tracked source. | It is not an existing working nearest-target implementation. Do not expose or activate it as one without new tests. |
| Simulation | `RenderFight` iterates live model lists for rendering, collision and AI. New roots still require active stage/action state and an eligible entry animation. | List support is useful, but successful insertion alone does not prove a functioning fighter. |
| Round results | `RenderRound` checks the main player/opponent parameters; `GetWinner`, control and campaign settlement retain duel semantics. | Actor knockout must have a separate lifecycle. Teams, alternate victory policies and encounter completion need explicit integration. |
| Lua routing | `EclipseFighterOperations` and damage/lifecycle dispatch distinguish main player/opponent identities. | Existing fighter callbacks do not automatically become actor behavior hosts. Actor events need their own instance identity and safe scoped handles. |
| Removal | `RemoveModel` removes camera, enemy references, perks, animation selection and native resources. A non-null parent assumes a `WeaponModel`. | Independent actors must remain roots; ownership cannot be implemented with the native weapon-parent pointer. Owned children and pending commands must be retired too. |
| Animation bindings | Form replacement already documents native definitions caching node bindings by side. | A same-side clone is a narrow feasibility scenario, not proof of arbitrary same-side rigs/loadouts or paired attacks. |

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
Reliable autonomous attack selection remains open. `DistancePoint` stores root
node bindings in two side slots; this is a concrete per-instance binding concern,
but its role in the observed idle behavior has not been isolated experimentally.
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
