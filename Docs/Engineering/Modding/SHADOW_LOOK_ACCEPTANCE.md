# Imported characters as Shadow's look

2026-10-04. Continues the arbitrary-rig priority: a rigged humanoid from another
tool should become a normal SF2 fighter, and the player should be able to use it
in place of Shadow with the orthographic silhouette renderer. Experimental 3D
rendering stays out of scope.

## Implemented contract

Rig matching (`Tools/Animation/RigMapping.py`, `map_rig`):

- Names are split into words; side markers and exporter prefixes are recognized
  anywhere (Mixamo, Unreal, Rigify DEF, VRoid, 3ds Max Biped, Character Creator,
  Daz). Names identify limbs and head only.
- Pelvis/spine/chest/neck come from the hierarchy: chest = common ancestor of the
  upper arms, pelvis = deepest named pelvis/hips above both thighs (or their
  junction), spine/neck = first bones on the pelvis-chest and chest-head paths.
- The previous names-only matcher rejected every complete Mixamo rig (`Spine` and
  `Spine1` both matched `spine`) and its hierarchy fallback rejected clavicles,
  toes and end bones. The fallback now prunes unweighted leaves, follows main
  branches past leaf twist bones, accepts clavicle/hip and finger/toe
  continuations, and picks upper/lower limb segments of comparable length.
- `match_bones`/`infer_humanoid` keep their signatures for existing callers.

Importer (`Tools/Animation/ImportCharacter.py`):

- The skin frame was the frontal plane (`forward x bone`), so profiles were lost
  and torsos showed shoulder width. It is now the sagittal plane the native
  renderer draws: across = counter-clockwise normal in forward/up, matching
  `ModelMacroNode.Skin`. Arms use the hanging-arm frame so T/A poses agree.
- Thickness used each segment's native/source length ratio, so short source
  segments (Mixamo hips) ballooned. Bindings now keep `Along` as a clamped joint
  fraction and write absolute `Extend`/`Offset` at one body scale (head-pelvis
  ratio). Head, hands and feet fit uniformly to the native end segments.
- Decimation starved long limbs (Michelle: 1 forearm vertex, 174 per hand).
  Joint-blend vertices get a protecting decimate vertex group; protection relaxes
  on later attempts so the budget is always reachable. Over-budget results retry
  with stricter per-mesh budgets (up to eight passes) instead of failing.
- Bones above the pelvis and unrelated top-level bones follow the pelvis. Rigid
  bone-parented meshes are included; hidden and LOD1+ meshes are skipped; edge-only
  meshes are skipped. Unweighted vertices follow the nearest segment and unstable
  tiny influences are dropped (`repaired_vertices`). Multiple armatures pick the
  one deforming the most vertices. Conversion samples the rest pose. Progress is
  printed as `SF2 IMPORT STEP:` lines. `import.json` gains `warrior`,
  `mapping_notes` and `repaired_vertices`; the warrior name is the title.

Skin format (runtime `ModelMacroNode.Skin.cs`, `ModCharacterGeometry.cs`, Python
`CharacterPipeline.py`): each influence has exactly one of relative `Across` or
absolute `Offset` (+ optional `Extend`, both +/-1000 native units). Absolute frames
use the on-screen segment direction with a 35% of 3D length floor. Existing
relative packages load unchanged.

Shadow look (`PlayerAppearance.cs`, `ModelParameters.Appearance.cs`):

- `Roster` marks its own `ModelParameters` (`EclipseRosterPlayer`), copied by the
  copy constructor. `PPFDLIBLNDG` omits Armor/Helm documents and appends the
  selected warrior's `skin_models` when a look is chosen. Items, stats, skeleton,
  collision, weapon/ranged/magic models are unchanged. Fighters that already carry
  a mod body/skins/character id (playable mod warriors, forms) are untouched.
- Selection is a local `PlayerPrefs` warrior ID, not save data. Any loaded warrior
  with `skin_models` is eligible. Missing/unloaded looks fall back to Shadow.
  Versus (local/online/replay/spectator) is excluded so peers simulate identical
  models; armor cloth nodes have non-zero mass.
- `ModelLoader` validates appearance skins with the existing authored preflight.

