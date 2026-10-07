"""Resolve native moves the way the runtime loads them, for queries and editor data.

Mirrors ResourceManager.NormalizeMoves (MoveCompatibility) and MovesParser
template inheritance: attributes are inherited only when the move omits them,
while child sections (intervals, actions, conditions, events, locks) are the
move's own entries followed by each applied template's entries in depth-first
order. Imported legacy moves resolve against the bundled legacy templates.
This is static XML evidence; it does not execute parsers or prove gameplay.
"""
import copy
from pathlib import Path
import xml.etree.ElementTree as ET

ROOT = Path(__file__).resolve().parents[2]
VANILLA_MOVES = ROOT / "Assets/vanillaXml/animations/moves.xml"
LEGACY_MOVES = ROOT / "Assets/Resources/gamedata/animations/moves.txt"
SECTIONS = ("Templates", "Moves", "Triggers")


def _section(root, name):
    found = root.find(name)
    return found if found is not None else []


def _named(section):
    return [node for node in section if node.get("Name") is not None] if section is not None else []


def load_document(vanilla=VANILLA_MOVES, legacy=LEGACY_MOVES):
    """Return (root, legacy_templates) after MoveCompatibility normalization."""
    root = ET.parse(vanilla).getroot()
    legacy_templates = {}
    if legacy is not None and Path(legacy).exists():
        baseline = ET.parse(legacy).getroot()
        restored = 0
        for name in SECTIONS:
            target, source = root.find(name), baseline.find(name)
            if target is None or source is None:
                continue
            names = {node.get("Name") for node in _named(target)}
            for node in _named(source):
                if node.get("Name") in names:
                    continue
                names.add(node.get("Name"))
                imported = copy.deepcopy(node)
                if name == "Moves":
                    imported.set("UseLegacyTemplates", "1")
                target.append(imported)
                restored += 1
        if restored:
            legacy_templates = {node.get("Name"): copy.deepcopy(node) for node in _named(baseline.find("Templates"))}
    templates_section = root.find("Templates")
    available = {node.get("Name") for node in _named(templates_section) if node.get("Name")}
    for node in _named(templates_section) + list(_section(root, "Moves")):
        value = node.get("Template")
        if value is None:
            continue
        kept = [name for name in value.split("|") if name in available]
        if len(kept) == len([name for name in value.split("|") if name]):
            continue
        if kept:
            node.set("Template", "|".join(kept))
        else:
            del node.attrib["Template"]
    return root, legacy_templates


def _applied_templates(move, templates):
    applied, order = set(), []

    def visit(node):
        value = node.get("Template")
        if value is None:
            return
        for name in value.split("|"):
            if name in applied or name not in templates:
                continue
            applied.add(name)
            order.append(templates[name])
            visit(templates[name])

    visit(move)
    return order


def _integer(value, fallback):
    if value is None:
        return fallback
    try:
        return int(value)
    except ValueError:
        return fallback


def _children(nodes, section):
    result = []
    for node in nodes:
        found = node.find(section)
        if found is not None:
            result.extend(list(found))
    return result


