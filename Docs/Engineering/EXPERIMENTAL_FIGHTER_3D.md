# Experimental perspective fighter rendering

2026-10-04. Enable **Options > Display > 3D fighters (experimental)**. The
default is off; toggling saves immediately to the installation's PlayerPrefs
(`Eclipse.ExperimentalFighter3D`) and applies to a live fight. Reset rendering
defaults clears it. The shared modern Options UI serves startup and in-game
Settings; the Display rows fit without overlapping their hit areas.

## Source and approach

- `SF2DisplayFrameRate` owns the preference/load/reset path in Eclipse.Runtime.
- `MeshNode.Render` keeps its original flattening by default. The experimental
  argument retains interpolated native XYZ in the presentation vertex buffer.
  It never writes ModelNode, physics, collision, combat or animation state.
- Native `MeshRender` feeds the same authored triangles into project-owned
  `FighterVolume`, with front/back surfaces and side walls six native units away
  from each triangle. `CapsuleRender` feeds its existing interpolated endpoints,
  margins and stroke into rounded 20-sided, 12-ring solid capsules. Coincident
  endpoints become spheres. `EdgeRender` hides flat outline lines in this mode.
  The volume pass attenuates animated Z separation to 35% because recovered
  panels were authored to overlap in a flattened view; solid limb cross-section
  radius is preserved. This reduces detached-looking face/cloth surfaces.
  Original renderers remain available and reappear on toggle off.
- `FighterVolume` owns/destroys its dynamic mesh, uses a shared resource shader
  with per-renderer colour, and preserves native arena/perk colour influence.
  Dark silhouettes receive a dark steel base so surface lighting reveals depth.
- `ExperimentalFighterCamera` belongs to the active player's native ViewerModel.
  Only descendants of that exact viewer can acquire volumes. Menu sparring and
  shop/title previews are excluded, including while in-game Options is open.
  An unparented owned camera uses perspective FOV 32, yaw -22 and pitch 8, matching
  the original orthographic framing at the central fighter plane. It inherits
  the native InvertCamera projection-reflection/culling lifecycle; dropping that
  reflection incorrectly flips the rig upside down.
- The additional depth-clearing camera draws only currently unused layer 30,
  after the original camera. The main camera's layer bit is removed while active
  and restored on off/disable/destruction, without replacing its other mask bits.
  Fight/viewer teardown destroys the camera. Backgrounds and UI use the native
  camera. No scenes, recovered meshes, sprite layouts or existing GUIDs change.
  New owned C# and shader assets have new meta GUIDs.

## Scope and limits

This is a visibly different rendering experiment built from the recovered rig,
not recovered complete 3D skins. Rounded limbs and thickened native polygons
leave seams, faceted heads/cloth and approximate equipment surfaces. There are
no new UVs/textures, smooth skinned character import, 3D arenas, orbit controls,
gameplay depth movement, real-time world shadows or public Lua camera API.
Native hit detection stays in XY. Existing flat effects, foreground occlusion,
floor decals, trails and screen filters are not perspective-integrated. All
equipment/forms/arenas, mod visual combinations, physical controller operation,
exported players and sustained GPU/performance acceptance remain open.

## Verification

`Tools/Tests/Presentation/TestExperimental3DUnity.ps1` uses the matching Unity
6.6 editor in a marked Temp full-game fixture, unique fresh post-tutorial profile,
empty mod root and shared immutable project-drive TAR cache. Native Campaign/core
Tournament 3 is used, with input/AI/spacing controlled. The root scene/profile
is untouched. The runner checks process exit and fresh result/log data.

The full-game run verifies defaults, an actual Options button click and updated
label/preference, native depth, solid meshes/normals and supported shader,
perspective camera/projection orientation, exclusive viewer/camera ownership,
paused frame/health/pose invariance, live off restore, resumed animation,
coincident-endpoint geometry, reset defaults and camera/mask teardown. PNGs show
the same paused pose before/after and the Settings switch; they are inspected
visually as well as checked structurally.

Initial harness failures were corrected (node-list owner and reveal-animation
layout lookup). Screenshot review found upside-down geometry that the initial
structural checks missed; the native reflection is now reused and orientation is
explicitly asserted. A global viewer-name lookup could include menu previews;
the camera now follows the actual active player's parent viewer, with isolation
assertions. Existing unrelated Search startup exceptions and title-preview
warnings remain in logs; logs are not claimed error-free.

All four managed projects compile against ignored Unity 6.6 reference remapping.
The 19 frame interpolation assertions pass after repairing stale recovered-name
references in the source audit to the existing GetStart/GetEnd/SetStart/SetEnd
names. The managed runner's default assembly location was populated from the
ignored remapped compile output and matching Unity managed references preloaded.
Those checks do not substitute for the graphics-enabled full-game acceptance.

Final full-game run passes 26 checks with 176 owned volumes and native body depth
-13.49503..40.14708 before presentation attenuation. The final spaced-fighter
before/after, resumed-animation and Settings PNGs were visually inspected and
copied into an immutable ignored review directory. Wiki types/build/search and
6249 links/assets across 60 pages pass (existing duplicate 404 warning).
