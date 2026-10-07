#!/usr/bin/env python3
"""Replace every fight location's art with the owner's 2026-10 drop and retire the old art.

Source: ResearchSources/new_assets/new_assets/locations/<id>_new/*.png (and whisper_raid/),
owner-supplied 2026-10-07, plus ResearchSources/locs remaining/<id>_new/ (dojo_india25 and
hunter_raid, supplied later). The drops hold one PNG per picture and no layouts. This tool:

* removes every old location picture and atlas from the art bundles (``Textures/Locations/**``,
  including the Cocos atlas data in LOCATION_DATA), except animated effect atlases under
  ``Textures/Locations/<id>/atlases/`` that layouts still play and the drop does not replace;
* adds each drop PNG as a full-quad sprite at ``Textures/Locations/<id>/<png stem>`` in
  CORE_LOCATIONS, with the drop's own import settings (filter, pivot, 1 pixel per unit);
* installs layouts (``Assets/vanillaXml/locations/<id>/<id>_params.xml`` and the
  ``Assets/Resources/gamedata/locations/<id>/params.txt`` fallback):
  - "upscaled": the Definitive Edition layout for the upscaled art from the earlier drop
    (ResearchSources/de128_assets/gamedata/locations/<id>_new/), as ImportUpscaledLocations.py
    installed it. A picture whose PNG changed size since that drop keeps its pixel scale: its
    box grows or shrinks with the image (a ``left``/``right`` frame keeps its outer edge).
  - "converted": the location's current layout (or the earlier drop's plain layout when none is
    installed) with the ``Atlas`` attribute removed from its own layers, so each picture loads
    the drop's single PNG of the same name into the same box. LAYOUT_REVIEW lists the ones
    whose art does not fit that layout.
* removes locations that have no art left (no drop folder and not drawn from one), with their
  versus roster arenas.

It never writes Unity YAML or sprite vertex data from Unity assets; descriptors are the runtime
TAR format documented in Tools/AssetPacker/README.md.

Usage:
    python Tools/Recovery/ImportLocationArt2026.py plan    # report, change nothing
    python Tools/Recovery/ImportLocationArt2026.py apply   # rewrite bundles, catalog, layouts
    python Tools/Recovery/ImportLocationArt2026.py check   # verify the installed result
"""

from __future__ import annotations

import argparse
import json
import re
import shutil
import struct
import subprocess
import sys
import xml.etree.ElementTree as ET  # parses only the owner's local drop and repository files
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
DROPS = (ROOT / "ResearchSources" / "new_assets" / "new_assets" / "locations",
         ROOT / "ResearchSources" / "locs remaining")
DE_DROP = ROOT / "ResearchSources" / "de128_assets"
DE_ART = DE_DROP / "assets" / "Locations"
DE_PARAMS = DE_DROP / "gamedata" / "locations"
VANILLA = ROOT / "Assets" / "vanillaXml" / "locations"
RESOURCES = ROOT / "Assets" / "Resources" / "gamedata" / "locations"
ROSTER = ROOT / "Assets" / "Resources" / "EclipseVersus" / "roster.json"
WORK = ROOT / "Library" / "LocationArt2026"
WORKSPACE = WORK / "ws"
PACKER = ROOT / "Tools" / "AssetPacker" / "bin" / "Release" / "net9.0" / "AssetPacker.dll"
GROUP = "CORE_LOCATIONS"
TEXTURES = "textures/locations_2026"

# Ids that are not locations of their own: Location.cs redirects them to another folder.
REDIRECTED = {"bridge", "emerald_forest_new"}
# Layout fixes the earlier importer applied to the DE params (names that exist nowhere).
RENAMES = {
    "waterfall_small": {
        'ClassName="waterfall_new_2"': 'ClassName="waterfall_2"',
        'ClassName="waterfall_new_left_tile"': 'ClassName="waterfall_left_tile"',
        'ClassName="waterfall_new_right_tile"': 'ClassName="waterfall_right_tile"',
    },
}
PATH_NEW = re.compile(r'(Path="Locations/[A-Za-z0-9_]+?)_new/"')


class ImportFailure(RuntimeError):
    pass


def packer(*args: str) -> str:
    result = subprocess.run(["dotnet", str(PACKER), *args], capture_output=True, text=True, cwd=ROOT)
    if result.returncode != 0:
        raise ImportFailure("AssetPacker " + args[0] + " failed:\n" + (result.stdout + result.stderr).strip())
    return result.stdout


