# Character and animation tools

`ImportCharacter.py` imports weighted `.blend`, FBX and glTF humanoids into a
fresh playable mod using core unarmed combat and Standard AI. `RigMapping.py`
(`map_rig`) matches limbs and the head by name words (Mixamo, Unreal, Rigify,
VRoid, Biped, Character Creator, Daz), derives pelvis/spine/chest/neck from the
hierarchy, and falls back to topology for unfamiliar names; explicit overrides
remain available. Source weights become rotating native SkinnedNode attachments
posed in 3D on frames from the native pose (front helpers, twist-free limb
swings) and projected orthographically, at one height-fitting scale. The skin's `<Proportions>`
keeps the source's own segment lengths on a visual-only rig
(`CharacterPipeline.visual_joints`, `SkinProportionRig.cs`) whose hands and feet
stay on the native wrists/ankles; extra bones follow mapped ancestors and root bones follow the pelvis.
Source materials, unusual body plans and custom collision proportions are not
imported. The game's **Mods > Characters** page runs this importer through the
user's Blender (`Assets/Scripts/Eclipse/Modding/CharacterImporter.cs`) and can
apply any loaded warrior's `skin_models` as Shadow's look; desktop builds copy
the five importer scripts to `<Data>/CharacterImport`
(`Assets/Editor/CharacterImporterBuildProcessor.cs`). `SkinPreview.py` draws the preview strip the
game shows after an import. `TestRealRigs.py --blender <exe> --rig <mdl_skeleton.xml> --models <folder>
--output Temp/<report>` re-imports a local folder of third-party rigs (never copied into the repository),
checks reach/budget/meshes and writes a preview per model. Read the
[import guide](../../Docs/Modding/src/content/docs/guides/character-authoring.md#import-a-weighted-humanoid-character)
for commands, limits and the mapping format. `CreateImportedRigFixture.py`
authors original weighted Blender/FBX/glTF fixtures; `TestRigMapping.py` checks
name matching and rejection. Full-game acceptance uses
`Tools/Tests/CharacterForms/TestPackagedCharacterUnity.ps1 -ImportedRig` with a
generated package.

`ImportCharacter.py --list-actions --source <file>` lists source animation names.
Repeat `--clip NAME ACTION KEY` during import to retarget selected evaluated
humanoid actions using the same automatic region map. `HumanoidMotion.py` keeps
core lengths, evaluated directions/rotations and forward/up root travel, with
constant rest-floor calibration. Generated scoped Lua moves have no inferred
attacks; author timing/contact through the public move tables. Use
`TestHumanoidMotionPipeline.ps1 -Blender <exe> -Rig <canonical-model>` for original
strike/jump, partial-action isolation, source preservation, binary geometry and
actual Lua input/AI contracts. `TestPackagedCharacterUnity.ps1 -ImportedMotionClip
<name>` separately checks a Punch-bound clip's native playback on both controlled
facings, weighted skin and AI; screenshots still require visual review.

`BuildPulseArenaArtwork.py --output Temp/PulseArtwork/assets` generates the
original four-frame atlas and cropped sprite descriptors used by Pulse Arena.
It needs only Python standard-library modules. Live sprite animation and motion
are Lua behavior; see the arena API and native acceptance runner.

`BuildAuthoredFighterExample.py` reproducibly authors the original point strike
and connected weighted sash in `Mods/example.authored-fighter` from its owned
body XML. It uses the existing baker/binary writer, preserves the standard 67
point bindings and emits editable `strike.frames.json`, native assets, fingerprints
and a point preview to a fresh directory. Run `TestAuthoredFighterExample.py` for
asset reproduction, motion/limb lengths and connected/deforming helper contracts.
The example's public Lua/HUD and real native loading/contact are checked separately
by `Tools/Tests/CharacterForms/TestAuthoredFighterUnity.ps1`.

For new SF2 characters and motions, start with the [Gymnast workflow](../../Docs/Modding/src/content/docs/guides/gymnast.md).
`OpenGymnast.ps1` prepares and opens the upstream visible IK body; `GymnastBridge.py`
validates native node bindings and exports evaluated motion. `PackageCharacter.py`
packages existing native exports as an installable, repeatable map preview with
an AI opponent that cycles through eligible authored moves. Add clips with
`--clip NAME KEY MID_FRAMES FILE`; see the Gymnast guide for controls, timing,
output files and the shared rig requirements. The supplied upstream checkout
is loaded unchanged; no third-party add-on or scene is bundled here.

Add `--playable` to `PackageCharacter.py` or `GymnastBridge.py export --package`
to use the exported warrior as the preview's player, with the same generated
opponent. Default watched previews retain the campaign player. This reuses the
public fight player_character selection and current move short-form tables;
it leaves source/native asset bytes, profile and timing intact. Authored preview
moves have no attack intervals; inherited moves can still deal damage.

Run `TestGymnastPipeline.ps1 -Blender <exe> -Suite <checkout>` for the real Blender,
Lua and Unity-reader integration fixture. The full-game `Tools/Tests/CharacterForms/TestPackagedCharacterUnity.ps1` checks
controlled generated Gymnast player input, both facings, point/skin binding and
AI playback. Hit contacts, physical devices and arbitrary rigs remain separate. The older `BlenderCharacter.py` tool
is a low-level point-rig workflow, not the primary visual authoring experience.

The offline `CharacterPipeline.py` model validator matches the runtime authored
body/skin preflight for helper counts/weights, positive COM mass, nonnegative
lengths/radii, expanded edge iteration names and composition limits. Run
`python Tools/Animation/TestCharacterPipeline.py` and `TestPackageCharacter.py`
for these contracts and packaging. `Tools/Tests/CharacterForms/TestAuthoredGeometry.ps1`
exercises the production C# validator and loader integration with a controlled
native parser. `TestAuthoredGeometryUnity.ps1` uses the full game and real native
model preparation, with cached test body/skin documents. That controlled loader
test does not establish public mod registration, authored animation/contact or
visual correctness.

## Tekken 8 to SF2 animation experiment

The first conversion produced a **format-compatible prototype**, not a finished
combat move. It uses the user's `grlsu_at_kakato.bin` (identified by the user as
Kazuya ff3), their existing Polaris importer and mannequin, and Eclipse's
canonical packaged `mdl_skeleton`. The conversion preserves vanilla assets and
existing Unity GUIDs. An editor-only preview hook now allows a local combat test;
the user reported that it worked in Eclipse on 2026-08-31.

