---
title: Character imports and point-rig tools
description: Import weighted humanoid characters with normal combat defaults, or author native SF2 point motion and geometric skins.
---

To bring in a weighted humanoid from another tool, use the character importer below. Players can run it from the game itself (**Mods > Characters > Import model...**) and use the result as Shadow's look; see [Replace Shadow's look](#replace-shadows-look). For authoring native SF2 motion with a visible body and IK controls, start with [Gymnast Tool Suite and Eclipse packaging](../gymnast/). The lower-level point tools later on this page support format experiments; their point objects alone are not a complete character authoring interface.

Eclipse characters use SF2's point-based physics rig. An animation stores the positions of those points in a fixed order; body and equipment models attach geometry to them. These low-level tools require Python 3 and Blender 3.6 or newer. Blender 3.6.23 is the tested version for this importer only.

Use Blender 3.6.23 for the source-action workflow below; newer Blender action APIs are unverified.

The point pipeline supports body proportions, native geometry overlays, animation import, constrained or keyframed point motion, baking, validation, a local preview, and runtime registration. The imported-character adapter below adds automatic matching for recognized humanoid rigs. Neither tool turns Blender materials into game shaders. Preserve the native rig's names and point order when sharing the game's moves, equipment, and physics.

## Import a weighted humanoid character

`Tools/Animation/ImportCharacter.py` accepts a `.blend`, `.fbx`, `.glb`, or `.gltf` with an armature and meshes weighted to it. It builds a fresh, installable mod with a playable fighter and an AI counterpart. You do not need to rename the source bones to SF2 points or author the default animations. The generated warrior uses normal core unarmed movement, attacks, hit reactions and the `Standard` tactic. It keeps the native combat skeleton behind the imported silhouette.

Extract the canonical skeleton using the command in the next section, then run:

```powershell
$blender = 'C:\Program Files\Blender Foundation\Blender 3.6\blender.exe'
& $blender --background --factory-startup --python-exit-code 1 --python Tools/Animation/ImportCharacter.py -- --source Temp/MyCharacter/fighter.glb --rig Temp/CharacterCore/models/mdl_skeleton.xml --mod-id local.my-fighter --output Mods/local.my-fighter --title "My Fighter"
```

Enable the mod, Apply & Restart, and choose **My Fighter** on the map. Both participants use the imported character. The encounter leaves your campaign equipment alone. Keep the source file separately; the importer does not save changes to it. The output directory must not exist and its name must equal the mod ID. `import.json` records bone matching, inherited extra bones, mesh reduction, source fingerprint and Blender version.

| Option | Requirement/default |
| --- | --- |
| `--source` | Required source file. The importer reads rest bones and mesh weights, independently of the current animation pose. |
| `--rig` | Required canonical `mdl_skeleton.xml`; default combat depends on its ordered 67 points. |
| `--mod-id`, `--output` | Required non-reserved lowercase mod ID and fresh directory with that name. |
| `--title` | `Imported Fighter`; 1–80 characters. |
| `--voice` | `Male` or `Female`; default `Male`. Chooses the character's gendered combat sounds (grunts and hit cries) and is written as the warrior's `voice`. |
| `--armature` | Optional exact object name. Without it, the armature that deforms the most mesh vertices is used. Only meshes bound to this armature participate. |
| `--mapping` | Optional JSON object from semantic role to exact source bone name. Overrides automatic matching; unfamiliar clear humanoid hierarchies can be inferred without this file. |
| `--max-vertices` | `3000`; 3–3500 total output vertices. Dense meshes receive seam welding and automatic decimation, which can change topology and interpolated weights. If the first pass is over budget, the importer retries with stricter reduction before rejecting. |

Matching splits bone names into words, so it recognizes Mixamo (`mixamorig:LeftForeArm`), Unreal (`lowerarm_l`), Blender/Rigify (`DEF-forearm.L`), VRoid (`J_Bip_L_LowerArm`), 3ds Max Biped (`Bip01 L Forearm`), Character Creator (`CC_Base_L_Forearm`) and Daz (`lForearmBend`) conventions, with namespace prefixes and left/right markers anywhere in the name. A leading word that at least 80% of the bones share, such as a model ID (`4jtr01t0 l thigh`), is ignored like a namespace; `import.json` notes it. It requires `pelvis`, `chest`, `head`, and each side's `upper_arm`, `forearm`, `hand`, `thigh`, `shin`, and `foot`; `spine` and `neck` are optional additional regions. Names identify the limbs and head. The hierarchy then derives the rest: the chest is where the two arms branch, the pelvis is the nearest pelvis/hips bone above both thighs (or their junction; when the legs hang from a bare root beside a bone named pelvis or hips, that bone is used), and the spine and neck are the first bones on the way from pelvis to chest and from chest to head. Numbered spine chains, clavicles, twist bones and end/nub bones therefore need no mapping file.

If limb names are unfamiliar, a conservative hierarchy/rest-position fallback looks for a pelvis with two downward leg branches, a trunk/chest with two arm branches, and a central head branch. Arm and leg chains may start with a clavicle or hip bone and continue into fingers or toes; the importer picks the upper/lower segments with comparable lengths. Unweighted leaf bones (end markers, IK targets) and short side branches such as twist bones are ignored. The report records `mapping_method` as `names` or `hierarchy`. Unusual or ambiguous layouts still need overrides: ambiguous matches fail with the role and candidate names instead of choosing silently. For unfamiliar names, supply only the missing or ambiguous roles, for example:

```json
{ "pelvis": "Joint_001", "left_forearm": "Joint_014" }
```

Source bones outside these regions, such as fingers and twist bones, follow their nearest mapped ancestor; bones above the pelvis (a root or hip helper) and unrelated top-level props follow the pelvis. The report lists these substitutions. Meshes participate when they have an Armature modifier pointing at the chosen armature, or when they are parented directly to one of its bones (rigid props such as a helmet or visor follow that bone). Mesh shapes are read in the armature's rest pose with the Armature modifier applied, as Blender shows them. Small meshes (under a quarter of the stature) that are enclosed by other geometry in all six axis directions, such as teeth, tongues and eyeballs, are skipped: they never reach the silhouette. The vertex budget is shared by each mesh's visible surface area as well as its detail, seams and doubled shells finer than 0.15% of the stature are welded, detached specks under 0.8% of the stature are removed, and an underused budget gets one more, finer pass. `import.json` reports every skipped mesh and removed speck. A mesh whose rest vertices mostly lie farther from their own bones than 60% of the character's height is skipped and listed in `import.json`: some game exports keep interior parts such as teeth at the origin and move them only with an animation pose the file does not contain. Meshes hidden from rendering and lower levels of detail (`LOD1`, `LOD2`, … when an `LOD0` exists) are skipped. Vertices without bone weights follow the nearest anatomical segment, and a negligible weight on a far-away bone is dropped; `import.json` counts these as `repaired_vertices`. Zero-length anatomical segments and meshes bound to two armatures reject with a diagnostic. Non-bone vertex groups are ignored.

The adapter **poses the source mesh in 3D** and draws its orthographic projection with the game's flat silhouette renderer, so you see what a profile camera would see of the posed model. The native skeleton is three-dimensional: SF2's stance turns the torso toward the camera, and heads, arms and legs move in depth. Each body region follows a 3D frame built from the native pose. The torso, neck and head take their facing from the native front points (`NPelvisF`, `NStomachF`, `NChestF`, `NHeadF`); arms and legs swing from their parent segment; feet use the ankle. Turned torsos, helmets, skirts and limbs pointing at the camera therefore keep their real outline. **Fingers follow the native hand**: SF2 animates each hand's knuckle bend (open for chops and handsprings, closed for fists and weapon grips), and the importer turns each finger's last three bones into phalanges that fold about the knuckle line by that amount, distal joint first, for a real fist at full curl. The thumb and any metacarpal bones stay on the palm, and the hand faces along the palm (wrist to knuckles) so a closing fist does not tilt it. Native animation has no forearm twist or thumb motion, so those stay at their rest orientation. The character keeps its **own proportions**: the importer picks one scale that makes it as tall as a standard SF2 fighter and applies it to every part, so a big head, short legs or long arms stay that way. Gameplay still runs on the standard 67-point skeleton (moves, reach, collision and equipment are unchanged); only the drawing uses a *visual proportion rig* built each frame from the native pose. The torso, neck and head follow the native directions at the character's own lengths. Hands stay on the native wrists and feet on the native ankles and heels, so held weapons, hits and floor contact line up; elbows and knees bend to the same side as the native elbow and knee, judged against the native limb itself, with the character's own limb lengths. A nearly straight native limb bends like a real one (elbows back, knees forward), and a limb that cannot reach is straightened and stretched. The pelvis moves along the body by the difference in leg length. Because hands and feet are pinned, a character with much shorter or longer limbs than Shadow strikes from the same places as Shadow, not from where its own limbs would reach. Reduction protects vertices where two body regions blend, so elbows and knees keep a bend. The adapter replaces visible armor and helmet meshes with empty models on owned logical equipment. It preserves core collision proportions, equipment anchors and animation ordering. Source textures, materials, independent extra-bone motion and custom hitbox proportions are not imported. Actions are optional, as described below. This is a humanoid compatibility adapter, not support for arbitrary quadrupeds, wings, tails or unusual locomotion. Inspect deformation, both facings and collision fit in the game before sharing. Custom combat moves can target the generated warrior through the [moves API](../../api/moves-and-tactics/); importing a mesh does not author those moves or their hit timing.

The original synthetic 19-bone fixture exercises Blender, FBX and glTF import without a hand-authored point mapping. A downloaded [Cesium Man sample](https://github.com/KhronosGroup/glTF-Sample-Assets/tree/edc7c9e67c639d230715049ee31f9a96a6babbbe/Models/CesiumMan) also imports through hierarchy matching without overrides and passes native movement, Punch/contact/reaction, controlled mirrored Kick and Standard AI contact. Source joint positions and weighted region extents determine calibration; display-bone tails are not trusted. These are bounded tests, not evidence that every rig or outfit works. Keep attribution and licensing with any third-party asset you distribute.

### Import from the game

Desktop builds and the editor can run the importer for you. Install [Blender](https://www.blender.org/) 3.6 or newer; Eclipse does not bundle it. Then open **Mods > Characters > Import model...**, choose a `.glb`, `.gltf`, `.fbx` or `.blend` file, give the character a name, pick a **Voice** (Male or Female; select it to switch), and choose **Import**.

- Eclipse looks for Blender in the standard install folders (`Program Files\Blender Foundation\Blender x.y`, Steam, `/Applications/Blender.app`, `/usr/bin`) and on `PATH`, newest version first. Use **Locate Blender...** to choose `blender.exe` yourself; the choice is remembered.
- Blender runs in the background with the same importer and the game's own combat skeleton. The page shows its progress; **Cancel import** stops it.
- **Animations** lists the file's animations (Blender reads them in the background). Select an animation to bind it to a control, cycling through Punch, Kick, Ranged, Magic, Up, Down, Forward and Back, or leave it not imported. Bound animations become [source actions](#import-source-actions): they play on that control but deal no damage until you add attack timing in the mod's `scripts/character.lua`.
- The new mod is written to your Mods folder as `local.<name>` (a number is added if the name is taken) and is chosen as Shadow's look. The result page shows a **preview**: your model from the side, then the fighter in the idle stance, a front kick and a back handspring, drawn exactly as the game will. **Apply & Restart** keeps and loads it, **Discard** deletes the new mod, and **Share as ZIP** packages it.
- If the skeleton cannot be matched automatically, choose **Match skeleton...**. Each body part shows the bone matched by name, if any; parts in red still need one. Select a part to pick its bone from the skeleton (indented by hierarchy), then **Import with this skeleton**. Spine and neck are optional. This writes the same [mapping file](#import-a-weighted-humanoid-character) the command line accepts.
- Other failures show the importer's message; the full Blender output is in the game log.

The in-game importer uses core combat and at most 3,000 vertices. The file picker is available in the editor and Windows builds. Released Windows, Linux and macOS players include the importer scripts in their data folder (`CharacterImport`). Blender 3.6.23 is the tested version.

### Replace Shadow's look

**Mods > Characters** lists every loaded mod warrior that declares `skin_models` (such as imported characters), plus imported characters installed since the last restart. Choose **Use** to make that warrior's skins Shadow's look; choose **Shadow** to go back.

| Behavior | Details |
| --- | --- |
| What changes | Shadow's armor and helmet still load (the same points, edges and physics) but their geometry is not drawn, unless **Armor: shown** is selected on the Characters page; then it is drawn over the chosen character, fitted to Shadow's body. The chosen warrior's `skin_models` are drawn over Shadow's own body. If the warrior declares a `voice`, Shadow's gendered combat sounds use it; otherwise Shadow keeps his voice. |
| What stays | Shadow's items, stats, perks, moves, skeleton, collision and weapon, ranged and magic visuals. Armor and helmets still give their stats. |
| Where it applies | Campaign fights, the shop and other screens built from your profile's fighter. A fight that sets its own `player_character`, or a form change to another character, uses that character's models. |
| Versus | Your own fighter uses your look on your device: player one in local versus and training, and your side online. Other players see the original look because looks are not sent over the network. A look changes only drawing, never points or physics, so rollback and replays stay in sync. Replays and spectating show the original look. |
| Where it does not apply | The title screen's sparring preview is unchanged. |
| Sharing | **Share** on an imported character (or **Share as ZIP** after importing) writes `<mod id>.zip` to an `Eclipse characters` folder on your desktop and opens it. Others install it with **Mods > Install ZIP**. Only share models you have the right to share; keep the source's license with it. |
| Storage | A local preference on this device, not part of your save. Missing or disabled mods fall back to Shadow's look with a log warning. |
| Restart | Switching between loaded looks applies to the next fighter the game builds. A newly imported or newly enabled mod needs **Apply & Restart**. |

The skins are fitted to the skeleton of the warrior that declared them. Imported characters use the canonical skeleton, which is the same as Shadow's, and draw with their own proportions as described above. Skins made for a different `body_model` still bind to the same native point names but can fit less well.

### Import source actions

An **action** is a Blender animation with keyed object or bone channels. FBX and glTF importers can create actions from the source file's animations. List their exact names without creating a package:

```powershell
& $blender --background --factory-startup --python-exit-code 1 --python Tools/Animation/ImportCharacter.py -- --source Temp/MyCharacter/fighter.glb --list-actions
```

Add `--clip NAME ACTION KEY` for each action you want to bring into a fresh character package. `NAME` is a unique lowercase identifier (1–48 characters, letters/digits/underscores, starting with a letter); `ACTION` is the exact source action name; `KEY` is a distinct control from `Punch`, `Kick`, `Ranged`, `Magic`, `Up`, `Down`, `Forward`, or `Back`. Quote action names containing spaces. For example:

```powershell
& $blender --background --factory-startup --python-exit-code 1 --python Tools/Animation/ImportCharacter.py -- --source Temp/MyCharacter/fighter.blend --rig Temp/CharacterCore/models/mdl_skeleton.xml --mod-id local.my-motion --output Mods/local.my-motion --clip strike "My Strike" Punch --clip jump "My Jump" Kick
```

The importer reuses semantic bone matching; no per-SF2-point mapping is required. It evaluates selected armature actions and constraints at 60 samples per second, keeps core segment lengths and hand/foot shapes, and recalculates derived points in native order. Forward and vertical root travel are retained; sideways root translation is removed for the arena plane. A fixed rest-floor offset preserves jumps; it is not foot-contact IK. Unkeyed bone channels begin at rest so a partial action does not borrow an old pose. The selected action replaces armature NLA blending during sampling; separately animated constraint targets remain evaluated. Inspect their timing when exporting an action in isolation.

Each clip needs positive duration, a scene rate of 1–240 fps, 2–36,000 output samples, and at most two million native point samples across the package. Tracks that do not resolve on the selected armature reject. Empty, missing, degenerate or over-limit actions fail before publication. Source files are not saved. Binary assets, editable `.frames.json`, `.rig.json` fingerprints and `.retarget.json` source observations are written under `assets/animations/`; `import.json` records the actions and controls.

Generated Lua moves are scoped to the character, face the opponent and use `mid_frames = 0`. The generated tactic cycles eligible imported clips, then falls back to Standard AI when none is eligible. Without `--clip`, the character keeps Standard AI and normal core controls. Imported clips have **no attack intervals** and hold `Uninterrupt` for their duration. They are motion previews, not a complete combat moveset; inherited core moves still exist. In `scripts/character.lua`, edit their [typed move declarations](../../api/moves-and-tactics/) to add attack edges, damage, hit reactions, active frames, recovery/cancel conditions, combos and appropriate AI decisions. Do not treat an animation's visual contact as proof that native damage or collision is configured.

## Create an authoring scene

Run these commands from the repository root. The extraction writes a separate working directory and leaves the shipped assets intact:

```powershell
dotnet run --project Tools/AssetPacker -- extract Assets/StreamingAssets/SF2Content/ArtBundles/MODELS.tar.lz4 Temp/CharacterCore
$blender = 'C:\Program Files\Blender Foundation\Blender 3.6\blender.exe'
& $blender --background --factory-startup --python-exit-code 1 --python Tools/Animation/BlenderCharacter.py -- create --rig Temp/CharacterCore/models/mdl_skeleton.xml --blend Temp/MyCharacter/character.blend
```

Open the resulting `.blend` in Blender. `SF2_Nodes` contains named point objects, and `SF2_Skins` is the collection for your optional geometry. Each point's `sf2_node` property preserves its binding; renaming the visible Blender object is harmless, but changing or removing the binding is rejected during export. `sf2_mass` controls the exported point mass. The original rig XML is stored on the scene.

To import a native animation too, add `--animation path/to/clip.bytes` to `create`. Add `--mid-frames N` if its move uses intermediate frames; the default is 0 and the supported import range is 0–8. This expands source keyframe spacing before baking. The scene runs at 60 frames per second. Blender frame 1 becomes game frame 0.

## Author the body and motion

Move the point objects or constrain them to your own Blender armature. The exporter samples their **evaluated world positions**, so constraints can drive the animation. Keep every rig binding. Author motions across the scene's start/end frame range; both endpoints are included.

The scene's first frame is also the exported body's rest pose. Changing proportions updates native edge lengths. Check the resulting collision shape and equipment fit in the game, especially after large proportion changes. Macro nodes and centers of mass are derived from their original dependencies during baking; animate their source points instead of trying to override a derived point directly.

The coordinate conversion is native `(x, y, z)` to Blender `(x, -z, y) / 100`. Native animation Y is up. Attack impulses use the separate native physics convention, so do not copy Blender coordinates directly into an impulse table.

## Retarget motion from another Blender armature

`Tools/Animation/RetargetCharacter.py` transfers an evaluated armature animation to the native point rig. Import your donor animation into Blender using its appropriate importer and save a `.blend` first. This tool reads that scene; it does not include FBX import, automatic bone matching, foot locking or a character controller.

Create a JSON mapping with `version: 1` and a `bindings` object. Map **every** native point whose XML `Type` is `Node` to a source pose-bone name. Do not map `MacroNode` or `CenterOfMass` helpers: their positions are calculated from their dependencies. Several points may use the same bone, allowing a rigid body segment to carry multiple landmarks. For example, this fragment shows the format; add the remaining native points before using it:

```json
{
  "version": 1,
  "bindings": {
    "NElbow_1": "forearm.L",
    "NElbow_2": "forearm.R"
  }
}
```

Choose a reference frame where the donor's pose is aligned with the native rig's reference pose. The tool calculates a bone-local offset for each native point at that frame. Sampling the reference frame therefore reproduces the native point positions; subsequent evaluated bone transforms carry those offsets. Bone rotation, constraints, object transforms and object animation participate. This preserves the initial SF2 proportions but does **not** guarantee constant edge lengths or planted feet during motion. Align the donor's facing, scale and pose before calibrating, and inspect bends and root travel afterward.

```powershell
& $blender --background Temp/Donor/donor.blend --python-exit-code 1 --python Tools/Animation/RetargetCharacter.py -- --rig Temp/CharacterCore/models/mdl_skeleton.xml --mapping Temp/Donor/bindings.json --armature Armature --reference-frame 1 --start 1 --end 60 --scale 100 --output Temp/Donor/retargeted
python Tools/Animation/CharacterPipeline.py validate --rig Temp/CharacterCore/models/mdl_skeleton.xml --animation Temp/Donor/retargeted/retargeted.bytes
python Tools/Animation/PackageCharacter.py --rig Temp/CharacterCore/models/mdl_skeleton.xml --animation Temp/Donor/retargeted/retargeted.bytes --mod-id local.retarget-preview --output Temp/Donor/local.retarget-preview
```

`--scale` defaults to **100 native units per Blender unit**. Use `1` for a donor already using Gymnast's native scene scale. The coordinate conversion is `(Blender X, Blender Z, -Blender Y) × scale`; source object/root movement is retained. Start/end frames are inclusive, and the source scene's effective frame rate must be 1–240 fps. The reference frame may be outside the sampled range. The output is baked at 60 fps with `mid_frames = 0`, subject to the same frame/sample limits as the point baker.

The output directory must not exist. It contains `retargeted.bytes`, its rig fingerprint sidecar, sampled `frames.json`, an interactive `preview.html`, and `retarget.json` recording the bindings and sampling settings. Keep the original donor scene separately. Missing bones, incomplete or derived-point bindings, invalid coordinates and singular calibration transforms are rejected. The tool restores the scene's original frame after sampling and does not save or modify the source file.

Blender 3.6.23 tests exercise evaluated bone constraints, translation, calibration offsets, helper nodes, frame-rate conversion and invalid mappings. `Tools/Animation/TestRetargetPipeline.ps1` additionally creates a synthetic donor, retargets the canonical 67-point rig, validates the exported fingerprints, packages the character, executes its real Lua registrations and reads its 61-frame output through the recovered animation reader in Unity 2022.3.62f3. This verifies integration, not humanoid motion quality: arbitrary donor skeletons, deformation and in-game contact quality still require creator validation. See the [Gymnast packaging guide](../gymnast/) for the generated preview mod and multi-clip controls.

## Add a geometric skin

Place mesh objects in `SF2_Skins`. Apply mesh modifiers before export. Mesh faces are triangulated, and each vertex is attached to four non-coplanar rig landmarks using the native weighted-point calculation. The exporter chooses nearby landmarks automatically. To control an attachment, assign that vertex to exactly four positive vertex groups whose names match rig points. Group membership chooses the landmarks; the exporter computes the attachment weights from the rest pose rather than using the Blender weight values.

Coplanar landmarks and unstable attachments are rejected with the mesh and vertex identified. Use nearby landmarks spanning all three dimensions, then preview the skin during strong twists and bends. The export limit is 2,048 skin vertices; the composed model permits at most 4,096 points. Geometry uses native triangle figures and the game's shadow rendering; Blender UVs, textures, materials, and arbitrary bone-weight skinning are not exported.

The original body and template equipment remain present. A skin is an additional geometric layer, not a replacement for every equipped item. Change the character's typed equipment bindings when a different outfit or weapon is required.

## Export, validate, and preview

```powershell
& $blender --background Temp/MyCharacter/character.blend --python-exit-code 1 --python Tools/Animation/BlenderCharacter.py -- export --output Temp/MyCharacter/package
python Tools/Animation/CharacterPipeline.py validate --rig Temp/MyCharacter/package/assets/models/body.xml --skin Temp/MyCharacter/package/assets/models/skin.xml --animation Temp/MyCharacter/package/assets/animations/authored.bytes
```

Omit `--skin` when your scene contains no skin mesh. Repeat it for additional overlays. Export produces:

| File | Purpose |
| --- | --- |
| `assets/models/body.xml` or `body.modelz` | Native body rig and geometry. `.modelz` is gzip-compressed UTF-8 geometry; use the extensionless model ID in Lua. |
| `assets/models/skin.xml` or `skin.modelz` | Optional native geometric overlay. |
| `assets/animations/authored.bytes` | Baked native animation at 60 Hz, with `mid_frames = 0`. |
| `assets/animations/authored.rig.json` | Node order and rig/animation fingerprints for validation. |
| `frames.json` | Portable sampled animation source. |
| `preview.html` | Local interactive point-rig preview with playback and frame scrubbing. |
| `character.generated.lua` | Character and input-bound move registration module. |

Open `preview.html` locally. It previews motion and rig edges; native materials, skin deformation, contact physics, and move selection still require a game test. Re-exporting writes the named outputs. Use a fresh output directory if you removed a skin, so an older `skin.xml` is not mistaken for current output.

The validator checks model references, helper dependency order, finite coordinates, animation node counts, truncated payloads, and sidecar fingerprints. It rejects XML DTDs/entities. Model XML is limited to 16 MiB; animation export permits 2–36,000 frames and at most two million node samples. A changed rig or changed clip requires rebaking its sidecar.

### Geometry checks when the game loads your character

The game also validates mod-authored `body_model` and `skin_models` before constructing their native geometry. A model handle checks that an asset exists; it does not validate every point binding during Lua registration. Geometry errors appear when the character is loaded or prepared. The diagnostic names the model, XML element, and field, for example:

```text
Authored character model 'my.mod:models/skin.xml' /Scene/Nodes/SkinTip @ChildNode1: unresolved or forward binding 'MissingPoint'; dependencies must appear earlier.
```

Fix the named field in your source model and export again. A failed character preparation does not replace a live fighter. An actor that fails preparation reports the error through its spawn receipt.

| Geometry rule | Supported values and defaults |
| --- | --- |
| Root structure | `Scene` with a `Figures` element, including an empty one. |
| Nodes | Unique composed names; `Type` is `Node`, `MacroNode`, `CenterOfMass`, or `SkinnedNode`. The complete character permits 1–4,096 nodes. |
| Numeric fields | Finite values within ±100,000. Missing ordinary coordinates, mass, radii, lengths, and margins default to zero. Mass, lengths, and radii cannot be negative. |
| Helper nodes | `NodesCount` is required, 1–128; every `ChildNode1`…`ChildNodeN` must resolve to a node declared earlier in the composition. Each `MacroNode` requires its corresponding finite `LCC1`…`LCCN` weight. `CenterOfMass` children need positive total mass. |
| Weighted skin nodes | `SkinnedNode` requires `BonesCount` 1–16. Each influence requires distinct earlier `BoneStartN`/`BoneEndN` names, `WeightN` in 0–1, `AlongN` in ±100, and exactly one of `AcrossN` (relative, ±100), `OffsetN` (absolute, ±1,000), or a 3D `LocalXN`/`LocalYN`/`LocalZN` position (±1,000; then without `AlongN`). `ExtendN` (absolute, ±1,000, default 0) is allowed only with `OffsetN`. Weights must sum to 1 within 0.0001. |
| Edges | `Type` is `Edge` or `Muscle`; `End1` and `End2` must resolve. `Iterations` defaults to 1 and permits 1–32. Extra iterations are named `EdgeNameCI1`, `EdgeNameCI2`, and so on; those expanded names must also be unique. The complete character permits at most 8,192 expanded edges. |
| Figures | Unique names within each authored document; `Type` is `Triangle` or `Capsule`. Triangles require resolved `Node1`, `Node2`, and `Node3`; capsules require a resolved `Edge`, including expanded iteration names. |

Body documents precede equipment; skins follow it in their listed order. Give overlay nodes and edges distinct names instead of redeclaring body bindings. These strict authoring checks apply to your mod's body/skin and declared dependency models. Core model handles and recovered equipment retain their legacy loading behavior. This preflight does not prove good deformation, animation compatibility, or hit contact; those still need the fight checks below.

`SkinnedNode` is a derived planar mesh vertex. For each influence, the file-space position is `start + Along × (end − start) + Across × perpendicular(end − start)`, then weighted and summed. `perpendicular(x, y) = (-y, x)`. Along/across values are fractions of that segment's current length. The runtime handles file/runtime Y conversion and reverses across with fighter facing, so the silhouette mirrors with the fighter. Z is zero. Unlike `MacroNode` landmark coefficients, the across offset rotates with the limb. These nodes follow native animation and physics; they are not additional independently animated bones or collision edges.

An influence can instead give absolute distances in native units. With `OffsetN` (and optional `ExtendN`), the position is `start + Along × (end − start) + Extend × u + Offset × perpendicular(u)`, where `u` is the segment's on-screen direction scaled to unit length. When a segment points toward the camera, its on-screen length is taken as at least 35% of its full 3D length, so absolute offsets shrink smoothly instead of jumping. Use absolute offsets for thickness that should not grow or shrink with a segment's length; the character importer writes them. Relative and absolute influences can be mixed in one node and one document.

A skin document can add a `<Proportions Version="1">` element beside `Nodes` and `Figures`. Its skinned nodes then bind to a visual rig instead of the native points: gameplay is unchanged, and only that document's drawing uses the listed lengths. It must contain one `<Segment Start End Length/>` for each of these 21 segments, with `Length` in native units from 0.01 to 1,000:

| Segments | How the visual rig uses them |
| --- | --- |
| `NPivot`→`NStomach`→`NChest`→`NNeck`→`NHead`→`NTop` | Torso chain from the visual pelvis, along the native directions. |
| `NNeck`→`NShoulder_1`/`_2`, `NPivot`→`NHip_1`/`_2` | Shoulder and hip offsets from the visual neck and pelvis. |
| `NShoulder_N`→`NElbow_N`→`NWrist_N` | Two-bone IK from the visual shoulder to the native wrist. The elbow goes to the side the native elbow bends to, relative to the native shoulder–wrist line; a nearly straight native arm bends backward, away from `NChestF`. |
| `NHip_N`→`NKnee_N`→`NAnkle_N` | Two-bone IK from the visual hip to the native ankle. The knee goes to the side the native knee bends to, relative to the native hip–ankle line; a nearly straight native leg bends forward, toward `NPelvisF`. |
| `NWrist_N`→`NFingertips_N`, `NHeel_N`→`NToeTip_N` | Hand and foot from the native wrist and heel. |

The body must also contain `NChestF` and `NPelvisF` for these bend directions. The visual pelvis moves from the native pelvis toward the feet (along the neck-to-pelvis axis) by the native average thigh-plus-shin length minus the listed one. Without `<Proportions>`, skinned nodes bind to the native points as before.

```xml
<Proportions Version="1">
  <Segment Start="NPivot" End="NStomach" Length="11.5" />
  <!-- ...one Segment for each of the 21 segments above... -->
</Proportions>
```

With `<Proportions>`, a document can also add `<Rest Version="1">` and use **3D influences**. When `<Rest>` also lists a `<Frame>` for every segment a node uses, the node's influences are blended as dual quaternions instead of averaged points, so elbows, knees and shoulders keep their volume instead of pinching. Each 3D influence gives `LocalXN`, `LocalYN`, `LocalZN`: the vertex's position in the segment's frame at the source rest pose, in native units. The runtime builds a right-handed frame for each segment on the visual rig, places the point, and keeps only its X and Y (orthographic projection); facing left mirrors the result. The segments and their frames:

| Segment | First axis | Second axis |
| --- | --- | --- |
| `NPivot`→`NStomach`, `NStomach`→`NChest`, `NChest`→`NNeck` | Along the segment. | Toward `NPelvisF`, `NStomachF`, `NChestF` from the segment start. |
| `NNeck`→`NHead`, `NHead`→`NTop` | Along the segment. | From `NHead` toward `NHeadF`. |
| `NShoulder_N`→`NElbow_N`→`NWrist_N`→`NFingertips_N`, `NHip_N`→`NKnee_N`→`NAnkle_N` | The parent frame turned by the shortest rotation from the `Rest` direction to the current direction. | Parents: arm from `NChest`→`NNeck`, leg from `NPivot`→`NStomach`, then each previous segment. |
| `NHeel_N`→`NToeTip_N` | Along the foot. | Toward the native ankle. |

Before posing, each arm and leg chain is put on its anatomical side. SF2 keeps `_1` points on the `up × front` side of the torso (front from `NChestF` for arms, `NPelvisF` for legs), but some native clips are authored with left/right labels reversed, and a fighter facing left can have its pairs swapped by the game. The native rig is symmetric so this never shows there; a 3D skin would hang one limb's mesh on the other limb. A chain found on the wrong side is swapped back; near edge-on poses keep their previous choice so a chain cannot flicker.

Optional `<Rest>` children:

| Element | Meaning |
| --- | --- |
| `<Frame Start End O A B/>` or `<Frame Dynamic="k" O A B/>` | A segment's source rest frame: origin `O` (`x,y,z`, ±100,000) and orthonormal first and second axes `A`, `B`. Enables dual-quaternion blending. |
| `<Dynamic Id Parent Head Dir Length Stiffness Damping/>` | A spring bone (coat tail, hair, cloth), numbered from 0. `Parent` is a segment (`NStomach-NChest`) or an earlier spring (`#2`); `Head` is its start in the parent frame, `Dir` its unit rest direction, `Length` 0.01–1,000, `Stiffness` (default 0.15) and `Damping` (default 0.9) 0–1. The runtime simulates its tip each step (gravity, lag behind the parent, fixed length) and snaps it back when the fighter turns or teleports. Influences on a spring add `DynamicN="k"`; their `BoneStartN`/`BoneEndN` name the chain's root segment. Drawing only. |
| `Twist` on a hand `Bone` | The native rest pose's hand roll in degrees, measured from `NKnucklesS`. The hand turns by the difference from it (at most ±150°), and the forearm follows with a share that grows toward the wrist. |

The importer writes all of these. It makes spring bones from unmapped rope-like chains (each bone with at most one child) of two or more weighted bones, or single bones named like hair, cloth, coat, skirt, cape, tail and similar. Face rigs, twist helpers and props stay rigid. The body must also contain `NKnucklesS_1`/`_2`.

`<Rest>` needs one `<Bone Start End X Y Z/>` for each of the ten arm and leg segments: the segment's rest direction as a unit vector in its parent's frame. The body must contain the four front points and `NKnuckles_1`/`_2`. The third axis is always the first crossed with the second. The hand frame's current direction is wrist to knuckles, so its `Rest` direction is the source palm (wrist to the finger bases).

A hand influence (on `NWrist_N`→`NFingertips_N`) can add `CurlN` (1–3, the phalanx level) and `PivotN`: one `x,y,z` hand-local joint position per level, proximal first, separated by spaces (each value ±1,000). The runtime measures the native knuckle bend (wrist→knuckles against knuckles→fingertips), subtracts the hand bone's optional `Curl` attribute in `<Rest>` (the source's own rest finger bend in degrees, 0–180), and divides by 75° for a curl share from 0 to 1. The point then rotates about each pivot from the deepest level up, by the share times 90°, 100° and 70° for levels 1, 2 and 3, about the native knuckle line.

```xml
<Rest Version="1">
  <Bone Start="NShoulder_1" End="NElbow_1" X="0.02" Y="-0.11" Z="0.99" />
  <!-- ...one Bone for each arm and leg segment... -->
</Rest>
<ImportedVertex3 Type="SkinnedNode" X="0" Y="0" Z="0" BonesCount="1"
  BoneStart1="NElbow_1" BoneEnd1="NWrist_1" Weight1="1" LocalX1="12" LocalY1="3.5" LocalZ1="-1" />
<!-- A fingertip on the second phalanx: folds at both knuckle joints. -->
<ImportedVertex4 Type="SkinnedNode" X="0" Y="0" Z="0" BonesCount="1"
  BoneStart1="NWrist_1" BoneEnd1="NFingertips_1" Weight1="1" LocalX1="9" LocalY1="0.4" LocalZ1="1.1"
  Curl1="2" Pivot1="6.2,0.3,1.0 7.9,0.3,1.1" />
```

```xml
<!-- Skin document: both endpoints already exist in the body. -->
<ImportedVertex Type="SkinnedNode" X="0" Y="0" Z="0" BonesCount="1"
  BoneStart1="NElbow_1" BoneEnd1="NWrist_1"
  Weight1="1" Along1="0.5" Across1="0.12" />
<!-- Absolute: halfway along the forearm, 4 units in front of it. -->
<ImportedVertex2 Type="SkinnedNode" X="0" Y="0" Z="0" BonesCount="1"
  BoneStart1="NElbow_1" BoneEnd1="NWrist_1"
  Weight1="1" Along1="0.5" Extend1="0" Offset1="4" />
```

Other authoring tools can produce `frames.json` with `version = 1`, `fps`, `names`, and `frames`. Each frame is an array of `[x,y,z]` points matching `names`. Names must match the rig exactly; the baker reorders them to native order and resamples input at 1–240 fps to 60 Hz:

```powershell
python Tools/Animation/CharacterPipeline.py bake --rig body.xml --source frames.json --output authored.bytes
python Tools/Animation/CharacterPipeline.py preview --rig body.xml --animation authored.bytes --output preview.html
```

## Install the character in a mod

Create a mod using the [editor starter](../vscode/) or an existing fight example. Its manifest needs `content.register` and a dependency on `core`. Copy the package's `assets` directory into the mod. Copy `character.generated.lua` to `scripts/character.lua`, then load it from `scripts/main.lua`:

```lua
local sf2 = require("sf2")
local authored = require("character")
-- In your existing fight registration:
-- warriors = { authored.warrior }
```

The generated module uses the core kung-fu template and binds the authored move to **Punch**, restricted to this character. It returns `warrior` and `move` handles. It is a movement preview without attack damage. Set a suitable tactic on the character if an AI opponent should choose it; a [programmable tactic](../../api/moves-and-tactics/#on_decide) receives it only when native conditions allow it.

To make the motion an attack, edit its existing registration: set `type = "ATTACK"`, choose a control, and add an attack interval. For example, this fragment assumes a clip long enough to include frame 12 and a leg motion that contacts during frames 6–8:

```lua
conditions = {
    { character = character },
    { keys = { { "Kick", press = "Tap" } } },
    { not_interval = "Uninterrupt" },
},
events = { "key_pressed" },
intervals = {
    { type = "Uninterrupt", from = 0, to = 12 },
    { type = "Attack", from = 6, to = 8, attack = {
        edges = { "ECalf_2" }, damage = 0.12,
        damage_type = "UnarmedDamage", hit = "High",
        impulse = { x = 245, z = 350 },
    } },
},

```

These are fields inside a move definition, not a standalone Lua script. Attack edges must exist on the composed fighter. [Moves and tactics](../../api/moves-and-tactics/) documents all field defaults and limits. Use ordinary [combat callbacks](../../api/combat-callbacks/) for procedural effects instead of adding an operation language to the move table.

## Verify in a fight

For a complete source example, see
[Authored Fighter Lab](https://github.com/dawc17/ProjectEclipse/tree/main/Mods/example.authored-fighter).
Its mod-owned body keeps the standard 67-point binding layout; a connected sash
adds six weighted helper points after equipment. Its original strike is generated
through the existing baker rather than copied from a core clip. The Lua move
declares attack frames and native contact edges, and public actor commands start
it from either side. The HUD also supports a spacing reset, repeated strikes
with Lua approach movement, and dismissal. Native hit reactions can change the
pair's positions; reset spacing before inspecting a controlled single strike.

Enable the mod, apply and restart, then open **Authored Fighter Lab** on the
Act I Campaign map. Its owned fight declares `player_character = character`
alongside its opponent handles. The character occupies the native player slot
at encounter creation; no later form request is needed. Its setup screen lets
you choose the authored fighter or core comparison character before launching.
The callback resolves a prepared plan with `player_character = selected`, so
the choice is saved with the encounter and retained for a launch retry. Back
cancels the pending choice without starting combat. The blueprint remains the
authored character. This field requires
`content.register`, keeps saved equipment intact, and also applies when a mode
uses that fight. Omit it to use the saved player's ordinary setup. See
[fight registration](../../api/content-graph/#sf2fightsregister) for defaults
and restrictions. The example's repeatable mode entry and Tournament 3 HUD
provide additional ways to try the character.

Within any lab HUD, choose **Try authored player (Punch)** and wait
for **Player form: applied**. Use your normal Punch control to select the strike.
The example declares its input and readiness checks in the move definition:

```lua
-- Fields within sf2.moves.register; character is the registered warrior handle.
events = "controlled",
conditions = {
    { character = character }, { key = "Punch" }, { controllable = true },
},
```

The character condition keeps this binding off other warriors. `controlled`
includes native key presses and eligible animation/interval endings;
`controllable` prevents starting the move during an uninterruptible action.
The example's HUD calls `fighter:change_form(character)` from its player rule's
tick callback and displays the live result. This needs `combat.transform`.
Try it while the pair is alive: the companions retain their IDs, behavior state,
health and remaining lifetime, then continue attacking each other. The owner
changes form; the companions keep their existing bodies. Actor references must
still be reacquired in each callback.

Use **Left form: core / authored** to change the companion's own body. The
core switch uses `actor:change_form(core_form)` from the main rule; the return
uses `fighter:change_form(character)` inside that actor's private behavior.
The actor keeps its ID, state, team, maximum/current health and remaining
lifetime. The button stops repeated strikes; restore the authored character,
reset spacing and restart repetition to verify its native attack again.
Actor references require `combat.actors`, and either form method requires
`combat.transform`. Native AI memory and ongoing attacks are not transferred;
owned projectiles from the retired actor body are removed. Read
[actor forms](../../api/actors/#actorchange_form) for timing and receipts.
Its **Try core comparison form** button selects another registered warrior
without the authored body. That is a comparison character, not a snapshot or
restoration of your original equipment. The owned lab uses its prepared
character choice; Tournament 3 uses the saved player setup on restart. Read
[form changes](../../api/combat-callbacks/#fighterchange_form) for
state retention, failure handling and supported limits.

Reproduce its assets using a fresh directory, then open the generated
`preview.html` to scrub the point motion:

```powershell
python Tools/Animation/BuildAuthoredFighterExample.py --body Mods/example.authored-fighter/assets/models/body.xml --output Temp/MyAuthoredFighter
python Tools/Animation/CharacterPipeline.py validate --rig Temp/MyAuthoredFighter/body.xml --skin Temp/MyAuthoredFighter/sash.xml --animation Temp/MyAuthoredFighter/strike.bytes
```

The source generator is a small editable example, not a replacement for
Blender/Gymnast or a general animation importer. Its body geometry is authored
on the compatible native rig; core idle, walking and hit reactions remain in use.
Equipment may add many further helper points without changing the animation's
67 ordered points. Read the sample README for installation and controls. The
experimental 3D renderer approximates standard anatomy and can change the look
of custom silhouette details.
Its connected body now uses native top/face/knuckle landmarks for the head and
hands. Missing those compatible bindings keeps the recovered surface/stroke
fallback. Clothing thickness follows the animated panel's local surface;
complex outfits can still need individual treatment. This presentation option
does not change the animation's point order or native collision geometry.

The repository's `Tools/Tests/CharacterForms/TestAuthoredFighterUnity.ps1`
tests this complete installed example in an isolated Unity fight. It checks
native clip decoding, moving skin helpers, mirrored attack contact in both
directions, repeated Lua attacks, actor-specific callbacks, pause and cleanup.
It also changes the player through the public form request, dispatches native
controller Punch events, checks the authored attack's real contact/callbacks,
and verifies that the comparison form uses core Punch selection.
Pass `-PlayerEntry` to choose the authored character through the native lab
setup screen, or `-ComparisonEntry` to choose its core comparison. These routes
also check cancellation, saved choice, isolated blueprint/instance parameters,
and player control flags before any form request. The runner
starts the registered fight directly; it does not verify clicking the map or
mode menu entry.
This establishes the sample's compatible procedural export; it does not validate
every Blender/Gymnast export, custom rig, outfit or platform. Run comparable
fight checks on your own character.

Check the character at idle, mirrored on each side, walking, attacking, hit, and knocked down. Confirm equipment follows the rig, the intended key selects the move only for this warrior, the attack deals damage during its intended frames, and unrelated fighters retain their controls. Test the skin under the largest bends. The repository's `Tools/Animation/TestCharacterPipeline.ps1` exercises Blender export, the unchanged native animation reader, and Lua/native-XML projection; those checks cannot establish visual or combat correctness for your authored content.
