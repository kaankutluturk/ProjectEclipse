# Authored Fighter Lab

Enable this mod and **Apply & Restart**. In Campaign, open **Authored Fighter Lab**
on the Act I map. Choose the authored fighter or core comparison in the setup
screen. The Lua `on_prepare` callback resolves `player_character = selected`
through `sf2.modes.resolve`; the choice is saved with this encounter, reused
for a launch retry, and consumed when its result resolves. Back cancels the
pending choice. This owned fight's default is the authored character;
use your normal Punch control for its original strike. The fight declares
`player_character = character` through `sf2.fights.register`. This temporary
player setup does not change saved equipment and needs `content.register`.
The example also registers a repeatable Eclipse Mode entry and adds its HUD
to Act I Tournament 3 in either mode. Its HUD summons an independent pair, lets each fighter perform
the authored strike, resets pair spacing, toggles repeated strikes, and dismisses the pair. Their lifetime is 3,600 simulation frames (60 seconds at 60 frames/second),
paused with the fight. They have
their own health and target each other. Main fighters remain playable, so leave
room for the pair; the example does not reserve or freeze the arena.
Native reactions can move or turn a fighter. Use **Reset pair spacing** before a
controlled single strike. In repeated mode each behavior moves toward its target
in bounded three-unit steps between attacks, then starts the clip when in range.
Spacing resets use successive requests of at most 100 units per axis; the HUD
shows **spacing** until the pair is formed. Wait for **manual** before casting.

Choose **Try authored player (Punch)** to change your current fighter to this
character. Wait for **Player form: applied**, then use the normal Punch action
(your configured keyboard/gamepad binding or touch button). Only this character
gets the authored strike. Choose **Try core comparison form** to switch to a
registered kung-fu character using the core body. This comparison is not a
snapshot of your original loadout. Form changes are temporary combat operations;
they retain health percentage and input ownership without editing saved
equipment. Try either form button while the summoned pair is alive: both keep
their actor IDs, health, private behavior state, targets and remaining lifetime.
Repeated strikes continue through the owner swap. The separate **Left form:
core / authored** button changes that actor's own body while retaining its
identity and state. The HUD reacquires actor references each callback.
The owned lab uses the character chosen for its prepared encounter;
restart Tournament 3 to use your saved player's ordinary setup. A resolved owned
encounter opens the chooser again on the next entry. Failed
requests display their error in the HUD. This requires `combat.transform` in
addition to the actor demonstration's capabilities.

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

Use **Left form: core / authored** to switch the left companion between the
registered comparison and authored warriors. The actor keeps its identity,
private behavior, team, health and remaining lifetime. This stops repeated
strikes; return to the authored form before using its authored strike again.
Reset spacing and toggle repeated strikes to test continued native combat.
The core switch uses `actor:change_form`; the return is requested by the actor
behavior using `fighter:change_form`. Actor references require `combat.actors`
and form requests require `combat.transform`; changing
form removes owned projectiles from the retired body and starts a fresh native
AI controller rather than transferring its memory.
