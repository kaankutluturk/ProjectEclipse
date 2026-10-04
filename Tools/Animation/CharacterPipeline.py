"""Validate, bake and preview authored SF2 rigs and point animation. Python stdlib only."""
import argparse
import hashlib
import json
import math
from pathlib import Path
import struct
import xml.etree.ElementTree as ET


def finite(value, where):
    number = float(value)
    if not math.isfinite(number) or abs(number) > 100000:
        raise ValueError(f'{where}: coordinate must be finite within +/-100000')
    return number


def model(path, base=None):
    raw = Path(path).read_bytes()
    if len(raw) > 16 * 1024 * 1024 or b'<!DOCTYPE' in raw.upper() or b'<!ENTITY' in raw.upper():
        raise ValueError('Model exceeds 16 MiB or contains DTD/entities')
    root = ET.fromstring(raw)
    if root.tag != 'Scene' or root.find('Figures') is None:
        raise ValueError('Model requires Scene and Figures')
    inherited = {} if base is None else {n.tag: n for n in base.find('Nodes')}
    nodes = list(root.find('Nodes')) if root.find('Nodes') is not None else []
    names = list(inherited)
    for node in nodes:
        if node.tag in names or node.get('Type') not in ('Node', 'MacroNode', 'CenterOfMass', 'SkinnedNode'):
            raise ValueError(f'Duplicate node or unsupported type: {node.tag}')
        names.append(node.tag)
        for axis in 'XYZ':
            finite(node.get(axis, '0'), node.tag + '.' + axis)
        if finite(node.get('Mass', '0'), node.tag + '.Mass') < 0:
            raise ValueError(node.tag + ': mass cannot be negative')
    if not names or len(names) > 4096:
        raise ValueError('A composed model requires 1..4096 nodes')
    rest_info = (read_rest(root) or {}) if root.find('Rest') is not None else {}
    for node in nodes:
        if node.get('Type') == 'SkinnedNode':
            count = int(node.get('BonesCount', 0))
            if not 1 <= count <= 16:
                raise ValueError(node.tag + ': BonesCount must be in 1..16')
            total = 0
            for i in range(1, count + 1):
                start, end = node.get('BoneStart' + str(i)), node.get('BoneEnd' + str(i))
                if start == end or any(n not in names or names.index(n) >= names.index(node.tag) for n in (start, end)):
                    raise ValueError(node.tag + ': skin endpoints must differ and exist earlier')
                weight = finite(node.get('Weight' + str(i), 'nan'), node.tag + '.weight')
                if not 0 <= weight <= 1:
                    raise ValueError(node.tag + ': skin weight must be in 0..1')
                total += weight
                # Relative bindings use Across; absolute bindings use Offset/Extend in
                # native units; 3D bindings use LocalX/Y/Z in a posed segment frame.
                if node.get('LocalX' + str(i)) is not None:
                    if any(node.get(f + str(i)) is not None for f in ('Along', 'Across', 'Offset', 'Extend')):
                        raise ValueError(node.tag + ': a Local binding cannot also use Along/Across/Offset/Extend')
                    if (start, end) not in SKIN_FRAMES or root.find('Proportions') is None or root.find('Rest') is None:
                        raise ValueError(node.tag + ': Local bindings need a supported segment, Proportions and Rest')
                    for axis in 'XYZ':
                        if abs(finite(node.get('Local' + axis + str(i), 'nan'), node.tag + '.Local' + axis)) > 1000:
                            raise ValueError(node.tag + ': local position must be in -1000..1000')
                    frame_key = (start, end)
                    if node.get('Dynamic' + str(i)) is not None:
                        index = node.get('Dynamic' + str(i))
                        if not index.isdigit() or int(index) >= len(rest_info.get('dynamic', [])):
                            raise ValueError(node.tag + ': Dynamic must name a Rest Dynamic bone')
                        frame_key = ('dyn', int(index))
                    if 'frames' in rest_info and frame_key not in rest_info['frames']:
                        raise ValueError(node.tag + ': Rest needs a Frame for every 3D binding segment')
                    if node.get('Curl' + str(i)) is not None or node.get('Pivot' + str(i)) is not None:
                        if not start.startswith('NWrist_') or node.get('Curl' + str(i)) not in ('1', '2', '3'):
                            raise ValueError(node.tag + ': Curl must be 1..3 on a hand (NWrist-NFingertips) binding')
                        if len(read_pivots(node.get('Pivot' + str(i), ''))) != int(node.get('Curl' + str(i))):
                            raise ValueError(node.tag + ': Pivot needs one x,y,z point per Curl level')
                    continue
                absolute = node.get('Offset' + str(i)) is not None
                if absolute == (node.get('Across' + str(i)) is not None):
                    raise ValueError(node.tag + ': each binding needs exactly one of Across, Offset or LocalX/Y/Z')
                if not absolute and node.get('Extend' + str(i)) is not None:
                    raise ValueError(node.tag + ': Extend requires Offset')
                for field in ('Along',) if absolute else ('Along', 'Across'):
                    if abs(finite(node.get(field + str(i), 'nan'), node.tag + '.' + field)) > 100:
                        raise ValueError(node.tag + ': attachment must be in -100..100')
                for field, default in (('Offset', 'nan'), ('Extend', '0')) if absolute else ():
                    if abs(finite(node.get(field + str(i), default), node.tag + '.' + field)) > 1000:
                        raise ValueError(node.tag + ': absolute offset must be in -1000..1000')
            if abs(total - 1) > .0001:
                raise ValueError(node.tag + ': skin weights must sum to 1')
            continue
        count = int(node.get('NodesCount', 0))
        helper = node.get('Type') in ('MacroNode', 'CenterOfMass')
        if helper and not 1 <= count <= 128:
            raise ValueError('Invalid helper node dependency count')
        if not helper:
            continue
        total_mass = 0
        for i in range(1, count + 1):
            child = node.get('ChildNode' + str(i))
            if child not in names or names.index(child) >= names.index(node.tag):
                raise ValueError(f'{node.tag}: helper dependency must exist earlier: {child}')
            if node.get('Type') == 'MacroNode':
                weight = node.get('LCC' + str(i))
                if weight is None:
                    raise ValueError(f'{node.tag}: missing LCC{i}')
                finite(weight, node.tag + '.weight')
            child_node = inherited.get(child)
            if child_node is None:
                child_node = next(n for n in nodes if n.tag == child)
            total_mass += float(child_node.get('Mass', '0'))
        if node.get('Type') == 'CenterOfMass' and total_mass <= 0:
            raise ValueError(node.tag + ': center of mass dependencies need positive total mass')
    edges = {}
    def expanded(edge):
        count = int(edge.get('Iterations', '1'))
        if not 1 <= count <= 32:
            raise ValueError(edge.tag + ': Iterations must be in 1..32')
        return [edge.tag + (f'CI{i}' if i else '') for i in range(count)]
    if base is not None and base.find('Edges') is not None:
        for edge in base.find('Edges'):
            for name in expanded(edge):
                edges[name] = edge
    for edge in (root.find('Edges') if root.find('Edges') is not None else []):
        if edge.get('Type') not in ('Edge', 'Muscle'):
            raise ValueError(f'Duplicate edge or unsupported type: {edge.tag}')
        if edge.get('End1') not in names or edge.get('End2') not in names:
            raise ValueError(f'{edge.tag}: unresolved endpoint')
        for key in ('Length', 'Radius', 'Margin1', 'Margin2'):
            number = finite(edge.get(key, '0'), edge.tag + '.' + key)
            if key in ('Length', 'Radius') and number < 0:
                raise ValueError(edge.tag + '.' + key + ': cannot be negative')
        for name in expanded(edge):
            if name in edges:
                raise ValueError('Duplicate expanded edge: ' + name)
            edges[name] = edge
    if len(edges) > 8192:
        raise ValueError('A composed model permits at most 8192 expanded edges')
    seen = set()
    for figure in root.find('Figures'):
        if figure.tag in seen:
            raise ValueError('Duplicate figure: ' + figure.tag)
        seen.add(figure.tag)
        if figure.get('Type') == 'Triangle':
            if any(figure.get('Node' + str(i)) not in names for i in (1, 2, 3)):
                raise ValueError(figure.tag + ': unresolved triangle node')
        elif figure.get('Type') == 'Capsule':
            if figure.get('Edge') not in edges:
                raise ValueError(figure.tag + ': unresolved capsule edge')
            for key in ('Radius1', 'Radius2', 'Margin1', 'Margin2'):
                number = finite(figure.get(key, '0'), figure.tag + '.' + key)
                if key.startswith('Radius') and number < 0:
                    raise ValueError(figure.tag + '.' + key + ': cannot be negative')
        else:
            raise ValueError('Unsupported figure type: ' + str(figure.get('Type')))
    if read_proportions(root) is not None and any(n not in names for n in [x for pair in PROPORTION_SEGMENTS for x in pair] + ['NChestF', 'NPelvisF']):
        raise ValueError('Proportions need every native torso, arm and leg point plus NChestF/NPelvisF in the composed body')
    if read_rest(root) is not None and any(n not in names for n in FRONT_HELPERS):
        raise ValueError('Rest needs the native front helpers ' + ', '.join(FRONT_HELPERS))
    return root


