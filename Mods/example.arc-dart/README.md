# Arc Dart Trial

Enable this mod, Apply & Restart, then enter Act I Tournament stage 3 in normal
or Eclipse mode. Press **Throw Arc Dart** when the fighter is not attacking.
No equipped ranged weapon or magic is needed: the cast equips a resolved core
item on a native child actor without adding it to your inventory.

The HUD click stores intent. The next simulation tick queues the owned cast with
`fighter:play_move`; its receipt confirms startup before a three-second cooldown.
Startup does not prove the later projectile spawned or hit. At cast sample 5,
`timeline.projectile` creates `example.arc-dart.dart` with `SkeletonMissile` and
the core Monk shuriken in its Weapon slot. `start_move` starts the owned launch;
its animation-end action starts the owned flight.

Flight uses authored native velocity 18 and real `RANGED-Edge1`/`RANGED-Edge2`
contact attacks. Damage follows native attributes, defense and health handling,
with block bypass and critical hits disabled by the declaration. Lua never calls
`change_health`. Strike deletes the child; a miss deletes itself at finite
animation end. Native missile template cleanup also applies. There is no returned
live projectile handle.

`Flights` observes the owned flight animation starting. `Hits` counts all damage
attributed to this main fighter, including normal attacks; it is not a projectile
collision identity. Round-local counters/cooldown reset with the behavior. Pause
holds simulation and flight. Round/fight end closes the HUD and discards intent.
Native child lifetime belongs to its move graph and fight, not to the HUD.

The animation files are unchanged copies from the base game's
`Assets/Resources/gamedata/animations/binary/`: `ranged_light_player.bytes`,
`ranged_light_weapon.bytes` and `shuriken_fly.bytes`, with 67, 4 and 4 nodes.
Launch uses native animation-origin alignment to Parent; flight retains its own
missile point. This demonstrates compatible native exports, not original art or
general rig conversion.

Explicit playback bypasses input/AI conditions and cancel windows and can
interrupt other moves. This example declines activation during attacks. Define
your own costs, hurt/stun gates and interruption policy. Another mod's shared
playback request can reject a cast; failed receipts restore the button without
spending cooldown.

See [the projectile guide](https://dawc17.github.io/ProjectEclipse/guides/projectile-abilities/)
for the complete graph, safe changes and verification limits.

## Verification

The shipped script passed a Unity 6.6 full-game Campaign Tournament 3 run with
visible flight, native contact health loss, caster damage attribution, pause and
resume, strike deletion, miss expiry and cooldown (28 checks). The screenshot was
visually inspected. Input, AI and spacing were controlled; this is not physical
input, all-arena, reverse-facing, interrupted-cast or exported-player acceptance.
Managed Lua and native-method fixtures separately check registration, receipts,
round reset, root attribution and deferred animation event batching.
