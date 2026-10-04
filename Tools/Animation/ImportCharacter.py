"""Blender: convert a weighted humanoid mesh into a playable native SF2 silhouette.

blender --background --factory-startup --python-exit-code 1 --python Tools/Animation/ImportCharacter.py -- --source fighter.glb --rig mdl_skeleton.xml --mod-id local.fighter --output Mods/local.fighter
Source meshes/armatures stay private; the fresh package uses core combat defaults.
"""
import argparse
import hashlib
import math
import json
from pathlib import Path
import re
import shutil
import sys
import tempfile
import time
import xml.etree.ElementTree as ET

import bpy
from mathutils import Matrix, Vector

sys.path.insert(0, str(Path(__file__).resolve().parent))
import CharacterPipeline as pipeline
import PackageCharacter
import HumanoidMotion
import SkinPreview
from RigMapping import map_rig, describe, suggest_roles

TARGETS = {'pelvis': ('NPivot', 'NStomach'), 'spine': ('NStomach', 'NChest'),
           'chest': ('NChest', 'NNeck'), 'neck': ('NNeck', 'NHead'), 'head': ('NHead', 'NTop')}
# Ordered names from the canonical MODELS archive, not a geometry/position hash.
CORE_NODE_ORDER = '563da17639d279ef93c1217d8872c92518e96eeaaa60f917e33c43de282004a7'
for side, suffix in (('left', '1'), ('right', '2')):
    for role, endpoints in {'upper_arm': ('NShoulder', 'NElbow'), 'forearm': ('NElbow', 'NWrist'),
                            'hand': ('NWrist', 'NFingertips'), 'thigh': ('NHip', 'NKnee'),
                            'shin': ('NKnee', 'NAnkle'), 'foot': ('NHeel', 'NToeTip')}.items():
        TARGETS[side + '_' + role] = tuple(name + '_' + suffix for name in endpoints)


def step(message):
    # Progress lines for the in-game importer UI; flushed so they arrive live.
    print('SF2 IMPORT STEP: ' + message, flush=True)


def load_source(path):
    extension = path.suffix.lower()
    if extension == '.blend':
        bpy.ops.wm.open_mainfile(filepath=str(path))
    elif extension == '.fbx':
        bpy.ops.import_scene.fbx(filepath=str(path))
    elif extension in ('.gltf', '.glb'):
        bpy.ops.import_scene.gltf(filepath=str(path))
    else:
        raise ValueError('Source must be .blend, .fbx, .glb or .gltf')


def finger_chains(armature, mapping, meshes):
    """Phalanges below each hand, the source palm direction and rest finger bend.

    Returns (fingers, palms, rest_curl): fingers maps a phalanx bone name to
    (hand role, finger index, level 1..3, world pivots of levels 1..level).
    Each child chain of the hand is a finger; its last three weighted bones are
    phalanges and earlier ones (metacarpals) stay on the palm. The thumb (named,
    or the chain starting nearest the wrist when there are five) stays on the
    palm because native animation has no thumb motion.
    """
    world = armature.matrix_world
    grouped = {group.name for obj in meshes for group in obj.vertex_groups}
    fingers, palms, rest_curl = {}, {}, {}
    for side in ('left', 'right'):
        hand = armature.data.bones[mapping[side + '_hand']]
        chains = []
        for child in hand.children:
            chain = [child]
            while chain[-1].children:
                chain.append(max(chain[-1].children, key=lambda b: (len(b.children_recursive), b.name)))
            while chain and chain[-1].name not in grouped:  # end/nub markers
                chain.pop()
            if chain:
                chains.append(chain)
        named = [c for c in chains if any('thumb' in describe(b.name)[1] for b in c)]
        if named:
            chains = [c for c in chains if c not in named]
        elif len(chains) == 5:
            wrist = world @ hand.head_local
            chains.remove(min(chains, key=lambda c: ((world @ c[0].head_local) - wrist).length))
        if not chains:
            continue
        knuckles, directions = [], []
        for index, chain in enumerate(chains):
            phalanges = chain[-3:]
            heads = [world @ b.head_local for b in phalanges]
            for level, bone in enumerate(phalanges, 1):
                fingers[bone.name] = (side + '_hand', index, level, tuple(tuple(h) for h in heads[:level]))
            knuckles.append(heads[0])
            tip = world @ phalanges[-1].tail_local
            if (tip - heads[0]).length > 1e-6:
                directions.append((tip - heads[0]).normalized())
        palm = sum(knuckles, Vector()) / len(knuckles) - world @ hand.head_local
        if palm.length < 1e-6 or not directions:
            continue
        palms[side] = palm
        finger = sum(directions, Vector())
        if finger.length > 1e-6:
            rest_curl[side] = math.degrees(palm.angle(finger))
    return fingers, palms, rest_curl


def source_profile(armature, mapping, report):
    """Side-view triangles of the source rest mesh (screen space), for comparison."""
    world = armature.matrix_world
    head = lambda role: world @ armature.data.bones[mapping[role]].head_local
    up = (head('head') - head('pelvis')).normalized()
    lateral = head('left_upper_arm') - head('right_upper_arm')
    lateral = (lateral - up * lateral.dot(up)).normalized()
    forward = lateral.cross(up)
    kept = {row['name'] for row in report if 'vertices' in row}
    triangles = []
    for obj in sum(bound_meshes(armature), []):
        if obj.name not in kept:
            continue
        points, faces = world_triangles(obj)
        flat = [(point.dot(forward), -point.dot(up)) for point in points]
        triangles.extend([flat[a], flat[b], flat[c]] for a, b, c in faces)
    return triangles


def write_preview(package, animations, armature, mapping, report):
    """preview.png: the source side profile, then the skin on native frames."""
    cells = [source_profile(armature, mapping, report)]
    for item in animations:
        path, _, frame = item.rpartition(':')
        cells.append(SkinPreview.posed_triangles(package, Path(path), int(frame or 0)))
    SkinPreview.render(cells, package / 'preview.png', size=256)


