"""Query native moves after runtime normalization and template inheritance.

Answers corpus questions such as "which moves use this condition" or "which
values does this attribute take" before a patch operation or hook is designed.
Counts include template-inherited elements unless --own is passed. Read-only,
except export-index, which writes the VS Code extension's native move data.
"""
import argparse
from collections import Counter, defaultdict
import json
from pathlib import Path
import sys

sys.path.insert(0, str(Path(__file__).resolve().parent))
import MoveCorpus  # noqa: E402

INDEX = MoveCorpus.ROOT / "Tools/ModdingEditor/data/native-moves.json"


def elements(move, own):
    for source in ([move["node"]] if own else move["sources"]):
        for section in source:
            yield from section.iter()


def matches(element, tag, filters):
    return element.tag == tag and all(element.get(key) == value for key, value in filters)


def parse_filters(values):
    result = []
    for value in values or []:
        key, separator, expected = value.partition("=")
        if not separator:
            raise SystemExit(f"--attr expects KEY=VALUE, got {value!r}")
        result.append((key, expected))
    return result


def summary(move):
    return {key: move[key] for key in ("name", "file", "priority", "mid_frames", "type", "templates", "legacy", "intervals", "sounds", "keys")}


def print_move(move):
    print(f'{move["name"]}  file={move["file"] or "-"}  priority={move["priority"]}  mid_frames={move["mid_frames"]}' +
          (f'  type={move["type"]}' if move["type"] else "") + ("  legacy" if move["legacy"] else ""))
    if move["templates"]:
        print("  templates: " + " > ".join(move["templates"]))
    for interval in move["intervals"]:
        label = interval["name"] or interval["type"] or "?"
        kind = f' type={interval["type"]}' if interval["type"] and interval["name"] else ""
        hits = ", ".join(h["name"] or "?" for h in interval["hits"])
        print(f'  interval {label}{kind} {interval["start"]}..{interval["end"] if interval["end"] >= 0 else "open"}' + (f"  hit {hits}" if hits else ""))
        if "attack" in interval:
            a = interval["attack"]
            terms = ", ".join(f"{k} {v:g}" for k, v in a["terms"].items())
            print(f'    attack id={a["id"]} damage={a["damage"]} terms=[{terms}] impulse={a["impulse"]}')
            print(f'    edges {" ".join(a["edges"])}')
    for group in move["keys"]:
        print("  keys: " + " + ".join(f'{k["type"]}' + (f'({k["press"]})' if k["press"] else "") for k in group))
    for sound in move["sounds"]:
        print(f'  sound {sound["name"]} ' + (f'frame {sound["frame"]}' if sound["frame"] is not None else "(event)"))


def main(argv=None):
    parser = argparse.ArgumentParser(description=__doc__.splitlines()[0])
    parser.add_argument("--json", action="store_true", help="print JSON instead of text")
    commands = parser.add_subparsers(dest="command", required=True)
    show = commands.add_parser("show", help="print one resolved move")
    show.add_argument("name")
    search = commands.add_parser("search", help="find moves by name or animation file substring")
    search.add_argument("text")
    search.add_argument("--limit", type=int, default=50)
    uses = commands.add_parser("uses", help="list moves containing an element, e.g. uses CurrentInterval --attr Name=Uninterrupt")
    uses.add_argument("tag")
    uses.add_argument("--attr", action="append", metavar="KEY=VALUE")
    uses.add_argument("--own", action="store_true", help="ignore template-inherited elements")
    uses.add_argument("--limit", type=int, default=50)
    values = commands.add_parser("values", help="count distinct values of an attribute, e.g. values Hit Name")
    values.add_argument("tag")
    values.add_argument("attribute")
    values.add_argument("--own", action="store_true", help="ignore template-inherited elements")
    templates = commands.add_parser("templates", help="list templates, or the moves that apply one")
    templates.add_argument("name", nargs="?")
    export = commands.add_parser("export-index", help="write the editor's native move data")
    export.add_argument("--check", action="store_true", help="fail when the committed data is stale instead of writing")
    args = parser.parse_args(argv)

    root, legacy_templates = MoveCorpus.load_document()
    moves = MoveCorpus.resolve_moves(root, legacy_templates)

    if args.command == "export-index":
        text = json.dumps(MoveCorpus.editor_index(moves, MoveCorpus.template_names(root)), separators=(",", ":"), ensure_ascii=False) + "\n"
        if args.check:
            current = INDEX.read_text(encoding="utf-8") if INDEX.exists() else ""
            if current != text:
                print(f"{INDEX.relative_to(MoveCorpus.ROOT)} is stale; run python Tools/Audits/QueryMoves.py export-index")
                return 1
            print(f"{INDEX.relative_to(MoveCorpus.ROOT)} is current ({len(moves)} moves).")
            return 0
        INDEX.write_text(text, encoding="utf-8", newline="\n")
        print(f"Wrote {len(moves)} moves to {INDEX.relative_to(MoveCorpus.ROOT)}.")
        return 0

    if args.command == "show":
        found = [move for move in moves if move["name"] == args.name]
        if not found:
            print(f"No native move named {args.name!r}.", file=sys.stderr)
            return 1
        if args.json:
            print(json.dumps([summary(move) for move in found], indent=2))
        else:
            if len(found) > 1:
                print(f"warning: {len(found)} moves share this name; runtime patches reject ambiguous targets.")
            for move in found:
                print_move(move)
        return 0

    if args.command == "search":
        text = args.text.lower()
        found = [m for m in moves if text in m["name"].lower() or text in m["file"].lower()][:args.limit]
        if args.json:
            print(json.dumps([{"name": m["name"], "file": m["file"]} for m in found], indent=2))
        else:
            for move in found:
                print(f'{move["name"]:40} {move["file"]}')
        return 0

    if args.command == "uses":
        filters = parse_filters(args.attr)
        counts = Counter()
        for move in moves:
            count = sum(1 for element in elements(move, args.own) if matches(element, args.tag, filters))
            if count:
                counts[move["name"]] += count
        if args.json:
            print(json.dumps({"moves": len(counts), "matches": dict(counts.most_common(args.limit))}, indent=2))
        else:
            print(f"{len(counts)} move(s) contain <{args.tag}>" + (" with " + ", ".join(f"{k}={v}" for k, v in filters) if filters else ""))
            for name, count in counts.most_common(args.limit):
                print(f"  {name}" + (f" x{count}" if count > 1 else ""))
        return 0

    if args.command == "values":
        usage = defaultdict(set)
        for move in moves:
            for element in elements(move, args.own):
                if element.tag == args.tag:
                    usage[element.get(args.attribute, "(absent)")].add(move["name"])
        ordered = sorted(usage.items(), key=lambda item: (-len(item[1]), item[0]))
        if args.json:
            print(json.dumps({value: len(names) for value, names in ordered}, indent=2))
        else:
            for value, names in ordered:
                example = sorted(names)[0]
                print(f"{len(names):6}  {value}   (e.g. {example})")
        return 0

    if args.command == "templates":
        if args.name is None:
            usage = Counter(name for move in moves for name in move["templates"])
            names = MoveCorpus.template_names(root)
            if args.json:
                print(json.dumps({name: usage[name] for name in names}, indent=2))
            else:
                for name in sorted(names, key=lambda n: (-usage[n], n)):
                    print(f"{usage[name]:6}  {name}")
            return 0
        found = sorted(move["name"] for move in moves if args.name in move["templates"])
        print(json.dumps(found, indent=2) if args.json else "\n".join(found))
        return 0
    return 1


if __name__ == "__main__":
    sys.exit(main())
