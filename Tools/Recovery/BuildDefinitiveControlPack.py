#!/usr/bin/env python3
"""Build the "Definitive" on-screen control pack from the owner's 2026-10 drop.

Source: ResearchSources/new_assets/new_assets/controls/ (finished outline-style buttons, no
pressed state). Output: Assets/StreamingAssets/ControlPacks/Definitive/ in the layer format of
Eclipse/UI/ControlTexturePacks.cs:

* each round button is split at the clear gap between its ring and its icon into
  ``fight_bg_frame`` (the ring, shared) and ``icon_*`` (the icon). ``fight_bg_normal`` is empty and
  ``fight_bg_active`` is a filled disc, so a pressed button shows its icon cut out of a solid disc
  like the game's other packs;
* the joystick ring is ``fight_bg_frame_big`` with an empty ``stick_arrows`` layer, and the filled
  knob ships as finished member images ``Joystick_norm`` / ``Joystick_action``;
* magic progress and the full magic/ranged icons are copied under their layer names.

Usage: python Tools/Recovery/BuildDefinitiveControlPack.py
"""

from __future__ import annotations

import math
import shutil
import sys
from pathlib import Path

from PIL import Image, ImageDraw

ROOT = Path(__file__).resolve().parents[2]
SOURCE = ROOT / "ResearchSources" / "new_assets" / "new_assets" / "controls"
TARGET = ROOT / "Assets" / "StreamingAssets" / "ControlPacks" / "Definitive"
BUTTONS = {"punch": "icon_Punch", "kick": "icon_Kick", "magic": "icon_Magic", "charge": "icon_Charge", "ranged": "ranged_attack"}
COPIES = {"magic_progress": "icon_Stroke", "icon_magic_full": "icon_Magic_Full", "icon_ranged_full": "icon_Range_Full",
          "joystick_container": "fight_bg_frame_big"}


def radial_profile(images: list[Image.Image]) -> list[int]:
    """Largest alpha at each whole radius from the centre, over all buttons."""
    size = images[0].width
    centre = (size - 1) / 2
    profile = [0] * (size // 2 + 2)
    for image in images:
        alpha = image.getchannel("A").load()
        for y in range(image.height):
            for x in range(image.width):
                r = int(math.hypot(x - centre, y - centre))
                if r < len(profile):
                    profile[r] = max(profile[r], alpha[x, y])
    return profile


def ring_bounds(profile: list[int]) -> tuple[int, int]:
    """(inner, outer) radius of the outermost opaque band, separated from the icon by a gap."""
    outer = max(r for r, a in enumerate(profile) if a > 0)
    inner = outer
    while inner > 0 and profile[inner - 1] > 0:
        inner -= 1
    if inner <= 0:
        raise SystemExit("No clear gap between ring and icon; cannot split the buttons.")
    return inner, outer


def split(image: Image.Image, inner: int) -> tuple[Image.Image, Image.Image]:
    """(ring, icon): pixels at or beyond the inner ring radius, and the rest."""
    centre = (image.width - 1) / 2
    ring = Image.new("RGBA", image.size, (0, 0, 0, 0))
    icon = Image.new("RGBA", image.size, (0, 0, 0, 0))
    src = image.load()
    for y in range(image.height):
        for x in range(image.width):
            (ring if math.hypot(x - centre, y - centre) >= inner - 0.5 else icon).putpixel((x, y), src[x, y])
    return ring, icon


def main() -> int:
    buttons = {name: Image.open(SOURCE / f"{name}.png").convert("RGBA") for name in BUTTONS}
    sizes = {image.size for image in buttons.values()}
    if len(sizes) != 1:
        raise SystemExit(f"Buttons differ in size: {sizes}")
    size = sizes.pop()
    inner, outer = ring_bounds(radial_profile(list(buttons.values())))
    if TARGET.exists():
        shutil.rmtree(TARGET)
    TARGET.mkdir(parents=True)
    ring = None
    for name, layer in BUTTONS.items():
        ring_part, icon = split(buttons[name], inner)
        ring = ring or ring_part
        icon.save(TARGET / f"{layer}.png")
    ring.save(TARGET / "fight_bg_frame.png")
    Image.new("RGBA", size, (0, 0, 0, 0)).save(TARGET / "fight_bg_normal.png")
    disc = Image.new("RGBA", size, (0, 0, 0, 0))
    centre = (size[0] - 1) / 2
    ImageDraw.Draw(disc).ellipse((centre - outer, centre - outer, centre + outer, centre + outer), fill=(255, 255, 255, 255))
    disc.save(TARGET / "fight_bg_active.png")
    Image.new("RGBA", (1, 1), (0, 0, 0, 0)).save(TARGET / "stick_arrows.png")
    for name, layer in COPIES.items():
        shutil.copyfile(SOURCE / f"{name}.png", TARGET / f"{layer}.png")
    for member in ("Joystick_norm", "Joystick_action"):
        shutil.copyfile(SOURCE / "joystick_nub.png", TARGET / f"{member}.png")
    print(f"ring radius {inner}-{outer} px of {size[0]}; wrote {len(list(TARGET.glob('*.png')))} files to {TARGET.relative_to(ROOT)}")
    return 0


if __name__ == "__main__":
    sys.exit(main())
