---
title: Moveset Lab
description: Tune moves in game — speed, damage, hit frames, hitboxes and priority — and save them as a mod without writing Lua.
---

The **Moveset Lab** is an in-game editor for existing moves. You pick a weapon subtype
(or one weapon), watch a move on a live fighter with its hitting parts drawn in red and a
victim playing the hit reaction, change its values, and save the result as an ordinary mod. You don't need Lua or a
text editor. The Lab writes a [moveset file](../../api/movesets/) that you can share,
edit by hand, or combine with other mods.

The Lab tunes existing moves, changes their inputs, and adds [new moves](#make-a-new-move)
based on existing ones. It cannot make animations or define moves from scratch; use
[`sf2.moves.register`](../../api/moves-and-tactics/#sf2movesregister) for those.

## Open the Lab

Either:

- open **Multiplayer** on the title screen and choose **MOVESET LAB** (or press **M**); or
- open **Mods** on the title screen and choose **Moveset Lab**.

## The screen

| Area | What it does |
| --- | --- |
| Top bar | **MOD** is the mod you are editing; **NEW** starts another. **FIGHTERS** chooses the *scope*: **Shared** (moves every fighter has, like most kicks), **Unarmed**, or a weapon subtype such as **Katana**. **WEAPON** narrows it to one weapon of that subtype (its subtype moves plus moves made only for that weapon; shared moves stay under **Shared**), or **Every Katana**. On the right are **UNDO**, **REDO**, **APPLY**, **TEST** and **CLOSE**. *Unapplied edits* means the fighters still show the last applied version. |
| Left | The moves fighters in this scope use. **family of 3** means three subtypes share the move; **shared** means every fighter has it; a gold dot marks moves you changed. **Search** filters by name; **EDITED** shows only moves you changed. **+ NEW MOVE** adds a [new move](#make-a-new-move); new moves are listed under the move they are based on, marked **new move**. |
| Centre, stage | Left: the attacker wearing the chosen weapon. Right: the *victim*, a training dummy that plays the hit reaction when an attack lands. |
| Centre, transport | `\|<` `<` **PLAY** `>` `>\|` jump to the first frame, step one keyframe back, play or pause, step one keyframe forward, and jump to the last frame. **RESTART** plays from the start, **LOOP** repeats the move, **0.1x**–**1x** slow the preview down, and **VICTIM** turns the reaction preview on or off. The readout shows the current *keyframe* (the frame number that attacks and windows use) and the game tick. |
| Centre, timeline | One lane per attack and frame window: red bars are attacks, blue-grey bars are windows such as `Uninterrupt` or `Block`, gold bars are windows you added. The gold line is the current keyframe. Click or drag along the ruler or an empty lane to scrub. Drag a bar to move it, or its left or right end to resize it; the change is one undo step when you let go. Click a bar to open it in the inspector. |
| Right | The **inspector**, one tab at a time: **MOVE**, **ATTACK**, **WINDOWS** and **HITBOX**. |

Number fields have `−` and `+` buttons and a box you can type in: press **Enter** or click
away to set the value. Hold **Shift** for ten steps at a time, or **Ctrl** for a tenth of
a step on fractional values such as damage. A changed value shows a gold dot, its
base-game value, and **RESET**.

Keys: **Space** play or pause, **Left**/**Right** one keyframe (**Shift**: five),
**Home**/**End** first or last keyframe, **Up**/**Down** previous or next move,
**1**–**4** inspector tabs, **Ctrl+Z** undo, **Ctrl+Y** or **Ctrl+Shift+Z** redo,
**Ctrl+F** search, **F5** apply, **T** test in training, **Esc** close a list or leave.
Keys do nothing while you type in a field.

## What you can change

| Tab | Field | Meaning |
| --- | --- | --- |
| Move | Input | The keys that start the move. See [change a move's input](#change-a-moves-input). |
| Move | Speed | How fast the whole move plays, from 0.50× up to the clip's limit (at most 2.00×), in steps of 0.05. Everything timed by frames (attacks, windows, sounds) moves with it. Looped and physics moves keep their speed. |
| Move | Priority | Which move wins when several match the same input. Higher wins. |
| Move | Clip | **CHANGE CLIP...** swaps the clip the move plays for another native clip. See [swap an animation](#swap-an-animation). |
| Move | Move | **Reset to base game** drops your changes; **Disable move** makes it unselectable; **Delete this copy** removes a fork. |
| Attack | Starts / ends at frame | The keyframes during which the attack can hit. A move with several attacks shows one button per attack. |
| Attack | Base damage | The attack's damage value, 0–16. Base-game hits mostly sit between 0.06 and 0.45; the fighters' attributes, blocking and critical hits scale it. |
| Attack | Damage shifts | One row per damage type the attack already has (`Weapon`, `Ranged`, `Magic`, `Unarmed`): a number from −1000 to 1000 added to the attacker's matching attribute before it is weighed against the defender's defence. Damage types cannot be added or removed here. |
| Attack | Reaction | How the opponent reacts, such as `High` or `SpinningHeavy`. Only shown when the attack has one reaction; attacks with several are edited in Lua. |
| Attack | Victim plays | The victim moves that answer this reaction. The game picks among them by the victim's state (blocking, critical hits and so on); `<` `>` browse them and **PLAY** shows the chosen one. Entries marked *near wall* play the same move with a wall behind the victim, set by **Wall behind victim**; if the victim reaches it during the move's wall-hit window, the game turns the recoil into `WallHit` or `WallHitFall`. Otherwise the preview has no walls. This is a preview only and is not saved. |
| Attack | Push X / Y / Z | The push (impulse) given to the victim on hit. |
| Windows | Start / End | Each window's frames, with **REMOVE**, **RESTORE** (for a removed window) or **DELETE** (for one you added). A window without an end lasts to the end of the move. |
| Windows | Add a window | Inserts a `Block`, `Invulnerable`, `Invisible` or `Throwable` window at the current keyframe, four frames long. |
| Hitbox | Hitting parts | The body or weapon parts that deal the attack's hit. See [edit a hitbox](#edit-a-hitbox). |

## Change a move's input

The **INPUT** section at the top of the **MOVE** tab shows the keys that start the move,
for example `Punch + hold Forward`. It works for every move whose keys form one chord, or a
set of alternative chords; a few moves mix their keys into other conditions, and the Lab
says so instead.

- A *chord* is the keys pressed together. Each key has a press type: **TAP** (press once),
  **HOLD** (keep it held) or **RELEASE** (let it go). Click a key to choose another one and
  its press type to change how it is pressed. The same key tapped twice is a two-tap
  sequence.
- **+ KEY** adds a key to the chord (up to 14); **x** removes one.
- **+ ALTERNATIVE** adds another chord that also starts the move, such as a dash that
  starts on Up-Back or on Back tapped twice. Up to 8 alternatives.
- **REMOVE INPUT** leaves the move without keys, and **ADD AN INPUT** gives one to a move
  that has none. Moves without keys are started by the game itself (hit reactions, combo
  steps, AI actions), so both change when the move can happen; the Lab warns you.
- **RESET** returns to the base game's input.

Directions are for a fighter facing right; the game mirrors them when it faces left. The
input does not change what the move must follow: a combo step still needs the move before
it. When two moves have the same input in the same situation, the higher **Priority** wins.

## Make a new move

**+ NEW MOVE** (under the move list's filters) adds a move based on an existing one:

1. Give it a name. It becomes the move's ID in your mod, for example `rising_slash`.
2. **Based on** is the move it starts as a copy of: animation, attacks, frame windows,
   sounds and what it must follow. It defaults to the selected move; the list shows the
   moves in the current view first.
3. **Who can use it**: the same fighters as the original, only the current subtype (for
   example only Katana), or only the chosen weapon.
4. **CREATE** selects the new move. Give it its own input under **INPUT**; with the
   original's input both moves compete and priority decides. Then edit it like any other
   move.

The original move is not changed. Combos written for the original's exact name do not
continue from the new move, but moves that follow a shared template still can. The preview
plays the original until you **APPLY**. **DELETE THIS MOVE** on the **MOVE** tab removes a new
move.

## Edit a hitbox

The **HITBOX** tab edits the selected attack's hitting parts (*edges*). The fighter shows
every part you can pick: red parts hit, grey parts don't, and the part under the pointer
turns gold.

- Click a part on the fighter, or its button in the list, to add or remove it. At least
  one part must stay on.
- Parts are grouped as **Weapon**, **Arm** and **Leg** for each side, and **Head and torso**.
  *Side 1* and *side 2* are the game's two limbs; a mirrored move swaps them. Hover a
  button to see which limb it is.
- **MIRROR SIDES** moves the hitbox to the other arm or leg; **RESET HITBOX** restores
  the base game's parts.
- Extra letters after a part's name, such as `Hand S` or `Chest HD`, mark extra edges
  that shape the body. Edges with no collision size are *braces*: they hold the rig's
  shape and rarely make sense as hitting parts, so they are hidden until you press
  **SHOW BRACES**.
- Parts that belong to another weapon or rig are listed under **Not on this fighter**.
  They still hit when a fighter that has them uses the move.

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
on the **MOVE** tab removes one.

## Swap an animation

**CHANGE CLIP...** on the **MOVE** tab turns the inspector into a clip list. Clips used by moves in the
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

The Lab saves into one mod at a time, shown as **MOD** in the top bar. The first
time, that is **Moveset Lab** (`local.moveset-lab`).

- **NEW** asks for a name and makes the ID from it: "Heavy Katana" becomes
  `local.heavy-katana`. Nothing is written until **APPLY**, which creates
  `Mods/local.heavy-katana/`.
- The **MOD** list opens another data-only mod (one without a Lua `entrypoint`),
  including one you installed. *(off)* marks a mod switched off in **Mods**. Its saved edits load
  into the Lab, and APPLY writes back into that mod's own folder, keeping its name,
  version and dependencies.

If you have unapplied changes, the first choice of another mod or **NEW** warns you and
the second discards them. APPLY needs the mod switched on in **Mods**; a new mod is on until
you switch it off.

## Apply, test and share

Changes stay in the Lab until you press **APPLY**. Applying:

1. saves the mod you are editing to its folder, for example `Mods/local.moveset-lab/`
   (`mod.toml` and `movesets/moveset.json`);
2. applies it, with every other enabled mod's move edits, to the running game;
3. rebuilds the preview fighters, so forks and speed changes show at once.

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
- Adding or removing damage types, attacks with several reactions, swapping in clips
  from a mod's `assets/` folder and editing a weapon's subtype are not in the Lab yet. Use
  a [moveset file](../../api/movesets/) or Lua for those.
- Scrubbing and stepping back replay the move from the fighter's start position up to the
  chosen keyframe, so every pose is one normal playback reaches. Stepping forward stops at
  the move's last keyframe. The victim's reaction while scrubbing is an approximation: it is
  matched by keyframe count since the hit, not by exact game ticks.