def png_size(path: Path) -> tuple[int, int]:
    with path.open("rb") as handle:
        header = handle.read(24)
    if header[:8] != b"\x89PNG\r\n\x1a\n" or header[12:16] != b"IHDR":
        raise ImportFailure(f"Not a PNG: {path}")
    return struct.unpack(">II", header[16:24])


def drop_folders() -> dict[str, Path]:
    """Location id -> drop folder. A location in a later drop must not repeat an earlier one."""
    result = {}
    for drop in DROPS:
        if not drop.is_dir():
            continue
        for folder in sorted(p for p in drop.iterdir() if p.is_dir()):
            location = folder.name[:-4] if folder.name.endswith("_new") else folder.name
            if location in result:
                raise ImportFailure(f"{location} is in two drops: {result[location]} and {folder}")
            result[location] = folder
    return result


def import_settings(png: Path) -> tuple[int, str]:
    """(filter mode, pivot) from the drop's Unity .meta, as the drop's own importer used them."""
    meta = png.with_name(png.name + ".meta")
    text = meta.read_text(encoding="utf-8") if meta.is_file() else ""
    match = re.search(r"filterMode: (\d+)", text)
    pivot = re.search(r"spritePivot: \{x: ([0-9.]+), y: ([0-9.]+)\}", text)
    return int(match.group(1)) if match else 0, (f"{pivot.group(1)},{pivot.group(2)}" if pivot else "0.5,0.5")


def descriptor(address: str, name: str, texture: str, width: int, height: int, filter_mode: int, pivot: str) -> str:
    fmt = lambda v: str(int(v)) if float(v).is_integer() else repr(v)
    px, py = (float(v) for v in pivot.split(","))
    left, right, top, bottom = -width * px, width * (1 - px), height * (1 - py), -height * py
    return (
        "type=sprite\nnamespace=core\n"
        f"address={address}\nname={name}\ntexture={texture}\n"
        f"rect=0,0,{width},{height}\npivot={pivot}\nborder=0,0,0,0\npixels_per_unit=1\n"
        f"filter={filter_mode}\naniso=1\nwrap_u=1\nwrap_v=1\nmipmaps=false\n"
        f"vertices={fmt(left)},{fmt(top)};{fmt(right)},{fmt(top)};{fmt(left)},{fmt(bottom)};{fmt(right)},{fmt(bottom)}\n"
        "triangles=0,1,2,2,1,3\nuv=0,1;1,1;0,0;1,0\n"
    )


# ---- Layouts ----

def installed_ids() -> set[str]:
    ids = {p.name for p in VANILLA.iterdir() if p.is_dir()} if VANILLA.is_dir() else set()
    ids |= {p.name for p in RESOURCES.iterdir() if p.is_dir()} if RESOURCES.is_dir() else set()
    return ids


def installed_layout(location: str) -> str | None:
    for path in (VANILLA / location / f"{location}_params.xml", RESOURCES / location / "params.txt"):
        if path.is_file():
            return path.read_text(encoding="utf-8-sig")
    return None


def upscaled_layout(location: str) -> str | None:
    path = DE_PARAMS / f"{location}_new" / f"{location}_new_params.xml"
    if not path.is_file():
        return None
    text = path.read_text(encoding="utf-8-sig")
    for old, new in RENAMES.get(location, {}).items():
        if old not in text:
            raise ImportFailure(f"{location}: expected {old} in the DE params")
        text = text.replace(old, new)
    return PATH_NEW.sub(r'\1/"', text)


def layer_folder(location: str, layer: ET.Element) -> str | None:
    """The Textures/Locations folder a layer draws from, or None for another texture root."""
    path = layer.get("Path")
    if not path:
        return location
    parts = path.strip("/").split("/")
    return parts[1] if len(parts) == 2 and parts[0] == "Locations" else None


def pictures(location: str, text: str) -> list[tuple[str, str, ET.Element]]:
    """(folder, ClassName, element) for every picture a layout draws from location art."""
    result = []
    for layer in ET.fromstring(text.encode("utf-8")).iter("Layer"):
        folder = layer_folder(location, layer)
        if folder is None:
            continue
        for element in layer.iter():
            name = element.get("ClassName")
            if not name:
                continue
            if element.tag in ("Image", "SpriteMask") or (
                    element.tag == "SimpleEffect" and element.get("Type") == "Picture"
                    and element.get("PictureLocation", "local") == "local"):
                result.append((folder, name, element))
    return result