# Visual proportion rig: segments whose drawn length a skin may set. Mirrors
# Assets/Scripts/Eclipse/Modding/ModelMacroNode.Skin.cs (SkinProportionRig).
TORSO_SEGMENTS = (('NPivot', 'NStomach'), ('NStomach', 'NChest'), ('NChest', 'NNeck'),
                  ('NNeck', 'NHead'), ('NHead', 'NTop'))
SIDE_SEGMENTS = (('NNeck', 'NShoulder'), ('NShoulder', 'NElbow'), ('NElbow', 'NWrist'),
                 ('NWrist', 'NFingertips'), ('NPivot', 'NHip'), ('NHip', 'NKnee'),
                 ('NKnee', 'NAnkle'), ('NHeel', 'NToeTip'))
PROPORTION_SEGMENTS = TORSO_SEGMENTS + tuple(
    (a if a in ('NNeck', 'NPivot') else a + '_' + s, b + '_' + s) for s in ('1', '2') for a, b in SIDE_SEGMENTS)


def read_proportions(scene):
    """Optional <Proportions><Segment Start End Length/></Proportions>; every segment required."""
    element = scene.find('Proportions')
    if element is None:
        return None
    if element.get('Version') != '1':
        raise ValueError('Proportions requires Version="1"')
    lengths = {}
    for segment in element:
        key = (segment.get('Start'), segment.get('End'))
        if segment.tag != 'Segment' or key not in PROPORTION_SEGMENTS or key in lengths:
            raise ValueError('Proportions: unsupported or duplicate segment ' + str(key))
        length = finite(segment.get('Length', 'nan'), 'Proportions ' + '-'.join(key))
        if not .01 <= length <= 1000:
            raise ValueError('Proportions: segment length must be in 0.01..1000')
        lengths[key] = length
    missing = [k for k in PROPORTION_SEGMENTS if k not in lengths]
    if missing:
        raise ValueError('Proportions: missing segments ' + ', '.join('-'.join(k) for k in missing))
    return lengths


def _sub(a, b): return [x - y for x, y in zip(a, b)]
def _add(a, b): return [x + y for x, y in zip(a, b)]
def _mul(a, k): return [x * k for x in a]
def _dot(a, b): return sum(x * y for x, y in zip(a, b))
def _len(a): return math.sqrt(_dot(a, a))
def _unit(a):
    length = _len(a)
    return _mul(a, 1 / length) if length > 1e-6 else [0.0, 0.0, 0.0]


