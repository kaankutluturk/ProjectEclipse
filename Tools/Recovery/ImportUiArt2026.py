#!/usr/bin/env python3
"""Install the UI art of the owner's 2026-10 drop (ResearchSources/new_assets/new_assets).

Everything goes into the packed art catalog, which the game reads before Assets/Resources, so
no Unity asset is imported or hand-edited:

* ProfileButtons members (ZONE_1) are re-pointed at the drop's 2x images;
* ShopButtons1/2 and SlidersSettings become packed atlases in the new UI_2026 group; members the
  drop does not redraw are copied from the recovered Resources atlas with their original rects;
* new sprites: the versus arrows (Textures/fight/pointers/arrow, arrowSecondPlayer), event
  avatars (UI/Users), the BattleBtnMidAutumn26* map button atlases, the preview_autumnFest26
  stage preview, weapon icons (UI/Items), plus the cursed_brothers_mafest26 music and four
  model XMLs (gamedata/...).

A replacement keeps its predecessor's on-screen size: its pixels-per-unit grows with the image.
Drop-to-member mappings are checked by comparing each image, scaled down, with the member it
replaces; ``plan`` prints the scores (lower is closer).

Usage:
    python Tools/Recovery/ImportUiArt2026.py plan
    python Tools/Recovery/ImportUiArt2026.py apply
"""

from __future__ import annotations

import argparse
import json
import re
import shutil
import subprocess
import sys
from pathlib import Path

from PIL import Image, ImageChops

ROOT = Path(__file__).resolve().parents[2]
DROP = ROOT / "ResearchSources" / "new_assets" / "new_assets"
RESOURCES_ATLASES = ROOT / "Assets" / "Resources" / "ui" / "atlases"
POINTERS = ROOT / "Assets" / "Resources" / "textures" / "fight" / "pointers"
CATALOG = ROOT / "Assets" / "Resources" / "SF2Content" / "Art" / "catalog.json"
WORK = ROOT / "Library" / "UiArt2026"
WORKSPACE = WORK / "ws"
PACKER = ROOT / "Tools" / "AssetPacker" / "bin" / "Release" / "net9.0" / "AssetPacker.dll"
GROUP = "UI_2026"
STATE = {"Active": "Active", "Inactive": "Inactive", "Pushed": "Pushed"}


class ImportFailure(RuntimeError):
    pass


def packer(*args: str) -> str:
    result = subprocess.run(["dotnet", str(PACKER), *args], capture_output=True, text=True, cwd=ROOT)
    if result.returncode != 0:
        raise ImportFailure("AssetPacker " + args[0] + " failed:\n" + (result.stdout + result.stderr).strip())
    return result.stdout


def fmt(value: float) -> str:
    return str(int(value)) if float(value).is_integer() else f"{value:.6f}".rstrip("0").rstrip(".")


def sprite(address: str, name: str, texture: str, width: int, height: int, ppu: float, filter_mode: int = 1,
           rect: tuple[int, int, int, int] | None = None, tex_size: tuple[int, int] | None = None, pivot=(0.5, 0.5)) -> str:
    x, y, w, h = rect or (0, 0, width, height)
    tw, th = tex_size or (width, height)
    left, right = -w * pivot[0] / ppu, w * (1 - pivot[0]) / ppu
    bottom, top = -h * pivot[1] / ppu, h * (1 - pivot[1]) / ppu
    u0, u1, v0, v1 = x / tw, (x + w) / tw, y / th, (y + h) / th
    return (
        f"type=sprite\nnamespace=core\naddress={address}\nname={name}\ntexture={texture}\n"
        f"rect={x},{y},{w},{h}\npivot={fmt(pivot[0])},{fmt(pivot[1])}\nborder=0,0,0,0\npixels_per_unit={fmt(ppu)}\n"
        f"filter={filter_mode}\naniso=1\nwrap_u=1\nwrap_v=1\nmipmaps=false\n"
        f"vertices={fmt(left)},{fmt(top)};{fmt(right)},{fmt(top)};{fmt(left)},{fmt(bottom)};{fmt(right)},{fmt(bottom)}\n"
        f"triangles=0,1,2,2,1,3\nuv={fmt(u0)},{fmt(v1)};{fmt(u1)},{fmt(v1)};{fmt(u0)},{fmt(v0)};{fmt(u1)},{fmt(v0)}\n"
    )


def descriptor_fields(path: Path) -> dict[str, str]:
    return dict(line.split("=", 1) for line in path.read_text(encoding="utf-8").splitlines() if "=" in line)