def rescale_changed(location: str, text: str, art: dict[str, dict[str, Path]]) -> tuple[str, list[str]]:
    """Keeps the pixel scale of pictures whose PNG changed size since the DE layout was made."""
    notes = []
    changed = {}
    for png in (art.get(location) or {}).values():
        old = DE_ART / f"{location}_new" / png.name
        if old.is_file() and png_size(old) != png_size(png):
            changed[png.stem] = (png_size(old), png_size(png))
    if not changed:
        return text, notes

    def fix(match: re.Match) -> str:
        tag = match.group(0)
        name = re.search(r'ClassName="([^"]+)"', tag)
        if not name or name.group(1) not in changed or re.search(r'\bPath="', tag):
            return tag
        (ow, oh), (nw, nh) = changed[name.group(1)]
        attrs = {k: float(v) for k, v in re.findall(r'\b(X|Width|Height)="(-?[0-9.]+)"', tag)}
        if "Width" not in attrs or "Height" not in attrs:
            return tag
        width, height = attrs["Width"] * nw / ow, attrs["Height"] * nh / oh
        x = attrs.get("X", 0.0)
        stem = name.group(1).lower()
        # Side frames keep their outer (screen) edge; everything else stays centred.
        if stem.startswith("left"):
            x -= (attrs["Width"] - width) / 2
        elif stem.startswith("right"):
            x += (attrs["Width"] - width) / 2
        fmt = lambda v: f"{v:.4f}".rstrip("0").rstrip(".")
        tag = re.sub(r'\bWidth="[^"]*"', f'Width="{fmt(width)}"', tag)
        tag = re.sub(r'\bHeight="[^"]*"', f'Height="{fmt(height)}"', tag)
        if "X" in attrs:
            tag = re.sub(r'\bX="[^"]*"', f'X="{fmt(x)}"', tag)
        notes.append(f"{name.group(1)} {ow}x{oh}->{nw}x{nh}: box {attrs['Width']:g}x{attrs['Height']:g} -> {fmt(width)}x{fmt(height)}")
        return tag

    return re.sub(r"<(?:Image|SpriteMask|SimpleEffect)\b[^>]*>", fix, text), notes


def strip_atlases(location: str, text: str) -> str:
    """Removes Atlas="" from layers drawing location art, so pictures load single sprites."""
    def fix(match: re.Match) -> str:
        tag = match.group(0)
        path = re.search(r'\bPath="([^"]*)"', tag)
        parts = path.group(1).strip("/").split("/") if path else ["Locations", location]
        if len(parts) == 2 and parts[0] == "Locations":
            tag = re.sub(r'\s+Atlas="[^"]*"', "", tag)
        return tag
    return re.sub(r"<Layer\b[^>]*>", fix, text)


def plan_layouts(art: dict[str, dict[str, Path]]) -> tuple[dict[str, str], dict[str, str], list[str]]:
    """(location -> layout text, location -> kind, notes) for every location that keeps a layout."""
    layouts, kinds, notes = {}, {}, []
    candidates = set(art) | installed_ids()
    for location in sorted(candidates - REDIRECTED):
        text = upscaled_layout(location)
        if text is not None:
            text, changed = rescale_changed(location, text, art)
            layouts[location], kinds[location] = text, "upscaled"
            notes += [f"{location}: {n}" for n in changed]
            continue
        if location not in art:
            continue
        text = installed_layout(location)
        if text is None:
            plain = DE_PARAMS / location / f"{location}_params.xml"
            text = plain.read_text(encoding="utf-8-sig") if plain.is_file() else None
        if text is None:
            kinds[location] = "no layout"
            continue
        layouts[location], kinds[location] = strip_atlases(location, text), "converted"
    # A location without its own art survives only if its layout draws drop art (a variant).
    for location in list(layouts):
        if location in art:
            continue
        drawn = [(f, n) for f, n, _ in pictures(location, layouts[location])]
        if not drawn or not all(n in art.get(f, {}) for f, n in drawn if not n.startswith("bird_")):
            del layouts[location]
            kinds.pop(location, None)
    return layouts, kinds, notes


# ---- Bundles ----

def old_location_addresses(catalog: dict) -> list[str]:
    result = []
    for group in catalog["bundles"]:
        for asset in group["assets"]:
            address = asset["address"]
            low = address.lower()
            if low.startswith("textures/locations/") and "/atlases/" not in low:
                result.append(address)
    return sorted(set(result))


