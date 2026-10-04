# Scripted Burst Trial

Enable this mod, choose **Apply & Restart**, and enter Act I Tournament 3
(normal or Eclipse). **Spawn three darts** queues three real native weapon
children without starting a caster move. Ordinary Lua computes their offsets,
records applied receipt IDs, reacquires live handles each tick, and guides them
in the direction chosen when firing. Contact uses the declared native attack;
only this mod's copied attack sources count as hits. Misses expire after 120
simulation frames. The button has a 180-frame cooldown, and pause freezes both.

The declarative projectile uses a four-node `SkeletonMissile`, the core Monk
shuriken item and the shipped original `shuriken_fly` animation binary. Each
spawn can independently fail; this example reports failed receipts and retains
the cooldown. It intentionally supports partial bursts. Offsets and motion use
world axes, with positive Y pointing down. This is native child-weapon behavior,
without independent health, AI, teams or solid/swept physics.

Capabilities: `content.register`, `content.patch`, `combat.projectiles`,
`ui.create`. The core dependency supplies the equipment and move templates.
No `combat.animation` is needed because no main-fighter move is requested.
Disable other projectile trial mods when inspecting its HUD.

[Projectile API](https://dawc17.github.io/ProjectEclipse/api/projectiles/)
and [ability guide](https://dawc17.github.io/ProjectEclipse/guides/projectile-abilities/).
The native acceptance fixture separately verifies direct three-child creation,
receipts, placement, rendering, native contact and teardown with controlled
AI/spacing. The actual example also runs its Lua/HUD, receipt trajectory, partial bursts,
cooldown and round reset in a managed fixture with controlled native models. It
is checked by the mod validator and actual LuaLS. These checks are not an
uncontrolled full-game playtest
of this HUD/example or every loadout and mode.
