"""Blender: convert a weighted humanoid mesh into a playable native SF2 silhouette.

blender --background --factory-startup --python-exit-code 1 --python Tools/Animation/ImportCharacter.py -- --source fighter.glb --rig mdl_skeleton.xml --mod-id local.fighter --output Mods/local.fighter
Source meshes/armatures stay private; the fresh package uses core combat defaults.
"""
import argparse
import hashlib
import json
from pathlib import Path
import re
import shutil
import sys
import tempfile
import xml.etree.ElementTree as ET

import bpy
from mathutils import Vector

sys.path.insert(0, str(Path(__file__).resolve().parent))
import CharacterPipeline as pipeline
import PackageCharacter
import HumanoidMotion
from RigMapping import match_bones, infer_humanoid

TARGETS = {'pelvis': ('NPivot', 'NStomach'), 'spine': ('NStomach', 'NChest'),
           'chest': ('NChest', 'NNeck'), 'neck': ('NNeck', 'NHead'), 'head': ('NHead', 'NTop')}
# Ordered names from the canonical MODELS archive, not a geometry/position hash.
CORE_NODE_ORDER = '563da17639d279ef93c1217d8872c92518e96eeaaa60f917e33c43de282004a7'
for side, suffix in (('left', '1'), ('right', '2')):
    for role, endpoints in {'upper_arm': ('NShoulder', 'NElbow'), 'forearm': ('NElbow', 'NWrist'),
                            'hand': ('NWrist', 'NFingertips'), 'thigh': ('NHip', 'NKnee'),
                            'shin': ('NKnee', 'NAnkle'), 'foot': ('NHeel', 'NToeTip')}.items():
        TARGETS[side + '_' + role] = tuple(name + '_' + suffix for name in endpoints)


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


