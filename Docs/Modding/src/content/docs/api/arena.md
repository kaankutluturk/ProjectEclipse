---
title: Arena regions and artwork
description: Query native fighter geometry, move warning regions and animate owned sprite art in combat.
---

Use a rectangle to make an environmental hazard, capture zone or proximity gate.
Ordinary Lua decides what happens when a fighter enters it. The sensor and marker
share arena model coordinates: **positive Y points down**, X points right, and
Z is ignored. These are the coordinates returned by `fighter:snapshot()`.

The rectangle is a plain table with four required numbers:

| Field | Meaning and limit |
| --- | --- |
| `x`, `y` | Minimum X/Y corner, each finite in −10000..10000. |
| `width`, `height` | Positive size, each at most 4000. |

Unknown fields, missing values, numeric strings, infinity and NaN raise an error.
The shape is axis-aligned in arena coordinates; it inherits the arena's rendering
transform, including mirrored Y. It does not use screen pixels or UI units.

Queries test the current XY capsules in the native collision-edge list, with
each edge's radius and endpoint margins. That list is the one used by native hit
contact, including any equipment edges marked collidable. This is **geometry**:
attack intervals, invulnerability, block, shields and collision-disable flags do
not change the sensor. Boundary contact counts. There is no swept test between
poses, solid collision, actor spawning or automatic damage. Native rigs must have
1..512 collision edges. A callback may make 32 queries, shared by its fighter and
opponent; excess calls raise an error. These are per-callback bounds, not a total
frame-time guarantee for an arbitrary mod pack.

Markers draw a filled rectangle or a typed sprite over arena art using the native
render transform. They are presentation only, with no automatic physics, damage
or timer. Lua can move/resize a marker and switch sprites to animate it.
The color accepts `#RRGGBB` or `#RRGGBBAA`; the last two digits are opacity.
At most 16 markers per script and 64 across the session may be active. Rejection
does not evict another mod's art. Markers disappear by the next presentation frame
on round end, surrender, fight exit or arena destruction, and immediately on
script disposal. Form changes keep the marker in the arena's coordinate system.
Frame and geometry updates in `on_tick` follow simulation pause. Setters have no
clock of their own: UI callbacks may deliberately update art while paused.
Native sprite crops, vertices and UVs are preserved; 3..2048 vertices are
supported. The complete crop stretches to the requested rectangle, so use
matching width/height to preserve its aspect ratio. Pixel transparency and
sprite shape do not change the separate geometric hazard sensor.
Handles are opaque and transient: retain them in Lua memory, never saved state.

All creation and queries require a current main fighter in an active offline
round. Title sparring, local versus, native PVP and online raids are excluded;
mod-owned offline raids may use the same contract. Missing host/rig, inactive
round and budget rejection return an error string as described below. Calling an
expired fighter or passing invalid arguments raises a Lua error.

## fighter:overlaps_rect

Query the current native collision capsules against a rectangle.

**Signature:** `fighter:overlaps_rect(rectangle)`

**Returns:** `boolean, nil` for a valid query, or `nil, error` when geometry is
unavailable. `false` means the valid rig does not overlap; it is distinct from `nil`.

**When:** Inside a supported combat callback during an active offline round.
The callable fighter reference expires at callback exit.

**Requires:** No additional capability for the callback's own fighter.

```lua
local region = { x = -160, y = -420, width = 320, height = 440 }
local inside, failure = fighter:overlaps_rect(region)
if inside == nil then
    sf2.log.warn(failure)
elseif inside then
    sf2.log.info("Inside the column")
end
```

## fighter.opponent:overlaps_rect

Query the opposing main fighter's collision capsules in the same coordinate space.

**Signature:** `fighter.opponent:overlaps_rect(rectangle)`

**Returns:** `boolean, nil`, or `nil, error` when the native rig is unavailable.

**When:** In the current combat callback during an active offline round. Check
that `fighter.opponent` exists before using it; its callable reference expires
with the callback.

**Requires:** `combat.target`.

```lua
if fighter.opponent then
    local inside, failure = fighter.opponent:overlaps_rect {
        x = -160, y = -420, width = 320, height = 440,
    }
    if inside then sf2.log.info("Opponent is in the column")
    elseif inside == nil then sf2.log.warn(failure) end
end
```

## fighter:mark_rect

Create an owned filled rectangle in the arena. The art stays fixed as fighters
move unless you update it. Keep the marker for geometry, sprite, color updates
or removal.

**Signature:** `fighter:mark_rect(rectangle, color?)`

**Returns:** `ArenaMarkerHandle, nil` on creation, or `nil, error` for an unavailable
host/round/render transform/shader or exhausted marker budget. The default color
is translucent yellow `#ffcc3366`.

**When:** During an active simulation callback, such as `on_tick`. Fight/round
begin and end callbacks, registration and UI cleanup cannot create markers. The marker lasts
through the current round, including pause; the fighter callable expires at
callback exit. The handle may be kept until the marker closes.

**Requires:** `presentation.visuals`.

```lua
-- Inside on_tick; marker is a Lua variable outside the callback.
if not marker then
    marker = assert(fighter:mark_rect {
        x = -160, y = -420, width = 320, height = 440,
    })
end
```

## fighter:mark_sprite

Create owned sprite art in arena coordinates. Sprite and rectangle markers
share ownership, lifetime and the 16-per-script/64-session capacity.

**Signature:** `fighter:mark_sprite(sprite, rectangle, color?)`

