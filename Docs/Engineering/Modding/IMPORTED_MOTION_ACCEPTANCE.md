# Imported humanoid source actions

2026-10-04. Continues the human's arbitrary-rig and complete custom combat
moveset priority. The preceding mesh import deliberately omitted source actions;
this change retargets selected evaluated actions. It does not close complete
custom movesets, independent extra bones or arbitrary body plans.

## Source and API path

- ImportCharacter.py adds --list-actions and repeatable --clip NAME ACTION KEY.
  Listing reads the source without package output. Import resolves action tracks
  on the selected armature, rejects absent/empty actions and duplicate/invalid
  names/controls, and samples before publishing a fresh atomic package.
- HumanoidMotion.py reuses the semantic regions from RigMapping.py. It evaluates
  armature actions and constraints at 60 Hz. Rest joint heads establish arena
  forward/up/depth, core node distances establish segment lengths, and evaluated
  joint directions/relative rotations drive the ordered native points. Core
  hand/foot/weapon marker shapes and torso/head helper offsets remain. Derived
  native points are recalculated through CharacterPipeline.py.
- Root travel retains forward/up and removes depth translation. A constant
  rest-floor offset retains jumps; it does not solve support-foot contact.
  Partial actions start from rest bone channels. Previous action, NLA use,
  frame/subframe and pose channels restore after sampling, including failure.
  Other animated constraint targets remain evaluated; no NLA stack is baked.
- Output includes binary clips, editable frames, rig/payload fingerprints,
  control/sample metadata and evaluated source observations. Limits are 1..240
  source fps, positive action duration, 2..36000 samples and at most two million
  node samples across all clips. At most eight distinct controls can be assigned.
- Existing sf2.moves.register declares character-scoped, opponent-facing,
  priority-150 MOVE clips, mid_frames=0 and full-duration Uninterrupt. Existing
  sf2.tactics.register inherits Standard and cycles eligible clips through
  on_decide; no eligible clip defers to native AI. Warrior/fight selection and
  logical equipment use the preceding imported-rig contract. Without --clip,
  normal core combat and Standard tactic remain.
- Generated actions have no attack intervals. Modders author attack edges,
  active/recovery frames, damage, reactions, combos/cancel rules and AI through
  ordinary Lua declarations. No arithmetic/operation DSL, new Lua binding,
  capability, manifest or save format is introduced. Wiki, editor guide and
  tool/test indexes document the feature in the same change.

## Verification

- Blender 3.6.23 original action integration passes:
  Temp/HumanoidMotion-4e668e2d8d6746c6909b99fc48842213. OriginalStrike and
  OriginalJump export 61 samples each from a 30 Hz source. Checks compare native
  arm/leg directions with evaluated source observations and core lengths,
  require wrist displacement, retained jump height and return to original root
  height. Separate Blender checks exercise partial-action isolation, restoration
  of action/frame/subframe/pose, and empty-action rejection. Source hash is
  unchanged. Missing actions, duplicate names/controls and unknown controls reject
  without publishing a package. Real Lua proves controls, character scope,
  exact sample bounds, no inferred attacks and per-clip eligible AI selection.
  Cooldown waits apply only while an imported action is eligible; no eligible
  action defers to native AI even during cooldown. Actual Lua covers both paths.
- Original mesh-only Blender/FBX/glTF/reduced pipeline passes again with current
  importer: Temp/ImportedRig-2fbef480327646d6a5c209f058c3a3c8. Six mapping, six
  point-pipeline and nine package regression tests pass.
- The pinned Cesium Man source from IMPORTED_RIG_ACCEPTANCE.md supplies
  Anim_0_Armature, source frames approximately 1..48 at 24 Hz. Hierarchy mapping
  requires no overrides; the selected action exports 119 samples. Local package:
  Temp/OnlineRig-CesiumMan/local.cesium-motion-v3. Source credit/license retained beside
  the ignored package; neither external source nor derived assets are vendored.
- Full native Unity 6.6 run passes 18070 assertions, successful exit and fresh
  result. Most assertions compare native binary points or weighted vertices.
  It proves the unchanged exported payload reaches the actual native reader,
  public Lua player/opponent identities, controller Punch selection and visible
  wrist motion on both controlled facings, weighted skin coordinates/bounds,
  eligible-action Lua AI and surrender cleanup. Log:
  Temp/FighterPlaybackUnity-d997a2c104cd4564b9767ae6eb4b68d8/validation-74be8fa143624d7d9f3debb3c9e83d2d.log.
- Two earlier probes timed out waiting for the exact StanceIdle name, despite
  an advancing native fight clock. The corrected fixture waits for STAGE_FIGHT
  and input readiness. The unchanged v2 package then passes; no forced initial
  animation or repaired asset substitutes for normal startup. Mirrored setup
  deliberately establishes a core stance after a controlled teleport.
- Captured imported-source-motion.png was inspected and copied to
  Temp/ImportedRigReview-20261004/cesium-source-motion.png. It shows recognizable
  walking silhouettes at combat scale. Final post-fallback-fix capture was also
  inspected and retained as cesium-source-motion-final.png. Shoulder deformation, floating/sliding
  feet and contact fit remain imperfect; numerical playback is not polished art.
- Full-game fixture compiles the updated native probe; actual Lua validator
  compilation passes. Production C# assemblies are unchanged in this feature.
  Editor generate/check/test pass (255 bindings, 74 constants, 296 structures,
  61 tests). LuaLS/VS Code integration is not repeated for unchanged API members.
  Wiki types/build/search and three source-link tests pass, 0 type diagnostics,
  7049 local links/assets and 64 current-branch source links across 64 pages.
  Existing duplicate-404 build warning remains.

## Remaining requirements

Source actions are now supported through this bounded humanoid adapter. Arbitrary
body plans, additional independently animated bones, full source proportion/mesh
preservation, materials, contact IK, general root/orientation conventions and
complex scene/NLA interchange remain open. Complete authored combat replacement,
custom attack/contact/reaction/KO/win/loss/throw coverage on retargeted characters,
physical devices, exported platforms, performance and multiplayer acceptance
remain unproven. Experimental 3D stays deferred and the broad objective stays active.