def _two_bone(root, target, upper, lower, native, hint):
    """Middle joint so root-mid-target has lengths upper/lower.

    The bend side comes from the native limb's own bend (native = root, mid,
    end), measured against the native root-end line, never against the visual
    line, so a shifted visual shoulder or hip cannot flip the joint backward.
    A nearly straight native limb falls back to the anatomical hint (elbows
    back, knees forward); a small hint share keeps that choice stable.
    """
    axis = _sub(target, root); distance = _len(axis); u = _unit(axis)
    native_root, native_mid, native_end = native
    if distance < 1e-6:
        return _add(root, _mul(_unit(_sub(native_mid, native_root)), upper))
    if distance >= upper + lower:  # Out of reach: stay straight and stretch.
        return _add(root, _mul(u, distance * upper / (upper + lower)))
    distance = max(distance, abs(upper - lower) + 1e-4)
    x = (upper * upper - lower * lower + distance * distance) / (2 * distance)
    h = math.sqrt(max(0.0, upper * upper - x * x))
    line = _unit(_sub(native_end, native_root))
    bend = _sub(native_mid, native_root); bend = _sub(bend, _mul(line, _dot(bend, line)))
    side = _add(bend, _mul(_unit(hint), BEND_HINT_SHARE * (upper + lower)))
    side = _sub(side, _mul(u, _dot(side, u)))
    if _len(side) < 1e-6:
        side = _sub(hint, _mul(u, _dot(hint, u)))
    if _len(side) < 1e-6:
        side = [-u[1], u[0], 0.0]
    return _add(_add(root, _mul(u, x)), _mul(_unit(side), h))


# Share of limb length given to the anatomical bend hint (elbows back, knees forward).
BEND_HINT_SHARE = .02


# Paired limb chains and the torso points that tell their true side apart.
PAIR_GROUPS = (
    (('NShoulder', 'NElbow', 'NWrist', 'NFingertips', 'NKnuckles', 'NKnucklesS'), ('NChest', 'NNeck', 'NChestF', 'NShoulder')),
    (('NHip', 'NKnee', 'NAnkle', 'NHeel', 'NToeTip'), ('NPivot', 'NStomach', 'NPelvisF', 'NHip')))


def limb_side(p, origin, top, front, joint):
    """Signed share of the _1-minus-_2 joint offset on the torso's up x front side."""
    up = _unit(_sub(p[top], p[origin])); facing = _sub(p[front], p[origin])
    facing = _unit(_sub(facing, _mul(up, _dot(up, facing))))
    return _dot(_cross(up, facing), _unit(_sub(p[joint + '_1'], p[joint + '_2'])))


def unswap_pairs(p):
    """Copy of p with left/right limb chains on their anatomical sides.

    SF2 keeps _1 on the up x front side of the torso, but some clips are
    authored with the labels reversed and MirrorNodes swaps pairs at runtime;
    the symmetric native rig does not care, a 3D skin does. Each chain is
    tested at its shoulders or hips and swapped back when reversed.
    """
    p = dict(p)
    for chain, torso in PAIR_GROUPS:
        if limb_side(p, *torso) < 0:
            for joint in chain:
                if joint + '_1' in p and joint + '_2' in p:
                    p[joint + '_1'], p[joint + '_2'] = p[joint + '_2'], p[joint + '_1']
    return p


def visual_joints(p, lengths):
    """Drawn joint positions with source proportions; gameplay positions are untouched.

    Torso, neck and head keep their proportioned lengths along the native
    directions. Hands and feet stay on the native wrists, ankles and heels so
    weapons, contact and the floor line up; elbows and knees are solved with
    two-bone IK on the native limb's own bend side (see _two_bone). The pelvis moves along the body axis by
    the leg-length difference so proportioned legs reach the native feet.
    """
    L = lambda a, b: lengths[(a, b)]
    v = {}
    native_leg = sum(_len(_sub(p['NKnee_' + s], p['NHip_' + s])) + _len(_sub(p['NAnkle_' + s], p['NKnee_' + s]))
                     for s in '12') / 2
    visual_leg = sum(L('NHip_' + s, 'NKnee_' + s) + L('NKnee_' + s, 'NAnkle_' + s) for s in '12') / 2
    down = _unit(_sub(p['NPivot'], p['NNeck']))
    v['NPivot'] = _add(p['NPivot'], _mul(down, native_leg - visual_leg))
    for a, b in TORSO_SEGMENTS:
        v[b] = _add(v[a], _mul(_unit(_sub(p[b], p[a])), L(a, b)))
    for s in '12':
        n = lambda name: name + '_' + s
        v[n('NShoulder')] = _add(v['NNeck'], _mul(_unit(_sub(p[n('NShoulder')], p['NNeck'])), L('NNeck', n('NShoulder'))))
        v[n('NWrist')] = list(p[n('NWrist')])
        v[n('NElbow')] = _two_bone(v[n('NShoulder')], v[n('NWrist')], L(n('NShoulder'), n('NElbow')),
                                   L(n('NElbow'), n('NWrist')), (p[n('NShoulder')], p[n('NElbow')], p[n('NWrist')]),
                                   _sub(p['NChest'], p['NChestF']))
        v[n('NFingertips')] = _add(v[n('NWrist')], _mul(_unit(_sub(p[n('NFingertips')], p[n('NWrist')])),
                                                         L(n('NWrist'), n('NFingertips'))))
        v[n('NHip')] = _add(v['NPivot'], _mul(_unit(_sub(p[n('NHip')], p['NPivot'])), L('NPivot', n('NHip'))))
        v[n('NAnkle')] = list(p[n('NAnkle')])
        v[n('NKnee')] = _two_bone(v[n('NHip')], v[n('NAnkle')], L(n('NHip'), n('NKnee')),
                                  L(n('NKnee'), n('NAnkle')), (p[n('NHip')], p[n('NKnee')], p[n('NAnkle')]),
                                  _sub(p['NPelvisF'], p['NPivot']))
        v[n('NHeel')] = list(p[n('NHeel')])
        v[n('NToeTip')] = _add(v[n('NHeel')], _mul(_unit(_sub(p[n('NToeTip')], p[n('NHeel')])),
                                                    L(n('NHeel'), n('NToeTip'))))
    return v


# 3D skin frames. Torso frames take their forward axis from the native front
# helpers; limb frames swing from their parent without twist; feet use the ankle.
TORSO_FRONT = {('NPivot', 'NStomach'): ('NPivot', 'NPelvisF'), ('NStomach', 'NChest'): ('NStomach', 'NStomachF'),
               ('NChest', 'NNeck'): ('NChest', 'NChestF'), ('NNeck', 'NHead'): ('NHead', 'NHeadF'),
               ('NHead', 'NTop'): ('NHead', 'NHeadF')}
