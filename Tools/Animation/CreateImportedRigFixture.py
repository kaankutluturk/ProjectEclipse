"""Create original weighted humanoid fixtures; no recovered/third-party mesh."""
import argparse
from pathlib import Path
import sys
import bpy
from mathutils import Vector


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--output', type=Path, required=True)
    args = parser.parse_args(sys.argv[sys.argv.index('--') + 1:])
    args.output.mkdir(parents=True, exist_ok=False)
    bpy.ops.object.select_all(action='SELECT'); bpy.ops.object.delete(use_global=False)
    data = bpy.data.armatures.new('Foreign weighted rig'); armature = bpy.data.objects.new('ForeignRig', data)
    bpy.context.collection.objects.link(armature); bpy.context.view_layer.objects.active = armature; armature.select_set(True)
    bpy.ops.object.mode_set(mode='EDIT')
    specs = {
        'Hips': ((0, 0, 1), (0, 0, 1.2), None),
        'Spine': ((0, 0, 1.2), (0, 0, 1.4), 'Hips'),
        'Spine2': ((0, 0, 1.4), (0, 0, 1.6), 'Spine'),
        'Neck': ((0, 0, 1.6), (0, 0, 1.7), 'Spine2'),
        'Head': ((0, 0, 1.7), (0, 0, 1.95), 'Neck'),
    }
    for side, sign in (('Left', 1), ('Right', -1)):
        specs.update({
            side + 'Arm': ((sign * .18, 0, 1.6), (sign * .48, 0, 1.6), 'Spine2'),
            side + 'ForeArm': ((sign * .48, 0, 1.6), (sign * .75, 0, 1.6), side + 'Arm'),
            side + 'Hand': ((sign * .75, 0, 1.6), (sign * .88, 0, 1.6), side + 'ForeArm'),
            side + 'UpLeg': ((sign * .12, 0, 1), (sign * .12, 0, .55), 'Hips'),
            side + 'Leg': ((sign * .12, 0, .55), (sign * .12, 0, .1), side + 'UpLeg'),
            side + 'Foot': ((sign * .12, 0, .1), (sign * .12 + .15, 0, .03), side + 'Leg'),
            side + 'FingerExtra': ((sign * .88, 0, 1.6), (sign * .93, 0, 1.6), side + 'Hand'),
        })
    for name, (start, end, parent) in specs.items():
        bone = data.edit_bones.new('mixamorig:' + name); bone.head = start; bone.tail = end
        if parent:
            bone.parent = data.edit_bones['mixamorig:' + parent]
    bpy.ops.object.mode_set(mode='OBJECT')
    vertices = []; faces = []; groups = []
    for name, (start, end, _) in specs.items():
        a, b = Vector(start), Vector(end); direction = (b - a).normalized()
        width = .13 if name in ('Hips', 'Spine', 'Spine2') else .1 if name == 'Head' else .045
        normal = Vector((-direction.z, 0, direction.x)) * width
        depth = Vector((0, width * .5, 0)); offset = len(vertices)
        for p, taper in ((a, 1), (b, .8)):
            vertices.extend([p + normal * taper + depth, p - normal * taper + depth,
                             p - normal * taper - depth, p + normal * taper - depth])
        faces.extend([tuple(offset + v for v in face) for face in
                      ((0, 1, 2, 3), (4, 7, 6, 5), (0, 4, 5, 1), (1, 5, 6, 2), (2, 6, 7, 3), (3, 7, 4, 0))])
        groups.append(('mixamorig:' + name, list(range(offset, offset + 8))))
    mesh = bpy.data.meshes.new('Original segmented silhouette'); mesh.from_pydata(vertices, [], faces); mesh.update()
    obj = bpy.data.objects.new('WeightedBody', mesh); bpy.context.collection.objects.link(obj)
    for name, indices in groups:
        obj.vertex_groups.new(name=name).add(indices, 1, 'REPLACE')
    # Real mixed influences at each elbow exercise weighted blending, rather
    # than only proving rigid one-bone attachments.
    for side in ('Left', 'Right'):
        indices = next(indices for name, indices in groups if name == 'mixamorig:' + side + 'ForeArm')[:4]
        obj.vertex_groups['mixamorig:' + side + 'ForeArm'].add(indices, .6, 'REPLACE')
        obj.vertex_groups['mixamorig:' + side + 'Arm'].add(indices, .4, 'REPLACE')
    modifier = obj.modifiers.new('Imported rig weights', 'ARMATURE'); modifier.object = armature
    # An unapplied shared object transform exercises world/rest coordinate handling.
    for target in (obj, armature):
        target.location = (3, -2, 4); target.scale = (1.7, 1.7, 1.7)
    bpy.context.view_layer.update()
    bpy.ops.wm.save_as_mainfile(filepath=str((args.output / 'foreign.blend').resolve()))
    bpy.ops.export_scene.gltf(filepath=str((args.output / 'foreign.glb').resolve()), export_format='GLB')
    bpy.ops.object.select_all(action='SELECT')
    bpy.ops.export_scene.fbx(filepath=str((args.output / 'foreign.fbx').resolve()), use_selection=True,
                             add_leaf_bones=False, bake_anim=False)
    print('IMPORTED RIG FIXTURE: ' + str(args.output))


if __name__ == '__main__':
    main()
