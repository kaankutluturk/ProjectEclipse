# Pulse Arena

An equipment-free timed hazard for Act I Tournament 3, normal and Eclipse. Install
this folder in `Mods/` alongside the core package, then enter that encounter.

The column is placed halfway between the fighters at the first tick, then sweeps 24 native units to either side. The marker geometry and capsule
sensor use the same updated rectangle. It warns in yellow for two simulation seconds, becomes red
for two seconds, then disappears for two seconds. Every half second of the active
phase, either fighter whose native collision capsules touch the rectangle loses
0.025 normalized health. Leave the column to avoid it. Combat pause freezes the
schedule; round/fight exit removes the marker and HUD. The original four-frame sprite atlas animates every 12 simulation frames;
pause freezes both motion and frame selection.

`fighter:mark_sprite` creates arena-local sprite warning art; `overlaps_rect` queries the
same rectangle against actual collision-rig edges with margins and radii.
`sf2.world.set_marker_rect` moves/resizes the existing marker without respawning it;
`set_marker_sprite` swaps cropped frames from one shared atlas. `set_marker_color`
and `remove_marker` change owned presentation. No Unity objects or texture pixels
are exposed to Lua. Color/opacity multiply the image's own pixels.
Ordinary Lua and `on_tick` own the phase, cooldown and contact schedule.

Damage uses the existing `change_health` path. It is direct environmental health
loss, without attack/block/armor/critical-hit rules, shields or hit reactions. The
sensor ignores attack intervals and invulnerability: it observes geometry only.
It samples the current pose in XY, without swept movement, solid collision,
projectiles or new physical actors. Markers are round-bound, transient and capped.

## Edit the art

Replace the sprite descriptors/PNG with your own warning art. Crops use pixels
from the texture's bottom left; the displayed rectangle stretches the complete
sprite crop to the requested width/height. Transparency does not change the
hazard sensor. The original procedural artwork can be rebuilt with:

```powershell
python Tools/Animation/BuildPulseArenaArtwork.py --output Temp/PulseArtwork/assets
```

Copy the generated `sprites/` and `textures/` together into this mod's `assets/`.
This is a Lua-driven presentation example, not an automatic damage/physics system.
