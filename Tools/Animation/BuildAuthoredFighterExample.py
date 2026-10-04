"""Build the example's original point animation and connected sash from its body rig.

Uses the existing CharacterPipeline baker/native binary writer. No core clip is
copied, Blender is not required, and recovered source assets are never changed.
"""
import argparse
import hashlib
import json
import math
from pathlib import Path
import shutil
import xml.etree.ElementTree as ET

import CharacterPipeline as pipeline


def add(a, b): return [x + y for x, y in zip(a, b)]
def sub(a, b): return [x - y for x, y in zip(a, b)]
def mul(a, value): return [x * value for x in a]
def dot(a, b): return sum(x * y for x, y in zip(a, b))
def cross(a, b): return [a[1]*b[2]-a[2]*b[1], a[2]*b[0]-a[0]*b[2], a[0]*b[1]-a[1]*b[0]]
def length(a): return math.sqrt(dot(a, a))
def unit(a):
    size = length(a)
    if size < 1e-8: raise ValueError('Collapsed vector in authored rig')
    return mul(a, 1 / size)


def rotate(point, old, new):
    """Shortest-arc rotation of a hand-local offset; no outside math package."""
    a, b = unit(old), unit(new)
    axis, cosine = cross(a, b), max(-1.0, min(1.0, dot(a, b)))
    if cosine < -0.999999:
        axis = unit(cross(a, [1, 0, 0] if abs(a[0]) < .9 else [0, 1, 0]))
        return sub(mul(axis, 2 * dot(axis, point)), point)
    return add(add(point, cross(axis, point)), mul(cross(axis, cross(axis, point)), 1 / (1 + cosine)))


def motion(rig):
    names = [node.tag for node in rig.find('Nodes')]
    rest = {node.tag: [float(node.get(axis, '0')) for axis in 'XYZ'] for node in rig.find('Nodes')}
    required = {'NWrist_1', 'NElbow_1', 'NShoulder_1', 'NHeel_1', 'NHeel_2', 'NToeTip_1', 'NToeTip_2'}
    if not required <= rest.keys(): raise ValueError('Example needs the standard named SF2 point rig')
    # Native files are Y-up. Ground the authored source, rather than pre-negating Y.
    floor = min(rest[name][1] for name in ('NToeTip_1', 'NToeTip_2', 'NHeel_1', 'NHeel_2'))
    for point in rest.values(): point[1] -= floor
    shoulder, elbow, wrist = (rest[name] for name in ('NShoulder_1', 'NElbow_1', 'NWrist_1'))
    upper, lower = length(sub(elbow, shoulder)), length(sub(wrist, elbow))
    start = sub(wrist, shoulder)
    target = [upper + lower - 2, -16, 0]
    frames = []
    for frame in range(61):
        # Ease out, hold contact, then recover. This is newly authored motion,
        # not a playback of a recovered animation or a generic Lua operation DSL.
        t = min(1, max(0, (frame - 6) / 16)) if frame <= 28 else max(0, 1 - (frame - 28) / 25)
        t = t*t*(3-2*t)
        direction = add(mul(start, 1-t), mul(target, t))
        distance = min(upper+lower-.1, max(abs(upper-lower)+.1, length(direction)))
        axis = unit(direction)
        along = (upper*upper-lower*lower+distance*distance)/(2*distance)
        bend = unit(sub([0, -1, 0], mul(axis, dot([0, -1, 0], axis))))
        new_elbow = add(shoulder, add(mul(axis, along), mul(bend, math.sqrt(max(0, upper*upper-along*along)))))
        new_wrist = add(shoulder, mul(axis, distance))
        pose = {name: point[:] for name, point in rest.items()}
        pose['NElbow_1'], pose['NWrist_1'] = new_elbow, new_wrist
        for name in names:
            if name.endswith('_1') and (name.startswith(('NKnuckles', 'NFingertips', 'Weapon-Node'))):
                pose[name] = add(new_wrist, rotate(sub(rest[name], wrist), sub(wrist, elbow), sub(new_wrist, new_elbow)))
        # A small torso lean gives the shared sash helpers actual deformation,
        # while the grounded lower body stays fixed in the source animation.
        angle = t*.06
        for name in names:
            if name.startswith(('NTop', 'NNeck', 'NHead', 'NChest', 'NStomach', 'NShoulder', 'NElbow', 'NWrist', 'NFingertips', 'NKnuckles', 'Weapon-Node')):
                delta = sub(pose[name], rest['NPivot'])
                pose[name] = add(rest['NPivot'], [math.cos(angle)*delta[0]+math.sin(angle)*delta[1], -math.sin(angle)*delta[0]+math.cos(angle)*delta[1], delta[2]])
        pipeline.helper_positions(rig, pose)
        frames.append([pose[name] for name in names])
    return {'version': 1, 'fps': 60, 'names': names, 'frames': frames}


