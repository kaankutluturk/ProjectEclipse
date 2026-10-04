# Ranged Companion Duel

Install this folder in your Eclipse mods directory and enable it. Enter Act I
tournament fight 3 (normal or Eclipse mode), then press **Summon ranged pair**.
The two independent fighters target each other and create native shuriken children
from their own Lua behavior, without playing a throwing move on either main fighter.
**Hold fire** stops new volleys; existing darts keep moving. **Dismiss pair** removes
the actors and their projectiles. Normal pause freezes simulation.

Each actor has private typed cooldown/shot/hit state. `fighter:spawn_projectile`
returns a queued receipt; a later callback checks the applied child ID.
`fighter:projectiles()` returns only this mod's initialized children whose root is
this actor. Lua guides their flight; native geometry, hostility and collision resolve
contact. Attack observations carry both projectile and actor identity.

Definitions/assets are owned by this mod. The bundled unchanged core shuriken flight
binary and `SkeletonMissile`/core ranged item provide the native rig. Retirement
cancels pending births and removes live children; references expire after callbacks.
This example is a foundation for ranged allies, enemies and turrets, not acceptance
of arbitrary rigs, every stance/arena, exported players, raids or multiplayer.
