# Owned moving sprite artwork

2026-10-04. This extends arena presentation toward the general creator platform;
it does not close E6/E8 or the Minecraft-style objective.

## Source and public contract

`MoonSharpScriptRuntimeArena.cs` adds `sf2.world.set_marker_rect` and
`sf2.world.set_marker_sprite`; the fighter binding adds `fighter:mark_sprite`.
All use `presentation.visuals`, existing script-owned opaque marker handles,
strict rectangle/color validation and typed sprite handles from that context.
Creation retains the active main-fighter/offline-round guard and rejects
registration, lifecycle begin/end and UI cleanup. Retained art can be updated
from ordinary runtime callbacks; its setters have no independent clock.

`ModArenaRuntime.cs` introduces optional artwork interfaces without breaking
existing region/marker backends. Rectangle and sprite creation share the same
16-per-script pool. Allocation failures dispose a partially returned native
marker even when the backend throws. Closed handles do not restart; malformed
updates do not reach the backend. Updates return boolean plus optional error.
`ModInstanceFighter` forwards the optional sprite interface and `Fight` uses its
existing arena eligibility, round lifetime and canonical render transform.

`ModArenaMarkerRenderer` uses one owned mesh/material with the shared 64-session
bound. It reads the native sprite vertices/UVs/triangle indices and maps the full
crop into the supplied positive-down arena rectangle. It supports 3..2048 native
vertices and uses the sprite texture, retaining imported asset identity/layout.
Changing a crop updates the existing mesh; geometry updates retain tint and
sprite. Sprite loading happens before replacing the prior artwork. There is no
Lua Unity object, texture-pixel access, physics collider or implicit damage.

## Complete example

Pulse Arena 1.1 and its mirrored editor starter include one original 256x1024 PNG
atlas and four cropped sprite descriptors. `BuildPulseArenaArtwork.py` produces
these files using Python's standard library; an independent fresh-directory run
reproduces all five shipped assets byte-for-byte. Lua switches frames every 12
simulation frames and sweeps the region 24 native units either side over its
six-second warning/active/recovery cycle. The marker and capsule sensor receive
the same updated rectangle; health changes remain explicit environmental policy.
Pause stops the simulation-driven art/motion/damage schedule. Round/fight cleanup
still removes the marker and HUD, with native lifetime cleanup as a fallback.

## Verification and limits

- **629 managed checks** execute production Lua/scopes/geometry plus the complete
  shipped sample using controlled native markers/rig/clock. New cases exercise
  typed sprite rejection, shared rectangle/sprite capacity, geometry/frame
  updates, failed updates retaining art, partial-allocation cleanup, callback
  escape, lifecycle/registration/UI cleanup guards and script disposal.
- **110 full-game Unity 6000.6.0f1 checks** pass with exit 0 in the marked isolated
  FighterPlaybackUnity project. Final log:
  `validation-384343099f424935a89cc3ebc91585b7.log`; the preceding native run also
  passes in `validation-f27aca4024934de082d2d6f86f9ee1d8.log`. The probe loads the
  installed mod/typed atlas, checks crop UV range, subsequent UV changes and
  movement without mesh/material replacement, capsule overlap/native health
  loss, pause-held vertices/UVs, recovery/recurrence, shared native allocation
  limits, surrender cleanup and suppression of post-end ticks. Warning/active
  PNGs are visually reviewed in ignored `Temp/ArenaSpriteReview-20261004`.
- Four managed assemblies compile with the existing ignored Windows-remapped
  projects; default Visual Studio SDK resolution remains unavailable. Wiki/API
  coverage includes **249** functions/aliases/callbacks. The public reference,
  examples, editor guide, schema/generated definitions and full starter ship
  together. Generation/check, **58** editor project tests, actual LuaLS and all
  **25** VS Code integration checks pass, including the new artwork calls.

The native scenario is controlled Campaign Tournament 3 with core fighters.
The rectangle remains axis-aligned; a sprite stretches to its supplied bounds.
Its transparent pixels are not collision geometry. The sensor samples current
capsules rather than swept motion and direct health loss does not imply native
attack/block/armor/reaction calculations. Actor hosts, rotation/depth/camera
authority, arbitrary arenas/rigs, exported platforms and aggregate performance
are not accepted by this follow-up. Lua UI callbacks may intentionally update
art while paused, so pause-following animation depends on using simulation ticks.
