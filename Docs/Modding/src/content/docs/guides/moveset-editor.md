---
title: Moveset Lab
description: Tune moves in game — speed, damage, hit frames, hitboxes and priority — and save them as a mod without writing Lua.
---

The **Moveset Lab** is an in-game editor for existing moves. You pick a weapon subtype
(or one weapon), watch a move on a live fighter with its hitting parts drawn in red,
change its values, and save the result as an ordinary mod. You don't need Lua or a
text editor. The Lab writes a [moveset file](../../api/movesets/) that you can share,
edit by hand, or combine with other mods.

The Lab tunes moves that already exist. It cannot create brand-new moves or animations
yet; use [`sf2.moves.register`](../../api/moves-and-tactics/#sf2movesregister) for those.

## Open the Lab

Either:

- open **Multiplayer** on the title screen and choose **MOVESET LAB** (or press **M**); or
- open **Mods** on the title screen and choose **Moveset Lab**.

## The screen

| Area | What it does |
| --- | --- |
| Left, first row | The **mod** you are editing. `<` `>` switch between the data-only mods in your Mods folder (those without a Lua `entrypoint`); **NEW** starts another. **(OFF)** marks a mod switched off in **Mods**. |
| Left, next rows | `<` `>` choose the **scope**: **Shared** (moves every fighter has, like most kicks), **Unarmed**, or a weapon subtype such as **Katana**. The second row picks one **weapon** of that subtype, or **Whole subtype**. |
| Left, list | The moves fighters in this scope use. **family of 3** means three subtypes share the move; **shared** means every fighter has it; a gold dot marks moves you changed. Type in **Search** to filter. |
| Centre | The fighter wearing the chosen weapon. **PLAY** starts the move, **PAUSE** freezes it, **STEP** advances one game tick, **LOOP** repeats it. The readout shows the tick and the *keyframe*, the frame number that intervals and attacks use. |
| Centre, timeline | One bar per frame window: red bars are attacks, grey bars are windows such as `Uninterrupt` or `Block`, gold bars are windows you added. The gold line is the current keyframe. |
| Right | The **inspector**: every value you can change, each with `−` and `+` buttons. |

Keys: **P** play, **K** pause, **L** step, **Z** undo, **Y** redo, **F5** apply,
**T** test in training, **Esc** back.

## What you can change

| Field | Meaning |
| --- | --- |
| Speed | How fast the whole move plays, from 0.50× to 2.00× in steps of 0.05. Faster moves keep their reach; everything timed by frames (attacks, windows, sounds) moves with them. Looped and physics moves keep their speed. |
| Priority | Which move wins when several match the same input. Higher wins. |
| Attack: damage | Base damage of the hit. |
| Attack: starts / ends | The keyframes during which the attack can hit. |
| Attack: hit reaction | How the opponent reacts, such as `High` or `MiddleShortPlus`. Only shown when the attack has one reaction; attacks with several are edited in Lua. |
| Attack: push X / Y | The push (impulse) applied on hit. |
| Attack: edges | The **hitbox**: the body or weapon parts that hit. Red toggles are active; at least one must stay on. During PLAY the active edges are drawn as red capsules. |
| Frame windows | Move a window's start and end, remove it, or restore a removed one. **New window type** and **Add** insert a `Block`, `Invulnerable`, `Invisible` or `Throwable` window at the current keyframe. |
| Animation | **Change animation** swaps the clip the move plays for another native clip. See [swap an animation](#swap-an-animation). |
| Move | **Reset to base game** drops your changes; **Disable move** makes it unselectable. |

## Who gets a change

Many moves are shared: all kicks belong to every fighter, and a katana move may belong to
the katana, the ninja sword and another subtype. The first time you change such a move,
the Lab asks who should get the change:

- **Whole family** or **Every fighter** changes the move itself, for everyone who has it.
- **Only Katana (copy)** makes a copy of the move for that subtype. The original stays
  for the rest of the family.
- **Only Golden Katana (copy)** appears when a weapon is chosen. It makes a copy for that
  one weapon, including weapons added by other mods. The original stays for every other
  weapon.

Copies are called *forks*. They appear in the list under their own name (for example
`HighKick_weapon_golden_katana`) and keep the original's combos. **Delete this copy**
in the inspector removes one.

## Swap an animation

**Change animation** turns the inspector into a clip list. Clips used by moves in the
current view come first; type in the search box to find any of the game's clips.

1. Click a clip. The fighter plays it, through a base-game move that uses it, and the
   panel shows how many keyframes the clip gives this move.
2. Read the checks under the clip name. The move keeps its own attacks, frame windows and
   sounds at the same keyframes, so a shorter clip can leave an attack after its last
   frame. The Lab lists each one, for example
   `Attack 64 (16–18) ends after the clip's last keyframe 14.` Move or remove those
   before you share the mod. A clip shorter than the move's first keyframe cannot be used.
3. Press **USE THIS CLIP**. **ORIGINAL** goes back to the move's own clip.

The new clip shows on the fighter after **APPLY**. After a swap, the frame limits for
attacks and windows follow the new clip's length.

A clip made for a different body or weapon can look wrong: the body and weapon parts
named as the attack's edges may not move where the new clip expects. Watch the red edges
in the preview. Clips shipped inside a mod's `assets/` folder are not listed; use a
[moveset file](../../api/movesets/) for those.

## Choose or start a mod

The Lab saves into one mod at a time, shown in the first row of the left panel. The first
time, that is **Moveset Lab** (`local.moveset-lab`).

- **NEW** asks for a name and makes the ID from it: "Heavy Katana" becomes
  `local.heavy-katana`. Nothing is written until **APPLY**, which creates
  `Mods/local.heavy-katana/`.
- `<` `>` open another data-only mod, including one you installed. Its saved edits load
  into the Lab, and APPLY writes back into that mod's own folder, keeping its name,
  version and dependencies.

If you have unapplied changes, the first press of `<`, `>` or **NEW** warns you and the
second discards them. APPLY needs the mod switched on in **Mods**; a new mod is on until
you switch it off.

## Apply, test and share

Changes stay in the Lab until you press **APPLY**. Applying:

1. saves the mod you are editing to its folder, for example `Mods/local.moveset-lab/`
   (`mod.toml` and `movesets/moveset.json`);
2. applies it, with every other enabled mod's move edits, to the running game;
3. rebuilds the preview fighter, so forks and speed changes show at once.

**TEST** applies unsaved changes, then starts training with the
Lab's fighter. In the training menu (**Esc**), **MOVESET LAB** returns to the Lab.

Leaving the Lab with **Esc** puts the game back to the move edits it started with. If you
have unapplied changes, the first **Esc** warns you and the second discards them. Your
applied mod is enabled like any other mod. It loads normally from the next start, and you
can switch it off under **Mods**.

To share your work, zip the mod's folder and [install it](../../api/installing-mods/)
on another game. Start shared work with **NEW** and a distinctive name, so its ID does not
clash with someone else's mod. A mod you share as `local.moveset-lab` would replace the
other player's own Lab mod.

When a fork names a weapon from another mod, the Lab adds that mod to `[[dependencies]]`,
so the moveset loads only when the weapon does.

## Limits

- The Lab edits data-only mods. Mods with a Lua `entrypoint` are not listed; use
  [`sf2.moves.patch`](../../api/moves-and-tactics/#sf2movespatch) in those instead.
- A mod's version is not changed when you save. Raise `version` in `mod.toml` yourself
  before sharing an update.
- Only one mod can patch a given move. If another enabled mod already patches a move,
  APPLY reports the conflict.
- Every change is guarded against the base game's value, so a game update that changes
  a move makes the mod refuse to load instead of applying a wrong edit.
- Online versus is unavailable while the Lab is open, and while any moveset mod is
  enabled.
- Edits are checked against the game data, not against gameplay. Test changed timings
  and hitboxes in a fight before you share them.
- Dragging bars on the timeline, editing damage types, swapping in clips from a mod's
  `assets/` folder and editing a weapon's subtype are not in the Lab yet. Use a
  [moveset file](../../api/movesets/) or Lua for those.