Generated local files are under `ResearchSources/TekkenRetarget/` and are ignored
by Git. Do not commit or redistribute the Tekken source, mannequin, or derived
animation as part of the open-source base project. The converter does not vendor
third-party code or download anything.

## Research findings

1. **Tekken input:** the supplied file has `PANM` at offset 4, an inclusive last
   sample index of 59 at `0x40`, 60.0 fps at `0x44`, and 26 named tracks at `0x94`.
   Its animated body tracks contain 60 samples. The supplied plugin evaluates
   those transforms on its specific Tekken mannequin in Blender 3.6.
2. **SF2 output:** fighters do not use Unity Animator/AnimationClip assets. They
   use absolute XYZ node positions indexed in model XML order. An FBX export
   alone therefore will not drive the recovered fighter animation system.
3. **Target rig:** `MODELS.tar.lz4` contains the canonical `mdl_skeleton.xml` with
   67 ordered nodes. It includes joints, hand/foot and orientation helpers, eight
   weapon markers, a COM node, and twelve macro nodes. Omitting the extra nodes
   can corrupt mesh orientation or node indexing even if the body looks correct.
4. **Timing:** `FightHolder.FixedUpdate` drives the fight; the project's fixed
   timestep is 1/60 second. `ModelAnimation.SetBufferFrame` and `Bezier` use
   `(MidFrames + 1)` sample spacing, multiplied by the game's slow-motion factor.
   The 60 Hz prototype should start testing with `MidFrames="0"`, not the common
   vanilla value 2. SF2 has its own transition and three-sample interpolation
   behavior, so exact start/end and attack timings still require a playtest.

