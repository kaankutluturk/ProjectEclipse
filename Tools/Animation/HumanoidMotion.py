"""Blender evaluated humanoid actions -> ordered native point motion.

Shares semantic regions with ImportCharacter. No combat timing is inferred.
"""
import math
import bpy
from mathutils import Matrix, Vector
import CharacterPipeline as pipeline


def unit(vector, where):
    if vector.length < 1e-6:
        raise ValueError(where + ': degenerate anatomical direction')
    return vector.normalized()


def basis(primary, secondary, where):
    a = unit(primary, where)
    b = unit(secondary - a * secondary.dot(a), where)
    return Matrix((a, b, a.cross(b))).transposed()


def sample(rig, armature, mapping, action):
    """Preserve core lengths; transfer evaluated directions, rotations and root travel.

    Uses rest joint heads, never source display-bone tails. Samples evaluated
    constraints at 60 Hz. Rest-floor calibration stays fixed, preserving jumps.
    """
    if action is None or not action.fcurves:
        raise ValueError('Choose a nonempty source action')
    scene = bpy.context.scene
    fps = scene.render.fps / scene.render.fps_base
    first, last = action.frame_range
    if not 1 <= fps <= 240 or not math.isfinite(first + last) or last <= first:
        raise ValueError('Source action requires a positive duration at 1..240 fps')
    count = round((last-first)*60/fps)+1
    nodes = list(rig.find('Nodes')); names = [n.tag for n in nodes]
    if not 2 <= count <= 36000 or count*len(nodes) > 2000000:
        raise ValueError('Action exceeds 2..36000 samples or two million node samples')
    animation = armature.animation_data_create()
    previous_action, previous_frame, previous_subframe = animation.action, scene.frame_current, scene.frame_subframe
    previous_nla = animation.use_nla
    previous_pose = {bone.name:bone.matrix_basis.copy() for bone in armature.pose.bones}
    try:
        animation.action = action; animation.use_nla = False
        # A partial action must not borrow unkeyed channels from the previously
        # selected action or the pose saved in the source file.
        for bone in armature.pose.bones: bone.matrix_basis = Matrix.Identity(4)
        scene.frame_set(math.floor(first), subframe=first-math.floor(first))
        world = armature.matrix_world.copy()
        rest = {role: world @ armature.data.bones[name].head_local for role,name in mapping.items()}
        up = unit(rest['head']-rest['pelvis'], 'head/pelvis')
        lateral = rest['left_upper_arm']-rest['right_upper_arm']
        lateral = unit(lateral-up*lateral.dot(up), 'shoulders')
        # Side view: forward is arena X, up is file Y, left/right is depth Z.
        forward = lateral.cross(up)
        transform = Matrix((forward, up, lateral))
        ref = {n.tag: Vector(tuple(float(n.get(axis,0)) for axis in 'XYZ')) for n in nodes}
        scale = (ref['NHead']-ref['NPivot']).length / (rest['head']-rest['pelvis']).length
        rest_rotation = {role: transform @ (world @ armature.data.bones[name].matrix_local).to_quaternion().to_matrix()
                         for role,name in mapping.items()}
        rest = {role: transform @ point for role,point in rest.items()}
        donor_body = basis(ref['NNeck']-ref['NPivot'],ref['NHip_1']-ref['NHip_2'],'native torso')
        source_body = basis(rest['head']-rest['pelvis'],rest['left_thigh']-rest['right_thigh'],'source torso')
        body_bind = source_body @ donor_body.inverted()
        hand_bind = {}
        foot_bind = {}
        for side,suffix in (('left','1'),('right','2')):
            donor_arm = ref['NWrist_'+suffix]-ref['NElbow_'+suffix]
            source_arm = rest[side+'_hand']-rest[side+'_forearm']
            hand_bind[side] = unit(donor_arm,'native arm').rotation_difference(unit(source_arm,'source arm')).to_matrix()
            foot_bind[side] = basis(Vector((1,0,0)),Vector((0,1,0)),'source foot') @ basis(
                ref['NToe_'+suffix]-ref['NHeel_'+suffix],ref['NAnkle_'+suffix]-ref['NHeel_'+suffix],'native foot').inverted()

        def pose(points, rotations):
            p = {'NPivot': Vector((0,ref['NPivot'].y,0))+(points['pelvis']-rest['pelvis'])*scale}
            p['NPivot'].z = 0
            def chain(child,parent,direction):
                p[child] = p[parent]+unit(direction,child)*(ref[child]-ref[parent]).length
            # Optional torso regions are interpolated, not silently dropped.
            torso = dict(points)
            torso.setdefault('spine',points['pelvis'].lerp(points['chest'],.5))
            torso.setdefault('neck',points['chest'].lerp(points['head'],.5))
            chain('NStomach','NPivot',torso['spine']-points['pelvis'])
            chain('NChest','NStomach',points['chest']-torso['spine'])
            chain('NNeck','NChest',torso['neck']-points['chest'])
            chain('NHead','NNeck',points['head']-torso['neck'])
            chain('NTop','NHead',rotations['head'] @ rest_rotation['head'].inverted() @ Vector((0,1,0)))
            for side,suffix in (('left','1'),('right','2')):
                chain('NHip_'+suffix,'NPivot',points[side+'_thigh']-points['pelvis'])
                chain('NKnee_'+suffix,'NHip_'+suffix,points[side+'_shin']-points[side+'_thigh'])
                chain('NAnkle_'+suffix,'NKnee_'+suffix,points[side+'_foot']-points[side+'_shin'])
                chain('NShoulder_'+suffix,'NNeck',points[side+'_upper_arm']-torso['neck'])
                chain('NElbow_'+suffix,'NShoulder_'+suffix,points[side+'_forearm']-points[side+'_upper_arm'])
                chain('NWrist_'+suffix,'NElbow_'+suffix,points[side+'_hand']-points[side+'_forearm'])
                for region,anchor,binding,prefixes in (
                    ('hand','NWrist_',hand_bind[side],('NKnuckles','NFingertips','Weapon-Node')),
                    ('foot','NAnkle_',foot_bind[side],('NToe','NHeel'))):
                    role = side+'_'+region
                    rotate = rotations[role] @ rest_rotation[role].inverted() @ binding
                    for name in names:
                        if name.endswith('_'+suffix) and name.startswith(prefixes):
                            p[name] = p[anchor+suffix]+rotate @ (ref[name]-ref[anchor+suffix])
            for anchor,role,helpers in (
                ('NPivot','pelvis',('NPelvisF',)),
                ('NStomach','spine',('NStomachS_1','NStomachS_2','NStomachF')),
                ('NChest','chest',('NChestS_1','NChestS_2','NChestF')),
                ('NHead','head',('NHeadS_1','NHeadS_2','NHeadF'))):
                if role not in rotations: role = 'pelvis'
                rotate = rotations[role] @ rest_rotation[role].inverted() @ body_bind
                for name in helpers: p[name] = p[anchor]+rotate @ (ref[name]-ref[anchor])
            return p

        rest_pose = pose(rest,rest_rotation)
        floor = min(rest_pose[n+'_'+s].y for n in ('NHeel','NToe','NToeTip','NToeS') for s in ('1','2'))
        frames = []; observations = []
        for index in range(count):
            time = min(last,first+index*fps/60)
            scene.frame_set(math.floor(time),subframe=time-math.floor(time))
            evaluated = armature.evaluated_get(bpy.context.evaluated_depsgraph_get())
            points = {role: transform @ (evaluated.matrix_world @ evaluated.pose.bones[name].head)
                      for role,name in mapping.items()}
            rotations = {role: transform @ (evaluated.matrix_world @ evaluated.pose.bones[name].matrix).to_quaternion().to_matrix()
                         for role,name in mapping.items()}
            p = {name:[v.x,v.y-floor,v.z] for name,v in pose(points,rotations).items()}
            pipeline.helper_positions(rig,p)
            if set(p) != set(names): raise ValueError('Incomplete native point mapping')
            frames.append([[pipeline.finite(value,name+' sample '+str(index)) for value in p[name]] for name in names])
            observations.append({'frame':time,'points':{role:list(point) for role,point in points.items()}})
        return {'version':1,'fps':60,'names':names,'frames':frames}, {
            'action':action.name,'source_fps':fps,'first':first,'last':last,'samples':count,
            'scale':scale,'rest_floor':floor,'observations':observations,
            'root_motion':'forward/up; depth translation removed; constant rest-floor offset',
            'limitations':['Core segment lengths and hand/foot shapes retained.',
                           'No contact IK, independent finger/tail animation or attack timing inferred.',
                           'Only the selected armature action is replaced; external animated constraints remain evaluated.']}
    finally:
        animation.action = previous_action; animation.use_nla = previous_nla
        for bone in armature.pose.bones: bone.matrix_basis = previous_pose[bone.name]
        scene.frame_set(previous_frame,subframe=previous_subframe)