def resolve_moves(root=None, legacy_templates=None):
    """Return resolved moves in runtime order, as plain dictionaries."""
    if root is None:
        root, legacy_templates = load_document()
    modern = {node.get("Name"): node for node in _named(root.find("Templates"))}
    legacy_templates = legacy_templates or {}
    moves = []
    for move in _section(root, "Moves"):
        templates = legacy_templates if move.get("UseLegacyTemplates") == "1" else modern
        applied = _applied_templates(move, templates)
        attributes = dict(move.attrib)
        for template in applied:
            for key, value in template.attrib.items():
                attributes.setdefault(key, value)
        sources = [move] + applied
        intervals = []
        for node in _children(sources, "Intervals"):
            hits = [{"name": hit.get("Name"), "start": hit.get("Start"), "end": hit.get("End")}
                    for hit in node.findall("Hit")]
            interval = {
                "name": node.get("Name"), "type": node.get("Type"),
                "start": _integer(node.get("Start"), 0), "end": _integer(node.get("End"), -1),
                "hits": hits,
            }
            if node.get("Type") == "Attack":
                damage = node.find("Damage")
                impulse = node.find("Impulse")
                parts = node.find("AttackingParts")
                interval["attack"] = {
                    "id": _integer(node.get("ID"), -1),
                    "damage": float(damage.get("Value", "0")) if damage is not None else None,
                    "terms": {term.get("Type", ""): float(term.get("Shift", "0")) for term in damage.findall("Damage")} if damage is not None else {},
                    "edges": [edge.get("Name", "") for edge in parts] if parts is not None else [],
                    "impulse": [float(impulse.get(axis, "0")) if impulse is not None else 0.0 for axis in ("X", "Y", "Z")],
                }
            intervals.append(interval)
        sounds = [{"name": node.get("Name"), "frame": _integer(node.get("Frame"), None) if node.get("Frame") is not None else None}
                  for node in _children(sources, "Actions") if node.tag == "Sound"]
        # Positive weapon lock groups (direct Item or top-level Or), as subtype lists.
        weapon_locks = []
        locks = move.find("Locks")
        for clause in (list(locks) if locks is not None else []):
            items = [clause] if clause.tag == "Item" else list(clause) if clause.tag == "Operator" and clause.get("Type") == "Or" and clause.get("Not") != "1" else []
            subtypes = [i.get("SubType") for i in items if i.tag == "Item" and i.get("Type") == "Weapon" and i.get("SubType") and not i.get("Name") and i.get("Not") != "1"]
            if subtypes and clause.get("Not") != "1":
                weapon_locks.append(subtypes)
        keys = []
        for node in _children(sources, "Conditions"):
            if node.tag == "Keys":
                keys.append([{"type": key.get("Type"), "press": key.get("PressType")} for key in node.findall("Key")])
        moves.append({
            "name": attributes.get("Name", ""),
            "file": attributes.get("FileName", ""),
            "priority": _integer(attributes.get("Priority"), 0),
            "mid_frames": _integer(attributes.get("MidFrames"), 0),
            "looped": attributes.get("Looped") in ("1", "true"),
            "physics": attributes.get("Physics") in ("1", "true"),
            "type": attributes.get("Type", ""),
            "templates": [template.get("Name") for template in applied],
            "legacy": move.get("UseLegacyTemplates") == "1",
            "intervals": intervals,
            "sounds": sounds,
            "keys": keys,
            "weapon_locks": weapon_locks,
            "node": move,
            "sources": sources,
        })
    return moves


def template_names(root=None):
    if root is None:
        root, _ = load_document()
    return [node.get("Name") for node in _named(root.find("Templates"))]


def editor_index(moves, templates):
    """Compact JSON-ready data consumed by Tools/ModdingEditor."""
    entries = {}
    duplicates = set()
    for move in moves:
        if move["name"] in entries:
            duplicates.add(move["name"])
        record = {"file": move["file"], "priority": move["priority"], "mid": move["mid_frames"]}
        if move["looped"]:
            record["looped"] = True
        if move["physics"]:
            record["physics"] = True
        if move["type"]:
            record["type"] = move["type"]
        if move["templates"]:
            record["templates"] = move["templates"]
        if move["legacy"]:
            record["legacy"] = True
        if move["intervals"]:
            record["intervals"] = [[i["name"], i["type"], i["start"], i["end"]] +
                                   ([[h["name"], h["start"], h["end"]] for h in i["hits"]] if i["hits"] else [])
                                   for i in move["intervals"]]
            attacks = [i["attack"] for i in move["intervals"] if "attack" in i]
            if attacks:
                record["attacks"] = [[a["id"], a["damage"], a["terms"], a["edges"], a["impulse"]] for a in attacks]
        if move["sounds"]:
            record["sounds"] = [[s["name"], s["frame"]] for s in move["sounds"]]
        if move["weapon_locks"]:
            record["weapons"] = move["weapon_locks"]
        if move["keys"]:
            record["keys"] = [[[k["type"], k["press"]] for k in group] for group in move["keys"]]
        entries[move["name"]] = record
    return {
        "schema": 2,
        "source": "Assets/vanillaXml/animations/moves.xml + legacy baseline; generated by Tools/Audits/QueryMoves.py export-index",
        "duplicates": sorted(duplicates),
        "templates": sorted(set(templates)),
        "moves": dict(sorted(entries.items())),
    }