def resources_member(atlas: str, member: str) -> tuple[Path, tuple[int, int, int, int], float, tuple[float, float]]:
    """(atlas PNG, rect, pixels per unit, pivot) of a recovered Resources sprite asset (read only)."""
    text = (RESOURCES_ATLASES / f"{atlas}.{member}.asset").read_text(encoding="utf-8")
    rect = re.search(r"m_Rect:\s*\n\s*serializedVersion: \d+\s*\n\s*x: ([-0-9.e]+)\s*\n\s*y: ([-0-9.e]+)\s*\n\s*width: ([-0-9.e]+)\s*\n\s*height: ([-0-9.e]+)", text) \
        or re.search(r"m_Rect:\s*\n\s*x: ([-0-9.e]+)\s*\n\s*y: ([-0-9.e]+)\s*\n\s*width: ([-0-9.e]+)\s*\n\s*height: ([-0-9.e]+)", text)
    ppu = float(re.search(r"m_PixelsToUnits: ([0-9.e]+)", text).group(1))
    pivot = re.search(r"m_Pivot: \{x: ([0-9.e-]+), y: ([0-9.e-]+)\}", text)
    if not rect:
        raise ImportFailure(f"No rect in {atlas}.{member}.asset")
    x, y, w, h = (round(float(v)) for v in rect.groups())
    return RESOURCES_ATLASES / f"{atlas}.png", (x, y, w, h), ppu, (float(pivot.group(1)), float(pivot.group(2)))


def crop(texture: Path, rect: tuple[int, int, int, int]) -> Image.Image:
    image = Image.open(texture).convert("RGBA")
    x, y, w, h = rect
    top = image.height - y - h  # sprite rects count from the bottom
    return image.crop((x, top, x + w, top + h))


def difference(a: Image.Image, b: Image.Image) -> float:
    a = a.convert("RGBA").resize((48, 48))
    b = b.convert("RGBA").resize((48, 48))
    pixels = ImageChops.difference(a, b).convert("L")
    return sum(pixels.tobytes()) / (48 * 48)


class Builder:
    def __init__(self) -> None:
        self.notes: list[str] = []
        self.index = 0
        self.group = WORKSPACE / GROUP

    def add_texture(self, source: Path, name: str) -> str:
        texture = f"textures/ui2026/{name}"
        target = self.group / texture
        target.parent.mkdir(parents=True, exist_ok=True)
        shutil.copyfile(source, target)
        return texture

    def write(self, text: str) -> None:
        (self.group / "assets").mkdir(parents=True, exist_ok=True)
        (self.group / "assets" / f"{self.index:06d}.meta").write_text(text, encoding="utf-8", newline="\n")
        self.index += 1

    def add_payload(self, kind: str, address: str, name: str, source: Path, folder: str) -> None:
        relative = f"{folder}/{source.name}"
        target = self.group / relative
        target.parent.mkdir(parents=True, exist_ok=True)
        shutil.copyfile(source, target)
        self.write(f"type={kind}\nnamespace=core\naddress={address}\nname={name}\nfile={relative}\n")


def profile_buttons(builder: Builder, apply: bool) -> None:
    """Re-point the ZONE_1 ProfileButtons members at the drop's images."""
    metas = {descriptor_fields(p)["name"]: p for p in (WORKSPACE / "ZONE_1" / "assets").glob("*.meta")
             if descriptor_fields(p).get("address") == "UI/Atlases/ProfileButtons"}
    for png in sorted((DROP / "profilebuttons").glob("*.png")):
        what, state = png.stem.rsplit("_", 1)
        name = f"ProfileButtons.{STATE[state]}_{what}"
        meta = metas.get(name)
        if meta is None:
            raise ImportFailure(f"ZONE_1 has no {name}")
        fields = descriptor_fields(meta)
        old_texture = WORKSPACE / "ZONE_1" / fields["texture"]
        rect = tuple(int(float(v)) for v in fields["rect"].split(","))
        # The packed members are uniform padded cells; the drop's images are the trimmed art at
        # 2x. Each is placed into a 2x cell where the old art sat, so layouts stay exact.
        old = crop(old_texture, rect)
        box = old.getchannel("A").getbbox() or (0, 0, rect[2], rect[3])
        scale = 2
        cell = Image.new("RGBA", (rect[2] * scale, rect[3] * scale), (0, 0, 0, 0))
        art = Image.open(png).convert("RGBA").resize(((box[2] - box[0]) * scale, (box[3] - box[1]) * scale), Image.LANCZOS)
        cell.alpha_composite(art, (box[0] * scale, box[1] * scale))
        score = difference(cell, old)
        ppu = float(fields["pixels_per_unit"]) * scale
        builder.notes.append(f"{name}: cell {rect[2]}x{rect[3]}, art {box[2] - box[0]}x{box[3] - box[1]} <- {png.name} "
                             f"{Image.open(png).size}, ppu {fmt(ppu)}, diff {score:.1f}")
        if apply:
            texture = f"textures/ui2026/profilebuttons/{png.name}"
            (WORKSPACE / "ZONE_1" / texture).parent.mkdir(parents=True, exist_ok=True)
            cell.save(WORKSPACE / "ZONE_1" / texture)
            meta.write_text(sprite("UI/Atlases/ProfileButtons", name, texture, cell.width, cell.height, ppu,
                                   int(fields.get("filter", 1))), encoding="utf-8", newline="\n")


