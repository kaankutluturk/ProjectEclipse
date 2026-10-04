# Actor Companions

Enable this mod, restart Eclipse, and play Zone 1 Tournament fight 3 (normal or
Eclipse mode). Its round HUD can summon an allied/opposing pair, move them,
damage the first actor, request its owned punch, defeat or dismiss it, and summon
a passive actor with a one-second simulation lifetime. Summoning is limited to
four owned actors across both main fighters and eight per fight.

The characters use the core Skeleton, Standard tactic and knives. They have
independent health and native AI. Lua reacquires references each tick and retains
only receipts/event sequences; native references expire with the callback.
The punch binary is the recovered core `high_punch.bytes`, also used by the
Active Strike example. It is an explicit move without selection events.

These companions do not change the two main fighters' victory/reward conditions.
Additional actor Lua behavior hosts, arbitrary rigs, raids and multiplayer are
not part of this example. See the wiki's Independent fighters reference.
