# Experimental perspective fighter rendering

2026-10-04. Enable **Options > Display > 3D fighters (experimental)**. Default
off; the installation's PlayerPrefs (`Eclipse.ExperimentalFighter3D`) saves
immediately. Live fights update, and resetting rendering defaults clears it.
The shared Options UI serves startup and in-game Settings.

## Current source and approach

- `SF2DisplayFrameRate` owns preference/load/reset in Eclipse.Runtime.
- `ModelPresentation` retains the native Model. Project-owned
  `ProceduralFighterBody` reads standard Skeleton anchors through presentation
  interpolation and builds tapered elliptical lofts: dedicated torso/pelvis,
  head, hand and foot profiles; continuous elbow and knee/ankle/heel/forefoot
  surfaces. Hermite paths and varying sections replace uniform stroke capsules.
  Shared vertices smooth normals within each section. Torso/head, arms and legs
  use separate depth factors around native pivot (.85/.9, .65, .7). Standard male
  and BODY_WOMAN outlines are hidden when this skin is available. Shoulder/hip
  attachments remain inset intersections, not a globally manifold anatomical
  skin. Missing-anchor rigs retain the panel/detail-stroke fallback.
- `MeshNode.AddTriangle` carries recovered figure names beside unchanged native
  node identities and triangle indices; ModelLoader supplies the names. The
  inferred method name has the required best-guess comment. `MeshNode.Render`
  retains interpolated XYZ only in the experimental presentation buffer; ordinary
  rendering still flattens Z. No simulation state or serialized assets change.
- `FighterVolume` groups triangles by recovered figure provenance, preserves
  shared native node identity, consistently winds faces and closes perimeter
  edges only. Cloth has two shared-edge subdivision levels, thin rolled hems,
  curved interiors and broad procedural folds. Independently animated coincident
  vertices are never welded by position. Recognized weapon/blade/helmet-name
  groups keep unsubdivided faces and separate sharp perimeter normals. Native
  equipment identifiers outside these groups remain a limitation: this is not
  complete semantic armour classification. Cloth/equipment depth (.65/.95)
  replaces blanket .35 attenuation. Fallback strokes are elliptical.
- The shared resource shader uses a matte near-black base, broad soft diffuse
  shading and faint directional arena-coloured edge light. Specular highlights
  and the blue rim are removed; native arena/perk tint has a small influence.
  `ReviewExposure`, default 1, is an acceptance-capture multiplier. The 4x image
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

This approximates recovered drawings. Head/hand anatomy is simple; shoulder/hip
joins can intersect. Shared standard body proportions do not reconstruct every
fighter's anatomy. Complex armour, helmets and skirt panels can still look
angular, noisy or separate under bright light; outfit-specific shapes/depth and
semantic equipment classification need more work. Dark shading is the intended
style, not evidence that these remaining geometry defects are solved.

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

Final refinement passes **34 checks**: defaults, actual Options button/label/
preference, supported shader/normals, original camera direction and reflected
projection, exclusive viewer ownership, paused frame/health/pose invariance, live
off restore, resumed animation and camera/mask teardown. Both standard bodies
have over 3000 vertices. Six volumes remain (two bodies, two native panel meshes,
two weapon strokes), with no old body capsules covering the new skin. A connected
two-triangle cloth fixture yields 50 vertices/288 indices without internal walls;
a rigid fixture yields 24 vertices/36 indices with sharp perimeter normals.
Coincident fallback geometry remains finite. Native source depth is
-13.49503..40.14708 before presentation adjustment.

Orthographic, default perspective, 4x bright, resumed animation and Settings PNGs
were visually inspected and saved to ignored `Temp/Procedural3DReview-20261004/`.
Review corrected a nonexistent hand anchor, disconnected ankles/feet and female
figure prefixes initially leaving old body capsules visible. Complex outfit
defects remain visible in the bright capture and are not claimed solved.

All four managed projects compile through ignored Unity 6.6 reference remapping;
19 frame-interpolation assertions pass with Unity managed references preloaded
and remapped assemblies in the runner's ignored directory. Wiki build/types/search
and links are recorded in the creator work log. Native rendering acceptance is
separate from managed compilation. Unrelated Search startup exceptions and title
preview warnings remain; logs are not claimed error-free.

## Initial experiment history

The first version passed 26 checks using 176 volumes. It independently extruded
each triangle and used constant-width capsules, steel-grey/specular/blue-rim
shading and an oblique camera. Those construction/shading choices are superseded;
the earlier count is historical evidence, not the current renderer contract.
