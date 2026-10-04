#!/usr/bin/env python3
"""Build original Pulse Arena warning art and four cropped sprite descriptors.

Uses only Python's standard library. Output is a loose-mod assets directory;
the PNG is an atlas, with each frame addressed through its sprite descriptor.
"""
import argparse
import math
from pathlib import Path
import struct
import zlib


SIZE = 256
FRAMES = 4


def pixel(x, y, frame):
    u, v = (x + .5) / SIZE, (y + .5) / SIZE
    dx, dy = (u - .5) * 2, (v - .5) * 2
    radius = math.hypot(dx, dy)
    angle = math.atan2(dy, dx) + frame * math.pi / 16
    border = max(0, 1 - abs(dx)) ** .6 * max(0, 1 - abs(dy)) ** .6
    column = .16 * border
    ring = math.exp(-((radius - .67) / .025) ** 2)
    inner = .5 * math.exp(-((radius - .45) / .018) ** 2)
    rays = max(0, math.cos(angle * 8)) ** 12 * math.exp(-((radius - .56) / .11) ** 2)
    spine = .25 * math.exp(-(dx / .12) ** 2) * border
    glow = .16 * math.exp(-(radius / .8) ** 2)
    alpha = min(1, column + glow + ring + inner + rays + spine)
    return bytes((255, 255, 255, round(alpha * 255)))


def chunk(kind, payload):
    return struct.pack('>I', len(payload)) + kind + payload + struct.pack('>I', zlib.crc32(kind + payload))


def build(output):
    output = Path(output)
    (output / 'textures').mkdir(parents=True, exist_ok=True)
    (output / 'sprites').mkdir(parents=True, exist_ok=True)
    # PNG rows run downwards; descriptor crop coordinates start at bottom left.
    rows = bytearray()
    for frame in reversed(range(FRAMES)):
        for y in range(SIZE):
            rows.append(0)
            for x in range(SIZE):
                rows.extend(pixel(x, y, frame))
    header = struct.pack('>IIBBBBB', SIZE, SIZE * FRAMES, 8, 6, 0, 0, 0)
    png = b'\x89PNG\r\n\x1a\n' + chunk(b'IHDR', header) + chunk(b'IDAT', zlib.compress(rows, 9)) + chunk(b'IEND', b'')
    (output / 'textures/pulse.png').write_bytes(png)
    for frame in range(FRAMES):
        (output / f'sprites/pulse_{frame + 1}.asset').write_text(
            f'type=sprite\ntexture=textures/pulse.png\nrect=[0, {frame * SIZE}, {SIZE}, {SIZE}]\n'
            'filter="bilinear"\nwrap="clamp"\nmipmaps=false\n', encoding='utf-8')


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--output', required=True, help='Output assets directory, for example Temp/PulseArtwork/assets')
    build(parser.parse_args().output)
