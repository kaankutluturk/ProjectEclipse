---
title: Moveset files
description: Tune and fork native moves with a movesets/*.json data file, without Lua.
---

A **moveset file** describes changes to the base game's moves as data. It can do
everything [`sf2.moves.patch`](../moves-and-tactics/#sf2movespatch) and
[`sf2.moves.fork`](../moves-and-tactics/#sf2movesfork) do (speed, damage, attacking
edges, intervals, hit reactions, priority, animation and per-subtype or per-weapon copies),
but needs no script. The in-game [Moveset Lab](../../guides/moveset-editor/) writes these
files, and you can also write or edit them by hand.

## Where the file goes

Put one or more `.json` files in a `movesets/` folder next to `mod.toml`:

```text
yourname.faster-kicks/
  mod.toml
  movesets/
    moveset.json
```

Files load in name order, before the mod's Lua entrypoint (if any), in the same
registration. A mod with only data files can omit `entrypoint`; see
[data-only mods](../../guides/manifest/#data-only-mods).

**Requires:** `content.patch` in `capabilities` whenever a file lists moves or forks, and a
dependency on `core` (or the item's owner) when a fork names an item.

## A complete example

```json
{
  "schema": 1,
  "kind": "eclipse.moveset",
  "name": "Faster kicks",
  "moves": [
    {
      "move": "HighKick",
      "note": "Quicker but weaker",
      "playback_rate": { "expected": 1.0, "value": 1.25 },
      "intervals": [
        { "select": { "name": "Uninterrupt", "start": 0, "end": 15 }, "end": 12 }
      ],
      "attacks": [
        {
          "id": 0,
          "damage": { "expected": 0.12, "value": 0.1 },
          "edges": {
            "expected": ["EThigh_2", "ECalf_2", "EInstep_2", "EToe_2", "EFoot_2"],
            "value": ["EInstep_2", "EFoot_2"]
          }
        }
      ]
    }
  ],
  "forks": [
    {
      "id": "KatanaHeavySlash_Ninja",
      "move": "KatanaHeavySlash",
      "subtype": "NinjaSword",
      "playback_rate": { "expected": 1.0, "value": 1.2 }
    }
  ]
}
```

## Top-level fields

| Field | Required | Meaning |
| --- | --- | --- |
| `schema` | yes | Format version. Must be `1`. |
| `kind` | yes | Must be `"eclipse.moveset"`. |
| `name` | no | A label for people reading the file. |
| `moves` | no | Array of [move edits](#move-edits), up to 2048. |
| `forks` | no | Array of [forks](#forks), up to 512. |

The file must be UTF-8, at most 2 MiB, and strict JSON: no comments, no trailing commas,
no duplicate field names. Unknown fields are errors, so a typo is reported instead of
silently ignored. An error stops the whole mod from loading and is listed in the mod
menu's **Details** with the file and field, such as
`movesets/moveset.json.moves[0].attacks[0].damage.expected must be a number.`

## Move edits

Each entry changes one native move. `move` is required; every other field is optional, but
an entry must change something. Most fields are **guarded**: they give the value the move
has now in `expected` and the new value in `value`. If the base game's value differs (for
example after a game update, or a typo), the mod fails to load instead of applying a wrong
edit.

| Field | Meaning |
| --- | --- |
| `move` | Exact, case-sensitive native move name, such as `"HighKick"`. |
| `note` | Free text for people, up to 2000 characters. Kept when the Moveset Lab saves. |
| `disable` | `true` keeps the move loaded but makes it unselectable. Must be the only change in its entry. |
| `priority` | `{ "expected", "value" }`, integers 0–100000. Higher-priority moves win when several match the same input. |
| `playback_rate` | `{ "expected", "value" }`, numbers 0.5–2.0. `1.25` is 25% faster. `expected` is `1.0` for a native move. At most `MidFrames + 1`; not for looped or physics moves. See [playback rate](../moves-and-tactics/#playback-rate). |
| `animation` | `{ "expected", "value" }`. `expected` is the current `.bytes` file. `value` is another native clip's filename, or `{ "asset": "animations/my_clip" }` for a clip in your mod's `assets/` folder. |
| `input` | `{ "expected", "value" }`, the move's key [input](#input): `expected` is the base game's input, `value` the new one. |
| `sound_frame` | `{ "name", "expected", "value" }`: move one directly scheduled sound to another frame. |
| `intervals` | Array of [interval edits](#intervals), up to 64. |
| `attacks` | Array of [attack edits](#attacks), up to 32. |

Only one mod can patch a given move. A second mod (or Lua patch) on the same move fails
to load; the VS Code extension [warns about this](../../guides/vscode/#find-conflicts-with-other-mods).

### Input

A move's input is the keys that start it. It is a list of *alternatives*; pressing any one
of them starts the move (for example a dash that starts on Up-Back, or on Back tapped
twice). Each alternative is a *chord*: the keys pressed together, 1–14 of them. A key is
written as its name, which means it is tapped, or as `{ "key": ..., "press": ... }` with
`press` one of `"Tap"`, `"Hold"` (keep it held) or `"Release"` (let it go):

```json
"input": {
  "expected": [ [ "Punch", { "key": "Forward", "press": "Hold" } ] ],
  "value":    [ [ "Kick",  { "key": "Back", "press": "Hold" } ] ]
}
```

- Key names: `Punch`, `Kick`, `Ranged`, `Magic`, `RaidCharge`, `Super`, and the directions
  `Forward`, `Back`, `Up`, `Down`, `Up-Forward`, `Up-Back`, `Down-Forward`, `Down-Back`.
  Directions are for a fighter facing right; the game mirrors them when it faces left.
- The same key tapped twice in one chord (`[ "Punch", "Punch" ]`) is a two-tap sequence.
- Up to 8 alternatives. `[]` is no input: a move without one is started by the game itself
  (hit reactions, combo steps, AI actions). Giving such a move an input, or removing a
  move's input, changes when it can happen; test it in a fight.
- A single tapped key can still be written as a plain string, as in older files:
  `"input": { "expected": "Kick", "value": "Punch" }`.
- The edit replaces the move's whole input. It works on moves whose keys form one chord,
  or one set of alternatives. Moves whose keys are mixed into other conditions report
  that their input is not editable; edit those in Lua.
- What a move must follow (a combo step, being in the air) is not part of its input and
  stays as it is.

### Intervals

An interval is a frame window with a meaning: `Uninterrupt` (the move cannot be cancelled),
`Block`, `Invulnerable`, `Throwable` and so on. Each entry is one of three shapes:

```json
{ "select": { "name": "Uninterrupt", "start": 0, "end": 15 }, "start": 2, "end": 12 }
{ "select": { "type": "Block", "start": 16 }, "remove": true }
{ "add": { "type": "Invulnerable", "name": "Dodge", "start": 0, "end": 3 } }
```

`select` names one existing interval exactly by `type`, `name`, `start` (default `0`) and
`end`. **Leave out `end` to select an open-ended interval**, one that lasts until the move
ends. The selector must match exactly one non-attack interval. `add` takes a `type`
(`Block`, `Invulnerable`, `Invisible`, `Throwable` or omitted), a `name` (required when
there is no type, and new to the move), `start`, and an optional `end`. The details match
[interval edits](../moves-and-tactics/#interval-edits) in Lua.

### Attacks

Each attack interval has an `id` (usually 0 for a move's first attack). An attack edit lists
the fields to change, each guarded:

| Field | `expected` / `value` |
| --- | --- |
| `start`, `end` | Attack frames, integers. |
| `damage` | Base damage 0–16. |
| `damage_terms` | Object of term type to shift, such as `{ "UnarmedDamage": 0 }`. The new terms are 1–4 of `UnarmedDamage`, `WeaponDamage`, `RangedDamage`, `MagicDamage`, and may not add or remove `RangedDamage`, `MagicDamage` or `RaidChargeDamage`. |
| `edges` | Array of attacking edge names: the parts of the body or weapon that hit. 1–64 distinct names. |
| `impulse` | `[x, y, z]` push applied on hit. |
| `hit` | Hit reaction name, such as `"High"` or `"MiddleShortPlus"`. |

See [attack edits](../moves-and-tactics/#attack-edits) for the exact rules.

## Forks

A fork copies a move for one weapon subtype or one weapon, leaving the original for
everyone else. It takes the fields of a move edit (applied to the copy; all optional for a
fork) plus:

| Field | Required | Meaning |
| --- | --- | --- |
| `id` | yes | Local name of the copy; the runtime name is `<mod id>.<id>`. |
| `move` | yes | The move to copy: a native move, or an earlier fork's runtime name. |
| `subtype` | one of these | Weapon subtype that gets the copy. Removed from the original's lock group. |
| `item` | one of these | Qualified item ID, such as `"core:items/weapon/weapon_golden_katana"`, that gets the copy. The original stops matching that item. |
| `add` | no, default `false` | `true` makes a [new move](#new-moves) instead of a replacement. |

```json
{ "id": "HighKick_Golden", "move": "HighKick", "item": "core:items/weapon/weapon_golden_katana",
  "attacks": [ { "id": 0, "damage": { "expected": 0.12, "value": 0.18 } } ] }
```

Guards in a fork compare against the copied source move. Read
[`sf2.moves.fork`](../moves-and-tactics/#sf2movesfork) for how copies keep combos and
which conflicts are rejected.

### New moves

With `"add": true` the copy is a new move beside the original rather than a replacement
for some fighters. The original keeps all its fighters and is not changed. The new move:

- starts as an exact copy of `move`: its animation, attacks, frame windows, sounds and
  the conditions it must follow;
- goes to the fighters named by `subtype` or `item` (at most one of them), or, with
  neither, to the same fighters as the original;
- shares the original's templates, so moves that follow a template (for example a
  `"Central"` step) can follow it too, but it does not answer to the original's own name:
  combos written for that exact move do not continue from the new one;
- should get its own [`input`](#input); with the original's input both moves compete and
  the higher [`priority`](#move-edits) wins.

```json
{ "id": "rising_slash", "move": "KatanaUpperSlash", "subtype": "Katana", "add": true,
  "input": { "expected": [ [ "Punch", { "key": "Up", "press": "Hold" } ] ],
             "value":    [ [ "Kick",  { "key": "Up", "press": "Hold" } ] ] },
  "priority": { "expected": 120, "value": 122 } }
```

New moves are copies of existing moves: they reuse a game clip, or a clip swapped in with
[`animation`](#move-edits). For a move built from your own data, use
[`sf2.moves.register`](../moves-and-tactics/#sf2movesregister).

## Finding the current values

Every `expected` value must match the base game exactly. To read a move's current
priority, intervals, attack IDs, damage and edges:

- open the move in the Moveset Lab;
- hover the move name inside a Lua `sf2.moves.patch` in VS Code; or
- run `python Tools/Audits/QueryMoves.py show HighKick` from an Eclipse source checkout.

## Writing by hand

The Moveset Lab rewrites files in a canonical layout: two-space indentation, a fixed field
order, numbers such as `1.0` for decimal fields, and one line per array of edge names.
Saving a file you wrote by hand may reorder fields; keep notes in `note`, not in
comments. Live testing works too: the fight debug menu's
[move patch reload](../../guides/runtime-diagnostics/#tune-move-patches-without-restarting)
re-reads edited moveset files. Patches apply at once; forks apply to fighters built
after the reload, such as the next fight.

## Limits

Moveset files edit and copy existing moves, and add new moves copied from them; they do
not define moves from scratch or new transitions (use
[`sf2.moves.register`](../moves-and-tactics/#sf2movesregister) for that). Edits
are checked against the base game when the mod loads. They are not checked against
gameplay, so test changed hitboxes, timings and speeds in a fight. Online versus refuses
to start while moveset content is active.