def sash(rig):
    rest = {node.tag: [float(node.get(axis, '0')) for axis in 'XYZ'] for node in rig.find('Nodes')}
    basis = ['NShoulder_1', 'NShoulder_2', 'NPivot', 'NChestF']
    origin = rest[basis[0]]
    a, b, c = [sub(rest[name], origin) for name in basis[1:]]
    determinant = dot(a, cross(b, c))
    if abs(determinant) < .001: raise ValueError('Sash needs a noncoplanar torso basis')
    # Two columns and three rows share triangle vertices and move with the torso.
    shoulder_mid = mul(add(rest[basis[0]], rest[basis[1]]), .5)
    hip = rest['NPivot']
    root = ET.Element('Scene'); nodes = ET.SubElement(root, 'Nodes')
    ET.SubElement(root, 'Edges'); figures = ET.SubElement(root, 'Figures')
    for row in range(3):
        for side in range(2):
            center = add(mul(shoulder_mid, 1-row*.5), mul(hip, row*.5))
            point = add(center, [(-1 if side == 0 else 1)*(22 + row*5), -row*10, -8-row*3])
            delta = sub(point, origin)
            weights = [dot(delta, cross(b, c))/determinant, dot(a, cross(delta, c))/determinant, dot(a, cross(b, delta))/determinant]
            weights = [1-sum(weights)] + weights
            attributes = {'Type': 'MacroNode', 'NodesCount': '4', 'X': '0', 'Y': '0', 'Z': '0'}
            for i, (name, weight) in enumerate(zip(basis, weights), 1):
                attributes['ChildNode'+str(i)] = name
                attributes['LCC'+str(i)] = format(weight, '.12g')
            ET.SubElement(nodes, 'AuthoredSashV'+str(row*2+side), attributes)
    for row in range(2):
        for corner, face in enumerate([(row*2, row*2+1, row*2+2), (row*2+1, row*2+3, row*2+2)]):
            ET.SubElement(figures, 'AuthoredSash-Triangle'+str(row*2+corner), dict(Type='Triangle', **{'Node'+str(i+1): 'AuthoredSashV'+str(v) for i, v in enumerate(face)}))
    ET.indent(root, space='  ')
    return root


def build(body, destination):
    body, destination = Path(body), Path(destination)
    if destination.exists(): raise ValueError('Choose a fresh output directory; authored files are never overwritten')
    rig = pipeline.model(body)
    source = motion(rig); clip = pipeline.bake(rig, source)
    destination.mkdir(parents=True)
    shutil.copyfile(body, destination / 'body.xml')
    ET.ElementTree(sash(rig)).write(destination / 'sash.xml', encoding='utf-8', xml_declaration=True)
    pipeline.model(destination / 'sash.xml', rig)
    pipeline.write_animation(destination / 'strike.bytes', clip)
    (destination / 'strike.frames.json').write_text(json.dumps(source, separators=(',', ':'))+'\n', encoding='utf-8')
    metadata = dict(version=1, fps=60, mid_frames=0, frames=len(clip['frames']), nodes=clip['names'],
                    rig_sha256=hashlib.sha256(body.read_bytes()).hexdigest(),
                    animation_sha256=hashlib.sha256((destination/'strike.bytes').read_bytes()).hexdigest())
    (destination / 'strike.rig.json').write_text(json.dumps(metadata, indent=2)+'\n', encoding='utf-8')
    pipeline.preview(destination / 'preview.html', rig, clip)
    return destination


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--body', type=Path, required=True)
    parser.add_argument('--output', type=Path, required=True)
    args = parser.parse_args()
    print('BUILT:', build(args.body, args.output))
