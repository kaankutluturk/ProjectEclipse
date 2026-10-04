# Projectile abilities on independent fighter roots

2026-10-04. Actor behavior hosts can now use the typed projectile API. This
advances the general creator objective with ranged companions, enemies and
turrets. G01–G14/E1–E8 remain open; arbitrary content/platform acceptance is not
implied by this core-content scenario.

## Source and API contract

`Assets/Scripts/Eclipse/Modding/FightProjectiles.cs` adds a dedicated projectile
root eligibility predicate. It accepts the existing main-fighter scope or a
registered, successfully initialized live actor via the production `ActorValid`
guard. Original owner health, current body/round/session, retirement and lifetime
remain prerequisites. The main-fighter motion/playback predicate is unchanged.

Queue acceptance, materialization, native timeline capacity checks, birth
initialization, queries and live-child mutation all use this root eligibility.
Ownership filtering remains declaring mod plus actual caster root; there is no
opponent query or main summoner access to its actors' children. Capacities remain
16 per mod across all roots and 64 per fight, counting reservations/births.
Definition ownership, offsets, finite motion, query budget and callback expiry
remain enforced. No new Lua function or schema field is introduced.

`FightActors.cs` cancels that root's pending projectile reservations and fails
their receipts, then retires live/initializing tracked children before removing
the actor registry identity. Its existing native-child teardown also remains.
`MoonSharpScriptRuntimeProjectiles.cs` rejects operations during `on_actor_end`;
`on_actor_spawn` may queue after settled birth. Lua uses the existing
`fighter:spawn_projectile`, `fighter:projectiles`, `projectile:snapshot`,
`projectile:move_by` and `projectile:remove` with `combat.projectiles`.

The existing native Model spawn path links children to the actual actor root
and its current hostile target. Existing attack-source capture supplies both
projectile ID/owner and actor ID/owner. Actor combat dispatch supplies the actual
caster and victim callbacks; main fighters receive no attribution for the
companion's shot. Native equipment timeline spawning has the same eligibility,
but full timeline-cast acceptance on actors remains a separate scenario.

## Complete public example and tooling

`Mods/example.ranged-actors/` (1.0.0) ships a Ranged Companion Duel HUD on Act I
Tournament 3. Each settled actor has private typed cooldown/shot/hit state and
creates/guides core shuriken children with ordinary Lua. The native flight has
zero forward velocity; Lua supplies motion. AI and controls are disabled for
these bodies, and no explicit caster playback or fixture attack brain is used.
Hold/resume changes firing, dismissal removes roots/children, and replacements
start fresh. The unchanged core flight binary is reused from Return Dart.

The `Tools/ModdingEditor/templates/ranged-actors/` starter mirrors all four
authored/asset files. Schema/generation describes actor-root projectile identity;
the validator rejects terminal actor projectile operations and still requires
`combat.projectiles`. The wiki reference, ability guide, actor guide, examples
and editor README describe the same scope, timing, lifetime and capabilities.

## Verification

`TestRangedActorsUnity.ps1` passes **49 full-game native checks** in a marked,
isolated Unity 6000.6.0f1 fixture with fresh post-tutorial Campaign profile:
`Temp/FighterPlaybackUnity-d997a2c104cd4564b9767ae6eb4b68d8/validation-9b72778cf39841e79ad74c6aa69134ab.log`.
Actual public Lua/HUD registration and spawning produce independent roots and
actor-scoped child receipts. Native shuriken linkage/rendering/flight/contact,
all six delivered hit/damage callback categories, repeated bidirectional damage,
copied actor/projectile provenance, unchanged main life, hold/resume and pause
pass. Applied native damage is 0.0749998092651367; first observed pools were
9.85000038146973 / 9.92500019073486, reflecting two hits versus one at that point.

Public Lua dismissal with live children clears registries and reservations;
fresh replacements initialize their own state. The final boundary race is
explicitly controlled through the production queue and actor reference:
queue another child, retire its root before materialization, require a failed
receipt/capacity release and preserve the sibling's live children. Surrender then
cleans the remaining root/children/HUD and delivers each terminal callback once.
The controlled boundary call is not presented as an uncontrolled Lua playtest.

Original native regressions pass on the same final source: Return Dart **45**
checks, log
`Temp/FighterPlaybackUnity-d997a2c104cd4564b9767ae6eb4b68d8/validation-cdcd0a9258b340c294a839dbd063fade.log`;
reactive Scripted Actor Sparring **57**, log
`Temp/FighterPlaybackUnity-d997a2c104cd4564b9767ae6eb4b68d8/validation-3fb07623fbd549e6a34c84b02e382ec1.log`.

`TestProjectiles.ps1` passes **585 production Lua/projectile checks**, compiling
the actual projectile/motion/playback partials with controlled native model
inputs. Added cases cover initialized/birthing actor eligibility, main/sibling
query isolation, actor/projectile provenance, motion, capability checks, terminal
operations and failed receipts before creation/initialization. Actor validity
inputs are deliberately controlled here; these are not native anatomy, collision
or full actor-owner/lifetime acceptance.

All four managed projects compile using remapped Unity 6.6 Windows references.
Editor generation/check and **57** project tests pass. Actual LuaLS accepts the
complete source and actor-host projectile method completion. Real VS Code passes
**21** integration checks. Wiki build passes with 246 dedicated API sections,
63 pages/search and **6815** checked local links/assets/fragments/Pages paths.
Astro reports zero diagnostics; the existing duplicate 404 route warning remains.

Initial fixture compilation used an incorrect UI member/type; the harness was
corrected to the established view/widget lookup and double health observations.
The example initially used a nonexistent warrior lookup/wrong shape; it now
uses the actual default template/items contract. A later run exposed flat
position access; both the source and guide now use copied `position.x` fields.
Those failures are not counted as acceptance.

## Limits

Evidence covers two core Skeleton/knife roots and a core missile/shuriken child
in a controlled offline Campaign encounter, with controlled main input/AI,
spacing and profile. Further native death/TTL/owner-disable races, mixed-mod packs,
actor equipment timeline casts, arbitrary rigs/outfits, every arena/stance,
custom outcomes, exports, raids, multiplayer/rollback and sustained performance
remain open. This does not complete the general Minecraft-style modding vision.