def build_workspace(art: dict[str, dict[str, Path]], report: list[str]) -> dict[str, str]:
    catalog = json.loads((ROOT / "Assets" / "Resources" / "SF2Content" / "Art" / "catalog.json").read_text(encoding="utf-8-sig"))
    if WORKSPACE.exists():
        shutil.rmtree(WORKSPACE)
    groups = sorted({g["name"] for g in catalog["bundles"]
                     if any(a["address"].lower().startswith("textures/locations/") for a in g["assets"])} | {GROUP})
    packer("unpack-all", str(WORKSPACE), "--only", ",".join(groups))
    old = old_location_addresses(catalog)
    listing = WORK / "old_location_addresses.txt"
    listing.write_text("\n".join(old) + "\n", encoding="utf-8")
    packer("delete", str(WORKSPACE), "@" + str(listing))
    report.append(f"removed {len(old)} old location addresses from {len(groups)} groups")

    tree = WORKSPACE / GROUP
    (tree / "assets").mkdir(parents=True, exist_ok=True)
    index = max((int(p.stem) for p in (tree / "assets").glob("*.meta") if p.stem.isdigit()), default=-1) + 1
    installed = {}
    for location, pngs in sorted(art.items()):
        for stem, png in sorted(pngs.items()):
            texture = f"{TEXTURES}/{location}/{png.name}"
            target = tree / texture
            target.parent.mkdir(parents=True, exist_ok=True)
            shutil.copyfile(png, target)
            width, height = png_size(png)
            filter_mode, pivot = import_settings(png)
            address = f"Textures/Locations/{location}/{stem}"
            (tree / "assets" / f"{index:06d}.meta").write_text(
                descriptor(address, stem, texture, width, height, filter_mode, pivot), encoding="utf-8", newline="\n")
            index += 1
            installed[address.lower()] = texture
    report.append(f"added {len(installed)} pictures for {len(art)} locations")
    packer("prune", str(WORKSPACE))
    packer("check", str(WORKSPACE))
    return installed


# ---- Plan / apply / check ----

def collect() -> dict[str, dict[str, Path]]:
    return {location: {p.stem: p for p in sorted(folder.glob("*.png"))} for location, folder in drop_folders().items()}


def review(art, layouts, kinds) -> tuple[list[str], list[str]]:
    """(unresolved pictures, layout review lines)."""
    unresolved, lines = [], []
    for location, text in sorted(layouts.items()):
        missing = sorted({f"{f}/{n}" for f, n, _ in pictures(location, text) if n not in art.get(f, {}) and not n.startswith("bird_")})
        if missing:
            unresolved.append(f"{location} ({kinds[location]}): " + ", ".join(missing))
    for location, pngs in sorted(art.items()):
        if kinds.get(location) == "no layout":
            lines.append(f"{location}: no layout at all")
            continue
        text = layouts.get(location)
        if text is None:
            continue
        used = {n for f, n, _ in pictures(location, text) if f == location}
        unused = sorted(n for n in set(pngs) - used if not n.startswith("pixel"))  # fill pixels are optional
        misfit = []
        for f, n, element in pictures(location, text):
            png = art.get(f, {}).get(n)
            if png is None or n.startswith("pixel") or element.get("Width") is None or element.get("Height") is None:
                continue
            w, h = png_size(png)
            box_w, box_h = float(element.get("Width")), float(element.get("Height"))
            if box_w > 0 and box_h > 0 and abs((w / h) / (box_w / box_h) - 1) > 0.05:
                misfit.append(n)
        if kinds[location] == "converted" and (unused or misfit):
            parts = []
            if misfit:
                parts.append("does not fit its box: " + ", ".join(sorted(set(misfit))))
            if unused:
                parts.append("not placed: " + ", ".join(unused))
            lines.append(f"{location}: " + "; ".join(parts))
    return unresolved, lines


def removed_locations(layouts) -> list[str]:
    return sorted(installed_ids() - set(layouts) - REDIRECTED)