def proportions(mapping, head, source, targets):
    """One height-fitting scale and the source's own segment lengths at that scale.

    Gameplay keeps the native skeleton; only the drawn skin uses these lengths
    (CharacterPipeline.visual_joints / ModelMacroNode.Skin). Missing optional
    spine/neck joints are interpolated at the native torso ratios.
    """
    native = lambda a, b: (targets[b] - targets[a]).length
    pelvis, chest, top = head('pelvis'), head('chest'), head('head')
    def between(role, a, b, first, second):
        if role in mapping:
            return head(role)
        ratio = native(*first) / max(1e-6, native(*first) + native(*second))
        return a.lerp(b, ratio)
    spine = between('spine', pelvis, chest, ('NPivot', 'NStomach'), ('NStomach', 'NChest'))
    neck = between('neck', chest, top, ('NChest', 'NNeck'), ('NNeck', 'NHead'))
    crown = source['head'][0] + source['head'][1]
    torso = [pelvis, spine, chest, neck, top, crown]
    leg = lambda side: ((head(side + '_shin') - head(side + '_thigh')).length +
                        (head(side + '_foot') - head(side + '_shin')).length)
    source_height = sum((b - a).length for a, b in zip(torso, torso[1:])) + (leg('left') + leg('right')) / 2
    native_height = sum(native(a, b) for a, b in pipeline.TORSO_SEGMENTS) + sum(
        native('NHip_' + s, 'NKnee_' + s) + native('NKnee_' + s, 'NAnkle_' + s) for s in '12') / 2
    if source_height < 1e-6:
        raise ValueError('The source skeleton has no height')
    scale = native_height / source_height
    lengths = {}
    for (a, b), (p, q) in zip(pipeline.TORSO_SEGMENTS, zip(torso, torso[1:])):
        lengths[(a, b)] = (q - p).length * scale
    for side, s in (('left', '1'), ('right', '2')):
        joint = lambda role: head(side + '_' + role)
        lengths[('NNeck', 'NShoulder_' + s)] = (joint('upper_arm') - neck).length * scale
        lengths[('NShoulder_' + s, 'NElbow_' + s)] = (joint('forearm') - joint('upper_arm')).length * scale
        lengths[('NElbow_' + s, 'NWrist_' + s)] = (joint('hand') - joint('forearm')).length * scale
        lengths[('NWrist_' + s, 'NFingertips_' + s)] = source[side + '_hand'][1].length * scale
        lengths[('NPivot', 'NHip_' + s)] = (joint('thigh') - pelvis).length * scale
        lengths[('NHip_' + s, 'NKnee_' + s)] = (joint('shin') - joint('thigh')).length * scale
        lengths[('NKnee_' + s, 'NAnkle_' + s)] = (joint('foot') - joint('shin')).length * scale
        lengths[('NHeel_' + s, 'NToeTip_' + s)] = source[side + '_foot'][1].length * scale
    # Bounds of the skin format; a degenerate source joint keeps a tiny segment.
    return scale, {key: min(1000.0, max(.01, length)) for key, length in lengths.items()}


class BudgetExceeded(ValueError):
    pass


def bound_meshes(armature):
    """Meshes deformed by the armature, plus rigid meshes parented to one of its bones."""
    skinned, rigid = [], []
    for obj in bpy.context.scene.objects:
        if obj.type != 'MESH' or obj.hide_render:
            continue
        modifiers = [m for m in obj.modifiers if m.type == 'ARMATURE']
        if any(m.object == armature for m in modifiers):
            if any(m.object != armature for m in modifiers):
                raise ValueError(obj.name + ': multiple armature targets are unsupported')
            skinned.append(obj)
        elif not modifiers and obj.parent == armature and obj.parent_type == 'BONE' and obj.parent_bone in armature.data.bones:
            rigid.append(obj)
    # Exporters often bind every level of detail; the most detailed one is enough.
    lod = re.compile(r'(?i)(^|[_. -])lod_?([0-9]+)($|[_. -])')
    def keep(obj):
        found = lod.search(obj.name)
        return found is None or found.group(2) == '0'
    if any(lod.search(o.name) for o in skinned + rigid) and any(lod.search(o.name) and keep(o) for o in skinned + rigid):
        skinned = [o for o in skinned if keep(o)]
        rigid = [o for o in rigid if keep(o)]
    return skinned, rigid


def bind_positions(obj):
    """World positions of obj's vertices in the armature rest pose, modifiers applied.

    Falls back to the stored coordinates when other modifiers change the topology.
    """
    depsgraph = bpy.context.evaluated_depsgraph_get()
    evaluated = obj.evaluated_get(depsgraph)
    mesh = evaluated.to_mesh()
    try:
        if len(mesh.vertices) == len(obj.data.vertices):
            return [obj.matrix_world @ v.co for v in mesh.vertices]
        return [obj.matrix_world @ v.co for v in obj.data.vertices]
    finally:
        evaluated.to_mesh_clear()


def misbound_meshes(armature, meshes, reach):
    """Meshes whose rest vertices sit far from the bones they are weighted to.

    Some game exports keep interior parts (teeth, tongue) at the origin in the
    rest pose and only move them with an animation pose the file does not
    include. Drawn at rest they would float at the feet; in a silhouette they are
    hidden inside the head anyway. A mesh is skipped when most of its vertices
    are farther than reach from the segment of their dominant bone.
    """
    world = armature.matrix_world
    bones = armature.data.bones
    skipped = []
    for obj in meshes:
        if is_rigid(obj):
            continue
        points = bind_positions(obj)
        far = counted = 0
        for vertex in obj.data.vertices:
            weights = vertex_weights(obj, vertex, bones)
            if not weights:
                continue
            bone = bones[max(weights, key=weights.get)]
            a, b = world @ bone.head_local, world @ bone.tail_local
            axis = b - a; point = points[vertex.index]
            t = 0.0 if axis.length_squared < 1e-12 else max(0.0, min(1.0, (point - a).dot(axis) / axis.length_squared))
            counted += 1
            far += (point - (a + axis * t)).length > reach
        if counted and far > counted / 2:
            skipped.append(obj)
    return skipped


def world_triangles(obj):
    """Rest-pose world triangles of obj (modifiers applied)."""
    depsgraph = bpy.context.evaluated_depsgraph_get()
    evaluated = obj.evaluated_get(depsgraph)
    mesh = evaluated.to_mesh()
    try:
        mesh.calc_loop_triangles()
        points = [obj.matrix_world @ v.co for v in mesh.vertices]
        return points, [tuple(t.vertices) for t in mesh.loop_triangles]
    finally:
        evaluated.to_mesh_clear()