LIMB_PARENTS = tuple(item for s in ('1', '2') for item in (
    (('NShoulder_' + s, 'NElbow_' + s), ('NChest', 'NNeck')),
    (('NElbow_' + s, 'NWrist_' + s), ('NShoulder_' + s, 'NElbow_' + s)),
    (('NWrist_' + s, 'NFingertips_' + s), ('NElbow_' + s, 'NWrist_' + s)),
    (('NHip_' + s, 'NKnee_' + s), ('NPivot', 'NStomach')),
    (('NKnee_' + s, 'NAnkle_' + s), ('NHip_' + s, 'NKnee_' + s))))
REST_SEGMENTS = tuple(child for child, _ in LIMB_PARENTS)
SKIN_FRAMES = tuple(TORSO_FRONT) + REST_SEGMENTS + (('NHeel_1', 'NToeTip_1'), ('NHeel_2', 'NToeTip_2'))
FRONT_HELPERS = ('NPelvisF', 'NStomachF', 'NChestF', 'NHeadF', 'NKnuckles_1', 'NKnuckles_2', 'NKnucklesS_1', 'NKnucklesS_2')


def read_pivots(text):
    """'x,y,z x,y,z ...' hand-local phalanx pivots, proximal first."""
    pivots = []
    for item in (text or '').split():
        values = item.split(',')
        if len(values) != 3:
            raise ValueError('Pivot entries must be x,y,z')
        point = [finite(value, 'Pivot') for value in values]
        if max(abs(x) for x in point) > 1000:
            raise ValueError('Pivot must be in -1000..1000')
        pivots.append(point)
    return pivots


def read_rest(scene):
    """Optional <Rest><Bone Start End X Y Z/></Rest>: limb rest directions in parent frames."""
    element = scene.find('Rest')
    if element is None:
        return None
    if element.get('Version') != '1':
        raise ValueError('Rest requires Version="1"')
    rest = {}
    for bone in element:
        if bone.tag == 'Frame':
            _read_frame(bone, rest)
            continue
        if bone.tag == 'Dynamic':
            _read_dynamic(bone, rest)
            continue
        key = (bone.get('Start'), bone.get('End'))
        if bone.tag != 'Bone' or key not in REST_SEGMENTS or key in rest:
            raise ValueError('Rest: unsupported or duplicate bone ' + str(key))
        direction = [finite(bone.get(axis, 'nan'), 'Rest ' + '-'.join(key)) for axis in 'XYZ']
        if abs(_len(direction) - 1) > .01:
            raise ValueError('Rest: bone direction must be a unit vector')
        rest[key] = direction
        if bone.get('Curl') is not None:  # source rest finger bend, hands only
            curl = finite(bone.get('Curl'), 'Rest Curl')
            if not key[1].startswith('NFingertips') or not 0 <= curl <= 180:
                raise ValueError('Rest: Curl is a hand bone angle in 0..180')
            rest[('curl', key[0][-1])] = curl
        if bone.get('Twist') is not None:  # native rest hand roll, hands only
            twist = finite(bone.get('Twist'), 'Rest Twist')
            if not key[1].startswith('NFingertips') or not -180 <= twist <= 180:
                raise ValueError('Rest: Twist is a hand bone angle in -180..180')
            rest[('twist', key[0][-1])] = twist
    missing = [k for k in REST_SEGMENTS if k not in rest]
    if missing:
        raise ValueError('Rest: missing bones ' + ', '.join('-'.join(k) for k in missing))
    return rest


def _triple(text, where, limit=1000.0):
    values = (text or '').split(',')
    if len(values) != 3:
        raise ValueError(where + ' must be x,y,z')
    point = [finite(value, where) for value in values]
    if max(abs(x) for x in point) > limit:
        raise ValueError(where + ' must be within +/-' + str(limit))
    return point


def _frame_key(text, rest):
    """'Start-End' native segment, or '#k' for an earlier dynamic bone."""
    if text and text.startswith('#'):
        index = int(text[1:])
        if not 0 <= index < len(rest.get('dynamic', [])):
            raise ValueError('Rest: dynamic parent must be an earlier Dynamic')
        return ('dyn', index)
    key = tuple((text or '').split('-', 1))
    if key not in SKIN_FRAMES:
        raise ValueError('Rest: unsupported segment ' + str(text))
    return key


def _read_frame(element, rest):
    """<Frame Start End | Dynamic="k" O A B/>: a segment's source rest frame (origin, axes)."""
    key = ('dyn', int(element.get('Dynamic'))) if element.get('Dynamic') is not None else (element.get('Start'), element.get('End'))
    if key[0] != 'dyn' and key not in SKIN_FRAMES or key in rest.setdefault('frames', {}):
        raise ValueError('Rest: unsupported or duplicate frame ' + str(key))
    origin = _triple(element.get('O'), 'Rest Frame O', 100000)
    a, b = _triple(element.get('A'), 'Rest Frame A'), _triple(element.get('B'), 'Rest Frame B')
    if abs(_len(a) - 1) > .01 or abs(_len(b) - 1) > .01 or abs(_dot(a, b)) > .01:
        raise ValueError('Rest: frame axes must be orthonormal')
    rest['frames'][key] = (origin, [a, b, _cross(a, b)])


def _read_dynamic(element, rest):
    """<Dynamic Id Parent Head Dir Length Stiffness Damping/>: a spring bone (coat, hair)."""
    dynamic = rest.setdefault('dynamic', [])
    if element.get('Id') != str(len(dynamic)):
        raise ValueError('Rest: Dynamic ids must count up from 0')
    parent = _frame_key(element.get('Parent'), rest)
    direction = _triple(element.get('Dir'), 'Rest Dynamic Dir')
    if abs(_len(direction) - 1) > .01:
        raise ValueError('Rest: Dynamic Dir must be a unit vector')
    length = finite(element.get('Length', 'nan'), 'Rest Dynamic Length')
    stiffness = finite(element.get('Stiffness', '0.15'), 'Rest Dynamic Stiffness')
    damping = finite(element.get('Damping', '0.9'), 'Rest Dynamic Damping')
    if not .01 <= length <= 1000 or not 0 <= stiffness <= 1 or not 0 <= damping <= 1:
        raise ValueError('Rest: Dynamic length 0.01..1000, stiffness and damping 0..1')
    dynamic.append({'parent': parent, 'head': _triple(element.get('Head'), 'Rest Dynamic Head'),
                    'dir': direction, 'length': length, 'stiffness': stiffness, 'damping': damping})


