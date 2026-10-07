---
title: On-screen control texture packs
description: Replace the joystick and fight-button artwork with local PNG packs.
---

Control packs replace on-screen fight controls. Open Options → Accessibility and cycle
the control pack setting. The selection is stored locally and applies when the
next fight or dojo creates its controls. `Default` uses the original artwork.

Create a folder called `ControlPacks/My Pack/` beside the game's data folder,
inside Unity's persistent data directory, or under `StreamingAssets/`. Persistent
data packs take priority, followed by the game folder and then StreamingAssets.
Folders with the same name are matched without regard to letter case. At least
one PNG is needed for a pack to appear in the list; reopen the option to rescan folders.
Loaded images are cached for the game session; restart after editing a pack's PNGs.

Each pack is a folder of layer images with fixed file names. The game builds every
on-screen control from these layers, so draw them as plain white (or any colour)
shapes on a transparent background. The `DE128` pack in `StreamingAssets/ControlPacks/`
is a complete example. `Clean` is an alternative pack with simplified white icons,
thin borders and restrained highlight rings. It keeps the same layer names and
dimensions as `DE128`, including the original background discs for consistent
idle transparency and solid pressed states.

`Moonforge` uses a lunar metal theme: translucent navy backgrounds, silver-blue
borders, and violet/gold highlights. Its action icons are an armored gauntlet,
armored boot, lunar flame, four-bladed throwing glaive, and crescent charge vortex.
It uses the same filenames and dimensions as `DE128`. Its pressed background
lights up blue-violet, with the action icon cut out by the existing compositor.

`Definitive` is the outline style of the 2026 Definitive Edition controls: thin
white rings with white icons on a transparent button. A pressed button fills
solid white with its icon cut out. Its joystick knob is a filled white dot, shipped
as finished images (see below).

| File | Size in DE128 | Used for |
| --- | --- | --- |
| `fight_bg_normal.png` | 200×200 | Button and joystick-knob background when idle. Partly transparent. |
| `fight_bg_active.png` | 200×200 | Button and knob background while pressed. |
| `fight_bg_frame.png` | 200×200 | Ring drawn over every button and the knob. |
| `fight_bg_frame_big.png` | 466×466 | Outer ring of the joystick area. |
| `stick_arrows.png` | 406×407 | Direction arrows and centre of the joystick area. |
| `icon_Punch.png` | 80×72 | Punch button icon. |
| `icon_Kick.png` | 49×112 | Kick button icon. |
| `icon_Magic.png` | 106×101 | Magic button icon. |
| `ranged_attack.png` | 96×113 | Ranged button icon. |
| `icon_Charge.png` | 128×116 | Raid charge button icon. |
| `icon_Stroke.png` | 218×218 | Magic recharge ring. |
| `icon_Magic_Full.png` | 120×113 | Magic ready icon. |
| `icon_Range_Full.png` | 110×125 | Ranged ready icon. |
| `icon_HighlightStick.png` | 512×512 | Joystick tutorial highlight. |
| `icon_KickHighlight.png` | 255×255 | Kick tutorial highlight. |
| `icon_ChargeHighlight.png` | 253×245 | Raid charge highlight. |

How the layers combine:

- **Buttons** (punch, kick, magic, ranged, raid charge): the background, then
  `fight_bg_frame.png`, then the icon, all centred at their own pixel size. Idle
  buttons use `fight_bg_normal.png` with the icon drawn on top. Pressed buttons use
  `fight_bg_active.png` and the icon is cut out of it, so it shows as a see-through
  shape on the solid button.
- **Joystick area**: `fight_bg_frame_big.png` with `stick_arrows.png` centred on it.
- **Joystick knob**: `fight_bg_normal.png` (idle) or `fight_bg_active.png` (held),
  with `fight_bg_frame.png` on top.
- **Single images** (rings, ready icons and highlights) are used as they are.

A pack can also replace a whole control with a finished image. Name the PNG after
the `UI/Atlases/FightButtons` member it replaces, for example `Joystick_norm.png`
and `Joystick_action.png` for the idle and held joystick knob, or
`btn_punch_normal.png` and `btn_punch_action.png` for the idle and pressed punch
button. A finished image is used as it is, instead of that control's layers, and
is shown at the built-in control's size. The `Definitive` pack uses this for its
knob.

Size the icons relative to the 200-pixel background, because they are placed at
their own pixel size before the whole control is scaled to its on-screen size. A
control is shown at the built-in control's size, so higher-resolution layers stay
the same size on screen. Keep layer proportions the same as the DE128 example.
If any layer a control needs is missing or unreadable, that control keeps the
built-in artwork. The pause button and the ranged ammo ring always use the
built-in artwork. Only the `UI/Atlases/FightButtons` atlas is affected. A mod's
explicit sprite replacement takes priority over the selected local control pack.

The example folders also include `fight_glow_big.png` and `fight_glow_small.png`.
The current control-pack loader does not use these two layers; they are included
for artwork completeness and do not add an in-game glow effect.

These are image folders, with no manifest or executable scripts. They do not
change bindings or gameplay and are not part of campaign saves. Eclipse ships
three example packs, `DE128`, `Clean`, and `Moonforge`. A missing selected folder falls back to
`Default`.
