# Return Dart Trial

Enable this complete folder with the Mod Manager and enter Act 1 Tournament 3
(normal or Eclipse). Use **Throw Return Dart** in the fight HUD.

The caster/launch/flight graph creates a native Monk shuriken even with no ranged
weapon equipped. The looping flight has zero forward velocity. Ordinary Lua
reacquires this mod's `fighter:projectiles()` each simulation tick, reads copied
`snapshot()` observations and queues `move_by()` displacement. After 28 guided
ticks it turns toward its recorded launch X and `remove()`s near that position.
Native contact still damages the enemy and deletes on strike. A 120-frame native
lifetime removes a stuck/missed projectile even if Lua stops guiding it.

This is a return to the launch position, not a hand catch; movement does not
supply swept collision or rotate the rig toward the trajectory. Lua stores copied
IDs/path state, not callback-scoped projectile references. Hit counts include
ordinary fighter damage. Round/fight endings close the HUD and retire the child.

The copied base animation binaries retain their original geometry. See the public
[projectile guide](https://dawc17.github.io/ProjectEclipse/guides/projectile-abilities/).