def _cross(a, b):
    return [a[1] * b[2] - a[2] * b[1], a[2] * b[0] - a[0] * b[2], a[0] * b[1] - a[1] * b[0]]


def frame(primary, secondary):
    """Right-handed orthonormal columns (a, b, a x b); b is secondary made perpendicular."""
    a = _unit(primary)
    if _len(a) == 0:
        a = [0.0, 1.0, 0.0]
    b = _sub(secondary, _mul(a, _dot(a, secondary)))
    if _len(b) < 1e-6:
        b = _cross(a, [0.0, 0.0, 1.0]) if abs(a[2]) < .9 else _cross(a, [1.0, 0.0, 0.0])
    b = _unit(b)
    return [a, b, _cross(a, b)]


def _apply(columns, local):
    return [sum(columns[k][i] * local[k] for k in range(3)) for i in range(3)]


def _transpose_apply(columns, vector):
    return [_dot(column, vector) for column in columns]


def _swing(p, q):
    """Rotation (columns) taking unit p to unit q by the shortest arc."""
    axis = _cross(p, q); sine = _len(axis); cosine = max(-1.0, min(1.0, _dot(p, q)))
    if sine < 1e-6:
        if cosine > 0:
            return [[1.0, 0.0, 0.0], [0.0, 1.0, 0.0], [0.0, 0.0, 1.0]]
        axis = _unit([p[1], -p[0], 0.0] if abs(p[2]) < .9 else [0.0, p[2], -p[1]]); sine = 0.0
    else:
        axis = _mul(axis, 1 / sine)
    angle = math.atan2(sine, cosine); s, c = math.sin(angle), math.cos(angle)
    x, y, z = axis
    rows = [[c + x * x * (1 - c), x * y * (1 - c) - z * s, x * z * (1 - c) + y * s],
            [y * x * (1 - c) + z * s, c + y * y * (1 - c), y * z * (1 - c) - x * s],
            [z * x * (1 - c) - y * s, z * y * (1 - c) + x * s, c + z * z * (1 - c)]]
    return [[rows[i][j] for i in range(3)] for j in range(3)]


# Finger curl: full curl at this native knuckle bend; per-phalanx share of it.
FULL_CURL_DEGREES = 75.0
PHALANX_CURL_DEGREES = (90.0, 100.0, 70.0)


def _rotate(vector, axis, degrees):
    """Rodrigues rotation of vector about unit axis."""
    angle = math.radians(degrees); c, s = math.cos(angle), math.sin(angle)
    return _add(_add(_mul(vector, c), _mul(_cross(axis, vector), s)), _mul(axis, _dot(axis, vector) * (1 - c)))


# Twist is limited so a near-degenerate native hand cannot spin the mesh around.
MAX_TWIST_DEGREES = 150.0


def _reference(direction):
    """A fixed perpendicular of a unit direction (same fallback as frame())."""
    return _unit(_cross(direction, [0.0, 0.0, 1.0]) if abs(direction[2]) < .9 else _cross(direction, [1.0, 0.0, 0.0]))


def hand_roll(p, columns, rest_direction, side):
    """Native hand roll (degrees) about the palm, from NKnucklesS, in a hand frame."""
    axis = _unit(_apply(columns, rest_direction))
    reference = _apply(columns, _reference(rest_direction))
    knuckle_side = _sub(p['NKnucklesS_' + side], p['NKnuckles_' + side])
    knuckle_side = _sub(knuckle_side, _mul(axis, _dot(axis, knuckle_side)))
    reference = _sub(reference, _mul(axis, _dot(axis, reference)))
    if _len(knuckle_side) < 1.0 or _len(reference) < 1e-6:
        return None
    return math.degrees(math.atan2(_dot(axis, _cross(reference, knuckle_side)), _dot(reference, knuckle_side)))


def hand_twist(p, columns, rest_direction, side, rest_roll):
    """Hand roll relative to the native rest pose's roll, wrapped and limited."""
    if rest_roll is None:
        return 0.0
    roll = hand_roll(p, columns, rest_direction, side)
    if roll is None:
        return 0.0
    twist = (roll - rest_roll + 180.0) % 360.0 - 180.0
    return max(-MAX_TWIST_DEGREES, min(MAX_TWIST_DEGREES, twist))


def finger_curl(p, columns, side, rest_curl):
    """(share 0..1, hand-local axis) of the native knuckle bend for one hand.

    The native hand has NWrist, NKnuckles and NFingertips: open hands (chops,
    handsprings) keep the fingertips in line, fists fold them. The curl axis is
    the knuckle line, palm x finger; the source rest bend is subtracted.
    """
    palm = _unit(_sub(p['NKnuckles_' + side], p['NWrist_' + side]))
    finger = _unit(_sub(p['NFingertips_' + side], p['NKnuckles_' + side]))
    axis = _cross(palm, finger)
    if _len(axis) < 1e-6:
        return 0.0, [0.0, 0.0, 1.0]
    bend = math.degrees(math.atan2(_len(axis), _dot(palm, finger)))
    share = max(0.0, min(1.0, (bend - rest_curl) / FULL_CURL_DEGREES))
    return share, _unit(_transpose_apply(columns, _unit(axis)))


def curl_point(local, pivots, share, axis):
    """Fold a hand-local finger point about its phalanx pivots, distal joint first."""
    for level in range(len(pivots), 0, -1):
        pivot = pivots[level - 1]
        local = _add(pivot, _rotate(_sub(local, pivot), axis, share * PHALANX_CURL_DEGREES[level - 1]))
    return local