The upstream [Polaris TKAnimation project](https://github.com/umin135/Polaris_TKAnimation)
documents Blender 3.6, a separately supplied mannequin, and recommends the
non-IK rig for importing. This workflow uses that existing local importer;
it does not substitute the older, limited Tekken 7 0xC8 importer.

Local evidence:

- `Assets/Scripts/Assembly-CSharp/InfoAnimation.cs`: `LoadAnimationClip`,
  `ReadAnimation`, `Init`, and `FillKeyFrames`.
- `Assets/Scripts/Assembly-CSharp/BinaryReaderNekki.cs`: little-endian integers
  and floats through `BitConverter`.
- `Assets/Scripts/Assembly-CSharp/ModelLoader.cs`: `ParseNode` assigns node IDs
  in XML order and negates file Y.
- `Assets/Scripts/Assembly-CSharp/ModelAnimation.cs`: `DrawFrame`,
  `SetBufferFrame`, `PlayInfo`, `MirrorNodes`, and `SetAttackingEdges`.
- `Assets/Scripts/Assembly-CSharp/ModelMacroNode.cs`: `UpdateFromWeights` weighted helpers.
- `Assets/Scripts/Assembly-CSharp/ModelObject.cs`: `UpdateCenterOfMass` mass-weighted COM.
- `Assets/Scripts/Assembly-CSharp/SF2Paths.cs`: `GetBinaryAnimationsPath` animation path.
- `Assets/Scripts/Assembly-CSharp/MovesParser.cs`: file, interval, and frame metadata.
- `Assets/Resources/SF2Content/Art/catalog.json`: `MODELS` archive authority.

### Binary layout

All numbers are little-endian. There is no embedded bone-name list or frame rate.

| Field | Encoding |
|---|---|
| Number of frames | int32 |
| Per-frame flag | uint8; recovered reader ignores it (vanilla stance uses 1 and 5) |
| Per-frame node count | int32 |
| Every node, in model order | float32 X, float32 Y, float32 Z |

The output uses flag 1 and 67 nodes per frame. Its length is
`4 + 60 * (5 + 67 * 12) = 48,544 bytes`. Raw Y is up; `ReadAnimation` changes it
to negative Y internally. Do not pre-negate the output a second time.

### Retargeting choices

- Evaluate the original add-on's body animation on `SINGLE-P1-ARMATURE`, without
  registering the add-on UI or modifying the installed plugin.
- Map Blender `(-Y, Z, -X)` to SF2 `(X, Y, Z)`. Retain pose depth but center root
  translation on the fighting plane. `L` maps to SF2 suffix `_1`, `R` to `_2`;
  runtime mirroring can swap paired nodes and needs in-game verification.
- Transfer limb directions, rebuilding each segment with the dimensions from
  frame 3 of the vanilla `stance_idle.bytes`. The sampled lengths are almost,
  but not exactly, the nominal XML edge lengths. This preserves the donor
  geometry rather than changing the recovered skeleton's dimensions.
- Keep SF2's fist, foot, and weapon-marker shapes, rotating them with the source
  hands/feet. The clip does not contain finger animation. This is an unarmed
  experiment; weapon orientation has not been validated for armed moves.
- Rotate torso/head helper points and recalculate the COM and macro nodes.
- For this grounded kick, shift the complete pose vertically so its lowest
  foot contact is on the floor. This also removes the source mannequin's
  vertical offset. It is **not** contact IK: support-foot sliding can remain.
  Do not use `--ground min-foot` for jumping moves; `--ground none` preserves
  vertical translation but then requires separate floor calibration.

The mannequin lacks `KOSI_NULL2` and `MUNE_jnt`, reported by the original importer.
They are recorded as omitted auxiliary tracks, not silently treated as proven
irrelevant. All required main body tracks are present. Exact Kazuya rig behavior,
mesh deformation, and those omitted tracks are not established by this test.

## Reproduce

From the Eclipse repository root, extract a local copy of the target model with
the existing project tool. If its DLL is absent, first build
`dotnet build Tools/AssetPacker/AssetPacker.csproj -c Release`.

```powershell
dotnet Tools/AssetPacker/bin/Release/net9.0/AssetPacker.dll extract `
  Assets/StreamingAssets/SF2Content/ArtBundles/MODELS.tar.lz4 `
  ResearchSources/TekkenRetarget/models

& 'C:\Program Files\Blender Foundation\Blender 3.6\blender.exe' `
  --background --factory-startup --python-exit-code 1 `
  --python Tools/Animation/ConvertTekkenToSf2.py -- `
  --plugin "$env:USERPROFILE\Polaris_TKAnimation" `
  --rig "$env:APPDATA\Blender Foundation\Blender\3.6\scripts\addons\Polaris_TKAnimation\assets\polaris_base.blend" `
  --input "$env:USERPROFILE\Polaris_TKAnimation\grlsu_at_kakato.bin" `
  --model ResearchSources/TekkenRetarget/models/models/mdl_skeleton.xml `
  --reference Assets/Resources/gamedata/animations/binary/stance_idle.bytes `
  --output ResearchSources/TekkenRetarget/kazuya_ff3.bytes

python Tools/Animation/PreviewSf2.py `
  ResearchSources/TekkenRetarget/kazuya_ff3.frames.json `
  --output ResearchSources/TekkenRetarget/preview.html

python Tools/Animation/AuditRetarget.py `
  ResearchSources/TekkenRetarget/kazuya_ff3.bytes `
  --model ResearchSources/TekkenRetarget/models/models/mdl_skeleton.xml `
  --reference Assets/Resources/gamedata/animations/binary/stance_idle.bytes `
  --report ResearchSources/TekkenRetarget/geometry-validation.json

./Tools/Animation/TestSf2Animation.ps1 `
  -Animation ResearchSources/TekkenRetarget/kazuya_ff3.bytes
```

`preview.html` is self-contained and works offline in a browser. Playback,
frame scrubbing, speed, side/oblique/front views, mirror, and node display are
available. It draws diagnostic limb geometry, **not the game's skinned mesh**.
The converter also writes `.frames.json` and `.report.json`, including hashes
of the source, importer, rig, model, donor, and output for reproducibility.

The Unity test makes a fresh project under `Temp/AnimationReader-*`; it does not
open or change the main project's scenes. If sandbox IPC prevents Unity from
connecting to its license client, run the same test with normal desktop access.
The fixture compiles the **unchanged `ReadAnimation` method extracted from the
working tree**, with the original `BinaryReaderNekki` and real Unity `Vector3`.
It reduces the containing class to two fields; this is not a full fighter test.

## Results for the supplied clip (2026-08-31)

- Blender 3.6.23: importer evaluated all 60 samples and all required body tracks.
- Output: 60 frames, 67 nodes, 48,544 bytes.
- SHA-256: `a999d7d0edc576e44bb0ae180f944f7d74ea266a2092d37907312ed0bb9d9a05`.
- Unity **2022.3.62f3**: TextAsset import and recovered-reader fixture passed
  **16,145 checks**, including count, finite coordinates, Y flip, end-frame
  assignment, and a truncated-payload rejection.
- Independent Python geometry audit: segment-length error below **0.000313 SF2
  units**, macro-node error below 0.000034, COM error below 0.000029. Every
  sampled pose has a grounded foot and a root centered in depth.
- Independent comparison with the imported Tekken skeleton: all **480 limb
  directions** agree within 0.000001 in unit-vector distance.
- The fast downward stroke at frame 17→18 is already present in the imported
  source: source ankle travel 80.123 units versus retargeted travel 109.239,
  consistent with the 1.36308 leg scale. It was not smoothed away. The geometry
  audit reports this fast transition as a review warning.
- Browser: inspected side and oblique poses, contact sheet, scrub and playback.
- **At the initial conversion stage, not run:** full fighter/skinned-mesh playtest, attack detection, opponent
  collision, mirrored combat, interruptions, or full-project managed builds.
  No runtime source files were changed at that stage; see the subsequent local
  gameplay preview below.

## Installed local gameplay preview

Run `python Tools/Animation/InstallFf3Preview.py` to install and enable the sample
for the next Unity Play session. **Forward + kick** temporarily uses ff3 in place
of FrontKick. This changes the shared move definition, so it is not player-only.
The installer clones the vanilla definition and retains its input, selection,
damage, and hit reaction. Provisional timing uses samples 18–22 for the attack
on side `_1`, recovery through sample 46, and `MidFrames="0"`.

Disable **SF2 > Animation Preview > Enable Local Move**, then restart Play, to
restore the ordinary front kick. Alternatively run
`python Tools/Animation/InstallFf3Preview.py --disable`.

The move XML, binary and enable marker live in ignored
`Library/EclipseAnimationPreview/`; clearing Library removes the installation.
Two `UNITY_EDITOR` hooks in MovesParser and ResourceManager apply the local move
and route its binary through `Eclipse.Content.LocalAnimationPreview`. Neither the
override nor the derived animation is included in player builds. This temporary
preview is not a public move-registration API and does not modify vanilla XML.

Main and editor managed builds passed. The user confirmed the move worked in
Eclipse. Hit detection, mirrored combat, balance, floor contact and interruptions
have not been systematically verified. The additional isolated hook test
(`TestSf2Animation.ps1 -Animation ResearchSources/TekkenRetarget/kazuya_ff3.bytes
-WithMovePreview`) was prepared but not run: its launch approval was declined.
The earlier 16,145-check binary-reader validation remains the completed automated
animation check.

## Future distributable integration

For an isolated manual integration test, the existing resource convention is
`Assets/Resources/gamedata/animations/binary/kazuya_ff3.bytes`, referenced by
`FileName="kazuya_ff3.bytes"` in move metadata. Let Unity create a **new** meta
for this new asset; never replace an existing animation or its meta.

Starting metadata values are `FirstFrame="0"`, `EndFrame="59"`, and
`MidFrames="0"`. These are **not a complete Move definition**: the move also
needs an explicit selection/trigger path, alignment, transitions, locks, and
appropriate active intervals. SF2's three-frame interpolation and transition
buffers mean these starting values do not promise frame-perfect Tekken timing.

Prefer an opt-in mod/content experiment when the animation asset path is
supported by that API. Do not put this move into canonical vanilla XML. For a
playable attack, author SF2 attack edges (kicking side `_1` before runtime
mirroring), active timing, damage, hit reaction, recovery, and interruption
rules after checking the skinned character and floor contact. The `.bin` alone
does not supply Tekken moveset gameplay metadata.

This profile has only been tested on the supplied clip and mannequin. It is a
repeatable starting point for other grounded humanoid moves, not a universal
Tekken-to-SF2 converter.

## Character authoring pipeline

`BlenderCharacter.py` imports the native point rig into Blender, samples authored
or constrained motion, exports body/skin models and native animations, and writes
a character-scoped Lua module. `CharacterPipeline.py` validates model references,
bakes portable frame data, and creates an interactive local preview. The full
workflow and format limits are in
[`guides/character-authoring.md`](../../Docs/Modding/src/content/docs/guides/character-authoring.md).

Run `Tools/Animation/TestCharacterPipeline.ps1` from the repository root for the
Blender 3.6 export, native Unity animation reader and Lua projection fixture.
Python-only format tests: `python Tools/Animation/TestCharacterPipeline.py`.
These checks do not replace a fighter deformation/contact-timing playtest.