def plan() -> int:
    art = collect()
    layouts, kinds, notes = plan_layouts(art)
    unresolved, lines = review(art, layouts, kinds)
    upscaled = sorted(l for l, k in kinds.items() if k == "upscaled")
    converted = sorted(l for l, k in kinds.items() if k == "converted")
    print(f"drop: {len(art)} locations, {sum(len(v) for v in art.values())} pictures")
    print(f"upscaled layouts ({len(upscaled)}): {' '.join(upscaled)}")
    print(f"converted layouts ({len(converted)}): {' '.join(converted)}")
    print(f"no layout: {' '.join(sorted(l for l, k in kinds.items() if k == 'no layout'))}")
    print(f"removed locations: {' '.join(removed_locations(layouts))}")
    for note in notes:
        print("  rescaled", note)
    print("LAYOUT_REVIEW")
    for line in lines:
        print("  " + line)
    print("UNRESOLVED pictures (skipped by Location.cs at runtime)")
    for line in unresolved:
        print("  " + line)
    return 0


def write_layouts(layouts: dict[str, str]) -> None:
    for location, text in layouts.items():
        for target in (VANILLA / location / f"{location}_params.xml", RESOURCES / location / "params.txt"):
            target.parent.mkdir(parents=True, exist_ok=True)
            target.write_text(text, encoding="utf-8", newline="\n")


def remove_locations(locations: list[str]) -> None:
    for location in locations:
        for base in (VANILLA, RESOURCES):
            folder = base / location
            if folder.is_dir():
                shutil.rmtree(folder)
            meta = base / (location + ".meta")
            if meta.is_file():
                meta.unlink()
    text = ROSTER.read_text(encoding="utf-8-sig")
    for location in locations:
        # One arena per line in the roster file: drop that line only.
        text = re.sub(r'\n[ \t]*\{ *"id": *"' + re.escape(location) + r'"[^\n]*\},?(?=\n)', "", text)
    text = re.sub(r",(\s*\])", r"\1", text)
    json.loads(text)
    ROSTER.write_text(text, encoding="utf-8", newline="\n")


def apply() -> int:
    art = collect()
    layouts, kinds, notes = plan_layouts(art)
    report: list[str] = []
    build_workspace(art, report)
    packer("repack-all", str(WORKSPACE))
    write_layouts(layouts)
    removed = removed_locations(layouts)
    remove_locations(removed)
    # Arenas whose location no longer exists (removed by an earlier run) leave the roster too.
    roster = json.loads(ROSTER.read_text(encoding="utf-8-sig"))
    stale = [a["id"] for a in roster.get("arenas", []) if a["id"] not in layouts and a["id"] not in REDIRECTED]
    if stale:
        remove_locations(stale)
        print("removed stale versus arenas: " + " ".join(stale))
    print("\n".join(report))
    print(f"installed {len(layouts)} layouts; removed {len(removed)} locations: {' '.join(removed)}")
    return check()


def check() -> int:
    failures = []
    art = collect()
    catalog = json.loads((ROOT / "Assets" / "Resources" / "SF2Content" / "Art" / "catalog.json").read_text(encoding="utf-8-sig"))
    addresses = {a["address"].lower(): g["name"] for g in catalog["bundles"] for a in g["assets"]}
    for location, pngs in art.items():
        for stem in pngs:
            if addresses.get(f"textures/locations/{location}/{stem}".lower()) != GROUP:
                failures.append(f"catalog lacks {location}/{stem} in {GROUP}")
    stale = [a for a in addresses if a.startswith("textures/locations/") and "/atlases/" not in a
             and a.split("/")[3] if len(a.split("/")) > 3]
    for address in stale:
        parts = address.split("/")
        if parts[2] not in {l.lower() for l in art} or parts[3] not in {s.lower() for s in art.get(next((l for l in art if l.lower() == parts[2]), ""), {})}:
            failures.append(f"old location address still in catalog: {address}")
    bundles = ROOT / "Assets" / "StreamingAssets" / "SF2Content" / "ArtBundles"
    for group in catalog["bundles"]:
        path = bundles / group["file"]
        if group["file"] and (not path.is_file() or path.stat().st_size != group["size"]):
            failures.append(f"bundle size differs from catalog: {group['file']}")
    for failure in failures[:50]:
        print("FAIL", failure)
    print("location art " + ("verified" if not failures else f"FAILED ({len(failures)})"))
    return 1 if failures else 0


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__.splitlines()[0])
    parser.add_argument("command", choices=("plan", "apply", "check"))
    args = parser.parse_args()
    try:
        return {"plan": plan, "apply": apply, "check": check}[args.command]()
    except ImportFailure as failure:
        print("ERROR:", failure, file=sys.stderr)
        return 1


if __name__ == "__main__":
    sys.exit(main())