def convert(armature, meshes, mapping, max_vertices, rig):
    """Use rest-space weights; extra bones inherit their nearest mapped ancestor."""
    bones = armature.data.bones
    role_for = {name: role for role, name in mapping.items()}
    inherited = {}
    for bone in bones:
        parent = bone
        while parent is not None and parent.name not in role_for:
            parent = parent.parent
        if parent is not None:
            inherited[bone.name] = role_for[parent.name]
    matrix = armature.matrix_world
    head = lambda role: matrix @ bones[mapping[role]].head_local
    up = head('head') - head('pelvis')
    if up.length < 1e-6:
        raise ValueError('Head and pelvis need different rest positions')
    up.normalize()
    right = head('left_upper_arm') - head('right_upper_arm')
    right -= up * right.dot(up)
    if right.length < 1e-6:
        raise ValueError('Shoulders need distinct rest positions; use a humanoid rest pose')
    right.normalize()
    normal = right.cross(up).normalized()
    region_points = {role: [] for role in mapping}
    for obj in meshes:
        for vertex in obj.data.vertices:
            weighted = {}
            for group in vertex.groups:
                role = inherited.get(obj.vertex_groups[group.group].name)
                if role and group.weight > 0:
                    weighted[role] = weighted.get(role, 0) + group.weight
            if weighted:
                # Dominant-region extents prevent a tiny distant influence from
                # determining an entire hand/head/foot's calibration length.
                role = max(weighted, key=weighted.get)
                region_points[role].append(obj.matrix_world @ vertex.co)
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
                axis = normal.copy()
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
        perpendicular = normal.cross(direction)
        if perpendicular.length < direction.length * .01:
            perpendicular = up - direction.normalized() * up.dot(direction.normalized())
        perpendicular.normalize(); perpendicular *= direction.length
        source[role] = (start, direction, perpendicular)
    root = ET.Element('Scene'); nodes = ET.SubElement(root, 'Nodes'); ET.SubElement(root, 'Edges'); figures = ET.SubElement(root, 'Figures')
    total = 0; mesh_report = []; used_inherited = {}
    for obj in sorted(meshes, key=lambda o: o.name):
        # Keep rest-space bone weights. Evaluate other modifiers, including an
        # automatic topology reduction; the source file is never saved.
        modifiers = [m for m in obj.modifiers if m.type == 'ARMATURE']
        previous = [(m, m.show_viewport) for m in modifiers]
        for modifier, _ in previous:
            modifier.show_viewport = False
        reducer = None; welder = None
        try:
            budget = max(3, int(max_vertices * len(obj.data.vertices) / sum(len(o.data.vertices) for o in meshes)))
            if len(obj.data.vertices) > budget:
                welder = obj.modifiers.new('Eclipse import seam weld', 'WELD')
                welder.merge_threshold = 0.000001
                reducer = obj.modifiers.new('Eclipse import reduction', 'DECIMATE')
                reducer.ratio = budget / len(obj.data.vertices) * .85
            depsgraph = bpy.context.evaluated_depsgraph_get()
            for attempt in range(5):
                evaluated = obj.evaluated_get(depsgraph)
                mesh = evaluated.to_mesh(preserve_all_data_layers=True, depsgraph=depsgraph)
                if len(mesh.vertices) <= budget or reducer is None:
                    break
                actual_count = len(mesh.vertices)
                evaluated.to_mesh_clear()
                reducer.ratio *= budget / actual_count * .85
                bpy.context.view_layer.update()
                depsgraph = bpy.context.evaluated_depsgraph_get()
            else:
                raise ValueError(obj.name + ': automatic reduction cannot meet the vertex budget')
            mesh.calc_loop_triangles()
            if not mesh.vertices or not mesh.loop_triangles:
                raise ValueError(obj.name + ': mesh needs vertices and triangles')
            prefix = 'Imported' + hashlib.sha256(obj.name.encode('utf-8')).hexdigest()[:10]
            vertex_names = []
            for vertex in mesh.vertices:
                total += 1
                if total > max_vertices:
                    raise ValueError('Automatic reduction exceeded --max-vertices; simplify the source mesh')
                point = obj.matrix_world @ vertex.co
                weights = {}
                for group in vertex.groups:
                    if group.weight <= 0:
                        continue
                    name = obj.vertex_groups[group.group].name
                    if name not in bones:  # Non-bone mask groups are not skin weights.
                        continue
                    if name not in inherited:
                        raise ValueError(f'{obj.name} vertex {vertex.index}: weighted bone {name} has no mapped ancestor')
                    role = inherited[name]; weights[role] = weights.get(role, 0) + group.weight
                    if name != mapping[role]:
                        used_inherited[name] = mapping[role]
                weight_sum = sum(weights.values())
                if weight_sum <= 0:
                    raise ValueError(f'{obj.name} vertex {vertex.index}: needs positive armature skin weights')
                if len(weights) > 16:
                    raise ValueError(f'{obj.name} vertex {vertex.index}: exceeds 16 mapped bone influences')
                name = prefix + 'V' + str(vertex.index); vertex_names.append(name)
                node = ET.SubElement(nodes, name, Type='SkinnedNode', X='0', Y='0', Z='0', BonesCount=str(len(weights)))
                for i, (role, weight) in enumerate(sorted(weights.items()), 1):
                    start, direction, perpendicular = source[role]; relative = point - start
                    along = relative.dot(direction) / direction.length_squared
                    across = relative.dot(perpendicular) / direction.length_squared
                    if max(abs(along), abs(across)) > 100:
                        raise ValueError(f'{obj.name} vertex {vertex.index}: unstable bone attachment')
                    for field, value in {'BoneStart': TARGETS[role][0], 'BoneEnd': TARGETS[role][1],
                                         'Weight': format(weight / weight_sum, '.9g'),
                                         'Along': format(along, '.9g'), 'Across': format(across, '.9g')}.items():
                        node.set(field + str(i), value)
            for i, triangle in enumerate(mesh.loop_triangles):
                ET.SubElement(figures, prefix + 'T' + str(i), Type='Triangle',
                              **{'Node' + str(k + 1): vertex_names[v] for k, v in enumerate(triangle.vertices)})
            mesh_report.append({'name': obj.name, 'source_vertices': len(obj.data.vertices),
                                'vertices': len(mesh.vertices), 'triangles': len(mesh.loop_triangles)})
        finally:
            if 'evaluated' in locals():
                evaluated.to_mesh_clear()
            if reducer is not None:
                obj.modifiers.remove(reducer)
            if welder is not None:
                obj.modifiers.remove(welder)
            for modifier, state in previous:
                modifier.show_viewport = state
    return root, mesh_report, used_inherited


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
    first_name = sf2.mod.id .. ":localization/fighter", last_name = "",
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
    load_source(source_path)
    candidates = [o for o in bpy.context.scene.objects if o.type == 'ARMATURE']
    if args.armature:
        candidates = [o for o in candidates if o.name == args.armature]
    if len(candidates) != 1:
        raise ValueError('Choose one source armature with --armature; found: ' + ', '.join(o.name for o in candidates))
    armature = candidates[0]
    overrides = json.loads(args.mapping.read_text(encoding='utf-8')) if args.mapping else None
    deform = [b for b in armature.data.bones if b.use_deform]
    method = 'names'
    try:
        mapping = match_bones([b.name for b in deform], overrides)
    except ValueError as error:
        if 'Cannot identify' not in str(error):
            raise
        parents = {b.name: b.parent.name if b.parent else None for b in deform}
        positions = {b.name: list(armature.matrix_world @ b.head_local) for b in deform}
        inferred = infer_humanoid(parents, positions)
        mapping = match_bones([b.name for b in deform], dict(inferred, **(overrides or {})))
        method = 'hierarchy'
    meshes = [o for o in bpy.context.scene.objects if o.type == 'MESH' and
              any(m.type == 'ARMATURE' and m.object == armature for m in o.modifiers)]
    if not meshes:
        raise ValueError('No meshes with an Armature modifier bound to the selected rig')
    for obj in meshes:
        if any(m.type == 'ARMATURE' and m.object != armature for m in obj.modifiers):
            raise ValueError(obj.name + ': multiple armature targets are unsupported')
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
    skin, report, inherited = convert(armature, meshes, mapping, args.max_vertices, rig)
    # Preserve gameplay node identity, masses, edges and order. Source geometry
    # replaces the visible body; inherited armor/helm meshes are not mounted.
    rig.find('Figures').clear()
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
        (staging / 'scripts/character.lua').write_text(motion_module(clips) if clips else CHARACTER, encoding='utf-8')
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
            f'title = {title}\nzones/preview = {title}\nfighter = "Imported Fighter"\n'
            'description = "Play the imported fighter with normal SF2 controls and combat."\n', encoding='utf-8')
        metadata = {'version': 1, 'source_sha256': hashlib.sha256(source_path.read_bytes()).hexdigest(),
                    'blender': bpy.app.version_string, 'armature': armature.name, 'mapping': mapping, 'mapping_method': method,
                    'inherited_bones': inherited, 'meshes': report, 'max_vertices': args.max_vertices,
                    'rig_sha256': hashlib.sha256(args.rig.read_bytes()).hexdigest(),
                    'combat': 'core unarmed defaults', 'renderer': 'planar weighted silhouette',
                    'clips':[{'name':c['name'],'control':c['key'],'action':c['report']['action'],'samples':c['report']['samples']} for c in clips],
                    'limitations': ['Textures and materials are not imported. Clip attack timing is not inferred.',
                                    'Extra bones follow mapped ancestors; they have no independent animation.',
                                    'Core collision proportions remain unchanged. Non-humanoid locomotion is not inferred.']}
        (staging / 'import.json').write_text(json.dumps(metadata, indent=2) + '\n', encoding='utf-8')
        (staging / 'README.md').write_text(
            f'# {args.title}\n\nEnable `{args.mod_id}`, Apply & Restart, then select **{args.title}** on the map. '
            'Both fighters use the imported silhouette and normal unarmed SF2 controls, damage, reactions and Standard AI. '
            'The encounter does not replace campaign equipment.\n\n'
            'Keep your source scene separately. import.json records matching and topology reduction. '
            'Inspect shoulders, wrists, knees, facing and contact fit in game before sharing. '
            'The adapter normalizes limbs to the native combat rig. Source materials and independent extra-bone motion are not imported. '
            'Selected actions are sampled at 60 Hz; generated moves have no attack intervals. Add typed attack timing in scripts/character.lua. '
            'Custom moves can be registered for this warrior in scripts/character.lua using the public moves API.\n', encoding='utf-8')
        staging.rename(destination)
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
    parser.add_argument('--mapping', type=Path)
    parser.add_argument('--max-vertices', type=int, default=2048)
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
