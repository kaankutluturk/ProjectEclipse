"""Rasterize an imported skin as silhouettes into a PNG (Python stdlib only).

Mirrors the runtime: skinned nodes are posed with CharacterPipeline.helper_positions
(proportion rig, 3D frames, finger curl) on native animation frames and drawn
flat, like the game's silhouette renderer. Used by ImportCharacter.py for the
preview the in-game importer shows, and by the real-rig regression runner.

python Tools/Animation/SkinPreview.py --package Mods/local.my-fighter --animation stance_idle.bytes:3 --output preview.png
"""
import argparse
import struct
import sys
import zlib
from pathlib import Path
import xml.etree.ElementTree as ET

sys.path.insert(0, str(Path(__file__).resolve().parent))
import CharacterPipeline as pipeline

INK = (25, 30, 40)
PAPER = (235, 225, 205)


def write_png(path, width, height, rows):
    raw = b''.join(b'\0' + bytes(row) for row in rows)

    def chunk(kind, data):
        block = struct.pack('>I', len(data)) + kind + data
        return block + struct.pack('>I', zlib.crc32(kind + data) & 0xffffffff)
    Path(path).write_bytes(b'\x89PNG\r\n\x1a\n' + chunk(b'IHDR', struct.pack('>IIBBBBB', width, height, 8, 2, 0, 0, 0))
                           + chunk(b'IDAT', zlib.compress(raw, 9)) + chunk(b'IEND', b''))


def posed_triangles(package, animation, frame_index):
    """Screen-space (x right, y down) triangles of the package skin on one native frame."""
    package = Path(package)
    body = pipeline.model(package / 'assets/models/body.xml')
    skin = pipeline.model(package / 'assets/models/skin1.xml', body)
    clip = pipeline.read_animation(animation, body)
    frame = clip['frames'][min(max(frame_index, 0), len(clip['frames']) - 1)]
    combined = ET.fromstring(ET.tostring(body))
    combined.find('Nodes').extend(list(skin.find('Nodes')))
    positions = {name: list(point) for name, point in zip(clip['names'], frame)}
    pipeline.helper_positions(combined, positions, pipeline.read_proportions(skin), pipeline.read_rest(skin))
    return [[(positions[f.get('Node' + str(k))][0], -positions[f.get('Node' + str(k))][1]) for k in (1, 2, 3)]
            for f in skin.find('Figures')]


def render(cells, output, size=320):
    """cells: lists of screen-space triangles; one square cell each, auto-fitted."""
    width, height = size * len(cells), size
    rows = [list(PAPER) * width for _ in range(height)]
    for index, triangles in enumerate(cells):
        points = [p for t in triangles for p in t]
        if not points:
            continue
        xs = [p[0] for p in points]; ys = [p[1] for p in points]
        span = max(max(ys) - min(ys), max(xs) - min(xs)) * 1.12 or 1.0
        scale = size / span; cx = (min(xs) + max(xs)) / 2; top = min(ys)
        def screen(p):
            return (index * size + size / 2 + (p[0] - cx) * scale, (p[1] - top) * scale + size * .05)
        for triangle in triangles:
            a, b, c = (screen(p) for p in triangle)
            y0 = max(0, int(min(a[1], b[1], c[1]))); y1 = min(height - 1, int(max(a[1], b[1], c[1])) + 1)
            for y in range(y0, y1 + 1):
                crossings = []
                for p, q in ((a, b), (b, c), (c, a)):
                    if (p[1] <= y + .5 < q[1]) or (q[1] <= y + .5 < p[1]):
                        crossings.append(p[0] + (y + .5 - p[1]) * (q[0] - p[0]) / (q[1] - p[1]))
                if len(crossings) >= 2:
                    left = max(index * size, int(min(crossings))); right = min((index + 1) * size - 1, int(max(crossings)))
                    for x in range(left, right + 1):
                        rows[y][x * 3:x * 3 + 3] = INK
    write_png(output, width, height, rows)


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--package', type=Path, required=True)
    parser.add_argument('--animation', action='append', required=True, metavar='FILE:FRAME')
    parser.add_argument('--output', type=Path, required=True)
    args = parser.parse_args()
    cells = []
    for item in args.animation:
        path, _, frame = item.rpartition(':')
        cells.append(posed_triangles(args.package, Path(path), int(frame)))
    render(cells, args.output)


if __name__ == '__main__':
    main()
