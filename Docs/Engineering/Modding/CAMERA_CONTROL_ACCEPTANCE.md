# Owned fight camera control

Public Lua can frame an encounter through `fighter:acquire_camera(settings?)`,
then retain its opaque handle for `sf2.world.set_camera`, `is_camera_active` and
`release_camera`. All require the independent `presentation.camera` capability.
The public reference, Camera Lab example, mirrored editor starter, generated
contracts and editor guide ship in the same change.

## Source and contract

`Runtime/Modding/ModCameraRuntime.cs` defines immutable validated settings, an
optional fighter backend, one exclusive native fight slot and a script scope.
Settings accept finite absolute `center_x` in -10000..10000, positive-down pan
`offset_y` in -1000..1000 and absolute layer `zoom` in 0.25..4. Omitted center/zoom
use automatic native values; omitted pan is zero. Updates replace all fields.
Defaults restore native framing while retaining ownership. Conflicts identify
the current owner without displacing it. Failed partial allocations are disposed
once, including disposal exceptions and reentrant scope teardown. Old leases
cannot clear a successor. Script teardown releases retained controls.

`Modding/MoonSharpScriptRuntimeArena.cs` binds creation and retained operations.
Opaque handles are associated with their creating context. Invalid fields/types,
arity, capability, expired fighter callables, incomplete registration and UI
teardown reject before allocation. Fight/round begin/end and actor end reject
creation; initialized actor spawn and active combat callbacks are allowed.
Retained operations remain available to UI and cleanup callbacks. No saved-state
schema or public raw Unity-object access is introduced.

`Modding/FightCameraControl.cs` adds the shared slot to each native fight.
Acquisition requires a living canonical main fighter or an initialized actor
owned by this mod, a live script session and an unpaused offline non-raid round.
Lifetime follows the captured session/round and current canonical side or stable
actor record, preserving supported form changes. Actor liveness delegates to
existing production owner/body/TTL guards. Dead/retired owners, changed sessions,
ended rounds and fight replacement release control. `Fight.CloseModelTransitions`
also clears the slot before round/fight teardown.

`Render.UpdatePosition` consults only the current fight's bound renderer. Native
automatic camera state and zoom/shake effect timers continue. Manual center and
zoom enter the existing horizontal positioning and clamp calculation. Manual
centering skips native minimum-zoom player-focus correction, while viewport,
location-edge and forced-minimum-zoom rules remain. Native layer scale/position
and effective authored/background parallax run normally. The owned vertical pan
is then applied per layer. `ModCameraProjection` subtracts its actual rounded
contribution before the next draw, preventing drift on repeated/interpolated
presentations. Release consequently restores current native framing on the next
draw. Physics, fighter poses, collision, health and screen UI remain separate.

The camera render getter used by the new hook is descriptively named `GetRender`
with the exact `// best guess for name` declaration comment. Its former token
`KKFIJLOMOJI` simply returned the render field; only Camera's declaration/property
and its typed Fight callers change. The unrelated boolean method with the same
token in `ModelCollision` is preserved. No confirmed recovery mapping is added.

## Verification

- `Modding/TestCameraRuntime.ps1`: **193 actual Lua/scope/example checks**. Covers
  strict settings, replacement defaults, capability independence, malformed and
  foreign handles, callback/registration/UI-cleanup gates, shared cross-mod
  conflicts, expired references, release idempotency, backend rejection and
  partial allocation/teardown. A garbage-collection check proves released handles
  do not retain their native owner through the lifetime delegate. The shipped HUD's focus/sweep/native/release
  buttons, simulation-driven settings and HUD/round/fight cleanup run against a
  controlled native provider. Existing owned/prepared player and showcase checks
  run first.
- `Modding/TestCameraNativePolicies.ps1`: **6,034 checks**, including **3,000
  repeated rounded layer redraws**. Compiles the actual camera runtime and native
  helper with controlled fight eligibility, sessions, actor records and layer
  services. Checks canonical/actor handover, pause retention, owner/round/session
  expiry, fight render isolation, conflicts, explicit clear, weighted parallax,
  changed native baselines and destroyed layers. The high check count mostly
  repeats the drift assertion; it does not imply broad scene coverage.
- `CharacterForms/TestModelTransitionBoundary.ps1`: production queue/render
  boundary still passes and each close also clears camera ownership. Existing
  arena Lua/marker tests remain a regression gate for the shared binding file.
- `Modding/TestCameraUnity.ps1` passes **32 native full-game checks**, exit 0,
  using Unity 6000.6.0f1 in the marked isolated project and a fresh post-tutorial
  profile. Installed Camera Lab executes public Lua HUD focus/sweep/native/
  release paths. Real arena transforms prove zoom and vertical pan; pause holds
  the simulation and sweep, and 200 extra native presentation passes preserve
  the layer position and local fighter poses/health. Release works while paused
  and restores native zoom/pan. Default settings keep ownership; a second native
  fighter host cannot evict the Lua owner. HUD close releases the Lua handle;
  surrender clears an independently acquired unclaimed native lease, and ended
  rounds suppress ticks. Final accepted log:
  `validation-40924931a93b4ba094f12cb4ed9e5c32.log`. The earlier run
  `validation-be2085a89efd42a5a5e3276c94852a23.log` also passed. The final source
  includes released-owner reference cleanup, the descriptive camera getter and
  the shortened native-view label;
  its focus screenshot was visually reviewed.
- All four managed assemblies compile through the ignored remapped projects.
  The normal Visual Studio SDK resolver remains unavailable. Editor generation
  and check cover **254 public functions/aliases/callbacks**, **296 structures**;
  **60** project tests, actual LuaLS settings/handle checks and all **25** VS Code
  integration checks pass. Wiki build/types/search and link validation pass:
  **64 pages**, **7,022 local links/assets**, **64 current Git source links** and
  three repository-link regression tests. The existing duplicate-404 route
  warning remains.

## Limits

Native acceptance uses one core tournament arena/rig with controlled input and
AI. Main/actor form preservation, actor death/TTL and cross-mod conflict use
production-source/managed services rather than additional native form scenes.
Native second-host conflict uses the same enabled mod. All arenas, outfits,
camera shake/zoom-effect combinations, exported players, physical devices and
aggregate mod-pack performance remain open. Vertical pan can expose backdrop
edges; there is no vertical art clamp, arbitrary rotation, projection change or
free 3D camera. This advances encounter/world presentation without claiming
complete creator-platform or Minecraft-style modding acceptance. Experimental
fighter 3D rendering is outside this change.