def resources_atlas(builder: Builder, atlas: str, members: dict[str, Path | None], apply: bool) -> None:
    """A packed copy of a Resources atlas: drop images where given, recovered members otherwise."""
    for member, png in sorted(members.items()):
        source, rect, ppu, pivot = resources_member(atlas, member)
        name = f"{atlas}.{member}"
        if png is None:
            builder.notes.append(f"{name}: kept recovered member {rect[2]}x{rect[3]}")
            if apply:
                texture = builder.add_texture(source, f"{atlas}/{source.name}")
                size = Image.open(source).size
                builder.write(sprite(f"UI/Atlases/{atlas}", name, texture, size[0], size[1], ppu, rect=rect, tex_size=size, pivot=pivot))
            continue
        new = Image.open(png)
        score = difference(new, crop(source, rect))
        scaled = ppu * new.width / rect[2]
        builder.notes.append(f"{name}: {rect[2]}x{rect[3]} -> {new.width}x{new.height}, ppu {fmt(scaled)}, diff {score:.1f}")
        if apply:
            texture = builder.add_texture(png, f"{atlas}/{png.name}")
            builder.write(sprite(f"UI/Atlases/{atlas}", name, texture, new.width, new.height, scaled, pivot=pivot))


def meshok_mapping() -> dict[str, Path]:
    """Matches the drop's Meshok_* to meshok_1/2/3 by appearance."""
    drop = {p.stem.split("_", 1)[1]: p for p in (DROP / "shopbuttons").glob("Meshok_*.png")}
    result, taken = {}, set()
    scores = []
    for member in ("meshok_1", "meshok_2", "meshok_3"):
        source, rect, _, _ = resources_member("ShopButtons2", member)
        old = crop(source, rect)
        for state, png in drop.items():
            scores.append((difference(Image.open(png), old), member, state))
    for score, member, state in sorted(scores):
        if member in result or state in taken:
            continue
        result[member] = drop[state]
        taken.add(state)
    return result


def shop_atlases() -> tuple[dict[str, Path | None], dict[str, Path | None]]:
    one, two = {}, {}
    for png in sorted((DROP / "shopbuttons").glob("*.png")):
        what, state = png.stem.rsplit("_", 1)
        suffix = {"Inactive": "", "Active": "_active", "Pushed": "_pushed"}[state]
        if what in ("Armor", "Helmet", "Magic", "Ranged_weapon", "Weapon"):
            one[what + suffix] = png
        elif what in ("Free", "Payment"):
            two[what + suffix] = png
    two.update(meshok_mapping())
    return one, two


