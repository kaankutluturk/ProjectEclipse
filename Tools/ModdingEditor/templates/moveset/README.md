# Moveset Starter

A data-only mod: it has no Lua script. `movesets/moveset.json` makes `HighKick` 20% faster
and a little weaker for every fighter, and gives ninja swords their own faster copy of
`KatanaHeavySlash` while katanas keep the original.

Copy this folder into the game's `Mods` folder, enable it, and test in training. Every
`expected` value is the base game's current value; the mod refuses to load if one is wrong,
which protects against game updates and typos. Hover a move name in a Lua
`sf2.moves.patch` or open it in the Moveset Lab to read current values.

The `core` dependency is only needed when a fork names a `core:` item; it is included so
you can add one. See the [moveset file reference](https://dawc17.github.io/ProjectEclipse/api/movesets/).