def interior_meshes(meshes, stature):
    """Small meshes enclosed by the others (teeth, tongue, inner mouth, eye backs).

    They never reach the silhouette but spend vertex budget. A mesh counts as
    interior when it is smaller than a quarter of the stature and rays from at
    least 85% of its sampled vertices hit other geometry in all six axis
    directions. Body-sized meshes are always kept: layered clothing can open up.
    """
    from mathutils.bvhtree import BVHTree
    shapes = {obj: world_triangles(obj) for obj in meshes}
    skipped = []
    for obj, (points, _) in shapes.items():
        if not points:
            continue
        low = Vector(tuple(min(p[i] for p in points) for i in range(3)))
        high = Vector(tuple(max(p[i] for p in points) for i in range(3)))
        if (high - low).length >= .25 * stature:
            continue
        verts, faces = [], []
        for other, (other_points, triangles) in shapes.items():
            if other is obj:
                continue
            offset = len(verts)
            verts.extend(other_points)
            faces.extend(tuple(offset + i for i in t) for t in triangles)
        if not faces:
            continue
        tree = BVHTree.FromPolygons(verts, faces)
        step = max(1, len(points) // 200)
        sample = points[::step]
        directions = [Vector(d) for d in ((1, 0, 0), (-1, 0, 0), (0, 1, 0), (0, -1, 0), (0, 0, 1), (0, 0, -1))]
        enclosed = sum(all(tree.ray_cast(p, d, 2 * stature)[0] is not None for d in directions) for p in sample)
        if enclosed >= .85 * len(sample):
            skipped.append(obj)
    return skipped


def mesh_importance(obj):
    """Visible surface area in world units, the outline's share of the budget."""
    points, triangles = world_triangles(obj)
    return sum(((points[b] - points[a]).cross(points[c] - points[a])).length / 2 for a, b, c in triangles)


def is_rigid(obj):
    return obj.parent_type == 'BONE' and not any(m.type == 'ARMATURE' for m in obj.modifiers)


def vertex_weights(obj, vertex, bones):
    if is_rigid(obj):
        return {obj.parent_bone: 1.0}
    weights = {}
    for group in vertex.groups:
        if group.weight > 0 and group.group < len(obj.vertex_groups):
            name = obj.vertex_groups[group.group].name
            if name in bones:  # Non-bone mask groups are not skin weights.
                weights[name] = weights.get(name, 0) + group.weight
    return weights


def convert(armature, meshes, mapping, max_vertices, rig, budget_scale=1.0):
    """Use rest-space weights; extra bones inherit their nearest mapped ancestor.

    Bones above the pelvis (root and hip helpers) and unrelated top-level props
    have no mapped ancestor; they follow the pelvis.
    """
    bones = armature.data.bones
    world = armature.matrix_world
    stature = (world @ bones[mapping['head']].head_local - world @ bones[mapping['left_foot']].head_local).length
    misbound = misbound_meshes(armature, meshes, .6 * stature)
    meshes = [obj for obj in meshes if obj not in misbound]
    if not meshes:
        raise ValueError('Every mesh lies far from its bones in the rest pose; check the source rest pose')
    interior = interior_meshes(meshes, stature)
    meshes = [obj for obj in meshes if obj not in interior]
    role_for = {name: role for role, name in mapping.items()}
    inherited = {}
    for bone in bones:
        parent = bone
        while parent is not None and parent.name not in role_for:
            parent = parent.parent
        inherited[bone.name] = role_for[parent.name] if parent is not None else 'pelvis'
    matrix = armature.matrix_world
    head = lambda role: matrix @ bones[mapping[role]].head_local
    up = head('head') - head('pelvis')
    if up.length < 1e-6:
        raise ValueError('Head and pelvis need different rest positions')
    up.normalize()
    # Side view: arena X is forward, Y is up and depth is the shoulder axis.
    lateral = head('left_upper_arm') - head('right_upper_arm')
    lateral -= up * lateral.dot(up)
    if lateral.length < 1e-6:
        raise ValueError('Shoulders need distinct rest positions; use a humanoid rest pose')
    lateral.normalize()
    forward = lateral.cross(up).normalized()
    region_points = {role: [] for role in mapping}
    for obj in meshes:
        rest_points = bind_positions(obj)
        for vertex in obj.data.vertices:
            weighted = {}
            for name, weight in vertex_weights(obj, vertex, bones).items():
                role = inherited[name]
                weighted[role] = weighted.get(role, 0) + weight
            if weighted:
                # Dominant-region extents prevent a tiny distant influence from
                # determining an entire hand/head/foot's calibration length.
                role = max(weighted, key=weighted.get)
                region_points[role].append(rest_points[vertex.index])
    targets = {node.tag: Vector(tuple(float(node.get(axis, 0)) for axis in 'XYZ')) for node in rig.find('Nodes')}
    body_scale = (targets['NHead']-targets['NPivot']).length / (head('head')-head('pelvis')).length
    next_role = {'pelvis': 'spine' if 'spine' in mapping else 'chest', 'spine': 'chest',
                 'chest': 'neck' if 'neck' in mapping else 'head', 'neck': 'head'}
    for side in ('left', 'right'):
        next_role.update({side+'_upper_arm': side+'_forearm', side+'_forearm': side+'_hand',
                          side+'_thigh': side+'_shin', side+'_shin': side+'_foot'})
    source = {}
    for role, name in mapping.items():
        bone = bones[name]
        start = matrix @ bone.head_local
        if role in next_role:
            end = head(next_role[role])
        else:
            if role == 'head':
                axis = up.copy()
            elif role.endswith('_hand'):
                axis = (start - head(role.replace('_hand', '_forearm'))).normalized()
            else:  # Feet use forward travel in the rest ground plane.
                axis = forward.copy()
                points = region_points[role]
                if points and sum((p-start).dot(axis) for p in points) < 0:
                    axis.negate()
                if points:
                    start += axis * min((p-start).dot(axis) for p in points)
                    start += up * min((p-start).dot(up) for p in points)
            extent = max(((p-start).dot(axis) for p in region_points[role]), default=0)
            if extent < 1e-6:
                a,b = TARGETS[role]; extent = (targets[b]-targets[a]).length / body_scale
            end = start + axis * extent
        direction = end - start
        if direction.length < 1e-6:
            raise ValueError(role + ': anatomical joint segment has zero length')
        # The native renderer draws the side (sagittal) silhouette, so "across"
        # measures forward/back thickness: the 90-degree counter-clockwise turn
        # of the segment in the forward/up plane, matching ModelMacroNode.Skin.
        # Arms are measured as if hanging, so T- and A-poses share a frame.
        along_axis = direction.normalized()
        preferred, fallback = lateral.cross(along_axis), forward - along_axis * forward.dot(along_axis)
        if role.endswith(('_upper_arm', '_forearm', '_hand')):
            preferred, fallback = fallback, preferred
        perpendicular = preferred if preferred.length >= .3 else fallback
        if perpendicular.length < 1e-6:
            raise ValueError(role + ': segment has no stable side-view frame')
        perpendicular = perpendicular - along_axis * perpendicular.dot(along_axis)
        perpendicular.normalize(); perpendicular *= direction.length
        source[role] = (start, direction, perpendicular)
    scale, lengths = proportions(mapping, head, source, targets)
    root = ET.Element('Scene'); nodes = ET.SubElement(root, 'Nodes'); ET.SubElement(root, 'Edges'); figures = ET.SubElement(root, 'Figures')
    element = ET.SubElement(root, 'Proportions', Version='1', Scale=format(scale, '.9g'))
    for (a, b), length in lengths.items():
        ET.SubElement(element, 'Segment', Start=a, End=b, Length=format(length, '.9g'))

    # Source rest frames matching CharacterPipeline.visual_frames: torso/head use
    # forward as their second axis (native: front helpers), feet the ankle, and
    # limbs inherit their parent's frame at rest (native: twist-free swing).
    def rest_frame(primary, secondary):
        a = primary.normalized(); b = secondary - a * a.dot(secondary)
        if b.length < 1e-6:
            b = up - a * a.dot(up)
        b.normalize()
        return Matrix((a, b, a.cross(b))).transposed()
    frames = {}
    for role in ('pelvis', 'spine', 'chest', 'neck', 'head'):
        if role in source:
            frames[role] = rest_frame(source[role][1], forward)
    fingers, palms, rest_curl = finger_chains(armature, mapping, meshes)
    rest = ET.SubElement(root, 'Rest', Version='1')
    for side in ('left', 'right'):
        foot = side + '_foot'
        frames[foot] = rest_frame(source[foot][1], head(foot) - source[foot][0])
        for role, parent in (('upper_arm', 'chest'), ('forearm', 'upper_arm'), ('hand', 'forearm'),
                             ('thigh', 'pelvis'), ('shin', 'thigh')):
            child = side + '_' + role
            parent = parent if parent in ('chest', 'pelvis') else side + '_' + parent
            frames[child] = frames[parent]
            # The runtime hand follows the native palm (wrist to knuckles).
            axis = palms[side] if role == 'hand' and side in palms else source[child][1]
            direction = frames[parent].transposed() @ axis.normalized()
            a, b = TARGETS[child]
            bone = ET.SubElement(rest, 'Bone', Start=a, End=b, **{k: format(v, '.9g') for k, v in zip('XYZ', direction)})
            if role == 'hand' and side in rest_curl:
                bone.set('Curl', format(rest_curl[side], '.6g'))
    finger_of = {}
    for bone in bones:
        current = bone
        while current is not None and current.name not in fingers and current.name not in mapping.values():
            current = current.parent
        if current is not None and current.name in fingers:
            finger_of[bone.name] = fingers[current.name]

    triple = lambda v: ','.join(format(x, '.9g') for x in v)
    # Source rest frames in scaled source space, for dual-quaternion blending.
    for role, columns in frames.items():
        if role in source:
            ET.SubElement(rest, 'Frame', Start=TARGETS[role][0], End=TARGETS[role][1],
                          O=triple(source[role][0] * scale), A=triple(columns.col[0]), B=triple(columns.col[1]))
    # Spring bones: unmapped rope-like chains (coats, hair, cloth, tails) that the
    # runtime simulates. Face rigs, twist helpers and props stay rigid.
    grouped = {group.name for obj in meshes for group in obj.vertex_groups}
    mapped = set(mapping.values())
    springy = re.compile(r'(?i)(hair|cloth|skirt|coat|cape|tail|scarf|ribbon|braid|pony|tie|bust|breast|jiggle|spring|phys|dyn|sleeve|robe|tassel|chain|belt|strap)')
    rigid_name = re.compile(r'(?i)(twist|roll|helper|ik|pole|target|eye|lid|lip|jaw|teeth|tongue|brow|mayu|cheek|nose|ear|finger|thumb|toe|wep|weapon|prop)')
    def rope(bone):
        depth = 0
        while bone is not None and bone.name not in mapped and bone.name in grouped and len(bone.children) <= 1:
            depth += 1
            bone = bone.children[0] if bone.children else None
        return depth
    dynamic, dynamic_index = [], {}
    for bone in sorted(bones, key=lambda b: len(b.parent_recursive)):
        if (bone.name in mapped or bone.name in fingers or bone.name not in grouped or rigid_name.search(bone.name)
                or len(bone.children) > 1):
            continue
        parent_dynamic = bone.parent is not None and bone.parent.name in dynamic_index
        if not (parent_dynamic or springy.search(bone.name) or rope(bone) >= 2):
            continue
        world_head = world @ bone.head_local
        world_tail = world @ (bone.children[0].head_local if bone.children else bone.tail_local)
        if (world_tail - world_head).length < 1e-6:
            continue
        if parent_dynamic:
            parent_key = '#' + str(dynamic_index[bone.parent.name])
            parent_origin, parent_frame = dynamic[dynamic_index[bone.parent.name]][1:3]
        else:
            role = inherited[bone.name]
            parent_key = TARGETS[role][0] + '-' + TARGETS[role][1]
            parent_origin, parent_frame = source[role][0] * scale, frames[role]
        index = len(dynamic)
        dynamic_index[bone.name] = index
        dynamic.append((bone.name, world_head * scale, parent_frame, world_head))
        head_local = parent_frame.transposed() @ (world_head * scale - parent_origin)
        direction = parent_frame.transposed() @ (world_tail - world_head).normalized()
        ET.SubElement(rest, 'Dynamic', Id=str(index), Parent=parent_key, Head=triple(head_local), Dir=triple(direction),
                      Length=format(max(.01, (world_tail - world_head).length * scale), '.9g'), Stiffness='0.2', Damping='0.88')
        ET.SubElement(rest, 'Frame', Dynamic=str(index), O=triple(world_head * scale),
                      A=triple(parent_frame.col[0]), B=triple(parent_frame.col[1]))
    dynamic_of = {}
    for bone in bones:
        current = bone
        while current is not None and current.name not in dynamic_index and current.name not in mapped:
            current = current.parent
        if current is not None and current.name in dynamic_index:
            dynamic_of[bone.name] = dynamic_index[current.name]

    def pivots(key):
        """Hand-local phalanx pivots (proximal first) at the height scale."""
        side_role = key[0]
        return ' '.join(','.join(format(c, '.6g') for c in frames[side_role].transposed() @ (Vector(point) - source[side_role][0]) * scale)
                        for point in key[3])

    def spring_attachment(index, point):
        """Source-rest position in a spring bone's frame at the height-fitting scale."""
        _, origin, columns, _ = dynamic[index]
        local = columns.transposed() @ (point * scale - origin)
        return None if max(abs(x) for x in local) > 1000 else tuple(local)

    def attachment(role, point):
        """Source-rest position in the role's frame at the height-fitting scale.

        At runtime the mesh is posed in 3D on frames built from the (visually
        proportioned) native pose and projected orthographically, so turned
        torsos, twisted heads and limbs pointing at the camera keep their shape.
        """
        local = frames[role].transposed() @ (point - source[role][0]) * scale
        return None if max(abs(x) for x in local) > 1000 else tuple(local)

    def nearest_role(point):
        def distance(role):
            start, direction, _ = source[role]
            t = max(0.0, min(1.0, (point - start).dot(direction) / direction.length_squared))
            return (point - (start + direction * t)).length
        return min(source, key=distance)

    total = 0; used_inherited = {}; repaired = [0]
    mesh_report = [{'name': obj.name, 'skipped': 'rest vertices lie far from their bones'} for obj in misbound]
    mesh_report += [{'name': obj.name, 'skipped': 'hidden inside other meshes'} for obj in interior]
    # Share the budget by visible surface (outline) as well as source detail, so
    # a dense face does not starve a long coat.
    area = {obj: mesh_importance(obj) for obj in meshes}
    total_area = sum(area.values()) or 1.0
    source_total = sum(len(o.data.vertices) for o in meshes)
    share = {obj: .5 * len(obj.data.vertices) / source_total + .5 * area[obj] / total_area for obj in meshes}
    dust = .008 * stature
    for obj in sorted(meshes, key=lambda o: o.name):
        # Evaluate the bind-pose mesh: the armature is at its rest position, and its
        # modifier stays on because some exporters (FBX teeth/eyes) store vertices
        # away from their bound place. Also evaluate the automatic reduction; the
        # source file is never saved.
        previous = []
        reducer = None; welder = None; detail = None
        try:
            budget = max(8, int(max_vertices * budget_scale * share[obj]))
            if len(obj.data.vertices) > budget:
                # Weld seams and doubled shells finer than a hair at fighter scale.
                welder = obj.modifiers.new('Eclipse import seam weld', 'WELD')
                welder.merge_threshold = .0015 * stature / max(1e-9, max(obj.matrix_world.to_scale()))
                reducer = obj.modifiers.new('Eclipse import reduction', 'DECIMATE')
                reducer.ratio = budget / len(obj.data.vertices) * .85
                if not is_rigid(obj):
                    # Collapse cost rises for weight-0 vertices. Protect joint blend
                    # rings so straight limbs keep a bend at the elbow/knee instead of
                    # becoming one long triangle, while fingers and flat areas reduce.
                    joints, free = [], []
                    for vertex in obj.data.vertices:
                        shares = {}
                        for name, weight in vertex_weights(obj, vertex, bones).items():
                            shares[inherited[name]] = shares.get(inherited[name], 0) + weight
                        total_share = sum(shares.values())
                        (joints if total_share > 0 and max(shares.values()) < .9 * total_share else free).append(vertex.index)
                    detail = obj.vertex_groups.new(name='Eclipse import detail')
                    detail.add(free, 1.0, "REPLACE"); detail.add(joints, .2, "REPLACE")
                    reducer.vertex_group = detail.name; reducer.vertex_group_factor = 6
            depsgraph = bpy.context.evaluated_depsgraph_get()
            for attempt in range(5):
                evaluated = obj.evaluated_get(depsgraph)
                mesh = evaluated.to_mesh(preserve_all_data_layers=True, depsgraph=depsgraph)
                if len(mesh.vertices) <= budget or reducer is None or attempt == 4:
                    break
                actual_count = len(mesh.vertices)
                evaluated.to_mesh_clear()
                reducer.ratio *= budget / actual_count * .85
                if attempt >= 1:  # Joint protection must not block the budget.
                    reducer.vertex_group_factor = max(0.0, reducer.vertex_group_factor / 4)
                bpy.context.view_layer.update()
                depsgraph = bpy.context.evaluated_depsgraph_get()
            # A mesh that cannot shrink further keeps its best reduction; the
            # package-wide limit below triggers a stricter retry instead.
            mesh.calc_loop_triangles()
            if not mesh.vertices or not mesh.loop_triangles:
                # Edge-only hair/curve helpers draw nothing in a filled silhouette.
                mesh_report.append({'name': obj.name, 'skipped': 'no triangles'})
                continue
            prefix = 'Imported' + hashlib.sha256(obj.name.encode('utf-8')).hexdigest()[:10]
            # Drop detached specks (loose eyelash cards, decimation crumbs) that only
            # roughen the outline.
            islands = list(range(len(mesh.vertices)))
            def find(i):
                while islands[i] != i:
                    islands[i] = islands[islands[i]]; i = islands[i]
                return i
            for triangle in mesh.loop_triangles:
                a, b, c = (find(v) for v in triangle.vertices)
                islands[b] = a; islands[find(c)] = a
            extent = {}
            for vertex in mesh.vertices:
                point = obj.matrix_world @ vertex.co; root_index = find(vertex.index)
                low, high = extent.get(root_index, (point.copy(), point.copy()))
                extent[root_index] = (Vector(map(min, low, point)), Vector(map(max, high, point)))
            speck = {r for r, (low, high) in extent.items() if (high - low).length < dust}
            vertex_names = []
            for vertex in mesh.vertices:
                if find(vertex.index) in speck:
                    vertex_names.append(None)
                    continue
                total += 1
                if total > max_vertices:
                    raise BudgetExceeded('Automatic reduction exceeded --max-vertices; simplify the source mesh')
                point = obj.matrix_world @ vertex.co
                # Influence keys: a role, or a finger phalanx (role, finger, level, pivots)
                # that folds with the native knuckle bend.
                weights = {}
                for name, weight in vertex_weights(obj, vertex, bones).items():
                    role = inherited[name]
                    key = finger_of.get(name) if role.endswith('_hand') else None
                    if key is None and name in dynamic_of:
                        key = ('dyn', dynamic_of[name], role)
                    key = key or role
                    weights[key] = weights.get(key, 0) + weight
                    if name != mapping[role]:
                        used_inherited[name] = mapping[role]
                role_of = lambda key: key if isinstance(key, str) else key[2] if key[0] == 'dyn' else key[0]
                bindings = {key: spring_attachment(key[1], point) if key[0] == 'dyn' else attachment(role_of(key), point)
                            for key in weights}
                # A tiny distant influence on a short segment would explode; drop
                # it and renormalize. Unweighted vertices follow the nearest segment.
                weights = {key: w for key, w in weights.items() if bindings[key] is not None}
                if sum(weights.values()) <= 0:
                    role = nearest_role(point)
                    weights = {role: 1.0}; bindings = {role: attachment(role, point)}
                    repaired[0] += 1
                    if bindings[role] is None:
                        raise ValueError(f'{obj.name} vertex {vertex.index}: unstable bone attachment')
                weight_sum = sum(weights.values())
                if len(weights) > 16:
                    raise ValueError(f'{obj.name} vertex {vertex.index}: exceeds 16 mapped bone influences')
                name = prefix + 'V' + str(vertex.index); vertex_names.append(name)
                node = ET.SubElement(nodes, name, Type='SkinnedNode', X='0', Y='0', Z='0', BonesCount=str(len(weights)))
                for i, (key, weight) in enumerate(sorted(weights.items(), key=lambda item: str(item[0])), 1):
                    x, y, z = bindings[key]; role = role_of(key)
                    for field, value in {'BoneStart': TARGETS[role][0], 'BoneEnd': TARGETS[role][1],
                                         'Weight': format(weight / weight_sum, '.9g'), 'LocalX': format(x, '.9g'),
                                         'LocalY': format(y, '.9g'), 'LocalZ': format(z, '.9g')}.items():
                        node.set(field + str(i), value)
                    if not isinstance(key, str) and key[0] == 'dyn':
                        node.set('Dynamic' + str(i), str(key[1]))
                    elif not isinstance(key, str):
                        node.set('Curl' + str(i), str(key[2])); node.set('Pivot' + str(i), pivots(key))
            emitted = 0
            for i, triangle in enumerate(mesh.loop_triangles):
                if any(vertex_names[v] is None for v in triangle.vertices):
                    continue
                emitted += 1
                ET.SubElement(figures, prefix + 'T' + str(i), Type='Triangle',
                              **{'Node' + str(k + 1): vertex_names[v] for k, v in enumerate(triangle.vertices)})
            mesh_report.append({'name': obj.name, 'source_vertices': len(obj.data.vertices),
                                'vertices': sum(n is not None for n in vertex_names), 'triangles': emitted,
                                'specks_removed': len(speck)})
        finally:
            if 'evaluated' in locals():
                evaluated.to_mesh_clear()
            if reducer is not None:
                obj.modifiers.remove(reducer)
            if welder is not None:
                obj.modifiers.remove(welder)
            if detail is not None:
                obj.vertex_groups.remove(detail)
            for modifier, state in previous:
                modifier.show_viewport = state
    if not len(figures):
        raise ValueError('No triangle meshes are bound to the selected rig')
    # Hand roll of the native rest pose, the zero for runtime hand twist.
    native = {name: list(point) for name, point in targets.items()}
    rest_info = pipeline.read_rest(root)
    visual = pipeline.visual_joints(native, lengths)
    rest_frames = pipeline.visual_frames(native, dict(native, **visual), rest_info)
    for element in root.find('Rest'):
        if element.tag == 'Bone' and element.get('End', '').startswith('NFingertips'):
            side = element.get('End')[-1]
            key = (element.get('Start'), element.get('End'))
            roll = pipeline.hand_roll(native, rest_frames[key][1], rest_info[key], side)
            if roll is not None:
                element.set('Twist', format(roll, '.6g'))
    return root, mesh_report, used_inherited, repaired[0]


CHARACTER = '''local sf2 = require("sf2")
local label = sf2.localization.key("fighter")
local icon = sf2.assets.sprite("core:ui/users/character_savage")
local empty = sf2.assets.model("models/empty")
local armor = sf2.items.register_armor {
    id = "imported_body", display_name = label, icon = icon, model = empty,
    initial_stats = { unarmed_damage = 0, body_defense = 0 },
}
local helm = sf2.items.register_helm {
    id = "imported_head", display_name = label, icon = icon, model = empty,
    initial_stats = { head_defense = 0 },
}
local character = sf2.warriors.register {
    id = "authored_character", level = 1,
    template = sf2.warriors.get_template("core:warrior-templates/man_kungfu"),
    first_name = sf2.mod.id .. ":localization/fighter", last_name = "", voice = "VOICE",
    skeleton = "Skeleton", items = { sf2.items.get("core:items/weapon/Fists"), armor, helm },
    body_model = sf2.assets.model("models/body"),
    skin_models = { sf2.assets.model("models/skin1") }, tactic = "Standard",
}
return { warrior = character }
'''


def motion_module(clips):
    module = CHARACTER.replace('return { warrior = character }', '')
    # Add a native-fallback tactic that selects only currently eligible actions.
    tactic = '''local preview_moves = NAMES
local tactic = sf2.tactics.register {
    id = "imported_motion", template = "Standard",
    on_decide = function(memory, event)
        for offset = 0, #preview_moves - 1 do
            local index = ((memory.next_clip or 1) - 1 + offset) % #preview_moves + 1
            for _, action in ipairs(event.actions) do
                if action.name == sf2.mod.id .. ":moves/" .. preview_moves[index] then
                    if event.seconds < (memory.ready or 0) then return "wait" end
                    memory.ready = event.seconds + 1.5
                    memory.next_clip = index % #preview_moves + 1
                    return action
                end
            end
        end
        return nil
    end,
}
'''.replace('NAMES','{ '+', '.join(json.dumps(c['name']) for c in clips)+' }')
    module = module.replace('local character =',tactic+'local character =').replace('tactic = "Standard"','tactic = tactic')
    module += 'local moves = {}\n'
    for clip in clips:
        module += '''moves[NAME] = sf2.moves.register {
    id = NAME, animation = ASSET, type = "MOVE", priority = 150,
    core_templates = { "Controlled", "NotTitan" },
    mid_frames = 0, first_frame = 0, end_frame = LAST, mirror_node = "NHeel_1",
    direction = "face_enemy", events = "controlled",
    conditions = { { character = character }, { key = KEY }, { controllable = true } },
    locks = { { item = "Skeleton", subtype = "Skeleton" } },
    intervals = { { name = "Uninterrupt", from = 0, to = LAST } },
}
'''.replace('NAME',json.dumps(clip['name'])).replace('ASSET',json.dumps('animations/'+clip['name']))\
    .replace('KEY',json.dumps(clip['key'])).replace('LAST',str(len(clip['clip']['frames'])-1))
    return module+'return { warrior = character, moves = moves }\n'


def run(args):
    destination = args.output.resolve()
    if not re.fullmatch(r'[a-z0-9][a-z0-9_.-]{0,127}', args.mod_id) or args.mod_id in ('core', 'sf2de'):
        raise ValueError('Choose a non-reserved lowercase mod ID')
    if destination.name != args.mod_id or destination.exists():
        raise ValueError('Output must be a fresh directory named after --mod-id')
    if not args.title.strip() or len(args.title) > 80 or not 3 <= args.max_vertices <= 3500:
        raise ValueError('Title needs 1..80 characters; max-vertices must be 3..3500')
    source_path = args.source.resolve()
    rig = pipeline.model(args.rig)
    target_names = {n.tag for n in rig.find('Nodes')}
    required_targets = {name for pair in TARGETS.values() for name in pair}
    order = '\n'.join(n.tag for n in rig.find('Nodes')).encode('utf-8')
    if not required_targets.issubset(target_names) or hashlib.sha256(order).hexdigest() != CORE_NODE_ORDER:
        raise ValueError('Use the canonical 67-point mdl_skeleton.xml for core combat defaults')
    step('Reading ' + source_path.name)
    load_source(source_path)
    candidates = [o for o in bpy.context.scene.objects if o.type == 'ARMATURE']
    if args.armature:
        candidates = [o for o in candidates if o.name == args.armature]
        if len(candidates) != 1:
            raise ValueError('Armature not found: ' + args.armature)
    if not candidates:
        raise ValueError('The source file contains no armature (skeleton)')
    # Several armatures (accessories, props): use the one that deforms the most geometry.
    bound = {o.name: sum(len(m.data.vertices) for m in sum(bound_meshes(o), [])) for o in candidates}
    armature = max(candidates, key=lambda o: (bound[o.name], o.name))
    overrides = json.loads(args.mapping.read_text(encoding='utf-8')) if args.mapping else None
    skinned, rigid = bound_meshes(armature)
    meshes = skinned + rigid
    if not meshes:
        raise ValueError('No meshes are bound to the selected rig with an Armature modifier or bone parent')
    bones = armature.data.bones
    deform = [b for b in bones if b.use_deform] or list(bones)
    deform_names = {b.name for b in deform}
    weighted = set()
    for obj in meshes:
        if is_rigid(obj):
            weighted.add(obj.parent_bone)
            continue
        positive = {group.group for vertex in obj.data.vertices for group in vertex.groups if group.weight > 0}
        weighted.update(obj.vertex_groups[i].name for i in positive if i < len(obj.vertex_groups))
    def deform_parent(bone):
        parent = bone.parent
        while parent is not None and parent.name not in deform_names:
            parent = parent.parent
        return parent.name if parent else None
    parents = {b.name: deform_parent(b) for b in deform}
    positions = {b.name: tuple(armature.matrix_world @ b.head_local) for b in deform}
    step('Matching the skeleton')
    try:
        mapping, method, mapping_notes = map_rig([b.name for b in deform], parents, positions, weighted & deform_names, overrides)
    except ValueError:
        # Give the in-game importer what it needs to let the player assign roles:
        # every candidate bone (hierarchy order) and the roles names could match.
        def walk(name, depth):
            print('SF2 IMPORT BONE: ' + str(depth) + '\t' + name, flush=True)
            for child in sorted(n for n, parent in parents.items() if parent == name):
                walk(child, depth + 1)
        for name in sorted(n for n, parent in parents.items() if parent is None):
            walk(name, 0)
        for role, name in suggest_roles([b.name for b in deform], overrides).items():
            print('SF2 IMPORT SUGGEST: ' + role + '\t' + name, flush=True)
        raise
    clips = []; used_names = set(); used_keys = set(); total_samples = 0
    for name, action_name, key in args.clip:
        if not re.fullmatch(r'[a-z][a-z0-9_]{0,47}',name) or name in used_names:
            raise ValueError('Clip names must be unique lowercase identifiers')
        if key not in ('Punch','Kick','Ranged','Magic','Up','Down','Forward','Back') or key in used_keys:
            raise ValueError('Clips need distinct native controls: Punch, Kick, Ranged, Magic, Up, Down, Forward, Back')
        action = bpy.data.actions.get(action_name)
        if action is None:
            raise ValueError('Source action not found: '+action_name+'; available: '+', '.join(a.name for a in bpy.data.actions))
        for curve in action.fcurves:
            try: armature.path_resolve(curve.data_path)
            except (ValueError,KeyError): raise ValueError('Action track does not bind to selected armature: '+curve.data_path)
        clip, report = HumanoidMotion.sample(rig,armature,mapping,action)
        total_samples += len(clip['frames'])*len(clip['names'])
        if total_samples > 2000000: raise ValueError('Imported clips exceed two million node samples')
        clips.append({'name':name,'key':key,'clip':clip,'report':report})
        used_names.add(name); used_keys.add(key)
    # Rest pose: bone-parented props and any posed source scene use bind positions.
    previous_pose = armature.data.pose_position
    armature.data.pose_position = 'REST'
    bpy.context.view_layer.update()
    try:
        scale = 1.0
        for attempt in range(8):
            step('Converting meshes' if attempt == 0 else 'Simplifying meshes (pass ' + str(attempt + 1) + ')')
            try:
                skin, report, inherited, repaired = convert(armature, meshes, mapping, args.max_vertices, rig, scale)
                break
            except BudgetExceeded:
                if attempt == 7:
                    raise
                scale *= .8
        # Decimation overshoots its target; spend an unused budget once on detail.
        used = sum(row.get('vertices', 0) for row in report)
        reduced = any(row.get('source_vertices', 0) > row.get('vertices', 0) for row in report)
        if reduced and used < .8 * args.max_vertices:
            step('Refining meshes')
            try:
                skin, report, inherited, repaired = convert(armature, meshes, mapping, args.max_vertices, rig,
                                                            scale * min(1.6, .95 * args.max_vertices / max(1, used)))
            except BudgetExceeded:
                pass
    finally:
        armature.data.pose_position = previous_pose
        bpy.context.view_layer.update()
    # Preserve gameplay node identity, masses, edges and order. Source geometry
    # replaces the visible body; inherited armor/helm meshes are not mounted.
    rig.find('Figures').clear()
    step('Writing the mod package')
    destination.parent.mkdir(parents=True, exist_ok=True)
    staging = Path(tempfile.mkdtemp(prefix='.imported-character-', dir=destination.parent))
    try:
        for directory in ('assets/models', 'scripts', 'localizations'):
            (staging / directory).mkdir(parents=True)
        body_path = staging / 'assets/models/body.xml'; skin_path = staging / 'assets/models/skin1.xml'
        ET.ElementTree(rig).write(body_path, encoding='utf-8', xml_declaration=True)
        ET.ElementTree(skin).write(skin_path, encoding='utf-8', xml_declaration=True)
        (staging / 'assets/models/empty.xml').write_text('<Scene><Nodes/><Edges/><Figures/></Scene>', encoding='utf-8')
        pipeline.model(skin_path, pipeline.model(body_path))
        module = motion_module(clips) if clips else CHARACTER
        (staging / 'scripts/character.lua').write_text(module.replace('voice = "VOICE"', 'voice = ' + json.dumps(args.voice)), encoding='utf-8')
        for entry in clips:
            animation_path = staging / ('assets/animations/'+entry['name']+'.bytes')
            animation_path.parent.mkdir(parents=True,exist_ok=True)
            pipeline.write_animation(animation_path,entry['clip'])
            animation_path.with_suffix('.frames.json').write_text(json.dumps(entry['clip']),encoding='utf-8')
            animation_path.with_suffix('.rig.json').write_text(json.dumps({
                'version':1,'fps':60,'mid_frames':0,'frames':len(entry['clip']['frames']),'control':entry['key'],
                'nodes':entry['clip']['names'],'rig_sha256':hashlib.sha256(body_path.read_bytes()).hexdigest(),
                'animation_sha256':hashlib.sha256(animation_path.read_bytes()).hexdigest()},indent=2),encoding='utf-8')
            animation_path.with_suffix('.retarget.json').write_text(json.dumps(entry['report']),encoding='utf-8')
        (staging / 'scripts/main.lua').write_text(PackageCharacter.preview_main(True), encoding='utf-8')
        (staging / 'mod.toml').write_text(
            f'schema = 1\nid = {json.dumps(args.mod_id)}\nname = {json.dumps(args.title, ensure_ascii=False)}\n'
            'version = "1.0.0"\nauthors = ["Local author"]\nentrypoint = "scripts/main.lua"\n'
            'capabilities = ["content.register"]\n[[dependencies]]\nid = "core"\nversion = ">=1.0 <2.0"\n', encoding='utf-8')
        title = json.dumps(args.title, ensure_ascii=False)
        (staging / 'localizations/eng.toml').write_text(
            f'title = {title}\nzones/preview = {title}\nfighter = {title}\n'
            'description = "Play the imported fighter with normal SF2 controls and combat."\n', encoding='utf-8')
        metadata = {'version': 1, 'source_sha256': hashlib.sha256(source_path.read_bytes()).hexdigest(),
                    'blender': bpy.app.version_string, 'armature': armature.name, 'mapping': mapping, 'mapping_method': method,
                    'mapping_notes': mapping_notes, 'repaired_vertices': repaired,
                    'warrior': args.mod_id + ':warriors/authored_character', 'voice': args.voice,
                    'inherited_bones': inherited, 'meshes': report, 'max_vertices': args.max_vertices,
                    'rig_sha256': hashlib.sha256(args.rig.read_bytes()).hexdigest(),
                    'combat': 'core unarmed defaults', 'renderer': 'planar weighted side-view silhouette on a proportioned visual rig',
                    'clips':[{'name':c['name'],'control':c['key'],'action':c['report']['action'],'samples':c['report']['samples']} for c in clips],
                    'limitations': ['Textures and materials are not imported. Clip attack timing is not inferred.',
                                    'Extra bones follow mapped ancestors; they have no independent animation.',
                                    'Core collision proportions remain unchanged. Non-humanoid locomotion is not inferred.']}
        (staging / 'import.json').write_text(json.dumps(metadata, indent=2) + '\n', encoding='utf-8')
        if args.preview_animation:
            step('Drawing the preview')
            write_preview(staging, args.preview_animation, armature, mapping, report)
        (staging / 'README.md').write_text(
            f'# {args.title}\n\nEnable `{args.mod_id}`, Apply & Restart, then select **{args.title}** on the map. '
            'Both fighters use the imported silhouette and normal unarmed SF2 controls, damage, reactions and Standard AI. '
            'The encounter does not replace campaign equipment.\n\n'
            'To use this look for Shadow everywhere, open Mods > Characters and choose it. '
            'Shadow keeps his items, stats and moves; armor and helmet visuals are hidden.\n\n'
            'Keep your source scene separately. import.json records matching and topology reduction. '
            'Inspect shoulders, wrists, knees, facing and contact fit in game before sharing. '
            'The adapter normalizes limbs to the native combat rig. Source materials and independent extra-bone motion are not imported. '
            'Selected actions are sampled at 60 Hz; generated moves have no attack intervals. Add typed attack timing in scripts/character.lua. '
            'Custom moves can be registered for this warrior in scripts/character.lua using the public moves API.\n', encoding='utf-8')
        # Antivirus/indexers can briefly lock a freshly written folder on Windows.
        for attempt in range(20):
            try:
                staging.rename(destination)
                break
            except PermissionError:
                if attempt == 19:
                    raise
                time.sleep(.25)
    finally:
        if staging.exists() and staging.parent == destination.parent and staging.name.startswith('.imported-character-'):
            shutil.rmtree(staging)
    print('SF2 IMPORT: ' + str(destination))
    print(json.dumps({'mapping': mapping, 'meshes': report, 'inherited_bones': inherited}, indent=2))


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--source',type=Path,required=True)
    for name in ('rig', 'output'):
        parser.add_argument('--' + name, type=Path)
    parser.add_argument('--mod-id')
    parser.add_argument('--title', default='Imported Fighter')
    parser.add_argument('--armature')
    parser.add_argument('--preview-animation', action='append', default=[], metavar='FILE:FRAME',
                        help='Native animation and frame to draw in preview.png (repeatable)')
    parser.add_argument('--voice', choices=('Male', 'Female'), default='Male',
                        help='Gendered combat sounds (grunts, hit cries) for this character')
    parser.add_argument('--mapping', type=Path)
    parser.add_argument('--max-vertices', type=int, default=3000)
    parser.add_argument('--clip',nargs=3,action='append',default=[],metavar=('NAME','ACTION','KEY'))
    parser.add_argument('--list-actions',action='store_true')
    args = parser.parse_args(sys.argv[sys.argv.index('--') + 1:])
    if args.list_actions:
        load_source(args.source.resolve())
        print(json.dumps({'fps':bpy.context.scene.render.fps/bpy.context.scene.render.fps_base,
                          'actions':[{'name':a.name,'first':a.frame_range[0],'last':a.frame_range[1]}
                                     for a in bpy.data.actions]},indent=2))
        return
    if args.rig is None or args.output is None or args.mod_id is None:
        parser.error('Import requires --rig, --output and --mod-id')
    run(args)


if __name__ == '__main__':
    main()