def visual_frames(p, v, rest):
    """Segment frames (origin, columns) on the visual rig, in canonical file coordinates.

    Also returns ('curl', side): (share, hand-local axis) for finger bindings.
    """
    frames = {}
    for key, (origin, front) in TORSO_FRONT.items():
        frames[key] = (v[key[0]], frame(_sub(v[key[1]], v[key[0]]), _sub(p[front], p[origin])))
    for child, parent in LIMB_PARENTS:
        columns = frames[parent][1]
        # The hand follows the palm (wrist to knuckles), not the fingertips,
        # so a closing fist does not tilt the whole hand.
        end = p['NKnuckles_' + child[0][-1]] if child[1].startswith('NFingertips') else v[child[1]]
        now = _transpose_apply(columns, _unit(_sub(end, v[child[0]])))
        swing = _swing(rest[child], _unit(now) if _len(now) else rest[child])
        frames[child] = (v[child[0]], [_apply(columns, axis) for axis in swing])
    for s in '12':
        key = ('NWrist_' + s, 'NFingertips_' + s)
        origin, hand = frames[key]
        twist = hand_twist(p, hand, rest[key], s, rest.get(('twist', s)))
        frames[('twist', s)] = twist
        if twist:
            axis = _apply(hand, rest[key])
            hand = [_rotate(column, axis, twist) for column in hand]
            frames[key] = (origin, hand)
        frames[('curl', s)] = finger_curl(p, hand, s, rest.get(('curl', s), 0.0))
    # Spring bones rest on their parent's frame here; the runtime simulates them.
    for index, bone in enumerate(rest.get('dynamic', [])):
        parent_origin, parent = frames[bone['parent']]
        frames[('dyn', index)] = (_add(parent_origin, _apply(parent, bone['head'])), parent)
    for s in '12':
        heel, toe, ankle = 'NHeel_' + s, 'NToeTip_' + s, 'NAnkle_' + s
        frames[(heel, toe)] = (v[heel], frame(_sub(v[toe], v[heel]), _sub(v[ankle], v[heel])))
    return frames


def _matrix(columns):
    return [[columns[j][i] for j in range(3)] for i in range(3)]


def _mat_mul(a, b):
    """Product of two column-list matrices."""
    return [_apply(a, column) for column in b]


def _mat_transpose(columns):
    return [[columns[j][i] for j in range(3)] for i in range(3)]


def _axis_rotation(axis, degrees):
    return [_rotate(e, axis, degrees) for e in ([1.0, 0.0, 0.0], [0.0, 1.0, 0.0], [0.0, 0.0, 1.0])]


def _quaternion(columns):
    """(w, x, y, z) of a rotation given as columns."""
    m = _matrix(columns)
    trace = m[0][0] + m[1][1] + m[2][2]
    if trace > 0:
        k = math.sqrt(trace + 1) * 2
        return [.25 * k, (m[2][1] - m[1][2]) / k, (m[0][2] - m[2][0]) / k, (m[1][0] - m[0][1]) / k]
    if m[0][0] > m[1][1] and m[0][0] > m[2][2]:
        k = math.sqrt(1 + m[0][0] - m[1][1] - m[2][2]) * 2
        return [(m[2][1] - m[1][2]) / k, .25 * k, (m[0][1] + m[1][0]) / k, (m[0][2] + m[2][0]) / k]
    if m[1][1] > m[2][2]:
        k = math.sqrt(1 + m[1][1] - m[0][0] - m[2][2]) * 2
        return [(m[0][2] - m[2][0]) / k, (m[0][1] + m[1][0]) / k, .25 * k, (m[1][2] + m[2][1]) / k]
    k = math.sqrt(1 + m[2][2] - m[0][0] - m[1][1]) * 2
    return [(m[1][0] - m[0][1]) / k, (m[0][2] + m[2][0]) / k, (m[1][2] + m[2][1]) / k, .25 * k]


def _qmul(a, b):
    return [a[0] * b[0] - a[1] * b[1] - a[2] * b[2] - a[3] * b[3],
            a[0] * b[1] + a[1] * b[0] + a[2] * b[3] - a[3] * b[2],
            a[0] * b[2] - a[1] * b[3] + a[2] * b[0] + a[3] * b[1],
            a[0] * b[3] + a[1] * b[2] - a[2] * b[1] + a[3] * b[0]]


def influence_transform(node, i, frames, rest, proportions):
    """Rigid (rotation columns, translation) taking a rest-space point to its posed place."""
    key = (node.get('BoneStart' + str(i)), node.get('BoneEnd' + str(i)))
    dynamic = node.get('Dynamic' + str(i))
    if dynamic is not None:
        key = ('dyn', int(dynamic))
    origin, columns = frames[key]
    source_origin, source = rest['frames'][key]
    local = [float(node.get('Local' + axis + str(i))) for axis in 'XYZ']
    curl, offset = [[1.0, 0.0, 0.0], [0.0, 1.0, 0.0], [0.0, 0.0, 1.0]], [0.0, 0.0, 0.0]
    if node.get('Curl' + str(i)) is not None:
        share, axis = frames[('curl', key[0][-1])]
        for level, pivot in reversed(list(enumerate(read_pivots(node.get('Pivot' + str(i))), 1))):
            turn = _axis_rotation(axis, share * PHALANX_CURL_DEGREES[level - 1])
            curl = _mat_mul(turn, curl)
            offset = _add(_apply(turn, _sub(offset, pivot)), pivot)
    if key[0].startswith('NElbow_') and dynamic is None:
        # Forearm skin takes a growing share of the hand twist toward the wrist.
        twist = frames.get(('twist', key[0][-1]), 0.0)
        if twist:
            direction = rest[key]
            along = _dot(local, direction) / proportions[key]
            columns = _mat_mul(_axis_rotation(_unit(_apply(columns, direction)), twist * max(0.0, min(1.0, along))), columns)
    rotation = _mat_mul(_mat_mul(columns, curl), _mat_transpose(source))
    translation = _sub(_add(origin, _apply(columns, offset)), _apply(rotation, source_origin))
    rest_point = _add(source_origin, _apply(source, local))
    return rotation, translation, rest_point


