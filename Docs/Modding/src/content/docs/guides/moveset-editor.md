---
title: Moveset Lab
description: Tune moves in game — speed, damage, hit frames, hitboxes and priority — and save them as a mod without writing Lua.
---

The **Moveset Lab** is an in-game editor for existing moves. You pick a weapon subtype
(or one weapon), watch a move on a live fighter with its hitting parts drawn in red and a
victim playing the hit reaction, change its values, and save the result as an ordinary mod. You don't need Lua or a
text editor. The Lab writes a [moveset file](../../api/movesets/) that you can share,
edit by hand, or combine with other mods.

The Lab tunes existing moves, changes their inputs, adds [new moves](#make-a-new-move)
based on existing ones, and [imports your own animation clips](#import-your-own-clip). It
does not create animations or define moves from scratch; use
[`sf2.moves.register`](../../api/moves-and-tactics/#sf2movesregister) for moves built
entirely from your own data.

## Open the Lab

Either:

- open **Multiplayer** on the title screen and choose **MOVESET LAB** (or press **M**); or
- open **Mods** on the title screen and choose **Moveset Lab**.

## The screen

| Area | What it does |
| --- | --- |
| Top bar | **MOD** is the mod you are editing; **NEW** starts another. **FIGHTERS** chooses the *scope*: **Shared** (moves every fighter has, like most kicks), **Unarmed**, or a weapon subtype such as **Katana**. **WEAPON** narrows it to one weapon of that subtype (its subtype moves plus moves made only for that weapon; shared moves stay under **Shared**), or **Every Katana**. **+ WEAPON** [makes a weapon](#make-a-weapon). On the right are **UNDO**, **REDO**, **APPLY**, **TEST**, **EXPORT** and **CLOSE**. *Unapplied edits* means the fighters still show the last applied version. |
| Left | The moves fighters in this scope use. **family of 3** means three subtypes share the move; **shared** means every fighter has it; a gold dot marks moves you changed. **Search** filters by name; **EDITED** shows only moves you changed. **+ NEW MOVE** adds a [new move](#make-a-new-move); new moves are listed under the move they are based on, marked **new move**. |
| Centre, stage | Left: the attacker wearing the chosen weapon. Right: the *victim*, a training dummy that plays the hit reaction when an attack lands. |
| Centre, transport | `\|<` `<` **PLAY** `>` `>\|` jump to the first frame, step one keyframe back, play or pause, step one keyframe forward, and jump to the last frame. **RESTART** plays from the start, **LOOP** repeats the move, **0.1x**–**1x** slow the preview down, **VICTIM** turns the reaction preview on or off, and **GHOST** draws the [base-game move](#compare-with-the-base-game) faintly behind your edit. The readout shows the current *keyframe* (the frame number that attacks and windows use) and the game tick. |
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
| Move | Clip | **CHANGE CLIP...** swaps the clip the move plays for another native clip or [one of your own](#import-your-own-clip). See [swap an animation](#swap-an-animation). |
| Move | Trim | **Starts at keyframe** and **Ends at keyframe** set which part of the clip the move plays: start later to skip a wind-up, end earlier to cut a long recovery. Attacks and windows keep their keyframe numbers. **UNDO TRIM** goes back to the base game. Looped and physics moves cannot be trimmed. |
| Move | Combos | **+ CAN FOLLOW...** lets this move start out of another move in this view during a window of its keyframes, even where it normally cannot be interrupted. See [link a combo](#link-a-combo). |
| Move | Move | **Reset to base game** drops your changes; **Disable move** makes it unselectable; **Delete this copy** removes a fork. |
| Attack | Starts / ends at frame | The keyframes during which the attack can hit. A move with several attacks shows one button per attack. |
| Attack | + NEW ATTACK | Adds an attack to the move, two frames long at the playhead, with the next free id. It copies the selected attack's damage, damage types, hitting parts, push and reaction (or plain defaults on a move without attacks), so you only change what differs. New attacks are marked **new**, edit like any other attack (timeline, damage, reaction, hitbox) and have **DELETE ATTACK**. |
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
  button to see which limb it is. On a mirrored move the fighter highlights the limb the
  attack really hits with, and the caption says *mirrored pose*: clicking a limb there
  picks the other side's name, as the game does.
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

1. Click a clip. The fighter plays this move with that clip, and the panel shows how many
   keyframes the clip gives the move. Nothing is changed until you choose **USE THIS CLIP**;
   closing the list puts the move's own clip back.
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
in the preview.

### Import your own clip

**IMPORT CLIP...** in the clip list adds a clip file to the mod you are editing. Make the
file with the [character pipeline](../character-authoring/): a native `.bytes` clip, for
example baked from Blender. Renaming another kind of file to `.bytes` does not work.

1. Press **IMPORT CLIP...** and choose the `.bytes` file. The Lab checks that it is a
   complete native clip and that it moves as many body nodes as this move's fighter (67
   for a normal fighter). A clip made for another rig is refused.
2. The clip joins the top of the list as `animations/<name>  (this mod)`. The name comes
   from the file name in lowercase, with other characters turned into `_`. A different
   file with the same name gets `_2`, `_3`, and so on.
3. Read the checks and press **USE THIS CLIP**, as for a game clip.
4. Press **APPLY**. The Lab writes the clip to
   `Mods/<your mod>/assets/animations/<name>.bytes` and the move plays it on the fighter.

Click your clip in the list to watch it on the fighter before you use it. Clips already in
the mod's `assets/animations/` folder,
including ones you copied there by hand, are listed too. In the
[moveset file](../../api/movesets/) the swap is saved as
`"animation": { "expected": "<original>.bytes", "value": { "asset": "animations/<name>" } }`.
Importing works on desktop. On Android, copy the file into the mod's
`assets/animations/` folder.

## Compare with the base game

**GHOST** in the transport bar draws the base-game version of the move, faint and blue,
behind the fighter you are editing. Both start together and run tick for tick, so a
faster speed, a trim or a swapped clip shows as the two drifting apart. The ghost ignores
every mod's changes, including your applied ones. A copy (fork) or new move is compared
with the base-game move it was made from.

## Link a combo

A *combo link* lets one move start out of another, on its own input, while the other is
still playing:

1. Select the move that should come second.
2. On the **MOVE** tab, under **COMBOS**, press **+ CAN FOLLOW...** and choose the move it
   follows.
3. Set **Window from keyframe** and **Window to keyframe**: the keyframes of the followed
   move during which the second move may start. The Lab starts with the second half of
   the followed move.
4. **APPLY**, then **TEST** in training: play the first move, then the second move's input
   inside the window.

**REMOVE LINK** deletes a link. A move can follow up to 16 moves. The window does not
change the followed move's own timing, and the AI does not use the link on purpose. The
link is saved as [`chains`](../../api/movesets/#combo-links).

## Make a weapon

**+ WEAPON** in the top bar adds a weapon to the mod you are editing. It starts as a copy
of a game weapon: the same subtype (so the same moves) and the same model.

| Field | Meaning |
| --- | --- |
| Name | The name players see. The weapon's ID is made from it, such as `heavy_blade`. |
| Based on | The game weapon to copy. Its subtype decides which moves the new weapon uses. |
| Model | The base weapon's model, or **IMPORT MODEL...** for an Eclipse model file (`.xml` or `.modelz` geometry with a `<Scene>` root, such as one exported by the character tools). **COPY BASE MODEL** puts the base weapon's model into the mod as `assets/models/<id>.xml`, ready for you to edit. |
| Hands | **ONE WEAPON**, or **ONE IN EACH HAND** to copy a one-hand model into the other hand, the way the game builds dual weapons such as daggers: each part ending `_1` (or without a number) gets a `_2` twin attached to the second hand, so `WEAPON_KATANA-Blade` becomes `WEAPON_KATANA-Blade_1` and `WEAPON_KATANA-Blade_2`. A model that already holds two weapons is kept as it is. Base the weapon on a two-weapon class such as Daggers, Knives or Axes, so its moves swing both hands; the dialog warns when the class uses one hand. |
| Shop icon | **IMPORT PNG...** for the weapon shop picture. Optional. |
| Shop price | The price in coins. |

The dialog checks an imported model's edges against the parts the subtype's moves hit
with. Moves find a weapon's hitting parts by name, such as `WEAPON_KATANA-Blade`, so a
model missing them never lands those attacks. Name the model's edges the same, or change
those attacks' hitting parts in the **HITBOX** tab.

**CREATE** writes `weapons/weapons.json` (see [weapon files](../../api/movesets/#weapon-files))
and the model and icon files into the mod, and applies your unapplied move edits too.
The Lab then loads the weapon into the running game and selects it under **WEAPON**, so
the preview and **TEST** use it straight away; edits you make while it is selected can
[go to this weapon only](#who-gets-a-change). The Lab also loads its mod's weapons each
time it opens. The weapon joins the weapon shop, at level 1 for the price you set, the
next time the game starts.

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

To share your work, press **EXPORT**. It applies your changes, adds the Eclipse core
version range to `mod.toml` if it is missing, and writes `<mod id>-<version>.zip` to an
**Eclipse mods** folder on the desktop, ready to [install](../../api/installing-mods/) on
another game or upload to [mod.io](../../api/installing-mods/#publishing-to-modio). Raise
`version` in `mod.toml` before you upload an update. Start shared work with **NEW** and a distinctive name, so its ID does not
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
