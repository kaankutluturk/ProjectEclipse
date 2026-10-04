# Experimental perspective fighter rendering

2026-10-04. Enable **Options > Display > 3D fighters (experimental)**. Default
off; the installation's PlayerPrefs (`Eclipse.ExperimentalFighter3D`) saves
immediately. Live fights update, and resetting rendering defaults clears it.
The shared Options UI serves startup and in-game Settings.

## Current source and approach

- `SF2DisplayFrameRate` owns preference/load/reset in Eclipse.Runtime.
- `ModelPresentation` retains the native Model. Project-owned
  `ProceduralFighterBody` reads standard Skeleton anchors through presentation
  interpolation. `SculptedFighterSkin` blends tapered elliptical sections into
  one signed-distance surface and extracts shared topology with marching
  tetrahedra. Shoulder and hip sections extend into the chest and pelvis;
  elbow/knee/ankle/foot transitions share that surface. Skull/jaw/brow/crown use
  a tapered profile, a small fused nose and a depth frame derived from native
  `NHeadF`; crown height follows `NTop` direction with bounded proportions.
  Hands use native knuckle anchors and flattened palm/knuckle profiles.
  A light alternating smoothing pass removes seed extraction ridges without
  repeated averaging shrinkage. Normals come from the actual deformed triangles,
  with adjacent normal smoothing across broad regions and a threshold that
  retains pronounced changes. Bent joints no longer use only rotated rest normals.
  Coordinates are relative to the native pivot during extraction, avoiding
  tiny-triangle precision loss at large arena coordinates. Section bounds cover
  both ends in both directions, including caps and blending margins.
- A connected seed surface binds each vertex to up to three nearby native
  sections. `FighterReferencePose` constructs that seed with separated arms and
  legs while retaining sampled torso/limb and head/hand landmark lengths and the native pivot. Mesh
  construction no longer fuses a guarding hand to the chest merely because the
  toggle was enabled during contact. `ProceduralFighterBody` binds in this
  reference, then deforms to the live pose in the same frame. The initial topology
  upload flag survives both steps; native pose data is only read. Subsequent
  interpolated poses deform positions and normals; topology
  uploads happen only when the rig changes. Unchanged poses reuse geometry while
  still updating tint/exposure. A disconnected initial surface keeps rebuilding
  until a connected seed is available. This is linear procedural skinning, not
  anatomical muscles, joint constraints or a general character importer.
  Torso/head, arms and legs use separate depth factors around native pivot
  (.85/.9, .65, .7). Standard male and BODY_WOMAN outlines are hidden when the
  skin is available. Missing-anchor rigs retain the panel/detail-stroke fallback.
- `MeshNode.AddTriangle` carries recovered figure names beside unchanged native
  node identities and triangle indices; ModelLoader supplies the names. The
  inferred method name has the required best-guess comment. `MeshNode.Render`
  retains interpolated XYZ only in the experimental presentation buffer; ordinary
  rendering still flattens Z. No simulation state or serialized assets change.
- `FighterVolume` groups triangles by recovered figure provenance, preserves
  shared native node identity, consistently winds faces and closes perimeter
  edges only. Cloth has two shared-edge subdivision levels, thin rolled hems,
  curved interiors and gentle procedural folds. Shell thickness and folds follow
  area-weighted native panel normals rather than the fixed world Z axis;
  distance to a hem uses the full native XYZ surface. A turned panel therefore
  keeps its curved thickness instead of becoming a depth slab.
  Independently animated coincident
  vertices are never welded by position. Recognized weapon/blade/helmet-name
  groups keep unsubdivided faces and separate sharp perimeter normals. Native
  equipment identifiers outside these groups remain a limitation: this is not
  complete semantic armour classification. Cloth/equipment depth (.75/.95) is relative to the native body pivot,
  with reduced soft thickness and fold amplitude. Fallback strokes are elliptical.
- The shared resource shader uses a matte near-black base, broad soft diffuse
  shading and faint directional arena-coloured edge light. Specular highlights
  and the blue rim are removed; native arena/perk tint has a small influence.
  `ReviewExposure`, default 1, is an acceptance-capture multiplier. The 4x/8x images
  uses the same geometry under brighter shading, not a Settings/Lua lighting API.
- `ExperimentalFighterCamera` belongs to the active player's native ViewerModel.
  Only that viewer's descendants acquire volumes; menu/shop/title previews remain
  excluded. FOV 32 perspective uses exactly the original camera rotation, matching
  framing at the central fighter plane. The original extra yaw/pitch are removed.
  Native InvertCamera reflection/culling preserves upright orientation.
- The owned depth-clearing camera draws layer 30 after the main camera. Its bit
  is removed from the main mask while active and restored on off/disable/destruction
  without replacing other bits. Fight teardown destroys the camera; owned meshes
  die with their volumes. Off restores original renderers. Arena/HUD projection,
  scenes, recovered meshes, sprite layouts and existing GUIDs are untouched.