def dual_quaternion_point(node, frames, rest, proportions):
    """Dual-quaternion blend of the node's influences (no candy-wrapper pinching); X/Y."""
    real = [0.0, 0.0, 0.0, 0.0]; dual = [0.0, 0.0, 0.0, 0.0]; point = None; first = None
    for i in range(1, int(node.get('BonesCount')) + 1):
        rotation, translation, rest_point = influence_transform(node, i, frames, rest, proportions)
        point = point or rest_point
        q = _quaternion(rotation)
        if first is None:
            first = q
        elif sum(x * y for x, y in zip(q, first)) < 0:
            q = [-x for x in q]
        d = [.5 * x for x in _qmul([0.0] + translation, q)]
        weight = float(node.get('Weight' + str(i)))
        real = [r + weight * x for r, x in zip(real, q)]
        dual = [r + weight * x for r, x in zip(dual, d)]
    norm = math.sqrt(sum(x * x for x in real))
    real = [x / norm for x in real]; dual = [x / norm for x in dual]
    conjugate = [real[0], -real[1], -real[2], -real[3]]
    rotated = _qmul(_qmul(real, [0.0] + point), conjugate)[1:]
    translation = [2 * x for x in _qmul(dual, conjugate)[1:]]
    return [rotated[0] + translation[0], rotated[1] + translation[1]]


def helper_positions(rig, positions, proportions=None, rest=None):
    """Mirror the native weighted helper/COM construction in file coordinates.

    proportions: optional segment lengths from a skin's <Proportions>; its
    skinned nodes then bind to visual_joints instead of the native points.
    rest: optional <Rest> directions; LocalX/Y/Z influences then pose the
    source mesh in 3D on visual_frames and project it orthographically.
    """
    by_name = {node.tag: node for node in rig.find('Nodes')}
    visual = frames = None
    for node in rig.find('Nodes'):
        kind = node.get('Type')
        if kind == 'SkinnedNode':
            if proportions is not None and visual is None:
                native = unswap_pairs(positions)
                visual = visual_joints(native, proportions)
                if rest is not None:
                    frames = visual_frames(native, dict(native, **visual), rest)
            joints = positions if visual is None else dict(positions, **visual)
            point = [0.0, 0.0, 0.0]
            if node.get('LocalX1') is not None and rest is not None and 'frames' in rest:
                positions[node.tag] = dual_quaternion_point(node, frames, rest, proportions) + [0.0]
                continue
            if node.get('LocalX1') is not None:
                for i in range(1, int(node.get('BonesCount')) + 1):
                    key = (node.get('BoneStart' + str(i)), node.get('BoneEnd' + str(i)))
                    origin, columns = frames[key]
                    local = [float(node.get('Local' + axis + str(i))) for axis in 'XYZ']
                    if node.get('Curl' + str(i)) is not None:
                        share, axis = frames[('curl', key[0][-1])]
                        local = curl_point(local, read_pivots(node.get('Pivot' + str(i))), share, axis)
                    world = _add(origin, _apply(columns, local))
                    weight = float(node.get('Weight' + str(i)))
                    point[0] += weight * world[0]; point[1] += weight * world[1]
                positions[node.tag] = point
                continue
            for i in range(1, int(node.get('BonesCount')) + 1):
                start = joints[node.get('BoneStart' + str(i))]
                end = joints[node.get('BoneEnd' + str(i))]
                weight, along = float(node.get('Weight' + str(i))), float(node.get('Along' + str(i)))
                dx, dy = end[0] - start[0], end[1] - start[1]
                if node.get('Offset' + str(i)) is None:
                    across = float(node.get('Across' + str(i)))
                    point[0] += weight * (start[0] + along * dx - across * dy)
                    point[1] += weight * (start[1] + along * dy + across * dx)
                    continue
                # Matches ModelMacroNode.Skin: a foreshortened segment keeps 35% of
                # its 3D length as the frame scale.
                offset, extend = float(node.get('Offset' + str(i))), float(node.get('Extend' + str(i), 0))
                scale = max(math.hypot(dx, dy), .35 * math.sqrt(dx * dx + dy * dy + (end[2] - start[2]) ** 2))
                ux, uy = (dx / scale, dy / scale) if scale > 1e-6 else (0.0, 0.0)
                point[0] += weight * (start[0] + along * dx + extend * ux - offset * uy)
                point[1] += weight * (start[1] + along * dy + extend * uy + offset * ux)
            positions[node.tag] = point
            continue
        count = int(node.get('NodesCount', 0))
        if kind not in ('MacroNode', 'CenterOfMass') or count == 0:
            continue
        children = [node.get('ChildNode' + str(i)) for i in range(1, count + 1)]
        weights = ([float(node.get('LCC' + str(i), 0)) for i in range(1, count + 1)]
                   if kind == 'MacroNode' else [float(by_name[n].get('Mass', 0)) for n in children])
        if kind == 'CenterOfMass':
            total = sum(weights)
            if total <= 0:
                raise ValueError('Center of mass has no positive mass')
            weights = [w / total for w in weights]
        positions[node.tag] = [finite(sum(positions[n][axis] * w for n, w in zip(children, weights)), node.tag) for axis in range(3)]
    return positions


def bake(rig, source):
    names = [node.tag for node in rig.find('Nodes')]
    supplied = source['names']
    if len(set(supplied)) != len(supplied) or set(supplied) != set(names):
        raise ValueError('Animation names must match the complete ordered rig; duplicates/missing/extra nodes rejected')
    fps = finite(source['fps'], 'fps')
    frames = source['frames']
    if not 1 <= fps <= 240 or not 2 <= len(frames) <= 36000:
        raise ValueError('Animation requires 2..36000 samples at 1..240 fps')
    ordered = []
    for index, frame in enumerate(frames):
        if len(frame) != len(supplied) or any(len(p) != 3 for p in frame):
            raise ValueError(f'Frame {index}: wrong node/coordinate count')
        positions = {n: [finite(v, f'frame {index}/{n}') for v in p] for n, p in zip(supplied, frame)}
        helper_positions(rig, positions)
        ordered.append([positions[n] for n in names])
    count = round((len(ordered) - 1) * 60 / fps) + 1
    if count < 2 or count > 36000 or count * len(names) > 2000000:
        raise ValueError('Baked animation requires 2..36000 frames and at most two million node samples')
    result = []
    for i in range(count):
        time = min(i * fps / 60, len(ordered) - 1)
        first = int(time); second = min(first + 1, len(ordered) - 1); amount = time - first
        result.append([[a + (b - a) * amount for a, b in zip(p, q)] for p, q in zip(ordered[first], ordered[second])])
    return {'version': 1, 'fps': 60, 'names': names, 'frames': result}


