---
title: Camera control
description: Own the fight camera, frame encounters and animate pan or zoom from Lua.
---

A camera handle gives one script exclusive control of fight framing. It changes
the native arena presentation while fighter geometry, collision, damage and HUD
coordinates keep their ordinary behavior. Arena layers keep their authored
horizontal parallax; vertical pan uses the same effective layer factors.

Add `"presentation.camera"` to your manifest's `capabilities`. This is separate
from `presentation.visuals`. Acquisition needs a living main fighter or an
initialized independent actor belonging to this mod, during an active offline
non-raid round. Title sparring, local versus and native PVP are excluded.

## Settings and native limits

All fields are optional. Settings replace the previous table completely.

| Field | Type, default and limits |
| --- | --- |
| `center_x` | Finite number in −10000..10000. Absolute arena X to center. Omit for automatic native duel following. |
| `offset_y` | Finite number in −1000..1000; default `0`. Positive-down camera pan in arena units. Negative values raise the camera's framing. |
| `zoom` | Finite number in 0.25..4. Absolute native game-layer scale, rather than a multiplier. Omit for automatic/native-effect zoom. |

Unknown fields, numeric strings, NaN and infinity raise a Lua error. `{}` or `nil`
restores native follow/zoom and zero vertical pan while **keeping ownership**.
Release the handle to let another script acquire the camera.

Native viewport and location minimum zoom and horizontal arena-edge limits still
apply, so requested framing can be clamped. A native rule that forces minimum
zoom takes precedence. Vertical pan has no art-boundary clamp and can reveal the
edges of a backdrop; choose offsets suitable for your location. Native shake and
effect timers continue. Releasing control draws the current automatic camera
state on the next presentation pass; it does not rewind a saved camera pose.

There is one owner per fight, across scripts and mods. A conflicting acquisition
returns an error instead of evicting the owner. A handle belongs only to the
script that created it. It is opaque and transient: keep it in Lua memory, never
saved state. Ownership expires on owner death/retirement, round or fight exit,
script-session replacement or script disposal. Supported main-fighter and actor
form changes preserve ownership. A stale handle cannot release a successor.

`on_tick` animation follows simulation pause. Retained handle operations are
also allowed from UI callbacks and cleanup, so a button can release control
while paused. Setters have no clock: a UI callback can deliberately update the
view during pause. New acquisition is rejected while paused and during cleanup.

## fighter:acquire_camera

**Signature:** `fighter:acquire_camera(settings?)`

**Returns:** `camera, nil`, or `nil, error` when the host is unsupported, the
fighter/round is inactive, another owner holds the camera, or this script already
has an active handle. Invalid arguments or expired callback references raise an
error. No competing owner is displaced.

**When:** In a supported active combat callback after registration has finished,
including an initialized actor's `on_actor_spawn` or `on_tick`. Fight/round begin
and end, `on_actor_end`, UI cleanup and paused acquisition are rejected. The
fighter reference itself expires at callback exit; the returned camera can persist.

**Requires:** `presentation.camera`; a supported living fighter and settings
matching the table above.

```lua
local camera
-- A fragment inside a registered behavior:
on_tick = function(_, fighter)
    if not camera or not sf2.world.is_camera_active(camera) then
        local view = fighter:snapshot()
        if not view then return end
        local failure
        camera, failure = fighter:acquire_camera {
            center_x = view.self.position.x, offset_y = -40, zoom = 1.3,
        }
        if failure then sf2.log.warn(failure) end
    end
end
```

## sf2.world.set_camera

**Signature:** `sf2.world.set_camera(camera, settings)`

**Returns:** `true, nil` when changed, or `false, error` after release/expiry or
backend rejection. Invalid settings or a foreign/forged handle raise an error.
The settings replace every previous field; omitted values return to their defaults.

**When:** After acquisition, including later combat or UI callbacks. The next
presentation pass applies the requested settings with native clamps.

**Requires:** `presentation.camera`; a camera created by this script.

```lua
-- camera was acquired earlier in this script.
local changed, failure = sf2.world.set_camera(camera, {
    center_x = 600, offset_y = -20, zoom = 1.2,
})
if failure then sf2.log.warn(failure) end
-- Return to native framing while still owning the camera:
sf2.world.set_camera(camera, {})
```

## sf2.world.is_camera_active

**Signature:** `sf2.world.is_camera_active(camera)`

**Returns:** `true` while ownership is live; `false` after release or expiry.
A foreign or forged handle raises an error.

**When:** After acquisition, including paused UI callbacks and cleanup.

**Requires:** `presentation.camera`; a camera created by this script.

```lua
if camera and not sf2.world.is_camera_active(camera) then
    camera = nil -- reacquire from a later active fighter callback if desired
end
```

## sf2.world.release_camera

**Signature:** `sf2.world.release_camera(camera)`

**Returns:** `true` on this handle's first release, even if its native ownership
has expired; `false` on subsequent releases. A foreign or forged handle raises an
error. It never clears a different script's current ownership.

**When:** After acquisition, including UI teardown and round/fight end cleanup.

**Requires:** `presentation.camera`; a camera created by this script.

```lua
-- Keep this cleanup function in the same script that acquired camera.
local function release_view()
    if camera then sf2.world.release_camera(camera) end
    camera = nil
end
-- Use it from on_round_end, on_fight_end or your HUD's on_close.
```

The complete [Camera Lab example](../../examples/#camera-lab) demonstrates all
four operations, simulation-timed motion and cleanup.