## Scope and limits

This approximates recovered drawings. The body is a connected surface in the
accepted native poses, but facial/hand anatomy and shared proportions remain
simple. Standard rendering now requires the native top, face and knuckle anchors
alongside the prior torso/limb anchors; missing bindings use the recovered panel
and detail-stroke fallback. Reference proportions use sampled native lengths;
extreme bends, intersecting
limbs, large form changes and all outfits are not accepted by these checks.
Complex armour, helmets and skirt panels can still look angular or separate under
bright light. Outfit-specific shapes/depth and semantic equipment classification
need more work. Dark shading is the intended style; bright captures remain part
of geometry review.

There are no UVs/textures, custom skinned-character imports, 3D arenas, orbit
controls, gameplay depth movement, world shadows or public Lua camera API.
Native hit detection remains XY. Flat effects, foreground occlusion, floor decals,
trails and screen filters are not perspective-integrated. All outfits/forms/arenas,
mod visuals, physical controller input, exported players and sustained GPU/
performance acceptance remain open.

## Verification

`Tools/Tests/Presentation/TestExperimental3DUnity.ps1` uses Unity 6.6 in a marked
Temp full-game fixture, unique fresh post-tutorial profile, empty mod root and
shared immutable project-drive TAR cache. Campaign Tournament 3 has controlled
input/AI/spacing. Root scene/profile are untouched; fresh logs/results and process
exit are required.

The sculpted refinement passes **47 full-game checks**: defaults, actual Options
button/label/preference, supported shader/normals, original camera direction and
reflected projection, exclusive viewer ownership, paused frame/health/pose
invariance, live off restore, native HighKick playback and camera/mask teardown.
The native probe requires separated reference binding on both fighters and
forces a fresh topology build during a paused high kick. Native point positions
remain unchanged, and the regenerated skins still pass the closed connected
surface and lighting checks. Reference construction is verified independently
with guarding hands deliberately placed against the chest.
Both bodies have closed edge incidence (two faces per edge), one connected
component, noncollapsed triangles and finite unit lighting normals after the kick.
Six volumes remain (two bodies, two native panel meshes, two weapon strokes),
with no old body capsules covering the skin. A connected two-triangle cloth
fixture yields 50 vertices/288 indices without internal walls; a rigid fixture
yields 24 vertices/36 indices with sharp perimeter normals. A 90-degree turned
cloth fixture retains its local curvature/thickness and does not extrude along
world Z. Coincident fallback
geometry remains finite. Final native log: `Temp/FighterPlaybackUnity-d997a2c104cd4564b9767ae6eb4b68d8/validation-36e40eb4840b49d5ad80a4bd870d772f.log`.

A separate production-source managed runner,
`Tools/Tests/Presentation/TestSculptedSkinRuntime.ps1`, passes **96 checks**:
closed finite surfaces, positive/negative section direction and cap extents,
large-coordinate anchors, lighting normals, rigid rotation/translation and
unchanged animation topology, separated reference limbs, preserved section
lengths/large-coordinate root and unchanged input poses. Explicit elliptical
depth frames turn bound geometry and lighting normals without rebuilding the
skin; top, face and knuckle reference lengths are preserved. It uses Unity managed math, without a graphics
engine; the full-game run separately checks actual rendering.

Measured editor samples including final capture: approximately 107-116 ms for initial
surface extraction, smoothing and binding per fighter, then 7.2-7.7 ms per animated body
for deformation (excluding Unity mesh upload and other rendering work). The
initial toggle can hitch. Rebuilding actual deformed normals and smoothing their
lighting costs more than the earlier 3.8-4.0 ms orientation-only normal path.
These samples are not sustained CPU/GPU acceptance;
all equipment, simultaneous mod actors and exported players remain unmeasured.

Orthographic, default perspective, 4x/8x bright, native kick, 4x/8x bright kick and Settings
PNGs were visually inspected and saved to ignored
`Temp/Sculpted3DLandmarkReview-20261004/`. The bright images retain visible outfit defects;
no claim of complete clothing or anatomical reconstruction is made. The fixture
now disables both fighters' control before the initial timing gate, avoiding an
opponent attack being queued before the controlled screenshot setup.

All four managed projects compile through ignored Unity 6.6 Windows-reference
remapping. The wiki build/type/search/reference and internal-link checks also
pass. Native rendering acceptance is separate from managed compilation.
Unrelated Search startup exceptions and title preview warnings remain; logs are
not claimed error-free.

## Initial experiment history

The first version passed 26 checks using 176 volumes. It independently extruded
each triangle and used constant-width capsules, steel-grey/specular/blue-rim
shading and an oblique camera. Those construction/shading choices are superseded;
the earlier count is historical evidence, not the current renderer contract.