def write_animation(path, clip):
    data = bytearray(struct.pack('<i', len(clip['frames'])))
    for frame in clip['frames']:
        data += struct.pack('<Bi', 1, len(frame))
        for point in frame:
            data += struct.pack('<3f', *point)
    Path(path).write_bytes(data)


def read_animation(path, rig):
    data = Path(path).read_bytes()
    names = [node.tag for node in rig.find('Nodes')]
    if len(data) < 4:
        raise ValueError('Truncated animation header')
    count, = struct.unpack_from('<i', data)
    expected = 4 + count * (5 + 12 * len(names))
    if not 1 <= count <= 36000 or expected != len(data) or count * len(names) > 2000000:
        raise ValueError('Invalid frame count, truncated/trailing data or incompatible rig')
    frames = []; offset = 4
    for i in range(count):
        _, size = struct.unpack_from('<Bi', data, offset); offset += 5
        if size != len(names):
            raise ValueError(f'Frame {i} has the wrong node count')
        frame = []
        for name in names:
            frame.append([finite(v, name) for v in struct.unpack_from('<3f', data, offset)])
            offset += 12
        frames.append(frame)
    return {'version': 1, 'fps': 60, 'names': names, 'frames': frames}


def preview(path, rig, clip):
    data = dict(clip)
    data['edges'] = [[e.get('End1'), e.get('End2')] for e in (rig.find('Edges') if rig.find('Edges') is not None else []) if e.get('Type') == 'Edge']
    payload = json.dumps(data, separators=(',', ':')).replace('<', '\\u003c')
    document = '''<!doctype html><meta charset="utf-8"><title>SF2 character preview</title>
<style>body{background:#28170e;color:#ecd1a1;font:18px Georgia;margin:24px}canvas{background:#b99b6c;width:100%;max-width:1100px}input{width:60%}button{font:inherit}</style>
<h1>Character and animation preview</h1><button id="play">Play</button> <input id="frame" type="range" min="0" value="0"> <span id="time"></span><p>Point rig preview; native collision, materials and move rules require a game playtest.</p><canvas id="canvas" width="1100" height="600"></canvas>
<script>const d=__DATA__,c=document.querySelector('canvas'),ctx=c.getContext('2d'),f=document.querySelector('#frame');f.max=d.frames.length-1;
let running=false,previous=0;document.querySelector('#play').onclick=()=>running=!running;
let loX=Infinity,hiX=-Infinity,loY=Infinity,hiY=-Infinity;for(const pose of d.frames)for(const p of pose){loX=Math.min(loX,p[0]);hiX=Math.max(hiX,p[0]);loY=Math.min(loY,p[1]);hiY=Math.max(hiY,p[1])}const scale=Math.min(1000/Math.max(1,hiX-loX),500/Math.max(1,hiY-loY));
function draw(){const pose=d.frames[+f.value],p=Object.fromEntries(d.names.map((n,i)=>[n,pose[i]]));ctx.clearRect(0,0,c.width,c.height);const xy=a=>[50+(a[0]-loX)*scale,550-(a[1]-loY)*scale];ctx.strokeStyle='#302013';ctx.lineWidth=5;for(const [a,b]of d.edges){ctx.beginPath();ctx.moveTo(...xy(p[a]));ctx.lineTo(...xy(p[b]));ctx.stroke()}ctx.fillStyle='#e5bc52';for(const a of pose){ctx.beginPath();ctx.arc(...xy(a),3,0,7);ctx.fill()}document.querySelector('#time').textContent=`Frame ${f.value} / ${f.max} · ${(+f.value/d.fps).toFixed(2)}s`}
f.oninput=()=>{running=false;draw()};function tick(t){if(running&&t-previous>=1000/d.fps){f.value=(+f.value+1)%d.frames.length;previous=t;draw()}requestAnimationFrame(tick)}draw();requestAnimationFrame(tick);</script>'''
    Path(path).write_text(document.replace('__DATA__', payload), encoding='utf-8')


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('command', choices=['validate', 'bake', 'preview'])
    parser.add_argument('--rig', type=Path, required=True)
    parser.add_argument('--skin', type=Path, action='append', default=[])
    parser.add_argument('--source', type=Path)
    parser.add_argument('--animation', type=Path)
    parser.add_argument('--output', type=Path)
    args = parser.parse_args(); rig = model(args.rig)
    combined = ET.fromstring(ET.tostring(rig))
    for skin in args.skin:
        overlay = model(skin, combined)
        for section in ('Nodes', 'Edges', 'Figures'):
            if overlay.find(section) is not None:
                combined.find(section).extend(list(overlay.find(section)))
    if args.command == 'bake':
        if args.source is None or args.output is None:
            parser.error('bake requires --source frames.json and --output clip.bytes')
        clip = bake(rig, json.loads(args.source.read_text(encoding='utf-8')))
        args.output.parent.mkdir(parents=True, exist_ok=True)
        write_animation(args.output, clip)
        manifest = {'version': 1, 'fps': 60, 'mid_frames': 0, 'frames': len(clip['frames']), 'nodes': clip['names'],
                    'rig_sha256': hashlib.sha256(args.rig.read_bytes()).hexdigest(), 'animation_sha256': hashlib.sha256(args.output.read_bytes()).hexdigest()}
        args.output.with_suffix('.rig.json').write_text(json.dumps(manifest, indent=2), encoding='utf-8')
        preview(args.output.with_suffix('.html'), rig, clip)
    elif args.animation:
        clip = read_animation(args.animation, rig)
        sidecar = args.animation.with_suffix('.rig.json')
        if sidecar.exists():
            saved = json.loads(sidecar.read_text(encoding='utf-8'))
            if saved.get('version') != 1 or saved.get('nodes') != clip['names'] or saved.get('rig_sha256') != hashlib.sha256(args.rig.read_bytes()).hexdigest() or saved.get('animation_sha256') != hashlib.sha256(args.animation.read_bytes()).hexdigest():
                raise ValueError('Animation/rig fingerprint or node order changed; rebake the clip')
        if args.command == 'preview':
            preview(args.output or args.animation.with_suffix('.html'), rig, clip)
    elif args.command == 'preview':
        parser.error('preview requires --animation')
    print('PASS: model references and requested animation payload validated')


if __name__ == '__main__':
    main()
