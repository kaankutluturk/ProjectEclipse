# Weighted humanoid import acceptance

## Implemented contract

The human has prioritized arbitrary rig import and complete custom combat
movesets above unrelated API additions. Experimental 3D is deferred. This change
delivers a humanoid compatibility adapter, not closure of arbitrary body plans.

Tools/Animation/ImportCharacter.py reads Blender, FBX and glTF weighted meshes.
RigMapping.py matches common semantic names, then tries a conservative rest-head
and hierarchy inference for unfamiliar clear humanoid layouts. Overrides are
semantic roles, not 67 manually assigned SF2 points. Ambiguous names/topologies
fail. Extra weighted bones inherit their nearest mapped ancestor.

Source geometry becomes native planar SkinnedNode helpers with up to 16 weighted
segment attachments per vertex. Along/across fractions rotate with native limbs;
runtime Y inversion and fighter facing are accounted for. Existing archival
MacroNode math is unchanged. A project-owned partial ModelMacroNode.Skin.cs holds
the implementation; recovered loader/update hooks are narrow. Model.FacingSign
renames an unused wrapper property with the required best-guess marker; the
underlying native method and same-token fields in other types remain unchanged.

The canonical 67-node order, masses, physics/collision edges and weapon anchors
are retained. A name-order fingerprint rejects incompatible targets. Core body
figures are replaced with the imported silhouette. Owned logical armor/helm with
empty models retain combat slots and explicit zero starting stats matching the
vanilla bare body/head; core Fists and Skeleton remain. The warrior inherits
normal native eligibility, unarmed combat and Standard AI. The generated owned
encounter selects this warrior for player and opponent; no campaign loadout
replacement, authored default clips or preview-only AI are required.

Geometry preflight and the Python validator support the new node format: earlier
distinct endpoints, 1..16 influences, weights in 0..1 summing to 1 within .0001,
finite along/across within +/-100, existing composition limits. XML asset format,
import guide, editor guide and test/tool indexes update together. Lua members,
manifest/save contracts and authored editor schema are unchanged.

## Real external source

At the human's explicit request, downloaded Cesium Man from the primary
[Khronos sample asset collection](https://github.com/KhronosGroup/glTF-Sample-Assets/tree/edc7c9e67c639d230715049ee31f9a96a6babbbe/Models/CesiumMan).
The asset is credited to Cesium and licensed CC BY 4.0; retain the source
README/license and trademark qualification. No third-party model or derived
geometry is vendored in Eclipse. FetchOnlineRig.py explicitly downloads the
pinned fixture into a fresh directory and verifies the GLB fingerprint.

- Revision: edc7c9e67c639d230715049ee31f9a96a6babbbe.
- GLB: 438044 bytes; SHA256
  b7001eaeea8254bd44773bcd247e78696d94169388fbb2a1800fc69434e777d9.
- Ignored source: Temp/OnlineRig-CesiumMan/CesiumMan.glb; source metadata/license
  and download fingerprints retained alongside it.
- The first import failed name matching. Hierarchy inference then mapped its
  unfamiliar torso/neck/arm/leg names without --mapping or source edits.
- 3273 source vertices become 1244 output vertices and 2484 triangles. Toe bones
  inherit the foot regions; original mixed weights participate.
- Default display-bone tails produced an exploding silhouette despite numerical
  combat checks passing. This result is explicitly rejected as visual acceptance.
  Calibration now uses anatomical joint heads and dominant weighted extents for
  terminal regions, with 3D source segment frames and planar native output. The
  native probe adds independent geometry bounds and requires screenshot review.
- Accepted local package: Temp/OnlineRig-CesiumMan/local.cesium-fighter-v2.
  Its README/SOURCE_LICENSE.md include source credit, license and modifications.

## Verification

- Six mapping tests cover naming families, explicit unknown names, ambiguity,
  clear unnamed humanoid topology, transformed rest space and unsupported layouts.
- Original 19-bone mixed-weight fixture exports Blender, FBX and glTF. Final
  TestImportedRigPipeline.ps1 fixture:
  Temp/ImportedRig-497f4bd20cb14cb5a837ffe75a3e729a. All three imports pass without
  overrides; the 100-vertex reduction path produces 59 vertices. Offline checks
  require unchanged core node/edge XML, inherited extra bones, mixed weights,
  composition limits and planar derivation. Real Lua registration proves logical
  equipment, core Fists, Standard tactic and playable encounter without clips.
- Six point-pipeline and nine package regressions pass. Authored geometry passes
  42 production C# validator/loader checks with a controlled native parser.
- All four Unity 6.6 Windows-reference managed assemblies compile through the
  ignored Temp/CreatorPlatformCompile projects. Local Visual Studio SDK resolution
  requires these portable compile projects; no exported build is inferred.
- Real downloaded Cesium package passes 17446 native checks with a fresh isolated
  profile and successful Unity exit. It checks movement, controlled Punch with
  damage and victim reaction, an explicitly established mirrored core stance
  followed by native Kick input, Standard AI outgoing attack/incoming contact,
  skin coordinates/geometry bounds in both facings and surrender cleanup.
  This is controlled native controller input, not physical device acceptance.
  Accepted numerical/visual run log:
  Temp/FighterPlaybackUnity-d997a2c104cd4564b9767ae6eb4b68d8/validation-ff0327e1a16e4a389a3c6928e277cca8.log.
  Final capture run also exits successfully with a fresh result:
  Temp/FighterPlaybackUnity-d997a2c104cd4564b9767ae6eb4b68d8/validation-c8bdb8ed0ce94b009a1d22260a953394.log.
  Separate idle/contact screenshots were inspected and copied to
  Temp/ImportedRigReview-20261004/cesium-idle.png and cesium-punch.png. They show
  recognizable fighters at combat scale, with imperfect shoulder/foot deformation;
  this is functional import acceptance, not finished visual quality.
- Editor generate/check/test pass: 255 functions/aliases/callbacks, 74 constants,
  296 structures and 61 project tests. The authored Lua contract is unchanged;
  LuaLS/VS Code integration tests were not repeated for this asset-format change.
- Wiki types/build/search and reference coverage pass: 0 type errors/warnings/hints,
  64 pages, 7045 local links/assets, 64 tracked current-branch source links and
  three source-link tests. Existing duplicate-404 build warning remains.

## Remaining scope

Native default combat on this external humanoid and original fixtures is proven.
Unusual body plans, clavicle/twist topology without recognizable semantic names,
independent extra-bone animation, source actions and complete custom combat
movesets remain open. Imported collision proportions still use the core rig.
Textures/materials and 3D rendering are not imported. Automatic reduction can
change topology and interpolated weights; every model needs visual/contact review.
Broad outfit quality, throws/KO/win/loss coverage, performance, physical devices,
exported platforms and multiplayer/rollback remain unverified. A passed set of
coordinate checks cannot substitute for a usable silhouette.