UI (`TitleScreenCharacters.cs`, Mods page button): Mods > Characters lists Shadow,
loaded eligible warriors and imported mods installed since the last restart
(enabling a disabled one on selection). Import model... picks a source file,
names it, finds or locates Blender, runs the importer on a worker thread
(`CharacterImporter.cs`) with the game's own `mdl_skeleton.xml`, shows progress,
supports cancel (removes the killed run's staging folder), selects the new look and
offers Apply & Restart. `CharacterImporterBuildProcessor` copies the five scripts
to `<Data>/CharacterImport` for Windows/Linux/macOS players.

## Follow-up: original proportions

The owner reported that per-segment fitting still resized body parts too
aggressively (a stocky character got Shadow's leg/arm lengths and a shrunken
head). Vanilla skeletons all share the same 67-point lengths and animations are
absolute positions, so gameplay keeps that skeleton. The skin now draws on a
visual-only proportion rig (`SkinProportionRig.cs`, Python `visual_joints`):

- One scale fits source height (torso chain + thigh/shin) to the native height.
  Every binding uses it: Along is source distance x scale / visual segment
  length, Offset is source thickness x scale. Head/hands/feet are no longer fitted
  to native sizes.
- `<Proportions Version="1">` lists 21 visual segment lengths. Torso/neck/head
  follow native directions; shoulders/hips offset from them; wrists, ankles and
  heels are pinned to native points (weapons, contact, floor); elbows/knees use
  two-bone IK toward the native bend; out-of-reach limbs straighten and stretch.
  The pelvis shifts along the neck-pelvis axis by the leg-length difference.
- Validated in C# (`ModCharacterGeometry`) and Python (`read_proportions`); a
  unit test checks pinned points, torso/IK lengths and missing-segment rejection.
- Offline renders of all four real rigs keep their own proportions; source side
  profiles rendered from Blender match (Cesium Man's box head is in the source).
- Known limit: hands/feet are pinned, so a long- or short-limbed character's
  strikes land where Shadow's would; limbs stretch when the native pose is out of
  their reach.

## Follow-up: 3D posing

The owner compared an armored knight with its in-game silhouette; flat 2D segment
frames could not reproduce it. Native data shows why: in `stance_idle` the front
helpers point mostly along depth (Z), so the torso faces the camera and neither a
side-plane nor a front-plane projection is right.

- Influences now store `LocalX/Y/Z`: the source-rest vertex in its region's
  frame at the height scale. Torso/neck/head frames take their second axis from
  `NPelvisF`/`NStomachF`/`NChestF`/`NHeadF`, limbs swing twist-free from their
  parent using `<Rest>` directions, feet use the ankle. The posed point's X/Y
  is drawn (orthographic). Runtime frames are built in canonical coordinates
  (facing sign undone, Y up) so they stay right-handed like the source frames.
- Source frames: segment axis plus forward (torso/head), foot axis plus ankle,
  limbs inherit their parent at rest. Native `up x front` points to the `_1`
  side, matching source `up x forward` = left, so left maps to `_1` without a
  reflection.
- A standalone harness compiled the production `SkinProportionRig.cs` and
  `ModelMacroNode.Skin.cs` with stub engine types and compared all 2289 Soldier
  vertices on a `front_kick` frame with the Python reference: max difference
  3.7e-5 units for both facings (the left-facing result is the exact mirror).
- Default vertex budget is now 3000. The final folder rename retries briefly
  because Windows can lock a just-written folder (seen once in testing).
- Limits: no forearm twist/finger data exists in native animation; MirrorNodes
  can swap paired limbs, which is visually symmetric in silhouette.

## Follow-up: elbow/knee flips

The owner saw elbows and knees bending outward ("broken") in some poses. The IK
chose its bend side from the native elbow/knee measured against the *visual*
shoulder-wrist (hip-ankle) line; once proportions move the visual shoulder or
hip, the native joint can lie across that line and the bend flips. The bend side
is now the native limb's own bend vector (perpendicular to the native root-end
line), projected onto the visual solve, plus a 2%-of-limb anatomical hint
(elbows away from `NChestF`, knees toward `NPelvisF`) for nearly straight limbs.
Skin files are unchanged; existing imports benefit without re-importing.

A sweep over every bundled native animation (18657 frames, 73130 bent limb
samples per character) counted visual bends on the opposite side of the native
bend: old solver 126/428/230/468 (Soldier/Cesium/Michelle/Xbot), new solver
5/0/5/5. The remaining cases are native knees hyperextended by about 2 units on
a ~120-unit leg, which the hint now bends forward like a real knee. The stub C#
harness matches Python within 8e-5 on front_kick, stance_idle and a flying
ability frame in both facings.

## Follow-up: character voice

The importer takes `--voice Male|Female` (default Male) and writes it as the
generated warrior's `voice`; the in-game import page has a Voice toggle. When a
look is chosen for Shadow, `ModelParameters.EclipseVoice` returns the warrior's
voice for `ActionSound`/`ActionRandomSound` gender matching, else Shadow's. The
recovered `ModelParameters.OLPCELPEDKD` (set from XML `Voice`, compared by
`SameGender`) is renamed `Voice` with the best-guess marker. Checked: generated
Lua/import.json carry the voice, invalid values reject, assemblies compile.
Not run: in-game sound playback.

## Follow-up: parts vanishing by facing

Not backface culling (`Mesh/Colored` is `Cull Off`). Facing left is a pure X
negation (`KeyFrames` mirror), but `MirrorNodes` can also swap `_1`/`_2` pair
data, and 12 bundled clips (hit reactions, double_jump_kick, knives_stance_idle,
sticks_heavy_slash, ...) are authored with pair labels reversed relative to the
torso front (up x front points to `_1` in the other 98.5% of 18657 frames). A 3D
skin then hangs a limb's mesh on the opposite limb, twisting shoulders/hips and
burying parts (Soldier's backpack) inside the body. Each arm/leg chain is now
tested against the torso front (shoulders vs `NChestF`, hips vs `NPelvisF`) and
swapped back before posing; at runtime a 0.15 dead band keeps the last choice
for edge-on poses. Offline: swapped and unswapped frames render identically;
reversed clips render coherent bodies. The C# stub harness matches Python within
8.1e-5 on swapped/unswapped frames in both facings.

## Follow-up: finger curl

The owner asked how to handle hands that were always open. Native animation does
animate hands: across 18657 frames the knuckle-to-fingertip bend ranges from 0
degrees (353 clips have open frames: chops, handsprings, flying abilities) to 90+
(fists, weapon grips); stance_idle sits at 48-80. Rather than baking a fist, the
importer finds each hand's finger chains (child chains of the hand bone, trailing
unweighted end bones dropped; thumb by name, else the chain nearest the wrist
when there are five; metacarpals and thumb stay on the palm), makes the last three
bones phalanx levels 1-3 with hand-local pivots, and records the source rest bend
(`<Rest>` hand `Curl`). Runtime folds levels about the native knuckle axis by
share = clamp((bend - rest) / 75 degrees) times 90/100/70 degrees, distal first.
The hand frame now follows wrist->knuckles (previously wrist->fingertips, which
tilted the hand as a fist closed); its rest direction is the source palm.

Checks: Xbot, Soldier and Michelle import with 2-3 phalanx levels (Cesium has no
fingers). Close-up renders show curled fingers in stance_idle and extended ones in
back_handflip. Unit test covers the fold and the native curl share/axis. The stub
C# harness matches Python within 6.7e-5 on stance/handflip/kick in both facings.
Original fixtures still pass. Re-import needed for existing characters.

## Follow-up: game-ripped FBX (prefixed bones, misplaced teeth)

An owner import of an FBX game model failed: every bone was prefixed with a model ID
(`4jtr01t0 l upperarm`), so names never matched, and helper branches (face,
collars, cloth, twist bones) defeated the topology fallback. `shared_prefix`
now ignores a first word shared by at least 80% of the bones (not a side or
anatomical word). The legs hung from a bare root beside the named `pelvis`; when
the first trunk bone matches pelvis/hips it becomes the pelvis. With these, the
model maps by names without overrides (spine2 chest, calf shins).

The rest pose then placed the teeth at the floor: their rest vertices sit about
4.5 units from the teeth bone (the original game lifts them with a pose the file
lacks). The importer now reads meshes with the Armature modifier applied in the
rest pose (previously disabled; identical for the other test rigs), and skips
meshes whose vertices mostly lie farther than 60% of the stature from their
dominant bone segment, reporting them in import.json. Offline renders on idle,
front kick and back handspring match the source profile (cap, long coat, chain)
with no stray pieces. Other test rigs skip nothing; fixtures, 11 mapping and 11
pipeline tests pass. The source model stays local and is not vendored.

## Follow-up: improvement batch

Implemented after the owner asked for the whole improvement list:

- Mesh quality: small enclosed meshes skipped (BVH rays in six axis directions),
  budget shared 50/50 by vertex count and surface area, stature-relative weld,
  speck removal, and one refinement pass when under 80% of the budget is used
  (typical models now land at 2,800-2,900 of 3,000 instead of 1,800-2,300).
- Import UX: SkinPreview.py renders source profile + idle/kick/handspring into the
  package's preview.png (game supplies native clips via ResourceManager.GetBinary);
  result page shows it with Keep (Apply & Restart), Discard and Share as ZIP. On a
  mapping failure the importer prints `SF2 IMPORT BONE/SUGGEST` lines and the game
  offers a role-by-role skeleton matcher that writes the mapping file. An
  Animations page lists source actions (`--list-actions`) and binds them to
  controls (`--clip`).
- Runtime: dual-quaternion blending when the skin has source frames; hand roll from
  NKnucklesS against the import-time native rest roll, spread along the forearm;
  spring bones for unmapped rope chains/named hair-cloth bones (Verlet tip, gravity,
  stiffness/damping, length kept, snap on turn/teleport); knuckle points added to
  the left/right un-swap (a bug since finger curl).
- Armor under a look now loads with its figures skipped instead of being omitted,
  so physics are identical with and without a look; an Armor shown/hidden toggle
  draws it over the character. Your own versus fighter (local P1/training/your
  online side) uses your look locally; LoadoutCode is a fixed 10-byte format the
  room server relays, so looks are not sent to peers. Rollback snapshots skip
  Eclipse.Modding state and macro nodes use an explicit codec.
- Share as ZIP: one top-level folder, as Install ZIP expects, saved to
  Desktop/Eclipse characters.
- TestRealRigs.py: re-imports a local rig folder, checks budget, reach (rest
  stature) and kept meshes, writes previews and report.json.

Checks: 5/5 real rigs pass the runner (Cesium, Xbot, Soldier, Michelle, Jotaro);
original fixtures pass; 13 pipeline and 11 mapping tests pass; all assemblies
compile. Stub C# harness vs Python within 1.4e-4 units on idle/kick/handspring/
hit frames, swapped frames and both facings (springs at rest). Sequence harness
over front_kick, back_handflip and double_jump_kick: no NaN, reach under 0.82
statures, springs settle to zero motion within 240 held steps. Not run: Unity,
in-game UI pages, Blender launched from the game, online versus with a look.

Not done, with reasons: retargeting gameplay to source proportions (would change
hitboxes, reach, throws between differently sized fighters and every native
animation; needs its own design), a Blender-free importer (porting the whole
pipeline including FBX parsing and mesh reduction), and explicit weapon-grip bone
alignment (hands stay pinned to the native wrists; roll and curl now follow the
native hand, and moving the hand to a source grip bone would detach it from the
forearm).

## Verification

- 10 mapping tests pass, including full Mixamo (Spine/Spine1/Spine2, clavicles,
  fingers, toes, end bones), Unreal 5 (spine_01..05, clavicles, twist), Character
  Creator (trunk beside pelvis), VRoid/Biped/Daz/Rigify name parsing and an
  unknown-name hierarchy with clavicles, toes, unweighted ends and a prop.
- 7 pipeline tests pass, including relative/absolute helper positions and the new
  Across/Offset/Extend rejection rules. Package and authored-example tests pass.
- Blender 3.6.23 original fixture: Blender/FBX/glTF/reduced imports pass
  `ValidateImportedRig.py` (Temp/ImportedRig-claude2).
- Real rigs imported without overrides (ignored local copies in Temp/RealRigs, not
  vendored): Cesium Man (hierarchy), and three.js r160 example models Xbot,
  Soldier and Michelle (Mixamo names). Before this change the three Mixamo rigs
  failed matching. Offline rasterization of the runtime formula on native
  `stance_idle` and `front_kick` frames was inspected: recognizable profiles facing
  forward, bent elbows/knees, no ballooned pelvis or arm "wings". Xbot's separate
  joint-sphere mesh leaves a visible ankle gap.
- All four managed assemblies compile through the temporary glob projects.
- Wiki build: 0 type errors/warnings/hints, 64 pages, 7058 local links/assets,
  64 tracked source links. Existing duplicate-404 warning remains.

Not run: Unity editor/player sessions (the owner playtests), the native
`TestPackagedCharacterUnity.ps1` probe, the in-game Characters page and Blender
launch from the game, facing-sign confirmation of absolute offsets in a mirrored
fight, LuaLS/VS Code integration (no Lua API change), and exported player builds.

## Remaining scope

Armor/helmet fitting onto imported bodies, textures/materials, non-humanoid body
plans, imported collision proportions, independent extra-bone motion and contact
IK remain open. Versus does not show looks. The in-game importer exposes no
mapping-file or action-clip options; those stay on the command line.