def single_sprites(builder: Builder, apply: bool) -> None:
    def add(address: str, png: Path, ppu: float, low: bool = False, name: str | None = None) -> None:
        image = Image.open(png)
        builder.notes.append(f"{address}: {image.width}x{image.height}, ppu {fmt(ppu)}" + (" (+_low)" if low else ""))
        if not apply:
            return
        texture = builder.add_texture(png, address.replace("/", "_") + ".png")
        for suffix in ("", "_low") if low else ("",):
            builder.write(sprite(address + suffix, (name or address.rsplit("/", 1)[1]), texture, image.width, image.height, ppu))

    # Versus arrows: same density as the recovered P1 arrow sprite (1 pixel per unit).
    add("Textures/fight/pointers/arrow", DROP / "arrow.png", 1)
    add("Textures/fight/pointers/arrowSecondPlayer", DROP / "arrowSecondPlayer.png", 1)
    # Event avatars at the density of the existing boss/character portraits.
    for png in sorted((DROP / "avatars").glob("*.png")):
        add(f"UI/Users/{png.stem}", png, 200)
    # Map buttons: atlas per state, members "<atlas>.<state>_<icon>", 2x the 300 px event buttons.
    for folder in sorted(DROP.glob("BattleBtnMidAutumn26*")):
        for png in sorted(folder.glob("*.png")):
            image = Image.open(png)
            builder.notes.append(f"UI/Atlases/{folder.name}: {png.stem} {image.width}x{image.height}, ppu 200 (+_low)")
            if apply:
                texture = builder.add_texture(png, f"{folder.name}/{png.name}")
                for suffix in ("", "_low"):
                    builder.write(sprite(f"UI/Atlases/{folder.name}{suffix}", png.stem, texture, image.width, image.height, 200))
    # Stage preview: atlas preview_autumnFest26, member preview_autumnFest26.Tower (same size as other previews).
    png = DROP / "preview_autumnFest26_Tower.png"
    image = Image.open(png)
    builder.notes.append(f"UI/battles/preview_autumnFest26: preview_autumnFest26.Tower {image.width}x{image.height}, ppu 100 (+_low)")
    if apply:
        texture = builder.add_texture(png, png.name)
        for suffix in ("", "_low"):
            builder.write(sprite(f"UI/battles/preview_autumnFest26{suffix}", "preview_autumnFest26.Tower", texture, image.width, image.height, 100))
    # Weapon icons keep the recovered axes icon's width on screen (638 px at 100 ppu). The axes
    # icon already ships in ITEMS: its descriptor there is re-pointed so no copy shadows it.
    png = DROP / "weapon_mercenary_axes.png"
    image = Image.open(png)
    ppu = 100 * image.width / 638
    builder.notes.append(f"UI/Items/weapon_mercenary_axes (ITEMS): {image.width}x{image.height}, ppu {fmt(ppu)}")
    if apply:
        meta = next(p for p in (WORKSPACE / "ITEMS" / "assets").glob("*.meta")
                    if descriptor_fields(p).get("address") == "UI/Items/weapon_mercenary_axes")
        fields = descriptor_fields(meta)
        texture = "textures/ui2026/weapon_mercenary_axes.png"
        (WORKSPACE / "ITEMS" / texture).parent.mkdir(parents=True, exist_ok=True)
        shutil.copyfile(png, WORKSPACE / "ITEMS" / texture)
        meta.write_text(sprite("UI/Items/weapon_mercenary_axes", fields["name"], texture, image.width, image.height, ppu,
                               int(fields.get("filter", 1))), encoding="utf-8", newline="\n")
    add("UI/Items/weapon_super_cudgel", DROP / "weapon_super_cudgel.png", 100 * Image.open(DROP / "weapon_super_cudgel.png").width / 638)


def payloads(builder: Builder, apply: bool) -> None:
    music = DROP / "cursed_brothers_mafest26.wav"
    builder.notes.append(f"gamedata/music/{music.stem}: {music.stat().st_size} bytes")
    if apply:
        builder.add_payload("audio", f"gamedata/music/{music.stem}", music.stem, music, "audio")
    for xml in sorted(DROP.glob("mdl_*.xml")):
        builder.notes.append(f"gamedata/models/{xml.stem}")
        if apply:
            builder.add_payload("model", f"gamedata/models/{xml.stem}", xml.stem, xml, "models")


def run(apply: bool) -> int:
    if WORKSPACE.exists():
        shutil.rmtree(WORKSPACE)
    packer("unpack-all", str(WORKSPACE), "--only", "ZONE_1,ITEMS")
    builder = Builder()
    profile_buttons(builder, apply)
    one, two = shop_atlases()
    resources_atlas(builder, "ShopButtons1", one, apply)
    resources_atlas(builder, "ShopButtons2", two, apply)
    resources_atlas(builder, "SlidersSettings", {"slider": DROP / "slider.png", "full": None, "SettingsEmpty": None}, apply)
    single_sprites(builder, apply)
    payloads(builder, apply)
    print("\n".join(builder.notes))
    if not apply:
        return 0
    catalog = json.loads(CATALOG.read_text(encoding="utf-8-sig"))
    taken = {a["address"].lower() for g in catalog["bundles"] for a in g["assets"] if g["name"] != GROUP}
    ours = {descriptor_fields(p)["address"].lower() for p in (builder.group / "assets").glob("*.meta")}
    clash = sorted(ours & taken)
    if clash:
        raise ImportFailure("addresses already in other groups (would be shadowed): " + ", ".join(clash))
    packer("check", str(WORKSPACE))
    packer("repack-all", str(WORKSPACE))
    print(f"installed {len(list((builder.group / 'assets').glob('*.meta')))} descriptors in {GROUP}")
    return 0


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__.splitlines()[0])
    parser.add_argument("command", choices=("plan", "apply"))
    args = parser.parse_args()
    try:
        return run(args.command == "apply")
    except ImportFailure as failure:
        print("ERROR:", failure, file=sys.stderr)
        return 1


if __name__ == "__main__":
    sys.exit(main())
