"""Install the seven owner-supplied maps as core art, preserving map lookup names.

Usage: python Tools/Recovery/ImportOverworldMaps.py plan | apply | check
Inputs: ResearchSources/maps/Map01.png through Map07.png (2040x972).
Only apply writes Assets; staging is under Temp/OverworldMapImport.
Runtime descriptors use Unity's full-rect Sprite.Create, with no authored mesh.
"""

from __future__ import annotations

import argparse
import json
import shutil
from pathlib import Path

from ImportUpscaledLocations import CATALOG, ROOT, packer, png_size, sha256

ART = ROOT / "Assets/StreamingAssets/SF2Content/ArtBundles"
DROP = ROOT / "ResearchSources/maps"
WORK = ROOT / "Temp/OverworldMapImport"
# Existing atlas members, including the act-one loose fallback and act-seven's
# standalone name used by ZoneScrollItem. Keep both normal and legacy low paths.
TARGETS = {
    "ZONE_1": [("Map1", "Map1.1", 1), ("Map1", "Map1.2", 2),
               ("Map1", "Map1.3", 3), ("Map2", "Map2.4", 4),
               ("Map2", "Map2.5", 5), ("Map2", "Map2.6", 6)],
    "ZONE_6": [("Map3", "Map3.7", 7)],
    "ZONES": [("7", "7", 7)],
}


def descriptor(atlas: str, name: str, act: int) -> bytes:
    # Maintain the old sprite's world-space width; map UI uses its existing fixed
    # RectTransform, so battle coordinates and scroll spacing remain unchanged.
    width = 683 if atlas.endswith("_low") else 1365
    ppu = 100 * 2040 / width
    return ("type=sprite\nnamespace=core\n"
            f"address=UI/zones/{atlas}\nname={name}\n"
            f"texture=textures/overworld/Map{act:02}.png\n"
            f"rect=0,0,2040,972\npivot=0.5,0.5\nborder=0,0,0,0\n"
            f"pixels_per_unit={ppu:.12g}\nfilter=1\naniso=1\n"
            "wrap_u=1\nwrap_v=1\nmipmaps=false\n").encode()


def rows(group: str) -> list[tuple[str, str, int]]:
    result = list(TARGETS[group])
    if group != "ZONES":
        result += [(atlas + "_low", name, act) for atlas, name, act in result]
    return result


def index(tree: Path) -> dict[tuple[str, str], Path]:
    result = {}
    for path in tree.rglob("*.meta"):
        values = dict(line.split("=", 1) for line in path.read_text().splitlines() if "=" in line)
        key = (values.get("address", "").lower(), values.get("name", "").lower())
        if key in result:
            raise RuntimeError(f"Duplicate descriptor: {key}")
        result[key] = path
    return result


def run(mode: str) -> None:
    sources = sorted(DROP.glob("*.png"))
    expected = [DROP / f"Map{act:02}.png" for act in range(1, 8)]
    if sources != expected or any(png_size(path) != (2040, 972) for path in sources):
        raise RuntimeError("Expected exactly Map01.png through Map07.png, each 2040x972; review changed input")
    catalog = json.loads(CATALOG.read_text(encoding="utf-8-sig"))
    staged = []
    for group_name in TARGETS:
        group = next(g for g in catalog["bundles"] if g["name"] == group_name)
        archive = ART / group["file"]
        if sha256(archive) != group["sha256"] or archive.stat().st_size != group["size"]:
            raise RuntimeError(f"Archive does not match catalog: {archive}")
        tree = WORK / mode / group_name
        if tree.exists():
            # Resolve before recursive removal; never follow a staging path out of Temp.
            if not tree.resolve().is_relative_to((ROOT / "Temp").resolve()):
                raise RuntimeError(f"Unsafe staging directory: {tree}")
            shutil.rmtree(tree)
        packer("extract", str(archive), str(tree))
        metas = index(tree)
        addresses = {a["address"].lower() for a in group["assets"]}
        for atlas, name, act in rows(group_name):
            address = f"UI/zones/{atlas}"
            key = (address.lower(), name.lower())
            meta = metas.get(key)
            # These three descriptors were loose-only in the old core content.
            can_add = name in ("Map1.1", "7")
            if meta is None and not can_add:
                raise RuntimeError(f"Missing original map member: {key}")
            texture = tree / f"textures/overworld/Map{act:02}.png"
            body = descriptor(atlas, name, act)
            if mode == "check":
                if meta is None or meta.read_bytes() != body:
                    raise RuntimeError(f"Missing/stale installed descriptor: {key}")
                if sha256(texture) != sha256(DROP / f"Map{act:02}.png"):
                    raise RuntimeError(f"Installed pixels differ: {texture}")
                if address.lower() not in addresses:
                    raise RuntimeError(f"Uncatalogued map address: {address}")
            else:
                meta = meta or tree / "assets" / f"overworld_{atlas}_{name}.meta"
                meta.write_bytes(body)
                texture.parent.mkdir(parents=True, exist_ok=True)
                shutil.copyfile(DROP / f"Map{act:02}.png", texture)
                if address.lower() not in addresses:
                    group["assets"].append(dict(address=address, texture="", sprites="", audio="", font=""))
                    addresses.add(address.lower())
            print(f"{group_name}: Map{act:02}.png -> {address} [{name}]")
        if mode == "apply":
            output = WORK / mode / (group_name + ".tar.lz4")
            packer("pack", str(tree), str(output))
            packer("verify", str(output))
            info = dict(line.split("=", 1) for line in packer("info", str(output)).splitlines() if "=" in line)
            group.update(size=int(info["size"]), unpackedSize=int(info["unpackedSize"]), sha256=info["sha256"])
            staged.append((output, archive))
    # Validate every staged archive before replacing the installed content.
    if mode == "apply":
        for output, archive in staged:
            shutil.copyfile(output, archive)
        CATALOG.write_text(json.dumps(catalog, indent=2) + "\n", encoding="utf-8")
    print(f"{mode.upper()}: 7 maps, 15 descriptors, 3 existing archives; Unity .meta files untouched.")


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("mode", choices=("plan", "apply", "check"))
    run(parser.parse_args().mode)
