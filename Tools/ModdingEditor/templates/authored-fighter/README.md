# Authored Fighter Lab

Enable this mod and **Apply & Restart**. Enter Act I Tournament 3 in Campaign
or Eclipse Mode. Its HUD summons an independent pair, lets each fighter perform
the authored strike, resets pair spacing, toggles repeated strikes, and dismisses the pair. They have
their own health and target each other. Main fighters remain playable, so leave
room for the pair; the example does not reserve or freeze the arena.
Native reactions can move or turn a fighter. Use **Reset pair spacing** before a
controlled single strike. In repeated mode each behavior moves toward its target
in bounded three-unit steps between attacks, then starts the clip when in range.
Spacing resets use successive requests of at most 100 units per axis; the HUD
shows **spacing** until the pair is formed. Wait for **manual** before casting.

The body contains the canonical 67-point binding layout with authored capsule
and torso geometry. Six weighted helper points form a connected four-triangle
sash. It appends after equipment without changing the animation point count.
The 61-sample strike is original procedural point animation, written through the
existing CharacterPipeline baker. Core walking/idle/hit moves still provide the
ordinary fighter behavior. This is a compatible rig example, not an arbitrary
mesh/FBX importer or a new humanoid skeleton.

## Rebuild and edit

From the repository root, choose a fresh output directory:

```powershell
python Tools/Animation/BuildAuthoredFighterExample.py --body Mods/example.authored-fighter/assets/models/body.xml --output Temp/MyAuthoredFighter
python Tools/Animation/CharacterPipeline.py validate --rig Temp/MyAuthoredFighter/body.xml --skin Temp/MyAuthoredFighter/sash.xml --animation Temp/MyAuthoredFighter/strike.bytes
```

The output includes model XML, `strike.frames.json`, a native binary, its rig
fingerprint and a local point preview. Edit the generator for this sample, or use
the wiki's Blender/Gymnast workflow to author your own source. Preserve point
order, re-export and copy the matching model/clip/sidecar together. Attack frames
18–30, edges and damage are ordinary move fields in `scripts/main.lua`; procedural
decisions and cooldowns are ordinary Lua behavior code. No operation DSL is used.

The experimental 3D option is an approximation of compatible rigs; its generated
anatomy does not preserve every custom silhouette detail. Test authored visuals
and equipment in both rendering modes before publishing your own character.