**Returns:** `ArenaMarkerHandle, nil`, or `nil, error` for unavailable
host/round/transform/shader/sprite or exhausted capacity. The default tint is
opaque white `#ffffffff`. Invalid handles, fields or argument counts raise an error.

**When:** An active simulation callback, such as `on_tick`, after registration.
Creation is forbidden during fight/round begin/end and UI cleanup. The fighter
callable expires at callback exit; the marker lasts until removal or teardown.

**Requires:** `presentation.visuals`; a sprite handle obtained in this script
with `sf2.assets.sprite`, including assets visible through declared dependencies.

```lua
-- During registration: your descriptor references a PNG texture.
local warning = sf2.assets.sprite("sprites/warning")
-- Inside on_tick. Keep marker in temporary Lua memory, not saved state.
marker = assert(fighter:mark_sprite(warning, {
    x = -160, y = -420, width = 320, height = 440,
}, "#ffcc3399"))
```

Use the [sprite descriptor format](../sprites-and-textures/); a PNG texture ID
is not a sprite ID. Crops/UVs are respected, so frames can share an atlas without
displaying the whole image. Tint multiplies sprite pixels and opacity. This art
has no collider, automatic damage or animation clock.

## sf2.world.set_marker_rect

Move/resize existing rectangle or sprite art without replacing the marker,
resetting its lifetime or changing its tint/sprite. Update the rectangle passed
to `overlaps_rect` separately if a hazard should follow the art.

**Signature:** `sf2.world.set_marker_rect(marker, rectangle)`

**Returns:** `true, nil` on success, `false, nil` for a closed marker, or
`false, error` if the native backend cannot update it. Invalid rectangles,
forged/foreign handles and extra arguments raise a Lua error.

**When:** After creation in the owning script's runtime callbacks. Use `on_tick`
for movement that follows combat pause. A closed marker cannot restart.

**Requires:** `presentation.visuals`; a marker created by this script context.

```lua
-- region is ordinary Lua data shared by your art and sensor.
region.x = region.x + 2
local moved, failure = sf2.world.set_marker_rect(marker, region)
if not moved and failure then sf2.log.warn(failure) end
local inside = fighter:overlaps_rect(region)
```

## sf2.world.set_marker_sprite

Switch owned art to a typed sprite while retaining rectangle, tint and lifetime.
Works on sprite markers or converts a filled rectangle to sprite art. Lua owns
the frame sequence and timing; imported sprite layouts/textures are not modified.

**Signature:** `sf2.world.set_marker_sprite(marker, sprite)`

**Returns:** `true, nil` on success, `false, nil` for a closed marker, or
`false, error` for an unavailable/unsupported native sprite/backend. Invalid
sprite types, forged/foreign handles and extra arguments raise a Lua error.

**When:** After creation in the owning script's runtime callbacks. Frame changes
from `on_tick` follow combat pause; UI callbacks may change art while paused.
Failed sprite loading keeps the prior art and cannot reopen a closed marker.

**Requires:** `presentation.visuals`; marker and sprite handles from this script
context. Obtain sprite handles during registration.

```lua
-- Register descriptors with four crops in one atlas texture.
local frames = {
    sf2.assets.sprite("sprites/frame_1"), sf2.assets.sprite("sprites/frame_2"),
    sf2.assets.sprite("sprites/frame_3"), sf2.assets.sprite("sprites/frame_4"),
}
-- Inside on_tick; elapsed_frames is your Lua simulation counter.
local index = math.floor(elapsed_frames / 12) % #frames + 1
local changed, failure = sf2.world.set_marker_sprite(marker, frames[index])
if not changed and failure then sf2.log.warn(failure) end
```

## sf2.world.set_marker_color

Update owned warning art without replacing its geometry or extending its lifetime.

**Signature:** `sf2.world.set_marker_color(marker, color)`

**Returns:** `true` if the marker was active and updated, `false` if it has closed.
Invalid colors, forged/foreign handles and extra arguments raise an error.

**When:** After marker creation, from the owning script's runtime callbacks.
Updating a closed handle cannot restart it.

**Requires:** `presentation.visuals`; a marker created by this script context.

```lua
if marker then sf2.world.set_marker_color(marker, "#ff332299") end
```

## sf2.world.is_marker_active

Check marker lifetime. This does not test whether the camera currently sees it.

**Signature:** `sf2.world.is_marker_active(marker)`

**Returns:** `true` while the round-bound marker is active, including pause,
otherwise `false`. A forged or foreign handle raises an error.

**When:** After creation; safe for a retained closed handle in its owning script.

**Requires:** `presentation.visuals`; a marker created by this script context.

```lua
if marker and not sf2.world.is_marker_active(marker) then marker = nil end
```

## sf2.world.remove_marker

Release owned arena art. Repeated removal is safe.

**Signature:** `sf2.world.remove_marker(marker)`

**Returns:** `true` if the marker was active, otherwise `false`. A forged or
foreign handle raises an error. The visual is hidden immediately; native Unity
resources are destroyed using the normal deferred destruction path.

**When:** After creation, including round/fight cleanup callbacks. Automatic
round/script cleanup still runs if the mod forgets to remove a marker.

**Requires:** `presentation.visuals`; a marker created by this script context.

```lua
if marker then sf2.world.remove_marker(marker); marker = nil end
```

For a complete six-second warning/active/recovery cycle, see Pulse Arena in the
[examples](../../examples/). It uses `on_tick`, `overlaps_rect`, owned markers,
and direct `change_health` loss. Direct health loss bypasses attack/block/armor/
critical-hit calculations and does not produce a hit reaction; choose that policy
explicitly when building an environmental hazard.
